namespace SEOJIN_TMES.Client.Common.Configuration
{
    /// <summary>App.config에서 로드되는 클라이언트 실행 설정. 값 자체는 SEOJIN_TMES.Client/App.config에서 관리.</summary>
    public class AppSettings
    {
        public string ApiBaseUrl { get; set; }
        public int ApiTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// 파일 업로드 제한 시간 = <see cref="ApiUploadTimeoutSeconds"/> + 본문 크기 ÷ <see cref="ApiUploadMinKBytesPerSecond"/>.
        /// 업로드는 <see cref="ApiTimeoutSeconds"/>를 쓰지 않는다 — 큰 파일은 보내는 데만 30초를 넘긴다.
        /// </summary>
        public int ApiUploadTimeoutSeconds { get; set; } = 60;

        /// <summary>업로드 제한 시간을 잡을 때 가정하는 최저 전송 속도(KB/초).</summary>
        public int ApiUploadMinKBytesPerSecond { get; set; } = 512;

        /// <summary>
        /// API 응답 본문을 로그에 남길지 여부. 서버 연동을 맞추는 동안 응답 모양을 확인하려고 켠다.
        /// 비밀번호·토큰 같은 필드는 기록 전에 가려지지만, <b>업무 데이터는 그대로 남으므로</b>
        /// 현장 운영에서는 끄는 것을 권한다(로그 용량도 크게 늘어난다).
        /// </summary>
        public bool LogApiResponseBody { get; set; } = true;

        /// <summary>로그에 남길 응답 본문의 최대 길이(자). 넘으면 잘라서 남긴다.</summary>
        public int LogApiResponseBodyMaxLength { get; set; } = 20000;
        public string DefaultCulture { get; set; } = "ko-KR";
        public string ServerName { get; set; } = "GUM";

        /// <summary>
        /// 테스트 서버로 붙는 실행인지(true) 운용 서버인지(false, 기본값).
        /// 현장에서 두 환경을 헷갈리지 않도록 화면 모양만 바꾼다 — 프로그램 아이콘(TMES_TEST.ico),
        /// 메인 화면 상단 바 색(#EB6E1D), 접힌 메뉴의 로고(TMES_TEST_256.png).
        /// 실제로 붙을 서버는 이 값이 아니라 <see cref="ApiBaseUrl"/>이 정하므로 둘을 함께 맞출 것.
        /// </summary>
        public bool IsTestServer { get; set; } = false;
        public int UploadMaxMegaBytes { get; set; } = 10;
        /// <summary>자동 로그아웃까지의 세션 유지 시간(분). 메인 화면 우측 상단 카운트다운의 시작 값이다.</summary>
        public int SessionTimeoutMinutes { get; set; } = 300;

        /// <summary>
        /// 안 읽은 알림 개수 폴링 주기(초). 상단 종 아이콘 배지 숫자를 이 간격으로 갱신한다.
        /// 0 이하로 두면 폴링을 하지 않는다(서버가 없을 때 호출을 아예 막고 싶은 경우).
        /// </summary>
        public int NotificationPollingSeconds { get; set; } = 60;

        /// <summary>
        /// 설비(Nutrunner) TCP 접속 제한 시간(ms). Request · Event 두 채널 모두 적용. 기본 500.
        /// 0 이하면 OS 기본값(약 21초)까지 기다린다. 망이 느려 "접속 시간 초과"가 잦으면 늘린다.
        /// </summary>
        public int ModbusConnectTimeoutMs { get; set; } = 500;

        /// <summary>
        /// 설비 연결의 TCP KeepAlive — 무통신 후 첫 확인까지의 시간(ms). 기본 5000. 0 이하면 KeepAlive를 켜지 않는다.
        /// 랜선 단선 · 장비 전원 차단처럼 신호 없이 끊긴 연결을 감지하려고 쓴다.
        /// </summary>
        public int ModbusKeepAliveTimeMs { get; set; } = 5000;

        /// <summary>
        /// TCP KeepAlive 재확인 간격(ms). 기본 5000. Windows는 10회 재확인하므로
        /// 끊김 감지까지 약 ModbusKeepAliveTimeMs + 10 × 이 값(기본 약 55초)이 걸린다.
        /// </summary>
        public int ModbusKeepAliveIntervalMs { get; set; } = 5000;

        /// <summary>
        /// 설비(Nutrunner) Request 채널 요청 하나의 응답 대기 시간(ms) — Alive(5초 주기) 응답 대기에도 쓴다. 기본 2000.
        /// 예전에는 <see cref="ModbusConnectTimeoutMs"/>(500)를 같이 써서, 장비가 체결 · 설정 처리로 잠깐만 늦어도
        /// Alive 응답 없음으로 연결을 끊고 재연결했다. 0 이하면 1000.
        /// </summary>
        public int ModbusResponseTimeoutMs { get; set; } = 2000;

        /// <summary>
        /// 설비 재연결 3회(2초 간격)가 모두 실패한 뒤 다음 3회를 시작하기까지 쉬는 시간(ms). 기본 10000. 0 이하면 10000.
        /// 실패 횟수가 늘어도 이 시간은 바꾸지 않는다.
        /// </summary>
        public int ModbusReconnectWaitMs { get; set; } = 10000;

        /// <summary>
        /// 설비 Request 채널(Port, 예 5000) 접속 완료 후 Event 채널(Port+1, 예 5001) 접속을 시작하기까지의 대기 시간(ms). 기본 500.
        /// 두 포트를 연달아 열면 Event 포트 접속을 받지 못하는 장비가 있어 순서를 두고 연다. 0 이하면 기다리지 않는다.
        /// </summary>
        public int ModbusEventConnectDelayMs { get; set; } = 500;

        /// <summary>
        /// 설비 접속 테스트 팝업에서 포트 하나에 접속을 기다리는 시간(ms). 이 안에 붙지 않으면 실패로 센다. 기본 2000.
        /// 운영 연결의 <see cref="ModbusConnectTimeoutMs"/>와 따로 둔다 — 테스트는 "느리지만 붙는지"까지 보려고 넉넉히 기다린다.
        /// </summary>
        public int ModbusTestTimeoutMs { get; set; } = 2000;

        /// <summary>
        /// 설비 접속 테스트에서 끊은 뒤 다음 접속까지 쉬는 시간(ms). 기본 300.
        /// 끊자마자 다시 붙으면 장비가 이전 소켓을 아직 정리하지 못해 거부하는 경우가 있어 잠깐 둔다.
        /// </summary>
        public int ModbusTestIntervalMs { get; set; } = 300;

        /// <summary>
        /// 작업 표준/도면 PDF가 들어 있는 SMB 공유 폴더 접속 정보(App.config의 <c>DocumentShare</c>).
        /// <see cref="DocumentShareSettings.RootPath"/>가 비어 있으면 POP 화면은 도면 영역에
        /// "공유 폴더 미설정" 안내만 표시하고 파일 접근을 시도하지 않는다.
        /// </summary>
        public DocumentShareSettings DocumentShare { get; set; } = new DocumentShareSettings();
    }

    /// <summary>
    /// 도면/작업표준 문서 보관소(SMB 공유 폴더) 접속 정보.
    ///
    /// 인증은 두 가지를 모두 지원한다:
    ///  - <see cref="UserName"/>이 비어 있으면 <b>현재 로그인한 윈도우 계정</b>으로 UNC 경로에 그대로 접근한다(권장).
    ///  - 값이 있으면 그 계정으로 공유에 연결한다(WNetAddConnection2).
    ///
    /// 보안 주의: <see cref="Password"/>를 채우면 App.config에 <b>평문으로 남는다</b>.
    /// 가능하면 PC를 도메인에 넣고 계정을 비워 두거나, 배포 시 설정 파일 접근 권한을 제한할 것.
    /// </summary>
    public class DocumentShareSettings
    {
        /// <summary>공유 폴더 루트. 예: <c>\\192.168.0.50\drawings</c> 또는 <c>\\fileserver\meis\도면</c>.</summary>
        public string RootPath { get; set; }

        /// <summary>공유 접속 계정. 비우면 현재 윈도우 계정을 사용한다.</summary>
        public string UserName { get; set; }

        /// <summary>공유 접속 비밀번호(<see cref="UserName"/>을 채운 경우에만 사용).</summary>
        public string Password { get; set; }

        /// <summary>도메인/컴퓨터 이름. 계정에 <c>DOMAIN\user</c> 형태로 이미 들어 있으면 비워 둔다.</summary>
        public string Domain { get; set; }

        /// <summary>공유 폴더 루트가 설정되어 있는지(설정 전에는 도면 조회를 시도하지 않는다).</summary>
        public bool IsConfigured => !string.IsNullOrWhiteSpace(RootPath);

        /// <summary>전용 계정으로 연결해야 하는지(비어 있으면 현재 윈도우 계정을 그대로 쓴다).</summary>
        public bool UsesExplicitCredentials => !string.IsNullOrWhiteSpace(UserName);

        /// <summary>WNetAddConnection2에 넘길 계정 이름(도메인이 따로 지정돼 있으면 <c>DOMAIN\user</c>로 합친다).</summary>
        public string ResolveUserName()
        {
            if (string.IsNullOrWhiteSpace(UserName))
                return null;

            if (string.IsNullOrWhiteSpace(Domain) || UserName.Contains("\\") || UserName.Contains("@"))
                return UserName.Trim();

            return Domain.Trim() + "\\" + UserName.Trim();
        }
    }
}
