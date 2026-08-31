namespace SEOJIN_MEIS.Client.Equipment.Protocol
{
    /// <summary>
    /// Modbus 표준 기능 코드(Function Code). 장비 스펙이 사용하는 코드만 골라 쓰면 되고,
    /// 여기 없는 벤더 확장 코드는 <see cref="ModbusRequest.Custom"/>로 raw PDU를 그대로 보낼 수 있다.
    /// </summary>
    public enum ModbusFunctionCode : byte
    {
        ReadCoils = 0x01,
        ReadDiscreteInputs = 0x02,
        ReadHoldingRegisters = 0x03,
        ReadInputRegisters = 0x04,
        WriteSingleCoil = 0x05,
        WriteSingleRegister = 0x06,
        WriteMultipleCoils = 0x0F,
        WriteMultipleRegisters = 0x10
    }

    /// <summary>Modbus 예외 응답(FC | 0x80)의 예외 코드.</summary>
    public enum ModbusExceptionCode : byte
    {
        None = 0x00,
        IllegalFunction = 0x01,
        IllegalDataAddress = 0x02,
        IllegalDataValue = 0x03,
        SlaveDeviceFailure = 0x04,
        Acknowledge = 0x05,
        SlaveDeviceBusy = 0x06,
        MemoryParityError = 0x08,
        GatewayPathUnavailable = 0x0A,
        GatewayTargetNoResponse = 0x0B
    }
}
