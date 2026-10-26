using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SEOJIN_TMES.Client.Theme;

namespace SEOJIN_TMES.Client.Forms.Common
{
    /// <summary>
    /// 카드형 타일 — <b>상단 색 띠 + 제목 + 큰 값(강조 색) + 단위 + 보조 문구</b>.
    /// 대시보드 카드(예: "P1 긴급 / 0 건 / 종결 0 · 미조치 0")와 같은 배치로, 눌러서 고르는 타일로도 쓴다.
    ///
    /// DevExpress TileItem은 상단 띠·둥근 테두리·값 옆 단위 같은 배치를 요소만으로 만들 수 없어 직접 그린다.
    /// 누르면 일반 <see cref="Control.Click"/>이 올라오고, 고른 상태(<see cref="Selected"/>)는 호출한 쪽이 켠다.
    /// </summary>
    public class CardTile : Control
    {
        private const int Radius = 8;
        private const int AccentHeight = 4;
        private const int Pad = 16;

        private readonly Font _titleFont = AppTheme.BodyBoldFont;
        private readonly Font _valueFont = AppTheme.CardValueFont;
        private readonly Font _compactValueFont = AppTheme.TitleFont;
        private readonly Font _unitFont = AppTheme.BodyBoldFont;
        private readonly Font _subFont = AppTheme.SmallFont;

        private Color _accent = AppTheme.Indigo;
        private string _title;
        private string _value;
        private string _unit;
        private string _subText;
        private bool _compactValue;
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

        /// <summary>상단 띠와 값의 색.</summary>
        public Color Accent { get => _accent; set { _accent = value; Invalidate(); } }

        public string Title { get => _title; set { _title = value; Invalidate(); } }

        /// <summary>크게 보이는 값(건수 등).</summary>
        public string Value { get => _value; set { _value = value; Invalidate(); } }

        /// <summary>값 오른쪽에 붙는 단위(예: 건). 비우면 그리지 않는다.</summary>
        public string Unit { get => _unit; set { _unit = value; Invalidate(); } }

        public string SubText { get => _subText; set { _subText = value; Invalidate(); } }

        /// <summary>값이 숫자가 아니라 긴 문구(알람 명 등)일 때 — 한 단계 작은 글꼴로 줄여 잘림을 줄인다.</summary>
        public bool CompactValue { get => _compactValue; set { _compactValue = value; Invalidate(); } }

        /// <summary>고른 타일 — 강조 색 테두리 + 옅은 바탕으로 그린다.</summary>
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
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = RoundedRect(bounds, Radius))
            {
                var back = _selected ? AppTheme.Blend(_accent, AppTheme.Card, 0.10f)
                         : _hovered ? AppTheme.Blend(_accent, AppTheme.Card, 0.04f)
                         : AppTheme.Card;
                using (var brush = new SolidBrush(back))
                    g.FillPath(brush, path);

                // 상단 색 띠 — 둥근 카드 모양 안쪽만 칠하도록 경로로 잘라 그린다.
                var clip = g.Clip;
                g.SetClip(path);
                using (var brush = new SolidBrush(_accent))
                    g.FillRectangle(brush, 0, 0, Width, AccentHeight);
                g.Clip = clip;

                var borderColor = _selected ? _accent : AppTheme.CardBorder;
                using (var pen = new Pen(borderColor, _selected ? 2f : 1f))
                    g.DrawPath(pen, path);
            }

            var textWidth = Width - Pad * 2;
            TextRenderer.DrawText(g, _title, _titleFont, new Rectangle(Pad, AccentHeight + 12, textWidth, _titleFont.Height),
                AppTheme.TextPrimary, TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);

            var valueFont = _compactValue ? _compactValueFont : _valueFont;
            var valueTop = AccentHeight + 12 + _titleFont.Height + 6;
            var valueFlags = TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;
            var valueSize = TextRenderer.MeasureText(g, _value ?? string.Empty, valueFont, new Size(textWidth, valueFont.Height), valueFlags);
            var valueWidth = Math.Min(valueSize.Width, textWidth);
            TextRenderer.DrawText(g, _value, valueFont, new Rectangle(Pad, valueTop, valueWidth, valueFont.Height), _accent, valueFlags);

            if (!string.IsNullOrEmpty(_unit) && valueWidth < textWidth)
            {
                // 단위는 값의 아랫줄에 맞춘다(이미지처럼 큰 숫자 옆에 작게 붙는다).
                var unitTop = valueTop + valueFont.Height - _unitFont.Height - 2;
                TextRenderer.DrawText(g, _unit, _unitFont, new Point(Pad + valueWidth + 4, unitTop), AppTheme.TextPrimary, TextFormatFlags.NoPadding);
            }

            TextRenderer.DrawText(g, _subText, _subFont, new Rectangle(Pad, Height - Pad - _subFont.Height + 2, textWidth, _subFont.Height),
                AppTheme.TextMuted, TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
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
                _titleFont.Dispose();
                _valueFont.Dispose();
                _compactValueFont.Dispose();
                _unitFont.Dispose();
                _subFont.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
