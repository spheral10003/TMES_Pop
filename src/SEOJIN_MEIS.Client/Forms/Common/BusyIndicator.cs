using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SEOJIN_MEIS.Client.Localization;
using SEOJIN_MEIS.Client.Theme;

namespace SEOJIN_MEIS.Client.Forms.Common
{
    /// <summary>
    /// 조회·처리 중에 띄우는 대기 표시(점 스피너 + "잠시만 기다려 주세요 / 처리 중..." + [취소]).
    ///
    /// DevExpress의 <c>SplashScreenManager</c>/<c>WaitForm</c>을 쓰지 않는 이유는
    /// 이 설치본에 <b>Splash Screen 모듈이 없기 때문</b>이다(Tab &amp; Navigation과 함께 빠져 있다).
    /// 그래서 같은 모양을 직접 그린다.
    ///
    /// 이 컨트롤은 <b>상자 하나</b>일 뿐이고, 화면 위에 얹고 입력을 막는 일은
    /// <see cref="BusyOverlay"/>가 한다.
    /// </summary>
    internal sealed class BusyIndicator : Control
    {
        /// <summary>상자 크기. 첨부 디자인 비율(가로로 긴 직사각형)에 맞춘 값이다.</summary>
        public const int BoxWidth = 260;
        public const int BoxHeight = 84;

        private const int DotCount = 8;
        private const int SpinnerSize = 34;
        private const int SpinnerLeft = 18;
        private const int TextLeft = SpinnerLeft + SpinnerSize + 14;

        /// <summary>점이 한 칸 도는 간격(ms). 8칸이므로 한 바퀴는 약 1초다.</summary>
        private const int TickIntervalMs = 125;

        private readonly Timer _timer = new Timer();
        private int _phase;
        private Rectangle _cancelBounds;
        private bool _cancelHover;

        /// <summary>[취소]를 눌렀을 때. 구독자가 없으면 취소 링크를 그리지 않는다.</summary>
        public event EventHandler CancelRequested;

        /// <summary>취소를 제공할지. 되돌릴 수 없는 처리(저장 등)에서는 false로 둔다.</summary>
        public bool Cancelable { get; set; }

        public BusyIndicator()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint
                     | ControlStyles.UserPaint
                     | ControlStyles.OptimizedDoubleBuffer
                     | ControlStyles.ResizeRedraw, true);

            Size = new Size(BoxWidth, BoxHeight);
            Cursor = Cursors.Default;

            _timer.Interval = TickIntervalMs;
            _timer.Tick += (s, e) =>
            {
                _phase = (_phase + 1) % DotCount;
                Invalidate();
            };
        }

        /// <summary>애니메이션을 돌린다. 보이지 않는 동안 타이머가 돌면 CPU만 쓰므로 붙였다 뗀다.</summary>
        public void StartAnimation() => _timer.Start();

        public void StopAnimation() => _timer.Stop();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _timer.Dispose();

            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            DrawBox(g);
            DrawSpinner(g);
            DrawTexts(g);

            if (Cancelable)
                DrawCancelLink(g);
        }

        /// <summary>흰 배경 + 옅은 테두리의 둥근 상자.</summary>
        private void DrawBox(Graphics g)
        {
            var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = RoundedPath(bounds, 3))
            using (var fill = new SolidBrush(AppTheme.Card))
            using (var pen = new Pen(AppTheme.CardBorder))
            {
                g.FillPath(fill, path);
                g.DrawPath(pen, path);
            }
        }

        /// <summary>
        /// 원형으로 늘어선 점 8개. 현재 위치에서 뒤로 갈수록 흐려지게 그려 도는 것처럼 보인다
        /// (이미지를 돌리지 않으므로 어느 각도에서도 점이 뭉개지지 않는다).
        /// </summary>
        private void DrawSpinner(Graphics g)
        {
            var center = new PointF(SpinnerLeft + SpinnerSize / 2f, Height / 2f);
            var radius = SpinnerSize / 2f - 3f;

            for (var i = 0; i < DotCount; i++)
            {
                // 현재 위상에서 몇 칸 뒤인지 → 그만큼 흐려진다.
                var age = (i - _phase + DotCount) % DotCount;
                var alpha = (int)(255 * (1.0 - age / (double)DotCount) * 0.85) + 30;
                alpha = Math.Min(255, alpha);

                var angle = i * (2 * Math.PI / DotCount) - Math.PI / 2;
                var x = center.X + (float)(Math.Cos(angle) * radius);
                var y = center.Y + (float)(Math.Sin(angle) * radius);

                // 앞선 점일수록 조금 크게 — 진행 방향이 눈에 더 잘 들어온다.
                var size = 3.4f + (DotCount - age) * 0.18f;

                using (var brush = new SolidBrush(Color.FromArgb(alpha, AppTheme.Indigo)))
                    g.FillEllipse(brush, x - size / 2, y - size / 2, size, size);
            }
        }

        private void DrawTexts(Graphics g)
        {
            var titleBounds = new Rectangle(TextLeft, 20, Width - TextLeft - 12, 20);
            TextRenderer.DrawText(g, Strings.Common_PleaseWait, AppTheme.BodyBoldFont, titleBounds,
                AppTheme.TextPrimary, TextFormatFlags.Left | TextFormatFlags.NoPrefix);

            var messageBounds = new Rectangle(TextLeft, 42, Width - TextLeft - 12, 18);
            TextRenderer.DrawText(g, Strings.Common_Loading, AppTheme.SmallFont, messageBounds,
                AppTheme.TextMuted, TextFormatFlags.Left | TextFormatFlags.NoPrefix);
        }

        /// <summary>우측 아래 [취소] 링크. 누를 수 있는 영역을 기억해 두었다가 클릭/커서에 쓴다.</summary>
        private void DrawCancelLink(Graphics g)
        {
            var text = Strings.Common_Cancel;
            var size = TextRenderer.MeasureText(text, AppTheme.SmallFont);

            _cancelBounds = new Rectangle(Width - size.Width - 14, Height - size.Height - 8, size.Width, size.Height);

            var color = _cancelHover ? AppTheme.Rose : AppTheme.Indigo;
            TextRenderer.DrawText(g, text, AppTheme.SmallFont, _cancelBounds, color,
                TextFormatFlags.Left | TextFormatFlags.NoPrefix);

            using (var pen = new Pen(color))
                g.DrawLine(pen, _cancelBounds.Left, _cancelBounds.Bottom - 1, _cancelBounds.Right - 1, _cancelBounds.Bottom - 1);
        }

        private static GraphicsPath RoundedPath(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            var d = radius * 2;

            path.AddArc(bounds.Left, bounds.Top, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Top, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var hover = Cancelable && _cancelBounds.Contains(e.Location);
            if (hover == _cancelHover)
                return;

            _cancelHover = hover;
            Cursor = hover ? Cursors.Hand : Cursors.Default;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (!_cancelHover)
                return;

            _cancelHover = false;
            Cursor = Cursors.Default;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left && Cancelable && _cancelBounds.Contains(e.Location))
                CancelRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
