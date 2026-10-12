using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using SEOJIN_TMES.Client.Theme;

namespace SEOJIN_TMES.Client.Forms.Common
{
    /// <summary>
    /// 공통 버튼 — <b>옅은 회색 바탕 + 또렷한 테두리 + 좌측 아이콘 + 진한 글자</b>.
    /// 흰 카드 위에 놓였을 때 바탕끼리 겹쳐 보이지 않도록 카드보다 한 톤 낮게 칠한다(<see cref="AppTheme.ButtonQuietBack"/>).
    ///
    /// DevExpress <see cref="SimpleButton"/>을 상속하고, 색은 <see cref="AppTheme.QuietButton"/>과 같은 방식으로
    /// Appearance에 직접 지정한다. 스킨이 배경·테두리를 대신 그려 색이 무시되는 것은
    /// <c>LookAndFeel.Style = Flat</c> + <c>PaintStyle = Light</c>로 막는다(카드 패널에서 쓰는 수법과 같다).
    ///
    /// 직접 그리지 않고 SimpleButton에 얹은 덕에 hover/press 상태, 대화상자 Enter/Esc(<c>IButtonControl</c>),
    /// 툴팁·DPI 스케일링을 DevExpress가 처리한다.
    ///
    /// 아이콘은 <see cref="AppIcons"/>의 SVG 템플릿 문자열을 <see cref="Icon"/>에 넣는다
    /// (예: <c>AppIcons.Search</c>). 넣지 않으면 글자만 가운데에 그린다.
    /// </summary>
    public class MeisButton : SimpleButton
    {
        private const int CornerRadius = 2;
        private const int IconSize = 16;

        /// <summary>아이콘과 글자 사이 간격.</summary>
        private const int IconTextGap = 10;

        /// <summary><see cref="TextAfterIcon"/>일 때 아이콘을 두는 왼쪽 여백.</summary>
        private const int IconLeft = 14;

        private string _icon;
        private bool _textAfterIcon;
        private bool _selected;
        private Color _accent = AppTheme.Indigo;

        public MeisButton()
        {
            Size = new Size(120, 34);
            Cursor = Cursors.Hand;
            ApplyTheme();
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
                ApplyIcon();
            }
        }

        /// <summary>글자가 있는 자리를 아이콘 오른쪽부터로 볼지(false면 아이콘+글자 덩어리를 가운데 정렬).</summary>
        public bool TextAfterIcon
        {
            get => _textAfterIcon;
            set
            {
                if (_textAfterIcon == value)
                    return;

                _textAfterIcon = value;
                ApplyContentLayout();
            }
        }

        /// <summary>
        /// 선택된 상태로 그릴지(탭처럼 여럿 중 하나가 켜져 있는 경우).
        /// 켜면 <see cref="Accent"/>를 옅게 깐 칩 모양이 된다.
        /// </summary>
        public bool Selected
        {
            get => _selected;
            set
            {
                if (_selected == value)
                    return;

                _selected = value;
                ApplyColors();
            }
        }

        /// <summary>선택 상태에서 쓰는 색. 평상시 모양은 이 값과 무관하게 중립 톤이다.</summary>
        public Color Accent
        {
            get => _accent;
            set
            {
                if (_accent == value)
                    return;

                _accent = value;
                ApplyColors();
            }
        }

        /// <summary>
        /// 색·아이콘·배치를 현재 테마로 다시 입힌다.
        /// Appearance에 색을 <b>박아 두는</b> 방식이라 테마를 바꾸면 화면이 이 메서드를 다시 불러 줘야 한다
        /// (직접 그리던 시절에는 Invalidate만으로 따라왔다).
        /// </summary>
        public void ApplyTheme()
        {
            // 스킨이 배경·테두리를 대신 그리면 아래 Appearance 색이 무시된다 — Flat + Light로 막는다.
            LookAndFeel.UseDefaultLookAndFeel = false;
            LookAndFeel.Style = LookAndFeelStyle.Flat;
            PaintStyle = PaintStyles.Light;

            // ButtonStyle은 건드리지 않는다 — PaintStyle=Light가 Appearance.BorderColor로 테두리를 그려 준다.
            // (Simple로 바꾸면 PaintStyle=Default일 때 진한 남색 테두리가 따로 그려진다.)

            // 점선 포커스 사각형은 끈다. 다만 포커스를 가진 버튼의 <b>바깥 테두리는 글자색으로</b> 그려지는데
            // (DevExpress 기본 동작, ShowFocusRectangle과 무관하다) 이건 그대로 둔다 —
            // 직접 그리던 시절에는 포커스가 눈에 보이지 않아 키보드로 조작할 때 어디에 있는지 알 수 없었다.
            ShowFocusRectangle = DefaultBoolean.False;

            ApplyColors();
            ApplyIcon();
        }

        /// <summary>
        /// 상태별 색. 선택 상태여도 글자는 진하게 둔다 —
        /// 바탕 칩이 이미 상태를 말해 주므로 글자까지 물들이면 오히려 읽기 나빠진다.
        /// </summary>
        private void ApplyColors()
        {
            var fore = AppTheme.TextPrimary;

            // 선택된 탭은 포인트 색을 아주 옅게 깐 칩으로 — 글자가 그대로 읽혀야 하므로 진하게 깔지 않는다.
            var selectedBack = AppTheme.Blend(_accent, AppTheme.Card, 0.18f);
            var border = _selected ? _accent : AppTheme.ButtonQuietBorder;

            // 올려놓거나 누르면 테두리만 한 톤 진해진다 — 시안처럼 색을 쓰지 않는다.
            var hotBorder = _selected ? _accent : AppTheme.TextMuted;

            PaintState(Appearance, _selected ? selectedBack : AppTheme.ButtonQuietBack, fore, border);
            PaintState(AppearanceHovered, _selected ? selectedBack : AppTheme.ButtonQuietHover, fore, hotBorder);
            PaintState(AppearancePressed, AppTheme.ButtonQuietPressed, fore, hotBorder);
            PaintState(AppearanceDisabled, AppTheme.ButtonDisabledBack, AppTheme.ButtonDisabledFore, AppTheme.ButtonDisabledBack);
        }

        private static void PaintState(AppearanceObject appearance, Color back, Color fore, Color border)
        {
            appearance.BackColor = back;
            appearance.ForeColor = fore;
            appearance.BorderColor = border;
            appearance.Font = AppTheme.BodyFont;
            appearance.Options.UseBackColor = true;
            appearance.Options.UseForeColor = true;
            appearance.Options.UseBorderColor = true;
            appearance.Options.UseFont = true;
        }

        private void ApplyIcon()
        {
            if (string.IsNullOrEmpty(_icon))
            {
                ImageOptions.SvgImage = null;
                ApplyContentLayout();
                return;
            }

            // 비활성이면 아이콘도 흐려져야 한다 — 색을 SVG에 끼워 넣는 방식이라 상태마다 다시 받아 온다.
            ImageOptions.SvgImage = AppIcons.Get(_icon, Enabled ? AppTheme.TextPrimary : AppTheme.ButtonDisabledFore);
            ImageOptions.SvgImageSize = new Size(IconSize, IconSize);

            // 스킨 팔레트가 아이콘 색을 다시 칠하지 못하게 막는다(AppIcons가 이미 색을 박아 넣었다).
            ImageOptions.SvgImageColorizationMode = SvgImageColorizationMode.None;

            ApplyContentLayout();
        }

        /// <summary>
        /// 아이콘과 글자를 놓는 방식.
        ///
        /// 기본값은 <see cref="ImageAlignToText.LeftCenter"/> — DevExpress가 <b>아이콘과 글자를 한 덩어리로 보고</b>
        /// 그 덩어리를 버튼 가운데에 둔다. 아이콘을 왼쪽에 고정해 놓고 글자만 버튼 전체 기준으로 가운데 정렬하면
        /// 폭이 빠듯한 버튼(104px에 "새로고침")에서 글자가 아이콘에 딱 붙어 "Q새로고침"처럼 보인다.
        /// 덩어리로 묶어 두면 폭·언어와 무관하게 <see cref="IconTextGap"/>만큼 벌어진다
        /// (언어를 바꾸면 같은 버튼에 더 긴 문구가 들어온다 — 예: "조회" → "Tra cứu").
        /// </summary>
        private void ApplyContentLayout()
        {
            ImageOptions.ImageToTextIndent = IconTextGap;
            Appearance.Options.UseTextOptions = true;

            if (_textAfterIcon)
            {
                ImageOptions.ImageToTextAlignment = ImageAlignToText.None;
                ImageOptions.Location = ImageLocation.MiddleLeft;
                Appearance.TextOptions.HAlignment = HorzAlignment.Near;
                Padding = new Padding(IconLeft, 0, 0, 0);
                return;
            }

            ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;
            Appearance.TextOptions.HAlignment = HorzAlignment.Center;
            Padding = Padding.Empty;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            // 폭이 바뀌는 화면이 있어 매번 다시 깎는다 — 한 번만 잘라 두면 잘린 자국이 남는다.
            AppTheme.RoundCorners(this, CornerRadius);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);

            ApplyIcon();
            Cursor = Enabled ? Cursors.Hand : Cursors.Default;
        }
    }
}
