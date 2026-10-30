using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SEOJIN_TMES.Client.Modbus.Abstractions;
using SEOJIN_TMES.Client.Modbus.Transport;

namespace SEOJIN_TMES.Client.Equipment.Diagnostics
{
    /// <summary>
    /// 설비 접속 점검(접속 테스트 팝업용) — Request 포트에 붙은 상태에서 Event 포트까지 붙는지 한 번 확인하고 바로 끊는다.
    ///
    /// 소켓을 직접 열지 않고 운영과 같은 <see cref="TcpModbusTransport"/>를 쓴다 — 현장에서 "가끔 안 붙는다"를 재현하려면
    /// 실제 접속 코드(접속 제한 시간 처리 포함)를 그대로 타야 의미가 있다.
    /// <see cref="Nutrunner.NutrunnerSequence"/>(재연결 · Alive · 알람)는 거치지 않는다. 회차마다 새 전송 객체로 붙였다가 버린다.
    /// 프레임은 주고받지 않는다(TCP 접속 성립 여부만 본다).
    /// </summary>
    public static class ModbusConnectionProbe
    {
        /// <summary>
        /// 전송 계층이 접속 결과(Connected/ErrorOccurred)를 끝내 올리지 않을 때를 대비한 여유 시간.
        /// 정상이면 제한 시간 안에 둘 중 하나가 온다 — 이건 테스트가 멈춰 버리지 않게 막는 마지막 장치다.
        /// </summary>
        private const int ResultGraceMs = 1000;

        /// <summary>
        /// Request 포트 → Event 포트 순으로 접속하고, 둘 다 붙으면 성공이다. 결과와 관계없이 두 연결은 모두 끊고 돌아온다.
        /// Request를 붙든 채 Event를 여는 것은 운영 중처럼 두 채널이 동시에 열린 상태를 장비가 받아 주는지까지 보기 위해서다.
        /// </summary>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/>으로 중지한 경우.</exception>
        public static async Task<ConnectionProbeResult> Probe(
            string host, int port, int eventPort, int timeoutMs, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            using (var request = new TcpModbusTransport(0, timeoutMs))
            {
                var failure = await TryConnect(request, host, port, timeoutMs, cancellationToken).ConfigureAwait(false);
                if (failure != null)
                    return ConnectionProbeResult.Fail(ProbeChannel.Request, failure, stopwatch.ElapsedMilliseconds);

                using (var events = new TcpModbusTransport(1, timeoutMs))
                {
                    failure = await TryConnect(events, host, eventPort, timeoutMs, cancellationToken).ConfigureAwait(false);
                    if (failure != null)
                        return ConnectionProbeResult.Fail(ProbeChannel.Event, failure, stopwatch.ElapsedMilliseconds);
                }
            }

            return ConnectionProbeResult.Ok(stopwatch.ElapsedMilliseconds);
        }

        /// <summary>접속에 성공하면 null, 실패하면 그 사유를 돌려준다.</summary>
        private static async Task<ProbeFailure?> TryConnect(
            IModbusTransport transport, string host, int port, int timeoutMs, CancellationToken cancellationToken)
        {
            // 전송 계층 이벤트는 소켓 콜백 스레드에서 오므로, 결과를 Task로 받아 호출부를 그 스레드에 묶지 않는다.
            var result = new TaskCompletionSource<ProbeFailure?>(TaskCreationOptions.RunContinuationsAsynchronously);
            EventHandler onConnected = (s, e) => result.TrySetResult(null);
            EventHandler<Exception> onError = (s, ex) => result.TrySetResult(new ProbeFailure(ex is TimeoutException, ex.Message));

            transport.Connected += onConnected;
            transport.ErrorOccurred += onError;
            try
            {
                transport.Connect(host, port);

                using (var delayCancel = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    var finished = await Task.WhenAny(result.Task, Task.Delay(timeoutMs + ResultGraceMs, delayCancel.Token)).ConfigureAwait(false);
                    delayCancel.Cancel();   // 결과가 먼저 왔으면 남은 대기 타이머를 바로 정리한다

                    if (finished != result.Task)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        return new ProbeFailure(true, null);
                    }
                }

                return await result.Task.ConfigureAwait(false);
            }
            finally
            {
                transport.Connected -= onConnected;
                transport.ErrorOccurred -= onError;
            }
        }
    }

    /// <summary>점검한 채널. <see cref="Nutrunner.NutrunnerDevice"/>의 Request / Event 채널과 같은 구분이다.</summary>
    public enum ProbeChannel
    {
        /// <summary>레지스터 Read/Write 요청 채널(Port).</summary>
        Request,

        /// <summary>체결 결과 Push 채널(EventPort, 기본 Port + 1).</summary>
        Event
    }

    /// <summary>접속 실패 사유. 시간 초과는 화면에서 다국어 문구로 바꿔 보이도록 플래그로 넘긴다.</summary>
    internal sealed class ProbeFailure
    {
        public ProbeFailure(bool timedOut, string? message)
        {
            TimedOut = timedOut;
            Message = message;
        }

        public bool TimedOut { get; }
        public string? Message { get; }
    }

    /// <summary><see cref="ModbusConnectionProbe.Probe"/> 1회 결과.</summary>
    public sealed class ConnectionProbeResult
    {
        private ConnectionProbeResult(bool success, ProbeChannel? failedChannel, bool timedOut, string? errorMessage, long elapsedMs)
        {
            Success = success;
            FailedChannel = failedChannel;
            TimedOut = timedOut;
            ErrorMessage = errorMessage;
            ElapsedMs = elapsedMs;
        }

        public bool Success { get; }

        /// <summary>어느 채널에서 실패했는지(성공이면 null).</summary>
        public ProbeChannel? FailedChannel { get; }

        /// <summary>제한 시간 안에 접속되지 않았는지.</summary>
        public bool TimedOut { get; }

        /// <summary>소켓 오류 메시지(접속 거부 등). OS 언어로 나온다.</summary>
        public string? ErrorMessage { get; }

        /// <summary>Request 포트 접속 시작부터 결과가 나기까지 걸린 시간(ms).</summary>
        public long ElapsedMs { get; }

        internal static ConnectionProbeResult Ok(long elapsedMs) =>
            new ConnectionProbeResult(true, null, false, null, elapsedMs);

        internal static ConnectionProbeResult Fail(ProbeChannel channel, ProbeFailure failure, long elapsedMs) =>
            new ConnectionProbeResult(false, channel, failure.TimedOut, failure.Message, elapsedMs);
    }
}
