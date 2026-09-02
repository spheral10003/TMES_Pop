using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SEOJIN_MEIS.Client.Localization;
using SEOJIN_MEIS.Client.Theme;

namespace SEOJIN_MEIS.Client.Forms
{
    /// <summary>
    /// 제목 줄이 없는 창(<see cref="FormBorderStyle.None"/>)에 창 조작 기능을 되돌려 놓는 부분.
    ///
    /// 원래 이 화면은 "POP 단말은 항상 최대화" 전제로 제목 줄을 없앴는데,
    /// 그러면서 최소화 · 크기 조절 · 창 이동이 전부 사라져 사무실 PC에서 쓸 수 없었다.
    /// 제목 줄을 되살리면 화면 높이를 다시 잡아먹으므로, 대신 상단 바가 제목 줄 역할을 하게 만든다.
    ///
    ///   · 최소화 / 최대화 버튼 : 상단 바 우측 끝(<c>btnMinimize</c> · <c>btnMaximize</c>)
    ///   · 창 이동             : 상단 바 빈 곳을 눌러 끌기(최대화 상태면 자동으로 이전 크기로 돌아간다)
    ///   · 최대화 토글         : 상단 바 더블 클릭
    ///   · 크기 조절           : 창 가장자리 4px 띠를 끌기(Padding으로 드러낸 폼 표면)
    /// </summary>
    public partial class MainForm
    {
        private const int WM_NCLBUTTONDOWN = 0x00A1;
        private const int HTCAPTION = 2;

        /// <summary>
        /// 창 가장자리에서 크기 조절이 잡히는 두께.
        /// 폼의 <see cref="Control.Padding"/>과 <b>같은 값이어야 한다</b> —
        /// 이 띠만큼만 폼 표면이 드러나고, 그보다 안쪽은 자식 컨트롤이 마우스를 가져간다.
        /// </summary>
        private const int ResizeBorder = 4;

        /// <summary>창을 이 크기 아래로는 줄이지 못하게 한다(상단 바 요소가 서로 겹치기 시작하는 폭).</summary>
        private static readonly Size MinimumWindowSize = new Size(1100, 700);

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// 창 조작 배선. <see cref="Initialize"/>에서 한 번 호출한다.
        /// 창 가장자리를 마우스로 잡으려면 폼 자신의 표면이 조금이라도 드러나 있어야 하는데,
        /// 자식 컨트롤(sidePanel · mainArea)이 폼을 빈틈없이 덮고 있어서 Padding으로 테두리를 만들어 준다.
        /// </summary>
        private void SetupWindowChrome()
        {
            MinimumSize = MinimumWindowSize;

            // Padding으로 드러나는 4px 테두리는 폼 배경색으로 칠해진다 — 상단 바와 같은 색이라야 창틀처럼 보인다.
            BackColor = AppTheme.TopBar;

            // 최소화/최대화 버튼의 Click은 디자이너에서 이미 연결돼 있다(여기서 또 걸면 두 번 실행된다).

            // 상단 바 자체와, 그 위에 놓인 "누를 일 없는" 라벨들이 제목 줄 노릇을 한다.
            // 버튼·콤보는 제외해야 클릭이 먹는다.
            EnableWindowDrag(panelTop);
            EnableWindowDrag(lblBreadcrumb);
            EnableWindowDrag(lblSessionTime);
            EnableWindowDrag(lblWelcome);

            // 가장자리 크기 조절 — Padding으로 드러난 4px 띠 위에서만 마우스 이벤트가 폼까지 온다.
            MouseDown += Border_MouseDown;
            MouseMove += Border_MouseMove;
            MouseUp += Border_MouseUp;
            MouseLeave += Border_MouseLeave;

            // 작업 표시줄에서 복원하거나 Win+↓ 로 상태가 바뀌는 길도 있으므로 버튼 클릭에만 기대지 않는다.
            _lastWindowState = WindowState;
            Resize += MainForm_WindowStateWatch;

            ApplyWindowStatePresentation();
        }

        private FormWindowState _lastWindowState;

        private void MainForm_WindowStateWatch(object sender, EventArgs e)
        {
            if (WindowState == _lastWindowState)
                return;

            _lastWindowState = WindowState;

            if (WindowState != FormWindowState.Minimized)
                ApplyWindowStatePresentation();
        }

        /// <summary>해당 컨트롤을 누른 채 끌면 창이 따라 움직이도록 한다.</summary>
        private void EnableWindowDrag(Control control)
        {
            control.MouseDown += TitleArea_MouseDown;
            control.DoubleClick += TitleArea_DoubleClick;
        }

        private void TitleArea_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            // 최대화 상태에서 끌면 윈도우 기본 동작과 같게 "이전 크기로 돌아가면서" 커서를 따라오게 한다.
            // 그냥 복원만 하면 창이 커서에서 멀리 떨어진 곳에 나타나 놓친 것처럼 보인다.
            if (WindowState == FormWindowState.Maximized)
                RestoreUnderCursor();

            // 이동은 OS에 맡긴다 — 화면 가장자리 스냅·다중 모니터 처리가 그대로 따라온다.
            ReleaseCapture();
            SendMessage(Handle, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero);
        }

        private void TitleArea_DoubleClick(object sender, EventArgs e) => ToggleMaximize();

        private void btnMinimize_Click(object sender, EventArgs e) => WindowState = FormWindowState.Minimized;

        private void btnMaximize_Click(object sender, EventArgs e) => ToggleMaximize();

        private void ToggleMaximize()
        {
            if (WindowState == FormWindowState.Maximized)
            {
                RestoreToNormal();
            }
            else
            {
                // 최대화 한계는 "지금 창이 있는 모니터"의 작업 영역이다.
                // 생성자에서 주 모니터로 한 번 잡아 두지만, 창을 옮긴 뒤 최대화하면 그 값이 틀리므로 다시 계산한다.
                MaximizedBounds = Screen.FromControl(this).WorkingArea;
                WindowState = FormWindowState.Maximized;
            }

            ApplyWindowStatePresentation();
        }

        /// <summary>
        /// 최대화 → 보통 크기로 돌리면서, 커서가 상단 바에서 잡고 있던 위치를 그대로 유지한다.
        /// (가로는 창 폭 대비 비율, 세로는 창 위쪽부터의 거리를 보존한다.)
        /// </summary>
        private void RestoreUnderCursor()
        {
            var cursor = Control.MousePosition;
            var ratioX = Width > 0 ? (cursor.X - Left) / (double)Width : 0.5;
            var offsetY = cursor.Y - Top;

            RestoreToNormal();

            Location = new Point((int)(cursor.X - Width * ratioX), cursor.Y - offsetY);
        }

        private bool _normalBoundsInitialized;

        /// <summary>
        /// 보통 크기로 되돌린다.
        /// 이 창은 처음부터 최대화 상태로 만들어져서 "이전 크기"라는 것이 아직 없다 —
        /// 그대로 두면 디자이너 기본값(1280×800)이 화면 밖에 걸친 채 나타나므로 처음 한 번만 정해 준다.
        /// </summary>
        private void RestoreToNormal()
        {
            WindowState = FormWindowState.Normal;
            ApplyWindowStatePresentation();

            if (_normalBoundsInitialized)
                return;

            _normalBoundsInitialized = true;

            var work = Screen.FromControl(this).WorkingArea;
            var size = new Size(
                Math.Max(MinimumWindowSize.Width, (int)(work.Width * 0.8)),
                Math.Max(MinimumWindowSize.Height, (int)(work.Height * 0.85)));

            Bounds = new Rectangle(
                work.X + (work.Width - size.Width) / 2,
                work.Y + (work.Height - size.Height) / 2,
                size.Width,
                size.Height);
        }

        /// <summary>
        /// 창 상태에 맞춰 겉모습을 맞춘다.
        /// 최대화일 때는 크기 조절 테두리가 필요 없으므로 Padding을 없애 화면을 한 픽셀도 낭비하지 않고,
        /// 최대화 버튼 아이콘은 "최대화 ↔ 복원"으로 바꿔 지금 상태를 알려 준다.
        /// </summary>
        private void ApplyWindowStatePresentation()
        {
            var maximized = WindowState == FormWindowState.Maximized;

            Padding = maximized
                ? new Padding(0)
                : new Padding(ResizeBorder);

            AppTheme.IconButton(
                btnMaximize,
                maximized ? AppIcons.WindowRestore : AppIcons.WindowMaximize,
                AppTheme.TopBarFore,
                16);

            btnMaximize.ToolTip = maximized ? Strings.MainForm_Restore : Strings.MainForm_Maximize;
        }

        // ────────────────────────────── 가장자리 크기 조절 ──────────────────────────────
        //
        // 창 이동은 OS에 맡기지만(WM_NCLBUTTONDOWN + HTCAPTION), 크기 조절은 직접 계산한다.
        // WM_NCHITTEST로 가장자리를 알려 주는 방법은 창에 사이징 테두리(WS_THICKFRAME)가 있어야 확실히 먹는데,
        // FormBorderStyle.None 창에는 그 스타일이 없어 동작이 환경을 탄다.
        // Padding으로 드러난 테두리는 <b>폼 자신의 표면</b>이라 마우스 이벤트가 그대로 들어오므로,
        // 그 위에서 Bounds를 직접 고치는 편이 예측 가능하다.

        [Flags]
        private enum ResizeEdge
        {
            None = 0,
            Left = 1,
            Right = 2,
            Top = 4,
            Bottom = 8
        }

        private ResizeEdge _resizeEdge;
        private Point _resizeStartCursor;
        private Rectangle _resizeStartBounds;

        /// <summary>커서가 놓인 지점이 어느 가장자리인지 — 최대화 상태에서는 크기를 바꿀 수 없으니 항상 None.</summary>
        private ResizeEdge EdgeAt(Point client)
        {
            if (WindowState != FormWindowState.Normal)
                return ResizeEdge.None;

            var edge = ResizeEdge.None;

            if (client.X <= ResizeBorder) edge |= ResizeEdge.Left;
            else if (client.X >= ClientSize.Width - ResizeBorder) edge |= ResizeEdge.Right;

            if (client.Y <= ResizeBorder) edge |= ResizeEdge.Top;
            else if (client.Y >= ClientSize.Height - ResizeBorder) edge |= ResizeEdge.Bottom;

            return edge;
        }

        private static Cursor CursorFor(ResizeEdge edge)
        {
            if (edge == (ResizeEdge.Left | ResizeEdge.Top) || edge == (ResizeEdge.Right | ResizeEdge.Bottom))
                return Cursors.SizeNWSE;

            if (edge == (ResizeEdge.Right | ResizeEdge.Top) || edge == (ResizeEdge.Left | ResizeEdge.Bottom))
                return Cursors.SizeNESW;

            if (edge == ResizeEdge.Left || edge == ResizeEdge.Right)
                return Cursors.SizeWE;

            if (edge == ResizeEdge.Top || edge == ResizeEdge.Bottom)
                return Cursors.SizeNS;

            return Cursors.Default;
        }

        private void Border_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            _resizeEdge = EdgeAt(e.Location);
            if (_resizeEdge == ResizeEdge.None)
                return;

            _resizeStartCursor = PointToScreen(e.Location);
            _resizeStartBounds = Bounds;
        }

        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            if (_resizeEdge == ResizeEdge.None)
            {
                Cursor = CursorFor(EdgeAt(e.Location));
                return;
            }

            var cursor = PointToScreen(e.Location);
            var dx = cursor.X - _resizeStartCursor.X;
            var dy = cursor.Y - _resizeStartCursor.Y;

            var x = _resizeStartBounds.X;
            var y = _resizeStartBounds.Y;
            var w = _resizeStartBounds.Width;
            var h = _resizeStartBounds.Height;

            if ((_resizeEdge & ResizeEdge.Left) != 0) { x += dx; w -= dx; }
            if ((_resizeEdge & ResizeEdge.Right) != 0) { w += dx; }
            if ((_resizeEdge & ResizeEdge.Top) != 0) { y += dy; h -= dy; }
            if ((_resizeEdge & ResizeEdge.Bottom) != 0) { h += dy; }

            // 최소 크기에 걸리면 "잡고 있는 쪽"만 멈춰야 한다 — 반대쪽 모서리는 제자리에 있어야
            // 계속 끌었을 때 창이 슬금슬금 밀려가지 않는다.
            if (w < MinimumWindowSize.Width)
            {
                if ((_resizeEdge & ResizeEdge.Left) != 0)
                    x = _resizeStartBounds.Right - MinimumWindowSize.Width;
                w = MinimumWindowSize.Width;
            }

            if (h < MinimumWindowSize.Height)
            {
                if ((_resizeEdge & ResizeEdge.Top) != 0)
                    y = _resizeStartBounds.Bottom - MinimumWindowSize.Height;
                h = MinimumWindowSize.Height;
            }

            Bounds = new Rectangle(x, y, w, h);
        }

        private void Border_MouseUp(object sender, MouseEventArgs e)
        {
            _resizeEdge = ResizeEdge.None;
        }

        private void Border_MouseLeave(object sender, EventArgs e)
        {
            if (_resizeEdge == ResizeEdge.None)
                Cursor = Cursors.Default;
        }
    }
}
