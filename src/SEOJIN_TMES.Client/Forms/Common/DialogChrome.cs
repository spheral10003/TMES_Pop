using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Theme;

namespace SEOJIN_TMES.Client.Forms.Common
{
    /// <summary>
    /// 팝업 창틀 — Windows 제목 줄을 걷어내고, 화면 안의 색 띠 헤더가 그 역할을 대신하게 한다.
    ///
    /// 제목 줄을 없애는 이유 두 가지:
    ///  1) 같은 제목이 제목 줄과 헤더에 <b>두 번</b> 나와, 창이 겹쳐 뜬 것처럼 보였다.
    ///  2) 모달 팝업이 떠 있는 동안 뒤쪽 화면을 누르면 Windows가 제목 줄을 <b>흔들어</b> 알린다.
    ///     표준 동작이지만 전체 화면 POP에서는 고장처럼 읽힌다 — 흔들 제목 줄이 없으면 일어나지 않는다.
    ///
    /// 제목 줄이 하던 일 중 <b>끌어서 이동</b>은 여기서 헤더에 다시 붙인다.
    /// 닫기는 각 팝업이 이미 가진 [닫기] 버튼과 Esc(<c>CancelButton</c>)가 맡는다
    /// (공용 메시지 창은 헤더 안의 X를 되살려 쓴다 — <see cref="AppMessagePanel.ShowCloseButton"/>).
    /// </summary>
    internal static class DialogChrome
    {
        /// <summary>창 경계가 옅은 배경에 묻히지 않도록 남기는 테두리 두께.</summary>
        private const int BorderThickness = 1;

        /// <summary>업무 팝업 창의 공통 크기(테두리 포함). 팝업마다 제각각이던 크기를 하나로 맞췄다.</summary>
        public static readonly Size PopupSize = new Size(1200, 700);

        // 창 이동은 좌표를 직접 계산하지 않고 OS에 맡긴다 — MainForm이 쓰는 것과 같은 방법이라
        // 끌기 중 화면 경계·다중 모니터 처리가 제목 줄을 잡고 끌 때와 똑같이 동작한다.
        private const int WM_NCLBUTTONDOWN = 0x00A1;
        private const int HTCAPTION = 2;
        private const int HTBOTTOMRIGHT = 17;

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// 팝업에서 Windows 제목 줄을 없애고 헤더를 제목 줄처럼 쓰게 한다.
        /// 각 팝업의 <c>Initialize()</c> 끝(테마를 다 입힌 뒤)에서 부른다.
        /// </summary>
        /// <param name="form">팝업 폼(XtraForm · 일반 .NET Form 모두).</param>
        /// <param name="header">제목 줄을 대신할 색 띠. 이 위를 끌면 창이 움직인다.</param>
        /// <param name="clickables">띠 안에 있지만 끌기가 아니라 클릭을 받아야 하는 컨트롤(라벨로 만든 X 닫기 등).
        /// 끌기를 붙이면 누르는 순간 창 이동이 시작돼 Click이 오지 않는다.</param>
        public static void Apply(Form form, Control header, params Control[] clickables)
        {
            // 테두리 스타일을 바꾸면 바깥 크기(Size)는 그대로인 채 안쪽(ClientSize)이 제목 줄·테두리만큼
            // 넓어진다 — 디자이너에서 잡아 둔 내부 배치가 그만큼 어긋나므로 원래 값으로 되돌린다.
            var client = form.ClientSize;

            form.FormBorderStyle = FormBorderStyle.None;

            // 제목 줄과 함께 창 테두리도 사라진다. 폼 자체를 테두리 색으로 칠하고 Padding만큼만
            // 남기면, 안쪽을 Fill로 채운 자식 덕에 그 부분이 1px 테두리로 보인다.
            form.BackColor = AppTheme.PopupBorder;
            form.Padding = new Padding(BorderThickness);
            form.ClientSize = new Size(
                client.Width + BorderThickness * 2,
                client.Height + BorderThickness * 2);

            if (header != null)
                EnableDrag(header, clickables);
        }

        /// <summary>
        /// 팝업을 부모 창 가운데에 놓는다(부모가 없으면 주 모니터 작업 영역 가운데). 각 팝업의 <c>CenterOn</c>이 이것을 부른다.
        ///
        /// 예전에는 팝업마다 <c>ownerForm.Bounds</c> 기준으로 직접 계산했는데,
        /// <b>부모 창이 최소화된 상태</b>면 Bounds가 (-32000, -32000, 160×28) 근처라 팝업이 화면 밖에 놓였다.
        /// 모달이라 부모 창은 입력이 막힌 채로, 사용자가 창을 복원해도 팝업은 화면 밖에 남아 있어
        /// "마우스 · 키보드가 아무것도 안 먹는" 상태가 됐다(설비 연결 실패 알림처럼 최소화 중에 뜨는 창이 주로 걸린다).
        /// 그래서 최소화 상태면 복원될 위치(RestoreBounds)를 기준으로 삼고, 결과도 화면 작업 영역 안으로 끌어 넣는다.
        /// </summary>
        public static void CenterOn(Form dialog, IWin32Window owner)
        {
            var ownerForm = (owner as Control)?.FindForm();

            Rectangle bounds;
            if (ownerForm == null || ownerForm.IsDisposed)
                bounds = Screen.PrimaryScreen.WorkingArea;
            else if (ownerForm.WindowState == FormWindowState.Minimized)
                bounds = ownerForm.RestoreBounds;
            else
                bounds = ownerForm.Bounds;

            var location = new Point(
                bounds.Left + ((bounds.Width - dialog.Width) / 2),
                bounds.Top + ((bounds.Height - dialog.Height) / 2));

            dialog.Location = ClampToWorkingArea(new Rectangle(location, dialog.Size)).Location;
        }

        /// <summary>
        /// 창이 어느 모니터에도 걸치지 않으면(화면 밖) 가장 가까운 모니터의 작업 영역 안으로 옮긴다.
        /// 창이 작업 영역보다 크면 왼쪽 위를 맞춘다 — 헤더(끌어서 옮기는 곳)와 닫기 버튼이 보이는 쪽을 남긴다.
        /// </summary>
        public static Rectangle ClampToWorkingArea(Rectangle rect)
        {
            var work = Screen.FromRectangle(rect).WorkingArea;

            var x = Math.Max(work.Left, Math.Min(rect.Left, work.Right - rect.Width));
            var y = Math.Max(work.Top, Math.Min(rect.Top, work.Bottom - rect.Height));

            return new Rectangle(x, y, rect.Width, rect.Height);
        }

        /// <summary>
        /// 부모 창이 다시 보이게 됐을 때(최소화 → 복원) 떠 있는 모달 팝업을 화면 안 · 맨 앞으로 되돌린다.
        ///
        /// 모달이 떠 있는 동안 부모 창은 입력이 막혀 있으므로, 팝업이 화면 밖에 있거나 부모 뒤로 가려지면
        /// 작업자 입장에서는 "화면이 멈춘 것"과 구분이 안 된다. 어떤 경로로 그렇게 됐든 여기서 한 번 더 바로잡는다.
        /// </summary>
        public static void BringModalsIntoView()
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form.IsDisposed || !form.Visible || !form.Modal)
                    continue;

                var clamped = ClampToWorkingArea(form.Bounds);
                if (clamped.Location != form.Location)
                {
                    AppLog.Warn($"[DIALOG] 화면 밖에 있던 모달({form.GetType().Name}, {form.Location})을 화면 안으로 옮긴다");
                    form.Location = clamped.Location;
                }

                form.BringToFront();
                form.Activate();
            }
        }

        /// <summary>
        /// 팝업 창 크기를 <see cref="PopupSize"/>(테두리 포함 바깥 크기)로 맞춘다.
        /// <see cref="Apply"/>가 테두리 두께만큼 ClientSize를 늘리므로 그 <b>뒤에</b> 부른다.
        /// 공용 메시지 창(AppMessageForm)은 내용에 맞춰 크기가 정해지므로 부르지 않는다.
        /// </summary>
        public static void FitPopupSize(Form form)
        {
            form.Size = PopupSize;
        }

        /// <summary>
        /// 제목 줄을 없앤 창(FormBorderStyle.None)에 크기 조절 손잡이를 붙인다 — 창 우하단에 두는 작은 컨트롤.
        /// 창틀이 없어 가장자리를 잡고 늘릴 수 없으므로, 이 손잡이를 끌면 OS가 창틀 우하단을 끌 때와 같이 크기를 바꾼다.
        /// 최소 크기는 폼의 <c>MinimumSize</c>가 지킨다.
        /// </summary>
        public static void EnableResizeGrip(Control grip)
        {
            grip.Cursor = Cursors.SizeNWSE;
            grip.MouseDown -= OnGripMouseDown;
            grip.MouseDown += OnGripMouseDown;
            grip.Paint -= OnGripPaint;
            grip.Paint += OnGripPaint;
        }

        private static void OnGripMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            var form = ((Control)sender).FindForm();
            if (form == null)
                return;

            ReleaseCapture();
            SendMessage(form.Handle, WM_NCLBUTTONDOWN, (IntPtr)HTBOTTOMRIGHT, IntPtr.Zero);
        }

        /// <summary>윈도우 표준 크기 조절 손잡이(점 무늬)를 그린다.</summary>
        private static void OnGripPaint(object sender, PaintEventArgs e)
        {
            var grip = (Control)sender;
            ControlPaint.DrawSizeGrip(e.Graphics, grip.BackColor, grip.ClientRectangle);
        }

        /// <summary>
        /// 헤더와 그 안의 <b>클릭을 받지 않는</b> 자식(아이콘·제목 라벨)에 끌기를 붙인다.
        /// 자식까지 붙이지 않으면 제목 글자 위를 잡았을 때 창이 따라오지 않는다.
        /// </summary>
        private static void EnableDrag(Control header, Control[] clickables)
        {
            Subscribe(header);

            foreach (Control child in header.Controls)
            {
                // 버튼은 제 역할이 있다(헤더의 닫기 X 등).
                if (child is SimpleButton || child is Button || Array.IndexOf(clickables, child) >= 0)
                    continue;

                Subscribe(child);
            }
        }

        /// <summary>테마를 다시 입히며 두 번 불려도 핸들러가 쌓이지 않게 한 번 떼고 붙인다.</summary>
        private static void Subscribe(Control control)
        {
            control.MouseDown -= OnHeaderMouseDown;
            control.MouseDown += OnHeaderMouseDown;
        }

        private static void OnHeaderMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            var form = ((Control)sender).FindForm();
            if (form == null)
                return;

            ReleaseCapture();
            SendMessage(form.Handle, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero);
        }
    }
}
