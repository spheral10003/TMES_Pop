---
name: seojin-client-equipment
description: SEOJIN_TMES.Client.Equipment(Edge Device 계층) 작업 시 사용. Nutrunner 등 장비의 정보/데이터 관리, ThreadBase 기반 통신 시퀀스, 다중 장비 관리(NutrunnerManager), 재연결·알람 규칙을 다룰 때 참조한다.
---

# SEOJIN_TMES.Client.Equipment

Edge Device(Nutrunner 등) 계층. **장비의 "정보"와 "데이터"를 관리**하고, 통신은 전부
`SEOJIN_TMES.Client.Modbus` 에 위임한다.

## 이 프로젝트가 지키는 경계

| 하는 것 | 하지 않는 것 |
| --- | --- |
| 장비 정보(ID / IP / SerialNo / Model …) 보관 | 소켓 직접 제어 |
| 장비 데이터(레지스터 응답, 체결 이벤트) 해석·보관 | Modbus 패킷 직접 조립 |
| 연결 유지 시퀀스(Thread), 재연결, 알람 | WinForms / View 참조 |
| 여러 대 동시 관리(ID 기반) | 화면 출력 |

> 통신이 필요하면 **반드시** `IModbusClient` / `IModbusTransport` 를 통해서만 한다.
> `SEOJIN_TMES.Client.Modbus` 폴더는 **수정 금지**.

## 폴더 구조

```
Core/                       장비 공통 기반
  IThreadBase.cs            Thread 제어 인터페이스
  ThreadBase.cs             반복 Thread 기반 클래스 (Execute() 1 tick만 구현하면 됨)
  IEquipment.cs             장비 공통 인터페이스 (Id / IsConnected / Connect / Disconnect / 이벤트)
  EquipmentEventArgs.cs     EquipmentEventArgs, EquipmentConnectionEventArgs, AlarmCode, AlarmEventArgs

Nutrunner/                  Nutrunner 전용
  NutrunnerTypes.cs         NutrunnerRegister(주소), ModelTypes, GenerationTypes, UnitTypes, EventTypes
  NutrunnerInfo.cs          장비 "정보" (ID = TransactionId)
  FasteningData.cs          장비 "데이터" (체결 결과)
  NutrunnerEventArgs.cs     Register / Fastening 이벤트 인자
  NutrunnerEventParser.cs   Event Transport 프레임 파서 (ProtocolId = 1 만 수용)
  INutrunner.cs             장비 인터페이스
  NutrunnerDevice.cs        장비 구현체 (Request 채널 + Event Transport 채널)
  INutrunnerSequence.cs     시퀀스 인터페이스
  NutrunnerSequence.cs      ThreadBase 기반 연결 유지 시퀀스

Management/
  INutrunnerManager.cs      다중 장비 관리 인터페이스
  NutrunnerManager.cs       ID(int index)를 키로 장비 + 시퀀스를 보관
```

## 장비 / 데이터 구분 규칙 (중요)

| 항목 | 값 | 의미 |
| --- | --- | --- |
| `NutrunnerInfo.Id` | int index | 장비를 구분하는 유일 키 |
| `TransactionId` | `= Id` | Modbus 프레임에서 **어느 장비인지** 구분 |
| `ProtocolId = 0` | Request | 레지스터 Read/Write 요청·응답 채널 |
| `ProtocolId = 1` | Event Transport | 체결 결과 Push 채널 |

- Request 채널의 ProtocolId 0 은 `ModbusTcpCodec` 이 항상 0으로 인코딩한다.
- Event Transport 프레임은 `NutrunnerEventParser.TryParse` 가 ProtocolId == 1 인 것만 받아들인다.
  (장비가 0으로 보낸다면 `NutrunnerEventParser.EventProtocolId` 만 바꾸면 된다.)
- 포트는 Request = `Port`, Event Transport = `Port + 1` (`NutrunnerInfo.ResolvedEventPort`).
- 소켓 ID 도 장비별로 분리한다: Request = `Id * 2`, Event = `Id * 2 + 1`.

## 시퀀스 규칙 (NutrunnerSequence)

1. `Start()` → Thread 기동, 최초 연결은 대기 없이 즉시 시도.
2. 연결 유지 중에는 5초마다
   - Event Transport 채널이 끊겨 있으면 그 채널만 다시 연결 (`ConnectEventChannel`)
   - Alive 송신 (레지스터 `Status(3200)` 1워드 Read)
3. 통신이 끊기면 **Disconnect 이벤트 핸들러**가 재연결 타이머를 리셋한다.
   → 끊긴 시점부터 3초 뒤에 루프가 재연결을 시도한다. (연결 실패 시 재시도 폭주 방지)
4. 재연결은 3초 간격, **최대 3회**(`MaxRetryCount`).
5. 3회를 넘기면 `AlarmCode.ConnectFailed` 알람 이벤트 발생 후 **시퀀스(Thread) 종료**.
6. 연결에 성공하면 재시도 카운트는 0으로 초기화된다.
7. 재연결 중 반복되는 소켓 예외는 알람으로 올리지 않는다. (연결된 상태의 예외만 `CommunicationError`)

## 사용 예

```csharp
INutrunnerManager manager = new NutrunnerManager();

manager.ConnectionChanged  += (s, e) => Console.WriteLine($"[{e.Id}] connected={e.IsConnected}");
manager.RegisterDataReceived += (s, e) => Console.WriteLine($"[{e.Id}] {e.StartAddress} : {string.Join(",", e.Data)}");
manager.FasteningReceived  += (s, e) => Console.WriteLine($"[{e.Id}] torque={e.Data.TorqueValue:0.00}");
manager.AlarmOccurred      += (s, e) => Console.WriteLine(e);

// 여러 대 등록 — ID 가 곧 TransactionId
manager.Add(new NutrunnerInfo { Id = 1, Ip = "10.200.3.104", Port = 5000 });
manager.Add(new NutrunnerInfo { Id = 2, Ip = "10.200.3.105", Port = 5000 });
manager.StartAll();

INutrunner? tool = manager[1];
tool?.WriteRegister(NutrunnerRegister.Torque, 1500);
tool?.ReadRegisters(3200, 13);
```

## 확장 규칙

- 새 장비를 추가할 때는 `Core/IEquipment` 를 구현하는 `XxxDevice` + `ThreadBase` 기반 `XxxSequence`
  + `XxxManager` 세 쌍을 같은 패턴으로 만든다.
- 장비 고유 패킷 해석은 **반드시** 해당 장비 폴더 안의 Parser 에만 둔다.
  (`SEOJIN_TMES.Client.Modbus` 로 올리지 않는다.)
- 공개 API는 인터페이스로 노출하고, 구현 클래스는 `sealed` 로 둔다.
