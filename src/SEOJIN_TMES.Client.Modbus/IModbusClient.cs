using System;
using SEOJIN_TMES.Client.Modbus.Protocol;

namespace SEOJIN_TMES.Client.Modbus
{
    /// <summary>
    /// Modbus TCP 통신을 캡슐화한 최상위 인터페이스.
    /// 다른 프로그램은 이 인터페이스(및 <see cref="Protocol"/> 네임스페이스의 타입)만
    /// 참조하면 되고, 소켓 구현(KwonLib)이나 패킷 조립 방식 같은 내부 구현에는
    /// 의존하지 않는다. 기존 코드와의 호환을 위해 이벤트 기반(콜백) 방식을 그대로
    /// 유지한다.
    /// (Top-level interface encapsulating Modbus TCP communication. Consumers only
    /// need to depend on this interface and the Protocol types — never on the
    /// concrete socket/codec implementation.)
    /// </summary>
    public interface IModbusClient : IDisposable
    {
        bool IsConnected { get; }

        event EventHandler? Connected;
        event EventHandler? Disconnected;
        event EventHandler<Exception>? ErrorOccurred;

        /// <summary>Read/Write 요청에 대한 응답을 수신했을 때 발생.</summary>
        event EventHandler<ModbusResponse>? ResponseReceived;

        void Connect(string host, int port);

        void Disconnect();

        /// <summary>
        /// Holding/Input Register Read 요청을 전송한다. 응답은
        /// <see cref="ResponseReceived"/> 이벤트로 비동기 전달된다.
        /// </summary>
        void ReadRegisters(ushort address, ushort count,
            ModbusFunctionCode function = ModbusFunctionCode.ReadInputRegisters, byte unitId = 0x00);

        /// <summary>
        /// Single Register Write 요청을 전송한다. 응답은
        /// <see cref="ResponseReceived"/> 이벤트로 비동기 전달된다.
        /// </summary>
        void WriteRegister(ushort address, ushort value,
            ModbusFunctionCode function = ModbusFunctionCode.WriteSingleRegister, byte unitId = 0x00);
    }
}
