using System;

namespace SEOJIN_TMES.Client.Modbus.Abstractions
{
    /// <summary>
    /// 원시 TCP 소켓 송수신만 담당하는 전송 계층 인터페이스.
    /// Modbus 프로토콜(패킷 조립/해석)에 대해서는 전혀 알지 못하며,
    /// 오직 바이트 배열을 주고받는 역할만 한다. KwonLib.Net 같은
    /// 구체적인 소켓 구현체를 이 인터페이스 뒤로 캡슐화해서, 상위
    /// 계층(코덱/클라이언트)이 소켓 구현 방식에 의존하지 않도록 한다.
    /// (Pure raw-socket transport, decoupled from the Modbus protocol itself.)
    /// </summary>
    public interface IModbusTransport : IDisposable
    {
        bool IsConnected { get; }

        event EventHandler? Connected;
        event EventHandler? Disconnected;
        event EventHandler<byte[]>? DataReceived;
        event EventHandler<Exception>? ErrorOccurred;

        void Connect(string host, int port);

        void Disconnect();

        void Send(byte[] data);
    }
}
