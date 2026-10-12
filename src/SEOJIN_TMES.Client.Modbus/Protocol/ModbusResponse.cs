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
        /// 응답 프레임 자체에는 주소가 실려오지 않으므로, 요청 시 사용한 시작 주소를 그대로 돌려준다.
        /// </summary>
        public ushort StartAddress { get; set; }

        public IReadOnlyList<ushort> Data { get; set; } = Array.Empty<ushort>();
    }
}
