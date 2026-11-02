using System;
using System.Drawing;
using System.Windows.Forms;
using SEOJIN_TMES.Client.Common.Imaging;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Theme;

namespace SEOJIN_TMES.Client.Forms.Pop
{
    /// <summary>
    /// POP 도면 전체화면 보기 — PopScreen의 도면(picView)을 더블클릭하면 연다.
    ///
    /// 도면이 떠 있던 모니터 전체를 테두리 없이 덮고, PopScreen과 같은 <see cref="ImageView"/> 규칙
    /// (Zoom In/Out 1.25배 단계 · Fit · Ctrl+휠 · 드래그 이동)을 그대로 쓴다.
    /// 닫기: [닫기] 버튼 · Esc · 도면 더블클릭.
    ///
    /// ⚠️ 그림은 <b>복사본</b>을 받는다. <see cref="ImageView"/>는 받은 Image를 소유하고 해제하므로,
    /// PopScreen의 원본을 그대로 넘기면 이 창을 닫을 때 PopScreen 도면까지 해제되어 빈 화면/예외가 난다.
    /// 또 이 창이 떠 있는 동안에도 PopScreen이 도면을 다시 읽어 원본을 교체·해제할 수 있다.
    /// </summary>
    public partial class DrawingFullScreenForm : Form
    {
        private ImageView _viewer;

        public DrawingFullScreenForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 색 · 문구 · 창 위치를 정하고 그림을 넣는다.
        /// </summary>
        /// <param name="image">보여 줄 그림. 이 창이 소유하고 닫힐 때 해제한다(호출 측 원본을 넘기지 말 것 — 복사본을 넘긴다).</param>
        /// <param name="screen">덮을 모니터(도면이 떠 있던 모니터).</param>
        public void Initialize(Image image, Screen screen)
        {
            // Normal 상태에서 Bounds를 직접 모니터 크기로 잡는다 — 작업 표시줄까지 덮고, 보조 모니터에서도 그 모니터에 뜬다.
            Bounds = screen.Bounds;

            BackColor = AppTheme.ImageViewerBack;
            pnlBar.BackColor = AppTheme.Surface;
            picView.BackColor = AppTheme.ImageViewerBack;
            foreach (var button in new[] { btnZoomIn, btnZoomOut, btnFit, btnClose })
                AppTheme.StandardButton(button, keepFont: true);

            btnClose.Text = Language.Common_Close;

            // ImageBox는 기본값(AllowDoubleClick=false)이면 DoubleClick 이벤트 자체를 올리지 않는다.
            picView.AllowDoubleClick = true;

            _viewer = new ImageView(picView);
            _viewer.SetImage(image);

            btnZoomIn.Click += BtnZoomIn_Click;
            btnZoomOut.Click += BtnZoomOut_Click;
            btnFit.Click += BtnFit_Click;
            btnClose.Click += BtnClose_Click;
            picView.DoubleClick += PicView_DoubleClick;
        }

        /// <summary>창이 떠서 크기가 확정된 뒤 한 번 더 맞춘다 — Initialize 시점의 picView 크기는 디자이너 크기다.</summary>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _viewer?.ZoomToFit();
            picView.Focus();
        }

        /// <summary>그림이 포커스를 가져가도 Esc로 닫히도록 폼 단계에서 먼저 잡는다.</summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // 들고 있던 복사본을 해제한다(PopScreen 원본과는 무관).
            _viewer?.Dispose();
            _viewer = null;
            base.OnFormClosed(e);
        }

        private void BtnZoomIn_Click(object sender, EventArgs e)
        {
            _viewer?.ZoomIn();
        }

        private void BtnZoomOut_Click(object sender, EventArgs e)
        {
            _viewer?.ZoomOut();
        }

        private void BtnFit_Click(object sender, EventArgs e)
        {
            _viewer?.ZoomToFit();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void PicView_DoubleClick(object sender, EventArgs e)
        {
            Close();
        }
    }
}
