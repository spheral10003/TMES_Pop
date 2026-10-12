using System.ComponentModel;

namespace SEOJIN_TMES.Client.Modbus.Protocol
{
    /// <summary>
    /// Modbus 함수 코드 (Modbus function code).
    /// </summary>
    public enum ModbusFunctionCode : byte
    {
        [Description("0xxxx영역, Read, Bit")]
        ReadCoils = 0x01,
        [Description("1xxxx영역, Read Only, Bit")]
        ReadDiscreteInputs = 0x02,
        [Description("4xxxx영역, Read, Word")]
        ReadHoldingRegisters = 0x03,
        [Description("3xxxx영역, Read Only, Word")]
        ReadInputRegisters = 0x04,
        [Description("0xxxx영역, Write, Bit")]
        WriteSingleCoil = 0x05,
        [Description("4xxxx영역, Write, Word")]
        WriteSingleRegister = 0x06,
        [Description("0xxxx영역, 다중 Write, Bit")]
        WriteMultipleCoils = 0x0F,
        [Description("4xxxx영역, 다중 Write, Word")]
        WriteMultipleRegisters = 0x10,
        [Description("3xxxx영역 ( 커스텀 ), Read Only, Word")]
        ReadInfomationRegister = 0x11
    }
}
