namespace SEOJIN_MEIS.Client.Equipment
{
    /// <summary>
    /// 설비 스펙에 정의된 두 종류의 Modbus-TCP Connection 구분.
    /// 한 설비에 두 Connection이 동시에 열릴 수 있고(포트가 다름), 상위로 올라오는 이벤트는 이 값으로 구분한다.
    /// </summary>
    public enum EquipmentChannel
    {
        /// <summary>Standard Modbus-TCP connection — 요청/응답 한 쌍. 실시간 이벤트를 제외한 모든 Read/Write가 여기로 나간다.</summary>
        Standard,

        /// <summary>Customized Modbus-TCP connection — 포트+1. Tool(Slave) → Controller(Master) 실시간 이벤트 전용, Read/Write 미지원.</summary>
        Event
    }
}
