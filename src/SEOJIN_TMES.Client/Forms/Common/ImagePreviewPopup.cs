using System;
using System.Drawing;
using System.Windows.Forms;
using SEOJIN_TMES.Client.Theme;

namespace SEOJIN_TMES.Client.Forms.Common
{
    /// <summary>
    /// 그리드 옆에 띄우는 그림 미리보기 창(ImageBoxEx, 화면 맞춤). <see cref="GridImagePreview"/>가 띄우고 내린다.
    ///  - 마우스 오버: 커서 옆(<see cref="MoveNear"/>)
    ///  - 행 클릭: 그 행 옆(<see cref="MoveBeside"/>)
    ///
    /// 툴팁처럼 동작해야 하므로 떠도 포커스를 가져가지 않는다(ShowWithoutActivation + WS_EX_NOACTIVATE) —
    /// 포커스를 뺏으면 그리드의 마우스 이벤트가 끊기고, 팝업(ShowDialog) 위에서는 소유 창이 비활성으로 보인다.
    /// 그림은 호출 측(<see cref="GridImagePreview"/>의 캐시)이 소유한다 — 여기서 Dispose하지 않는다.
    /// </summary>
    public partial class ImagePreviewPopup : Form
    {
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        /// <summary>커서와 창 사이 간격 — 커서 바로 아래에 붙으면 창이 마우스 이벤트를 가로챈다.</summary>
        private const int CursorGap = 18;

        public ImagePreviewPopup()
        {
            InitializeComponent();
            ApplyTheme();
        }

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;   // 작업 표시줄 · Alt+Tab에 나오지 않고, 클릭해도 활성화되지 않는다
                return cp;
            }
        }

        private void ApplyTheme()
        {
            BackColor = AppTheme.CardBorder;   // Padding(1)으로 비친 바탕이 테두리가 된다
            imageBox.BackColor = AppTheme.Surface;
        }

        /// <summary>그림을 바꿔 끼우고 커서 옆에 띄운다(이미 떠 있으면 위치만 옮긴다). 마우스 오버 미리보기용.</summary>
        public void ShowImage(Image image, Point cursor)
        {
            MoveNear(cursor);
            ShowImage(image);
        }

        /// <summary>그림을 바꿔 끼우고 그리드 행(화면 좌표) 옆에 띄운다. 클릭으로 고정한 미리보기용.</summary>
        public void ShowImageBeside(Image image, Rectangle rowOnScreen)
        {
            MoveBeside(rowOnScreen);
            ShowImage(image);
        }

        /// <summary>위치는 그대로 두고 그림만 바꿔 띄운다 — 위치는 먼저 잡아 둬야 엉뚱한 자리에서 한 번 깜빡이지 않는다.</summary>
        private void ShowImage(Image image)
        {
            // SizeMode.Fit이라 배율은 ImageBox가 창 크기에 맞춰 알아서 정한다.
            if (!ReferenceEquals(imageBox.Image, image))
                imageBox.Image = image;

            if (!Visible)
                Show();
        }

        /// <summary>
        /// 행 오른쪽 끝 바깥에, 위쪽을 행에 맞춰 둔다. 오른쪽이 화면 밖이면 행 왼쪽 바깥으로 넘기고,
        /// 그래도 모자라면(행이 화면 폭만큼 넓으면) 행 아래로 내려 행을 가리지 않게 한다.
        /// </summary>
        public void MoveBeside(Rectangle row)
        {
            var area = Screen.FromRectangle(row).WorkingArea;

            int x = row.Right + CursorGap;
            int y = row.Top;
            if (x + Width > area.Right)
            {
                x = row.Left - CursorGap - Width;
                if (x < area.Left)
                {
                    x = Math.Min(area.Right - Width, Cursor.Position.X + CursorGap);
                    y = row.Bottom + CursorGap;
                    if (y + Height > area.Bottom)
                        y = row.Top - CursorGap - Height;   // 아래가 모자라면 행 위로
                }
            }

            // 화면 아래로 넘치면 위로 올린다(행 높이 기준으로 붙어 있던 위쪽 정렬만 포기).
            if (y + Height > area.Bottom)
                y = area.Bottom - Height;

            x = Math.Max(area.Left, x);
            y = Math.Max(area.Top, y);
            Location = new Point(x, y);
        }

        /// <summary>창을 내리고 그림 참조를 놓는다.</summary>
        public void HidePreview()
        {
            if (Visible)
                Hide();
            imageBox.Image = null;
        }

        /// <summary>
        /// 커서 오른쪽 아래에 둔다. 화면 밖으로 나가면 커서 반대편(왼쪽 · 위)으로 넘긴다 —
        /// 창이 커서를 덮으면 MouseLeave가 걸려 미리보기가 깜빡인다.
        /// </summary>
        public void MoveNear(Point cursor)
        {
            var area = Screen.FromPoint(cursor).WorkingArea;

            int x = cursor.X + CursorGap;
            if (x + Width > area.Right)
                x = cursor.X - CursorGap - Width;

            int y = cursor.Y + CursorGap;
            if (y + Height > area.Bottom)
                y = cursor.Y - CursorGap - Height;

            x = Math.Max(area.Left, x);
            y = Math.Max(area.Top, y);
            Location = new Point(x, y);
        }
    }
}
