using System;
using System.Text;

namespace SEOJIN_MEIS.Client.Equipment.Protocol
{
    /// <summary>
    /// 수신해서 파싱한 Modbus-TCP ADU 1개(MBAP 헤더 + PDU).
    /// 표준 Connection에서는 요청에 대한 "응답", Customized(이벤트) Connection에서는 장비가
    /// 능동적으로 밀어 올린 "실시간 이벤트" 프레임이 이 형태로 올라온다.
    /// </summary>
    public class ModbusFrame
    {
        /// <summary>MBAP Transaction Identifier. 표준 Connection은 이 값으로 요청/응답 짝을 맞춘다.</summary>
        public ushort TransactionId { get; set; }

        /// <summary>MBAP Unit Identifier (Slave 주소).</summary>
        public byte UnitId { get; set; }

        /// <summary>PDU 첫 바이트. 예외 응답이면 최상위 비트(0x80)가 제거된 원래 기능 코드가 들어간다.</summary>
        public byte FunctionCode { get; set; }

        /// <summary>기능 코드를 제외한 PDU 본문(데이터부).</summary>
        public byte[] Data { get; set; } = new byte[0];

        /// <summary>예외 응답(FC | 0x80) 여부.</summary>
        public bool IsException { get; set; }

        public ModbusExceptionCode ExceptionCode { get; set; } = ModbusExceptionCode.None;

        /// <summary>수신한 ADU 원본 바이트(로그/디버깅 용).</summary>
        public byte[] RawAdu { get; set; } = new byte[0];

        /// <summary>
        /// Read 계열 응답(FC 0x01~0x04)의 데이터부를 16bit 레지스터 배열로 해석한다.
        /// 데이터부 형식은 [ByteCount][Data...] 이며 Modbus는 big-endian이다.
        /// 형식이 맞지 않으면 빈 배열을 돌려준다.
        /// </summary>
        public ushort[] GetRegisters()
        {
            if (Data == null || Data.Length < 1)
                return new ushort[0];

            var byteCount = Data[0];
            if (byteCount + 1 > Data.Length)
                byteCount = (byte)(Data.Length - 1);

            var registerCount = byteCount / 2;
            var registers = new ushort[registerCount];
            for (var i = 0; i < registerCount; i++)
                registers[i] = (ushort)((Data[1 + i * 2] << 8) | Data[2 + i * 2]);
            return registers;
        }

        /// <summary>Read Coils/Discrete Inputs 응답의 데이터부를 bit 배열로 해석한다.</summary>
        public bool[] GetBits(int bitCount)
        {
            if (Data == null || Data.Length < 1 || bitCount <= 0)
                return new bool[0];

            var bits = new bool[bitCount];
            for (var i = 0; i < bitCount; i++)
            {
                var byteIndex = 1 + (i / 8);
                if (byteIndex >= Data.Length)
                    break;
                bits[i] = ((Data[byteIndex] >> (i % 8)) & 0x01) == 0x01;
            }
            return bits;
        }

        public string ToHexString() => ToHex(RawAdu);

        public static string ToHex(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return string.Empty;

            var sb = new StringBuilder(bytes.Length * 3);
            foreach (var b in bytes)
            {
                if (sb.Length > 0) sb.Append(' ');
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }

        public override string ToString()
        {
            if (IsException)
                return $"Tx={TransactionId} Unit={UnitId} FC=0x{FunctionCode:X2} EXCEPTION={ExceptionCode}(0x{(byte)ExceptionCode:X2})";

            var registers = GetRegisters();
            var body = registers.Length > 0
                ? string.Join(",", Array.ConvertAll(registers, r => r.ToString()))
                : ToHex(Data);
            return $"Tx={TransactionId} Unit={UnitId} FC=0x{FunctionCode:X2} [{body}]";
        }
    }
}
