using System;
using System.Net.Sockets;
using System.Threading;
using SEOJIN_MEIS.Client.Common.Logging;
using SEOJIN_MEIS.Client.Equipment.Protocol;

namespace SEOJIN_MEIS.Client.Equipment
{
    /// <summary>
    /// Modbus-TCP 연결 1개(= 소켓 1개 + 전용 수신 스레드 1개)의 공통 동작.
    /// 요구 구조("하나의 수신 지점 → 소켓별 멀티 스레드 클라이언트")에 따라 연결마다 스레드를 하나 소유하고,
    /// 블로킹 Read → MBAP 프레이밍 → 프레임 처리 순으로 동작한다.
    ///
    /// 표준 Connection(<see cref="EquipmentModbusClient"/>)과 이벤트 Connection(<see cref="EquipmentEventClient"/>)이
    /// 이 클래스를 상속하며, 차이는 "수신한 프레임을 어떻게 처리하는지"(<see cref="HandleFrame"/>)뿐이다.
    /// </summary>
    public abstract class ModbusTcpConnection : IDisposable
    {
        private const int ReceiveBufferSize = 4096;
        private const int ConnectTimeoutMs = 5000;

        private readonly object _sendLock = new object();

        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private Thread _receiveThread;
        private volatile bool _stopRequested;

        private byte[] _accumulateBuffer = new byte[ReceiveBufferSize];
        private int _accumulateLength;

        protected IModbusCodec Codec { get; }

        public string EquipmentId { get; }
        public string Host { get; }
        public int Port { get; }
        public byte UnitId { get; }
        public abstract EquipmentChannel Channel { get; }
        public EquipmentConnectionState State { get; private set; } = EquipmentConnectionState.Disconnected;

        public event EventHandler<EquipmentDataReceivedEventArgs> DataReceived;
        public event EventHandler<EquipmentConnectionChangedEventArgs> ConnectionChanged;
        public event EventHandler<EquipmentErrorEventArgs> ErrorOccurred;

        protected ModbusTcpConnection(string equipmentId, string host, int port, byte unitId, IModbusCodec codec)
        {
            EquipmentId = equipmentId;
            Host = host;
            Port = port;
            UnitId = unitId;
            Codec = codec ?? new ModbusTcpCodec();
        }

        /// <summary>
        /// 즉시 리턴한다 — 실제 TCP 접속은 전용 스레드 안에서 수행한다.
        /// (호출 스레드, 특히 UI 스레드를 장비 접속 지연/타임아웃으로 막지 않기 위함.
        ///  과거 버전은 여기서 TcpClient.Connect를 직접 호출해 UI 스레드를 막았었다 — 재발 방지 주석.)
        /// </summary>
        public void Connect()
        {
            if (State == EquipmentConnectionState.Connected || State == EquipmentConnectionState.Connecting)
                return;

            AppLog.Step($"[{EquipmentId}/{Channel}] Modbus-TCP 접속 시도 - {Host}:{Port}");
            SetState(EquipmentConnectionState.Connecting);
            _stopRequested = false;
            _accumulateLength = 0;
            _receiveThread = new Thread(ConnectAndReceiveLoop);
            _receiveThread.IsBackground = true;
            _receiveThread.Name = $"Modbus-{Channel}-{EquipmentId}";
            _receiveThread.Start();
        }

        public void Disconnect()
        {
            _stopRequested = true;
            try { _stream?.Close(); } catch { /* ignore */ }
            try { _tcpClient?.Close(); } catch { /* ignore */ }

            if (_receiveThread != null && _receiveThread.IsAlive && Thread.CurrentThread != _receiveThread)
                _receiveThread.Join(2000);

            OnDisconnected(new InvalidOperationException($"[{EquipmentId}/{Channel}] 연결이 종료되었습니다."));
            SetState(EquipmentConnectionState.Disconnected);
        }

        /// <summary>ADU 바이트를 그대로 전송한다(전송 자체는 소켓별 lock으로 직렬화).</summary>
        protected void SendRaw(byte[] adu)
        {
            if (State != EquipmentConnectionState.Connected || _stream == null)
                throw new InvalidOperationException($"설비 [{EquipmentId}/{Channel}]가 연결되어 있지 않습니다.");

            lock (_sendLock)
            {
                _stream.Write(adu, 0, adu.Length);
            }
        }

        private void ConnectAndReceiveLoop()
        {
            try
            {
                _tcpClient = new TcpClient();
                _tcpClient.NoDelay = true; // 소량 프레임의 실시간성이 중요하므로 Nagle 지연을 끈다.
                var connectTask = _tcpClient.ConnectAsync(Host, Port);
                if (!connectTask.Wait(ConnectTimeoutMs))
                    throw new TimeoutException($"{Host}:{Port} 접속 시간 초과({ConnectTimeoutMs}ms)");

                _stream = _tcpClient.GetStream();
                AppLog.Step($"[{EquipmentId}/{Channel}] Modbus-TCP 접속 성공 - {Host}:{Port}");
                SetState(EquipmentConnectionState.Connected);
            }
            catch (Exception ex)
            {
                try { _tcpClient?.Close(); } catch { /* ignore */ }

                if (!_stopRequested)
                {
                    AppLog.Warn($"[{EquipmentId}/{Channel}] Modbus-TCP 접속 실패 - {Host}:{Port} :: {ex.Message}");
                    SetState(EquipmentConnectionState.Faulted, ex.Message);
                    ErrorOccurred?.Invoke(this, new EquipmentErrorEventArgs(EquipmentId, ex, Channel));
                }
                return;
            }

            ReceiveLoop();
        }

        private void ReceiveLoop()
        {
            var readBuffer = new byte[ReceiveBufferSize];
            try
            {
                while (!_stopRequested)
                {
                    int bytesRead;
                    try
                    {
                        bytesRead = _stream.Read(readBuffer, 0, readBuffer.Length);
                    }
                    catch (Exception)
                    {
                        if (_stopRequested) break;
                        throw;
                    }

                    if (bytesRead == 0)
                        break; // 원격에서 연결 종료

                    AppendToAccumulateBuffer(readBuffer, bytesRead);
                    ExtractAndDispatchFrames();
                }
            }
            catch (Exception ex)
            {
                if (!_stopRequested)
                {
                    AppLog.Warn($"[{EquipmentId}/{Channel}] 수신 중 오류 - {ex.Message}");
                    OnDisconnected(ex);
                    SetState(EquipmentConnectionState.Faulted, ex.Message);
                    ErrorOccurred?.Invoke(this, new EquipmentErrorEventArgs(EquipmentId, ex, Channel));
                }
                return;
            }

            if (!_stopRequested)
            {
                AppLog.Step($"[{EquipmentId}/{Channel}] 원격 연결 종료");
                OnDisconnected(new InvalidOperationException("원격에서 연결을 종료했습니다."));
                SetState(EquipmentConnectionState.Disconnected, "원격 연결 종료");
            }
        }

        private void AppendToAccumulateBuffer(byte[] data, int length)
        {
            if (_accumulateLength + length > _accumulateBuffer.Length)
            {
                var newSize = Math.Max(_accumulateBuffer.Length * 2, _accumulateLength + length);
                Array.Resize(ref _accumulateBuffer, newSize);
            }
            Array.Copy(data, 0, _accumulateBuffer, _accumulateLength, length);
            _accumulateLength += length;
        }

        /// <summary>
        /// 누적 버퍼에서 꺼낼 수 있는 프레임을 모두 꺼내 처리한다.
        /// 코덱이 "버릴 바이트 수"를 알려준 경우(MBAP 동기 이탈)에도 그만큼 확실히 소비해 무한 루프를 막는다.
        /// </summary>
        private void ExtractAndDispatchFrames()
        {
            while (_accumulateLength > 0)
            {
                var extracted = Codec.TryExtractFrame(_accumulateBuffer, _accumulateLength, out var frame, out var consumed);

                if (consumed > 0)
                    RemoveConsumedBytes(consumed);

                if (extracted && frame != null)
                {
                    try
                    {
                        HandleFrame(frame);
                    }
                    catch (Exception ex)
                    {
                        AppLog.Warn($"[{EquipmentId}/{Channel}] 프레임 처리 중 오류 - {ex.Message}");
                        ErrorOccurred?.Invoke(this, new EquipmentErrorEventArgs(EquipmentId, ex, Channel));
                    }
                    continue;
                }

                if (consumed <= 0)
                    break; // 데이터가 더 도착해야 함
            }
        }

        private void RemoveConsumedBytes(int consumed)
        {
            var remaining = _accumulateLength - consumed;
            if (remaining > 0)
                Array.Copy(_accumulateBuffer, consumed, _accumulateBuffer, 0, remaining);
            _accumulateLength = remaining > 0 ? remaining : 0;
        }

        /// <summary>수신한 프레임 1개를 처리한다. 표준/이벤트 Connection이 각각 다르게 구현한다.</summary>
        protected abstract void HandleFrame(ModbusFrame frame);

        /// <summary>연결이 끊겼을 때의 정리 훅(대기 중인 요청 취소 등). 기본 동작 없음.</summary>
        protected virtual void OnDisconnected(Exception reason) { }

        protected void RaiseDataReceived(ModbusFrame frame) =>
            DataReceived?.Invoke(this, new EquipmentDataReceivedEventArgs(EquipmentId, frame, Channel));

        private void SetState(EquipmentConnectionState state, string message = null)
        {
            State = state;
            ConnectionChanged?.Invoke(this, new EquipmentConnectionChangedEventArgs(EquipmentId, state, message, Channel));
        }

        public void Dispose() => Disconnect();
    }
}
