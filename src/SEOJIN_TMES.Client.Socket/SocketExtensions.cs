using System;
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
                try
                {
                    return !(Sock.Poll(1, SelectMode.SelectRead) && Sock.Available == 0);
                }
                catch (ObjectDisposedException)
                {
                    // Socket이 이미 dispose된 경우
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
