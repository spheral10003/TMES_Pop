namespace SEOJIN_TMES.Client.Equipment.Nutrunner
{
    /// <summary>
    /// Nutrunner 한 대의 "정보". (통신으로 흘러다니는 데이터와 분리해서 관리)
    ///
    /// <see cref="Id"/> 는 장비를 구분하는 유일 키이며, 그대로 Modbus TransactionId 로 사용된다.
    /// </summary>
    public sealed class NutrunnerInfo
    {
        /// <summary>장비 구분 ID. Modbus TransactionId 와 동일한 값으로 사용한다.</summary>
        public int Id { get; set; }

        public string Ip { get; set; } = string.Empty;

        /// <summary>Request(요청/응답) 채널 포트.</summary>
        public int Port { get; set; } = 5000;

        /// <summary>Event Transport(체결 결과 Push) 채널 포트. 기본은 Request 포트 + 1.</summary>
        public int EventPort { get; set; }

        public string Name { get; set; } = string.Empty;

        public string SerialNo { get; set; } = string.Empty;

        public string FirmwareVer { get; set; } = string.Empty;

        public string ModelName { get; set; } = string.Empty;

        public ModelTypes Model { get; set; } = ModelTypes.Bm;

        public GenerationTypes Generation { get; set; } = GenerationTypes.GenRev1;

        public UnitTypes Unit { get; set; } = UnitTypes.KgfCm;

        /// <summary>Modbus TransactionId. 장비 ID와 동일하다.</summary>
        public ushort TransactionId => (ushort)Id;

        /// <summary>실제 사용할 Event Transport 포트 (미지정 시 Port + 1).</summary>
        public int ResolvedEventPort => EventPort > 0 ? EventPort : Port + 1;

        public string DisplayName => string.IsNullOrWhiteSpace(Name) ? $"EQ-{Id:D3}" : Name;

        public override string ToString() => $"[{Id}] {DisplayName} ({Ip}:{Port})";
    }
}
