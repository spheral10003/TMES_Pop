using SEOJIN_TMES.Client.Common.Logging;
using System;

namespace SEOJIN_TMES.Client.Core
{
    /// <summary>
    /// 앱 전역에서 공유하는 설정 · 시스템 · 설비 묶음.
    ///
    /// 예전에는 Lazy&lt;Global&gt;로 "처음 접근하는 순간" 만들었다. 그 방식은 생성 중 예외를 Lazy가 저장해 두고
    /// 이후 모든 접근(종료 정리 포함)에서 같은 예외를 다시 던져서, 정리도 로그 닫기도 못 하고
    /// 원인도 남지 않았다. 재시도 모드(PublicationOnly)는 생성자 부작용(전역 예외 처리기 · 스킨 등록)을
    /// 두 번 실행할 수 있어 쓸 수 없다.
    /// 그래서 Program.Main에서 <see cref="Initialize"/>로 딱 한 번, 순서가 보이게 만든다.
    /// </summary>
    public sealed class Global
    {
        private static readonly object _sync = new object();
        private static volatile Global _instance;

        /// <summary>
        /// 초기화된 인스턴스. <see cref="Initialize"/> 전에 접근하면 원인이 분명한 예외를 던진다
        /// (예전처럼 접근한 쪽에서 뜬금없이 설정 로드 · 설비 생성이 시작되지 않게).
        /// </summary>
        public static Global Instance =>
            _instance ?? throw new InvalidOperationException(
                "Global이 초기화되지 않았다. Program.Main에서 Global.Initialize()를 먼저 호출해야 한다.");

        /// <summary>초기화가 끝났는지 여부. 종료 정리처럼 초기화 실패 후에도 도는 코드에서 확인용.</summary>
        public static bool IsInitialized => _instance != null;

        public Setting Settings { get; }
        public RuntimeState System { get; }
        public Devices Devices { get; }

        private Global(Setting settings, RuntimeState system, Devices devices)
        {
            Settings = settings;
            System = system;
            Devices = devices;
        }

        /// <summary>
        /// 전역 객체를 만든다. 앱 시작 시 로그인 화면보다 먼저 한 번만 호출한다.
        /// 실패하면 예외를 그대로 던지고 <see cref="IsInitialized"/>는 false로 남는다 — 저장된 예외는 없다.
        /// </summary>
        public static void Initialize()
        {
            lock (_sync)
            {
                if (_instance != null)
                {
                    throw new InvalidOperationException("Global.Initialize()가 이미 호출되었다.");
                }

                Setting settings = null;

                try
                {
                    // 단계마다 로그를 남겨 두면 실패했을 때 어느 단계에서 멈췄는지 바로 보인다.
                    AppLog.Step("전역 초기화 1/3 - 설정 로드 · API 클라이언트 · 문서 공유");
                    settings = new Setting();

                    AppLog.Step("전역 초기화 2/3 - 전역 예외 처리 · 스킨");
                    var system = new RuntimeState();

                    AppLog.Step("전역 초기화 3/3 - 설비");
                    var devices = new Devices();

                    _instance = new Global(settings, system, devices);
                    AppLog.Step("전역 초기화 완료");
                }
                catch (Exception ex)
                {
                    AppLog.Error("전역 초기화 실패", ex);

                    // 먼저 만들어진 문서 공유 연결(SMB)은 Global이 없으면 Shutdown()에서 닫을 수 없으므로 여기서 닫는다.
                    SafeRun(() => settings?.DocShare?.Dispose(), "초기화 실패 후 문서 공유 정리");
                    throw;
                }
            }
        }

        /// <summary>
        /// 앱 종료 시 정리. 초기화가 실패했거나 한 번도 안 됐어도 안전하게 호출할 수 있고,
        /// 한 항목의 정리가 실패해도 나머지 정리는 계속한다.
        /// </summary>
        public static void Shutdown()
        {
            Global instance;

            lock (_sync)
            {
                instance = _instance;
                _instance = null;
            }

            if (instance == null)
            {
                return;
            }

            SafeRun(() => instance.Devices?.Manager?.Dispose(), "설비 Modbus 매니저 정리");
            SafeRun(() => instance.Settings?.DocShare?.Dispose(), "문서 공유 폴더 정리");
        }

        private static void SafeRun(Action action, string what)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                AppLog.Error($"{what} 중 예외", ex);
            }
        }
    }
}
