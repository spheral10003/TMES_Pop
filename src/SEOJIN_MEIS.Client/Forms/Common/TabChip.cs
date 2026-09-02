using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SEOJIN_MEIS.Client.Theme;

namespace SEOJIN_MEIS.Client.Forms.Common
{
    /// <summary>
    /// 상단 탭 스트립의 탭 하나. 브라우저/IDE의 탭처럼 <b>윗 모서리만 둥근 사각형</b>이며,
    /// 아랫변은 각져서 스트립 하단 경계선 위에 그대로 얹힌다.
    ///
    /// 예전에는 Panel + Label 2개에 <c>Region</c>으로 알약 모양을 오려 썼는데,
    /// Region은 안티에일리어싱이 없어 곡선이 계단처럼 보이고 닫기(✕) 글자까지 잘려서 모양이 어색했다.
    /// 여기서는 컨트롤 하나가 직접 그린다 — 배경/글자/닫기 버튼을 한 번에 안티에일리어싱으로 그리고,
    /// 마우스 위치로 탭 본체와 ✕ 영역을 구분해 각각 hover 표시를 준다.
    ///
    /// 색은 <see cref="AppTheme"/>에서만 가져온다(앱 포인트 컬러를 그대로 따른다).
    /// </summary>
    public class TabChip : Control
    {
        /// <summary>탭 높이. 스트립 높이(32)에서 아래 여백을 뺀 값과 맞춘다.</summary>
        public const int TabHeight = 24;

        private const int CornerRadius = 4;
        private const int TextLeftPadding = 14;
        private const int CloseButtonSize = 18;
        private const int CloseButtonRightMargin = 8;
        private const int MinimumWidth = 110;
        private const int MaximumWidth = 240;

        private bool _hover;
        private bool _closeHover;
        private bool _active;

        /// <summary>이 탭이 나타내는 화면 키(메뉴 노드 키).</summary>
        public string Key { get; }

        /// <summary>탭 클릭(활성화 요청).</summary>
        public event EventHandler Activated;

        /// <summary>✕ 클릭(닫기 요청). 닫을 수 없는 탭에서는 발생하지 않는다.</summary>
        public event EventHandler CloseRequested;

        /// <summary>
        /// 닫기(✕)를 보여줄지. 홈 탭처럼 항상 열려 있어야 하는 탭은 <c>false</c>로 만든다 —
        /// ✕를 그리지 않고 그 자리만큼 폭도 줄인다.
        /// </summary>
        public bool Closable { get; }

        public TabChip(string key, string text, bool closable = true)
        {
            Key = key;
            Closable = closable;

            SetStyle(ControlStyles.AllPaintingInWmPaint
                     | ControlStyles.UserPaint
                     | ControlStyles.OptimizedDoubleBuffer
                     | ControlStyles.ResizeRedraw, true);

            Text = text;
            Height = TabHeight;
            Margin = new Padding(0, 0, 4, 0);
            Cursor = Cursors.Hand;
            Font = AppTheme.BodyFont;
            Width = MeasureWidth(text);
        }

        /// <summary>선택된 탭인지. 바뀌면 다시 그린다.</summary>
        public bool Active
        {
            get => _active;
            set
            {
                if (_active == value)
                    return;

                _active = value;
                Font = value ? AppTheme.BodyBoldFont : AppTheme.BodyFont;
                Width = MeasureWidth(Text);
                Invalidate();
            }
        }

        /// <summary>제목이 바뀌면 폭도 다시 계산한다(화면 전환으로 탭 제목이 바뀌는 경우).</summary>
        public override string Text
        {
            get => base.Text;
            set
            {
                base.Text = value;
                Width = MeasureWidth(value);
                Invalidate();
            }
        }

        /// <summary>글자 길이에 맞춘 탭 폭(너무 길면 잘라서 … 로 표시한다).</summary>
        private int MeasureWidth(string text)
        {
            var textWidth = TextRenderer.MeasureText(text ?? string.Empty, AppTheme.BodyBoldFont).Width;
            var width = Closable
                ? TextLeftPadding + textWidth + 8 + CloseButtonSize + CloseButtonRightMargin
                : TextLeftPadding + textWidth + TextLeftPadding;

            // 닫기가 없는 탭은 최소 폭도 줄인다 — 짧은 제목("홈")에 빈 공간이 남지 않도록.
            var minimum = Closable ? MinimumWidth : MinimumWidth / 2;
            return Math.Max(minimum, Math.Min(MaximumWidth, width));
        }

        /// <summary>✕ 버튼 영역(가운데 정렬). 닫을 수 없는 탭은 빈 영역이라 어디를 눌러도 활성화가 된다.</summary>
        private Rectangle CloseBounds =>
            Closable
                ? new Rectangle(Width - CloseButtonSize - CloseButtonRightMargin,
                                (Height - CloseButtonSize) / 2,
                                CloseButtonSize,
                                CloseButtonSize)
                : Rectangle.Empty;

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 탭 바깥(둥근 모서리 밖)은 스트립 배경색이 그대로 보여야 한다.
            g.Clear(Parent != null ? Parent.BackColor : AppTheme.Surface);

            var back = _active ? AppTheme.TabActiveBack
                     : _hover ? AppTheme.TabHoverBack
                     : AppTheme.TabInactiveBack;

            // 아래쪽은 스트립 경계선과 맞물리도록 컨트롤 밖으로 살짝 넘겨 그린다(아랫변 곡선 제거).
            using (var path = TopRoundedPath(new Rectangle(0, 0, Width - 1, Height + CornerRadius), CornerRadius))
            using (var brush = new SolidBrush(back))
            {
                g.FillPath(brush, path);

                if (!_active)
                {
                    using (var pen = new Pen(AppTheme.TabBorder))
                        g.DrawPath(pen, path);
                }
            }

            var foreColor = _active ? Color.White : AppTheme.TextMuted;

            var textRightPadding = Closable
                ? CloseButtonSize + CloseButtonRightMargin + 4
                : TextLeftPadding;

            var textBounds = new Rectangle(
                TextLeftPadding, 0,
                Width - TextLeftPadding - textRightPadding, Height);

            TextRenderer.DrawText(g, Text, Font, textBounds, foreColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis
                | TextFormatFlags.NoPrefix);

            if (Closable)
                DrawCloseButton(g, foreColor);
        }

        /// <summary>✕ 를 글꼴 대신 선 두 개로 그린다(작은 크기에서 글리프보다 또렷하다).</summary>
        private void DrawCloseButton(Graphics g, Color foreColor)
        {
            var bounds = CloseBounds;

            if (_closeHover)
            {
                var highlight = _active
                    ? Color.FromArgb(60, 255, 255, 255)
                    : AppTheme.TabBorder;

                using (var brush = new SolidBrush(highlight))
                    g.FillEllipse(brush, bounds);
            }

            var cross = Rectangle.Inflate(bounds, -6, -6);
            using (var pen = new Pen(foreColor, 1.3f))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                g.DrawLine(pen, cross.Left, cross.Top, cross.Right, cross.Bottom);
                g.DrawLine(pen, cross.Right, cross.Top, cross.Left, cross.Bottom);
            }
        }

        /// <summary>윗 모서리만 둥근 경로. 아랫변은 각지게 둔다(탭이 경계선 위에 앉은 모양).</summary>
        private static GraphicsPath TopRoundedPath(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            var d = radius * 2;

            path.AddArc(bounds.Left, bounds.Top, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Top, d, d, 270, 90);
            path.AddLine(bounds.Right, bounds.Bottom, bounds.Left, bounds.Bottom);
            path.CloseFigure();
            return path;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var overClose = CloseBounds.Contains(e.Location);
            if (overClose == _closeHover && _hover)
                return;

            _hover = true;
            _closeHover = overClose;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            _hover = false;
            _closeHover = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button != MouseButtons.Left)
                return;

            if (CloseBounds.Contains(e.Location))
                CloseRequested?.Invoke(this, EventArgs.Empty);
            else
                Activated?.Invoke(this, EventArgs.Empty);
        }
    }
}
