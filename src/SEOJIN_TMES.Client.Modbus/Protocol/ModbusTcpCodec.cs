using System;
using System.Collections.Generic;

namespace SEOJIN_TMES.Client.Modbus.Protocol
{
    /// <summary>
    /// 표준 Modbus TCP(MBAP Header + PDU) 인코더/디코더.
    /// 기존 PacketProcessor의 패킷 조립/해석 로직을 그대로 옮기되,
    /// 가변 필드(_header/_pduPacket)에 상태를 들고 있던 방식을 없애고
    /// 요청/응답을 매개변수로 주고받는 순수 함수 형태로 정리했다.
    /// </summary>
    public sealed class ModbusTcpCodec : IModbusPacketCodec
    {
        private const ushort ProtocolId = 0x0000;
        private const ushort HeaderFrameLength = 0x0006;

        

        



        private enum ResponseIndex : int
        {
            TransactionId = 0,
            ProtocolId = 2,
            FrameLength = 4,
            UnitId = 6,
            FuncCode = 7,
            Error = 7,
            ReadAckDataLength = 8,
            WriteAckAddress = 8,
            ReadAckData = 9,
            WriteAckData = 10,
        }

        /// <summary>수신 프레임이 최소한 이 길이는 되어야 정상적으로 해석할 수 있다.</summary>
        private const int MinFrameLength = 8;

        public byte[] EncodeRead(ModbusRequest request)
            => Encode(request.TransactionId, request.UnitId, request.FunctionCode, request.Address, request.Value);

        public byte[] EncodeWrite(ModbusRequest request)
            => Encode(request.TransactionId, request.UnitId, request.FunctionCode, request.Address, request.Value);

        private byte[] Encode(ushort transactionId, byte unitId, ModbusFunctionCode function, ushort address, ushort valueOrCount)
        {
            var packet = new List<byte>(12);
            packet.AddRange(ToBigEndian(transactionId));
            packet.AddRange(ToBigEndian(ProtocolId));
            packet.AddRange(ToBigEndian(HeaderFrameLength));
            packet.Add(unitId);
            packet.Add((byte)function);
            packet.AddRange(ToBigEndian(address));
            packet.AddRange(ToBigEndian(valueOrCount));

            return packet.ToArray();
        }

        public ModbusResponse Decode(byte[] frame, ModbusRequest lastRequest)
        {
            if (frame == null || frame.Length < MinFrameLength)
            {
                return new ModbusResponse
                {
                    TransactionId = lastRequest.TransactionId,
                    UnitId = lastRequest.UnitId,
                    FunctionCode = lastRequest.FunctionCode,
                    StartAddress = lastRequest.Address,
                    IsError = true,
                    Data = Array.Empty<ushort>()
                };
            }

            ushort transactionId = FromBigEndian(frame, (int)ResponseIndex.TransactionId);
            ushort porotocolId = FromBigEndian(frame, (int)ResponseIndex.ProtocolId);
            byte unitId = frame[(int)ResponseIndex.UnitId];
            var functionCode = (ModbusFunctionCode)frame[(int)ResponseIndex.FuncCode];

            bool isError = false;
            int dataLength;
            int dataStartIndex;

            switch (functionCode)
            {
                case ModbusFunctionCode.ReadHoldingRegisters:
                case ModbusFunctionCode.ReadInputRegisters:
                    dataLength = frame[(int)ResponseIndex.ReadAckDataLength];
                    dataStartIndex = (int)ResponseIndex.ReadAckData;
                    break;

                case ModbusFunctionCode.WriteSingleRegister:
                    dataLength = 2;
                    dataStartIndex = (int)ResponseIndex.WriteAckData;
                    break;

                case ModbusFunctionCode.ReadInfomationRegister:
                default:
                    isError = true;
                    dataLength = 2;
                    dataStartIndex = (int)ResponseIndex.ReadAckData;
                    break;
            }

            var data = new List<ushort>(dataLength / 2);
            int index = dataStartIndex;

            for (int i = 0; i < dataLength && index + 1 < frame.Length; i += 2)
            {
                //data.Add((ushort)((frame[index++] << 8) + frame[index++]));
                data.Add(FromBigEndian(frame, index));
            }

            return new ModbusResponse
            {
                TransactionId = transactionId,
                UnitId = unitId,
                FunctionCode = functionCode,
                IsError = isError,
                StartAddress = lastRequest.Address,
                Data = data
            };
        }

        private byte[] ToBigEndian(ushort value) => new[] { (byte)(value >> 8), (byte)value };

        private ushort FromBigEndian(byte[] data, int index) => (ushort)((data[index] << 8) | data[index + 1]);
    }
}
