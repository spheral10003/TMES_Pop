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
        public int LogApiResponseBodyMaxLength { get; set; } = 4000;
        public string DefaultCulture { get; set; } = "ko-KR";

        /// <summary>자동 로그아웃까지의 세션 유지 시간(분). 메인 화면 우측 상단 카운트다운의 시작 값이다.</summary>
        public int SessionTimeoutMinutes { get; set; } = 300;
        //public List<EquipmentEndpoint> Equipments { get; set; } = new List<EquipmentEndpoint>();

        /// <summary>
        /// 서버 API 대신 목업 데이터를 사용할지 여부(화면 확인용). App.config의 <c>UseMockData</c>로 켠다.
        /// 현재는 POP 진입 화면의 [조회]에만 적용된다. 운영 배포에서는 반드시 false로 둘 것.
        /// </summary>
        public bool UseMockData { get; set; }

        /// <summary>
        /// 안 읽은 알림 개수 폴링 주기(초). 상단 종 아이콘 배지 숫자를 이 간격으로 갱신한다.
        /// 0 이하로 두면 폴링을 하지 않는다(서버가 없을 때 호출을 아예 막고 싶은 경우).
        /// </summary>
        public int NotificationPollingSeconds { get; set; } = 60;

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
