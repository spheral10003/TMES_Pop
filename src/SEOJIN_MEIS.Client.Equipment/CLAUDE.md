# SEOJIN_MEIS.Client.Equipment — 작업 규칙

설비(Tool) Modbus-TCP 통신 전담. 솔루션 공통 규칙은 [루트 CLAUDE.md](../../CLAUDE.md) 참고.
프로토콜 구조·코덱·기능코드 설명은 [README의 설비 통신](../../README.md#설비-통신--modbus-tcp-요구사항-6).

## 1. 두 Connection의 역할을 섞지 말 것

설비 1대에 커넥션이 두 개 열린다. 각각 **소켓 1개 + 전용 수신 스레드 1개**다.

| 클래스 | 포트 | 역할 |
| --- | --- | --- |
| `EquipmentModbusClient` (Standard) | `Port` | 요청/응답 한 쌍. **실시간 이벤트 수신을 제외한 모든 Read/Write는 이쪽으로만 나간다.** |
| `EquipmentEventClient` (Customized) | `EventPort`(기본 `Port + 1`) | Tool→Controller 실시간 이벤트 **수신 전용**. Modbus Read/Write 미지원 |

`EquipmentEventClient`에 요청 전송 API를 추가하지 말 것 — 장비 스펙상 지원하지 않아서
일부러 빼 둔 것이다. 두 커넥션의 공통 부분(TCP 접속, 수신 스레드, MBAP 프레이밍)은
`ModbusTcpConnection`에 있고, 차이는 `HandleFrame` 하나뿐이다.

모든 이벤트 인자에는 어느 커넥션에서 왔는지 `EquipmentChannel`(Standard/Event)이 함께 실린다.
새 이벤트를 만들 때도 이 필드를 빠뜨리지 말 것.

## 2. 이벤트는 UI 스레드가 아닌 곳에서 올라온다

`DataReceived` / `ConnectionChanged` / `ErrorOccurred`는 **각 커넥션의 전용 수신 스레드**에서 발생한다
(`EquipmentModbusManager`는 그대로 중계할 뿐 스레드를 바꾸지 않는다).

**화면에서 이 이벤트를 받아 컨트롤을 만지려면 반드시 UI 스레드로 넘길 것.**
`PopScreen`에 그 헬퍼가 있다(`if (InvokeRequired) BeginInvoke(action);`).

반대로 **이 프로젝트 안에서 `Invoke`/`SynchronizationContext`를 쓰지 말 것** —
UI를 모르는 것이 이 프로젝트의 경계다(루트 CLAUDE.md의 "프로젝트 경계").

## 3. 주소·값 변환은 App.config가 담당한다

화면/업무 코드는 **논리 명령**만 다룬다 — `EquipmentCommand.Start / Stop / Reset / StatusRequest`.
실제 레지스터 주소와 값은 `App.config`의 `Equipments[].RegisterMap`이 정한다.

**레지스터 주소를 코드에 박지 말 것.** 맵이 바뀌어도 화면 코드는 손대지 않아야 한다.
현재 App.config의 `RegisterMap` 값은 **임시값**이며 장비 문서 확정 후 교체 대상이다.

## 4. 접속은 UI를 막지 않는다

`Connect()`는 즉시 리턴하고, 실제 TCP 접속은 각 커넥션 전용 스레드에서 수행한다.
접속 타임아웃은 5초(`ConnectTimeoutMs`).

**`Connect()`를 동기 대기로 바꾸지 말 것** — POP 화면이 뜨는 동안 설비 수만큼 멈춘다.

App.config의 기본 예시 IP(`192.168.0.101/102`)는 실재하지 않는 더미 값이라
항상 5초 후 `Faulted`가 된다. **정상 동작이다** — 버그로 착각해 재시도 로직을 넣지 말 것.
당장 필요 없으면 `AutoConnect`를 `false`로 둔다.

### 시간 초과로 버리는 접속 작업은 반드시 "관찰"할 것

`ConnectAsync(...).Wait(ConnectTimeoutMs)`가 false를 돌려줘도 **그 Task는 죽지 않고 살아 있다.**
이어서 `catch`에서 `_tcpClient.Close()`를 부르면, 살아 있던 작업의 `TcpClient.EndConnect`가
없어진 소켓을 만져 `NullReferenceException`으로 끝난다.

버린 작업이라 아무도 예외를 보지 않으므로, GC가 정리할 때
`TaskScheduler.UnobservedTaskException`으로 올라와 전역 핸들러가 **ERROR로 로그에 남긴다.**
설비가 꺼져 있으면 5초마다 커넥션 수만큼 반복돼 로그가 도배되고 진짜 오류가 묻힌다.

그래서 `ObserveFaults(connectTask)`로 예외를 미리 관찰해 둔다.
**접속 실패 처리와는 별개다** — 실패 자체는 이미 `catch`에서 `Faulted`로 처리하고 있고,
이건 같은 실패를 두 번 보고하지 않게 막는 것이다.
비슷하게 **기다리기를 포기하는 Task를 새로 만들 때는 같은 처리를 함께 넣을 것.**

## 5. 프레이밍과 예외

- MBAP 코덱: `[Transaction Id(2)][Protocol Id(2)=0][Length(2)][Unit Id(1)][FC(1)][Data]`, big-endian.
  Length 필드로 프레임 경계가 확정되므로 **STX/ETX 같은 구분자나 체크섬 추정을 넣지 말 것**(TCP가 무결성을 보장).
  분할 수신·연속 수신·동기 이탈은 이미 처리되어 있다.
- 장비가 MBAP를 변형해 쓰면 **`IModbusCodec` 구현체만 교체**한다. 상위 코드는 그대로 둘 것.
- 요청/응답 짝은 **MBAP Transaction Id**로 맞춘다. 장비가 동시 요청을 못 받는 경우가 흔해
  요청은 커넥션 단위로 **직렬화**한다 — 이 직렬화를 풀지 말 것.
- 예외 타입을 유지할 것: 응답 없음 → `ModbusTimeoutException`, 예외 응답(FC | 0x80) → `ModbusProtocolException`.
  연결이 끊기면 대기 중인 요청은 즉시 실패시킨다.
- 표에 없는 벤더 확장 기능코드는 `ModbusRequest.Custom(fc, data)`로 raw PDU를 보낸다
  (`ModbusFunctionCode`에 임의 값을 추가하지 말 것).

## 6. 아직 없는 것

`Advertise` / `Scan ack` / `Search ack`로 IP·포트를 자동 수신하는 **탐색(discovery) 절차는 미구현**이다.
현재는 App.config에 직접 적어 쓰고 `EventPort`는 `Port + 1`로 자동 계산한다.
탐색을 붙일 때는 App.config 값을 **기본값/폴백**으로 남겨 둘 것.
