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
        public string Code { get; set; } = string.Empty;
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

        /// <summary>TCP 접속 제한 시간(ms). Request · Event 두 채널 모두 적용. App.config <c>ModbusConnectTimeoutMs</c>에서 온다.</summary>
        public int ConnectTimeoutMs { get; set; } = 500;

        /// <summary>
        /// Request 채널 요청 하나의 응답 대기 시간(ms) — Alive 응답 대기에도 쓴다. App.config <c>ModbusResponseTimeoutMs</c>에서 온다.
        /// 예전에는 <see cref="ConnectTimeoutMs"/>를 같이 써서 500ms만 늦어도 응답 없음으로 연결을 끊었다.
        /// </summary>
        public int ResponseTimeoutMs { get; set; } = 2000;

        /// <summary>
        /// 재연결 <c>3회</c>가 모두 실패한 뒤 다음 3회를 시작하기까지 쉬는 시간(ms). App.config <c>ModbusReconnectWaitMs</c>에서 온다.
        /// 0 이하면 30000.
        /// </summary>
        public int ReconnectWaitMs { get; set; } = 30000;

        /// <summary>무통신 후 첫 TCP KeepAlive 확인까지의 시간(ms). App.config <c>ModbusKeepAliveTimeMs</c>에서 온다.</summary>
        public int KeepAliveTimeMs { get; set; } = 5000;

        /// <summary>KeepAlive 재확인 간격(ms). App.config <c>ModbusKeepAliveIntervalMs</c>에서 온다.</summary>
        public int KeepAliveIntervalMs { get; set; } = 5000;

        /// <summary>Modbus TransactionId. 장비 ID와 동일하다.</summary>
        public ushort TransactionId => (ushort)Id;

        /// <summary>실제 사용할 Event Transport 포트 (미지정 시 Port + 1).</summary>
        public int ResolvedEventPort => EventPort > 0 ? EventPort : Port + 1;

        public string DisplayName => string.IsNullOrWhiteSpace(Name) ? $"EQ-{Id:D3}" : Name;

        public override string ToString() => $"[{Id}] {DisplayName} ({Ip}:{Port})";
    }
}
