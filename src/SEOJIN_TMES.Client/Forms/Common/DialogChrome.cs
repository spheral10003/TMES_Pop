using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DevExpress.XtraEditors;
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

        // 창 이동은 좌표를 직접 계산하지 않고 OS에 맡긴다 — MainForm이 쓰는 것과 같은 방법이라
        // 끌기 중 화면 경계·다중 모니터 처리가 제목 줄을 잡고 끌 때와 똑같이 동작한다.
        private const int WM_NCLBUTTONDOWN = 0x00A1;
        private const int HTCAPTION = 2;

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// 팝업에서 Windows 제목 줄을 없애고 헤더를 제목 줄처럼 쓰게 한다.
        /// 각 팝업의 <c>Initialize()</c> 끝(테마를 다 입힌 뒤)에서 부른다.
        /// </summary>
        /// <param name="form">팝업 폼.</param>
        /// <param name="header">제목 줄을 대신할 색 띠. 이 위를 끌면 창이 움직인다.</param>
        public static void Apply(XtraForm form, Control header)
        {
            // 테두리 스타일을 바꾸면 바깥 크기(Size)는 그대로인 채 안쪽(ClientSize)이 제목 줄·테두리만큼
            // 넓어진다 — 디자이너에서 잡아 둔 내부 배치가 그만큼 어긋나므로 원래 값으로 되돌린다.
            var client = form.ClientSize;

            form.FormBorderStyle = FormBorderStyle.None;

            // 제목 줄과 함께 창 테두리도 사라진다. 폼 자체를 테두리 색으로 칠하고 Padding만큼만
            // 남기면, 안쪽을 Fill로 채운 자식 덕에 그 부분이 1px 테두리로 보인다.
            form.BackColor = AppTheme.CardBorder;
            form.Padding = new Padding(BorderThickness);
            form.ClientSize = new Size(
                client.Width + BorderThickness * 2,
                client.Height + BorderThickness * 2);

            if (header != null)
                EnableDrag(header);
        }

        /// <summary>
        /// 헤더와 그 안의 <b>클릭을 받지 않는</b> 자식(아이콘·제목 라벨)에 끌기를 붙인다.
        /// 자식까지 붙이지 않으면 제목 글자 위를 잡았을 때 창이 따라오지 않는다.
        /// </summary>
        private static void EnableDrag(Control header)
        {
            Subscribe(header);

            foreach (Control child in header.Controls)
            {
                // 버튼은 제 역할이 있다(헤더의 닫기 X 등).
                if (child is SimpleButton)
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
