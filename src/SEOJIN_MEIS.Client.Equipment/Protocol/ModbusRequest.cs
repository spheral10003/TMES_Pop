using System;

namespace SEOJIN_MEIS.Client.Equipment.Protocol
{
    /// <summary>
    /// 장비(Slave)로 보낼 Modbus 요청 1개. MBAP 헤더(Transaction Id/Length)는 전송 시점에
    /// <see cref="ModbusTcpCodec"/>가 붙이므로, 여기에는 PDU(기능 코드 + 데이터부)만 담는다.
    /// </summary>
    public class ModbusRequest
    {
        public byte FunctionCode { get; set; }

        /// <summary>기능 코드를 제외한 PDU 본문.</summary>
        public byte[] Data { get; set; } = new byte[0];

        /// <summary>지정하면 이 값으로 MBAP Unit Id를 덮어쓴다(미지정 시 Connection 설정값 사용).</summary>
        public byte? UnitId { get; set; }

        /// <summary>로그에 남길 사람이 읽는 설명.</summary>
        public string Description { get; set; }

        public static ModbusRequest ReadCoils(ushort startAddress, ushort count) =>
            Create(ModbusFunctionCode.ReadCoils, AddressAndCount(startAddress, count), $"ReadCoils({startAddress},{count})");

        public static ModbusRequest ReadDiscreteInputs(ushort startAddress, ushort count) =>
            Create(ModbusFunctionCode.ReadDiscreteInputs, AddressAndCount(startAddress, count), $"ReadDiscreteInputs({startAddress},{count})");

        public static ModbusRequest ReadHoldingRegisters(ushort startAddress, ushort count) =>
            Create(ModbusFunctionCode.ReadHoldingRegisters, AddressAndCount(startAddress, count), $"ReadHoldingRegisters({startAddress},{count})");

        public static ModbusRequest ReadInputRegisters(ushort startAddress, ushort count) =>
            Create(ModbusFunctionCode.ReadInputRegisters, AddressAndCount(startAddress, count), $"ReadInputRegisters({startAddress},{count})");

        public static ModbusRequest WriteSingleCoil(ushort address, bool value) =>
            Create(ModbusFunctionCode.WriteSingleCoil, AddressAndCount(address, value ? (ushort)0xFF00 : (ushort)0x0000), $"WriteSingleCoil({address},{value})");

        public static ModbusRequest WriteSingleRegister(ushort address, ushort value) =>
            Create(ModbusFunctionCode.WriteSingleRegister, AddressAndCount(address, value), $"WriteSingleRegister({address},{value})");

        public static ModbusRequest WriteMultipleRegisters(ushort startAddress, ushort[] values)
        {
            if (values == null || values.Length == 0)
                throw new ArgumentException("쓸 레지스터 값이 없습니다.", nameof(values));

            var byteCount = (byte)(values.Length * 2);
            var data = new byte[5 + byteCount];
            data[0] = (byte)(startAddress >> 8);
            data[1] = (byte)(startAddress & 0xFF);
            data[2] = (byte)(values.Length >> 8);
            data[3] = (byte)(values.Length & 0xFF);
            data[4] = byteCount;
            for (var i = 0; i < values.Length; i++)
            {
                data[5 + i * 2] = (byte)(values[i] >> 8);
                data[6 + i * 2] = (byte)(values[i] & 0xFF);
            }
            return Create(ModbusFunctionCode.WriteMultipleRegisters, data, $"WriteMultipleRegisters({startAddress},{values.Length})");
        }

        /// <summary>표준 기능 코드 표에 없는 벤더 확장 코드용. PDU 데이터부를 그대로 실어 보낸다.</summary>
        public static ModbusRequest Custom(byte functionCode, byte[] data, string description = null)
        {
            var request = new ModbusRequest();
            request.FunctionCode = functionCode;
            request.Data = data ?? new byte[0];
            request.Description = description ?? $"Custom(0x{functionCode:X2})";
            return request;
        }

        private static ModbusRequest Create(ModbusFunctionCode functionCode, byte[] data, string description)
        {
            var request = new ModbusRequest();
            request.FunctionCode = (byte)functionCode;
            request.Data = data;
            request.Description = description;
            return request;
        }

        private static byte[] AddressAndCount(ushort first, ushort second)
        {
            return new[]
            {
                (byte)(first >> 8),
                (byte)(first & 0xFF),
                (byte)(second >> 8),
                (byte)(second & 0xFF)
            };
        }
    }
}
