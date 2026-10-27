using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SEOJIN_TMES.Client.Common.Models;

namespace SEOJIN_TMES.Client.Common.Logging
{
    /// <summary>
    /// 서버로 보낼 이벤트 로그의 단일 지점. 종류는 <see cref="ClientLogType"/> 여섯 가지뿐이다.
    ///
    /// 모든 기록은 <b>먼저 로컬 파일(AppLog)에 남기고</b> 그 다음 서버 전송 대기열에 넣는다 —
    /// 전송이 실패하거나 서버 API가 아직 없어도 파일에는 같은 내용이 있다.
    ///
    /// 전송 시점:
    ///  - 메인 화면이 뜨기 전(시작 · 로그인 · 시작 직후 설비 접속)에는 <b>모아만 둔다</b>.
    ///    로그인 전에는 토큰이 없어 보낼 수 없기 때문이다.
    ///  - 메인 화면 Load에서 <see cref="Attach"/>를 부르면 모아 둔 것을 한 번에 보내고,
    ///    그 뒤로는 생길 때마다 바로 보낸다.
    ///
    /// 설비 수신 스레드에서도 불리므로 스레드 안전하게 짰다.
    /// 전송은 백그라운드에서 돌며 <b>어떤 실패도 호출자에게 던지지 않는다</b> — 로그 때문에 화면·설비 통신이 멈추면 안 된다.
    /// 이 클래스 안에서는 실패를 <see cref="AppLog"/>로만 남긴다(ServerLog를 다시 부르면 실패가 실패를 낳는다).
    /// </summary>
    public static class ServerLog
    {
        /// <summary>
        /// 메인 화면이 끝내 뜨지 않는 경우(로그인 화면에서 오래 머무름 등) 대기열이 끝없이 늘지 않게 막는 상한.
        /// 넘치면 오래된 것부터 버린다 — 버려도 로컬 파일에는 남아 있다.
        /// </summary>
        private const int MaxPending = 1000;

        /// <summary>한 번에 보내는 최대 건수.</summary>
        private const int BatchSize = 100;

        private static readonly object SyncRoot = new object();
        private static readonly List<ClientLogDto> Pending = new List<ClientLogDto>();

        private static IClientLogSender _sender;
        private static bool _sending;

        private static string _appVersion;
        private static string _userId;
        private static string _corpCd;
        private static string _workstation;

        // ────────────────────────────── 수명 ──────────────────────────────

        /// <summary>
        /// 서버 통로를 붙이고 모아 둔 로그를 보낸다. 메인 화면 Load에서 한 번 부른다.
        /// 다시 부르면 통로만 바뀐다(재로그인 등).
        /// </summary>
        public static void Attach(IClientLogSender sender)
        {
            int count;
            lock (SyncRoot)
            {
                _sender = sender;
                count = Pending.Count;
            }

            AppLog.Step($"[서버로그] 전송 시작 - 대기 {count}건");
            Flush();
        }

        // ────────────────────────────── 기록 ──────────────────────────────

        /// <summary>프로그램 시작. 버전은 이후 모든 로그에 함께 실린다.</summary>
        public static void AppStarted(string appVersion)
        {
            lock (SyncRoot)
                _appVersion = appVersion;

            AppLog.Step($"[서버로그] 프로그램 시작 - 버전 {appVersion}, PC {Environment.MachineName}");
            Enqueue(New(ClientLogType.AppStart, "프로그램 시작"));
        }

        /// <summary>로그인 성공. 사용자 정보는 이후 모든 로그에 함께 실린다.</summary>
        public static void LoggedIn(string userId, string corpCd, string workstation)
        {
            lock (SyncRoot)
            {
                _userId = userId;
                _corpCd = corpCd;
                _workstation = workstation;
            }

            AppLog.Step($"[서버로그] 로그인 - UserId={userId}, CorpCd={corpCd}, Workstation={workstation}");
            Enqueue(New(ClientLogType.Login, $"로그인 - {userId}"));
        }

        /// <summary>메뉴 클릭. <paramref name="action"/>은 <c>Open</c>(새 탭) / <c>Focus</c>(열린 탭으로 전환).</summary>
        public static void MenuClicked(string menuKey, string menuName, string action)
        {
            AppLog.Info($"[서버로그] 메뉴 클릭 - {menuName}({menuKey}) {action}");

            var log = New(ClientLogType.MenuClick, $"메뉴 {menuName} {action}");
            log.MenuKey = menuKey;
            log.MenuName = menuName;
            log.MenuAction = action;
            Enqueue(log);
        }

        /// <summary>설비 접속 성공/실패. 실패 사유는 <paramref name="reason"/>에.</summary>
        public static void EquipmentConnected(string equipmentId, string channel, string host, int port, bool success, string reason = null)
        {
            var summary = success
                ? $"설비 접속 성공 - {equipmentId}/{channel} {host}:{port}"
                : $"설비 접속 실패 - {equipmentId}/{channel} {host}:{port} :: {reason}";

            if (success)
                AppLog.Step($"[서버로그] {summary}");
            else
                AppLog.Warn($"[서버로그] {summary}");

            var log = New(ClientLogType.EquipmentConnect, summary);
            log.EquipmentId = equipmentId;
            log.Channel = channel;
            log.Host = host;
            log.Port = port;
            log.Success = success;
            Enqueue(log);
        }

        /// <summary>
        /// 설정 레지스터 쓰기의 송신 또는 응답 1건. 설정 변경(토크 · 드라이버 잠금)만 부른다 —
        /// 읽기 · Alive · 핸드셰이크 · 이벤트 수신 확인(Ack)은 여기 오지 않는다.
        ///
        /// 이 장비 통신은 쓰기와 응답이 따로 올라오므로(요청 시점에 응답을 기다리지 않는다)
        /// 송신 1건, 응답 1건으로 나눠 남긴다 — 송신만 있고 응답이 없으면 장비가 받지 못한 것이다.
        /// </summary>
        public static void ModbusWritten(string equipmentId, string host, int port, string request,
            bool isResponse, string values, bool success)
        {
            var direction = isResponse ? "응답" : "송신";
            var summary = success
                ? $"레지스터 쓰기 {direction} - {equipmentId} {request} = {values}"
                : $"레지스터 쓰기 {direction}(오류 응답) - {equipmentId} {request} = {values}";

            AppLog.Info($"[서버로그] {summary}");

            var log = New(ClientLogType.ModbusWrite, summary);
            log.EquipmentId = equipmentId;
            log.Channel = "Request";
            log.Host = host;
            log.Port = port;
            log.ModbusRequest = request;
            log.Direction = isResponse ? "Receive" : "Send";
            log.RegisterValues = values;
            log.Success = success;
            Enqueue(log);
        }

        /// <summary>
        /// 처리되지 않은 예외(전역 핸들러) — 시스템 차원의 치명적 오류.
        /// </summary>
        public static void Critical(string origin, Exception exception)
        {
            var wrapped = UnexpectedException.Wrap(origin, exception);
            AppLog.Error($"[서버로그] 치명적 오류 ({origin})", wrapped);
            Enqueue(NewError(origin, wrapped));
        }

        /// <summary>
        /// 화면이 잡기는 했지만 예상하지 못한 예외(<c>catch (Exception)</c>).
        /// <see cref="UnexpectedException"/>으로 묶어 남기고 서버로도 보낸다 —
        /// 예상한 실패(ApiException 등)는 여기로 보내지 말고 AppLog에만 남길 것.
        /// </summary>
        public static UnexpectedException Unexpected(string origin, Exception exception)
        {
            var wrapped = UnexpectedException.Wrap(origin, exception);
            AppLog.Error($"[서버로그] 예기치 못한 오류 ({origin})", wrapped);
            Enqueue(NewError(origin, wrapped));
            return wrapped;
        }

        // ────────────────────────────── 내부 ──────────────────────────────

        private static ClientLogDto New(ClientLogType type, string message)
        {
            lock (SyncRoot)
            {
                return new ClientLogDto
                {
                    LogType = type.ToString(),
                    OccurredAt = DateTime.Now,
                    UserId = _userId,
                    CorpCd = _corpCd,
                    Workstation = _workstation,
                    MachineName = Environment.MachineName,
                    AppVersion = _appVersion,
                    Message = message
                };
            }
        }

        private static ClientLogDto NewError(string origin, UnexpectedException wrapped)
        {
            var root = wrapped.InnerException ?? wrapped;

            var log = New(ClientLogType.CriticalError, wrapped.Message);
            log.Origin = origin;
            log.ExceptionType = root.GetType().FullName;
            log.ExceptionDetail = root.ToString();
            return log;
        }

        private static void Enqueue(ClientLogDto log)
        {
            lock (SyncRoot)
            {
                if (Pending.Count >= MaxPending)
                    Pending.RemoveAt(0);

                Pending.Add(log);
            }

            Flush();
        }

        /// <summary>
        /// 통로가 붙어 있고 이미 보내는 중이 아니면 백그라운드에서 대기열을 비운다.
        /// 보내는 중에 들어온 것은 같은 루프가 이어서 보낸다(동시에 여러 요청이 나가지 않는다).
        /// </summary>
        private static void Flush()
        {
            lock (SyncRoot)
            {
                if (_sender == null || _sending || Pending.Count == 0)
                    return;

                _sending = true;
            }

            Task.Run(DrainLoop);
        }

        private static async Task DrainLoop()
        {
            while (true)
            {
                IClientLogSender sender;
                List<ClientLogDto> batch;

                lock (SyncRoot)
                {
                    if (_sender == null || Pending.Count == 0)
                    {
                        _sending = false;
                        return;
                    }

                    sender = _sender;
                    var take = Math.Min(BatchSize, Pending.Count);
                    batch = Pending.GetRange(0, take);
                    Pending.RemoveRange(0, take);
                }

                try
                {
                    await sender.SetClientLogs(batch).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    // 다시 넣지 않는다 — 같은 내용이 이미 로컬 파일에 있고,
                    // 서버가 계속 실패하는 동안 재시도로 대기열이 쌓이면 메모리만 먹는다.
                    AppLog.Warn($"[서버로그] 전송 실패 {batch.Count}건(로컬 파일에는 남아 있다) - {ex.Message.Replace(Environment.NewLine, " ")}");
                }
            }
        }
    }
}
