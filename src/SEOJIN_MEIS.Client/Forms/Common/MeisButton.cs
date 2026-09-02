using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DevExpress.Utils.Svg;
using SEOJIN_MEIS.Client.Theme;

namespace SEOJIN_MEIS.Client.Forms.Common
{
    /// <summary>
    /// 공통 버튼 — <b>흰 바탕 + 얇은 회색 테두리 + 좌측 아이콘 + 진한 글자</b>.
    ///
    /// DevExpress <c>SimpleButton</c>을 테마로 칠하는 대신 직접 그리는 이유:
    /// SimpleButton은 스킨이 배경·테두리·눌림 효과를 함께 그려서, Appearance로 색만 바꿔도
    /// 시안과 미묘하게 어긋난다(모서리를 Region으로 깎으면 가장자리가 거칠어지는 문제도 있다).
    /// 여기서는 배경·테두리·아이콘·글자를 전부 직접 그리므로 어느 테마에서도 모양이 같다.
    ///
    /// 아이콘은 <see cref="AppIcons"/>의 SVG 템플릿 문자열을 <see cref="Icon"/>에 넣는다
    /// (예: <c>AppIcons.Search</c>). 넣지 않으면 글자만 가운데에 그린다.
    /// </summary>
    public class MeisButton : Control
    {
        private const int CornerRadius = 2;
        private const int IconSize = 16;
        private const int IconLeft = 14;
        private const int IconTextGap = 10;

        private string _icon;
        private SvgImage _iconImage;
        private Color _iconColor;
        private bool _hover;
        private bool _pressed;

        public MeisButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint
                     | ControlStyles.UserPaint
                     | ControlStyles.OptimizedDoubleBuffer
                     | ControlStyles.ResizeRedraw
                     | ControlStyles.SupportsTransparentBackColor, true);

            Size = new Size(120, 34);
            Cursor = Cursors.Hand;
            BackColor = Color.Transparent;
            Font = AppTheme.BodyFont;
        }

        /// <summary>좌측 아이콘(<see cref="AppIcons"/>의 SVG 템플릿). 비우면 글자만 그린다.</summary>
        public string Icon
        {
            get => _icon;
            set
            {
                if (_icon == value)
                    return;

                _icon = value;
                _iconImage = null;   // 색과 함께 다시 만든다
                Invalidate();
            }
        }

        /// <summary>글자가 있는 자리를 아이콘 오른쪽부터로 볼지(false면 버튼 전체 기준으로 가운데 정렬).</summary>
        public bool TextAfterIcon { get; set; }

        public override string Text
        {
            get => base.Text;
            set { base.Text = value; Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var back = ResolveBackColor();
            var border = ResolveBorderColor();
            var fore = Enabled ? AppTheme.TextPrimary : AppTheme.ButtonDisabledFore;

            DrawSurface(g, back, border);

            var textLeft = 0;
            if (!string.IsNullOrEmpty(_icon))
            {
                DrawIcon(g, fore);
                textLeft = IconLeft + IconSize + IconTextGap;
            }

            DrawText(g, fore, textLeft);
        }

        private Color ResolveBackColor()
        {
            if (!Enabled)
                return AppTheme.Card;

            if (_pressed)
                return AppTheme.ButtonQuietPressed;

            return _hover ? AppTheme.GridRowAlt : AppTheme.Card;
        }

        private Color ResolveBorderColor()
        {
            if (!Enabled)
                return AppTheme.ButtonDisabledBack;

            // 올려놓거나 누르면 테두리만 한 톤 진해진다 — 시안처럼 색을 쓰지 않는다.
            return _hover || _pressed ? AppTheme.TextMuted : AppTheme.CardBorder;
        }

        private void DrawSurface(Graphics g, Color back, Color border)
        {
            // 부모 배경이 비쳐야 모서리 바깥이 깔끔하다.
            g.Clear(Parent?.BackColor ?? AppTheme.Surface);

            var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = RoundedPath(bounds, CornerRadius))
            using (var fill = new SolidBrush(back))
            using (var pen = new Pen(border))
            {
                g.FillPath(fill, path);
                g.DrawPath(pen, path);
            }
        }

        private void DrawIcon(Graphics g, Color color)
        {
            // 색이 바뀌면(비활성 등) 다시 만든다 — SVG 템플릿에 색을 끼워 넣는 방식이라 캐시가 색에 묶인다.
            if (_iconImage == null || _iconColor != color)
            {
                _iconImage = AppIcons.Get(_icon, color);
                _iconColor = color;
            }

            var top = (Height - IconSize) / 2;
            using (var image = _iconImage.Render(new Size(IconSize, IconSize), null))
                g.DrawImage(image, new Rectangle(IconLeft, top, IconSize, IconSize));
        }

        private void DrawText(Graphics g, Color color, int textLeft)
        {
            if (string.IsNullOrEmpty(Text))
                return;

            // 기본은 "아이콘을 뺀 나머지 폭의 가운데" — 첨부 시안이 그 배치다.
            var left = TextAfterIcon ? textLeft : 0;
            var bounds = new Rectangle(left, 0, Width - left - (TextAfterIcon ? IconLeft : 0), Height);

            var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix
                        | (TextAfterIcon ? TextFormatFlags.Left : TextFormatFlags.HorizontalCenter);

            TextRenderer.DrawText(g, Text, Font, bounds, color, flags);
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

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _hover = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = false;
            _pressed = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button != MouseButtons.Left)
                return;

            _pressed = true;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _pressed = false;
            Invalidate();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);

            // 비활성이면 아이콘 색도 흐려져야 하므로 다음 그리기에서 다시 만들게 한다.
            _iconImage = null;
            Cursor = Enabled ? Cursors.Hand : Cursors.Default;
            Invalidate();
        }
    }
}
