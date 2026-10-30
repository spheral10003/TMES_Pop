using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace SEOJIN_TMES.Client.Sockets
{
    public class StateObject
    {
        private const int _BUFFER_SIZE = 327680;

        private Socket _worker;
        private byte[] _buffer;

        public StateObject(Socket worker)
        {
            this._worker = worker;
            this._buffer = new byte[_BUFFER_SIZE];
        }

        public Socket Worker
        {
            get { return this._worker; }
            set { this._worker = value; }
        }

        public byte[] Buffer
        {
            get { return this._buffer; }
            set { this._buffer = value; }
        }

        public int BufferSize
        {
            get { return _BUFFER_SIZE; }
        }
    } // end of class StateObject

    /// <summary>
    /// 비동기 소켓에서 발생한 에러 처리를 위한 이벤트 Argument Class
    /// </summary>
    public class AsyncSocketErrorEventArgs : EventArgs
    {
        private readonly Exception _exception;
        private readonly int _id = 0;

        public AsyncSocketErrorEventArgs(int id, Exception exception)
        {
            this._id = id;
            this._exception = exception;
        }

        public Exception AsyncSocketException
        {
            get { return this._exception; }
        }

        public int ID
        {
            get { return this._id; }
        }
    }

    /// <summary>
    /// 비동기 소켓의 연결 및 연결해제 이벤트 처리를 위한 Argument Class
    /// </summary>
    public class AsyncSocketConnectionEventArgs : EventArgs
    {
        private readonly int _id = 0;

        public AsyncSocketConnectionEventArgs(int id)
        {
            this._id = id;
        }

        public int ID
        {
            get { return this._id; }
        }
    }

    /// <summary>
    /// 비동기 소캣의 데이터 전송 이벤트 처리를 위한 Argument Class
    /// </summary>
    public class AsyncSocketSendEventArgs : EventArgs
    {
        private readonly int _id = 0;
        private readonly int _sendBytes;

        public AsyncSocketSendEventArgs(int id, int sendBytes)
        {
            this._id = id;
            this._sendBytes = sendBytes;
        }

        public int SendBytes
        {
            get { return this._sendBytes; }
        }

        public int ID
        {
            get { return this._id; }
        }
    }

    /// <summary>
    /// 비동기 소켓의 데이터 수신 이벤트 처리를 위한 Argument Class
    /// </summary>
    public class AsyncSocketReceiveEventArgs : EventArgs
    {
        private readonly int _id = 0;
        private readonly int _receiveBytes;
        private readonly byte[] _receiveData;

        public AsyncSocketReceiveEventArgs(int id, int receiveBytes, byte[] receiveData)
        {
            this._id = id;
            this._receiveBytes = receiveBytes;
            this._receiveData = receiveData;
        }

        public int ReceiveBytes
        {
            get { return this._receiveBytes; }
        }

        public byte[] ReceiveData
        {
            get { return this._receiveData; }
        }

        public int ID
        {
            get { return this._id; }
        }
    }

    /// <summary>
    /// 비동기 서버의 Accept 이벤트를 위한 Argument Class
    /// </summary>
    public class AsyncSocketAcceptEventArgs : EventArgs
    {
        private readonly Socket _conn;

        public AsyncSocketAcceptEventArgs(Socket conn)
        {
            this._conn = conn;
        }

        public Socket Worker
        {
            get { return this._conn; }
        }
    }

    ///
    /// delegate 정의
    /// 
    public delegate void AsyncSocketErrorEventHandler(object sender, AsyncSocketErrorEventArgs e);
    public delegate void AsyncSocketConnectEventHandler(object sender, AsyncSocketConnectionEventArgs e);
    public delegate void AsyncSocketCloseEventHandler(object sender, AsyncSocketConnectionEventArgs e);
    public delegate void AsyncSocketSendEventHandler(object sender, AsyncSocketSendEventArgs e);
    public delegate void AsyncSocketReceiveEventHandler(object sender, AsyncSocketReceiveEventArgs e);
    public delegate void AsyncSocketAcceptEventHandler(object sender, AsyncSocketAcceptEventArgs e);

    public class AsyncSocketClass
    {
        protected int _id;

        // Event Handler
        public event AsyncSocketErrorEventHandler OnError;
        public event AsyncSocketConnectEventHandler OnConnet;
        public event AsyncSocketCloseEventHandler OnClose;
        public event AsyncSocketSendEventHandler OnSend;
        public event AsyncSocketReceiveEventHandler OnReceive;
        public event AsyncSocketAcceptEventHandler OnAccept;

        public AsyncSocketClass()
        {
            this._id = -1;
        }

        public AsyncSocketClass(int id)
        {
            this._id = id;
        }

        public int ID
        {
            get { return this._id; }
        }

        protected virtual void ErrorOccured(AsyncSocketErrorEventArgs e)
        {
            AsyncSocketErrorEventHandler handler = OnError;

            if (handler != null)
                handler(this, e);
        }

        protected virtual void Connected(AsyncSocketConnectionEventArgs e)
        {
            AsyncSocketConnectEventHandler handler = OnConnet;

            if (handler != null)
                handler(this, e);
        }

        protected virtual void Closed(AsyncSocketConnectionEventArgs e)
        {
            AsyncSocketCloseEventHandler handler = OnClose;

            if (handler != null)
                handler(this, e);
        }

        protected virtual void Sent(AsyncSocketSendEventArgs e)
        {
            AsyncSocketSendEventHandler handler = OnSend;

            if (handler != null)
                handler(this, e);
        }

        protected virtual void Received(AsyncSocketReceiveEventArgs e)
        {
            AsyncSocketReceiveEventHandler handler = OnReceive;

            if (handler != null)
                handler(this, e);
        }

        protected virtual void Accepted(AsyncSocketAcceptEventArgs e)
        {
            AsyncSocketAcceptEventHandler handler = OnAccept;

            if (handler != null)
                handler(this, e);
        }

    } // end of class AsyncSocketClass

    /// <summary>
    /// 비동기 소켓
    /// </summary>
    public class AsyncSocketClient : AsyncSocketClass
    {
        // connection socket
        private Socket _conn = null;

        /// <summary>
        /// 접속을 기다리는 시간(ms). 0 이하면 OS 기본값(Windows 약 21초)까지 기다린다.
        /// 시간이 지나면 소켓을 닫고 <see cref="TimeoutException"/>을 OnError로 올린다.
        /// </summary>
        public int ConnectTimeoutMs { get; set; }

        /// <summary>
        /// 무통신 상태에서 첫 TCP KeepAlive 확인을 보내기까지의 시간(ms). 0 이하면 KeepAlive를 켜지 않는다(OS 기본 2시간).
        /// 랜선 단선·장비 전원 차단처럼 FIN/RST 없이 끊긴 연결은 이것이 없으면 수신 대기가 끝나지 않아 끊김을 모른다.
        /// </summary>
        public int KeepAliveTimeMs { get; set; }

        /// <summary>KeepAlive 확인에 응답이 없을 때 재확인 간격(ms). Windows는 10회 재확인 후 연결을 끊는다.</summary>
        public int KeepAliveIntervalMs { get; set; }

        /// <summary>접속 시도 1회의 상태. 완료 콜백과 타임아웃 중 먼저 온 쪽만 처리하도록 Status로 겨룬다.</summary>
        private sealed class ConnectState
        {
            public const int Pending = 0;
            public const int Completed = 1;
            public const int TimedOut = 2;

            public Socket Socket;
            public IPEndPoint Remote;   // 닫힌 소켓의 RemoteEndPoint는 예외가 나므로 따로 둔다(타임아웃 메시지용).
            public int Status;
            public RegisteredWaitHandle TimeoutWait;
        }

        public AsyncSocketClient(int id)
        {
            this._id = id;
        }

        public AsyncSocketClient(int id, Socket conn)
        {
            this._id = id;
            this._conn = conn;
        }

        public Socket Connection
        {
            get { return this._conn; }
            set { this._conn = value; }
        }

        public bool IsConnected
        {
            get { return _conn.IsConnected(); }
        }
        /// <summary>
        /// 연결을 시도한다.
        /// </summary>
        /// <param name="hostAddress"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool Connect(string hostAddress, int port)
        {

            try
            {
                IPAddress[] ips = Dns.GetHostAddresses(hostAddress);
                IPEndPoint remoteEP = new IPEndPoint(ips[0], port);
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                ConnectState state = new ConnectState();
                state.Socket = client;
                state.Remote = remoteEP;

                IAsyncResult ar = client.BeginConnect(remoteEP, new AsyncCallback(OnConnectCallback), state);

                // BeginConnect 자체에는 제한 시간이 없어 응답 없는 장비는 OS 기본값(약 21초)까지 붙잡힌다.
                if (ConnectTimeoutMs > 0 && !ar.IsCompleted)
                {
                    state.TimeoutWait = ThreadPool.RegisterWaitForSingleObject(
                        ar.AsyncWaitHandle, new WaitOrTimerCallback(OnConnectTimeout), state, ConnectTimeoutMs, true);
                }
            }
            catch (System.Exception e)
            {
                AsyncSocketErrorEventArgs eev = new AsyncSocketErrorEventArgs(this._id, e);

                ErrorOccured(eev);

                return false;
            }

            return true;

        }

        /// <summary>
        /// 연결 요청 처리 콜백 함수
        /// </summary>
        /// <param name="ar"></param>
        private void OnConnectCallback(IAsyncResult ar)
        {
            ConnectState state = (ConnectState)ar.AsyncState;

            if (state.TimeoutWait != null)
                state.TimeoutWait.Unregister(null);

            // 이미 타임아웃으로 소켓을 닫은 시도다 — 실패는 타임아웃 쪽에서 보고했으므로 여기서는 정리만 한다.
            if (Interlocked.CompareExchange(ref state.Status, ConnectState.Completed, ConnectState.Pending) != ConnectState.Pending)
            {
                try { state.Socket.EndConnect(ar); } catch { /* 닫힌 소켓의 EndConnect 예외 — 이미 보고함 */ }
                return;
            }

            try
            {
                Socket client = state.Socket;

                // 보류 중인 연결을 완성한다.
                client.EndConnect(ar);

                ApplyKeepAlive(client);

                // 이전 소켓이 남아 있으면 정리한다 — 바뀐 뒤라 그 소켓의 수신 실패는 HandleBroken이 무시한다.
                Socket old = Interlocked.Exchange(ref _conn, client);
                if (old != null && old != client)
                {
                    try { old.Close(); } catch { /* 이미 닫힌 소켓 */ }
                }

                // 연결에 성공하였다면, 데이터 수신을 대기한다.
                Receive();

                // 연결 성공 이벤트를 날린다.
                AsyncSocketConnectionEventArgs cev = new AsyncSocketConnectionEventArgs(this._id);

                Connected(cev);
            }
            catch (System.Exception e)
            {
                AsyncSocketErrorEventArgs eev = new AsyncSocketErrorEventArgs(this._id, e);

                ErrorOccured(eev);
            }
        }

        /// <summary>
        /// 접속 제한 시간이 지났을 때 불린다(timedOut=false면 제한 시간 안에 접속이 끝난 것이라 할 일이 없다).
        /// 소켓을 닫으면 대기 중인 BeginConnect가 끝나며 OnConnectCallback이 정리한다.
        /// </summary>
        private void OnConnectTimeout(object stateObject, bool timedOut)
        {
            if (!timedOut)
                return;

            ConnectState state = (ConnectState)stateObject;
            if (Interlocked.CompareExchange(ref state.Status, ConnectState.TimedOut, ConnectState.Pending) != ConnectState.Pending)
                return;

            try { state.Socket.Close(); } catch { /* 이미 닫힌 소켓 */ }

            ErrorOccured(new AsyncSocketErrorEventArgs(this._id,
                new TimeoutException(string.Format("{0} 접속 시간 초과({1}ms)", state.Remote, ConnectTimeoutMs))));
        }

        /// <summary>
        /// TCP KeepAlive를 켠다. 끊김 감지까지 대략 KeepAliveTimeMs + 10 × KeepAliveIntervalMs가 걸린다.
        /// 실패해도 접속은 유지한다 — 감지를 빠르게 할 뿐 통신에 필수는 아니라서 오류로 올리지 않는다.
        /// </summary>
        private void ApplyKeepAlive(Socket client)
        {
            if (KeepAliveTimeMs <= 0)
                return;

            int interval = KeepAliveIntervalMs > 0 ? KeepAliveIntervalMs : KeepAliveTimeMs;

            try
            {
                // SIO_KEEPALIVE_VALS 구조체: { u_long onoff; u_long keepalivetime; u_long keepaliveinterval; } (little-endian)
                byte[] values = new byte[12];
                BitConverter.GetBytes(1u).CopyTo(values, 0);
                BitConverter.GetBytes((uint)KeepAliveTimeMs).CopyTo(values, 4);
                BitConverter.GetBytes((uint)interval).CopyTo(values, 8);
                client.IOControl(IOControlCode.KeepAliveValues, values, null);
            }
            catch (SocketException)
            {
                // 일부 환경(드라이버 · 정책)에서 지원하지 않을 수 있다 — KeepAlive 없이 계속 통신한다.
            }
        }

        /// <summary>
        /// 데이터 수신을 비동기적으로 처리
        /// </summary>
        public void Receive()
        {
            Receive(_conn);
        }

        private void Receive(Socket worker)
        {
            if (worker == null)
                return;

            try
            {
                StateObject so = new StateObject(worker);

                so.Worker.BeginReceive(so.Buffer, 0, so.BufferSize, 0, new AsyncCallback(OnReceiveCallBack), so);
            }
            catch (System.Exception e)
            {
                HandleBroken(worker, e);
            }
        }

        /// <summary>
        /// 데이터 수신 처리 콜백 함수
        /// </summary>
        /// <param name="ar"></param>
        private void OnReceiveCallBack(IAsyncResult ar)
        {
            StateObject so = (StateObject)ar.AsyncState;
            int bytesRead;

            try
            {
                bytesRead = so.Worker.EndReceive(ar);
            }
            catch (System.Exception e)
            {
                // 연결 리셋 · 소켓 오류 — 수신 대기를 다시 걸 수 없으니 끊긴 것으로 처리한다.
                HandleBroken(so.Worker, e);
                return;
            }

            // 0바이트 = 상대가 연결을 닫음(FIN). 여기서 다시 Receive()를 걸면 즉시 0으로 끝나기를 무한 반복하고
            // OnClose도 올라가지 않아, 장비가 끊은 것을 상위가 모른다.
            if (bytesRead == 0)
            {
                HandleBroken(so.Worker, null);
                return;
            }

            byte[] buf = new byte[bytesRead];

            Array.Copy(so.Buffer, buf, bytesRead);

            try
            {
                // 데이터 수신 이벤트를 처리한다.
                Received(new AsyncSocketReceiveEventArgs(this._id, bytesRead, buf));
            }
            catch (System.Exception e)
            {
                // 받는 쪽 처리 오류는 연결과 무관하다 — 알리기만 하고 수신은 계속한다(멈추면 이후 응답을 영영 못 받는다).
                ErrorOccured(new AsyncSocketErrorEventArgs(this._id, e));
            }

            // 다음 읽을 데이터를 처리한다.
            Receive(so.Worker);
        }

        /// <summary>
        /// 송수신 중 소켓이 끊긴 것을 알았을 때 — 오류를 알리고 소켓을 닫은 뒤 OnClose를 올린다.
        /// 지금 쓰고 있는 소켓일 때만 처리한다: 직접 닫았거나(Close) 새 소켓으로 바뀐 뒤 늦게 온 실패는 무시한다.
        /// </summary>
        /// <param name="reason">끊김 원인. 상대가 정상 종료(FIN)한 경우 null.</param>
        private void HandleBroken(Socket worker, Exception reason)
        {
            if (worker == null || Interlocked.CompareExchange(ref _conn, null, worker) != worker)
                return;

            if (reason != null)
                ErrorOccured(new AsyncSocketErrorEventArgs(this._id, reason));

            try { worker.Close(); } catch { /* 이미 닫힌 소켓 */ }

            Closed(new AsyncSocketConnectionEventArgs(this._id));
        }

        /// <summary>
        /// 데이터 송신을 비동기적으로 처리
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public bool Send(byte[] buffer)
        {
            Socket client = _conn;

            if (client == null)
            {
                ErrorOccured(new AsyncSocketErrorEventArgs(this._id, new SocketException((int)SocketError.NotConnected)));
                return false;
            }

            try
            {
                client.BeginSend(buffer, 0, buffer.Length, 0, new AsyncCallback(OnSendCallBack), client);
            }
            catch (System.Exception e)
            {
                HandleBroken(client, e);

                return false;
            }

            return true;
        }

        /// <summary>
        /// 데이터 송신 처리 콜백 함수
        /// </summary>
        /// <param name="ar"></param>
        private void OnSendCallBack(IAsyncResult ar)
        {
            Socket client = (Socket)ar.AsyncState;

            try
            {
                int bytesWritten = client.EndSend(ar);

                AsyncSocketSendEventArgs sev = new AsyncSocketSendEventArgs(this._id, bytesWritten);

                Sent(sev);
            }
            catch (System.Exception e)
            {
                HandleBroken(client, e);
            }
        }

        /// <summary>
        /// 소켓 연결을 종료한다.
        /// </summary>
        /// <remarks>
        /// 소켓을 먼저 떼어 내(_conn = null) 수신 콜백의 "닫힘" 처리와 겹치지 않게 한다.
        /// 끊긴 소켓은 Shutdown이 예외를 던지는데, 예전에는 거기서 멈춰 소켓이 닫히지 않고 OnClose도 올라가지 않았다 —
        /// 이미 끊긴 연결을 정리하는 것이 목적이므로 Shutdown 실패와 상관없이 닫고 OnClose를 올린다.
        /// </remarks>
        public void Close()
        {
            Socket client = Interlocked.Exchange(ref _conn, null);
            if (client == null)
                return;

            try { client.Shutdown(SocketShutdown.Both); } catch { /* 이미 끊긴 소켓 */ }
            try { client.Close(); } catch { /* 이미 닫힌 소켓 */ }

            Closed(new AsyncSocketConnectionEventArgs(this._id));
        }

    } // end of class AsyncSocketClient

    /// <summary>
    /// 비동기 방식의 서버 
    /// </summary>
    public class AsyncSocketServer : AsyncSocketClass
    {
        private const int backLog = 100;

        private int port;
        private Socket listener;

        public AsyncSocketServer(int port)
        {
            this.port = port;
        }

        public int Port
        {
            get { return this.port; }
        }

        public void Listen()
        {
            try
            {
                listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(new IPEndPoint(IPAddress.Any, this.port));
                listener.Listen(backLog);

                StartAccept();
            }
            catch (System.Exception e)
            {
                AsyncSocketErrorEventArgs eev = new AsyncSocketErrorEventArgs(this._id, e);

                ErrorOccured(eev);
            }
        }

        /// <summary>
        /// Client의 접속을 비동기적으로 대기한다.
        /// </summary>
        /// <returns></returns>
        private void StartAccept()
        {
            try
            {
                listener.BeginAccept(new AsyncCallback(OnListenCallBack), listener);
            }
            catch (System.Exception e)
            {
                AsyncSocketErrorEventArgs eev = new AsyncSocketErrorEventArgs(this._id, e);

                ErrorOccured(eev);
            }
        }

        /// <summary>
        /// Client의 비동기 접속을 처리한다.
        /// </summary>
        /// <param name="ar"></param>
        private void OnListenCallBack(IAsyncResult ar)
        {
            try
            {
                Socket listener = (Socket)ar.AsyncState;
                Socket worker = listener.EndAccept(ar);

                // Client를 Accept 했다고 Event를 발생시킨다.
                AsyncSocketAcceptEventArgs aev = new AsyncSocketAcceptEventArgs(worker);

                Accepted(aev);

                // 다시 새로운 클라이언트의 접속을 기다린다.
                StartAccept();
            }
            catch (System.Exception e)
            {
                AsyncSocketErrorEventArgs eev = new AsyncSocketErrorEventArgs(this._id, e);

                ErrorOccured(eev);
            }
        }

        public void Stop()
        {
            try
            {
                if (listener != null)
                {
                    if (listener.IsBound)
                        listener.Close(100);
                }
            }
            catch (System.Exception e)
            {
                AsyncSocketErrorEventArgs eev = new AsyncSocketErrorEventArgs(this._id, e);

                ErrorOccured(eev);
            }
        }

    } // end of class AsyncSocketServer
}
