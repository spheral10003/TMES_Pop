using System;

namespace SEOJIN_MEIS.Client.Equipment.Protocol
{
    /// <summary>장비가 Modbus 예외 응답(FC | 0x80)을 돌려준 경우.</summary>
    public class ModbusProtocolException : Exception
    {
        public byte FunctionCode { get; }
        public ModbusExceptionCode ExceptionCode { get; }

        public ModbusProtocolException(byte functionCode, ModbusExceptionCode exceptionCode)
            : base($"Modbus 예외 응답 - FC=0x{functionCode:X2}, 예외코드={exceptionCode}(0x{(byte)exceptionCode:X2})")
        {
            FunctionCode = functionCode;
            ExceptionCode = exceptionCode;
        }
    }

    /// <summary>요청을 보냈지만 응답 대기 시간(ResponseTimeoutMs) 안에 응답이 오지 않은 경우.</summary>
    public class ModbusTimeoutException : Exception
    {
        public ModbusTimeoutException(string message) : base(message) { }
    }
}
