using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SEOJIN_TMES.Client.Theme;

namespace SEOJIN_TMES.Client.Forms.Common
{
    /// <summary>
    /// 카드형 타일 — <b>상단 색 띠 + 위쪽 작은 코드 + 가운데 큰 글자 + 아래 보조 문구</b>.
    /// 대시보드 등급 카드 시안과 같은 모양으로, 눌러서 고르는 타일로 쓴다.
    ///
    /// 고른 상태(<see cref="Selected"/>)는 강조 색으로 옅게 칠하고 테두리를 두르며 우상단에 체크 표시를 단다 —
    /// 색만으로는 비슷한 색 카드끼리 구분이 어려워 체크로 한 번 더 보여 준다.
    /// DevExpress TileItem은 상단 띠·둥근 테두리·체크 배지 배치를 요소만으로 만들 수 없어 직접 그린다.
    /// 누르면 일반 <see cref="Control.Click"/>이 올라오고, 고른 상태는 호출한 쪽이 켠다.
    /// </summary>
    public class CardTile : Control
    {
        private const int Radius = 8;
        private const int AccentHeight = 4;
        private const int Pad = 14;
        private const int CheckSize = 22;

        private readonly Font _captionFont = AppTheme.SmallFont;
        private readonly Font _titleFont = AppTheme.CardTitleFont;
        private readonly Font _subFont = AppTheme.SmallFont;

        private Color _accent = AppTheme.Indigo;
        private string _caption;
        private string _title;
        private string _subText;
        private bool _selected;
        private bool _hovered;

        public CardTile()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
            Size = new Size(200, 104);
            Cursor = Cursors.Hand;
            BackColor = Color.Transparent;
        }

        /// <summary>상단 띠 · 큰 글자 · 선택 표시의 색.</summary>
        public Color Accent { get => _accent; set { _accent = value; Invalidate(); } }

        /// <summary>위쪽 작은 글자(코드).</summary>
        public string Caption { get => _caption; set { _caption = value; Invalidate(); } }

        /// <summary>가운데 큰 글자.</summary>
        public string Title { get => _title; set { _title = value; Invalidate(); } }

        /// <summary>아래쪽 보조 문구. 비우면 그리지 않는다.</summary>
        public string SubText { get => _subText; set { _subText = value; Invalidate(); } }

        /// <summary>고른 타일 — 옅은 강조 색 바탕 + 강조 색 테두리 + 우상단 체크.</summary>
        public bool Selected { get => _selected; set { _selected = value; Invalidate(); } }

        protected override void OnMouseEnter(EventArgs e)
        {
            _hovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hovered = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = RoundedRect(bounds, Radius))
            {
                var back = AppTheme.Card;
                if (_selected)
                    back = AppTheme.Blend(_accent, AppTheme.Card, 0.14f);
                else if (_hovered)
                    back = AppTheme.Blend(_accent, AppTheme.Card, 0.05f);

                using (var brush = new SolidBrush(back))
                    g.FillPath(brush, path);

                // 상단 색 띠 — 둥근 카드 모양 안쪽만 칠하도록 경로로 잘라 그린다.
                var clip = g.Clip;
                g.SetClip(path);
                using (var brush = new SolidBrush(_accent))
                    g.FillRectangle(brush, 0, 0, Width, _selected ? AccentHeight + 2 : AccentHeight);
                g.Clip = clip;

                using (var pen = new Pen(_selected ? _accent : AppTheme.CardBorder, _selected ? 2.5f : 1f))
                    g.DrawPath(pen, path);
            }

            var textWidth = Width - Pad * 2;
            var flags = TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;

            // 위쪽 코드 — 선택 체크가 들어갈 자리만큼 오른쪽을 비운다.
            TextRenderer.DrawText(g, _caption, _captionFont,
                new Rectangle(Pad, AccentHeight + 10, textWidth - CheckSize, _captionFont.Height), AppTheme.TextMuted, flags);

            // 가운데 큰 글자 — 카드 세로 가운데에 놓는다.
            var titleTop = (Height - _titleFont.Height) / 2 + 2;
            TextRenderer.DrawText(g, _title, _titleFont,
                new Rectangle(Pad, titleTop, textWidth, _titleFont.Height), _selected ? _accent : AppTheme.TextPrimary, flags);

            if (!string.IsNullOrEmpty(_subText))
            {
                TextRenderer.DrawText(g, _subText, _subFont,
                    new Rectangle(Pad, Height - Pad - _subFont.Height + 4, textWidth, _subFont.Height), AppTheme.TextMuted, flags);
            }

            if (_selected)
                DrawCheck(g);
        }

        /// <summary>우상단 체크 배지(강조 색 원 + 흰 체크).</summary>
        private void DrawCheck(Graphics g)
        {
            var circle = new Rectangle(Width - Pad - CheckSize + 4, AccentHeight + 8, CheckSize, CheckSize);
            using (var brush = new SolidBrush(_accent))
                g.FillEllipse(brush, circle);

            using (var pen = new Pen(Color.White, 2.4f))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                pen.LineJoin = LineJoin.Round;
                g.DrawLines(pen, new[]
                {
                    new PointF(circle.Left + CheckSize * 0.27f, circle.Top + CheckSize * 0.52f),
                    new PointF(circle.Left + CheckSize * 0.44f, circle.Top + CheckSize * 0.68f),
                    new PointF(circle.Left + CheckSize * 0.74f, circle.Top + CheckSize * 0.35f)
                });
            }
        }

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            var d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(r.Left, r.Top, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Top, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.Left, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _captionFont.Dispose();
                _titleFont.Dispose();
                _subFont.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
