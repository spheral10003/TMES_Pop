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
    ///  2. 연결되면 주기적으로 Alive(상태 레지스터 Read)를 보낸다.
    ///  3. 통신이 끊기면 Disconnect 이벤트 핸들러가 동작해서 즉시 재연결을 시도한다.
    ///  4. 재연결은 최대 <see cref="MaxRetryCount"/>(3)회. 3회를 넘기면
    ///     연결 실패 Alarm 이벤트를 발생시키고 시퀀스(Thread)를 종료한다.
    /// </summary>
    public sealed class NutrunnerSequence : ThreadBase, INutrunnerSequence
    {
        /// <summary>최대 재연결 시도 횟수.</summary>
        public const int MaxRetryCount = 3;

        private const double RetryIntervalMs = 3000;
        private const double AliveIntervalMs = 5000;

        private readonly INutrunner _nutrunner;
        private readonly Stopwatch _retryTimer = new();
        private readonly Stopwatch _aliveTimer = new();

        private volatile bool _firstAttempt;
        private volatile bool _disposed;
        private int _retryCount;

        public event EventHandler<AlarmEventArgs>? AlarmOccurred;

        public NutrunnerSequence(INutrunner nutrunner)
        {
            _nutrunner = nutrunner ?? throw new ArgumentNullException(nameof(nutrunner));

            _nutrunner.Connected += OnConnected;
            _nutrunner.Disconnected += OnDisconnected;
            _nutrunner.ErrorOccurred += OnErrorOccurred;
        }

        public int Id => _nutrunner.Id;

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
            if (_nutrunner.IsConnected)
            {
                KeepAlive();
                return;
            }

            TryReconnect();
        }

        private void KeepAlive()
        {
            if (_aliveTimer.Elapsed.TotalMilliseconds < AliveIntervalMs)
            {
                return;
            }

            _aliveTimer.Restart();

            // Request 채널은 살아 있는데 Event Transport 채널만 끊긴 경우 다시 붙인다.
            _nutrunner.ConnectEventChannel();

            _nutrunner.ReadRegisters((ushort)NutrunnerRegister.Status, 1);
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
            _aliveTimer.Restart();

            Console.WriteLine($"{e.Id} , 연결 {e.IsConnected}");

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
            AlarmOccurred?.Invoke(this, new AlarmEventArgs(Id, code, message));
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

            base.Dispose();

            _nutrunner.Dispose();
        }
    }
}
