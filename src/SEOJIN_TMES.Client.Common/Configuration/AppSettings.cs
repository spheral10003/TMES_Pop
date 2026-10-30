using System.Collections.Generic;

namespace SEOJIN_TMES.Client.Common.Configuration
{
    /// <summary>App.config에서 로드되는 클라이언트 실행 설정. 값 자체는 SEOJIN_TMES.Client/App.config에서 관리.</summary>
    public class AppSettings
    {
        public string ApiBaseUrl { get; set; }
        public int ApiTimeoutSeconds { get; set; } = 30;

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
        //public List<EquipmentEndpoint> Equipments { get; set; } = new List<EquipmentEndpoint>();

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

    /// <summary>
    /// 설비(Tool) 1대에 대한 Modbus-TCP 접속 정보.
    /// 장비 스펙에 따라 한 설비에 두 종류의 Connection이 존재한다.
    ///  - Standard Modbus-TCP connection    : <see cref="Port"/>       (요청/응답 한 쌍으로 동작하는 일반 Modbus 통신, 모든 Read/Write)
    ///  - Customized Modbus-TCP connection  : <see cref="EventPort"/>  (Port+1, Tool→Controller 실시간 이벤트 전용, Read/Write 미지원)
    /// 두 포트 번호는 원래 Advertise / Scan ack / Search ack 메시지로 받는 값이며,
    /// 현재는 App.config에 직접 적어 사용한다(자동 탐색은 TODO).
    /// </summary>
    //public class EquipmentEndpoint
    //{
    //    public string EquipmentId { get; set; }
    //    public string EquipmentName { get; set; }
    //    public string Host { get; set; }

    //    /// <summary>Standard Modbus-TCP connection 포트. Modbus 기본값은 502.</summary>
    //    public int Port { get; set; } = 502;

    //    /// <summary>
    //    /// Customized(이벤트) Modbus-TCP connection 포트.
    //    /// 0 이하면 스펙 기본 규칙대로 <see cref="Port"/> + 1을 사용한다(<see cref="ResolveEventPort"/>).
    //    /// </summary>
    //    public int EventPort { get; set; }

    //    /// <summary>표준 Connection(요청/응답) 사용 여부.</summary>
    //    public bool UseStandardConnection { get; set; } = true;

    //    /// <summary>Customized(이벤트) Connection 사용 여부.</summary>
    //    public bool UseEventConnection { get; set; } = true;

    //    /// <summary>MBAP Unit Identifier(Slave 주소).</summary>
    //    public byte UnitId { get; set; } = 1;

    //    /// <summary>표준 Connection에서 요청 후 응답을 기다리는 시간(ms).</summary>
    //    public int ResponseTimeoutMs { get; set; } = 3000;

    //    public bool AutoConnect { get; set; } = true;

    //    /// <summary>실시간 이벤트 수신 시 장비에 확인(ACK)을 돌려줄지 여부.</summary>
    //    public ModbusEventAckMode EventAckMode { get; set; } = ModbusEventAckMode.None;

    //    /// <summary>논리 명령(Start/Stop/Reset/상태조회)이 어떤 레지스터로 매핑되는지. 미지정 시 기본값 사용.</summary>
    //    public ModbusRegisterMap RegisterMap { get; set; }

    //    /// <summary>이벤트 Connection 실제 포트 번호(설정값 없으면 Port+1).</summary>
    //    public int ResolveEventPort() => EventPort > 0 ? EventPort : Port + 1;

    //    public ModbusRegisterMap ResolveRegisterMap() => RegisterMap ?? new ModbusRegisterMap();
    //}

    ///// <summary>Customized(이벤트) Connection에서 수신 이벤트에 대한 응답 처리 방식.</summary>
    //public enum ModbusEventAckMode
    //{
    //    /// <summary>아무것도 돌려보내지 않는다(장비가 단방향 push만 기대하는 경우).</summary>
    //    None,

    //    /// <summary>받은 프레임을 같은 Transaction Id로 그대로 되돌려 보낸다.</summary>
    //    Echo
    //}

    ///// <summary>
    ///// 논리 명령/상태 조회를 실제 Modbus 주소로 옮기는 매핑 표.
    ///// TODO(장비 레지스터 맵 확정 전 임시값): 실제 주소/값 표를 받으면 App.config의
    ///// Equipments[].RegisterMap 으로 덮어쓰거나 여기 기본값을 교체한다.
    ///// </summary>
    //public class ModbusRegisterMap
    //{
    //    /// <summary>Start/Stop/Reset 명령을 쓰는 제어 레지스터 주소.</summary>
    //    public ushort ControlRegisterAddress { get; set; } = 0;

    //    public ushort StartValue { get; set; } = 1;
    //    public ushort StopValue { get; set; } = 2;
    //    public ushort ResetValue { get; set; } = 3;

    //    /// <summary>상태 조회 시 읽을 시작 주소와 레지스터 개수.</summary>
    //    public ushort StatusRegisterAddress { get; set; } = 0;
    //    public ushort StatusRegisterCount { get; set; } = 8;

    //    /// <summary>true면 상태 조회에 Input Registers(FC 0x04), false면 Holding Registers(FC 0x03)를 쓴다.</summary>
    //    public bool StatusUsesInputRegisters { get; set; }
    //}
}
