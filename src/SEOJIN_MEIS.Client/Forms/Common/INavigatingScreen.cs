using System;
using System.Windows.Forms;

namespace SEOJIN_MEIS.Client.Forms.Common
{
    /// <summary>
    /// 같은 탭 안에서 다른 화면으로 넘어가야 하는 화면이 구현하는 계약.
    /// 예: POP 진입 화면(작업지시 조회)에서 [이동]을 누르면 같은 탭이 POP 화면으로 바뀐다.
    /// 탭을 새로 열지 않고 내용만 교체하므로, 화면 스스로 탭을 조작하지 않고 MainForm에 요청만 보낸다.
    /// </summary>
    public interface INavigatingScreen
    {
        event EventHandler<ScreenNavigationEventArgs> NavigateRequested;
    }

    /// <summary>이동 요청 — 새로 표시할 화면과 탭에 쓸 제목.</summary>
    public class ScreenNavigationEventArgs : EventArgs
    {
        public Control Screen { get; }
        public string Title { get; }

        public ScreenNavigationEventArgs(Control screen, string title)
        {
            Screen = screen;
            Title = title;
        }
    }
}
