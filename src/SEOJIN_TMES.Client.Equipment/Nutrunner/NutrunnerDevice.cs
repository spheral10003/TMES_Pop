using System;
using SEOJIN_TMES.Client.Equipment.Core;
using SEOJIN_TMES.Client.Modbus;
using SEOJIN_TMES.Client.Modbus.Abstractions;
using SEOJIN_TMES.Client.Modbus.Protocol;
using SEOJIN_TMES.Client.Modbus.Transport;

namespace SEOJIN_TMES.Client.Equipment.Nutrunner
{
    /// <summary>
    /// <see cref="INutrunner"/> 기본 구현체.
    ///
    /// 통신은 전적으로 SEOJIN_TMES.Client.Modbus 에 위임한다.
    ///  - Request 채널     : <see cref="IModbusClient"/> (TransactionId = 장비 ID, ProtocolId = 0)
    ///  - Event Transport  : <see cref="IModbusTransport"/> 원시 채널 + Nutrunner 전용 파서 (ProtocolId = 1)
    ///
    /// 장비 고유 로직(이벤트 포맷, 핸드셰이크, Ack)은 이 클래스 안에만 존재한다.
    /// </summary>
    public sealed class NutrunnerDevice : INutrunner
    {
        private readonly IModbusClient _client;
        private readonly IModbusTransport _eventTransport;
        private readonly IModbusTransport? _ownedRequestTransport;

        /// <summary>요청 송신 직렬화용. (시퀀스 Thread / 소켓 콜백 / UI Thread 가 같이 쓴다)</summary>
        private readonly object _requestSync = new();

        private bool _disposed;

        public event EventHandler<EquipmentConnectionEventArgs>? Connected;
        public event EventHandler<EquipmentConnectionEventArgs>? Disconnected;
        public event EventHandler<Exception>? ErrorOccurred;
        public event EventHandler<NutrunnerRegisterEventArgs>? RegisterDataReceived;
        public event EventHandler<NutrunnerFasteningEventArgs>? FasteningReceived;

        public NutrunnerInfo Info { get; }

        public int Id => Info.Id;

        public string Name => Info.DisplayName;

        public bool IsConnected => _client.IsConnected;

        public bool IsEventChannelConnected => _eventTransport.IsConnected;

        public FasteningData LastFastening { get; private set; }

        /// <summary>
        /// 기본 생성자. 장비 ID로 소켓/TransactionId 를 구분해서 여러 대를 동시에 운용할 수 있다.
        /// </summary>
        public NutrunnerDevice(NutrunnerInfo info)
            : this(info,
                   new TcpModbusTransport(info.Id * 2),
                   new TcpModbusTransport((info.Id * 2) + 1))
        {
        }

        private NutrunnerDevice(NutrunnerInfo info, IModbusTransport requestTransport, IModbusTransport eventTransport)
            : this(info,
                   new ModbusTcpClient(requestTransport, new ModbusTcpCodec(), info.TransactionId),
                   eventTransport)
        {
            // 직접 만든 Request 전송은 이 클래스가 Dispose 책임을 진다.
            _ownedRequestTransport = requestTransport;
        }

        /// <summary>
        /// 통신 구현을 직접 주입하는 생성자 (테스트 더블 / 다른 전송 방식용).
        /// </summary>
        public NutrunnerDevice(NutrunnerInfo info, IModbusClient client, IModbusTransport eventTransport)
        {
            Info = info ?? throw new ArgumentNullException(nameof(info));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _eventTransport = eventTransport ?? throw new ArgumentNullException(nameof(eventTransport));

            _client.Connected += OnClientConnected;
            _client.Disconnected += OnClientDisconnected;
            _client.ErrorOccurred += OnError;
            _client.ResponseReceived += OnResponseReceived;

            _eventTransport.Connected += OnEventTransportConnected;
            _eventTransport.DataReceived += OnEventDataReceived;
            _eventTransport.ErrorOccurred += OnError;
        }

        public void Connect() => _client.Connect(Info.Ip, Info.Port);

        public void ConnectEventChannel()
        {
            if (!_client.IsConnected || _eventTransport.IsConnected)
            {
                return;
            }

            _eventTransport.Connect(Info.Ip, Info.ResolvedEventPort);
        }

        public void Disconnect()
        {
            _eventTransport.Disconnect();
            _client.Disconnect();
        }

        public void ReadRegisters(ushort address, ushort count,
            ModbusFunctionCode function = ModbusFunctionCode.ReadInputRegisters)
        {
            lock (_requestSync)
            {
                _client.ReadRegisters(address, count, function);
            }
        }

        public void WriteRegister(ushort address, ushort value,
            ModbusFunctionCode function = ModbusFunctionCode.WriteSingleRegister)
        {
            lock (_requestSync)
            {
                _client.WriteRegister(address, value, function);
            }
        }

        public void WriteRegister(NutrunnerRegister register, ushort value)
            => WriteRegister((ushort)register, value);

        private void OnClientConnected(object? sender, EventArgs e)
        {
            // Request 채널이 붙으면 Event Transport 채널도 연결한다.
            _eventTransport.Connect(Info.Ip, Info.ResolvedEventPort);
            Connected?.Invoke(this, new EquipmentConnectionEventArgs(Id, true));
        }

        private void OnClientDisconnected(object? sender, EventArgs e)
        {
            _eventTransport.Disconnect();
            Disconnected?.Invoke(this, new EquipmentConnectionEventArgs(Id, false));
        }

        private void OnEventTransportConnected(object? sender, EventArgs e)
        {
            // 이벤트 채널 핸드셰이크: EventDataEnable 레지스터 ON.
            WriteRegister(NutrunnerRegister.EventDataEnable, 2);
        }

        private void OnResponseReceived(object? sender, ModbusResponse response)
        {
            // TransactionId 로 장비를 구분한다. (0 은 장비가 echo 하지 않는 경우 대비)
            if (response.TransactionId != 0 && response.TransactionId != Info.TransactionId)
            {
                return;
            }

            RegisterDataReceived?.Invoke(this,
                new NutrunnerRegisterEventArgs(Id, !response.IsError, response.StartAddress, response.Data));
        }

        private void OnEventDataReceived(object? sender, byte[] data)
        {
            if (!NutrunnerEventParser.TryParse(data, out FasteningData fastening))
            {
                return;
            }

            LastFastening = fastening;
            FasteningReceived?.Invoke(this, new NutrunnerFasteningEventArgs(Id, fastening));

            // 수신 확인(Ack).
            WriteRegister(NutrunnerRegister.EventAck, 1);
        }

        private void OnError(object? sender, Exception exception) => ErrorOccurred?.Invoke(this, exception);

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            _client.Connected -= OnClientConnected;
            _client.Disconnected -= OnClientDisconnected;
            _client.ErrorOccurred -= OnError;
            _client.ResponseReceived -= OnResponseReceived;

            _eventTransport.Connected -= OnEventTransportConnected;
            _eventTransport.DataReceived -= OnEventDataReceived;
            _eventTransport.ErrorOccurred -= OnError;

            _client.Dispose();
            _eventTransport.Dispose();
            _ownedRequestTransport?.Dispose();
        }
    }
}
