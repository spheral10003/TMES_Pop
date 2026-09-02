using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.Utils;
using DevExpress.XtraBars.Navigation;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraLayout;
using DevExpress.XtraTreeList;

namespace SEOJIN_MEIS.Client.Theme
{
    /// <summary>
    /// 애플리케이션 공통 테마(색/폰트/컨트롤 스타일)를 한 곳에 모아둔 지점.
    /// 화면 코드는 색 값을 직접 쓰지 않고 이 클래스의 메서드를 호출하기만 한다 —
    /// 톤을 바꾸고 싶으면 여기 색 상수만 고치면 전체 화면에 반영된다.
    ///
    /// DevExpress 기능을 두 층으로 쓴다:
    ///  1) 스킨 + SVG 팔레트 : <see cref="ApplySkin"/>가 Basic 스킨의 Violet 팔레트(라이트/다크)를 적용한다.
    ///     에디터·그리드·스크롤바 등 기본 컨트롤 톤이 팔레트에 맞춰 한 번에 바뀐다.
    ///  2) Appearance 재정의 : 좌측 사이드바/상단바/카드/그리드 헤더처럼 디자인 시안 색을 정확히 맞춰야 하는
    ///     부분은 각 컨트롤의 Appearance(BackColor/BackColor2/GradientMode/ForeColor/Font)로 직접 지정한다.
    ///
    /// 주의: 이 메서드들은 커스텀 static 호출이므로 *.Designer.cs의 InitializeComponent()에서 부르면
    /// VS 디자이너가 깨진다. 반드시 각 폼/화면의 Initialize()(런타임)에서 호출할 것.
    /// </summary>
    public static class AppTheme
    {
        // ── 사이드바(다크 인디고) : 라이트/다크 모드와 무관하게 항상 진한 남보라 톤을 유지한다 ──
        public static readonly Color SidebarTop = Color.FromArgb(43, 38, 118);      // #2B2676
        public static readonly Color SidebarBottom = Color.FromArgb(62, 46, 152);   // #3E2E98
        public static readonly Color SidebarBrand = Color.FromArgb(33, 29, 92);     // #211D5C
        public static readonly Color SidebarText = Color.FromArgb(226, 224, 247);   // #E2E0F7
        public static readonly Color SidebarTextMuted = Color.FromArgb(178, 173, 224);
        public static readonly Color SidebarHover = Color.FromArgb(78, 68, 175);
        public static readonly Color SidebarSelected = Color.FromArgb(108, 99, 224); // #6C63E0

        // ── 포인트 컬러 ──
        public static readonly Color Indigo = Color.FromArgb(79, 70, 229);   // #4F46E5
        public static readonly Color Violet = Color.FromArgb(124, 58, 237);  // #7C3AED
        public static readonly Color Cyan = Color.FromArgb(6, 182, 212);     // #06B6D4
        public static readonly Color Teal = Color.FromArgb(0, 158, 179);     // #009EB3 (공통 메시지 창 확인 버튼)
        public static readonly Color Emerald = Color.FromArgb(16, 185, 129); // #10B981
        public static readonly Color Amber = Color.FromArgb(245, 158, 11);   // #F59E0B
        public static readonly Color Rose = Color.FromArgb(244, 63, 94);     // #F43F5E
        public static readonly Color Slate = Color.FromArgb(71, 85, 105);    // #475569

        // ── 콘솔(설비 로그) 색 ──
        public static readonly Color ConsoleBack = Color.FromArgb(23, 23, 43);
        public static readonly Color ConsoleFore = Color.FromArgb(134, 239, 172);

        // ── 내보내기 문서(엑셀) 색 ──
        // 화면 색과 달리 다크 모드를 따라가면 안 된다 — 어두운 테마로 쓰던 중에 내보냈다고
        // 검은 배경 엑셀이 나오면 인쇄·공유가 곤란하다. 그래서 라이트 톤 고정이다.
        public static readonly Color ExportHeaderBack = Color.FromArgb(238, 236, 248); // #EEECF8

        /// <summary>현재 다크 모드 여부. <see cref="ApplySkin"/>가 갱신한다.</summary>
        public static bool IsDarkMode { get; private set; }

        // ── 모드에 따라 바뀌는 색(본문 영역) ──
        public static Color Surface => IsDarkMode ? Color.FromArgb(28, 27, 46) : Color.FromArgb(244, 246, 251);
        public static Color Card => IsDarkMode ? Color.FromArgb(38, 37, 61) : Color.White;
        public static Color CardBorder => IsDarkMode ? Color.FromArgb(58, 56, 90) : Color.FromArgb(228, 231, 240);
        public static Color TopBar => IsDarkMode ? Color.FromArgb(33, 32, 54) : Color.White;
        public static Color TextPrimary => IsDarkMode ? Color.FromArgb(233, 233, 245) : Color.FromArgb(30, 33, 56);
        public static Color TextMuted => IsDarkMode ? Color.FromArgb(160, 158, 190) : Color.FromArgb(115, 122, 145);
        public static Color GridRowAlt => IsDarkMode ? Color.FromArgb(45, 44, 70) : Color.FromArgb(247, 248, 253);

        // ── 커스텀 그리드(MeisGridView) 색 ──
        // 진한 색을 꽉 채운 헤더 대신 아주 옅은 회색 헤더 + 컬럼별 색 점을 쓴다.
        // 색 면적을 줄이면 헤더보다 데이터가 먼저 눈에 들어온다.
        /// <summary>그리드 헤더 배경(옅은 회색).</summary>
        public static Color GridHeaderBack => IsDarkMode ? Color.FromArgb(46, 45, 72) : Color.FromArgb(246, 247, 251);

        /// <summary>그리드 구분선(행 사이 얇은 가로선).</summary>
        public static Color GridLine => IsDarkMode ? Color.FromArgb(58, 56, 90) : Color.FromArgb(230, 233, 241);

        // ── 상단 탭 ── 선택 탭은 앱 포인트 컬러(Indigo), 나머지는 배경보다 한 톤 진한 회색 ──
        public static Color TabActiveBack => Indigo;
        public static Color TabInactiveBack => IsDarkMode ? Color.FromArgb(40, 39, 62) : Color.FromArgb(236, 239, 246);
        public static Color TabHoverBack => IsDarkMode ? Color.FromArgb(50, 48, 78) : Color.FromArgb(225, 229, 240);
        public static Color TabBorder => IsDarkMode ? Color.FromArgb(58, 56, 90) : Color.FromArgb(214, 219, 232);

        // ── 폰트 ──
        private const string FontFamily = "맑은 고딕";
        public static Font BrandFont => new Font(FontFamily, 12F, FontStyle.Bold);
        public static Font BrandSubFont => new Font(FontFamily, 7.5F, FontStyle.Regular);
        public static Font TitleFont => new Font(FontFamily, 11.5F, FontStyle.Bold);
        public static Font SectionFont => new Font(FontFamily, 10F, FontStyle.Bold);
        public static Font BodyFont => new Font(FontFamily, 9.5F, FontStyle.Regular);
        public static Font BodyBoldFont => new Font(FontFamily, 9.5F, FontStyle.Bold);
        public static Font SmallFont => new Font(FontFamily, 8.5F, FontStyle.Regular);
        public static Font MonoFont => new Font("Consolas", 9F, FontStyle.Regular);

        /// <summary>POP 화면의 워크스테이션/공정 상태 띠처럼 멀리서도 읽혀야 하는 큰 글자.</summary>
        public static Font BannerFont => new Font(FontFamily, 14F, FontStyle.Bold);

        /// <summary>
        /// 전역 스킨 + SVG 팔레트를 적용한다(DevExpress 벡터 스킨 기능).
        /// Basic 스킨의 VioletLight/VioletDark 팔레트를 쓰면 시안의 남보라 톤과 자연스럽게 이어진다.
        /// </summary>
        public static void ApplySkin(bool dark)
        {
            IsDarkMode = dark;
            UserLookAndFeel.Default.SetSkinStyle(dark
                ? SkinSvgPalette.DefaultSkin.VioletDark
                : SkinSvgPalette.DefaultSkin.VioletLight);
        }

        /// <summary>좌측 사이드바 배경(수직 그라데이션).</summary>
        public static void Sidebar(PanelControl panel)
        {
            panel.BorderStyle = BorderStyles.NoBorder;
            panel.Appearance.BackColor = SidebarTop;
            panel.Appearance.BackColor2 = SidebarBottom;
            panel.Appearance.GradientMode = LinearGradientMode.Vertical;
            panel.Appearance.Options.UseBackColor = true;
        }

        /// <summary>사이드바 상단 로고 블록.</summary>
        public static void BrandPanel(PanelControl panel)
        {
            panel.BorderStyle = BorderStyles.NoBorder;
            panel.Appearance.BackColor = SidebarBrand;
            panel.Appearance.Options.UseBackColor = true;
        }

        public static void BrandTitle(LabelControl label)
        {
            label.Appearance.Font = BrandFont;
            label.Appearance.ForeColor = Color.White;
            label.Appearance.Options.UseFont = true;
            label.Appearance.Options.UseForeColor = true;
        }

        public static void BrandSubTitle(LabelControl label)
        {
            label.Appearance.Font = BrandSubFont;
            label.Appearance.ForeColor = Color.FromArgb(168, 162, 224);
            label.Appearance.Options.UseFont = true;
            label.Appearance.Options.UseForeColor = true;
        }

        // ── 좌측 메뉴 계층 표현 값 ──
        /// <summary>깊이별 들여쓰기(px). 0=루트, 1=대분류, 2=화면.</summary>
        private static readonly int[] MenuIndents = { 4, 18, 36 };

        /// <summary>깊이별 행 높이(px). 아래로 갈수록 조금씩 낮아져 계층이 눈에 들어온다.</summary>
        private static readonly int[] MenuHeights = { 44, 40, 34 };

        /// <summary>화면(리프) 항목이 대분류 아래에 "들어가 있는" 느낌을 주는 살짝 어두운 배경.</summary>
        public static readonly Color SidebarLeafBack = Color.FromArgb(34, 29, 88); // #221D58

        /// <summary>트리 계층 안내선(리프 앞의 └ 가지) 색.</summary>
        public static readonly Color SidebarGuide = Color.FromArgb(120, 112, 190);

        /// <summary>좌측 메뉴(AccordionControl)를 사이드바 톤에 맞춘다.</summary>
        public static void Accordion(AccordionControl accordion)
        {
            accordion.ItemHeight = MenuHeights[2];
            accordion.GroupHeight = MenuHeights[1];
            accordion.DistanceBetweenRootGroups = 6;
            accordion.AllowItemSelection = true;
            accordion.AnimationType = AnimationType.Office2016;
            accordion.ShowGroupExpandButtons = true;
            accordion.ScrollBarMode = ScrollBarMode.AutoCollapse;

            // 컨트롤 배경은 반드시 불투명 색으로 지정한다 —
            // Color.Transparent를 주면 사이드바 그라데이션이 비치지 않고 검게 칠해진다(실측 확인).
            accordion.Appearance.AccordionControl.BackColor = SidebarTop;
            accordion.Appearance.AccordionControl.BackColor2 = SidebarBottom;
            accordion.Appearance.AccordionControl.GradientMode = LinearGradientMode.Vertical;
            accordion.Appearance.AccordionControl.Options.UseBackColor = true;

            StyleAccordionElement(accordion.Appearance.Group, SidebarText, BodyBoldFont);
            StyleAccordionElement(accordion.Appearance.Item, SidebarTextMuted, BodyFont);
            StyleAccordionElement(accordion.Appearance.ItemWithContainer, SidebarTextMuted, BodyFont);
        }

        private static void StyleAccordionElement(AccordionControlElementAppearances appearances, Color fore, Font font)
        {
            Paint(appearances.Normal, Color.Transparent, fore, font);
            Paint(appearances.Hovered, SidebarHover, Color.White, font);
            Paint(appearances.Pressed, SidebarSelected, Color.White, font);
        }

        /// <summary>
        /// 메뉴 항목 하나에 "깊이별 표현"을 적용한다 — 들여쓰기 · 행 높이 · 글꼴 · 글자색 · 배경.
        /// 깊이가 눈에 보이게 만드는 규칙:
        ///  - 0 (루트, MEIS)   : 들여쓰기 없음, 가장 큰 행, 흰 굵은 글씨
        ///  - 1 (대분류)       : 한 단 들여쓰기, 밝은 글씨 + 굵게 (펼침 화살표는 컨트롤이 그린다)
        ///  - 2 (화면=리프)    : 두 단 들여쓰기, 옅은 글씨 + 살짝 어두운 배경 밴드(대분류 아래에 들어간 느낌)
        /// </summary>
        public static void MenuElement(AccordionControlElement element, int level, bool isLeaf)
        {
            var depth = Math.Max(0, Math.Min(level, MenuIndents.Length - 1));

            element.HeaderIndent = MenuIndents[depth];
            element.Height = MenuHeights[depth];

            var font = isLeaf ? BodyFont : BodyBoldFont;
            var fore = depth == 0 ? Color.White : (isLeaf ? SidebarTextMuted : SidebarText);
            var back = isLeaf ? SidebarLeafBack : Color.Transparent;

            Paint(element.Appearance.Normal, back, fore, font);
            Paint(element.Appearance.Hovered, SidebarHover, Color.White, font);
            Paint(element.Appearance.Pressed, SidebarSelected, Color.White, font);
        }

        /// <summary>선택된 화면 항목을 보라색 밴드로 강조한다(선택 해제 시 <paramref name="selected"/>=false).</summary>
        public static void MenuElementSelected(AccordionControlElement element, bool selected)
        {
            var back = selected ? SidebarSelected : SidebarLeafBack;
            var fore = selected ? Color.White : SidebarTextMuted;
            Paint(element.Appearance.Normal, back, fore, selected ? BodyBoldFont : BodyFont);
        }

        /// <summary>상단바(밝은 배경 + 아래쪽 구분선 느낌).</summary>
        public static void TopBarPanel(PanelControl panel)
        {
            panel.BorderStyle = BorderStyles.NoBorder;
            panel.Appearance.BackColor = TopBar;
            panel.Appearance.Options.UseBackColor = true;
            panel.Padding = new Padding(0, 0, 0, 1);
        }

        /// <summary>브레드크럼(홈 / 대분류 / 화면) 텍스트.</summary>
        public static void Breadcrumb(LabelControl label)
        {
            label.Appearance.Font = BodyFont;
            label.Appearance.ForeColor = TextMuted;
            label.Appearance.Options.UseFont = true;
            label.Appearance.Options.UseForeColor = true;
        }

        /// <summary>배경 없는 아이콘 버튼(상단바의 햄버거/알림/테마 전환 등).</summary>
        public static void IconButton(SimpleButton button, string icon, Color color, int size = 20)
        {
            Flat(button);
            button.Text = string.Empty;
            button.ImageOptions.SvgImage = AppIcons.Get(icon, color);
            button.ImageOptions.SvgImageSize = new Size(size, size);
            button.ImageOptions.SvgImageColorizationMode = SvgImageColorizationMode.None;
            button.Appearance.BackColor = Color.Transparent;
            button.Appearance.BorderColor = Color.Transparent;
            button.AppearanceHovered.BackColor = IsDarkMode ? Color.FromArgb(58, 56, 92) : Color.FromArgb(238, 240, 250);
            button.AppearanceHovered.Options.UseBackColor = true;
            button.Appearance.Options.UseBackColor = true;
            button.Appearance.Options.UseBorderColor = true;
        }

        /// <summary>
        /// 배경 없는 아이콘 버튼 — 아이콘이 SVG가 아니라 <b>PNG 파일</b>(Images 폴더)인 경우.
        /// 원본은 640×640이라 <see cref="AppImages.Icon"/>이 쓸 크기로 줄이고 테마 색으로 칠해 준다.
        /// </summary>
        public static void ImageIconButton(SimpleButton button, string imageResource, Color color, int size = 20)
        {
            Flat(button);
            button.Text = string.Empty;
            button.ImageOptions.SvgImage = null;
            button.ImageOptions.Image = AppImages.Icon(imageResource, size, color);
            button.Appearance.BackColor = Color.Transparent;
            button.Appearance.BorderColor = Color.Transparent;
            button.AppearanceHovered.BackColor = IsDarkMode ? Color.FromArgb(58, 56, 92) : Color.FromArgb(238, 240, 250);
            button.AppearanceHovered.Options.UseBackColor = true;
            button.Appearance.Options.UseBackColor = true;
            button.Appearance.Options.UseBorderColor = true;
        }

        /// <summary>
        /// 상단 바에 얹히는 콤보(언어 선택) — 입력칸처럼 보이지 않게 테두리를 없애고 배경을 상단 바와 맞춘다.
        /// 목록에서 고르기만 하는 컨트롤이라 직접 입력은 막는다.
        /// </summary>
        public static void TopBarCombo(ComboBoxEdit combo)
        {
            combo.Properties.Appearance.Font = BodyFont;
            combo.Properties.Appearance.ForeColor = TextPrimary;
            combo.Properties.Appearance.BackColor = TopBar;
            combo.Properties.Appearance.Options.UseFont = true;
            combo.Properties.Appearance.Options.UseForeColor = true;
            combo.Properties.Appearance.Options.UseBackColor = true;
            combo.Properties.BorderStyle = BorderStyles.NoBorder;
            combo.Properties.TextEditStyle = TextEditStyles.DisableTextEditor;

            // 펼친 목록은 상단 바가 아니라 카드 위에 뜨는 것이므로 카드 배경/본문 글자색을 쓴다.
            combo.Properties.AppearanceDropDown.Font = BodyFont;
            combo.Properties.AppearanceDropDown.ForeColor = TextPrimary;
            combo.Properties.AppearanceDropDown.BackColor = Card;
            combo.Properties.AppearanceDropDown.Options.UseFont = true;
            combo.Properties.AppearanceDropDown.Options.UseForeColor = true;
            combo.Properties.AppearanceDropDown.Options.UseBackColor = true;

            combo.BackColor = TopBar;
        }

        /// <summary>색을 채운 주요 실행 버튼(시작/정지/조회 등).</summary>
        public static void AccentButton(SimpleButton button, Color back, string icon = null)
        {
            Flat(button);
            button.Appearance.BackColor = back;
            button.Appearance.ForeColor = Color.White;
            button.Appearance.Font = BodyBoldFont;
            button.Appearance.BorderColor = back;
            button.Appearance.Options.UseBackColor = true;
            button.Appearance.Options.UseForeColor = true;
            button.Appearance.Options.UseFont = true;
            button.Appearance.Options.UseBorderColor = true;

            button.AppearanceHovered.BackColor = ControlPaint.Light(back, 0.15f);
            button.AppearanceHovered.ForeColor = Color.White;
            button.AppearanceHovered.Options.UseBackColor = true;
            button.AppearanceHovered.Options.UseForeColor = true;

            button.AppearancePressed.BackColor = ControlPaint.Dark(back, 0.05f);
            button.AppearancePressed.ForeColor = Color.White;
            button.AppearancePressed.Options.UseBackColor = true;
            button.AppearancePressed.Options.UseForeColor = true;

            button.AppearanceDisabled.BackColor = Color.FromArgb(206, 209, 224);
            button.AppearanceDisabled.ForeColor = Color.White;
            button.AppearanceDisabled.Options.UseBackColor = true;
            button.AppearanceDisabled.Options.UseForeColor = true;

            if (icon != null)
            {
                button.ImageOptions.SvgImage = AppIcons.Get(icon, Color.White);
                button.ImageOptions.SvgImageSize = new Size(16, 16);
                button.ImageOptions.SvgImageColorizationMode = SvgImageColorizationMode.None;
                button.ImageOptions.Location = ImageLocation.MiddleLeft;
                button.ImageOptions.ImageToTextIndent = 8;
            }
        }

        /// <summary>
        /// 타일 바 — 큼직하게 눌러야 하는 실행 버튼 묶음(POP 상단 공정 제어, 조회 화면의 실행 버튼).
        ///
        /// DevExpress <see cref="TileBar"/>를 쓰면 SimpleButton을 나열하는 것보다
        ///  - 타일 크기·간격을 컨트롤이 알아서 계산하고(개별 Location 계산이 필요 없다),
        ///  - 아이콘/글자를 타일 안에서 각각 다른 위치에 앵커할 수 있고(좌상단 아이콘 + 좌하단 글자),
        ///  - hover/press 애니메이션이 붙어 터치 조작 피드백이 분명해진다.
        ///
        /// 선택 상태는 끈다 — 누르는 즉시 동작하는 <b>명령</b>이라 "선택된 타일"이라는 개념이 없다.
        /// </summary>
        /// <param name="tileHeight">타일 높이(px). TileBar가 자체 내부 여백을 갖기 때문에
        /// 이 값은 <b>바 높이보다 24px 정도 작게</b> 잡아야 글자가 타일 밖으로 밀리지 않는다.</param>
        /// <param name="tileWidth">Wide 타일의 폭(px). 한글 두 단어가 한 줄에 들어가도록 정사각형보다 넓게 둔다.</param>
        public static void TileStrip(TileBar bar, int tileHeight, int tileWidth)
        {
            bar.LookAndFeel.UseDefaultLookAndFeel = false;
            bar.LookAndFeel.Style = LookAndFeelStyle.Flat;
            bar.BackColor = Card;
            bar.AllowSelectedItem = false;
            bar.AllowSelectedItemBorder = false;
            bar.ItemSize = tileHeight;
            bar.WideTileWidth = tileWidth;
            bar.IndentBetweenItems = 8;
            bar.Padding = new Padding(0);
        }

        /// <summary>
        /// 색을 채운 타일 하나 — 좌상단에 아이콘, 좌하단에 글자를 둔다(참조 시안과 같은 배치).
        /// 색은 버튼과 같은 규칙을 따른다(hover는 밝게, press는 어둡게, 비활성은 회색).
        /// </summary>
        public static void Tile(TileBarItem item, Color back, string icon)
        {
            item.ItemSize = TileBarItemSize.Wide;

            Paint(item.AppearanceItem.Normal, back, Color.White, BodyBoldFont);
            Paint(item.AppearanceItem.Hovered, ControlPaint.Light(back, 0.18f), Color.White, BodyBoldFont);
            Paint(item.AppearanceItem.Pressed, ControlPaint.Dark(back, 0.08f), Color.White, BodyBoldFont);
            Paint(item.AppearanceItem.Disabled, Color.FromArgb(206, 209, 224), Color.White, BodyBoldFont);

            item.ImageOptions.SvgImage = AppIcons.Get(icon, Color.White);
            item.ImageOptions.SvgImageSize = new Size(24, 24);
            item.ImageOptions.SvgImageColorizationMode = SvgImageColorizationMode.None;

            // 아이콘을 글자 위에 두고 블록 전체를 타일 가운데에 놓는다.
            // (좌상단 아이콘 + 좌하단 글자로 따로 앵커하면 Elements를 직접 구성해야 하고,
            //  한글 두 단어가 들어가는 폭에서는 글자가 타일 밖으로 밀려 잘린다.)
            item.ImageToTextAlignment = TileControlImageToTextAlignment.Top;
            item.ImageToTextIndent = 6;
            item.ImageAlignment = TileItemContentAlignment.MiddleCenter;
            item.TextAlignment = TileItemContentAlignment.MiddleCenter;
            item.Padding = new Padding(8);
        }

        /// <summary>
        /// 조회 조건 폼(LayoutControl) — 라벨 열 정렬·간격·리사이즈를 컨트롤이 맡는다.
        ///
        /// DevExpress <see cref="LayoutControl"/>을 쓰면 조건이 늘거나 창 폭이 바뀌어도
        /// 라벨과 에디터가 스스로 정렬되므로, 조건마다 Location을 손으로 계산하지 않아도 된다.
        /// 그룹 테두리는 카드 안에 또 테두리를 겹치지 않도록 끄고, 캡션만 카드 제목처럼 남긴다.
        /// </summary>
        public static void SearchLayout(LayoutControl layout, LayoutControlGroup group, Color accent)
        {
            layout.LookAndFeel.UseDefaultLookAndFeel = false;
            layout.LookAndFeel.Style = LookAndFeelStyle.Flat;
            layout.BackColor = Card;
            layout.AllowCustomization = false;
            layout.Padding = new Padding(6);

            group.GroupBordersVisible = false;
            group.AppearanceGroup.Font = SectionFont;
            group.AppearanceGroup.ForeColor = IsDarkMode ? Color.White : Color.FromArgb(30, 33, 56);
            group.AppearanceGroup.Options.UseFont = true;
            group.AppearanceGroup.Options.UseForeColor = true;

            group.AppearanceItemCaption.Font = BodyFont;
            group.AppearanceItemCaption.ForeColor = TextMuted;
            group.AppearanceItemCaption.Options.UseFont = true;
            group.AppearanceItemCaption.Options.UseForeColor = true;

            group.CaptionImageOptions.SvgImage = AppIcons.Get(AppIcons.Dot, accent);
            group.CaptionImageOptions.SvgImageSize = new Size(12, 12);
            group.CaptionImageOptions.SvgImageColorizationMode = SvgImageColorizationMode.None;
        }

        /// <summary>테두리만 있는 보조 버튼.</summary>
        public static void QuietButton(SimpleButton button, Color accent, string icon = null)
        {
            Flat(button);
            button.Appearance.BackColor = IsDarkMode ? Color.FromArgb(45, 44, 70) : Color.White;
            button.Appearance.ForeColor = accent;
            button.Appearance.Font = BodyBoldFont;
            button.Appearance.BorderColor = accent;
            button.Appearance.Options.UseBackColor = true;
            button.Appearance.Options.UseForeColor = true;
            button.Appearance.Options.UseFont = true;
            button.Appearance.Options.UseBorderColor = true;

            button.AppearanceHovered.BackColor = Blend(accent, Card, 0.12f);
            button.AppearanceHovered.ForeColor = accent;
            button.AppearanceHovered.Options.UseBackColor = true;
            button.AppearanceHovered.Options.UseForeColor = true;

            if (icon != null)
            {
                button.ImageOptions.SvgImage = AppIcons.Get(icon, accent);
                button.ImageOptions.SvgImageSize = new Size(16, 16);
                button.ImageOptions.SvgImageColorizationMode = SvgImageColorizationMode.None;
                button.ImageOptions.Location = ImageLocation.MiddleLeft;
                button.ImageOptions.ImageToTextIndent = 8;
            }
        }

        /// <summary>
        /// 대화상자 상단 색 띠(사용자 정보 팝업의 남보라 헤더와 같은 역할).
        /// 강조 색에서 어두운 쪽으로 가는 수직 그라데이션이라, 흰 본문 위에서 창 경계가 분명하게 보인다.
        /// </summary>
        public static void DialogHeader(PanelControl panel, Color accent)
        {
            panel.BorderStyle = BorderStyles.NoBorder;
            panel.Appearance.BackColor = accent;
            panel.Appearance.BackColor2 = ControlPaint.Dark(accent, 0.12f);
            panel.Appearance.GradientMode = LinearGradientMode.Vertical;
            panel.Appearance.Options.UseBackColor = true;
        }

        /// <summary>대화상자 색 띠 위의 제목(흰 굵은 글자).</summary>
        public static void DialogHeaderTitle(LabelControl label)
        {
            label.Appearance.Font = SectionFont;
            label.Appearance.ForeColor = Color.White;
            label.Appearance.Options.UseFont = true;
            label.Appearance.Options.UseForeColor = true;
        }

        /// <summary>카드 안쪽에 겹쳐 쓰는 보조 패널(테두리 없이 카드와 같은 배경).</summary>
        public static void PlainPanel(PanelControl panel)
        {
            panel.BorderStyle = BorderStyles.NoBorder;
            panel.Appearance.BackColor = Card;
            panel.Appearance.Options.UseBackColor = true;
        }

        /// <summary>본문 배경(카드가 놓이는 바닥면).</summary>
        public static void ContentSurface(PanelControl panel)
        {
            panel.BorderStyle = BorderStyles.NoBorder;
            panel.Appearance.BackColor = Surface;
            panel.Appearance.Options.UseBackColor = true;
        }

        /// <summary>
        /// 카드(밝은 배경 + 얇은 테두리).
        /// 주의: 스킨 기본 LookAndFeel에서 테두리(BorderStyles.Simple)를 켜면 스킨이 패널 배경을 직접 칠해
        /// Appearance.BackColor가 무시된다(실측: 흰색 지정에도 #F0F0F0으로 렌더링).
        /// 그래서 이 패널만 Flat 스타일로 바꿔 우리가 지정한 색/테두리가 그대로 나오게 한다.
        /// </summary>
        public static void CardPanel(PanelControl panel)
        {
            panel.LookAndFeel.UseDefaultLookAndFeel = false;
            panel.LookAndFeel.Style = LookAndFeelStyle.Flat;
            panel.BorderStyle = BorderStyles.Simple;
            panel.Appearance.BackColor = Card;
            panel.Appearance.BorderColor = CardBorder;
            panel.Appearance.Options.UseBackColor = true;
            panel.Appearance.Options.UseBorderColor = true;
        }

        /// <summary>제목이 있는 카드(GroupControl) — 캡션은 포인트 컬러, 본문은 카드 배경.</summary>
        public static void CardGroup(GroupControl group, Color accent)
        {
            group.LookAndFeel.UseDefaultLookAndFeel = false;
            group.LookAndFeel.Style = LookAndFeelStyle.Flat;
            group.Appearance.BackColor = Card;
            group.Appearance.BorderColor = CardBorder;
            group.Appearance.Options.UseBackColor = true;
            group.Appearance.Options.UseBorderColor = true;

            group.AppearanceCaption.Font = SectionFont;
            group.AppearanceCaption.ForeColor = accent;
            group.AppearanceCaption.Options.UseFont = true;
            group.AppearanceCaption.Options.UseForeColor = true;
        }

        /// <summary>카드 제목(좌측에 색 점을 함께 두면 시안 느낌이 난다).</summary>
        public static void SectionTitle(LabelControl label, Color accent)
        {
            label.Appearance.Font = SectionFont;
            label.Appearance.ForeColor = IsDarkMode ? Color.White : Color.FromArgb(30, 33, 56);
            label.Appearance.Options.UseFont = true;
            label.Appearance.Options.UseForeColor = true;
            label.ImageOptions.SvgImage = AppIcons.Get(AppIcons.Dot, accent);
            label.ImageOptions.SvgImageSize = new Size(12, 12);
            label.ImageOptions.SvgImageColorizationMode = SvgImageColorizationMode.None;
            label.ImageAlignToText = ImageAlignToText.LeftCenter;
            label.IndentBetweenImageAndText = 6;
        }

        /// <summary>상단바 세션 남은 시간(고정폭 숫자라 초가 바뀌어도 글자가 흔들리지 않는다).</summary>
        public static void SessionTime(LabelControl label)
        {
            label.Appearance.Font = new Font("Consolas", 11F, FontStyle.Bold);
            label.Appearance.ForeColor = Indigo;
            label.Appearance.Options.UseFont = true;
            label.Appearance.Options.UseForeColor = true;
        }

        /// <summary>
        /// 사이드바 맨 아래 배포 버전 표시.
        /// 짙은 그라데이션 위에 얹히므로 본문 색이 아니라 사이드바 전용 흐린 색을 쓴다.
        /// 배경을 투명으로 두어야 뒤쪽 그라데이션이 그대로 비친다
        /// (불투명 색을 주면 그 줄만 색이 끊겨 보인다).
        /// </summary>
        public static void SidebarVersion(LabelControl label)
        {
            label.Appearance.Font = SmallFont;
            label.Appearance.ForeColor = SidebarTextMuted;
            label.Appearance.BackColor = Color.Transparent;
            label.Appearance.TextOptions.HAlignment = HorzAlignment.Center;
            label.Appearance.Options.UseFont = true;
            label.Appearance.Options.UseForeColor = true;
            label.Appearance.Options.UseBackColor = true;
            label.Appearance.Options.UseTextOptions = true;
        }

        /// <summary>보조 설명/캡션 텍스트.</summary>
        public static void Caption(LabelControl label)
        {
            label.Appearance.Font = SmallFont;
            label.Appearance.ForeColor = TextMuted;
            label.Appearance.Options.UseFont = true;
            label.Appearance.Options.UseForeColor = true;
        }

        /// <summary>본문 일반 텍스트.</summary>
        public static void BodyText(LabelControl label, bool bold = false)
        {
            label.Appearance.Font = bold ? BodyBoldFont : BodyFont;
            label.Appearance.ForeColor = TextPrimary;
            label.Appearance.Options.UseFont = true;
            label.Appearance.Options.UseForeColor = true;
        }

        /// <summary>
        /// 상태 표시용 알약 라벨(연한 배경 + 진한 글자).
        /// LabelControl은 기본이 자동 크기라서 빈 문자열 상태에서 폭이 0으로 줄어든 채 남는다 —
        /// 그래서 크기를 명시적으로 다시 지정한다(<paramref name="size"/>).
        /// </summary>
        public static void StatePill(LabelControl label, Color accent, Size size)
        {
            label.AutoSizeMode = LabelAutoSizeMode.None;
            label.Size = size;
            label.Appearance.Font = BodyBoldFont;
            label.Appearance.ForeColor = accent;
            label.Appearance.BackColor = Blend(accent, Card, 0.16f);
            label.Appearance.Options.UseFont = true;
            label.Appearance.Options.UseForeColor = true;
            label.Appearance.Options.UseBackColor = true;
            label.Appearance.TextOptions.HAlignment = HorzAlignment.Center;
            label.Appearance.TextOptions.VAlignment = VertAlignment.Center;
            label.Appearance.Options.UseTextOptions = true;
            label.Padding = new Padding(10, 0, 10, 0);
        }

        /// <summary>
        /// 색을 꽉 채운 굵은 띠(POP 화면의 워크스테이션 / 공정 진행 상태 표시).
        /// 알약(StatePill)과 달리 배경을 강조 색으로 다 채우고 글자를 흰색으로 둬서 멀리서도 눈에 들어온다.
        ///
        /// LabelControl은 기본이 세로 자동 크기라, 도킹된 상태에서는 디자이너에서 준 높이가
        /// 글자 높이(약 20px)까지 줄어든 채로 남아 큰 글꼴이 위아래로 잘린다 —
        /// 그래서 <paramref name="height"/>로 띠 높이를 명시적으로 다시 잡는다(StatePill의 size와 같은 이유).
        /// </summary>
        public static void Banner(LabelControl label, Color accent, int height)
        {
            label.AutoSizeMode = LabelAutoSizeMode.None;
            label.Height = height;
            label.Appearance.Font = BannerFont;
            label.Appearance.ForeColor = Color.White;
            label.Appearance.BackColor = accent;
            label.Appearance.Options.UseFont = true;
            label.Appearance.Options.UseForeColor = true;
            label.Appearance.Options.UseBackColor = true;
            label.Appearance.TextOptions.HAlignment = HorzAlignment.Center;
            label.Appearance.TextOptions.VAlignment = VertAlignment.Center;
            label.Appearance.Options.UseTextOptions = true;
        }

        /// <summary>알림 개수 배지(빨간 원). <paramref name="diameter"/>로 원 크기를 명시한다.</summary>
        public static void Badge(LabelControl label, int diameter)
        {
            label.AutoSizeMode = LabelAutoSizeMode.None;
            label.Size = new Size(diameter, diameter);
            label.Appearance.Font = new Font(FontFamily, 7.5F, FontStyle.Bold);
            label.Appearance.ForeColor = Color.White;
            label.Appearance.BackColor = Rose;
            label.Appearance.Options.UseFont = true;
            label.Appearance.Options.UseForeColor = true;
            label.Appearance.Options.UseBackColor = true;
            label.Appearance.TextOptions.HAlignment = HorzAlignment.Center;
            label.Appearance.TextOptions.VAlignment = VertAlignment.Center;
            label.Appearance.Options.UseTextOptions = true;
            RoundToCircle(label);
        }

        /// <summary>사용자 아바타(첫 글자를 넣은 보라색 원). <paramref name="diameter"/>로 원 크기를 명시한다.</summary>
        public static void Avatar(LabelControl label, string initial, int diameter)
        {
            label.AutoSizeMode = LabelAutoSizeMode.None;
            label.Size = new Size(diameter, diameter);
            label.Text = initial;
            label.Appearance.Font = BodyBoldFont;
            label.Appearance.ForeColor = Color.White;
            label.Appearance.BackColor = Violet;
            label.Appearance.Options.UseFont = true;
            label.Appearance.Options.UseForeColor = true;
            label.Appearance.Options.UseBackColor = true;
            label.Appearance.TextOptions.HAlignment = HorzAlignment.Center;
            label.Appearance.TextOptions.VAlignment = VertAlignment.Center;
            label.Appearance.Options.UseTextOptions = true;
            RoundToCircle(label);
        }

        /// <summary>입력 에디터(콤보/텍스트/날짜) 톤.</summary>
        public static void Editor(BaseEdit edit)
        {
            edit.Properties.Appearance.Font = BodyFont;
            edit.Properties.Appearance.ForeColor = TextPrimary;
            edit.Properties.Appearance.BackColor = IsDarkMode ? Color.FromArgb(45, 44, 70) : Color.White;
            edit.Properties.Appearance.Options.UseFont = true;
            edit.Properties.Appearance.Options.UseForeColor = true;
            edit.Properties.Appearance.Options.UseBackColor = true;
            edit.Properties.BorderStyle = BorderStyles.Simple;
        }

        /// <summary>체크박스(예: 로그인 화면의 "아이디 저장") — 카드 배경 위에 얹히도록 배경은 투명하게 둔다.</summary>
        public static void CheckBox(CheckEdit check)
        {
            check.Properties.Appearance.Font = BodyFont;
            check.Properties.Appearance.ForeColor = TextPrimary;
            check.Properties.Appearance.Options.UseFont = true;
            check.Properties.Appearance.Options.UseForeColor = true;
            check.BackColor = Color.Transparent;
        }

        /// <summary>
        /// 그리드 형태 — 앱의 모든 목록이 공유하는 단일 기준.
        ///
        /// 예전에는 화면마다 <b>진한 포인트 컬러를 꽉 채운 헤더</b>를 썼는데,
        /// 화면마다 헤더 색이 달라 앱이 산만하고 데이터보다 헤더가 먼저 눈에 들어왔다.
        /// 그래서 DevExpress 표준 그리드 시안처럼 <b>밝은 헤더 + 얇은 가로 구분선</b>으로 바꿨다:
        ///
        ///  - 헤더   : 아주 옅은 회색 배경 + 진한 글자. 포인트 컬러는 면적으로 쓰지 않는다.
        ///  - 행     : 흰 배경 단색(줄무늬 없음) + 얇은 가로선. 세로선은 없앤다.
        ///  - 포커스 : 포인트 컬러를 아주 옅게 깐 배경(<see cref="Forms.Common.MeisGridView"/>는 왼쪽 색 띠도 그린다).
        ///
        /// 포인트 컬러는 화면 구분용으로 <b>포커스 행과 컬럼 색 점</b>에만 쓴다.
        /// </summary>
        public static void Grid(GridControl grid, GridView view, Color accent)
        {
            grid.LookAndFeel.UseDefaultLookAndFeel = false;
            grid.LookAndFeel.Style = LookAndFeelStyle.Flat;
            grid.BackColor = Card;
            view.BorderStyle = BorderStyles.NoBorder;

            view.OptionsView.ShowGroupPanel = false;
            view.OptionsView.ShowIndicator = false;
            // 세로선을 켜서 컬럼 경계를 눈으로 따라갈 수 있게 한다
            // (컬럼이 10개를 넘는 화면이 많아, 선이 없으면 어느 열의 값인지 놓치기 쉽다).
            view.OptionsView.ShowVerticalLines = DefaultBoolean.True;
            view.OptionsView.ShowHorizontalLines = DefaultBoolean.True;

            // 한 줄 걸러 옅은 톤을 깔아 행을 구분한다(줄무늬).
            // 짝수 행만 켜면 홀수 행은 Appearance.Row(흰색)가 그대로 쓰여 두 톤이 번갈아 나온다.
            view.OptionsView.EnableAppearanceEvenRow = true;
            view.OptionsView.EnableAppearanceOddRow = false;
            view.OptionsView.ColumnAutoWidth = true;
            view.OptionsBehavior.Editable = false;
            view.OptionsSelection.EnableAppearanceFocusedCell = false;

            // 시안처럼 행을 여유 있게 — 터치로도 집기 쉽고 글자가 답답하지 않다.
            view.RowHeight = 34;
            view.ColumnPanelRowHeight = 38;

            Paint(view.Appearance.HeaderPanel, GridHeaderBack, TextPrimary, BodyBoldFont);
            view.Appearance.HeaderPanel.BackColor2 = GridHeaderBack;
            view.Appearance.HeaderPanel.BorderColor = GridLine;
            view.Appearance.HeaderPanel.Options.UseBorderColor = true;
            view.Appearance.HeaderPanel.TextOptions.HAlignment = HorzAlignment.Center;
            view.Appearance.HeaderPanel.Options.UseTextOptions = true;

            Paint(view.Appearance.Row, Card, TextPrimary, BodyFont);
            Paint(view.Appearance.EvenRow, GridRowAlt, TextPrimary, BodyFont);
            Paint(view.Appearance.FocusedRow, Blend(accent, Card, 0.14f), TextPrimary, BodyBoldFont);
            Paint(view.Appearance.SelectedRow, Blend(accent, Card, 0.11f), TextPrimary, BodyFont);
            Paint(view.Appearance.HotTrackedRow, Blend(accent, Card, 0.06f), TextPrimary, BodyFont);
            Paint(view.Appearance.Empty, Card, TextPrimary, BodyFont);

            view.Appearance.HorzLine.BackColor = GridLine;
            view.Appearance.HorzLine.Options.UseBackColor = true;
            view.Appearance.VertLine.BackColor = GridLine;
            view.Appearance.VertLine.Options.UseBackColor = true;
        }

        /// <summary>
        /// 계층 트리(TreeList): 그리드와 같은 톤 — 밝은 헤더 + 얇은 가로 구분선 + 옅은 포커스 행.
        /// 그리드와 나란히 놓여도 이질감이 없도록 <see cref="Grid"/>와 같은 색·글꼴 규칙을 따른다.
        /// </summary>
        public static void Tree(TreeList tree, Color accent)
        {
            tree.LookAndFeel.UseDefaultLookAndFeel = false;
            tree.LookAndFeel.Style = LookAndFeelStyle.Flat;
            tree.BorderStyle = BorderStyles.NoBorder;

            tree.OptionsView.ShowIndicator = false;

            // 그리드와 같은 규칙 — 한 줄 걸러 옅은 톤 + 세로선으로 컬럼 경계 표시.
            // 트리도 값 컬럼이 10개라 그리드와 다르게 두면 화면끼리 어긋나 보인다.
            tree.OptionsView.EnableAppearanceEvenRow = true;
            tree.OptionsView.ShowHorzLines = true;
            tree.OptionsView.ShowVertLines = true;
            tree.OptionsBehavior.Editable = false;
            tree.RowHeight = 34;
            tree.ColumnPanelRowHeight = 38;

            Paint(tree.Appearance.HeaderPanel, GridHeaderBack, TextPrimary, BodyBoldFont);
            tree.Appearance.HeaderPanel.BorderColor = GridLine;
            tree.Appearance.HeaderPanel.Options.UseBorderColor = true;
            tree.Appearance.HeaderPanel.TextOptions.HAlignment = HorzAlignment.Center;
            tree.Appearance.HeaderPanel.Options.UseTextOptions = true;

            Paint(tree.Appearance.Row, Card, TextPrimary, BodyFont);

            // 줄무늬 색은 그리드와 같은 값(GridRowAlt)을 써야 한다.
            // EnableAppearanceEvenRow만 켜고 색을 지정하지 않으면 DevExpress 기본색이 쓰여
            // 이 화면만 다른 톤으로 보인다(실제로 그렇게 어긋났었다).
            Paint(tree.Appearance.EvenRow, GridRowAlt, TextPrimary, BodyFont);

            Paint(tree.Appearance.FocusedRow, Blend(accent, Card, 0.14f), TextPrimary, BodyBoldFont);
            Paint(tree.Appearance.SelectedRow, Blend(accent, Card, 0.11f), TextPrimary, BodyFont);
            Paint(tree.Appearance.HotTrackedRow, Blend(accent, Card, 0.06f), TextPrimary, BodyFont);
            Paint(tree.Appearance.Empty, Card, TextPrimary, BodyFont);

            tree.Appearance.HorzLine.BackColor = GridLine;
            tree.Appearance.HorzLine.Options.UseBackColor = true;
            tree.Appearance.VertLine.BackColor = GridLine;
            tree.Appearance.VertLine.Options.UseBackColor = true;
            tree.Appearance.TreeLine.BackColor = GridLine;
            tree.Appearance.TreeLine.Options.UseBackColor = true;
        }

        /// <summary>설비 로그(콘솔처럼 어두운 배경 + 초록 고정폭 글꼴).</summary>
        public static void Console(MemoEdit memo)
        {
            memo.Properties.Appearance.BackColor = ConsoleBack;
            memo.Properties.Appearance.ForeColor = ConsoleFore;
            memo.Properties.Appearance.Font = MonoFont;
            memo.Properties.Appearance.Options.UseBackColor = true;
            memo.Properties.Appearance.Options.UseForeColor = true;
            memo.Properties.Appearance.Options.UseFont = true;
            memo.Properties.BorderStyle = BorderStyles.NoBorder;
        }

        /// <summary>탭 칩(열린 화면) 색: 활성 탭은 인디고 배경 + 흰 글자.</summary>
        /// <summary>
        /// 탭 스트립 배경 + 하단 경계선.
        /// 위 여백은 탭이 스트립 아래쪽에 붙어 경계선 위에 앉도록 계산한다(탭 아랫변 = 경계선 바로 위).
        /// </summary>
        public static void TabStrip(Control strip)
        {
            strip.BackColor = Surface;
            // 스트립 높이 44 - 탭 높이 32 - 경계선 1 = 위 여백 11 (탭이 아래로 붙는다)
            strip.Padding = new Padding(10, 11, 8, 0);
            strip.Paint += (s, e) =>
            {
                var control = (Control)s;
                using (var pen = new Pen(TabBorder))
                    e.Graphics.DrawLine(pen, 0, control.Height - 1, control.Width, control.Height - 1);
            };
        }

        /// <summary>설비 연결 상태 → 색 (연결=초록, 연결중=주황, 오류=빨강, 해제=회색).</summary>
        public static Color ConnectionColor(string state)
        {
            if (state == "Connected") return Emerald;
            if (state == "Connecting") return Amber;
            if (state == "Faulted") return Rose;
            return Slate;
        }

        /// <summary>설비 연결 상태 → 짧은 한글 표기(알약 라벨 폭에 맞추기 위함).</summary>
        public static string ConnectionStateText(string state)
        {
            if (state == "Connected") return "연결";
            if (state == "Connecting") return "연결중";
            if (state == "Faulted") return "오류";
            if (state == "Disconnected") return "해제";
            return state;
        }

        /// <summary>컨트롤을 원형으로 잘라낸다(아바타/배지).</summary>
        public static void RoundToCircle(Control control)
        {
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(0, 0, control.Width, control.Height);
                control.Region = new Region(path);
            }
        }

        /// <summary>컨트롤 네 귀퉁이를 둥글게 잘라낸다(탭 칩 등).</summary>
        public static void RoundCorners(Control control, int radius)
        {
            if (control.Width <= radius * 2 || control.Height <= radius * 2)
                return;

            using (var path = new GraphicsPath())
            {
                var d = radius * 2;
                path.AddArc(0, 0, d, d, 180, 90);
                path.AddArc(control.Width - d - 1, 0, d, d, 270, 90);
                path.AddArc(control.Width - d - 1, control.Height - d - 1, d, d, 0, 90);
                path.AddArc(0, control.Height - d - 1, d, d, 90, 90);
                path.CloseFigure();
                control.Region = new Region(path);
            }
        }

        /// <summary>두 색을 섞는다(ratio=0이면 back, 1이면 fore).</summary>
        public static Color Blend(Color fore, Color back, float ratio)
        {
            var r = (int)(fore.R * ratio + back.R * (1 - ratio));
            var g = (int)(fore.G * ratio + back.G * (1 - ratio));
            var b = (int)(fore.B * ratio + back.B * (1 - ratio));
            return Color.FromArgb(r, g, b);
        }

        private static void Paint(AppearanceObject appearance, Color back, Color fore, Font font)
        {
            appearance.BackColor = back;
            appearance.ForeColor = fore;
            appearance.Font = font;
            appearance.Options.UseBackColor = true;
            appearance.Options.UseForeColor = true;
            appearance.Options.UseFont = true;
        }

        private static void Flat(SimpleButton button)
        {
            button.LookAndFeel.UseDefaultLookAndFeel = false;
            button.LookAndFeel.Style = LookAndFeelStyle.Flat;
            button.PaintStyle = PaintStyles.Light;
        }
    }
}
