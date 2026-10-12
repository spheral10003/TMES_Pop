using System;
using System.Linq;
using System.Threading;
using SEOJIN_TMES.Client.Sockets;
using SEOJIN_TMES.Client.Modbus.Abstractions;

namespace SEOJIN_TMES.Client.Modbus.Transport
{
    /// <summary>
    /// KwonLib.Net.AsyncSocketClient를 감싸는 TCP 전송 구현체.
    /// 기존 TcpBase에 있던 연결/재연결/송신 로직을 그대로 옮겨왔다.
    /// 이 클래스 밖으로는 IModbusTransport 인터페이스만 노출되므로,
    /// KwonLib에 대한 의존성은 이 라이브러리 내부로 캡슐화된다.
    /// </summary>
    public sealed class TcpModbusTransport : IModbusTransport
    {
        private readonly AsyncSocketClient _sock;
        private bool _disposed;

        public event EventHandler? Connected;
        public event EventHandler? Disconnected;
        public event EventHandler<byte[]>? DataReceived;
        public event EventHandler<Exception>? ErrorOccurred;

        public bool IsConnected => _sock.IsConnected;

        public TcpModbusTransport(int socketId = 0)
        {
            _sock = new AsyncSocketClient(socketId);

            _sock.OnConnet += OnSockConnect;
            _sock.OnClose += OnSockClose;
            _sock.OnReceive += OnSockReceive;
            _sock.OnError += OnSockError;
        }

        public void Connect(string host, int port)
        {
            if (string.IsNullOrEmpty(host))
            {
                return;
            }

            if (_sock.Connection == null)
            {
                _sock.Connect(host, port);
            }
            else if (_sock.Connection.Connected)
            {
                _sock.Close();
                Thread.Sleep(200);
                _sock.Connect(host, port);
            }
            else
            {
                _sock.Connect(host, port);
            }
        }

        public void Disconnect()
        {
            if (_sock.Connection?.Connected == true)
            {
                _sock.Close();
            }
        }

        public void Send(byte[] data)
        {
            try
            {
                _sock.Send(data);
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, ex);
            }
        }

        private void OnSockConnect(object sender, AsyncSocketConnectionEventArgs e)
            => Connected?.Invoke(this, EventArgs.Empty);

        private void OnSockClose(object sender, AsyncSocketConnectionEventArgs e)
            => Disconnected?.Invoke(this, EventArgs.Empty);

        private void OnSockReceive(object sender, AsyncSocketReceiveEventArgs e)
            => DataReceived?.Invoke(this, e.ReceiveData.ToArray());

        private void OnSockError(object sender, AsyncSocketErrorEventArgs e)
            => ErrorOccurred?.Invoke(this, e.AsyncSocketException);

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _sock.OnConnet -= OnSockConnect;
            _sock.OnClose -= OnSockClose;
            _sock.OnReceive -= OnSockReceive;
            _sock.OnError -= OnSockError;

            Disconnect();

            _disposed = true;
        }
    }
}
