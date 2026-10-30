using System;
using System.Collections.Generic;

namespace SEOJIN_TMES.Client.Modbus.Protocol
{
    /// <summary>
    /// Modbus 응답(Read/Write ack) 디코딩 결과.
    /// (Decoded result of a Modbus response frame.)
    /// </summary>
    public sealed class ModbusResponse
    {
        public ushort TransactionId { get; set; }
        public ushort ProtocolId { get; set; }
        public ushort FrameLength { get; set; }
        public byte UnitId { get; set; }
        public ModbusFunctionCode FunctionCode { get; set; }

        /// <summary>true면 오류 응답이거나 비정상 프레임.</summary>
        public bool IsError { get; set; }

        /// <summary>
        /// 쓰기 응답은 프레임에 실려 온 주소, 읽기 · 오류 응답은 프레임에 주소가 없으므로 그 응답의 요청 주소.
        /// </summary>
        public ushort StartAddress { get; set; }

        public IReadOnlyList<ushort> Data { get; set; } = Array.Empty<ushort>();
    }
}
