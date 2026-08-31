using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using SEOJIN_MEIS.Client.Common.Configuration;
using SEOJIN_MEIS.Client.Common.Logging;
using SEOJIN_MEIS.Client.Equipment.Protocol;

namespace SEOJIN_MEIS.Client.Equipment
{
    /// <summary>
    /// Standard Modbus-TCP connection — 설비 스펙의 첫 번째 통신 방식.
    /// 요청/응답이 한 쌍으로 구성된 일반적인 Modbus-TCP 통신이며, 실시간 이벤트 수신을 제외한
    /// 모든 Read/Write 동작은 이 Connection을 통해서 이루어져야 한다.
    ///
    /// MBAP Transaction Id로 요청과 응답을 짝지으며, 장비가 동시 요청을 못 받는 경우가 흔해
    /// 요청 자체는 Connection 단위로 직렬화(SemaphoreSlim)한다.
    /// </summary>
    public class EquipmentModbusClient : ModbusTcpConnection
    {
        private readonly ConcurrentDictionary<ushort, TaskCompletionSource<ModbusFrame>> _pending =
            new ConcurrentDictionary<ushort, TaskCompletionSource<ModbusFrame>>();

        private readonly SemaphoreSlim _requestGate = new SemaphoreSlim(1, 1);
        private readonly ModbusRegisterMap _registerMap;

        private int _transactionCounter;

        public override EquipmentChannel Channel => EquipmentChannel.Standard;

        /// <summary>요청 후 응답을 기다리는 시간(ms).</summary>
        public int ResponseTimeoutMs { get; }

        public EquipmentModbusClient(
            string equipmentId,
            string host,
            int port,
            byte unitId = 1,
            int responseTimeoutMs = 3000,
            ModbusRegisterMap registerMap = null,
            IModbusCodec codec = null)
            : base(equipmentId, host, port, unitId, codec)
        {
            ResponseTimeoutMs = responseTimeoutMs > 0 ? responseTimeoutMs : 3000;
            _registerMap = registerMap ?? new ModbusRegisterMap();
        }

        /// <summary>논리 명령(Start/Stop/Reset/상태조회)을 레지스터 맵에 따라 Modbus 요청으로 바꿔 보낸다.</summary>
        public Task<ModbusFrame> SendCommandAsync(EquipmentCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            return SendRequestAsync(command.ToModbusRequest(_registerMap));
        }

        /// <summary>레지스터 맵에 설정된 상태 영역을 읽어 레지스터 배열로 돌려준다.</summary>
        public async Task<ushort[]> ReadStatusAsync()
        {
            var response = await SendCommandAsync(EquipmentCommand.StatusRequest()).ConfigureAwait(false);
            return response.GetRegisters();
        }

        public async Task<ushort[]> ReadHoldingRegistersAsync(ushort startAddress, ushort count)
        {
            var response = await SendRequestAsync(ModbusRequest.ReadHoldingRegisters(startAddress, count)).ConfigureAwait(false);
            return response.GetRegisters();
        }

        public async Task<ushort[]> ReadInputRegistersAsync(ushort startAddress, ushort count)
        {
            var response = await SendRequestAsync(ModbusRequest.ReadInputRegisters(startAddress, count)).ConfigureAwait(false);
            return response.GetRegisters();
        }

        public async Task<bool[]> ReadCoilsAsync(ushort startAddress, ushort count)
        {
            var response = await SendRequestAsync(ModbusRequest.ReadCoils(startAddress, count)).ConfigureAwait(false);
            return response.GetBits(count);
        }

        public Task<ModbusFrame> WriteSingleRegisterAsync(ushort address, ushort value) =>
            SendRequestAsync(ModbusRequest.WriteSingleRegister(address, value));

        public Task<ModbusFrame> WriteMultipleRegistersAsync(ushort startAddress, ushort[] values) =>
            SendRequestAsync(ModbusRequest.WriteMultipleRegisters(startAddress, values));

        public Task<ModbusFrame> WriteSingleCoilAsync(ushort address, bool value) =>
            SendRequestAsync(ModbusRequest.WriteSingleCoil(address, value));

        /// <summary>
        /// Modbus 요청 1개를 보내고 짝이 맞는 응답을 기다린다.
        /// 응답이 예외 응답(FC | 0x80)이면 <see cref="ModbusProtocolException"/>,
        /// 제한 시간 안에 응답이 없으면 <see cref="ModbusTimeoutException"/>을 던진다.
        /// </summary>
        public async Task<ModbusFrame> SendRequestAsync(ModbusRequest request, int? timeoutMs = null)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var timeout = timeoutMs.HasValue && timeoutMs.Value > 0 ? timeoutMs.Value : ResponseTimeoutMs;

            await _requestGate.WaitAsync().ConfigureAwait(false);
            var transactionId = NextTransactionId();
            var completion = new TaskCompletionSource<ModbusFrame>(TaskCreationOptions.RunContinuationsAsynchronously);
            _pending[transactionId] = completion;

            try
            {
                var adu = Codec.EncodeRequest(request, transactionId, UnitId);
                AppLog.Info($"[{EquipmentId}/{Channel}] 요청 Tx={transactionId} {request.Description} :: {ModbusFrame.ToHex(adu)}");
                SendRaw(adu);

                var finished = await Task.WhenAny(completion.Task, Task.Delay(timeout)).ConfigureAwait(false);
                if (finished != completion.Task)
                    throw new ModbusTimeoutException($"[{EquipmentId}/{Channel}] 응답 시간 초과({timeout}ms) - Tx={transactionId} {request.Description}");

                var frame = await completion.Task.ConfigureAwait(false);
                if (frame.IsException)
                    throw new ModbusProtocolException(frame.FunctionCode, frame.ExceptionCode);

                return frame;
            }
            finally
            {
                _pending.TryRemove(transactionId, out _);
                _requestGate.Release();
            }
        }

        /// <summary>
        /// 수신 프레임을 대기 중인 요청과 짝지어 완료시킨다.
        /// 짝이 없는 프레임(장비가 표준 Connection으로 먼저 밀어 올린 경우)은 DataReceived로 상위에 올린다.
        /// </summary>
        protected override void HandleFrame(ModbusFrame frame)
        {
            if (_pending.TryRemove(frame.TransactionId, out var completion))
            {
                completion.TrySetResult(frame);
                return;
            }

            AppLog.Info($"[{EquipmentId}/{Channel}] 짝 없는 수신 프레임 - {frame}");
            RaiseDataReceived(frame);
        }

        /// <summary>연결이 끊기면 응답을 기다리던 요청들을 즉시 실패시킨다(타임아웃까지 매달리지 않도록).</summary>
        protected override void OnDisconnected(Exception reason)
        {
            foreach (var key in _pending.Keys)
            {
                if (_pending.TryRemove(key, out var completion))
                    completion.TrySetException(reason ?? new InvalidOperationException("연결이 종료되었습니다."));
            }
        }

        /// <summary>MBAP Transaction Id는 1~65535를 순환 사용한다(0은 피한다).</summary>
        private ushort NextTransactionId()
        {
            var next = Interlocked.Increment(ref _transactionCounter);
            var wrapped = (ushort)(next & 0xFFFF);
            return wrapped == 0 ? (ushort)1 : wrapped;
        }
    }
}
