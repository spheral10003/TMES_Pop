using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SEOJIN_TMES.Client.Theme;

namespace SEOJIN_TMES.Client.Forms.Common
{
    /// <summary>
    /// 카드형 타일 — <b>상단 색 띠 + 위쪽 작은 코드 + 가운데 큰 글자 + 아래 보조 문구</b>.
    /// 큰 글자가 길 때는 두 가지 중 하나로 맞춘다 — 글자를 줄이거나(<see cref="AutoFitText"/>, 로그인 워크스테이션 카드),
    /// 줄을 바꾸고 카드를 키우거나(<see cref="WrapTitle"/>, 알람 코드 카드).
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

        /// <summary>글자 자동 조절 때 큰 글자를 더 줄이지 않는 하한(px).</summary>
        private const float MinTitlePixel = 11f;

        /// <summary>카드의 기본(최소) 높이 — 큰 글자가 한 줄일 때의 높이다.</summary>
        public const int MinHeight = 104;

        private const TextFormatFlags WrapFlags =
            TextFormatFlags.Left | TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl | TextFormatFlags.NoPadding;

        private Font _captionFont = AppTheme.SmallFont;
        private Font _titleFont = AppTheme.CardTitleFont;
        private Font _subFont = AppTheme.SmallFont;

        private Color _accent = AppTheme.Indigo;
        private string _caption;
        private string _title;
        private string _subText;
        private bool _selected;
        private bool _hovered;
        private bool _autoFitText;
        private bool _wrapTitle;

        public CardTile()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);

            // 포커스를 받지 않는다 — 받으면 스크롤 패널(FlowLayoutPanel.AutoScroll)이 누른 카드를 화면 안으로 끌어오느라
            // 목록을 어중간하게 스크롤해 윗줄 카드의 상단이 잘려 보였다(실측: 3번째 줄 카드를 누르면 40px 스크롤).
            SetStyle(ControlStyles.Selectable, false);
            Size = new Size(200, MinHeight);
            Cursor = Cursors.Hand;
            BackColor = Color.Transparent;
        }

        /// <summary>상단 띠 · 큰 글자 · 선택 표시의 색.</summary>
        public Color Accent { get => _accent; set { _accent = value; Invalidate(); } }

        /// <summary>위쪽 작은 글자(코드).</summary>
        public string Caption { get => _caption; set { _caption = value; Invalidate(); } }

        /// <summary>가운데 큰 글자.</summary>
        public string Title { get => _title; set { _title = value; FitFonts(); Invalidate(); } }

        /// <summary>아래쪽 보조 문구. 비우면 그리지 않는다.</summary>
        public string SubText { get => _subText; set { _subText = value; Invalidate(); } }

        /// <summary>고른 타일 — 옅은 강조 색 바탕 + 강조 색 테두리 + 우상단 체크.</summary>
        public bool Selected { get => _selected; set { _selected = value; Invalidate(); } }

        /// <summary>
        /// 큰 글자가 카드 폭을 넘으면 줄을 바꾼다(끄면 한 줄로 그리고 넘치는 부분은 …로 줄인다).
        /// 서버가 준 이름 · 언어에 따라 길이가 크게 달라지는 알람 명에 쓴다 —
        /// 카드 높이는 <see cref="GetPreferredHeight"/>로 구해 호출한 쪽이 맞춘다.
        /// </summary>
        public bool WrapTitle { get => _wrapTitle; set { _wrapTitle = value; Invalidate(); } }

        /// <summary>줄바꿈한 큰 글자가 시작하는 높이 — 위쪽 코드 바로 아래.</summary>
        private int WrappedTitleTop => AccentHeight + 10 + _captionFont.Height + 8;

        /// <summary>
        /// 이 폭일 때 큰 글자를 줄바꿈해 다 보여 주는 데 필요한 카드 높이(최소 <see cref="MinHeight"/>).
        /// </summary>
        public int GetPreferredHeight(int width)
        {
            var textWidth = Math.Max(1, width - Pad * 2);
            var text = TextRenderer.MeasureText(_title ?? string.Empty, _titleFont, new Size(textWidth, int.MaxValue), WrapFlags);
            return Math.Max(MinHeight, WrappedTitleTop + text.Height + Pad);
        }

        /// <summary>
        /// 켜면 글자 크기를 카드 높이에 맞춰 정하고, 큰 글자가 카드 폭을 넘으면 맞을 때까지 줄인다.
        /// 끄면(기본) 고정 글꼴 — 크기가 정해진 알람 카드는 이 상태로 쓴다.
        /// </summary>
        public bool AutoFitText { get => _autoFitText; set { _autoFitText = value; FitFonts(); Invalidate(); } }

        protected override void OnSizeChanged(EventArgs e)
        {
            FitFonts();
            base.OnSizeChanged(e);
        }

        /// <summary>
        /// 카드 높이 비율로 작은 글자 · 큰 글자 크기를 잡는다(px 단위 — 높이와 바로 비교하려고).
        /// 비율은 세 줄(코드 · 이름 · 보조 문구)이 서로 겹치지 않는 선에서 정했다.
        /// </summary>
        private void FitFonts()
        {
            if (!_autoFitText || Width <= 0 || Height <= 0)
                return;

            var family = _titleFont.FontFamily.Name;
            var smallPixel = Math.Max(10f, Math.Min(18f, Height * 0.14f));
            var titlePixel = Math.Max(MinTitlePixel, Math.Min(40f, Height * 0.26f));

            var title = new Font(family, titlePixel, FontStyle.Bold, GraphicsUnit.Pixel);
            var textWidth = Width - Pad * 2;
            while (titlePixel > MinTitlePixel && !string.IsNullOrEmpty(_title)
                   && TextRenderer.MeasureText(_title, title).Width > textWidth)
            {
                titlePixel -= 1f;
                title.Dispose();
                title = new Font(family, titlePixel, FontStyle.Bold, GraphicsUnit.Pixel);
            }

            ReplaceFonts(new Font(family, smallPixel, FontStyle.Regular, GraphicsUnit.Pixel),
                         title,
                         new Font(family, smallPixel, FontStyle.Regular, GraphicsUnit.Pixel));
        }

        private void ReplaceFonts(Font caption, Font title, Font sub)
        {
            _captionFont.Dispose();
            _titleFont.Dispose();
            _subFont.Dispose();
            _captionFont = caption;
            _titleFont = title;
            _subFont = sub;
        }

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

            // 선택 테두리(2.5px)가 선 가운데 기준으로 그려져 바깥 절반이 잘리지 않도록 1px 안쪽에 그린다.
            var bounds = new Rectangle(1, 1, Width - 3, Height - 3);
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

            var titleColor = _selected ? _accent : AppTheme.TextPrimary;
            if (_wrapTitle)
            {
                // 줄바꿈 — 코드 아래 남은 영역의 세로 가운데에 놓는다(같은 줄 카드끼리 높이를 맞춰도 글자가 위로 쏠리지 않게).
                var top = WrappedTitleTop;
                TextRenderer.DrawText(g, _title, _titleFont,
                    new Rectangle(Pad, top, textWidth, Height - top - Pad), titleColor, WrapFlags | TextFormatFlags.VerticalCenter);
            }
            else
            {
                // 한 줄 — 카드 세로 가운데에 놓는다.
                var titleTop = (Height - _titleFont.Height) / 2 + 2;
                TextRenderer.DrawText(g, _title, _titleFont,
                    new Rectangle(Pad, titleTop, textWidth, _titleFont.Height), titleColor, flags);
            }

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
