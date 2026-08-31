using System;

namespace SEOJIN_MEIS.Client.Equipment.Protocol
{
    /// <summary>
    /// 표준 Modbus-TCP 코덱.
    /// ADU 구조: [Transaction Id(2)][Protocol Id(2)=0x0000][Length(2)][Unit Id(1)][Function Code(1)][Data(n)]
    /// Length 필드는 "그 뒤에 오는 바이트 수"(Unit Id + PDU)이며, 모든 다중 바이트 값은 big-endian이다.
    /// 표준 Connection(요청/응답)과 Customized Connection(이벤트 수신) 양쪽에서 같은 코덱을 쓴다.
    /// </summary>
    public class ModbusTcpCodec : IModbusCodec
    {
        /// <summary>MBAP 헤더 길이(Transaction Id 2 + Protocol Id 2 + Length 2 + Unit Id 1).</summary>
        public const int MbapHeaderLength = 7;

        /// <summary>Length 필드를 읽기 위해 최소로 필요한 바이트 수.</summary>
        private const int LengthFieldEnd = 6;

        /// <summary>Length 필드의 유효 범위: Unit Id(1) + PDU(최소 1, 최대 253).</summary>
        private const int MinLengthField = 2;
        private const int MaxLengthField = 254;

        public byte[] EncodeRequest(ModbusRequest request, ushort transactionId, byte defaultUnitId)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var data = request.Data ?? new byte[0];
            var lengthField = 1 + 1 + data.Length; // Unit Id + Function Code + Data
            var adu = new byte[LengthFieldEnd + lengthField];

            adu[0] = (byte)(transactionId >> 8);
            adu[1] = (byte)(transactionId & 0xFF);
            adu[2] = 0x00; // Protocol Id = 0 (Modbus)
            adu[3] = 0x00;
            adu[4] = (byte)(lengthField >> 8);
            adu[5] = (byte)(lengthField & 0xFF);
            adu[6] = request.UnitId ?? defaultUnitId;
            adu[7] = request.FunctionCode;
            Array.Copy(data, 0, adu, MbapHeaderLength + 1, data.Length);
            return adu;
        }

        public byte[] EncodeEcho(ModbusFrame frame)
        {
            if (frame == null) throw new ArgumentNullException(nameof(frame));

            var echo = new ModbusRequest();
            echo.FunctionCode = frame.FunctionCode;
            echo.Data = frame.Data ?? new byte[0];
            echo.UnitId = frame.UnitId;
            return EncodeRequest(echo, frame.TransactionId, frame.UnitId);
        }

        public bool TryExtractFrame(byte[] buffer, int length, out ModbusFrame frame, out int consumedLength)
        {
            frame = null;
            consumedLength = 0;

            if (length < LengthFieldEnd)
                return false; // Length 필드를 아직 못 읽음

            var protocolId = (buffer[2] << 8) | buffer[3];
            var lengthField = (buffer[4] << 8) | buffer[5];

            if (protocolId != 0 || lengthField < MinLengthField || lengthField > MaxLengthField)
            {
                // MBAP로 해석할 수 없는 바이트 → 1바이트 버리고 다시 동기를 맞춘다.
                consumedLength = 1;
                return false;
            }

            var totalLength = LengthFieldEnd + lengthField;
            if (length < totalLength)
                return false; // 프레임이 아직 다 도착하지 않음

            var functionCode = buffer[7];
            var isException = (functionCode & 0x80) != 0;
            var dataLength = lengthField - 2; // Unit Id, Function Code 제외

            frame = new ModbusFrame();
            frame.TransactionId = (ushort)((buffer[0] << 8) | buffer[1]);
            frame.UnitId = buffer[6];
            frame.FunctionCode = (byte)(functionCode & 0x7F);
            frame.IsException = isException;

            var data = new byte[dataLength];
            Array.Copy(buffer, MbapHeaderLength + 1, data, 0, dataLength);
            frame.Data = data;

            if (isException && dataLength >= 1)
                frame.ExceptionCode = (ModbusExceptionCode)data[0];

            var raw = new byte[totalLength];
            Array.Copy(buffer, 0, raw, 0, totalLength);
            frame.RawAdu = raw;

            consumedLength = totalLength;
            return true;
        }
    }
}
