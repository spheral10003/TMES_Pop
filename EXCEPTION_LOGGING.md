# 예외 · 로그 정리

클라이언트에서 일어나는 예외를 어디서 잡고, 무엇을 **로컬 파일**에 남기고, 무엇을 **서버**로 보내는지 정리한다.
(2026-10-27 기준 코드. 줄 번호는 이 시점 값이다.)

## 1. 한눈에 보기 — 로그 두 갈래

| 갈래 | 진입점 | 남는 곳 | 대상 |
| --- | --- | --- | --- |
| 로컬 로그 | `AppLog.Step/Info/Warn/Error` | `logs\meis-client-yyyyMMdd.log` (7일 압축 · 30일 보관) | **서버로 안 보내는 전부** — 처리 흐름, 예상된 실패, API 호출 기록, 설비 Alive · 체결 결과 수신 등 |
| 서버 로그 | `ServerLog.*` | 로컬 파일 **+** 서버 전송 대기열 | 아래 6종만 |

`ServerLog`는 항상 **로컬 파일에 먼저 쓰고** 그 다음 서버로 보낸다. 서버 전송이 실패하거나 API가 없어도 내용은 파일에 있다.

### 서버로 보내는 6종 (`ClientLogType`)

| 종류(`evntNm`) | 호출 위치 | 언제 | `msg` · 그 밖의 필드 |
| --- | --- | --- | --- |
| `AppStart` | [Program.cs](src/SEOJIN_TMES.Client/Program.cs) | 프로그램 시작 직후 | msg에 버전 · PC 이름 |
| `Login` | [LoginForm.cs](src/SEOJIN_TMES.Client/Forms/LoginForm.cs) | 로그인 성공 | msg에 아이디 · PC 이름 |
| `MenuClick` | [MainForm.cs](src/SEOJIN_TMES.Client/Forms/MainForm.cs) `Accordion_ElementClick` | 좌측 메뉴(화면 항목) 클릭 | msg에 메뉴명(키) · `Open`/`Focus` |
| `EquipmentConnect` | [Devices.cs](src/SEOJIN_TMES.Client/Core/Devices.cs) ← `NutrunnerDevice.ConnectAttempted` | 너트러너 접속 성공 / 실패(Request · Event 채널 각각) | `eqpCd · eqpIp · eqpPort`, 실패면 `errYn=Y` · `errCd=EQP_001` · `errMsg`=코드 문구 + 실패 사유 |
| `ModbusWrite` | [Devices.cs](src/SEOJIN_TMES.Client/Core/Devices.cs) ← `NutrunnerDevice.SettingWritten` | **설정 레지스터(토크 · 드라이버 잠금) 쓰기** 송신 1건 + 응답 1건 | `eqpCd · eqpIp · eqpPort`, msg에 요청 · 송신/응답 · 값, 오류 응답이면 `errYn=Y` · `errCd=EQP_002` |
| `CriticalError` | `GlobalErrorHandler` · `Program` · 각 화면의 `catch (Exception)` | 처리되지 않은 예외 / 예기치 못한 예외 | `errYn=Y`, `errCd`=`SYS_001`/`SYS_002`(감싼 예외가 ApiException이면 그 코드), `errMsg`=코드 문구 + 예외 전체(타입 · 스택) |

모든 항목에 공통으로 `corpCd · userId · wkstnCd · deptCd · lineCd · evntOcrnDtm(yyyy-MM-dd HH:mm:ss) · errYn`이 실린다.
로그인 전에 생긴 로그는 corpCd · userId가, 워크스테이션을 고르기 전(메인 화면 Shown)에 생긴 로그는 wkstnCd · deptCd · lineCd가 비어 있다.
서버로 보내는 `msg` · `errMsg`는 화면 언어와 상관없이 **항상 한국어**다(아래 "오류 코드" 참고).

### 오류 코드

[ErrorCodes.cs](src/SEOJIN_TMES.Client.Common/Errors/ErrorCodes.cs) — 형식은 서버 업무 코드(`WORK_005`)와 같은 `분류_번호`.
코드별 문구는 Languages의 `Error_{코드}` 키(ko/en/vi)에 있고, [ErrorText.cs](src/SEOJIN_TMES.Client.Common/Errors/ErrorText.cs)가 꺼낸다.

| 코드 | 뜻 | 어디서 |
| --- | --- | --- |
| `API_001` | 서버에 연결할 수 없음 | `ApiException.FromTransportFailure` (HttpRequestException) |
| `API_002` | 서버 응답 시간 초과 | `ApiException.FromTransportFailure` (취소 = 타임아웃) |
| `API_003` | 그 밖의 통신 오류 | `ApiException.FromTransportFailure` |
| `API_004` | 서버 응답 형식 오류 | `ApiException.FromInvalidResponse` |
| `API_005` | 서버 오류 응답(업무 코드 없음, HTTP 상태코드 표시) | `ApiException.FromErrorResponse` |
| `API_006` | 로그인 응답에 토큰 없음 | `ApiException.FromMissingToken` |
| `EQP_001` | 설비 접속 실패 | `ServerLog.EquipmentConnected` · POP 접속 실패 창 |
| `EQP_002` | 설정 레지스터 쓰기 오류 응답 | `ServerLog.ModbusWritten` |
| `EQP_003` | 설비 재연결 시작 실패 | POP 장비 그리드 [재연결] |
| `FILE_001` | 도면 공유 폴더 파일 읽기 실패(입출력 오류) | `DocumentShareClient.ReadAllBytesAsync` |
| `FILE_002` | 로컬 파일 읽기 · 쓰기 실패 | `IOException` · `UnauthorizedAccessException` (엑셀 저장 등) |
| `FILE_003` | 도면 파일 경로 비어 있음 | `DocumentShareClient.Resolve` |
| `FILE_004` | 도면 공유 폴더(RootPath) 미설정 | `DocumentShareClient.Resolve` |
| `FILE_005` | 도면 파일 없음 | `DocumentShareClient.ReadAllBytesAsync` |
| `FILE_006` | 도면 파일 크기 상한(64MB) 초과 | `DocumentShareClient.ReadAllBytesAsync` |
| `FILE_007` | 도면 파일 접근 권한 없음 | `DocumentShareClient.ReadAllBytesAsync` |
| `FILE_008` | 공유 폴더 접근 거부(Windows 5) | `SmbConnection.Ensure` |
| `FILE_009` | 공유 폴더를 찾을 수 없음(Windows 53) | `SmbConnection.Ensure` |
| `FILE_010` | 공유 이름 오류(Windows 67) | `SmbConnection.Ensure` |
| `FILE_011` | 공유 계정 또는 비밀번호 오류(Windows 86 · 1326) | `SmbConnection.Ensure` |
| `FILE_012` | 같은 서버에 다른 계정으로 이미 연결(Windows 1219) | `SmbConnection.Ensure` |
| `FILE_013` | 그 밖의 공유 연결 실패(Windows 오류 번호 표시) | `SmbConnection.Ensure` |

`DocumentShareException`도 ApiException처럼 Message를 읽는 시점의 화면 언어로 만들고 코드 줄을 붙인다(파일 · 공유 경로는 번역 없이 그 아래에).
POP 도면 영역은 "도면을 불러오지 못했습니다" 아래에 이 사유와 코드를 함께 보여 준다.
| `SYS_001` | 화면이 잡은 예기치 못한 예외 | `ServerLog.Unexpected` |
| `SYS_002` | 어디서도 잡지 못한 예외 | `ServerLog.Critical` (전역 처리기) |

서버가 오류 본문에 업무 코드(`WORK_005` 등)를 주면 화면 · 로그에는 그 코드가 쓰인다(`ApiException.Code`).
서버 업무 코드에는 클라이언트 문구가 없어 서버가 준 사유(`message`)를 그대로 보여 준다.

**화면** — 현재 언어의 코드 문구 아래에 빈 줄 + `오류 코드: XXX_000` 줄.
- `ApiException.Message`는 읽는 시점의 화면 언어로 만들어지고 코드 줄까지 들어 있다 → 화면은 `ex.Message`를 그대로 띄운다.
- 그 밖의 예외는 `ErrorText.ForDisplay(ex)`(코드 문구 + 원인 + 코드 줄), 고정 안내 문구에는 `ErrorText.WithCode(문구, 코드)`.

**서버** — `errCd`=코드, `errMsg`=`ErrorText.Korean(코드)` 첫 줄 + 상세. ApiException은 `MessageFor(ko-KR)`로 한국어 문구를 꺼낸다.
.NET이 만든 예외 메시지(스택 앞 줄)는 PC의 Windows 언어를 따르므로 여기서는 바꾸지 않는다.

**코드를 추가할 때** — `ErrorCodes` 상수 + 세 resx의 `Error_{코드}` 키(같은 순서) + `Language.cs` 속성 + 이 표.
이미 나간 코드의 의미는 바꾸지 말 것(서버에 쌓인 로그와 현장 문의가 코드로 연결된다).

### 설비(너트러너) 쪽 구조

Equipment 프로젝트는 Common을 참조하지 않으므로 `ServerLog`를 직접 부르지 않는다. 대신 이벤트를 올리고 화면 프로젝트가 받는다.

```
NutrunnerDevice ──ConnectAttempted / SettingWritten──▶ NutrunnerManager(중계) ──▶ Devices(Client/Core) ──▶ ServerLog
```

- **접속 결과** — 접속 실패는 별도 이벤트 없이 `ErrorOccurred`로만 올라온다. 그래서 `Connect()` 때 "시도 중" 표시를 켜 두고,
  시도 중에 난 오류만 실패로 한 번 보고한다. `NutrunnerSequence`가 3초 간격 최대 3회 재접속하므로 실패는 시도마다 1건씩 남는다.
- **설정 변경 이력** — 이 장비 통신은 쓰기 요청이 응답을 기다리지 않는다. 그래서 송신과 응답을 **각각 1건**으로 남긴다
  (송신만 있고 응답이 없으면 장비가 받지 못한 것이다). 응답은 기능 코드 0x06 + 주소(Torque=2 / DriverLock=4001)로 고른다.

### 서버로 보내지 않는 것 (로컬에만)

- 설비 **Alive 신호**(5초 주기 Status 읽기)와 그 응답
- **동작 결과 데이터** — Event 채널로 올라오는 체결 결과(`FasteningReceived`)와 수신 확인(EventAck) 쓰기
- 이벤트 채널 핸드셰이크(EventDataEnable 쓰기)
- 연결 끊김 · 통신 알람(`AlarmOccurred`)
- 예상된 실패 전부 — API 오류 응답, 통신 실패, 도면 · 첨부 파일 접근 실패, 엑셀 저장 실패, 설정 파일 실패

## 2. 전송 시점

```
프로그램 시작 ─ AppStart ─┐
로그인 성공 ─ Login ──────┘  대기열에 모아 둔다 (토큰이 없어 보낼 수 없음)
                           │
메인 화면 Load ── ServerLog.Attach(ClientLogApiClient) ── 모아 둔 것을 한 번에 전송
                           │
이후 ─ MenuClick / EquipmentConnect / ModbusWrite / CriticalError ── 생길 때마다 바로 전송
```

- 전송은 백그라운드에서 한 줄로(동시에 여러 요청 없이) **한 건씩** 보낸다 — 서버 API가 한 요청에 1건만 받는다.
- 대기열 상한은 1000건 — 넘치면 오래된 것부터 버린다(파일에는 남아 있다).
- 전송 실패는 **재시도하지 않는다** — 그 건만 버리고 `AppLog.Warn`으로 "전송 실패 종류 시각"을 남긴 뒤 다음 건을 보낸다. 같은 내용이 이미 파일에 있다.
- `ServerLog`의 어떤 메서드도 호출자에게 예외를 던지지 않는다(설비 통신 스레드에서도 불린다).

## 3. 로그 전송 API — `POST api/v1.0/pop/event-logs`

[IClientLogApiClient.cs](src/SEOJIN_TMES.Client.ApiClient/IClientLogApiClient.cs)의 `ClientLogApiClient.SetEventLog`가 통로다.
요청 바디는 로그 **1건**(배열 아님)이며 필드는 [ClientLogDto.cs](src/SEOJIN_TMES.Client.Common/Models/ClientLogDto.cs)의
`[JsonProperty]` 이름(`corpCd · evntNm · eqpCd · eqpIp · eqpPort · msg · errMsg · evntOcrnDtm · wkstnCd · deptCd · lineCd · userId · errYn · errCd`)이 서버 계약이다.
값을 채우는 곳은 `ServerLog`뿐이다.

## 4. 예기치 못한 예외 — `UnexpectedException`

[UnexpectedException.cs](src/SEOJIN_TMES.Client.Common/Logging/UnexpectedException.cs)

예상한 실패(`ApiException`, `DocumentShareException`, `IOException` …)가 아닌 예외는
원래 타입이 무엇이든 `UnexpectedException(Origin, inner)`으로 묶어서 남긴다.
로그 파일에서 `UnexpectedException`으로 검색하면 "처리하지 못한 오류"만 골라 볼 수 있다.

| 경로 | 처리 |
| --- | --- |
| UI 스레드 미처리 예외 (`Application.ThreadException`) | `ServerLog.Critical("UI 스레드", ex)` + 안내 창 |
| 백그라운드 스레드 미처리 예외 (`AppDomain.UnhandledException`) | `ServerLog.Critical("백그라운드 스레드", ex)` + 안내 창 |
| 관찰되지 않은 Task 예외 (`TaskScheduler.UnobservedTaskException`) | `ServerLog.Critical("처리되지 않은 백그라운드 작업", ex)` (창 없음) |
| `Program.Main`의 마지막 catch | `ServerLog.Critical("애플리케이션 실행", ex)` |
| 화면의 `catch (Exception)` 중 원인을 예상할 수 없는 곳 | `ServerLog.Unexpected(origin, ex)` |
| POP 화면 — `PopScreen.HandleError` | ApiException은 Warn(로컬), 그 밖은 `ServerLog.Unexpected("POP …")` |

새 코드 규칙:
- **예상한 실패는 구체 타입으로 잡고 `AppLog`에만 남긴다.**
- `catch (Exception)`이 꼭 필요하고 원인을 예상할 수 없으면 `ServerLog.Unexpected(...)`를 쓴다.
- POP 화면은 `HandleError(actionName, ex)` 하나로 모은다 — 분류는 그 안에서 한다.

## 5. try/catch 전수 목록

분류: **[예상]** 예상된 실패 · 로컬만 / **[예기치 못함]** `UnexpectedException` · 서버 전송 / **[변환]** 다른 예외로 바꿔 올림 / **[무시]** 의도적으로 무시 / **[이력]** 서버 이력 기록

(`_backup*` 폴더, `PopScreen2`, `*_개선소스(미적용).cs`는 빌드에서 빠지거나 쓰이지 않아 제외했다.)

### Common

| 위치 | 잡는 예외 | 처리 | 분류 |
| --- | --- | --- | --- |
| `Logging/AppLog.cs:112` | Exception | 파일 기록 중단, 콘솔·Trace 계속 | [무시] 로그가 앱을 죽이면 안 됨 |
| `Logging/AppLog.cs:195,231,270,290,333` | Exception | 압축·정리·닫기 실패 → `Trace` | [무시] AppLog 재귀 금지 |
| `Logging/AppLog.cs:370` | (전체) | 쓰기 불가 폴더 → 대체 폴더 | [무시] |
| `Logging/ServerLog.cs:262` | Exception | 전송 실패 → `AppLog.Warn` | [예상] 재시도 없음 |
| `Configuration/UserPreferences.cs:40` | Exception | 설정 로드 실패 → 기본값 | [예상] |
| `Configuration/UserPreferences.cs:56` | Exception | 설정 저장 실패 → Warn | [예상] |
| `Imaging/ByteToImage.cs:40` | FormatException | Base64 아님 → false | [예상] |
| `Storage/DocumentShareClient.cs:97` | DocumentShareException | 그대로 rethrow | [변환] |
| `Storage/DocumentShareClient.cs:101` | UnauthorizedAccessException | → DocumentShareException | [변환] |
| `Storage/DocumentShareClient.cs:105` | IOException | → DocumentShareException | [변환] |

### ApiClient

| 위치 | 잡는 예외 | 처리 | 분류 |
| --- | --- | --- | --- |
| `ApiClientBase.cs:170` | ApiException | 그대로 rethrow | [변환] |
| `ApiClientBase.cs:174` | OperationCanceledException(호출자 취소) | 그대로 rethrow | [무시] |
| `ApiClientBase.cs:178` | Exception | Error 로그 → ApiException(StatusCode 0, 타임아웃 포함) | [변환] |
| `ApiClientBase.cs:199` | Exception(본문 해석 실패) | Error 로그 → `ApiException.FromInvalidResponse` | [변환] |
| `LoggingHandler.cs:56` | OperationCanceledException(취소) | 로그 후 rethrow | [예상] |
| `LoggingHandler.cs:64` | Exception | `AppLog.Error` → ApiException(StatusCode 0) | [변환] |
| `LoggingHandler.cs:97` | Exception | 본문 읽기 실패 문구로 대체 | [무시] |
| `AuthTokenHandler.cs:67` | Exception | 토큰 갱신 실패 → Warn, 401 그대로 | [예상] |

### Equipment · Modbus · Socket

| 위치 | 잡는 예외 | 처리 | 분류 |
| --- | --- | --- | --- |
| `Equipment/Core/ThreadBase.cs:98` | Exception | 시퀀스 스레드 오류 → `OnError` → 통신 알람(로컬) | [예상] |
| `Modbus/Transport/TcpModbusTransport.cs:74` | Exception | 송신 실패 → ErrorOccurred | [예상] |
| `Socket/AsyncSocket.cs:289` | Exception | 접속 시작 실패 → OnError → **접속 시도 중이면 `EquipmentConnect`(실패)** | [이력] |
| `Socket/AsyncSocket.cs:325~573` (11곳) | Exception | 접속 콜백 · 송수신 · 종료 실패 → OnError(접속 콜백 실패도 위와 같이 [이력]) | [예상] |
| `Socket/SocketExtensions.cs:21` | ObjectDisposedException | 이미 닫힌 소켓 → 무시 | [무시] |

### Client (화면 · 조립)

| 위치 | 잡는 예외 | 처리 | 분류 |
| --- | --- | --- | --- |
| `Program.cs:52` | Exception | `ServerLog.Critical("애플리케이션 실행")` | [예기치 못함] |
| `Composition/AppVersion.cs:30` | InvalidDeploymentException | 어셈블리 버전으로 대체 + Warn | [예상] |
| `Composition/GlobalErrorHandler.cs` 3개 핸들러 | 미처리 예외 | `ServerLog.Critical` | [예기치 못함] |
| `Composition/GlobalErrorHandler.cs:57` | Exception | 안내 창 표시 실패 → Error | [예상] |
| `Core/Global.cs:72` | Exception | 전역 초기화 실패 → Error, SMB 정리 후 rethrow(→ Program) | [변환] |
| `Core/Global.cs:112` | Exception | 종료 정리 단계 실패 → Error | [예상] |
| `Core/Setting.cs:75` | CultureNotFoundException | 언어 설정 무시 | [무시] |
| `Core/Setting.cs:110` | Exception | 화면 언어 저장 실패 → Warn | [예상] |
| `Core/Devices.cs:111` | ArgumentException | 같은 장비 ID 중복 → 건너뜀 | [예상] |
| `Forms/LoginForm.cs:300` | ApiException 401 | 로그인 실패 안내 | [예상] |
| `Forms/LoginForm.cs:305` | ApiException 통신 실패 | 연결 불가 안내 | [예상] |
| `Forms/LoginForm.cs:311` | ApiException | 상태코드 안내 | [예상] |
| `Forms/LoginForm.cs:316` | Exception | `ServerLog.Unexpected("로그인")` | [예기치 못함] |
| `Forms/MainForm.cs:297` | CultureNotFoundException | 언어 전환 취소 | [무시] |
| `Forms/MainForm.cs:572` | ApiException | 알림 폴링 실패 → Warn(창 없음) | [예상] |
| `Forms/MainForm.cs:576` | Exception | `ServerLog.Unexpected("알림 미읽음 개수 조회")` | [예기치 못함] |
| `Forms/MainForm.cs:737` | ApiException | 메뉴 조회 실패 → Warn + 안내 창 | [예상] |
| `Forms/MainForm.cs:743` | Exception | `ServerLog.Unexpected("메뉴 조회")` | [예기치 못함] |
| `Forms/UserProfileForm.cs:136` | ApiException | 조회 실패 안내 | [예상] |
| `Forms/UserProfileForm.cs:141` | Exception | `ServerLog.Unexpected("사용자 프로필 조회")` | [예기치 못함] |
| `Forms/Notification/NotificationForm.cs:218` | OperationCanceledException | 사용자 취소 | [무시] |
| `Forms/Notification/NotificationForm.cs:222,270` | ApiException | 안내 창(닫힌 뒤면 생략) | [예상] |
| `Forms/Attachment/AttachmentSearchScreen.cs:207,285,396,478,548` | ApiException | 안내 창 | [예상] |
| `Forms/Attachment/AttachmentSearchScreen.cs:402,581` | IOException · UnauthorizedAccessException · ArgumentException | 파일 저장/열기 실패 안내 | [예상] |
| `Forms/Pop/AdminAlarmForm.cs:135` | Exception | `ServerLog.Unexpected("POP 알람 코드 조회")` | [예기치 못함] |
| `Forms/Pop/AdminAlarmForm.cs:153,349` | ApiException | 안내 창 | [예상] |
| `Forms/Pop/AdminAlarmForm.cs:321` | Exception | `ServerLog.Unexpected("POP 알람 등록")` | [예기치 못함] |
| `Forms/Pop/ByteToImage.cs:40` | FormatException | Base64 아님 → false | [예상] |
| `Forms/Pop/FileUploadForm.cs:89,199,284,332` | ApiException | 안내 창 | [예상] |
| `Forms/Pop/FileUploadForm.cs:114` | IOException · UnauthorizedAccessException · ArgumentException | 파일 읽기 실패 안내 | [예상] |
| `Forms/Pop/FileUploadForm.cs:174` | ArgumentException | 잘못된 파일 경로 처리 | [예상] |
| `Forms/Pop/PopScreen.cs:246,292,304,316,328,340,550` | Exception | `HandleError` → API면 Warn, 아니면 `ServerLog.Unexpected` | [예상]/[예기치 못함] |
| `Forms/Pop/PopScreen.cs:259` | Exception | `ServerLog.Unexpected("POP 설비 연결 해제")` (창 없음) | [예기치 못함] |
| `Forms/Pop/PopScreen.Grid.cs:511,563,660,808,890` | Exception | `HandleError`(위와 같음) | [예상]/[예기치 못함] |
| `Forms/Pop/PopScreen.Grid.cs:623` | OverflowException | 토크 입력값 오류 처리 | [예상] |
| `Forms/Pop/PopScreen.Method.cs:160,286,412,634,674,718` | ApiException | 안내 창 | [예상] |
| `Forms/Pop/PopScreen.Method.cs:919` | Exception | 도면 읽기 실패(깨진 파일 등) → Warn + 뷰어 안내 | [예상] |
| `Forms/ProductionStatus/ProductionStatusScreen.cs:75 / 79` | OperationCanceledException / ApiException | 취소 무시 / 안내 창 | [무시]/[예상] |
| `Forms/WorkIntegratedStatus/WorkIntegratedStatusScreen.cs:209 / 213` | OperationCanceledException / ApiException | 취소 무시 / 안내 창 | [무시]/[예상] |
| `Forms/WorkIntegratedStatus/WorkIntegratedStatusScreen.cs:258` | Exception | 엑셀 저장 실패(파일 잠김·권한) → 안내 | [예상] |
| `Forms/WorkOrderEntry/WorkOrderEntryScreen.cs:160,208 / 164,212` | OperationCanceledException / ApiException | 취소 무시 / 안내 창 | [무시]/[예상] |
| `Forms/WorkStatus/ItemPickerForm.cs:150 / 154` | OperationCanceledException / ApiException | 취소 무시 / 안내 창 | [무시]/[예상] |
| `Forms/WorkStatus/WorkStatusScreen.cs:270 / 274, 314` | OperationCanceledException / ApiException | 취소 무시 / 안내 창 | [무시]/[예상] |

## 6. 남은 확인 사항

- **서버 로그 API 경로 · 필드명** — 3장 참고.
- **설정 쓰기 오류 응답** — 장비가 예외 응답(FC | 0x80)을 주면 응답에 주소가 없어 어느 레지스터였는지 고를 수 없다.
  지금은 이 경우 응답 이력이 남지 않는다(송신 이력만 남는다).
- **접속 실패 감지** — 소켓이 접속 실패를 `ErrorOccurred`로 올린다는 전제다. 조용히 끝나는 실패가 있으면
  `EquipmentConnect`(실패)가 빠지고, 3회 재시도 뒤의 `ConnectFailed` 알람(로컬)에만 남는다. 실기에서 확인 필요.
- `ThreadBase.OnError`의 기본 구현이 `Console.WriteLine`을 쓴다(공통 규약 위반). Equipment가 Common을 참조하지 않아 이번에 손대지 않았다.
- 메인 화면 Load 이전에 앱이 끝나면(로그인 취소, 로그인 화면에서의 치명 오류) 모아 둔 로그는 서버로 가지 않는다(파일에만 남는다).
- 로그아웃 직후 · 종료 직전에 생긴 로그는 토큰이 지워진 뒤라 전송이 401로 실패할 수 있다(파일에는 남는다).
