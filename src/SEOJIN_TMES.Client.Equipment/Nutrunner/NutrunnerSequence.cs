using System;
using System.Diagnostics;
using SEOJIN_TMES.Client.Equipment.Core;

namespace SEOJIN_TMES.Client.Equipment.Nutrunner
{
    /// <summary>
    /// <see cref="ThreadBase"/> 기반의 Nutrunner 통신 시퀀스.
    ///
    /// 동작 규칙
    ///  1. Start() 하면 Thread 가 뜨고, 연결이 유지되는 동안 계속 동작한다.
    ///  2. 연결되면 5초마다 Alive(상태 레지스터 Read)를 보낸다.
    ///     장비는 15초 동안 Alive가 없으면 스스로 연결을 끊으므로 이 주기를 늘리지 말 것.
    ///  3. Alive 응답이 응답 대기 시간(App.config ModbusConnectTimeoutMs) 안에 오지 않으면 바로 다시 보낸다.
    ///     <see cref="AliveMaxRetry"/>(3)회 다시 보내도 응답이 없으면 연결 상태를 확인하고 끊은 뒤 재연결한다.
    ///  4. 통신이 끊기면 Disconnect 이벤트 핸들러가 동작해서 재연결을 시도한다.
    ///  5. 재연결은 최대 <see cref="MaxRetryCount"/>(3)회. 3회를 넘기면
    ///     연결 실패 Alarm 이벤트를 발생시키고 시퀀스(Thread)를 종료한다.
    /// </summary>
    public sealed class NutrunnerSequence : ThreadBase, INutrunnerSequence
    {
        /// <summary>최대 재연결 시도 횟수.</summary>
        public const int MaxRetryCount = 3;

        /// <summary>Alive 응답이 없을 때 다시 보내는 횟수. 이 횟수를 넘기면 연결을 끊고 재연결한다.</summary>
        public const int AliveMaxRetry = 3;

        private const double RetryIntervalMs = 3000;
        private const double AliveIntervalMs = 5000;

        /// <summary>ModbusConnectTimeoutMs가 0 이하(접속은 OS 기본값)일 때 쓰는 Alive 응답 대기 시간.</summary>
        private const double DefaultAliveResponseTimeoutMs = 1000;

        private readonly INutrunner _nutrunner;
        private readonly Stopwatch _retryTimer = new();
        private readonly Stopwatch _aliveTimer = new();
        private readonly Stopwatch _aliveResponseTimer = new();

        private volatile bool _firstAttempt;
        private volatile bool _disposed;
        private int _retryCount;

        /// <summary>Alive를 보내고 응답을 기다리는 중인지. 응답은 수신 스레드에서 지우므로 volatile.</summary>
        private volatile bool _aliveWaiting;
        private int _aliveRetry;

        public event EventHandler<AlarmEventArgs>? AlarmOccurred;

        public NutrunnerSequence(INutrunner nutrunner)
        {
            _nutrunner = nutrunner ?? throw new ArgumentNullException(nameof(nutrunner));

            _nutrunner.Connected += OnConnected;
            _nutrunner.Disconnected += OnDisconnected;
            _nutrunner.ErrorOccurred += OnErrorOccurred;
            _nutrunner.RegisterDataReceived += OnRegisterDataReceived;
        }

        public int Id => _nutrunner.Id;

        private double AliveResponseTimeoutMs
            => _nutrunner.Info.ConnectTimeoutMs > 0 ? _nutrunner.Info.ConnectTimeoutMs : DefaultAliveResponseTimeoutMs;

        public INutrunner Nutrunner => _nutrunner;

        public int RetryCount => _retryCount;

        public override void Start()
        {
            _retryCount = 0;
            _firstAttempt = true;      // 최초 연결은 대기 없이 바로 시도한다.
            _retryTimer.Restart();
            _aliveTimer.Restart();

            base.Start();
        }

        public override void Stop()
        {
            _retryTimer.Stop();
            _aliveTimer.Stop();

            base.Stop();
        }

        /// <summary>Thread 1 tick.</summary>
        protected override void Execute()
        {
            try
            {
                if (_nutrunner.IsConnected)
                {
                    // 핸드셰이크는 Alive 주기(5초)와 별도로, 50ms tick 마다 확인한다.
                    _nutrunner.ProcessEventHandshake();
                    KeepAlive();
                    return;
                }

                TryReconnect();
            }
            catch (Exception ex)
            {
                // 예외가 ThreadBase 루프 밖으로 나가면 시퀀스가 끝나 다시는 재연결하지 않는다 —
                // 끊긴 소켓을 만지다 난 예외로 보고, 알린 뒤 연결을 정리해 다음 tick에 재연결하게 한다.
                RaiseAlarm(AlarmCode.CommunicationError, $"통신 예외 - 재연결합니다. ({ex.GetType().Name}: {ex.Message})");
                ForceReconnect();
            }
        }

        private void KeepAlive()
        {
            if (_aliveWaiting)
            {
                CheckAliveResponse();
                return;
            }

            if (_aliveTimer.Elapsed.TotalMilliseconds < AliveIntervalMs)
            {
                return;
            }

            _aliveTimer.Restart();

            // Request 채널은 살아 있는데 Event Transport 채널만 끊긴 경우 다시 붙인다.
            _nutrunner.ConnectEventChannel();

            _aliveRetry = 0;
            SendAlive();
        }

        /// <summary>
        /// 대기 중인 Alive의 응답 시간을 본다. 시간을 넘기면 바로 다시 보내고,
        /// <see cref="AliveMaxRetry"/>회 다시 보내도 응답이 없으면 연결을 확인해 재연결한다.
        /// </summary>
        private void CheckAliveResponse()
        {
            if (_aliveResponseTimer.Elapsed.TotalMilliseconds < AliveResponseTimeoutMs)
            {
                return;
            }

            if (_aliveRetry < AliveMaxRetry)
            {
                _aliveRetry++;
                SendAlive();
                return;
            }

            _aliveWaiting = false;

            // 소켓이 "연결됨"으로 나와도 끊는다 — 신호 없이 끊긴 연결(랜선 · 전원)은 Poll로 알 수 없고,
            // 응답 없는 연결을 붙잡고 있으면 장비가 15초 뒤 먼저 끊어 그동안의 체결 이벤트를 놓친다.
            string socketState = _nutrunner.IsConnected ? "소켓 연결 유지(응답 없음)" : "소켓 끊김";
            RaiseAlarm(AlarmCode.CommunicationError,
                $"Alive 응답 없음({AliveMaxRetry + 1}회, {AliveResponseTimeoutMs}ms) - {socketState}. 재연결합니다.");
            ForceReconnect();
        }

        private void SendAlive()
        {
            // 응답이 송신 직후 바로 올 수 있으므로 대기 표시를 먼저 한다.
            _aliveWaiting = true;
            _aliveResponseTimer.Restart();
            _nutrunner.ReadRegisters((ushort)NutrunnerRegister.Status, 1);
        }

        /// <summary>
        /// 연결을 끊고 다음 tick에 바로 재연결하게 한다(재연결 3회 · 실패 알람 흐름은 <see cref="TryReconnect"/> 그대로).
        /// </summary>
        private void ForceReconnect()
        {
            _aliveWaiting = false;
            _aliveRetry = 0;

            try
            {
                _nutrunner.Disconnect();
            }
            catch (Exception ex)
            {
                // 이미 끊긴 소켓 정리 중 예외 — 재연결이 새 소켓을 만들므로 무시해도 된다.
                RaiseAlarm(AlarmCode.CommunicationError, $"연결 정리 중 예외 ({ex.GetType().Name}: {ex.Message})");
            }

            _firstAttempt = true;
        }

        /// <summary>
        /// Request 채널의 응답 수신(수신 스레드). 어떤 응답이든 장비가 살아 있다는 뜻이라 Alive 대기를 끝낸다 —
        /// Alive 응답만 골라내지 않는 것은, 토크 쓰기 등 다른 요청과 겹치면 응답의 시작 주소가 바뀌어 나오기 때문이다.
        /// </summary>
        private void OnRegisterDataReceived(object? sender, NutrunnerRegisterEventArgs e)
        {
            _aliveWaiting = false;
        }

        private void TryReconnect()
        {
            if (!_firstAttempt && _retryTimer.Elapsed.TotalMilliseconds < RetryIntervalMs)
            {
                return;
            }

            if (_retryCount >= MaxRetryCount)
            {
                RaiseAlarm(AlarmCode.ConnectFailed,
                    $"재연결 {MaxRetryCount}회 시도 실패. 시퀀스를 종료합니다. ({_nutrunner.Info.Ip}:{_nutrunner.Info.Port})");
                Stop();
                return;
            }

            _firstAttempt = false;
            _retryCount++;
            _retryTimer.Restart();
            _nutrunner.Connect();
        }

        private void OnConnected(object? sender, EquipmentConnectionEventArgs e)
        {
            _retryCount = 0;
            _firstAttempt = false;
            _aliveWaiting = false;
            _aliveRetry = 0;
            _aliveTimer.Restart();

            Console.WriteLine($"{e.Code} , 연결 {e.IsConnected}");

        }

        /// <summary>
        /// Disconnect 이벤트 핸들러.
        /// 재연결 타이머를 다시 돌려서, 끊긴 시점부터 <see cref="RetryIntervalMs"/> 뒤에
        /// 루프가 재연결을 시도하도록 한다. (연결 실패로 인한 재시도 폭주 방지)
        /// </summary>
        private void OnDisconnected(object? sender, EquipmentConnectionEventArgs e)
        {
            _retryTimer.Restart();
        }

        private void OnErrorOccurred(object? sender, Exception exception)
        {
            // 재연결 중에 반복해서 올라오는 연결 실패 예외는 알람으로 올리지 않는다.
            if (!_nutrunner.IsConnected)
            {
                return;
            }

            RaiseAlarm(AlarmCode.CommunicationError, exception.Message);
        }

        protected override void OnError(Exception exception)
        {
            RaiseAlarm(AlarmCode.CommunicationError, exception.Message);
        }

        private void RaiseAlarm(AlarmCode code, string message)
        {
            AlarmOccurred?.Invoke(this, new AlarmEventArgs(_nutrunner.Info.Code, code, message));
        }
            

        public override void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            _nutrunner.Connected -= OnConnected;
            _nutrunner.Disconnected -= OnDisconnected;
            _nutrunner.ErrorOccurred -= OnErrorOccurred;
            _nutrunner.RegisterDataReceived -= OnRegisterDataReceived;

            base.Dispose();

            _nutrunner.Dispose();
        }
    }
}
