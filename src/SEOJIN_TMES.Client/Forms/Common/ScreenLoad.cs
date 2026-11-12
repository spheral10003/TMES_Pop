using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SEOJIN_TMES.Client.Forms.Common
{
    /// <summary>
    /// 화면을 연 직후의 첫 조회를 <b>화면이 다 그려진 뒤</b>에 시작시키는 공용 배선.
    ///
    /// <code>
    /// public void Init()
    /// {
    ///     ...
    ///     ScreenLoad.AfterShown(this, SearchAsync);
    /// }
    /// </code>
    ///
    /// <c>Load</c>에서 바로 조회하면 안 되는 이유: <c>Load</c>는 화면이 <b>처음 그려지기 전</b>에 걸린다.
    /// 그 자리에서 조회를 시작하면 빈 화면(또는 이전 탭의 잔상)이 응답이 올 때까지 남고,
    /// 대기 덮개(BusyOverlay)도 아직 자리가 잡히지 않은 화면 위에 얹혀 제대로 보이지 않는다.
    /// 그래서 화면 틀을 먼저 그려 보여 준 다음 서버를 부른다.
    ///
    /// 탭 화면(UserControl)에는 Form의 <c>Shown</c>이 없다. <c>Load</c>는 MainForm이 탭에 얹는 처리 <b>도중</b>에 걸리므로
    /// <c>BeginInvoke</c>로 그 처리가 끝난 뒤로 보내고, 소유 창을 한 번 그린 다음 조회한다.
    /// </summary>
    public sealed class ScreenLoad
    {
        private readonly Control _screen;
        private readonly Func<Task> _load;

        private ScreenLoad(Control screen, Func<Task> load)
        {
            _screen = screen;
            _load = load;
        }

        /// <summary>탭 화면이 처음 표시되어 그려진 뒤 <paramref name="load"/>를 한 번 실행하도록 배선한다.</summary>
        public static void AfterShown(UserControl screen, Func<Task> load)
        {
            screen.Load += new ScreenLoad(screen, load).Screen_Load;
        }

        /// <summary>
        /// 지금 처리 중인 일이 끝나고 화면이 그려진 뒤 <paramref name="load"/>를 실행한다.
        /// Designer가 <c>Load</c>를 직접 구독하는 화면(PopScreen)은 그 처리기 안에서 이것을 바로 부른다.
        /// </summary>
        public static void AfterPaint(Control screen, Func<Task> load)
        {
            new ScreenLoad(screen, load).Defer();
        }

        private void Screen_Load(object sender, EventArgs e)
        {
            Defer();
        }

        private void Defer()
        {
            // VS 디자이너에서도 Load가 걸린다 — 디자인 화면에서 서버를 부르면 안 된다.
            if (_screen.Site?.DesignMode == true)
                return;

            _screen.BeginInvoke(new Action(Run));
        }

        private async void Run()
        {
            if (_screen.IsDisposed || _screen.Disposing) return;   // 미뤄진 사이에 탭을 닫았다

            _screen.FindForm()?.Update();
            await _load();
        }
    }
}
