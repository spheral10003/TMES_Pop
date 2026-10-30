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
    ///  3. Alive 응답이 응답 대기 시간(App.config ModbusResponseTimeoutMs) 안에 오지 않으면 바로 다시 보낸다.
    ///     <see cref="AliveMaxRetry"/>(3)회 다시 보내도 응답이 없으면 연결 상태를 확인하고 끊은 뒤 재연결한다.
    ///  4. 통신이 끊기면 Disconnect 이벤트 핸들러가 동작해서 재연결을 시도한다.
    ///  5. 재연결은 <b>연결될 때까지 계속</b>한다. 처음 <see cref="MaxRetryCount"/>(3)회는 5초 간격,
    ///     그 뒤로는 10초 → 20초 → 30초(최대)로 간격을 늘린다. 3회 연속 실패하면 연결 실패 Alarm을 <b>한 번</b> 올린다.
    ///
    /// 예전에는 3회(3초 간격) 실패하면 시퀀스를 끝냈다. 그런데 너트러너는 Request 포트에 동시 접속을 1개만 받고,
    /// 접속이 실패하거나 끊긴 뒤 3~6초 동안 새 접속을 받지 않는다(2026-10 실측, 172.16.151.70/71) —
    /// 잠깐의 끊김에도 약 9초 만에 포기해 앱을 다시 켤 때까지 장비를 못 쓰는 일이 생겨 포기하지 않게 바꿨다.
    /// </summary>
    public sealed class NutrunnerSequence : ThreadBase, INutrunnerSequence
    {
        /// <summary>연결 실패 Alarm을 올리는 연속 재연결 실패 횟수. 이 뒤로도 재연결은 계속한다.</summary>
        public const int MaxRetryCount = 3;

        /// <summary>Alive 응답이 없을 때 다시 보내는 횟수. 이 횟수를 넘기면 연결을 끊고 재연결한다.</summary>
        public const int AliveMaxRetry = 3;

        /// <summary>재연결 기본 간격. 장비가 접속 실패 뒤 다시 받기까지 3~6초가 걸려 그보다 짧으면 연달아 실패한다.</summary>
        private const double RetryIntervalMs = 5000;

        /// <summary>오래 끊겨 있을 때 재연결 간격의 상한. 장비가 돌아오면 이 시간 안에는 다시 붙는다.</summary>
        private const double MaxRetryIntervalMs = 30000;

        private const double AliveIntervalMs = 5000;

        /// <summary>ModbusResponseTimeoutMs가 0 이하일 때 쓰는 Alive 응답 대기 시간.</summary>
        private const double DefaultAliveResponseTimeoutMs = 1000;

        private readonly INutrunner _nutrunner;
        private readonly Stopwatch _retryTimer = new();
        private readonly Stopwatch _aliveTimer = new();
        private readonly Stopwatch _aliveResponseTimer = new();

        private volatile bool _firstAttempt;
        private volatile bool _disposed;
        private int _retryCount;

        /// <summary>이번 끊김에서 연결 실패 Alarm을 이미 올렸는지. 다시 붙을 때까지 같은 알람을 반복하지 않는다.</summary>
        private bool _connectFailedRaised;

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

        /// <summary>
        /// Alive 응답 대기 시간. 접속 대기(ConnectTimeoutMs)와 따로 둔다 — 예전에는 같은 500ms를 써서
        /// 장비가 체결 · 설정 처리로 잠깐만 늦어도 응답 없음으로 보고 연결을 끊었다.
        /// </summary>
        private double AliveResponseTimeoutMs
            => _nutrunner.Info.ResponseTimeoutMs > 0 ? _nutrunner.Info.ResponseTimeoutMs : DefaultAliveResponseTimeoutMs;

        /// <summary>
        /// 다음 재연결까지 기다릴 시간. 처음 <see cref="MaxRetryCount"/>회는 기본 간격,
        /// 그 뒤로는 두 배씩 늘려 <see cref="MaxRetryIntervalMs"/>에서 멈춘다(장비가 꺼져 있을 때 로그 · 서버 로그 폭주 방지).
        /// </summary>
        private double CurrentRetryIntervalMs
        {
            get
            {
                if (_retryCount <= MaxRetryCount)
                    return RetryIntervalMs;

                double interval = RetryIntervalMs * Math.Pow(2, _retryCount - MaxRetryCount);
                return Math.Min(interval, MaxRetryIntervalMs);
            }
        }

        public INutrunner Nutrunner => _nutrunner;

        public int RetryCount => _retryCount;

        public override void Start()
        {
            _retryCount = 0;
            _connectFailedRaised = false;
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
        /// 연결을 끊고 다음 tick에 바로 재연결하게 한다(이후 재시도 · 실패 알람 흐름은 <see cref="TryReconnect"/> 그대로).
        /// 우리가 정상적으로 끊은 직후의 첫 접속은 장비가 바로 받아 준다(실측) — 막히는 건 접속 실패가 쌓인 뒤다.
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
        /// Alive 응답만 골라내지 않는 것은, 요청이 하나씩 차례로 나가므로(ModbusTcpClient) 토크 쓰기 등 앞선 요청이 밀려 있으면
        /// Alive 응답이 대기 시간보다 늦게 오는데, 그동안 받은 다른 응답도 장비가 살아 있다는 증거이기 때문이다.
        /// </summary>
        private void OnRegisterDataReceived(object? sender, NutrunnerRegisterEventArgs e)
        {
            _aliveWaiting = false;
        }

        private void TryReconnect()
        {
            if (!_firstAttempt && _retryTimer.Elapsed.TotalMilliseconds < CurrentRetryIntervalMs)
            {
                return;
            }

            // 포기하지 않는다 — 알람만 한 번 올리고 간격을 늘려 계속 시도한다(클래스 설명 5번 참고).
            if (_retryCount >= MaxRetryCount && !_connectFailedRaised)
            {
                _connectFailedRaised = true;
                RaiseAlarm(AlarmCode.ConnectFailed,
                    $"재연결 {MaxRetryCount}회 시도 실패. 최대 {MaxRetryIntervalMs / 1000:0}초 간격으로 계속 재시도합니다. ({_nutrunner.Info.Ip}:{_nutrunner.Info.Port})");
            }

            _firstAttempt = false;
            _retryCount++;
            _retryTimer.Restart();
            _nutrunner.Connect();
        }

        private void OnConnected(object? sender, EquipmentConnectionEventArgs e)
        {
            _retryCount = 0;
            _connectFailedRaised = false;
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
