namespace SEOJIN_TMES.Client.Modbus.Protocol
{
    /// <summary>
    /// Modbus TCP(MBAP + PDU) 패킷의 인코딩/디코딩을 담당하는 인터페이스.
    /// 전송(Transport) 계층과 분리되어 있어 단독으로 테스트하거나 다른 코덱으로
    /// 교체할 수 있다.
    /// (Encodes/decodes Modbus TCP frames, independent of the transport layer.)
    /// </summary>
    public interface IModbusPacketCodec
    {
        byte[] EncodeRead(ModbusRequest request);

        byte[] EncodeWrite(ModbusRequest request);

        /// <summary>
        /// 수신한 원시 프레임을 해석한다. Modbus 응답 프레임에는 요청 주소가 포함되지
        /// 않으므로, 직전에 보낸 요청(<paramref name="lastRequest"/>)을 함께 전달해야 한다.
        /// </summary>
        ModbusResponse Decode(byte[] frame, ModbusRequest lastRequest);
    }
}
