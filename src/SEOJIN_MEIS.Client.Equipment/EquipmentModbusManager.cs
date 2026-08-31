using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SEOJIN_MEIS.Client.Common.Configuration;
using SEOJIN_MEIS.Client.Equipment.Protocol;

namespace SEOJIN_MEIS.Client.Equipment
{
    /// <summary>
    /// 설비별 Modbus-TCP Connection들을 등록/관리하고, 각 소켓에서 올라오는 이벤트를
    /// 하나의 지점(단일 이벤트)으로 모아 상위(POP 화면 등)에 전달한다.
    /// "One received -> socket multi-thread client" 구조의 통합 수신 지점 역할.
    ///
    /// 설비 1대당 스펙에 정의된 두 Connection을 각각 소켓 1개 + 전용 수신 스레드 1개로 유지한다.
    ///  - Standard Modbus-TCP connection   : 요청/응답. 모든 Read/Write는 여기로만 나간다.
    ///  - Customized Modbus-TCP connection : 포트+1. 실시간 이벤트 수신 전용(Read/Write 불가).
    /// 상위로 올라오는 모든 이벤트 인자에는 어느 Connection에서 온 것인지 <see cref="EquipmentChannel"/>이 함께 실린다.
    /// </summary>
    public class EquipmentModbusManager : IDisposable
    {
        private readonly ConcurrentDictionary<string, EquipmentConnections> _equipments =
            new ConcurrentDictionary<string, EquipmentConnections>();

        private readonly IModbusCodec _codec;

        public event EventHandler<EquipmentDataReceivedEventArgs> DataReceived;
        public event EventHandler<EquipmentConnectionChangedEventArgs> ConnectionChanged;
        public event EventHandler<EquipmentErrorEventArgs> ErrorOccurred;

        public EquipmentModbusManager(IModbusCodec codec = null)
        {
            _codec = codec ?? new ModbusTcpCodec();
        }

        public void Register(EquipmentEndpoint endpoint)
        {
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));
            if (_equipments.ContainsKey(endpoint.EquipmentId))
                return;

            var connections = new EquipmentConnections(endpoint);

            if (endpoint.UseStandardConnection)
            {
                connections.Standard = new EquipmentModbusClient(
                    endpoint.EquipmentId,
                    endpoint.Host,
                    endpoint.Port,
                    endpoint.UnitId,
                    endpoint.ResponseTimeoutMs,
                    endpoint.ResolveRegisterMap(),
                    _codec);
                Wire(connections.Standard);
            }

            if (endpoint.UseEventConnection)
            {
                connections.Event = new EquipmentEventClient(
                    endpoint.EquipmentId,
                    endpoint.Host,
                    endpoint.ResolveEventPort(),
                    endpoint.UnitId,
                    endpoint.EventAckMode,
                    _codec);
                Wire(connections.Event);
            }

            _equipments[endpoint.EquipmentId] = connections;

            if (endpoint.AutoConnect)
                Connect(endpoint.EquipmentId);
        }

        public void RegisterAll(IEnumerable<EquipmentEndpoint> endpoints)
        {
            if (endpoints == null) return;
            foreach (var endpoint in endpoints)
                Register(endpoint);
        }

        /// <summary>설정된 두 Connection을 모두 접속 시도한다(각 접속은 해당 소켓 전용 스레드에서 비동기 진행).</summary>
        public void Connect(string equipmentId)
        {
            var connections = GetOrThrow(equipmentId);
            connections.Standard?.Connect();
            connections.Event?.Connect();
        }

        public void Connect(string equipmentId, EquipmentChannel channel) => GetConnectionOrThrow(equipmentId, channel).Connect();

        public void Disconnect(string equipmentId)
        {
            var connections = GetOrThrow(equipmentId);
            connections.Standard?.Disconnect();
            connections.Event?.Disconnect();
        }

        public void Disconnect(string equipmentId, EquipmentChannel channel) => GetConnectionOrThrow(equipmentId, channel).Disconnect();

        /// <summary>논리 명령(Start/Stop/Reset/상태조회)을 표준 Connection으로 보내고 응답을 돌려준다.</summary>
        public Task<ModbusFrame> SendCommandAsync(string equipmentId, EquipmentCommand command) =>
            GetStandardOrThrow(equipmentId).SendCommandAsync(command);

        /// <summary>Modbus 요청을 표준 Connection으로 보내고 응답을 돌려준다.</summary>
        public Task<ModbusFrame> SendRequestAsync(string equipmentId, ModbusRequest request, int? timeoutMs = null) =>
            GetStandardOrThrow(equipmentId).SendRequestAsync(request, timeoutMs);

        /// <summary>레지스터 맵에 설정된 상태 영역을 표준 Connection으로 읽어온다.</summary>
        public Task<ushort[]> ReadStatusAsync(string equipmentId) => GetStandardOrThrow(equipmentId).ReadStatusAsync();

        public Task<ushort[]> ReadHoldingRegistersAsync(string equipmentId, ushort startAddress, ushort count) =>
            GetStandardOrThrow(equipmentId).ReadHoldingRegistersAsync(startAddress, count);

        public Task<ushort[]> ReadInputRegistersAsync(string equipmentId, ushort startAddress, ushort count) =>
            GetStandardOrThrow(equipmentId).ReadInputRegistersAsync(startAddress, count);

        public Task<ModbusFrame> WriteSingleRegisterAsync(string equipmentId, ushort address, ushort value) =>
            GetStandardOrThrow(equipmentId).WriteSingleRegisterAsync(address, value);

        public Task<ModbusFrame> WriteMultipleRegistersAsync(string equipmentId, ushort startAddress, ushort[] values) =>
            GetStandardOrThrow(equipmentId).WriteMultipleRegistersAsync(startAddress, values);

        /// <summary>표준 Connection의 연결 상태.</summary>
        public EquipmentConnectionState GetState(string equipmentId) => GetState(equipmentId, EquipmentChannel.Standard);

        public EquipmentConnectionState GetState(string equipmentId, EquipmentChannel channel)
        {
            if (!_equipments.TryGetValue(equipmentId, out var connections))
                return EquipmentConnectionState.Disconnected;

            var connection = channel == EquipmentChannel.Standard ? (ModbusTcpConnection)connections.Standard : connections.Event;
            return connection?.State ?? EquipmentConnectionState.Disconnected;
        }

        /// <summary>해당 설비에 그 Connection이 설정되어 있는지.</summary>
        public bool HasChannel(string equipmentId, EquipmentChannel channel)
        {
            if (!_equipments.TryGetValue(equipmentId, out var connections))
                return false;
            return channel == EquipmentChannel.Standard ? connections.Standard != null : connections.Event != null;
        }

        public EquipmentEndpoint GetEndpoint(string equipmentId) =>
            _equipments.TryGetValue(equipmentId, out var connections) ? connections.Endpoint : null;

        public IReadOnlyCollection<string> EquipmentIds => _equipments.Keys.ToList();

        private void Wire(ModbusTcpConnection connection)
        {
            connection.DataReceived += (s, e) => DataReceived?.Invoke(this, e);
            connection.ConnectionChanged += (s, e) => ConnectionChanged?.Invoke(this, e);
            connection.ErrorOccurred += (s, e) => ErrorOccurred?.Invoke(this, e);
        }

        private EquipmentConnections GetOrThrow(string equipmentId)
        {
            if (!_equipments.TryGetValue(equipmentId, out var connections))
                throw new InvalidOperationException($"등록되지 않은 설비ID입니다: {equipmentId}");
            return connections;
        }

        private EquipmentModbusClient GetStandardOrThrow(string equipmentId)
        {
            var standard = GetOrThrow(equipmentId).Standard;
            if (standard == null)
                throw new InvalidOperationException($"설비 [{equipmentId}]에 표준 Modbus-TCP connection이 설정되어 있지 않습니다. Read/Write는 표준 Connection에서만 가능합니다.");
            return standard;
        }

        private ModbusTcpConnection GetConnectionOrThrow(string equipmentId, EquipmentChannel channel)
        {
            var connections = GetOrThrow(equipmentId);
            var connection = channel == EquipmentChannel.Standard ? (ModbusTcpConnection)connections.Standard : connections.Event;
            if (connection == null)
                throw new InvalidOperationException($"설비 [{equipmentId}]에 {channel} Connection이 설정되어 있지 않습니다.");
            return connection;
        }

        public void Dispose()
        {
            foreach (var connections in _equipments.Values)
            {
                connections.Standard?.Dispose();
                connections.Event?.Dispose();
            }
            _equipments.Clear();
        }

        /// <summary>설비 1대가 보유하는 Connection 구성(표준 + 이벤트).</summary>
        private class EquipmentConnections
        {
            public EquipmentEndpoint Endpoint { get; }
            public EquipmentModbusClient Standard { get; set; }
            public EquipmentEventClient Event { get; set; }

            public EquipmentConnections(EquipmentEndpoint endpoint)
            {
                Endpoint = endpoint;
            }
        }
    }
}
