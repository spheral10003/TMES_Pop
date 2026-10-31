using SEOJIN_TMES.Client.Modbus.Abstractions;
using SEOJIN_TMES.Client.Modbus.Protocol;
using SEOJIN_TMES.Client.Modbus.Transport;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace SEOJIN_TMES.Client.Modbus
{
    /// <summary>
    /// <see cref="IModbusClient"/>의 기본 구현체.
    /// 내부적으로 <see cref="IModbusTransport"/>(소켓 송수신)와
    /// <see cref="IModbusPacketCodec"/>(패킷 조립/해석)를 조합해서 동작하며,
    /// 이 두 구현은 생성자로 주입할 수 있어 테스트/교체가 쉽다.
    /// 기본 생성자는 기존과 동일하게 KwonLib 기반 TCP 소켓을 사용한다.
    ///
    /// 요청은 한 번에 하나만 보낸다(응답 또는 <see cref="ResponseTimeoutMs"/> 초과 후 다음 요청).
    /// 토크 → 프리셋 변경 → 잠금 해제처럼 연달아 쓰면 장비가 한 TCP 조각에 붙은 뒤쪽 요청을 버리는 일이 있었고,
    /// TransactionId가 고정이라 응답이 어느 요청의 것인지도 가릴 수 없었다.
    /// </summary>
    public sealed class ModbusTcpClient : IModbusClient
    {
        private readonly IModbusTransport _transport;
        private readonly IModbusPacketCodec _codec;
        private readonly bool _ownsTransport;

        /// <summary>마지막으로 끝난(응답 · 시간 초과) 요청. 늦게 온 응답을 해석할 때 쓴다.</summary>
        private ModbusRequest _lastRequest;
        private bool _disposed;

        /// <summary>ResponseTimeoutMs가 0 이하일 때 쓰는 응답 대기 시간.</summary>
        private const int DefaultResponseTimeoutMs = 1000;

        /// <summary>대기열 한도. 장비가 응답하지 않는 동안 요청이 끝없이 쌓이지 않게 한다.</summary>
        private const int MaxPendingRequests = 64;

        private readonly object _txSync = new();
        private readonly Queue<ModbusRequest> _pending = new();
        private readonly Stopwatch _inFlightTimer = new();
        private readonly Timer _responseTimer;
        private ModbusRequest? _inFlight;

        private const int MbapPrefixLength = 6;

        /// <summary>Modbus TCP 프레임 최대 길이. 이보다 길면 스트림이 어긋난 것으로 보고 버린다.</summary>
        private const int MaxFrameLength = 260;

        private readonly List<byte> _rxBuffer = new();
        private readonly object _rxSync = new();


        public event EventHandler? Connected;
        public event EventHandler? Disconnected;
        public event EventHandler<Exception>? ErrorOccurred;
        public event EventHandler<ModbusResponse>? ResponseReceived;

        public bool IsConnected => _transport.IsConnected;

        public bool HasPendingRequests
        {
            get { lock (_txSync) { return _inFlight.HasValue || _pending.Count > 0; } }
        }

        /// <summary>
        /// 이 클라이언트가 보내는 모든 요청에 사용할 Transaction ID.
        /// 기존 구현과 동일하게 요청마다 증가시키지 않고, 장비(연결)를 구분하는
        /// 고정 값으로 사용한다.
        /// </summary>
        public ushort TransactionId { get; set; }

        /// <summary>
        /// 요청 하나의 응답을 기다리는 시간(ms). 넘기면 그 요청은 실패로 보고(<see cref="ErrorOccurred"/>) 다음 요청을 보낸다.
        /// 0 이하면 <see cref="DefaultResponseTimeoutMs"/>.
        /// </summary>
        public int ResponseTimeoutMs { get; set; } = DefaultResponseTimeoutMs;

        private int EffectiveResponseTimeoutMs => ResponseTimeoutMs > 0 ? ResponseTimeoutMs : DefaultResponseTimeoutMs;

        /// <summary>
        /// 기본 생성자. KwonLib 기반 TCP 소켓 전송(<see cref="TcpModbusTransport"/>)과
        /// 표준 Modbus TCP 코덱(<see cref="ModbusTcpCodec"/>)을 사용한다.
        /// </summary>
        public ModbusTcpClient(ushort transactionId = 1)
            : this(new TcpModbusTransport(), new ModbusTcpCodec(), transactionId, ownsTransport: true)
        {
        }

        /// <summary>
        /// 전송/코덱 구현을 직접 주입하는 생성자. 테스트 더블이나 다른 전송 방식을
        /// 사용하고 싶을 때 사용한다. 이 경우 전송 객체의 Dispose는 호출자가 책임진다.
        /// </summary>
        public ModbusTcpClient(IModbusTransport transport, IModbusPacketCodec codec, ushort transactionId = 1)
            : this(transport, codec, transactionId, ownsTransport: false)
        {
        }

        private ModbusTcpClient(IModbusTransport transport, IModbusPacketCodec codec, ushort transactionId, bool ownsTransport)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
            _codec = codec ?? throw new ArgumentNullException(nameof(codec));
            _ownsTransport = ownsTransport;
            TransactionId = transactionId;
            _responseTimer = new Timer(OnResponseTimeout, null, Timeout.Infinite, Timeout.Infinite);

            _transport.Connected += OnTransportConnected;
            _transport.Disconnected += OnTransportDisconnected;
            _transport.DataReceived += OnTransportDataReceived;
            _transport.ErrorOccurred += OnTransportError;
        }

        public void Connect(string host, int port) => _transport.Connect(host, port);

        public void Disconnect() => _transport.Disconnect();

        public void ReadRegisters(ushort address, ushort count,
            ModbusFunctionCode function = ModbusFunctionCode.ReadInputRegisters, byte unitId = 0x00)
        {
            Enqueue(new ModbusRequest(TransactionId, address, count, function, unitId));
        }

        public void WriteRegister(ushort address, ushort value,
            ModbusFunctionCode function = ModbusFunctionCode.WriteSingleRegister, byte unitId = 0x00)
        {
            Enqueue(new ModbusRequest(TransactionId, address, value, function, unitId));
        }

        /// <summary>응답을 기다리는 요청이 없으면 바로 보내고, 있으면 대기열 뒤에 붙인다.</summary>
        private void Enqueue(ModbusRequest request)
        {
            lock (_txSync)
            {
                if (_inFlight.HasValue)
                {
                    if (_pending.Count >= MaxPendingRequests)
                    {
                        ErrorOccurred?.Invoke(this, new InvalidOperationException(
                            $"Modbus 요청 대기열이 가득 차 버립니다. (주소 {request.Address}, 대기 {_pending.Count}건)"));
                        return;
                    }

                    _pending.Enqueue(request);
                    return;
                }

                BeginRequest(request);
            }

            Send(request);
        }

        /// <summary>요청을 "응답 대기"로 표시하고 응답 시간을 잰다. _txSync 안에서 부른다.</summary>
        private void BeginRequest(ModbusRequest request)
        {
            _inFlight = request;
            _inFlightTimer.Restart();
            _responseTimer.Change(EffectiveResponseTimeoutMs, Timeout.Infinite);
        }

        /// <summary>
        /// 응답 대기를 끝내고 대기열의 다음 요청을 꺼낸다. _txSync 안에서 부른다.
        /// 송신은 잠금 밖에서 해야 한다 — 송신 실패가 끊김 이벤트로 바로 이어져 이 클래스로 되돌아올 수 있다.
        /// </summary>
        private ModbusRequest? CompleteRequest()
        {
            if (_inFlight.HasValue)
                _lastRequest = _inFlight.Value;

            _inFlight = null;
            _inFlightTimer.Reset();
            _responseTimer.Change(Timeout.Infinite, Timeout.Infinite);

            if (_pending.Count == 0)
                return null;

            ModbusRequest next = _pending.Dequeue();
            BeginRequest(next);
            return next;
        }

        private void Send(ModbusRequest request)
        {
            byte[] packet = request.FunctionCode == ModbusFunctionCode.WriteSingleRegister
                ? _codec.EncodeWrite(request)
                : _codec.EncodeRead(request);

            _transport.Send(packet);
        }

        /// <summary>응답 대기 시간 초과(타이머 스레드). 이 요청은 실패로 알리고 다음 요청을 보낸다.</summary>
        private void OnResponseTimeout(object? state)
        {
            ModbusRequest timedOut;
            ModbusRequest? next;

            lock (_txSync)
            {
                // 그 사이 응답이 와서 다음 요청으로 넘어갔으면 그 요청의 시간은 아직 남아 있다.
                if (!_inFlight.HasValue || _inFlightTimer.ElapsedMilliseconds < EffectiveResponseTimeoutMs)
                    return;

                timedOut = _inFlight.Value;
                next = CompleteRequest();
            }

            // 다음 요청을 먼저 보낸다 — 알림 처리에서 예외가 나도 대기열이 멈추지 않게.
            if (next.HasValue)
                Send(next.Value);

            ErrorOccurred?.Invoke(this, new TimeoutException(
                $"Modbus 응답 없음({EffectiveResponseTimeoutMs}ms) - {timedOut.FunctionCode} 주소 {timedOut.Address} 값 {timedOut.Value}"));
        }

        /// <summary>끊기거나 새로 붙으면 이전 연결에 보낸 요청의 응답은 오지 않는다 — 대기 중인 요청을 모두 버린다.</summary>
        private void ClearRequests()
        {
            lock (_txSync)
            {
                _pending.Clear();
                _inFlight = null;
                _inFlightTimer.Reset();
                _responseTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        private void OnTransportConnected(object? sender, EventArgs e)
        {
            lock (_rxSync) { _rxBuffer.Clear(); }
            ClearRequests();
            Connected?.Invoke(this, EventArgs.Empty);
        }

        private void OnTransportDisconnected(object? sender, EventArgs e)
        {
            ClearRequests();
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        private void OnTransportError(object? sender, Exception e) => ErrorOccurred?.Invoke(this, e);

        private void OnTransportDataReceived(object? sender, byte[] data)
        {
            var frames = new List<byte[]>();

            lock (_rxSync)
            {
                // TCP는 경계를 보장하지 않는다 — 응답 두 개가 한 번에 오거나 하나가 잘려서 올 수 있다.
                _rxBuffer.AddRange(data);

                while (_rxBuffer.Count >= MbapPrefixLength)
                {
                    int frameLength = MbapPrefixLength + ((_rxBuffer[4] << 8) | _rxBuffer[5]);

                    if (frameLength <= MbapPrefixLength || frameLength > MaxFrameLength)
                    {
                        _rxBuffer.Clear();      // 동기가 깨진 스트림은 다음 수신부터 다시 맞춘다.
                        break;
                    }

                    if (_rxBuffer.Count < frameLength)
                    {
                        break;                  // 나머지 조각을 기다린다.
                    }

                    frames.Add(_rxBuffer.GetRange(0, frameLength).ToArray());
                    _rxBuffer.RemoveRange(0, frameLength);
                }
            }

            foreach (byte[] frame in frames)
            {
                ModbusRequest request;
                ModbusRequest? next = null;

                lock (_txSync)
                {
                    if (IsResponseOf(frame, _inFlight))
                    {
                        request = _inFlight!.Value;
                        next = CompleteRequest();
                    }
                    else
                    {
                        // 시간 초과로 넘긴 요청의 응답이 늦게 왔다 — 지금 기다리는 요청의 응답으로 셈하지 않는다.
                        request = _lastRequest;
                    }
                }

                // 다음 요청을 먼저 보낸다 — 응답 처리(화면 · 로그)에서 예외가 나도 대기열이 멈추지 않게.
                if (next.HasValue)
                    Send(next.Value);

                ResponseReceived?.Invoke(this, _codec.Decode(frame, request));
            }
        }

        /// <summary>
        /// 응답 프레임의 기능 코드(오류 응답은 0x80을 뺀 값)가 기다리는 요청과 같은지.
        /// TransactionId가 장비마다 고정이라 요청을 가릴 수 있는 것은 기능 코드 · (쓰기면) 주소뿐이다.
        /// </summary>
        private bool IsResponseOf(byte[] frame, ModbusRequest? request)
        {
            const int FunctionCodeIndex = 7;
            const int WriteAddressIndex = 8;

            if (!request.HasValue || frame.Length <= FunctionCodeIndex)
                return false;

            byte function = frame[FunctionCodeIndex];
            bool isException = (function & 0x80) != 0;
            if ((function & 0x7F) != (byte)request.Value.FunctionCode)
                return false;

            // 쓰기 정상 응답은 주소를 그대로 돌려준다 — 주소까지 같아야 이 요청의 응답이다.
            if (!isException && request.Value.FunctionCode == ModbusFunctionCode.WriteSingleRegister
                && frame.Length > WriteAddressIndex + 1)
            {
                ushort address = (ushort)((frame[WriteAddressIndex] << 8) | frame[WriteAddressIndex + 1]);
                return address == request.Value.Address;
            }

            return true;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _transport.Connected -= OnTransportConnected;
            _transport.Disconnected -= OnTransportDisconnected;
            _transport.DataReceived -= OnTransportDataReceived;
            _transport.ErrorOccurred -= OnTransportError;

            _responseTimer.Dispose();

            if (_ownsTransport)
            {
                _transport.Dispose();
            }

            _disposed = true;
        }
    }
}
