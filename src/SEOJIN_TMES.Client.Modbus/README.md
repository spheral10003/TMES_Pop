# SeojinModbus

Modbus TCP 통신을 캡슐화한 독립 클래스 라이브러리입니다. `ModbusTest`(WinForms 테스트 툴)
뿐 아니라 다른 프로그램에서도 이 라이브러리 하나만 참조(ProjectReference 또는 dll)하면
Modbus TCP 장비와 통신할 수 있습니다.

## 계층 구조

- `IModbusTransport` / `TcpModbusTransport` — 순수 TCP 소켓 송수신만 담당 (KwonLib 캡슐화)
- `IModbusPacketCodec` / `ModbusTcpCodec` — Modbus TCP(MBAP+PDU) 패킷 조립/해석
- `IModbusClient` / `ModbusTcpClient` — 위 두 계층을 조합한 최상위 API (다른 프로그램은 보통 이 인터페이스만 사용)

장비별 커스텀 로직(예: Nutrunner 체결 이벤트 파싱)은 이 라이브러리에 포함하지 않습니다.
`ModbusTest/Devices/Nutrunner` 폴더처럼, 이 라이브러리를 참조하는 애플리케이션 쪽에
별도 계층으로 분리해서 구현하세요.

## 사용 예

```csharp
using SeojinModbus;
using SeojinModbus.Protocol;

IModbusClient client = new ModbusTcpClient();

client.Connected += (s, e) => Console.WriteLine("connected");
client.ResponseReceived += (s, response) =>
{
    if (response.IsError)
    {
        Console.WriteLine("error response");
        return;
    }

    Console.WriteLine($"[{response.StartAddress:D4}] {string.Join(",", response.Data)}");
};

client.Connect("10.200.3.104", 5000);

// Holding Register 읽기
client.ReadRegisters(0x0C80, 0x000D, ModbusFunctionCode.ReadHoldingRegisters);

// 레지스터 쓰기
client.WriteRegister(0x0FA1, 0x0001, ModbusFunctionCode.WriteSingleRegister);
```

## 테스트 더블 주입

`ModbusTcpClient(IModbusTransport transport, IModbusPacketCodec codec, ushort transactionId = 1)`
생성자로 가짜(fake) 전송/코덱을 주입할 수 있어, 실제 장비 없이도 단위 테스트가 가능합니다.
