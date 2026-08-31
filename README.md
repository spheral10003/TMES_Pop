# SEOJIN_MEIS.Client — MES POP 클라이언트

WinForms(.NET Framework 4.7.2) + DevExpress 26.1 기반 MES POP 클라이언트 스캐폴딩.
서버와는 REST API(Token 인증)로만 통신하며, 업무 로직(수량 계산/판정/집계 등)은 전부 서버에 있고
클라이언트는 화면 표시·입력 전달·설비 Modbus-TCP 통신만 담당한다.

> **이 문서는 시스템 설명서다.** 코드를 고칠 때 지켜야 할 **규칙**은 `CLAUDE.md`에 따로 모아 두었다:
> [루트](CLAUDE.md) · [SEOJIN_MEIS.Client](src/SEOJIN_MEIS.Client/CLAUDE.md) · [Common](src/SEOJIN_MEIS.Client.Common/CLAUDE.md) ·
> [ApiClient](src/SEOJIN_MEIS.Client.ApiClient/CLAUDE.md) · [Equipment](src/SEOJIN_MEIS.Client.Equipment/CLAUDE.md)
>
> 빌드 방법(`dotnet build`가 실패하는 이유와 우회 방법)도 [루트 CLAUDE.md](CLAUDE.md#빌드)에 있다.

## 빌드 확인 상태 (2026-08-24 기준, 이 개발 PC)

- `SEOJIN_MEIS.Client.Common` / `SEOJIN_MEIS.Client.ApiClient` / `SEOJIN_MEIS.Client.Equipment` : `dotnet build`로 **정상 빌드 확인**.
- `SEOJIN_MEIS.Client` (WinForms, DevExpress) : **Visual Studio(또는 VS에 포함된 MSBuild.exe)로 정상 빌드 확인**.
  ClickOnce 매니페스트를 만들기 때문에 `dotnet build`로는 빌드되지 않는다 → [빌드 방법](CLAUDE.md#빌드)
- 이 PC에는 DevExpress 26.1이 설치되어 있지만 **Tab & Navigation(XtraTab), Splash Screen 모듈은 빠져 있다.**
  그래서 메인 화면의 탭 영역은 우선 표준 WinForm 컨트롤로 만든 자체 탭 스트립으로 구현했다(`Forms/MainForm.cs`).
  DevExpress Installer에서 [Modify]로 두 모듈을 추가 설치하면, `MainForm.cs`의 탭 스트립 부분만
  `DevExpress.XtraTab.XtraTabControl`/`XtraTabPage`로 교체하면 된다(다른 코드는 변경 불필요).

## 폼 구조 — VS 디자이너 호환

모든 폼/유저컨트롤은 `X.cs`(로직) + `X.Designer.cs`(디자이너 생성 코드) 두 파일로 분리되어 있고,
각 폼은 **매개변수 없는 생성자** + 생성 직후 호출하는 **`Initialize(context)`** 구조를 갖는다.

`InitializeComponent()`에 넣으면 VS 디자이너가 죽는 구문 목록과 그 이유, 새 화면 추가 절차는
→ **[src/SEOJIN_MEIS.Client/CLAUDE.md](src/SEOJIN_MEIS.Client/CLAUDE.md)** (1~3번, 9번 항목)

## 솔루션 구조

```
SEOJIN_MEIS.sln
src/
  SEOJIN_MEIS.Client/            WinForms 실행 프로젝트 (DevExpress UI, ClickOnce 배포 대상)
    Forms/
      LoginForm.cs               로그인 (RestAPI Token 발급)
      MainForm.cs                메인 셸: 좌측 3-Depth 트리 메뉴 + 우측 화면 탭 영역
      WorkOrderEntry/            MEIS > 작업관리 > POP 진입 화면 (작업지시 조회 → 작업 진입 확인 팝업)
      Pop/                       POP 본 화면 (진행 중 공정 정보 + 투입자재/BOM 스팩/검사 스팩/연결 장비 탭)
      Notification/              알림 팝업 (상단 종 아이콘 → 목록 + 상세)
      Common/                    공통 UI (메시지/확인 창, 그리드 컬럼 구성, 화면 이동 계약)
      ProductionStatus/          MEIS > 작업 현황 조회 > 생산 현황조회
      WorkStatus/                MEIS > 작업 현황 조회 > 작업 조회 현황 (+ 생산품목 선택 팝업)
      WorkIntegratedStatus/      MEIS > 작업통합 개요 > 작업통합 현황 조회
    Theme/                       색·폰트·아이콘 (AppTheme, AppIcons, AppImages)
    Images/                      브랜드 이미지 원본 (로고, 로그인 일러스트, 상단 바 아이콘, flag/ 국기)
    Menu/                        3-Depth 메뉴 트리 정의 (MenuCatalog, MenuNode)
    Localization/                다국어 리소스 (Strings.resx=한국어 기본, .en=영어, .vi=베트남어)
    Mock/                        서버 없이 화면 확인용 목업 데이터 (App.config의 UseMockData=true일 때만 사용)
    Composition/                 컴포지션 루트 보조 (App.config 로더, ClientContext)
    Program.cs                   진입점: 설정 로드 → API/설비 조립 → 로그인 → 메인화면
    App.config                   ApiBaseUrl, 설비(Host/Port) 목록 등 환경설정

  SEOJIN_MEIS.Client.Common/     DTO, 설정 모델, 로깅 (서버·클라이언트 공용 계약)
    Storage/                     도면 PDF 보관소(SMB 공유 폴더) 접근 — 자격증명 연결 + 파일 읽기
  SEOJIN_MEIS.Client.ApiClient/  REST API 클라이언트 (Token 인증, 401 자동 재시도/재로그인 신호)
  SEOJIN_MEIS.Client.Equipment/  설비 Modbus-TCP 통신 (표준 + Customized 이벤트 Connection, 각 소켓별 전용 수신 스레드)
```

## 서버 통신 / 로그인 (요구사항 3, 5)

- 모든 엔드포인트는 `ApiClientBase.ApiPrefix` 상수(`"api/v1.0/"`)를 접두사로 사용한다. **API 개정(Revision) 시 이 상수 한 곳만 바꾸면 전체에 반영**되므로, 각 클라이언트에 버전 문자열을 직접 적지 말 것.
- `SEOJIN_MEIS.Client.ApiClient.AuthApiClient` : `GET api/v1.0/common/my-salt`, `POST api/v1.0/auth/login`, `POST api/v1.0/auth/refresh`
- **로그인은 nonce 방식이라 두 번 호출한다.** 순서는 `AuthApiClient.LoginAsync` 안에서 이어지고, 화면은 평문 비밀번호만 넘긴다(평문은 서버로 나가지 않는다):

  | 순서 | 호출 | 내용 |
  | --- | --- | --- |
  | ① | `GET api/v1.0/common/my-salt` | 응답이 `{ "data": "31d5dd…" }` 형태로 한 겹 싸여 온다 → `SaltResponse.Data` |
  | ② | 해시 계산 | `password = SHA256(평문 비밀번호 + salt)` (소문자 16진수 64자) — [PasswordHash.cs](src/SEOJIN_MEIS.Client.Common/Security/PasswordHash.cs) |
  | ③ | `POST api/v1.0/auth/login` | 바디 `{ "orgNo": "…", "password": "<해시>", "salt": "<①에서 받은 값>" }` |

  요청 필드명은 서버 스펙 그대로 나가야 하므로 `LoginRequest`에 `[JsonProperty]`로 고정했다(기본 직렬화는 PascalCase라 그냥 두면 `OrgNo`로 나간다).
  **해시 조합 순서가 서버와 다르면 인증이 안 통과한다** — 다르면 `PasswordHash.ForLogin` 한 줄(예: `Sha256Hex(salt + plainPassword)`)만 고치면 된다.
- 응답 바디는 `AccessToken`/`RefreshToken` 두 필드만 받는다(`LoginModels.cs`).
  성공/실패는 **HTTP 상태코드로 판정**한다 — 2xx면 성공, 401이면 `LoginForm`이 "아이디 또는 비밀번호가 올바르지 않습니다"로 별도 처리, 그 외 상태코드는 코드번호와 함께 표시.
- 로그인 응답에는 토큰만 있고 사용자명/부서 등 프로필 정보가 없다. 그래서 프로필은 별도 API(`SEOJIN_MEIS.Client.ApiClient.UserProfileApiClient` → `GET api/v1.0/users/me`, **경로는 가정값**)로 조회하며, 사용자 정보 팝업을 열 때 호출한다(아래 [사용자 정보 팝업](#사용자-정보-팝업) 참고). 조회 전 상단바에는 로그인 아이디만 표시되고, 조회 후에는 `이름 (아이디)`로 바뀐다.
- 로그인 시 선택한 **접속서버/워크스테이션**은 로그인 API 인자에는 넣지 않지만, 화면 표시용으로 `UserSession.Server`/`UserSession.Workstation`에 담아둔다(팝업의 워크스테이션 항목이 이 값을 쓴다).
- `SEOJIN_MEIS.Client.ApiClient.AuthTokenHandler` : 모든 인증 필요 호출에 `Authorization: Bearer` 자동 첨부, 401 시 RefreshToken으로 1회 재시도, 실패하면 `TokenProvider.SessionExpired` 이벤트 발행(재로그인 유도는 UI 계층에서 처리하도록 열어둠)
- 클라이언트는 토큰을 프로세스 메모리에만 보관(`ITokenProvider`)하고, 업무 판정 로직은 전혀 갖지 않는다 — 화면은 DTO를 그대로 표시/전송만 한다.
- 로그인 화면(`LoginForm`)은 좌측 로고 자리(`PictureEdit`, 현재 빈 플레이스홀더) + 우측 "접속 사용자 정보" `GroupControl`(접속서버/아이디/비밀번호/워크스테이션/아이디 저장)로 구성. 접속서버·워크스테이션은 현재 UI 표시용 콤보일 뿐 로그인 API 인자에는 포함하지 않는다(TODO: 용도 확정 후 연결).
- **아이디 저장**: 체크하면 로그인 **성공 시점에만** 아이디를 남긴다(실패한 아이디는 저장하지 않는다). 체크를 풀면 저장해 둔 값도 함께 지운다. **비밀번호는 어떤 경우에도 저장하지 않는다.**
  저장 위치는 PC/계정 단위 설정이라 App.config가 아니라 `%APPDATA%\SEOJIN_MEIS.Client\user-preferences.json`([UserPreferences.cs](src/SEOJIN_MEIS.Client.Common/Configuration/UserPreferences.cs)).
  파일을 못 읽거나 못 써도 로그인은 그대로 진행된다(실패는 로그만 남긴다). 다음 실행 때 아이디가 채워져 있으면 커서는 비밀번호 칸으로 간다.

## 설비 통신 — Modbus-TCP (요구사항 6)

장비 연결은 TCP/IP이고 그 위의 통신 규격은 **Modbus**다. `SEOJIN_MEIS.Client.Equipment` 프로젝트에
장비 스펙의 **두 가지 Connection을 모두** 구현했으며, 구조는 기존 요구("하나의 수신 지점 → 소켓별 멀티 스레드 클라이언트")를 유지한다.

| 구분 | 포트 | 용도 |
| --- | --- | --- |
| Standard Modbus-TCP connection (`EquipmentModbusClient`) | `Port` | 요청/응답이 한 쌍으로 구성된 일반 Modbus-TCP 통신. **실시간 이벤트 수신을 제외한 모든 Read/Write는 이 Connection으로만 나간다.** |
| Customized Modbus-TCP connection (`EquipmentEventClient`) | `EventPort`(기본 `Port + 1`) | Tool(Slave) → Controller(Master) 방향 실시간 이벤트 수신 전용. **Modbus Read/Write 미지원** — 그래서 이 클래스에는 요청 전송 API가 아예 없다. |

설비 1대는 위 두 Connection을 각각 **소켓 1개 + 전용 수신 스레드 1개**로 유지하고, 두 Connection에서 올라오는 이벤트는
`EquipmentModbusManager`가 하나의 지점(DataReceived/ConnectionChanged/ErrorOccurred)으로 모아 상위(POP 화면)에 전달한다.
모든 이벤트 인자에는 어느 Connection에서 온 것인지 `EquipmentChannel`(Standard/Event)이 함께 실린다.

- `ModbusTcpConnection` : 두 Connection의 공통 부분(TCP 접속, 전용 수신 스레드, MBAP 프레이밍, 상태/오류 이벤트). 차이는 "수신한 프레임을 어떻게 처리하는가"(`HandleFrame`)뿐이다.
- `Protocol/ModbusTcpCodec` : 표준 MBAP 코덱 — `[Transaction Id(2)][Protocol Id(2)=0][Length(2)][Unit Id(1)][FC(1)][Data]`, big-endian.
  Length 필드로 프레임 경계가 확정되므로 **STX/ETX 같은 구분자나 체크섬 추정이 필요 없다**(TCP가 무결성을 보장). 분할 수신·연속 수신·동기 이탈(쓰레기 바이트) 모두 처리한다.
  장비가 MBAP를 변형해서 쓰면 `IModbusCodec` 구현체만 교체하면 되고 상위 코드는 그대로다.
- 요청/응답 짝은 **MBAP Transaction Id**로 맞춘다. 장비가 동시 요청을 못 받는 경우가 흔해 요청은 Connection 단위로 직렬화하고, `ResponseTimeoutMs`(기본 3초) 안에 응답이 없으면 `ModbusTimeoutException`, 예외 응답(FC | 0x80)이면 `ModbusProtocolException`을 던진다. 연결이 끊기면 대기 중인 요청은 즉시 실패한다.
- 지원 기능 코드: `0x01/0x02/0x03/0x04`(Read), `0x05/0x06/0x0F/0x10`(Write). 표에 없는 벤더 확장 코드는 `ModbusRequest.Custom(fc, data)`로 raw PDU를 그대로 보낼 수 있다.
- 화면/업무 코드는 논리 명령(`EquipmentCommand.Start/Stop/Reset/StatusRequest`)만 다루고, 실제 주소·값 변환은 `App.config`의 `RegisterMap` 설정이 담당한다 — 레지스터 맵이 바뀌어도 화면 코드는 손대지 않는다.
- 이벤트 수신 시 장비에 확인(ACK)을 돌려줘야 하는 구성이면 `EventAckMode`를 `"Echo"`로 두면 받은 프레임을 같은 Transaction Id로 회신한다(기본 `"None"`).
- 설비 접속 정보는 `App.config`의 `Equipments`(JSON)에서 로드한다. 필드 설명은 App.config 주석 참고: `Port`/`EventPort`/`UseStandardConnection`/`UseEventConnection`/`UnitId`/`ResponseTimeoutMs`/`EventAckMode`/`RegisterMap`.
- `Connect()`는 즉시 리턴하고, 실제 TCP 접속은 각 Connection 전용 스레드 안에서 수행한다(접속 시도 자체가 UI 스레드를 막지 않도록). 접속 타임아웃은 5초(`ConnectTimeoutMs`). **주의**: `App.config`의 기본 예시 IP(`192.168.0.101/102`)는 실재하지 않는 더미 값이라 항상 5초 후 `Faulted` 상태가 된다 — 정상 동작이며, 실제 설비 IP로 교체하거나 당장 필요 없으면 `AutoConnect`를 `false`로 두면 된다.
- **확인 방법**: 로컬 가짜 슬레이브(TcpListener)를 띄워 코덱 프레이밍(정상/분할/연속/예외응답/재동기화), 표준 Connection의 요청→응답, 이벤트 Connection의 실시간 push 수신까지 스모크 테스트로 통과 확인했다(2026-08-24). 실장비 연동 시에는 아래 TODO의 레지스터 맵만 채우면 된다.

**아직 안 붙인 것**: `Advertise` / `Scan ack` / `Search ack` 메시지로 IP·포트를 자동으로 받아오는 탐색(discovery) 절차는 구현하지 않았다.
현재는 그 값들을 `App.config`에 직접 적어 쓰고, `EventPort`는 스펙 기본 규칙대로 `Port + 1`을 자동 계산한다.

설비 코드를 고칠 때 지켜야 할 규칙(두 커넥션 역할 분리, 이벤트가 올라오는 스레드, 레지스터 맵 위치,
접속의 비동기 성질, 코덱 교체 지점) → **[src/SEOJIN_MEIS.Client.Equipment/CLAUDE.md](src/SEOJIN_MEIS.Client.Equipment/CLAUDE.md)**

## API 오류 처리 — 앱이 죽지 않고 안내 창만 띄운다

서버가 안 떠 있거나 네트워크가 끊긴 상태에서 API를 호출하면 `HttpRequestException`/타임아웃이 그대로 올라와
`async void` 이벤트 핸들러를 타고 앱이 중단됐다(특히 디버거가 붙은 Debug 실행). 아래 3중으로 막는다.

| 단계 | 위치 | 하는 일 |
| --- | --- | --- |
| 1. 통신 실패 변환 | [LoggingHandler.cs](src/SEOJIN_MEIS.Client.ApiClient/LoggingHandler.cs) | 연결 실패·DNS·SSL 등 응답을 못 받은 예외를 `ApiException`(`StatusCode=0`)으로 바꿔 올린다 |
| 2. 타임아웃 판별 | [ApiClientBase.cs](src/SEOJIN_MEIS.Client.ApiClient/ApiClientBase.cs) | HttpClient는 타임아웃도 `TaskCanceledException`으로 던진다. **호출자가 준 원본 토큰**이 취소되지 않았으면 타임아웃으로 보고 `ApiException`으로 바꾼다(핸들러 안에서는 링크 토큰이라 구분 불가). 호출자가 실제로 취소한 경우는 오류가 아니므로 그대로 통과 |
| 3. 마지막 방어선 | [GlobalErrorHandler.cs](src/SEOJIN_MEIS.Client/Composition/GlobalErrorHandler.cs) | `Application.ThreadException` / `AppDomain.UnhandledException`을 받아 공통 안내 창으로 바꾼다. `SetUnhandledExceptionMode(CatchException)`을 걸어 Debug 실행에서도 같은 경로를 타게 한다. fire-and-forget 작업 예외(`TaskScheduler.UnobservedTaskException`)는 창 없이 로그만 |

이 구조를 유지하기 위해 API 클라이언트를 고칠 때 지켜야 할 규칙(`ApiPrefix` 한 곳, 예외를 삼키지 않기,
401 재시도 범위, 바디 로깅 금지) → **[src/SEOJIN_MEIS.Client.ApiClient/CLAUDE.md](src/SEOJIN_MEIS.Client.ApiClient/CLAUDE.md)**

1·2단계 덕분에 각 화면에 이미 있던 `catch (ApiException)` 이 그대로 동작해서, **안내 창을 닫고 계속 작업**할 수 있다.
통신 실패인지 서버가 준 오류인지는 `ApiException.IsTransportFailure`로 구분한다(로그인 화면은 이 값으로 "서버에 연결할 수 없습니다" 안내를 따로 띄운다).

안내 문구 예 — `서버에 연결할 수 없습니다. 서버 주소와 네트워크 상태를 확인해 주세요.` / `(GET http://localhost:8080/api/v1.0/workorders/search?...)` / `원인: 대상 컴퓨터에서 연결을 거부했으므로 연결하지 못했습니다`.
전체 스택은 `AppLog.Error`로 콘솔·출력 창에 남는다.

> VS에서 **디버그 > 예외 설정**의 "던질 때 중단(Common Language Runtime Exceptions)"이 켜져 있으면 위 처리와 무관하게 디버거가 먼저 멈춘다 — 이건 IDE 설정이라 코드로 못 막는다. 계속(F5)하면 안내 창이 뜬다.

## 알림 — 1분 폴링 배지 + 알림 팝업(목록/상세)

상단 바의 종 아이콘에 안 읽은 알림 개수가 배지로 붙고, 누르면 알림 팝업이 열린다.

```
[1분마다] GET notifications/unread-count  →  종 아이콘 빨간 배지 숫자 갱신
   ↓ (종 아이콘 클릭)
알림 팝업 (NotificationForm)
   조회 조건: 사용자명 / 제목 / 읽음여부(전체·안읽음·읽음)  →  [조회]
   좌측 목록에서 행 선택  →  우측 상세(발신자아이디 / 제목 / 내용)가 바로 따라 바뀐다
   체크박스로 여러 건 선택  →  [읽음]  →  읽음 처리 후 목록 재조회
   ↓ (닫기)
배지 숫자를 다시 조회해서 읽음 처리 결과를 반영한다
```

| 파일 | 역할 |
| --- | --- |
| [Forms/Notification/NotificationForm.cs](src/SEOJIN_MEIS.Client/Forms/Notification/NotificationForm.cs) | 조회조건 카드 + 좌측 목록 그리드([읽음]) + 우측 상세(읽기 전용) |
| [SEOJIN_MEIS.Client.ApiClient/INotificationApiClient.cs](src/SEOJIN_MEIS.Client.ApiClient/INotificationApiClient.cs) | 개수 폴링 / 목록 조회 / 읽음 처리 |
| [Models/NotificationDto.cs](src/SEOJIN_MEIS.Client.Common/Models/NotificationDto.cs) | 알림 한 건 + 읽음여부 조건 enum + 읽음 처리 요청 바디 |
| [Models/ApiDataResponse.cs](src/SEOJIN_MEIS.Client.Common/Models/ApiDataResponse.cs) | 이 서버가 쓰는 `{ "data": ... }` 래퍼 (my-salt·알림 응답에 공통 사용) |

**서버 API**(경로/응답 형태는 **가정값** — 실제 스펙 확정 후 조정):

| 용도 | 호출 | 응답 |
| --- | --- | --- |
| 미읽음 개수(폴링) | `GET api/v1.0/notifications/unread-count` | `{ "data": 3 }` |
| 목록 조회 | `GET api/v1.0/notifications?userName=&subject=&isRead=` | `{ "data": [ … ] }` (입력된 조건만 붙고, 읽음여부 "전체"면 `isRead`를 아예 보내지 않는다) |
| 읽음 처리 | `POST api/v1.0/notifications/read` | 바디 `{ "notificationIds": [1,2] }` |

**목록 컬럼**: NO · 법인코드 · 알림양식코드 · 제목 · 발신자아이디 · 발신일시 · 읽음여부.
맨 앞 체크박스 열은 `MultiSelectMode = CheckBoxRowSelect`로 나오는 것이며, [읽음]에서 여러 건을 한 번에 처리하기 위한 것이다.

**동작 규칙 / 가정** — 목업에 명시가 없어 다음과 같이 정했다. 운영 규칙이 다르면 알려주면 맞춘다:

- **폴링 주기는 `App.config`의 `NotificationPollingSeconds`(기본 60초)**. `0` 이하로 두면 폴링을 아예 하지 않는다(서버가 없을 때 호출을 막고 싶은 경우).
- **폴링 실패는 안내 창을 띄우지 않는다** — 서버가 잠깐 끊긴 동안 1분마다 오류 창이 뜨면 작업을 할 수 없다. 로그(`AppLog`)만 남기고 배지는 이전 값을 유지한다.
- 팝업을 열면 조건 없이 한 번 자동 조회한다(알림은 전체 목록을 보는 것이 기본).
- 조건 입력란에서 **Enter를 누르면 조회**된다.
- [읽음]은 체크한 행들을 대상으로 하고, **체크가 하나도 없으면 현재 포커스 행 한 건**을 대상으로 본다. 이미 읽은 건은 서버로 보내지 않는다.
- 읽음 처리 후에는 목록을 **다시 조회**한다(서버 상태와 화면이 어긋나지 않도록).
- 배지 숫자는 10 이상이면 `9+`로 표시한다.
- 내용에 LF(`\n`)만 섞여 와도 줄이 나뉘도록 표시 직전에 CRLF로 정규화한다(WinForms 여러 줄 편집기는 LF만으로는 줄을 나누지 않는다).

> **바뀐 점**: 이전에는 이 배지가 *설비 연결 오류 건수*(클라이언트가 모은 값)를 보여주고, 클릭하면 그 목록을 메시지 창으로 띄웠다.
> 이제 배지는 **서버 미읽음 알림 수**이고 클릭하면 알림 팝업이 열린다.
> 설비 연결 오류는 `AppLog`와 POP 화면의 설비 로그 콘솔에 그대로 남는다 — 배지로 다시 올리고 싶으면 알려주면 붙인다.

## POP 진입 화면 — 작업지시 조회 → 작업 진입 확인 → POP

POP을 실행(메뉴 클릭)하면 **도입 화면**이 먼저 열리고, 여기서 작업 대상을 고른 뒤 POP 본 화면으로 넘어간다.

```
[POP 메뉴]
   ↓
작업지시 조회 화면 (WorkOrderEntryScreen)
   조회 조건: 작업지시번호 / 생산품목 시리얼번호  →  [조회]  →  작업 지시 정보 목록
   목록에서 1건 선택  →  [작업]
   ↓
<작업 진입 확인> 팝업 (WorkEntryConfirmForm)
   선택한 작업지시의 공정 목록(라우팅 그룹 / 라우팅 공정 / 페이지 공정 / 진행 상태)
   이동할 페이지 공정 선택  →  [이동]
   ↓
POP 화면 (PopScreen) — 같은 탭의 내용만 바뀌고, 탭 제목은 "POP - {작업지시번호}"가 된다
```

| 파일 | 역할 |
| --- | --- |
| [Forms/WorkOrderEntry/WorkOrderEntryScreen.cs](src/SEOJIN_MEIS.Client/Forms/WorkOrderEntry/WorkOrderEntryScreen.cs) | 조회 조건 카드([조회]/[작업] + 조건 2개) + 작업 지시 정보 그리드 |
| [Forms/WorkOrderEntry/WorkEntryConfirmForm.cs](src/SEOJIN_MEIS.Client/Forms/WorkOrderEntry/WorkEntryConfirmForm.cs) | &lt;작업 진입 확인&gt; 모달. 결과는 `SelectedProcess`로 받는다 |
| [Forms/WorkOrderEntry/WorkEntryContext.cs](src/SEOJIN_MEIS.Client/Forms/WorkOrderEntry/WorkEntryContext.cs) | 고른 작업 대상(작업지시 1건 + 페이지 공정 1건). POP 화면 상단에 `작업 대상 : {작업지시번호} / {시리얼번호} / {라우팅그룹-라우팅공정-페이지공정}`으로 표시된다 |
| [Forms/Common/INavigatingScreen.cs](src/SEOJIN_MEIS.Client/Forms/Common/INavigatingScreen.cs) | "같은 탭에서 다른 화면으로 넘어가겠다"는 요청 계약. 화면이 탭을 직접 조작하지 않고 `NavigateRequested`만 올리면 `MainForm.SwapTabContent`가 내용과 탭 제목을 바꾼다 |
| [Forms/Common/GridColumns.cs](src/SEOJIN_MEIS.Client/Forms/Common/GridColumns.cs) | 그리드 컬럼 구성(순서·헤더 문구·폭·정렬·일자 서식 + 순번 NO 컬럼). 헤더 문구는 리소스에서 오므로 언어 전환에도 그대로 따라간다 |

**목록 컬럼**(작업 지시 정보): NO · 작업지시번호 · 품목번호 · 시리얼번호 · 작업지시 수량 · 수주번호 · 고객사발주번호 · FCID ·
시작 예정일자 · 종료 예정일자 · 작업 시작일자 · 작업 종료일자 · 작업지시 상태 · 비고 (뒤 4개는 가로 스크롤 영역).

**서버 API**(둘 다 **경로는 가정값** — 실제 스펙 확정 후 조정):

| 용도 | 호출 |
| --- | --- |
| 작업지시 조회 | `GET api/v1.0/workorders/search?workOrderNo=&serialNo=` → `List<WorkOrderEntryDto>` (입력된 조건만 쿼리에 붙는다) |
| 공정(라우팅) 목록 | `GET api/v1.0/workorders/{작업지시번호}/routing?serialNo=` → `List<RoutingProcessDto>` |

**동작 규칙 / 가정** — 목업에 명시가 없어 다음과 같이 정했다. 운영 규칙이 다르면 알려주면 맞춘다:

- 조회 조건 두 개 모두 `(*)`로 표시돼 있지만 **둘 중 최소 하나만 입력**하면 조회한다(둘 다 비면 안내 후 중단 — 전체 조회는 POP에서 의미가 없으므로).
- 조건 입력란에서 **Enter를 누르면 조회**된다(바코드 스캐너 입력이 Enter로 끝나는 경우를 고려).
- 목록 행을 **더블클릭하면 [작업]**과 같고, 팝업의 행을 더블클릭하면 **[이동]**과 같다.
- 공정 목록은 팝업을 열기 전에 미리 조회한다 — 공정이 없으면 빈 팝업을 띄우지 않고 "공정 정보가 없습니다" 안내만 한다.
- 팝업에서 페이지 공정을 고르지 않고 [이동]을 누르면 안내만 하고 창은 유지한다.
- POP 본 화면은 메뉴에서 직접 열리지 않는다(진입 화면을 거친다). 작업 대상 없이 열리는 경우도 대비해 `PopScreen.Initialize(context)` 오버로드는 그대로 남겨 뒀다.

**목업 데이터로 진입 흐름 확인** — 서버 API가 아직 없어서, 목업 모드를 켜면 [조회] → [작업] → &lt;작업 진입 확인&gt; → POP까지 화면 설계서 샘플 데이터로 끝까지 돌려볼 수 있다.

| 항목 | 내용 |
| --- | --- |
| 켜고 끄기 | `App.config`의 **`UseMockData`** (현재 `true`. 서버가 붙으면 `false`로 되돌리면 원래의 REST 호출 경로로 돌아간다) |
| 샘플 데이터 | [Mock/MockWorkOrderData.cs](src/SEOJIN_MEIS.Client/Mock/MockWorkOrderData.cs) — 작업 지시 정보 8건 (`WO_260701-01` ~ `WO_260705-01`, 시리얼 `SN-260701-0001` ~, 상태 진행중/대기/완료/보류) + 각 지시의 공정 목록 |
| 조회 동작 | 작업지시번호 / 시리얼번호를 **부분 일치(대소문자 무시)** 로 거른다. 둘 다 입력하면 AND. 예: `WO_2607` → 8건, `WO_260701` → 2건, `0703` → 2건 |
| 공정(라우팅) | 품목 계열로 그룹이 갈린다 — `ITM-A/B` → `MAIN-01`/`MAIN-02` 5공정, `ITM-C/D` → `SUB-01`/`SUB-02` 4공정. 라우팅 공정 `OP10`~, 페이지 공정 `0010`~ |
| 공정 진행 상태 | 작업지시 상태를 따라간다 — 완료 지시는 전 공정 **완료**, 진행중 지시는 앞 2공정 완료 + 3번째 **진행중** + 나머지 대기, 대기/보류 지시는 전 공정 **대기** |
| 적용 범위 | 진입 화면의 [조회]와 [작업](공정 목록) + POP 본 화면의 공정 정보/탭 4개. 설비 Modbus 통신은 목업 모드와 무관하게 늘 실제 설비를 탄다 |

## POP 본 화면 — 진행 중 공정 정보 + 하단 4개 탭

진입 화면에서 [이동]으로 고른 **페이지 공정 한 건**을 실제로 작업하는 화면이다.

```
┌ 상단 : [이전 공정][다음 공정][공정 생략][공정 중지][공정 시작][공정 목록][관리자 알람]   작업자 · 현재시각
├ 좌측 : 작업 표준 / 도면 PDF (SMB 공유)   ├ 우측 : 진행 중 공정 정보 (11개 필드)
│                                          │        워크스테이션 띠(주황) / 공정 진행 상태 띠(보라)
└ 하단 : [투입자재] [BOM 스팩] [검사 스팩] [연결 장비]        (BOM 스팩 탭에만 ☐수동 렌치) [결과 저장]
```

| 파일 | 역할 |
| --- | --- |
| [Forms/Pop/PopScreen.cs](src/SEOJIN_MEIS.Client/Forms/Pop/PopScreen.cs) | 본 화면 전체(상단 버튼 · 공정 정보 · 탭 4개 · 설비 이벤트 수신) |
| [Forms/Pop/MaterialSerialForm.cs](src/SEOJIN_MEIS.Client/Forms/Pop/MaterialSerialForm.cs) | &lt;투입자재 정보 입력&gt; 팝업. 결과는 `SerialNumbers`로 받는다 |
| [Forms/Pop/TorqueInputForm.cs](src/SEOJIN_MEIS.Client/Forms/Pop/TorqueInputForm.cs) | &lt;토크 결과값 입력&gt; 팝업(수동 렌치). 결과는 `Value`로 받는다 |
| [Models/PopWorkDto.cs](src/SEOJIN_MEIS.Client.Common/Models/PopWorkDto.cs) | 공정 정보 / 투입자재 / BOM 스팩 / 검사 스팩 / 연결 장비 DTO + 조회 키(`PopProcessKey`) |
| [IPopApiClient.cs](src/SEOJIN_MEIS.Client.ApiClient/IPopApiClient.cs) | POP 전용 REST 클라이언트(조회 5종 + 저장 3종 + 상태 전이 + 관리자 알람) |
| [Mock/MockPopData.cs](src/SEOJIN_MEIS.Client/Mock/MockPopData.cs) | 화면 설계서 샘플 데이터(목업 모드용) |
| [Storage/DocumentShareClient.cs](src/SEOJIN_MEIS.Client.Common/Storage/DocumentShareClient.cs) | SMB 공유 폴더에서 도면 PDF를 읽어 오는 클라이언트 |
| [Storage/SmbConnection.cs](src/SEOJIN_MEIS.Client.Common/Storage/SmbConnection.cs) | 공유 폴더 자격증명 연결(mpr.dll WNetAddConnection2) |

**탭별 동작**

| 탭 | 표시 | 입력 |
| --- | --- | --- |
| 투입자재 | 투입자재 코드 · 수량 · 단위 · 위치 · 시리얼번호 검사여부 · 입력상태 | **"시리얼번호 입력상태" 셀 클릭** → &lt;투입자재 정보 입력&gt; 팝업. 투입수량만큼 줄이 만들어지고 Enter로 다음 줄로 내려간다(바코드 스캐너). 검사 대상이 아닌 자재는 안내만 하고 열지 않는다 |
| BOM 스팩 | 기준정보의 토크 스펙(하한/상한/단위) + 토크 결과값 | 연결 장비에서 값이 올라오면 **아직 비어 있는 첫 줄**에 자동으로 채운다. **☐수동 렌치**를 켜면 "결과값 입력" 컬럼이 나타나고 [입력] → &lt;토크 결과값 입력&gt; 팝업으로 직접 적는다(이때는 설비 값을 자동 반영하지 않는다) |
| 검사 스팩 | 기준정보의 검사 항목(스팩명 · 비고 · 검사방법) | 그리드에서 직접 입력 — 결과상태는 합격/불합격 콤보, 결과값은 **정량 검사 줄에서만** 입력 가능 |
| 연결 장비 | 워크스테이션에 물린 장비 기준정보(코드 · 명 · 유형 · 가동범위) | 없음. **통신상태/장비상태는 Modbus 연결 상태로 실시간 갱신**된다(연결 성공=초록, 그 외=빨강) |

**상단 버튼** — 이전/다음/생략/중지/시작은 확인 창을 거쳐 서버에 **상태 전이만 요청**하고, 응답으로 받은 공정 정보로 화면을 다시 그린다(전이 가능 여부는 서버가 판단).
[공정 목록]은 화면에 보이는 작업지시번호를 키로 라우팅을 조회해 진입 화면과 **같은 &lt;작업 진입 확인&gt; 팝업**(`WorkEntryConfirmForm`)을 그대로 띄우고, [이동]으로 고른 공정으로 4개 탭을 다시 읽는다.

**서버 API**(경로는 모두 **가정값** — 실제 스펙 확정 후 조정):

| 용도 | 호출 |
| --- | --- |
| 진행 중 공정 정보 | `GET api/v1.0/pop/process?workOrderNo=&serialNo=&routingGroup=&routingOperation=&pageOperation=` |
| 탭 1/2/3 조회 | `GET api/v1.0/pop/{materials \| bom-specs \| inspection-specs}` (조건 동일) |
| 탭 4 조회 | `GET api/v1.0/pop/equipments?workstation=` |
| 시리얼번호 저장 | `POST api/v1.0/pop/materials/serials` |
| 결과 저장 | `POST api/v1.0/pop/{bom-specs \| inspection-specs}/results` |
| 공정 상태 전이 | `POST api/v1.0/pop/process/action` (Start/Stop/Skip/Previous/Next) |
| 관리자 알람 | `POST api/v1.0/pop/admin-alarm` |

**동작 규칙 / 가정** — 목업에 명시가 없어 다음과 같이 정했다. 운영 규칙이 다르면 알려주면 맞춘다:

- **합/불 판정은 하지 않는다.** 토크 하한/상한과 검사 결과는 화면에 그대로 보여만 주고 판정은 서버 몫이다(값이 없는 숫자 칸은 `-`로 표기).
- 설비에서 올라온 프레임의 **첫 레지스터를 1/100 스케일의 토크 값**으로 본다 — 레지스터 맵이 확정되면 `App.config`의 `Equipments[].RegisterMap`으로 지정할 것(아래 TODO 2).
- 연결 장비 탭의 **장비코드(`EquipmentCode`)가 `App.config`의 `EquipmentId`와 같아야** 실시간 연결 상태가 붙는다. 두 Connection(표준/이벤트) 중 하나라도 연결돼 있으면 "연결 성공"으로 본다.
- 목업에 없던 **[결과 저장]** 버튼을 BOM 스팩/검사 스팩 탭에 두었다(입력한 결과를 서버로 올릴 통로가 필요해서).
- 좌측 **작업 표준/도면**은 SMB 공유 폴더의 PDF를 띄운다(바로 아래 [도면 PDF](#작업-표준--도면-pdf--smb-공유-폴더) 참고). 도면이 없거나 읽지 못하면 대각선 자리표시자에 사유만 적고 화면의 나머지 기능은 그대로 쓸 수 있다.
- 하단 탭은 DevExpress **XtraTab 없이** SimpleButton 4개 + 그리드 Visible 토글로 만들었다(이 PC에 Tab &amp; Navigation 모듈 미설치 — 위 [빌드 확인 상태](#빌드-확인-상태-2026-08-24-기준-이-개발-pc) 참고).
- 이전 버전(설비 제어/작업지시 목록/로그 콘솔)의 POP 화면은 [Forms/Pop/_backup/](src/SEOJIN_MEIS.Client/Forms/Pop/_backup) 에 `*.bak`으로 남겨 두었다(빌드 대상 아님).

### 작업 표준 / 도면 PDF — SMB 공유 폴더

POP 화면 좌측에는 **SMB 공유 폴더에 보관된 작업 표준/도면 PDF**를 띄운다.

```
서버(진행 중 공정 정보 API)  →  DrawingFilePath  ("2026\ITEM_01\OP_10.pdf")
                                      +
App.config DocumentShare.RootPath     ("\\fileserver\drawings")
                                      ↓
                        \\fileserver\drawings\2026\ITEM_01\OP_10.pdf
                                      ↓
        SmbConnection(자격증명 연결)  →  파일 전체를 메모리로 읽기  →  PdfViewer.LoadDocument(stream)
```

| 항목 | 내용 |
| --- | --- |
| 파일 경로 | **서버가 내려준다** — `PopProcessInfoDto.DrawingFilePath`. 공유 폴더 루트 기준 상대 경로가 기본이고, 전체 UNC 경로(`\\서버\공유\...`)를 내려줘도 그대로 쓴다. 비어 있으면 "표시할 도면이 없습니다" 안내만 나온다 |
| 공유 폴더 설정 | `App.config`의 **`DocumentShare`**(JSON) — `RootPath` / `UserName` / `Password` / `Domain`. `RootPath`가 비어 있으면 파일 접근을 아예 시도하지 않는다 |
| 인증 | **두 가지를 모두 지원한다.** `UserName`이 비어 있으면 현재 로그인한 윈도우 계정으로 UNC 경로에 그대로 접근하고(권장), 값이 있으면 그 계정으로 공유에 연결한다(`mpr.dll`의 `WNetAddConnection2` — 드라이브 문자를 쓰지 않는 무명 연결) |
| 뷰어 | DevExpress `PdfViewer`(`DevExpress.XtraPdfViewer.v26.1`). 폭 맞춤(FitToWidth) + 탐색 창/아이콘 띠 숨김 — 작업장에서 도면만 크게 보이고 터치로 잘못 눌릴 여지를 줄인다 |
| 읽는 방식 | 파일을 **메모리로만** 올린다(로컬에 사본을 남기지 않고, 뷰어가 네트워크 파일을 잡고 있어 공유 쪽 파일 교체를 막는 일도 없다). 최대 64MB |
| 스레드 | UNC 파일 열기는 서버가 응답하지 않을 때 수 초~수십 초 멈출 수 있어 **전 과정을 백그라운드 스레드**에서 수행한다. 공정을 빠르게 옮기면 이전 읽기는 취소한다 |
| 실패 처리 | 접근 거부·경로 없음·자격증명 오류 등을 사람이 읽을 수 있는 문구로 바꿔(`DocumentShareException`) 도면 자리표시자에 사유와 전체 경로를 적는다. **화면의 나머지 기능은 계속 동작한다** |

**보안 주의** — `DocumentShare.Password`를 채우면 `App.config`에 **평문으로 남는다**.
가능하면 PC를 도메인에 넣고 `UserName`/`Password`를 비운 채(현재 윈도우 계정) 쓰거나, 배포 시 설정 파일의 접근 권한을 제한할 것.

**연결 동작 참고** — 윈도우는 한 서버에 서로 다른 자격증명을 동시에 유지하지 못한다.
이미 다른 계정으로 연결된 서버면(오류 1219) 기존 연결을 끊고 한 번 재시도한다.

## 메뉴 구조 (요구사항 7)

```
MEIS
 ├─ 작업관리 > POP
 ├─ 작업 현황 조회 > 생산 현황조회
 │                > 작업 조회 현황
 └─ 작업통합 개요 > 작업통합 현황 조회
```

POP 노드는 진입 화면(작업지시 조회)을 열고, [이동] 시 같은 탭이 POP 본 화면으로 바뀐다(위 [POP 진입 화면](#pop-진입-화면--작업지시-조회--작업-진입-확인--pop) 참고).
`Menu/MenuCatalog.cs`에 노드만 추가하면 화면이 늘어난다. 좌측 `AccordionControl`은 이 트리를 그대로 렌더링하고,
리프 노드 클릭 시 우측 탭 영역에 화면을 열거나(이미 열려있으면) 포커스만 이동한다.

## 작업 조회 현황 — 조건 조회 → 작업 지시 정보 → 작업 진행 현황

MEIS &gt; 작업 현황 조회 &gt; **작업 조회 현황**. 작업지시를 조건으로 찾고, 고른 지시가 공정·스팩별로 어디까지 갔는지 한 화면에서 본다.

```
[조회 조건]  지시일자(*) [from]~[to]  작업지시번호  생산품목(코드+명+🔍)  생산품목 시리얼번호  작업지시 상태
     ↓ [조회]
[작업 지시 정보]  NO · 작업지시번호 · 품목번호 · 시리얼번호 · 작업지시 수량 · 수주번호 · 고객사발주번호 · FCID ·
                  시작/종료 예정일자 · 작업 시작/종료일자 · 작업지시 상태 · 비고
     ↓ 행 선택(자동)
[작업 진행 현황]  필터: 진행 상태 / 스팩 결과 상태
                  NO · 라우팅 그룹 · 라우팅 공정 · 페이지 공정명 · 스팩명 · 스팩 유형 · 진행 상태 · 스팩 결과 상태
```

| 파일 | 역할 |
| --- | --- |
| [Forms/WorkStatus/WorkStatusScreen.cs](src/SEOJIN_MEIS.Client/Forms/WorkStatus/WorkStatusScreen.cs) | 조회 조건 + 작업 지시 정보 + 작업 진행 현황(필터 포함) |
| [Forms/WorkStatus/ItemPickerForm.cs](src/SEOJIN_MEIS.Client/Forms/WorkStatus/ItemPickerForm.cs) | &lt;생산품목 선택&gt; 팝업. 결과는 `SelectedItem`으로 받는다 |
| [Models/WorkStatusDto.cs](src/SEOJIN_MEIS.Client.Common/Models/WorkStatusDto.cs) | 조회 조건(`WorkStatusQuery`) · 진행 현황(`WorkProgressDto`) · 품목(`ItemDto`) |
| [IWorkStatusApiClient.cs](src/SEOJIN_MEIS.Client.ApiClient/IWorkStatusApiClient.cs) | 조건 조회 + 진행 현황 + 품목 검색 |
| [Mock/MockWorkStatusData.cs](src/SEOJIN_MEIS.Client/Mock/MockWorkStatusData.cs) | 화면 설계서 샘플 데이터(목업 모드용) |

**서버 API**(경로는 모두 **가정값**):

| 용도 | 호출 |
| --- | --- |
| 작업 지시 정보 | `GET api/v1.0/work-status/workorders?orderDateFrom=&orderDateTo=&workOrderNo=&itemCode=&serialNo=&statusName=` |
| 작업 진행 현황 | `GET api/v1.0/work-status/workorders/{작업지시번호}/progress?serialNo=` |
| 품목 검색 | `GET api/v1.0/items/search?keyword=` |

**동작 규칙 / 가정** — 목업에 명시가 없어 다음과 같이 정했다:

- **지시일자만 필수**다(목업의 `(*)` 표기). 기본값은 최근 한 달이며, 시작일이 종료일보다 늦으면 안내 후 중단한다. 나머지 조건은 비우면 걸리지 않는다.
- 작업 지시 정보에서 **행을 고르면 아래 진행 현황이 자동으로 바뀐다**(별도 버튼 없음). 조회 직후에는 첫 행이 선택된다.
- [진행 상태] / [스팩 결과 상태] 두 필터는 **이미 받아 둔 목록을 화면에서 좁히는 것**이라 서버를 다시 부르지 않는다. 첫 항목 "전체"는 조건 없음이다.
- 생산품목은 코드를 직접 치거나 **돋보기 버튼**으로 고른다. 코드를 손으로 고치면 옆의 품목명은 비운다(더 이상 맞지 않으므로).
- 조건 입력란에서 **Enter를 누르면 조회**된다(바코드 스캐너 입력 고려).
- 진행 현황의 스팩 결과 상태가 아직 없으면 빈칸 대신 `-`로 표기한다(POP 화면과 같은 규칙).

## 관리자 알람 — 유형 선택 팝업

POP 본 화면의 **[관리자 알람]** 버튼은 &lt;관리자 알람 유형 선택&gt; 팝업([Forms/Pop/AdminAlarmForm.cs](src/SEOJIN_MEIS.Client/Forms/Pop/AdminAlarmForm.cs))을 연다.

```
· 알람 유형  [자재 결품 ▾]
┌ 알람 상세 정보 ───────────────────────────
│ · 결품 자재 코드  [M-00001-02223 ▾]   ← 유형에 따라 라벨·목록이 바뀌고, 필요 없으면 줄 자체가 사라진다
│ · 비고           [                  ]
└──────────────────────────────  [닫기] [전송]
```

알람 유형은 **기준정보**(`GET api/v1.0/pop/admin-alarm/types`)에서 받아오며, 유형마다 함께 보낼 대상이 다르다:

| 알람 유형 | 대상(`TargetKind`) | 상세 입력 |
| --- | --- | --- |
| 자재 결품 | `Material` | 결품 자재 코드 — **지금 화면의 [투입자재] 목록**에서 고른다 |
| 설비 이상 | `Equipment` | 이상 장비 코드 — **지금 화면의 [연결 장비] 목록**에서 고른다 |
| 품질 이상 / 기타 | `None` | 비고만 |

- 대상 코드를 손으로 치지 않고 **현재 공정에 실제로 물려 있는 목록에서 고르게** 해 오타를 없앤다.
- 대상이 없는 유형을 고르면 대상 줄이 사라지고 비고 입력란이 그만큼 커진다.
- [전송] 시 유형·대상·비고에 **현재 공정 키와 작업자 아이디**를 붙여 `POST api/v1.0/pop/admin-alarm`으로 보낸다. 누구에게 어떻게 통지할지는 서버가 정한다.
- 유형을 안 골랐거나, 대상이 필요한 유형인데 대상이 비면 안내만 하고 창은 유지한다.

## 작업통합 현황 조회 — 생산품목 한 개체의 전 이력 트리

MEIS &gt; 작업통합 개요 &gt; **작업통합 현황 조회**. "이 시리얼번호 하나가 어떻게 만들어졌는가"를 계층 트리로 펼쳐 본다.

```
[조회] [엑셀 내보내기]   · 생산품목 시리얼번호(*) [SER-01]

「작업 통합 개요」
 항목 코드            항목 설명       토크 결과값 토크 목푯값 토크 하한값 토크 최댓값 앵글 목푯값 앵글 하한값 앵글 상한값 앵글 결과값
 A001                 생산품목 A001        ..        ..        ..        ..        ..        ..        ..        ..
  └ ROUTING_GROUP_01  메인 라우팅 그룹     ..        ..        ..        ..        ..        ..        ..        ..
     └ OP10           조립 공정            ..        ..        ..        ..        ..        ..        ..        ..
        ├ 0010 ~ 0080 페이지 공정          ..        ..        ..        ..        ..        ..        ..        ..
        └ 0080 > M001 > TORQUE_01       6.180     6.000     5.400     6.600    90.000    85.000    95.000    91.200
```

| 파일 | 역할 |
| --- | --- |
| [WorkIntegratedStatusScreen.cs](src/SEOJIN_MEIS.Client/Forms/WorkIntegratedStatus/WorkIntegratedStatusScreen.cs) | 조회 조건 + 작업 통합 개요 트리 + 엑셀 내보내기 |
| [Models/WorkIntegratedDto.cs](src/SEOJIN_MEIS.Client.Common/Models/WorkIntegratedDto.cs) | 트리 한 줄(`WorkIntegratedNodeDto`) + 줄 종류 상수(`WorkIntegratedNodeKind`) |
| [Mock/MockWorkIntegratedData.cs](src/SEOJIN_MEIS.Client/Mock/MockWorkIntegratedData.cs) | 화면 설계서 샘플 계층(목업 모드용) |

**서버 API**(경로는 **가정값**): `GET api/v1.0/production/work-integrated-overview?serialNo=`

**동작 규칙 / 가정** — 목업에 명시가 없어 다음과 같이 정했다:

- **계층은 평평한 목록으로 받는다.** 중첩 객체 대신 `NodeId`/`ParentNodeId` 부모-자식 관계로 내려주면 DevExpress `TreeList`가 그대로 트리를 만든다(루트는 `ParentNodeId = 0`). 계층이 깊어지거나 단계가 늘어도 클라이언트 수정 없이 받는다.
- **값 컬럼은 스팩 줄에서만** 채워진다. 상위 줄(품목/라우팅/공정/자재)은 계층을 잡아 주는 역할이며, 값이 없는 칸은 목업과 같이 `..`로 표기한다.
- **시리얼번호는 필수**다 — 한 개체의 이력을 보는 화면이라 대상 없이 전체를 긁어오면 의미가 없다. Enter로도 조회된다.
- 조회 결과는 **전부 펼친 상태**로 보여 준다(`ExpandAll`). 목업처럼 한눈에 전 과정이 보이는 것이 이 화면의 목적이다.
- **[엑셀 내보내기]** 는 화면에 보이는 트리를 그대로 xlsx로 저장한다(계층·컬럼 구성 유지, `TreeList.ExportToXlsx`). 저장 위치는 사용자가 고르고, 기본 파일명에 시리얼번호와 시각을 넣어 덮어쓰기를 피한다. 조회 결과가 없으면 안내만 한다.
- 값 컬럼은 **고정 폭 + 가로 스크롤**로 뒀다 — 앵글 컬럼까지 억지로 줄여 넣지 않고 화면 설계서처럼 옆으로 밀어 본다.
- 이전 버전(부서/라인 집계 그리드)의 화면은 [Forms/WorkIntegratedStatus/_backup/](src/SEOJIN_MEIS.Client/Forms/WorkIntegratedStatus/_backup) 에 `*.bak`으로 남겨 두었다(빌드 대상 아님).

## 다국어 (요구사항 8)

- 위성 리소스 3벌: `Localization/Strings.resx`(중립/기본=한국어), `Strings.en.resx`(영어), `Strings.vi.resx`(베트남어).
- `Localization/Strings.cs`가 강타입 접근자 역할(`Strings.Login_Title` 등). VS의 resx 디자이너 코드생성 없이 수동 작성했다.
- 시작 언어는 `App.config`의 `DefaultCulture`(기본 `ko-KR`)로 결정되며 `Program.cs`에서 `CurrentUICulture`에 반영한다.
- 실행 중에는 메인 화면 우측 상단의 **언어 콤보(한국어 / English / Việt Nam)**로 바꿀 수 있다.
  국기 아이콘(`Images/flag/{ko,en,vi}.png`)이 선택 항목과 펼친 목록 양쪽에 함께 표시된다(`ImageComboBoxEdit`).
  문화권 변경 후 셸 문구·메뉴를 다시 만들고 열려 있던 탭을 다시 열어 적용한다
  (아래 [상단 바 우측 정보 블록](#상단-바-우측-정보-블록) 참고).

문자열 키를 추가할 때 함께 갱신해야 하는 파일과, 언어를 새로 추가할 때 필요한 것
→ **[src/SEOJIN_MEIS.Client/CLAUDE.md](src/SEOJIN_MEIS.Client/CLAUDE.md)** (7번 항목)

## UI 테마 — DevExpress 스킨/팔레트 + 컬러 셸 (Theme/)

웹 MES 시안(남보라 사이드바 + 밝은 본문 + 포인트 컬러)에 맞춰 셸과 화면 전체를 컬러 테마로 구성했다.
색 값은 화면 코드에 흩어놓지 않고 **[Theme/AppTheme.cs](src/SEOJIN_MEIS.Client/Theme/AppTheme.cs) 한 곳**에만 둔다 —
톤을 바꾸려면 이 파일의 색 상수만 고치면 모든 화면에 반영된다.
아이콘은 코드로 만드는 SVG(`Theme/AppIcons.cs`)와 파일로 들어온 이미지(`Theme/AppImages.cs`) 두 갈래이며,
후자는 `Images/` 원본을 어셈블리에 포함해서 배포한다.

화면 코드에서 색·아이콘·이미지를 다룰 때 지켜야 할 규칙
→ **[src/SEOJIN_MEIS.Client/CLAUDE.md](src/SEOJIN_MEIS.Client/CLAUDE.md)** (4번, 8번 항목)

DevExpress 기능을 두 층으로 쓴다:

1. **스킨 + SVG 팔레트**(벡터 스킨 기능) — `Program.cs`에서 시작 시 한 번 `AppTheme.ApplySkin(false)` 호출
   ```csharp
   UserLookAndFeel.Default.SetSkinStyle(dark
       ? SkinSvgPalette.DefaultSkin.VioletDark      // 다크 팔레트
       : SkinSvgPalette.DefaultSkin.VioletLight);   // 라이트 팔레트(기본)
   ```
   에디터/그리드/스크롤바/드롭다운 등 기본 컨트롤 톤이 팔레트 하나로 남보라 계열로 맞춰진다.
   다른 팔레트를 쓰고 싶으면 `SkinSvgPalette.Bezier.*`(Tokyo, Nebula, BlackberryShake 등), `SkinSvgPalette.WXI.*`(Darkness, Calmness 등)로 바꾸면 된다.
   **화면에서 테마를 전환하는 버튼은 두지 않는다** — 다크로 쓰고 싶으면 `Program.cs`의 `AppTheme.ApplySkin(true)` 한 곳만 바꾸면 되고,
   `AppTheme`의 색 속성과 각 화면의 `ApplyTheme()`(`IThemedScreen`)은 그때도 그대로 동작한다.
2. **Appearance 재정의** — 사이드바/상단바/카드/그리드 헤더처럼 시안 색을 정확히 맞춰야 하는 부분은
   각 컨트롤의 `Appearance`(BackColor/BackColor2/GradientMode/ForeColor/Font)로 직접 지정한다.

### 화면 구성 (MainForm)

| 영역 | 내용 |
| --- | --- |
| 좌측 사이드바 | 남보라 수직 그라데이션(#2B2676 → #3E2E98) + 상단 로고 블록(제품명/부제) + `AccordionControl` 3-Depth 메뉴. 깊이별로 들여쓰기·행 높이·아이콘·배경이 달라 계층이 드러난다(아래 [좌측 트리 메뉴 계층 표현](#좌측-트리-메뉴-계층-표현)), 선택 항목은 보라색(#6C63E0) 밴드로 강조 |
| 상단 바 | (좌) 햄버거 · 브레드크럼(`홈 / MEIS / 대분류 / 화면`) / (우) 알림 종+빨간 배지(안 읽은 알림 수, 1분 폴링 — 누르면 [알림 팝업](#알림--1분-폴링-배지--알림-팝업목록상세)) · **`{이름} 님 환영합니다.` · 세션 남은 시간 · 언어 콤보 · 사용자 아이콘 · 로그아웃 아이콘** |
| 탭 스트립 | 열린 화면을 브라우저/IDE 형태의 탭으로 표시 — **윗 모서리만 둥근 사각형**이 스트립 하단 경계선 위에 앉는다. 활성 탭 = 인디고 배경 + 흰 글자, 비활성 = 연회색 + 테두리(마우스 올리면 한 톤 진해짐), 탭마다 ✕ 버튼([TabChip.cs](src/SEOJIN_MEIS.Client/Forms/Common/TabChip.cs)) |
| 본문 | 밝은 회청색 바닥(#F4F6FB) 위에 흰 카드. 열린 화면이 없으면 접속 서버/등록 설비 수/로그인 사용자를 색 타일로 보여주는 환영 카드 표시 |

- **사이드바 접기**: 햄버거 버튼 → 사이드바 폭 240 ↔ 64, `AccordionControl.OptionsMinimizing.State`로 아이콘만 남는 최소화 모드 사용.
- **알림 배지**: 서버에 안 읽은 알림이 있으면 종 아이콘에 빨간 배지(개수)가 붙는다. 1분마다 갱신되고, 클릭하면 알림 팝업이 열린다.
- **탭**: 예전에는 `Panel` + `Label` 2개에 `Region`으로 알약 모양을 오려 썼는데, `Region`은 안티에일리어싱이 없어 곡선이 계단처럼 보이고 닫기(✕) 글자까지 잘려 모양이 어색했다.
  지금은 [TabChip.cs](src/SEOJIN_MEIS.Client/Forms/Common/TabChip.cs) 컨트롤 하나가 배경·글자·✕를 직접(안티에일리어싱으로) 그린다 — 마우스 위치로 탭 본체와 ✕ 영역을 구분해 각각 hover 표시를 준다. 색은 전부 `AppTheme`에서 가져오므로 포인트 컬러를 바꾸면 탭도 따라온다.
- **아이콘**: [Theme/AppIcons.cs](src/SEOJIN_MEIS.Client/Theme/AppIcons.cs)에 SVG 문자열로 보관하고 `SvgImage.FromStream`으로 만든다 —
  이미지 파일을 프로젝트에 추가하지 않아도 되고, 같은 아이콘을 색만 바꿔 재사용하며(캐시), 고DPI에서도 깨지지 않는다.

### 좌측 트리 메뉴 계층 표현

메뉴 구조(MEIS > 대분류 > 화면)는 그대로 두고, **어느 깊이인지 한눈에 보이도록** 표현만 정리했다.
깊이별 규칙은 [AppTheme.MenuElement](src/SEOJIN_MEIS.Client/Theme/AppTheme.cs)와 `MainForm.BuildAccordionElement` 두 곳에만 있다.

| 깊이 | 들여쓰기 / 높이 | 아이콘 | 글꼴·색 | 배경 |
| --- | --- | --- | --- | --- |
| 0 · 루트(MEIS) | 4px / 44px | 분류 아이콘 18px, 흰색 | 굵게, 흰색 | 사이드바 그라데이션 그대로 |
| 1 · 대분류 | 18px / 40px | 분류 아이콘 16px(클립보드/차트/레이어) | 굵게, 밝은 라벤더 | 그대로 |
| 2 · 화면(리프) | 36px / 34px | **트리 가지(└) + 화면별 색 점** | 보통, 옅은 라벤더 | 살짝 어두운 밴드(`#221D58`) |

- **트리 가지 아이콘**: [AppIcons.TreeLeaf](src/SEOJIN_MEIS.Client/Theme/AppIcons.cs)가 SVG로 만든다. 세로 안내선이 행 높이(34px)를 가득 채우므로
  같은 분류 아래 화면이 여러 개면 선이 이어져 계층이 보이고, 마지막 항목은 선을 중간까지만 그려 `└` 모양이 된다
  (마지막 여부는 `BuildAccordionElement`가 형제 순번으로 판단한다).
- **화면별 색 점**: 점 색은 `MenuNode.Accent`로 정한다 — POP=Emerald, 생산 현황조회=Cyan, 작업통합 현황 조회=Amber.
  메뉴에서 색으로도 화면을 구분할 수 있고, 참고 이미지처럼 상태 색(예: 이상 발생 시 Rose)으로 바꿀 여지도 남겨 뒀다.
- **선택 항목**은 기존과 같이 보라색 밴드(`#6C63E0`) + 흰 굵은 글씨다(`AppTheme.MenuElementSelected`).
- 펼침/접힘 화살표는 `AccordionControl`이 그리는 기본 표시를 그대로 쓴다(`ShowGroupExpandButtons = true`).
- 루트 분류 사이 간격은 `DistanceBetweenRootGroups = 6`으로 벌려 블록 구분이 더 잘 보이게 했다.

### 상단 바 우측 정보 블록

요청받은 형식(`{이름} 님 환영합니다.  04:59:49  [Korean ▼]  [사용자]`)에 맞춰 우측 정보 묶음을 구성했다.
배치는 [MainForm.cs](src/SEOJIN_MEIS.Client/Forms/MainForm.cs)의 `LayoutTopBar()`가 오른쪽 정렬로 계산한다(창 폭이 바뀌어도 유지).

| 요소 | 내용 |
| --- | --- |
| `{이름} 님 환영합니다.` | 프로필을 받았으면 이름, 아직이면 로그인 아이디를 넣는다(`MainForm_Welcome` 리소스, 영어는 `Welcome, {0}.`) |
| 세션 남은 시간 | `HH:mm:ss` 카운트다운. 시작 값은 `App.config`의 **`SessionTimeoutMinutes`**(기본 300분 = `05:00:00`), 1초 간격으로 갱신 |
| 언어 콤보 | `Korean` / `English`. 고르면 즉시 화면 언어가 바뀐다 |
| 사용자 아이콘 | 클릭 → [사용자 정보 팝업](#사용자-정보-팝업) |
| 로그아웃 아이콘 | 클릭 → 공통 확인 창으로 한 번 물은 뒤 로그아웃 |

- **세션 카운트다운**: 로그인 시점부터의 **고정 세션 길이**이며 조작으로 연장되지 않는다. 0이 되면 타이머를 멈추고
  공통 메시지 창으로 안내한 뒤 토큰을 지우고 창을 닫는다(재로그인). 조작할 때마다 연장되는 "유휴 시간 기준"으로 바꾸려면
  마우스/키 이벤트에서 `ResetSessionCountdown()`을 호출하면 된다. 시각을 표시하는 시계로 바꾸려면 `RefreshSessionTime()` 한 곳만 고치면 된다.
- **언어 전환**: `CurrentUICulture`를 바꾼 뒤 셸 문구·메뉴 트리를 다시 만들고, 열려 있던 탭을 같은 순서로 다시 열어 화면 문구까지 새 언어로 적용한다
  (`Strings`는 호출 시점의 문화권을 보기 때문에, 이미 만들어진 컨트롤은 다시 만들어야 문구가 바뀐다).
  시작 언어는 여전히 `App.config`의 `DefaultCulture`가 정한다.
- 화면에 하드코딩되어 있던 한글 문구(설비 통신 로그 / 조회 조건 / 부서·라인 집계 / 환영 카드 문구·타일 캡션 / "새 알림이 없습니다.")도
  이번에 전부 리소스로 옮겨서, 언어를 바꾸면 남는 한글이 없다.
- 시안은 남색 헤더 바탕이지만 이 앱은 흰 상단바 + 남보라 사이드바 톤을 쓰고 있어 **배치/문구 형식만 맞췄다**.
  헤더도 남색으로 칠하려면 `AppTheme.TopBarPanel`에서 배경색만 바꾸면 된다.

### 사용자 정보 팝업

상단바의 **아바타 또는 아이디를 클릭**하면 [Forms/UserProfileForm.cs](src/SEOJIN_MEIS.Client/Forms/UserProfileForm.cs)가 모달로 열린다.
표시 항목은 요구된 5개 — **사번 · 이름 · 연락처 · 부서 · 워크스테이션**.

값의 출처가 두 가지다:

| 항목 | 출처 |
| --- | --- |
| 워크스테이션 | 로그인 화면에서 선택한 값(`UserSession.Workstation`) — 서버 호출 없이 바로 표시 |
| 사번 / 이름 / 연락처 / 부서 | 팝업이 열릴 때 `GET api/v1.0/users/me`(`IUserProfileApiClient`)로 조회 → `UserSession.ApplyProfile()`로 세션에 반영 |

- 조회 중에는 "조회 중...", 실패하면 붉은 안내 문구를 남기고 값은 `-`로 표시한다. **서버/API가 아직 없어도 팝업 자체는 정상 동작한다**(예외 대화상자 없음).
- 팝업을 닫으면 상단바 표시가 `이름 (아이디)`로, 아바타 첫 글자도 이름 기준으로 갱신된다.
- 팝업 상단 헤더는 사이드바와 같은 남보라 그라데이션 + 원형 아바타를 쓴다(`AppTheme.Sidebar`/`AppTheme.Avatar` 재사용).
- **TODO**: `users/me` 경로와 응답 필드명(`employeeNo`/`userName`/`phoneNumber`/`departmentName`)은 가정값이다.
  실제 서버 스펙 확정 후 [UserProfileDto](src/SEOJIN_MEIS.Client.Common/Models/UserProfileDto.cs) 속성명(또는 `JsonProperty` 매핑)과 엔드포인트를 맞출 것.

### 공통 메시지/확인 창 (MessageBox 대체)

**공통 유저컨트롤**로 메시지/확인 창을 만들고, 화면에서 쓰던 `MessageBox`/`XtraMessageBox` 호출을 전부 이 창으로 바꿨다.
형태는 [사용자 정보 팝업](#사용자-정보-팝업)과 같은 구성이다 — **상단 색 띠(성격별 아이콘 + 제목) + 옅은 회청색 본문 + 우측 하단 버튼**,
그리고 창 테두리가 있는 고정 크기 대화상자.
(처음엔 테두리 없는 흰 카드였는데 흰 화면 위에서 창 경계가 잘 보이지 않아 이 형태로 바꿨다.)

| 파일 | 역할 |
| --- | --- |
| [Forms/Common/AppMessagePanel.cs](src/SEOJIN_MEIS.Client/Forms/Common/AppMessagePanel.cs) | **공통 유저컨트롤**(XtraUserControl). 창에 대한 의존이 없어 모달 대화상자로도, 화면 안쪽 인라인 확인 영역으로도 쓸 수 있다. 결과는 `ResultSelected` 이벤트로 알린다. |
| [Forms/Common/AppMessageForm.cs](src/SEOJIN_MEIS.Client/Forms/Common/AppMessageForm.cs) | 위 유저컨트롤을 감싸 모달로 띄우는 호스트 폼. 사용자 정보 팝업과 같은 고정 크기 대화상자(`FixedDialog`)이고, 창 제목은 헤더와 같은 문구를 쓴다. |
| [Forms/Common/AppMessageBox.cs](src/SEOJIN_MEIS.Client/Forms/Common/AppMessageBox.cs) | 화면이 호출하는 유일한 진입점(정적 메서드). |
| [Forms/Common/AppMessageOptions.cs](src/SEOJIN_MEIS.Client/Forms/Common/AppMessageOptions.cs) | `AppMessageKind`(Info/Question/Success/Warning/Error), `AppMessageButtons`(Ok/OkCancel). |

```csharp
AppMessageBox.Warn(this, ex.Message, Strings.Pop_Title);        // 확인 1개
AppMessageBox.Info(this, message, Strings.Common_Notice);
if (!AppMessageBox.Confirm(this, "이동하시겠습니까?")) return;    // 확인/취소 → 확인이면 true
```

- 강조 색은 성격에 따라 바뀌고, **헤더 색 띠 · 헤더 아이콘 · [확인] 버튼**이 같은 색을 쓴다:
  Info/Question=Teal `#009EB3`, Success=Emerald, Warning=Amber, Error=Rose.
  아이콘도 성격별로 다르다(i / ? / 체크 / 삼각형+! / 원+X).
- 제목을 넘기지 않으면 성격별 기본 제목(안내/확인/완료/주의/오류, 영어는 Information/Confirm/Done/Warning/Error)이 헤더와 창 제목에 들어간다 —
  제목 없이 흰 본문만 있으면 창 경계를 알아보기 어려워서 헤더는 항상 표시한다.
- 메시지 줄 수에 맞춰 창 높이가 자동으로 늘어난다(`AppMessagePanel.MeasureHeight`). 폭은 420px 고정.
- 키보드: **Enter=확인, Esc=취소/닫기**. 기본 포커스는 [확인]에 둔다.
- 헤더 안의 X는 `AppMessagePanel.ShowCloseButton`으로 켜고 끈다 — 제목줄이 있는 모달에서는 닫기가 겹치므로 호스트 폼이 끄고,
  화면 안쪽에 인라인으로 올려 쓸 때는 켜두면 닫기 수단이 된다.
- 모든 메시지는 `AppLog`에도 남으므로 사용자가 본 안내를 콘솔 로그로 추적할 수 있다.
- 교체된 호출 지점: POP / 생산 현황조회 / 작업통합 현황 조회의 조회 실패 안내(`Warn`), 상단바 알림 목록(`Info`).
  여기에 **로그아웃 확인**(`Confirm`)을 추가했다 — 되돌릴 수 없는 동작이라 한 번 묻는다.
- 새 화면에서도 `MessageBox`/`XtraMessageBox`를 직접 쓰지 말고 `AppMessageBox`를 쓸 것(톤·문구·로그가 한곳에서 관리된다).

### 화면별 포인트 컬러

화면마다 포인트 컬러를 하나씩 정해 카드 제목 점·그리드 헤더·버튼에 일관되게 쓴다.

| 화면 | 포인트 컬러 | 비고 |
| --- | --- | --- |
| POP 본 화면 | 탭마다 다름 | 공정 시작=Emerald, 공정 중지=Rose, 공정 목록=Violet, 관리자 알람=Amber / 워크스테이션 띠=Amber, 공정 상태 띠=Violet / 그리드 헤더는 탭별로 투입자재=Emerald, BOM 스팩=Indigo, 검사 스팩=Violet, 연결 장비=Cyan / 선택된 하단 탭은 연한 Emerald 칩 |
| 생산 현황조회 | Cyan `#06B6D4` | 조회 조건 카드 + 결과 그리드 카드 |
| 작업 조회 현황 | Cyan `#06B6D4` | 조회 조건 + 작업 지시 정보 그리드는 Cyan, 아래 작업 진행 현황은 Violet으로 구분 |
| 생산품목 선택 팝업 | Cyan `#06B6D4` | 작업 조회 현황의 돋보기에서 열린다 |
| 관리자 알람 팝업 | Amber `#F59E0B` | POP 상단 [관리자 알람] 버튼과 같은 색 |
| 작업통합 현황 조회 | Violet `#7C3AED` | 조회 조건 카드 + 작업 통합 개요 계층 트리(TreeList도 그리드와 같은 톤). [엑셀 내보내기]만 Emerald 외곽선 버튼 |
| 로그인 | Indigo/Violet | 좌측 그라데이션 브랜드 패널 + 우측 흰 카드 |

### 주의 사항 (실측으로 확인한 것들)

DevExpress 컨트롤을 다루다 실제로 밟은 함정들(`PanelControl` 테두리와 배경색 충돌, `AccordionControl`의
투명색 처리, `LabelControl` 자동 크기, 도킹 `Add` 순서 등)은 규칙 문서로 옮겼다
→ **[src/SEOJIN_MEIS.Client/CLAUDE.md](src/SEOJIN_MEIS.Client/CLAUDE.md)** (5번 항목)

### 확인 방법

실제로 앱을 띄워 화면을 캡처해 확인했다(2026-08-24, 이 개발 PC): 로그인 → 환영 카드 → POP 진입 화면(조회 결과 2건) → 작업 진입 확인 팝업(공정 9건) → [이동] 후 POP 본 화면 → 생산 현황조회 → 작업통합 현황 조회 → 사용자 정보 팝업 → 우측 상단 정보 블록(환영/카운트다운/언어) → 언어 English 전환 → 공통 메시지 창(확인/취소, 주의) → 사이드바 접기.
로그인/서버 없이도 확인할 수 있게, 임시 하네스에서 로컬 HttpListener가 응답(작업지시 등은 빈 배열, `users/me`는 샘플 프로필)을 돌려주도록 하고 `MainForm`을 직접 띄워 캡처했다.
원형 아바타/버튼 색처럼 화면 캡처 방식(`DrawToBitmap`)이 제대로 담지 못하는 부분은 실제 렌더링 픽셀을 1점씩 읽어 확인했다 — 아바타 모서리 = 헤더 그라데이션 색(원형 클립 정상), 닫기 버튼 테두리 = `#475569`, 공통 메시지 창의 [확인] 채움 = `#009EB3`, [취소] 테두리 = `#475569`.
`DevExpress.BonusSkins.v26.1` 참조는 `SEOJIN_MEIS.Client.csproj`에 그대로 두었다(확장 스킨용).

## 로깅 (Debug 콘솔 + 로그 파일 + API 호출 URL)

- `SEOJIN_MEIS.Client.Common.Logging.AppLog` : `Step`/`Info`/`Warn`/`Error` — 타임스탬프를 붙여
  **콘솔 · VS 출력 창 · 로그 파일** 세 곳에 동시에 남긴다.
- **로그 파일**은 첫 기록 때 자동으로 열린다(별도 초기화 호출 없음). 위치는 실행 파일 옆 `logs`,
  쓸 수 없으면 `%LOCALAPPDATA%\MEIS\logs`. 시작 로그에 실제 경로가 찍힌다.

  | 정책 | 내용 |
  | --- | --- |
  | 일별 생성 | `logs\meis-client-yyyyMMdd.log` |
  | 7일 압축 | 7일 지난 로그를 7일 단위로 묶어 `logs\archive\meis-client-{시작}_{끝}.zip`으로 옮기고 원본 삭제 |
  | 30일 보관 | 압축본·원본 모두 30일이 지나면 삭제 |

  정리·압축은 파일을 새로 열 때(앱 시작 / 자정 넘김) 한 번씩 돈다. 고칠 때 지켜야 할 것 →
  **[src/SEOJIN_MEIS.Client.Common/CLAUDE.md](src/SEOJIN_MEIS.Client.Common/CLAUDE.md)** (Logging/AppLog 항목)
- `SEOJIN_MEIS.Client.csproj`에서 **Debug 구성만 `OutputType=Exe`**(콘솔 서브시스템)로 빌드하도록 조건부 설정했다 — VS에서 F5(디버그 실행)하면 WinForms 창과 별개로 **검은 콘솔 창이 함께 뜨고**, 거기에 `AppLog`가 쓴 로그가 실시간으로 보인다. Release(배포용)는 그대로 `WinExe`라 콘솔 창이 뜨지 않는다.
- `Program.cs`에 앱 처리 단계(설정 로드 → API 클라이언트 구성 → 설비 매니저 등록 → 로그인 화면 → 로그인 성공/실패 → 메인 화면)를 `AppLog.Step`으로 남겼다.
- `SEOJIN_MEIS.Client.ApiClient.LoggingHandler`(모든 `HttpClient`에 장착됨)가 **모든 REST 호출의 전체 URL**(`request.RequestUri`, BaseAddress까지 합쳐진 절대경로) + Method + 응답 상태코드 + 소요시간(ms)을 자동으로 남긴다. 요청/응답 바디는 비밀번호 등 민감정보가 섞일 수 있어 남기지 않는다.
- 설비 Modbus-TCP 접속 시도/성공/실패, 요청 ADU(16진), 실시간 이벤트 수신, POP 화면의 설비 이벤트 로그도 같은 `AppLog`를 통해 콘솔에 함께 보인다.
- VS의 [출력] 창(디버그)에서도 `Trace.WriteLine` 덕분에 동일한 로그를 볼 수 있다 — 콘솔 창을 닫아도 로그는 계속 그쪽에 남는다.

## ClickOnce 배포 (요구사항 4)

`src/SEOJIN_MEIS.Client/SEOJIN_MEIS.Client.csproj`에 ClickOnce 기본 속성이 들어있고, `Properties/PublishProfiles/ClickOnceProfile.pubxml`이
게시 프로필의 시작점이다. **배포 서버 URL/경로, 서명 인증서, 버전 정책은 환경별로 다르므로**
Visual Studio에서 `SEOJIN_MEIS.Client` 프로젝트 우클릭 → **게시(Publish)** 마법사로 최종 확정할 것을 권장한다.

## 다음에 정해줘야 할 것들 (TODO)

1. **REST API 실제 스펙**: 로그인/작업지시/생산실적/설비상태 엔드포인트 경로·필드명 확정 → `SEOJIN_MEIS.Client.ApiClient`의 각 `*ApiClient` 클래스 조정
2. **설비 Modbus 레지스터 맵**: 제어/상태 레지스터 주소와 값의 의미, 실시간 이벤트 프레임의 payload 정의 → `App.config`의 `Equipments[].RegisterMap`과 `PopScreen`의 상태 매핑 채우기 (필요하면 Advertise/Scan ack/Search ack 자동 탐색도 함께)
3. **화면 필드 상세**: 생산현황조회/작업통합현황조회 화면에 실제로 표시·입력할 컬럼/필드 목록
4. **DevExpress Tab & Navigation, Splash Screen 모듈 설치 여부** → 설치되면 `MainForm.cs` 탭 영역을 `XtraTabControl`로 교체
5. **ClickOnce 배포 서버 정보**(URL, 서명 인증서) 및 회사명/제품명 확정
6. **도면 공유 폴더(SMB) 실제 값**: `App.config`의 `DocumentShare.RootPath`(공유 경로)와, 전용 계정을 쓴다면 계정/비밀번호 → 지금은 비어 있어 도면 영역에 "공유 폴더 미설정" 안내만 나온다
7. **POP 본 화면 남은 결정 사항**
   - **토크 결과값이 실려 오는 레지스터 주소와 스케일** (현재는 첫 레지스터 ÷ 100으로 가정)
   - 설비 값을 채울 **대상 줄을 고르는 규칙** (현재는 "비어 있는 첫 줄"; 투입위치/자재코드로 매칭해야 한다면 알려줄 것)
   - **결과 저장 시점** — 지금은 [결과 저장] 버튼을 눌러야 서버로 올라간다(값을 넣을 때마다 자동 저장할지 결정 필요)
   - 이전/다음/생략/중지/시작 각각의 **전이 규칙과 필요한 실적 항목**
7. **작업 조회 현황 / 관리자 알람 남은 결정 사항**
   - 지시일자를 **어떤 날짜 기준**으로 거를지 (현재는 시작 예정일자 기준으로 가정)
   - 작업지시 상태 / 진행 상태 / 스팩 결과 상태의 **실제 코드값 목록** (현재는 대기·진행중·완료·보류 / 합격·불합격으로 가정) — 기준정보로 내려주면 콤보를 서버 값으로 채우도록 바꾼다
   - **관리자 알람 유형 목록과 코드** (현재 목업: 자재 결품 · 설비 이상 · 품질 이상 · 기타) 및 유형별로 서버가 받아야 하는 항목
   - 관리자 알람의 **통지 방식**(앱 알림 / 문자 / 방송)과 수신 대상 — 서버가 정할 부분이지만 화면에 결과를 되돌려 줄지 결정 필요
8. **작업통합 현황 조회 남은 결정 사항**
   - 트리 **계층 단계가 확정인지** (현재: 품목 → 라우팅 그룹 → 라우팅 공정 → 페이지 공정 → 투입자재 → 스팩). 단계가 늘거나 줄어도 `NodeId`/`ParentNodeId`만 맞으면 화면 수정은 필요 없다
   - 스팩 줄 외에 **중간 단계에도 값을 표시**할지 (예: 공정별 합계/판정) — 현재는 상위 줄을 `..`로 비워 둔다
   - 토크·앵글 외에 **다른 계측 항목**이 생길 수 있는지 — 컬럼이 고정이 아니라면 컬럼 정의도 서버에서 받는 구조로 바꿔야 한다
   - 엑셀 내보내기의 **양식 요구사항**(머리글/로고/시트명 등) — 현재는 화면 트리를 그대로 저장한다
