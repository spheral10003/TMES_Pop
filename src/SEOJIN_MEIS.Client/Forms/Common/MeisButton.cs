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
    public class MeisButton : Control, IButtonControl
    {
        private const int CornerRadius = 2;
        private const int IconSize = 16;
        private const int IconLeft = 14;

        /// <summary>아이콘과 글자 사이 간격.</summary>
        private const int IconTextGap = 10;

        /// <summary>아이콘+글자 덩어리 양옆에 두는 여백.</summary>
        private const int ContentPadding = 6;

        /// <summary>
        /// 폭이 빠듯할 때 쓰는 간격·여백. 글자를 …로 자르기 전에 여기까지 줄여서 버텨 본다
        /// (언어를 바꾸면 같은 버튼에 더 긴 문구가 들어온다 — 예: "조회" → "Tra cứu").
        /// 이 값 밑으로는 내려가지 않으므로 아이콘과 글자가 붙어 보이는 일은 없다.
        /// </summary>
        private const int TightIconTextGap = 4;
        private const int TightContentPadding = 2;

        private string _icon;
        private SvgImage _iconImage;
        private Color _iconColor;
        private bool _hover;
        private bool _pressed;
        private bool _selected;
        private Color _accent = AppTheme.Indigo;

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

        /// <summary>
        /// 선택된 상태로 그릴지(탭처럼 여럿 중 하나가 켜져 있는 경우).
        /// 켜면 <see cref="Accent"/>를 옅게 깐 칩 모양이 된다.
        /// </summary>
        public bool Selected
        {
            get => _selected;
            set { _selected = value; _iconImage = null; Invalidate(); }
        }

        /// <summary>선택 상태에서 쓰는 색. 평상시 모양은 이 값과 무관하게 중립 톤이다.</summary>
        public Color Accent
        {
            get => _accent;
            set { _accent = value; _iconImage = null; Invalidate(); }
        }

        public override string Text
        {
            get => base.Text;
            set { base.Text = value; Invalidate(); }
        }

        // ── IButtonControl ─────────────────────────────────────────────────────
        // 팝업들이 AcceptButton/CancelButton과 DialogResult를 쓴다.
        // 이걸 구현하지 않으면 <b>대화상자에서 Enter/Esc가 먹지 않는다</b> — 현장에서 자주 쓰는 조작이라
        // 단순한 모양 교체로 끝낼 수 없는 부분이다.

        /// <summary>이 버튼을 누르면 대화상자가 돌려줄 결과. 팝업의 [확인]/[닫기]가 쓴다.</summary>
        public DialogResult DialogResult { get; set; } = DialogResult.None;

        /// <summary>기본 버튼(Enter로 눌리는 버튼)으로 지정될 때 호출된다. 표시는 바꾸지 않는다.</summary>
        public void NotifyDefault(bool value)
        {
        }

        /// <summary>클릭한 것과 같게 동작시킨다(Enter/Esc가 이 경로로 들어온다).</summary>
        public void PerformClick()
        {
            if (!Enabled)
                return;

            OnClick(EventArgs.Empty);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var back = ResolveBackColor();
            var border = ResolveBorderColor();
            var fore = ResolveForeColor();

            DrawSurface(g, back, border);
            DrawContent(g, fore);
        }

        /// <summary>
        /// 아이콘과 글자를 놓는다.
        ///
        /// 가운데 정렬(<see cref="TextAfterIcon"/>가 false)일 때는 <b>아이콘과 글자를 한 덩어리로 보고</b>
        /// 그 덩어리를 버튼 가운데에 둔다. 예전에는 아이콘을 왼쪽 14px에 고정해 놓고 글자만 버튼 전체
        /// 기준으로 가운데 정렬해서, 폭이 빠듯한 버튼(104px에 "새로고침")에서는 글자가 아이콘에 딱 붙어
        /// "Q새로고침"처럼 보였다. 이제는 폭·언어와 무관하게 <see cref="IconTextGap"/>만큼 벌어진다.
        /// </summary>
        private void DrawContent(Graphics g, Color fore)
        {
            var text = Text ?? string.Empty;
            var textWidth = text.Length == 0 ? 0 : TextRenderer.MeasureText(text, Font).Width;

            if (string.IsNullOrEmpty(_icon))
            {
                // 아이콘이 없으면 예전 그대로 — 가운데 정렬이거나, 왼쪽부터 채운다.
                DrawText(g, fore, 0, Width - (TextAfterIcon ? IconLeft : 0), centered: !TextAfterIcon);
                return;
            }

            var gap = IconTextGap;
            var padding = ContentPadding;

            if (IconSize + gap + textWidth + padding * 2 > Width)
            {
                gap = TightIconTextGap;
                padding = TightContentPadding;
            }

            var content = IconSize + (textWidth > 0 ? gap + textWidth : 0);

            var iconLeft = TextAfterIcon
                ? IconLeft
                : Math.Max(padding, (Width - content) / 2);

            DrawIcon(g, fore, iconLeft);

            if (textWidth == 0)
                return;

            var textLeft = iconLeft + IconSize + gap;
            DrawText(g, fore, textLeft, Width - textLeft - padding, centered: false);
        }

        private Color ResolveBackColor()
        {
            if (!Enabled)
                return AppTheme.Card;

            if (_pressed)
                return AppTheme.ButtonQuietPressed;

            // 선택된 탭은 포인트 색을 아주 옅게 깐 칩으로 — 글자는 그대로 읽혀야 하므로 진하게 깔지 않는다.
            if (_selected)
                return AppTheme.Blend(_accent, AppTheme.Card, 0.18f);

            return _hover ? AppTheme.GridRowAlt : AppTheme.Card;
        }

        private Color ResolveBorderColor()
        {
            if (!Enabled)
                return AppTheme.ButtonDisabledBack;

            if (_selected)
                return _accent;

            // 올려놓거나 누르면 테두리만 한 톤 진해진다 — 시안처럼 색을 쓰지 않는다.
            return _hover || _pressed ? AppTheme.TextMuted : AppTheme.CardBorder;
        }

        /// <summary>
        /// 글자·아이콘 색. 선택 상태여도 진한 글자를 그대로 둔다 —
        /// 바탕 칩이 이미 상태를 말해 주므로 글자까지 물들이면 오히려 읽기 나빠진다.
        /// </summary>
        private Color ResolveForeColor() =>
            Enabled ? AppTheme.TextPrimary : AppTheme.ButtonDisabledFore;

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

        private void DrawIcon(Graphics g, Color color, int left)
        {
            // 색이 바뀌면(비활성 등) 다시 만든다 — SVG 템플릿에 색을 끼워 넣는 방식이라 캐시가 색에 묶인다.
            if (_iconImage == null || _iconColor != color)
            {
                _iconImage = AppIcons.Get(_icon, color);
                _iconColor = color;
            }

            var top = (Height - IconSize) / 2;
            using (var image = _iconImage.Render(new Size(IconSize, IconSize), null))
                g.DrawImage(image, new Rectangle(left, top, IconSize, IconSize));
        }

        private void DrawText(Graphics g, Color color, int left, int width, bool centered)
        {
            if (string.IsNullOrEmpty(Text) || width <= 0)
                return;

            var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix
                        | (centered ? TextFormatFlags.HorizontalCenter : TextFormatFlags.Left);

            TextRenderer.DrawText(g, Text, Font, new Rectangle(left, 0, width, Height), color, flags);
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

        /// <summary>
        /// <see cref="DialogResult"/>가 지정돼 있으면 누를 때 대화상자 결과를 세운다.
        /// WinForms의 Button이 하는 일을 그대로 옮긴 것으로, 이게 없으면
        /// <c>btnClose.DialogResult = Cancel</c> 같은 지정이 아무 일도 하지 않는다.
        /// </summary>
        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            if (DialogResult == DialogResult.None)
                return;

            var form = FindForm();
            if (form != null)
                form.DialogResult = DialogResult;
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
