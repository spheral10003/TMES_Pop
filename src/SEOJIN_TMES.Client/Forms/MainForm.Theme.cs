using DevExpress.Utils;
using DevExpress.XtraEditors;
using SEOJIN_TMES.Client.Composition;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Theme;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEOJIN_TMES.Client.Forms
{
    public partial class MainForm
    {
        private const int SidebarWidth = 240;
        private const int SidebarCollapsedWidth = 64;

        /// <summary>언어 콤보 국기 아이콘 한 변(px). 원본 24×24를 상단 바 글자 높이에 맞춰 줄인다.</summary>
        private const int LanguageFlagSize = 20;

        /// <summary>
        /// 상단 바 가운데 시계 형식 — YYYY-MM-DD HH:MI:SS PM/AM.
        /// AM/PM은 화면 언어와 무관하게 영문으로 고정한다(한국어 문화권에서 tt는 "오전/오후"로 나온다).
        /// </summary>
        private const string ClockFormat = "yyyy-MM-dd hh:mm:ss tt";

        /// <summary>
        /// 언어 콤보에 표시할 목록(표시 문구 ↔ 문화권 코드 ↔ 국기 아이콘).
        /// 국기는 Images\flag\{두 글자 코드}.png 이며 문화권 코드 앞 두 글자로 찾는다.
        ///
        /// 언어를 추가할 때는 <b>세 곳을 함께</b> 갖춰야 한다 —
        /// 여기 한 줄 + Common.Languages\Language.{두 글자 코드}.resx + Images\flag\{두 글자 코드}.png.
        /// 번역 리소스 없이 항목만 넣으면 골라도 중립 리소스(한국어)로 되돌아가 고장처럼 보인다.
        /// </summary>
        private static readonly LanguageOption[] Languages =
        {
            new LanguageOption("한국어", "ko-KR"),
            new LanguageOption("English", "en-US"),
            new LanguageOption("Việt Nam", "vi-VN")
        };

        /// <summary>언어 콤보의 국기 아이콘 모음. 콤보가 살아 있는 동안 함께 유지된다.</summary>
        private ImageCollection _languageFlags;







        private void InitializeTheme()
        {

            Text = Language.MainForm_Title;

            // 작업 표시줄 아이콘은 이 값이 결정한다.
            // 이 창은 제목 줄이 없어(FormBorderStyle.None) 창 좌상단에는 안 보이지만,
            // 작업 표시줄·Alt+Tab에는 그대로 쓰이므로 반드시 지정해야 한다.
            // (Designer가 아니라 여기서 넣는 이유는 프로젝트 CLAUDE.md 1번 — 디자이너에 커스텀 static 호출 금지.)
            Icon = AppImages.AppIcon();

            lblBrand.Text = "MEIS POP";
            lblBrandSub.Text = "MANUFACTURING SYSTEM";
            SetBreadcrumb(string.Empty);
            lblVersion.Text = string.Format(Language.MainForm_VersionFormat, AppVersion.Current);


            ApplyTheme();
        }


        /// <summary>
        /// 테마 색을 셸 전체에 적용한다(시작 시 1회).
        /// 팔레트 자체는 Program.cs의 <see cref="AppTheme.ApplySkin"/>에서 정해진다.
        /// </summary>
        private void ApplyTheme()
        {
            AppTheme.Sidebar(sidePanel);
            AppTheme.BrandPanel(brandPanel);
            AppTheme.Accordion(accordion);
            AppTheme.SidebarVersion(lblVersion);

            // 브랜드 블록은 회사 로고 하나로 채운다(제품명 두 줄은 로고에 자리를 내주고 숨긴다).
            // 로고 워드마크는 원본이 검정이라 짙은 사이드바에서는 안 보이므로 흰색으로 바꿔 받는다.
            lblBrand.Visible = false;
            lblBrandSub.Visible = false;

            brandIcon.SizeMode = SvgImageSizeMode.Squeeze;   // 원본 비율(약 4.2:1) 유지
            brandIcon.BackColor = Color.Transparent;
            ApplyBrandLogo();

            AppTheme.TopBarPanel(panelTop);
            AppTheme.Breadcrumb(lblBreadcrumb);
            AppTheme.ContentSurface(mainArea);
            AppTheme.ContentSurface(rightPanel);
            AppTheme.ContentSurface(contentHost);
            AppTheme.TabStrip(tabStrip);

            AppTheme.IconButton(btnMenuToggle, AppIcons.Menu, AppTheme.TopBarFore, 16);

            // 알림/사용자/로그아웃은 Images 폴더의 PNG 아이콘을 쓴다(모양만 쓰고 색은 테마에서 받는다).
            AppTheme.ImageIconButton(btnAlerts, AppImages.IconBell, AppTheme.TopBarFore, 18);
            AppTheme.Badge(lblAlertBadge, 16);

            // 우측 상단 정보 묶음: 언어 · 알림 · "{이름} 님 환영합니다." · 사용자 · 창 버튼 · 로그아웃 (현재 일시는 상단 바 가운데)
            AppTheme.TopBarText(lblWelcome);
            AppTheme.SessionTime(lblSessionTime);
            AppTheme.TopBarCombo(cboLanguage);
            cboLanguage.ToolTip = Language.MainForm_LanguageHint;
            AppTheme.ImageIconButton(btnUser, AppImages.IconUser, AppTheme.TopBarFore, 18);
            btnUser.ToolTip = Language.UserProfile_Title;
            AppTheme.ImageIconButton(btnLogout, AppImages.IconPowerOff, AppTheme.TopBarForeMuted, 18);
            btnLogout.ToolTip = Language.Common_Logout;

            // 창 버튼(최소화 · 최대화/복원) — 제목 줄이 없어 상단 바가 대신 낸다. MainForm.Window.cs 참고.
            AppTheme.IconButton(btnMinimize, AppIcons.WindowMinimize, AppTheme.TopBarFore, 16);
            btnMinimize.ToolTip = Language.MainForm_Minimize;
            ApplyWindowStatePresentation();   // 최대화 버튼은 상태에 따라 아이콘·툴팁이 달라진다

            lblAlertBadge.BringToFront();

            foreach (var screen in _openScreens.Values)
            {
                screen.Chip.Active = screen.Key == _activeKey;
                screen.Chip.Invalidate(); // 테마 색이 바뀐 경우에도 탭이 새 색으로 다시 그려지도록
                (screen.Content as IThemedScreen)?.ApplyTheme();
            }
        }
    }
}
