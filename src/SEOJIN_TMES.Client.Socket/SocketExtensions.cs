using System;
using System.Net.Sockets;

namespace SEOJIN_TMES.Client.Sockets
{
    internal static class SocketExtensions
    {
        /// <summary>
        /// Socket 확장, 접속지점과의 연결 상태 체크
        /// </summary>
        /// <remarks>
        /// 이 검사로 알 수 있는 것은 "상대가 보낸 종료(FIN) · 리셋(RST)이 이미 도착했는가"뿐이다.
        /// 랜선 단선 · 장비 전원 차단처럼 아무 신호 없이 끊긴 연결(half-open)은 true로 나온다 —
        /// 그런 끊김은 Alive 응답 시간 초과(NutrunnerSequence)와 TCP KeepAlive로 잡는다.
        /// 끊긴 소켓에서는 Poll이 SocketException을 던질 수 있어 "끊김"으로 본다(호출하는 시퀀스 스레드가 죽지 않게).
        /// </remarks>
        public static bool IsConnected(this Socket Sock)
        {
            // Connected는 마지막 송수신 결과라 실제 상태보다 늦지만, 이미 false면 Poll까지 갈 필요가 없다.
            if (Sock == null || !Sock.Connected)
            {
                return false;
            }

            try
            {
                // 읽을 수 있다(SelectRead)는데 읽을 데이터가 0이면 상대가 연결을 닫은 것이다.
                // 대기 시간은 0 — 상태만 보면 되고, 50ms 주기로 불리므로 기다릴 이유가 없다.
                if (Sock.Poll(0, SelectMode.SelectError))
                {
                    return false;
                }

                return !(Sock.Poll(0, SelectMode.SelectRead) && Sock.Available == 0);
            }
            catch (ObjectDisposedException)
            {
                // Socket이 이미 dispose된 경우
                return false;
            }
            catch (SocketException)
            {
                // 끊긴 소켓에 대한 Poll · Available 오류
                return false;
            }
        }
    }
}
