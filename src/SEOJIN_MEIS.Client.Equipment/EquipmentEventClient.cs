using SEOJIN_MEIS.Client.Common.Configuration;
using SEOJIN_MEIS.Client.Common.Logging;
using SEOJIN_MEIS.Client.Equipment.Protocol;

namespace SEOJIN_MEIS.Client.Equipment
{
    /// <summary>
    /// Customized Modbus-TCP connection — 설비 스펙의 두 번째 통신 방식.
    ///  - Tool(Slave) → Controller(Master) 방향으로 실시간 이벤트를 전송받는 전용 Connection이다.
    ///  - 접속 포트는 Advertise / Scan ack / Search ack 메시지의 TCP 포트 번호 + 1 이다.
    ///  - 이벤트 전용이므로 Modbus Read/Write 동작은 지원하지 않는다 — 그래서 이 클래스에는 요청 전송 API가 없다.
    ///
    /// 프레임 구조 자체는 표준 MBAP과 같다고 보고 같은 코덱으로 파싱하며, 수신 즉시 DataReceived로 상위에 올린다.
    /// TODO(이벤트 payload 스펙 확정 시): 기능 코드/데이터부를 이벤트 종류별 의미로 해석하는 매핑 추가.
    /// </summary>
    public class EquipmentEventClient : ModbusTcpConnection
    {
        private readonly ModbusEventAckMode _ackMode;

        public override EquipmentChannel Channel => EquipmentChannel.Event;

        public EquipmentEventClient(
            string equipmentId,
            string host,
            int eventPort,
            byte unitId = 1,
            ModbusEventAckMode ackMode = ModbusEventAckMode.None,
            IModbusCodec codec = null)
            : base(equipmentId, host, eventPort, unitId, codec)
        {
            _ackMode = ackMode;
        }

        protected override void HandleFrame(ModbusFrame frame)
        {
            AppLog.Info($"[{EquipmentId}/{Channel}] 실시간 이벤트 수신 - {frame}");

            // 장비가 이벤트에 대한 확인(ACK)을 기대하는 구성이면 같은 Transaction Id로 그대로 되돌려 보낸다.
            if (_ackMode == ModbusEventAckMode.Echo)
                SendRaw(Codec.EncodeEcho(frame));

            RaiseDataReceived(frame);
        }
    }
}
