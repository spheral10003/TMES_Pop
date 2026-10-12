namespace SEOJIN_TMES.Client.Equipment.Nutrunner
{
    /// <summary>
    /// Event Transport 채널로 수신한 체결(Fastening) 결과 데이터.
    /// </summary>
    public struct FasteningData
    {
        /// <summary>어느 장비의 데이터인지 구분하는 값. Modbus TransactionId = Nutrunner ID.</summary>
        public int Id { get; set; }

        /// <summary>장비가 보고한 Tool ID.</summary>
        public ushort ToolId { get; set; }

        public ushort FastenTime { get; set; }
        public ushort Preset { get; set; }
        public ushort TargetTorque { get; set; }
        public ushort Torque { get; set; }
        public ushort Speed { get; set; }
        public ushort Angle1 { get; set; }
        public ushort Angle2 { get; set; }
        public ushort Angle3 { get; set; }
        public ushort RemainScrew { get; set; }
        public ushort Error { get; set; }

        /// <summary>회전 방향.</summary>
        public ushort Direction { get; set; }

        /// <summary>이벤트 상태.</summary>
        public ushort EventStatus { get; set; }

        public ushort SnugAngle { get; set; }

        private string? _barcode;

        public string Barcode
        {
            get => _barcode ?? string.Empty;
            set => _barcode = value;
        }

        /// <summary>실제 토크 값 (원시값 / 100).</summary>
        public double TorqueValue => Torque / 100.0;

        /// <summary>실제 목표 토크 값 (원시값 / 100).</summary>
        public double TargetTorqueValue => TargetTorque / 100.0;

        public bool IsOk => Error == 0;
    }
}
