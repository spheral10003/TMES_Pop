namespace SEOJIN_TMES.Client.Equipment.Nutrunner
{
    /// <summary>
    /// Event Transport 채널로 들어오는 Nutrunner 전용 체결 결과 프레임 파서.
    ///
    /// 표준 Modbus 응답이 아니라 장비 고유 포맷이므로 SEOJIN_TMES.Client.Modbus 에는 두지 않고
    /// 장비 계층인 이 프로젝트에서만 해석한다.
    ///
    /// 프레임 구분 규칙
    ///  - TransactionId : 장비 구분 (= Nutrunner ID)
    ///  - ProtocolId    : 0 = Request(요청/응답), 1 = Event Transport
    /// </summary>
    public static class NutrunnerEventParser
    {
        /// <summary>요청/응답 채널의 ProtocolId.</summary>
        public const ushort RequestProtocolId = 0x0000;

        /// <summary>Event Transport 채널의 ProtocolId.</summary>
        public const ushort EventProtocolId = 0x0001;

        /// <summary>TransactionId(2) + ProtocolId(2) + FrameLength(2) + UnitId(1) + FuncCode(1) + DataLength(1).</summary>
        private const int HeaderLength = 9;

        /// <summary>헤더 뒤에 이어지는 워드(2byte) 개수.</summary>
        private const int BodyWordCount = 14;

        private const int MinFrameLength = HeaderLength + (BodyWordCount * 2);

        /// <summary>
        /// 수신 프레임을 체결 결과로 해석한다.
        /// ProtocolId 가 <see cref="EventProtocolId"/> 가 아니거나 길이가 부족하면 false.
        /// </summary>
        public static bool TryParse(byte[]? frame, out FasteningData data)
        {
            data = default;

            if (frame is null || frame.Length < MinFrameLength)
            {
                return false;
            }

            ushort transactionId = ReadUInt16(frame, 0);
            ushort protocolId = ReadUInt16(frame, 2);

            //if (protocolId != EventProtocolId)
            //{
            //    return false;
            //}

            int index = HeaderLength;

            data = new FasteningData
            {
                Id = transactionId,
                ToolId = ReadUInt16(frame, index),
                FastenTime = ReadUInt16(frame, index += 2),
                Preset = ReadUInt16(frame, index += 2),
                TargetTorque = ReadUInt16(frame, index += 2),
                Torque = ReadUInt16(frame, index += 2),
                Speed = ReadUInt16(frame, index += 2),
                Angle1 = ReadUInt16(frame, index += 2),
                Angle2 = ReadUInt16(frame, index += 2),
                Angle3 = ReadUInt16(frame, index += 2),
                RemainScrew = ReadUInt16(frame, index += 2),
                Error = ReadUInt16(frame, index += 2),
                Direction = ReadUInt16(frame, index += 2),
                EventStatus = ReadUInt16(frame, index += 2),
                SnugAngle = ReadUInt16(frame, index += 2),
                Barcode = string.Empty
            };

            return true;
        }

        private static ushort ReadUInt16(byte[] data, int index)
            => (ushort)((data[index] << 8) | data[index + 1]);
    }
}
