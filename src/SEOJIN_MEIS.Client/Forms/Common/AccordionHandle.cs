using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SEOJIN_MEIS.Client.Theme;

namespace SEOJIN_MEIS.Client.Forms.Common
{
    /// <summary>손잡이가 붙는 가장자리. 화살표 방향과 글자 회전이 여기서 갈린다.</summary>
    public enum AccordionEdge
    {
        /// <summary>패널 왼쪽 가장자리에 세로로 붙는다(우측 정보 패널용).</summary>
        Left,

        /// <summary>가로로 붙는다(하단 영역용).</summary>
        Bottom
    }

    /// <summary>
    /// 아코디언 접기/펴기 손잡이 — 가장자리에 붙는 띠에 <b>이름과 화살표</b>를 함께 보여 준다.
    ///
    /// 처음에는 <c>SimpleButton</c>에 화살표 글자만 넣었는데,
    /// 흰 카드 위에 흰 버튼이라 <b>있는지조차 보이지 않았다</b>. 그래서
    ///  · 바탕을 한 톤 어둡게 깔고 안쪽 가장자리에 경계선을 그어 띠로 읽히게 하고,
    ///  · 무엇을 접는지 알 수 있도록 이름을 함께 적는다(세로 배치는 글자를 90° 돌려 그린다).
    ///
    /// <see cref="Expanded"/>는 표시만 바꾼다 — 실제로 접는 일은 화면(PopScreen)이 한다.
    /// </summary>
    public class AccordionHandle : Control
    {
        private const int ChevronMargin = 8;

        private bool _hover;
        private bool _expanded = true;
        private AccordionEdge _edge = AccordionEdge.Left;
        private Color _accent = AppTheme.Violet;

        public AccordionHandle()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint
                     | ControlStyles.UserPaint
                     | ControlStyles.OptimizedDoubleBuffer
                     | ControlStyles.ResizeRedraw, true);

            Width = 26;
            Cursor = Cursors.Hand;
            Font = AppTheme.SmallFont;
        }

        /// <summary>지금 펼쳐져 있는지. 화살표 방향과 색이 이 값에 따라 바뀐다.</summary>
        public bool Expanded
        {
            get => _expanded;
            set { _expanded = value; Invalidate(); }
        }

        public AccordionEdge Edge
        {
            get => _edge;
            set { _edge = value; Invalidate(); }
        }

        /// <summary>접혀 있을 때 쓰는 포인트 색(눈에 띄어야 다시 펼 수 있다는 것을 알아챈다).</summary>
        public Color Accent
        {
            get => _accent;
            set { _accent = value; Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // 접혀 있으면 포인트 색으로, 펼쳐져 있으면 회색 톤으로 — 접힌 쪽이 더 눈에 띄어야 한다.
            var fore = _expanded ? AppTheme.TextMuted : _accent;

            DrawSurface(g);

            if (_edge == AccordionEdge.Left)
                DrawVertical(g, fore);
            else
                DrawHorizontal(g, fore);
        }

        /// <summary>흰 카드 위에서도 띠로 보이도록 바탕을 한 톤 깔고 안쪽에 경계선을 긋는다.</summary>
        private void DrawSurface(Graphics g)
        {
            var back = _hover ? AppTheme.ButtonQuietPressed : AppTheme.GridRowAlt;
            using (var brush = new SolidBrush(back))
                g.FillRectangle(brush, ClientRectangle);

            using (var pen = new Pen(AppTheme.CardBorder))
            {
                if (_edge == AccordionEdge.Left)
                    g.DrawLine(pen, Width - 1, 0, Width - 1, Height - 1);   // 내용 쪽 경계
                else
                    g.DrawLine(pen, 0, 0, Width - 1, 0);
            }
        }

        /// <summary>세로 띠 — 화살표는 위, 이름은 90° 돌려 아래쪽에 채운다.</summary>
        private void DrawVertical(Graphics g, Color fore)
        {
            var chevron = _expanded ? "›" : "‹";
            var chevronBounds = new Rectangle(0, ChevronMargin, Width, 16);
            TextRenderer.DrawText(g, chevron, Font, chevronBounds, fore,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.NoPrefix);

            if (string.IsNullOrEmpty(Text))
                return;

            // 글자를 세로로 세운다 — 아래에서 위로 읽히도록 -90° 회전한다.
            //
            // ⚠️ 여기서는 TextRenderer(GDI)를 쓸 수 없다. TextRenderer는 Graphics의 변환 행렬을 무시해서
            // 회전이 먹지 않고 글자가 엉뚱한 자리(컨트롤 밖)에 그려진다 — 실제로 그렇게 나왔다.
            // 회전된 글자는 GDI+의 DrawString으로 그린다.
            var state = g.Save();
            try
            {
                g.TranslateTransform(Width / 2f, Height - ChevronMargin);
                g.RotateTransform(-90f);

                var textLength = Math.Max(0, Height - ChevronMargin * 2 - 20);
                var layout = new RectangleF(0, -Font.Height / 2f, textLength, Font.Height + 2);

                using (var brush = new SolidBrush(fore))
                using (var format = new StringFormat())
                {
                    format.Alignment = StringAlignment.Near;
                    format.LineAlignment = StringAlignment.Center;
                    format.Trimming = StringTrimming.EllipsisCharacter;
                    format.FormatFlags = StringFormatFlags.NoWrap;

                    g.DrawString(Text, Font, brush, layout, format);
                }
            }
            finally
            {
                g.Restore(state);
            }
        }

        /// <summary>가로 띠 — [이름 + 화살표]를 가운데에 나란히 둔다.</summary>
        private void DrawHorizontal(Graphics g, Color fore)
        {
            var chevron = _expanded ? " ⌄" : " ⌃";
            var caption = (Text ?? string.Empty) + chevron;

            TextRenderer.DrawText(g, caption, Font, ClientRectangle, fore,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix
                | TextFormatFlags.EndEllipsis);
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
            Invalidate();
        }
    }
}
