using System.Drawing;
using System.Windows.Forms;
using SEOJIN_TMES.Client.Theme;

namespace SEOJIN_TMES.Client.Forms.Common
{
    /// <summary>
    /// 마우스 커서 옆에 잠깐 띄우는 그림 미리보기 창(ImageBoxEx, 화면 맞춤).
    /// 그리드 행 위에 마우스를 올렸을 때 <see cref="GridImagePreview"/>가 띄우고 내린다.
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

        /// <summary>그림을 바꿔 끼우고 커서 옆에 띄운다(이미 떠 있으면 위치만 옮긴다).</summary>
        public void ShowImage(Image image, Point cursor)
        {
            // SizeMode.Fit이라 배율은 ImageBox가 창 크기에 맞춰 알아서 정한다.
            if (!ReferenceEquals(imageBox.Image, image))
                imageBox.Image = image;

            MoveNear(cursor);
            if (!Visible)
                Show();
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

            x = System.Math.Max(area.Left, x);
            y = System.Math.Max(area.Top, y);
            Location = new Point(x, y);
        }
    }
}
