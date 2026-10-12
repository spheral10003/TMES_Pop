namespace SEOJIN_TMES.Client.Modbus.Protocol
{


    /// <summary>
    /// 한 번의 Modbus 요청(Read/Write)을 표현하는 불변 값.
    /// (An immutable value describing one Modbus request.)
    /// </summary>
    public struct ModbusRequest
    {
        public ushort TransactionId { get; set; }
        public ushort ProtocolId { get; set; }
        public ushort FrameLength { get; set; }
        public byte UnitId { get; set; }
        public ModbusFunctionCode FunctionCode { get; set; }

        /// <summary>Read/Write 공통 시작 주소.</summary>
        public ushort Address { get; set; }

        /// <summary>
        /// Read 요청일 때는 읽을 개수(register count),
        /// Write 요청일 때는 쓸 값(value) 으로 사용된다.
        /// </summary>
        public ushort Value { get; set; }

        public ModbusRequest(ushort transactionId, ushort address, ushort count, 
            ModbusFunctionCode function = ModbusFunctionCode.ReadInputRegisters, byte unitId = 0x00)
        {
            TransactionId = transactionId;
            ProtocolId = 0x0000;
            UnitId = unitId;
            FunctionCode = function;
            Address = address;
            Value = count;
        }
    }

}
