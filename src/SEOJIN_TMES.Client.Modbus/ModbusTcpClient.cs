using SEOJIN_TMES.Client.Modbus.Abstractions;
using SEOJIN_TMES.Client.Modbus.Protocol;
using SEOJIN_TMES.Client.Modbus.Transport;
using System;
using System.Collections.Generic;

namespace SEOJIN_TMES.Client.Modbus
{
    /// <summary>
    /// <see cref="IModbusClient"/>의 기본 구현체.
    /// 내부적으로 <see cref="IModbusTransport"/>(소켓 송수신)와
    /// <see cref="IModbusPacketCodec"/>(패킷 조립/해석)를 조합해서 동작하며,
    /// 이 두 구현은 생성자로 주입할 수 있어 테스트/교체가 쉽다.
    /// 기본 생성자는 기존과 동일하게 KwonLib 기반 TCP 소켓을 사용한다.
    /// </summary>
    public sealed class ModbusTcpClient : IModbusClient
    {
        private readonly IModbusTransport _transport;
        private readonly IModbusPacketCodec _codec;
        private readonly bool _ownsTransport;

        private ModbusRequest _lastRequest;
        private bool _disposed;

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

        /// <summary>
        /// 이 클라이언트가 보내는 모든 요청에 사용할 Transaction ID.
        /// 기존 구현과 동일하게 요청마다 증가시키지 않고, 장비(연결)를 구분하는
        /// 고정 값으로 사용한다.
        /// </summary>
        public ushort TransactionId { get; set; }

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
            var request = new ModbusRequest(TransactionId, address, count, function, unitId);
            _lastRequest = request;
            _transport.Send(_codec.EncodeRead(request));
        }

        public void WriteRegister(ushort address, ushort value,
            ModbusFunctionCode function = ModbusFunctionCode.WriteSingleRegister, byte unitId = 0x00)
        {
            var request = new ModbusRequest(TransactionId, address, value, function, unitId);
            _lastRequest = request;
            _transport.Send(_codec.EncodeWrite(request));
        }

        private void OnTransportConnected(object? sender, EventArgs e)
        {
            lock (_rxSync) { _rxBuffer.Clear(); }
            Connected?.Invoke(this, EventArgs.Empty);
        }

        private void OnTransportDisconnected(object? sender, EventArgs e) => Disconnected?.Invoke(this, EventArgs.Empty);

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
                ResponseReceived?.Invoke(this, _codec.Decode(frame, _lastRequest));
            }
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

            if (_ownsTransport)
            {
                _transport.Dispose();
            }

            _disposed = true;
        }
    }
}
