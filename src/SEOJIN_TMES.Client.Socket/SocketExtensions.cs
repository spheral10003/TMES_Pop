using System.Net.Sockets;

namespace SEOJIN_TMES.Client.Sockets
{
    internal static class SocketExtensions
    {
        /// <summary>
        /// Socket 확장, 접속지점과의 연결 상태 체크
        /// </summary>
        /// <param name="Sock"></param>
        /// <returns></returns>
        public static bool IsConnected(this Socket Sock)
        {
            if (Sock != null)
            {
                return !(Sock.Poll(1, SelectMode.SelectRead) && Sock.Available == 0);
            }
            else
            {
                return false;
            }
        }
    }
}
