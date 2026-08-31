namespace SEOJIN_MEIS.Client.Equipment.Protocol
{
    /// <summary>
    /// Modbus-TCP 바이트 스트림 ↔ 논리 프레임 변환 계약.
    /// 표준 Modbus-TCP는 MBAP 헤더의 Length 필드로 프레임 경계가 확정되므로 구분자/체크섬이 필요 없다.
    /// 장비가 MBAP를 변형해서 쓰는 경우(벤더 커스텀)에는 이 인터페이스 구현체만 교체하면 되고,
    /// 이를 사용하는 상위 코드(Connection/Manager)는 변경할 필요가 없다.
    /// </summary>
    public interface IModbusCodec
    {
        /// <summary>요청 PDU에 MBAP 헤더를 붙여 전송할 ADU 바이트를 만든다.</summary>
        byte[] EncodeRequest(ModbusRequest request, ushort transactionId, byte defaultUnitId);

        /// <summary>수신한 프레임을 그대로 되돌려 보내는 ADU(이벤트 수신 확인용 echo)를 만든다.</summary>
        byte[] EncodeEcho(ModbusFrame frame);

        /// <summary>
        /// 누적 수신 버퍼 앞쪽에서 완전한 ADU 1개를 추출한다.
        /// - 프레임 1개 완성: frame != null, consumedLength = 소비한 바이트 수, 반환 true
        /// - 아직 더 받아야 함: frame = null, consumedLength = 0, 반환 false
        /// - 깨진 바이트(동기 이탈): frame = null, consumedLength > 0(버릴 바이트 수), 반환 false
        /// </summary>
        bool TryExtractFrame(byte[] buffer, int length, out ModbusFrame frame, out int consumedLength);
    }
}
