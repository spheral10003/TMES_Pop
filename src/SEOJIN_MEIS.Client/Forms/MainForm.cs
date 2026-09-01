using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraBars.Navigation;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using SEOJIN_MEIS.Client.ApiClient;
using SEOJIN_MEIS.Client.Common.Logging;
using SEOJIN_MEIS.Client.Forms.Common;
using SEOJIN_MEIS.Client.Forms.Home;
using SEOJIN_MEIS.Client.Forms.Notification;
using SEOJIN_MEIS.Client.Composition;
using SEOJIN_MEIS.Client.Equipment;
using SEOJIN_MEIS.Client.Localization;
using SEOJIN_MEIS.Client.Menu;
using SEOJIN_MEIS.Client.Theme;

namespace SEOJIN_MEIS.Client.Forms
{
    /// <summary>
    /// 메인 셸: 좌측 브랜드 블록 + 3-Depth 트리 메뉴(AccordionControl), 상단 바, 우측 탭 스트립 + 화면 영역.
    /// 상단 바 우측 정보 묶음은 "언어 · 알림 · {이름} 님 환영합니다. · 사용자 · 로그아웃" 순서로 배치하고,
    /// 현재 일시(시계)는 상단 바 가로 가운데에 따로 둔다.
    ///
    /// 색·폰트·아이콘은 전부 <see cref="AppTheme"/>/<see cref="AppIcons"/>를 통해 적용한다(런타임).
    /// Designer(InitializeComponent)에는 구조/도킹만 들어있어 VS 디자이너가 그대로 열린다.
    ///
    /// 탭은 <see cref="TabChip"/>(직접 그리는 컨트롤)을 FlowLayoutPanel에 얹어 만든다.
    /// TODO: DevExpress 26.1 "Tab &amp; Navigation" 모듈(XtraTab)이 설치되면 XtraTabControl로 교체할 수 있다.
    /// </summary>
    public partial class MainForm : DevExpress.XtraEditors.XtraForm
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
        /// 여기 한 줄 + Localization\Strings.{두 글자 코드}.resx + Images\flag\{두 글자 코드}.png.
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

        private ClientContext _context;
        private readonly Dictionary<string, OpenedScreen> _openScreens = new Dictionary<string, OpenedScreen>();
        private readonly Dictionary<string, string> _breadcrumbs = new Dictionary<string, string>();
        private readonly Dictionary<string, AccordionControlElement> _menuElements = new Dictionary<string, AccordionControlElement>();
        /// <summary>서버가 알려준 "안 읽은 알림" 개수. 폴링으로 갱신되며 배지 숫자의 근거가 된다.</summary>
        private int _unreadNotificationCount;

        /// <summary>
        /// 홈 탭의 화면 키. 메뉴에서 오는 키(<see cref="MenuNode.Key"/>)와 겹치면 안 되므로
        /// <c>MenuCatalog</c>가 쓰지 않는 이름으로 둔다.
        /// </summary>
        private const string HomeTabKey = "__HOME__";

        /// <summary>홈 탭 내용. 항상 열려 있으므로(닫기 없음) null이 되지 않는다.</summary>
        private HomeScreen _homeScreen;

        private string _activeKey;
        private bool _sidebarCollapsed;

        private System.Windows.Forms.Timer _sessionTimer;
        private System.Windows.Forms.Timer _notificationTimer;
        private DateTime _sessionEndUtc;
        private bool _suppressLanguageChange;

        public MainForm()
        {
            InitializeComponent();

            // 제목 줄이 없는 창(FormBorderStyle.None)을 그냥 최대화하면 작업 표시줄까지 덮는다.
            // 최대화 크기를 화면 작업 영역으로 제한해 "작업 표시줄은 그대로, 제목 줄만 사라진" 상태로 띄운다.
            // (전체 화면 키오스크로 쓰려면 이 줄을 지우면 된다.)
            // WM_GETMINMAXINFO는 창이 만들어질 때(이미 최대화 상태) 한 번 오므로 핸들 생성 전인 여기서 잡아야 한다.
            MaximizedBounds = Screen.PrimaryScreen.WorkingArea;
        }

        public void Initialize(ClientContext context)
        {
            _context = context;

            Text = Strings.MainForm_Title;
            lblBrand.Text = "MEIS POP";
            lblBrandSub.Text = "MANUFACTURING SYSTEM";
            lblBreadcrumb.Text = Strings.MainForm_Home;
            lblVersion.Text = string.Format(Strings.MainForm_VersionFormat, AppVersion.Current);

            BuildLanguageList();
            RefreshWelcome();

            ApplyTheme();
            BuildMenu();
            OpenHomeTab();

            accordion.ElementClick += Accordion_ElementClick;
            panelTop.Resize += (s, e) => LayoutTopBar();
            tabStrip.Resize += (s, e) => LayoutTabChips();
            cboLanguage.SelectedIndexChanged += CboLanguage_SelectedIndexChanged;

            RefreshAlertBadge();
            WireEquipmentAlerts();
            StartNotificationPolling();
            StartSessionCountdown();
            LayoutTopBar();
        }

        // ────────────────────────────── 테마 적용 ──────────────────────────────

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

            AppTheme.IconButton(btnMenuToggle, AppIcons.Menu, AppTheme.TextPrimary);

            // 알림/사용자/로그아웃은 Images 폴더의 PNG 아이콘을 쓴다(모양만 쓰고 색은 테마에서 받는다).
            AppTheme.ImageIconButton(btnAlerts, AppImages.IconBell, AppTheme.TextPrimary);
            AppTheme.Badge(lblAlertBadge, 16);

            // 우측 상단 정보 묶음: 언어 · 알림 · "{이름} 님 환영합니다." · 사용자 · 로그아웃 (현재 일시는 상단 바 가운데)
            AppTheme.BodyText(lblWelcome, true);
            AppTheme.SessionTime(lblSessionTime);
            AppTheme.TopBarCombo(cboLanguage);
            cboLanguage.ToolTip = Strings.MainForm_LanguageHint;
            AppTheme.ImageIconButton(btnUser, AppImages.IconUser, AppTheme.Indigo);
            btnUser.ToolTip = Strings.UserProfile_Title;
            AppTheme.ImageIconButton(btnLogout, AppImages.IconPowerOff, AppTheme.TextMuted);
            btnLogout.ToolTip = Strings.Common_Logout;

            lblAlertBadge.BringToFront();

            foreach (var screen in _openScreens.Values)
            {
                screen.Chip.Active = screen.Key == _activeKey;
                screen.Chip.Invalidate(); // 테마 색이 바뀐 경우에도 탭이 새 색으로 다시 그려지도록
                (screen.Content as IThemedScreen)?.ApplyTheme();
            }
        }

        // ────────────────────────────── 사용자 정보 ──────────────────────────────

        private void btnUser_Click(object sender, EventArgs e) => ShowUserProfile();

        /// <summary>
        /// 사용자 정보 팝업을 연다(우측 상단 사용자 아이콘).
        /// 팝업이 서버에서 프로필(사번/이름/연락처/부서)을 받아 세션에 채우므로, 닫힌 뒤 환영 문구를 갱신한다.
        /// </summary>
        private void ShowUserProfile()
        {
            using (var form = new UserProfileForm())
            {
                form.Initialize(_context);
                form.ShowDialog(this);
            }

            RefreshWelcome();
            // 프로필을 방금 받았으면 홈 탭의 사용자 타일도 아이디에서 이름으로 바뀐다.
            _homeScreen?.RefreshInfo();
            LayoutTopBar();
        }

        /// <summary>
        /// 우측 상단 환영 문구 — "{이름} 님 환영합니다.".
        /// 프로필을 아직 못 받았으면 이름 자리에 로그인 아이디를 쓴다.
        /// </summary>
        private void RefreshWelcome()
        {
            var user = _context.CurrentUser;
            if (user == null)
            {
                lblWelcome.Text = string.Empty;
                return;
            }

            var displayName = string.IsNullOrEmpty(user.UserName) ? user.UserId : user.UserName;
            lblWelcome.Text = string.Format(Strings.MainForm_Welcome, displayName);
        }

        // ─────────────────── 상단 바 시계 · 세션 자동 로그아웃 ───────────────────

        /// <summary>
        /// 우측 상단 카운트다운 시작 — App.config의 SessionTimeoutMinutes(기본 300분 = 05:00:00)부터 1초씩 줄어든다.
        /// 로그인 시점부터의 고정 세션 길이이며, 사용자 조작으로 연장되지는 않는다.
        /// (조작할 때마다 연장하는 "유휴 시간 기준"으로 바꾸려면 마우스/키 이벤트에서 ResetSessionCountdown()을 호출하면 된다.)
        /// </summary>
        private void StartSessionCountdown()
        {
            _sessionTimer = new System.Windows.Forms.Timer();
            _sessionTimer.Interval = 1000;
            _sessionTimer.Tick += SessionTimer_Tick;

            ResetSessionCountdown();
            _sessionTimer.Start();

            FormClosed += (s, e) =>
            {
                _sessionTimer.Stop();
                _sessionTimer.Dispose();
            };
        }

        /// <summary>세션 종료 시각을 지금부터 다시 계산한다.</summary>
        private void ResetSessionCountdown()
        {
            var minutes = _context.Settings.SessionTimeoutMinutes > 0 ? _context.Settings.SessionTimeoutMinutes : 300;
            _sessionEndUtc = DateTime.UtcNow.AddMinutes(minutes);
            RefreshSessionTime();
        }

        private void SessionTimer_Tick(object sender, EventArgs e)
        {
            var remaining = _sessionEndUtc - DateTime.UtcNow;
            if (remaining.TotalSeconds > 0)
            {
                RefreshSessionTime();
                return;
            }

            // 만료: 타이머를 멈추고 안내한 뒤 로그아웃(창 닫기)한다.
            _sessionTimer.Stop();
            AppMessageBox.Info(this, Strings.MainForm_SessionExpired, Strings.Common_Notice);
            _context.ApiClients.TokenProvider.Clear();
            Close();
        }

        /// <summary>
        /// 상단 바 가운데 표시를 1초마다 갱신한다 — 현재 일시(<see cref="ClockFormat"/>).
        ///
        /// 자동 로그아웃까지 남은 시간은 화면에서 빠졌지만 판정에는 그대로 쓰이며,
        /// 마우스를 올리면 툴팁에서 확인할 수 있다.
        /// </summary>
        private void RefreshSessionTime()
        {
            var remaining = _sessionEndUtc - DateTime.UtcNow;
            if (remaining.TotalSeconds < 0)
                remaining = TimeSpan.Zero;

            lblSessionTime.Text = DateTime.Now.ToString(ClockFormat, CultureInfo.InvariantCulture);
            lblSessionTime.ToolTip = $"{Strings.MainForm_SessionRemainingHint} : " +
                                     $"{(int)remaining.TotalHours:00}:{remaining.Minutes:00}:{remaining.Seconds:00}";
            CenterSessionTime();
        }

        // ────────────────────────────── 화면 언어 ──────────────────────────────

        /// <summary>
        /// 언어 콤보를 [국기 + 언어명]으로 채우고 현재 문화권을 선택 상태로 만든다.
        /// 국기는 <see cref="ImageComboBoxEdit"/>의 SmallImages에 순서대로 담고 항목이 그 번호를 가리킨다 —
        /// 선택된 항목은 물론 펼친 목록에서도 같은 아이콘이 함께 보인다.
        /// </summary>
        private void BuildLanguageList()
        {
            _suppressLanguageChange = true;

            if (_languageFlags == null)
            {
                _languageFlags = new ImageCollection();
                _languageFlags.ImageSize = new Size(LanguageFlagSize, LanguageFlagSize);

                foreach (var language in Languages)
                    _languageFlags.AddImage(AppImages.Flag(language.FlagCode, LanguageFlagSize));

                cboLanguage.Properties.SmallImages = _languageFlags;
            }

            cboLanguage.Properties.Items.Clear();
            for (var i = 0; i < Languages.Length; i++)
                cboLanguage.Properties.Items.Add(new ImageComboBoxItem(Languages[i].Display, Languages[i].Culture, i));

            var current = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
            var index = Array.FindIndex(Languages, l => l.Culture.StartsWith(current, StringComparison.OrdinalIgnoreCase));
            cboLanguage.SelectedIndex = index >= 0 ? index : 0;
            _suppressLanguageChange = false;
        }

        private void CboLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_suppressLanguageChange)
                return;

            var index = cboLanguage.SelectedIndex;
            if (index < 0 || index >= Languages.Length)
                return;

            ApplyCulture(Languages[index].Culture);
        }

        /// <summary>
        /// 화면 언어를 바꾼다. Strings는 호출 시점의 CurrentUICulture를 보므로,
        /// 문화권을 바꾼 뒤 셸 문구·메뉴를 다시 만들고 열려 있던 화면은 다시 열어 문구를 새로 적용한다.
        /// </summary>
        private void ApplyCulture(string cultureName)
        {
            try
            {
                var culture = CultureInfo.GetCultureInfo(cultureName);
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
            }
            catch (CultureNotFoundException)
            {
                return;
            }

            // 화면들은 문구를 Initialize()에서 한 번 읽으므로, 언어가 바뀌면 닫았다 다시 연다.
            // 홈 탭은 메뉴에서 오는 화면이 아니라 다시 열 수단이 없으니 제외하고, 문구만 갈아 끼운다.
            var openKeys = _openScreens.Keys.Where(k => k != HomeTabKey).ToList();
            var activeKey = _activeKey;
            foreach (var key in openKeys)
                CloseTab(key);

            Text = Strings.MainForm_Title;
            lblBreadcrumb.Text = Strings.MainForm_Home;
            RefreshWelcome();
            RefreshSessionTime();
            cboLanguage.ToolTip = Strings.MainForm_LanguageHint;
            btnUser.ToolTip = Strings.UserProfile_Title;
            btnLogout.ToolTip = Strings.Common_Logout;

            BuildMenu();
            RefreshHomeTabLanguage();

            var root = MenuCatalog.BuildRoot();
            foreach (var key in openKeys)
            {
                var node = FindNode(root, key);
                if (node != null)
                    OpenOrFocusTab(node);
            }

            if (activeKey != null && _openScreens.ContainsKey(activeKey))
                ActivateTab(activeKey);

            LayoutTopBar();
        }

        private static MenuNode FindNode(MenuNode node, string key)
        {
            if (node.Key == key)
                return node;

            foreach (var child in node.Children)
            {
                var found = FindNode(child, key);
                if (found != null)
                    return found;
            }
            return null;
        }

        // ────────────────────────────── 상단 바 ──────────────────────────────

        /// <summary>
        /// 상단 바를 배치한다.
        /// 우측 묶음(왼쪽 → 오른쪽): 언어 · 알림 · "{이름} 님 환영합니다." · 사용자 · 로그아웃
        /// 남은 시간은 이 묶음에서 빼내 <b>상단 바 가로 가운데</b>에 따로 둔다.
        /// </summary>
        private void LayoutTopBar()
        {
            var right = panelTop.Width - 16;

            btnLogout.Top = 11;
            btnLogout.Left = right - btnLogout.Width;

            btnUser.Top = 11;
            btnUser.Left = btnLogout.Left - 4 - btnUser.Width;

            lblWelcome.Top = 21;
            lblWelcome.Left = btnUser.Left - 10 - lblWelcome.Width;

            btnAlerts.Top = 11;
            btnAlerts.Left = lblWelcome.Left - 14 - btnAlerts.Width;

            lblAlertBadge.Top = btnAlerts.Top - 1;
            lblAlertBadge.Left = btnAlerts.Right - 13;

            cboLanguage.Top = 15;
            cboLanguage.Left = btnAlerts.Left - 12 - cboLanguage.Width;

            CenterSessionTime();
        }

        /// <summary>
        /// 남은 시간을 상단 바 가로 가운데에 놓는다.
        /// 시각은 1초마다 바뀌지만 고정폭 글꼴(Consolas)에 자릿수도 일정해서 폭은 그대로다 —
        /// 그래도 글꼴이 없는 PC에서 대체 글꼴이 쓰일 수 있으므로 갱신할 때마다 다시 맞춘다.
        /// </summary>
        private void CenterSessionTime()
        {
            lblSessionTime.Top = 21;

            // 기본은 상단 바 한가운데. 다만 창이 좁으면 좌측 브레드크럼이나 우측 묶음과 겹치므로
            // 두 이웃 사이 구간 안으로 가둔다(글자가 겹쳐 뭉개지는 것보다 조금 치우치는 편이 낫다).
            var centered = (panelTop.Width - lblSessionTime.Width) / 2;
            var leftLimit = lblBreadcrumb.Right + 12;
            var rightLimit = cboLanguage.Left - 12 - lblSessionTime.Width;

            lblSessionTime.Left = rightLimit < leftLimit
                ? leftLimit
                : Math.Min(Math.Max(centered, leftLimit), rightLimit);
        }

        /// <summary>
        /// 사이드바 폭에 맞는 로고를 브랜드 블록에 놓는다.
        /// 펼친 상태(240px)는 로고 전체를, 접은 상태(64px)는 가로로 긴 로고가 들어가지 않으므로
        /// 심벌(초록 지구본)만 잘라 낸 것을 쓴다. 두 경우 모두 brandPanel(높이 74) 안에서 세로 가운데다.
        /// </summary>
        private void ApplyBrandLogo()
        {
            if (_sidebarCollapsed)
            {
                brandIcon.SvgImage = AppImages.BrandMark();
                brandIcon.Bounds = new Rectangle(12, 22, 40, 29);
                return;
            }

            brandIcon.SvgImage = AppImages.BrandSignature(Color.White);
            brandIcon.Bounds = new Rectangle(22, 14, 196, 46);
        }

        private void btnMenuToggle_Click(object sender, EventArgs e)
        {
            _sidebarCollapsed = !_sidebarCollapsed;

            sidePanel.Width = _sidebarCollapsed ? SidebarCollapsedWidth : SidebarWidth;
            ApplyBrandLogo();

            // 접힌 폭(64px)에는 "Ver. 1.0.0.7"이 들어가지 않아 잘린 글자만 남는다 — 아예 숨긴다.
            lblVersion.Visible = !_sidebarCollapsed;

            // AccordionControl 자체의 최소화(아이콘만 표시) 기능을 사용한다.
            accordion.OptionsMinimizing.AllowMinimizing = true;
            accordion.OptionsMinimizing.MinimizedWidth = SidebarCollapsedWidth;
            accordion.OptionsMinimizing.NormalWidth = SidebarWidth;
            accordion.OptionsMinimizing.State = _sidebarCollapsed
                ? AccordionControlState.Minimized
                : AccordionControlState.Normal;
        }

        /// <summary>종 아이콘 → 알림 팝업. 닫은 뒤에는 배지 숫자를 바로 다시 맞춘다(읽음 처리 반영).</summary>
        private async void btnAlerts_Click(object sender, EventArgs e)
        {
            using (var form = new NotificationForm())
            {
                form.Initialize(_context);
                form.CenterOn(this);
                form.ShowDialog(this);
            }

            await RefreshUnreadCountAsync();
        }

        /// <summary>
        /// 안 읽은 알림 개수를 주기적으로(기본 1분) 물어본다. 주기는 App.config의 NotificationPollingSeconds.
        /// 열자마자 한 번 조회해서 로그인 직후에도 배지가 비어 보이지 않게 한다.
        /// </summary>
        private void StartNotificationPolling()
        {
            var seconds = _context.Settings.NotificationPollingSeconds;
            if (seconds <= 0)
            {
                AppLog.Info("[알림] NotificationPollingSeconds <= 0 이라 폴링을 하지 않는다");
                return;
            }

            _ = RefreshUnreadCountAsync();

            _notificationTimer = new System.Windows.Forms.Timer();
            _notificationTimer.Interval = seconds * 1000;
            _notificationTimer.Tick += async (s, e) => await RefreshUnreadCountAsync();
            _notificationTimer.Start();

            FormClosed += (s, e) =>
            {
                _notificationTimer.Stop();
                _notificationTimer.Dispose();
            };
        }

        /// <summary>
        /// 폴링 1회. <b>실패해도 안내 창을 띄우지 않는다</b> —
        /// 서버가 잠깐 끊긴 동안 1분마다 오류 창이 뜨면 작업을 할 수 없기 때문이다(로그만 남기고 이전 숫자를 유지한다).
        /// </summary>
        private async Task RefreshUnreadCountAsync()
        {
            try
            {
                var count = await _context.ApiClients.NotificationApiClient.GetUnreadCountAsync();

                if (IsDisposed)
                    return;

                _unreadNotificationCount = count;
                RefreshAlertBadge();
            }
            catch (ApiException ex)
            {
                AppLog.Warn($"[알림] 미읽음 개수 조회 실패(배지는 이전 값 유지): {ex.Message.Replace(Environment.NewLine, " ")}");
            }
            catch (Exception ex)
            {
                AppLog.Error("[알림] 미읽음 개수 조회 중 예외", ex);
            }
        }

        /// <summary>설비 연결 이상/오류는 화면 로그(POP 콘솔)와 AppLog에 남긴다.</summary>
        private void WireEquipmentAlerts()
        {
            _context.EquipmentManager.ErrorOccurred += (s, e) => AppLog.Warn($"[설비] [{e.EquipmentId}/{e.Channel}] {e.Exception.Message}");
            _context.EquipmentManager.ConnectionChanged += (s, e) =>
            {
                if (e.State == EquipmentConnectionState.Faulted)
                    AppLog.Warn($"[설비] [{e.EquipmentId}/{e.Channel}] 연결 실패 - {e.Message}");
            };
        }

        /// <summary>배지 숫자 = 서버가 알려준 안 읽은 알림 수(10건 이상이면 "9+").</summary>
        private void RefreshAlertBadge()
        {
            lblAlertBadge.Text = _unreadNotificationCount > 9 ? "9+" : _unreadNotificationCount.ToString();
            lblAlertBadge.Visible = _unreadNotificationCount > 0;
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            // 되돌릴 수 없는 동작이므로 공통 확인 창(확인/취소)으로 한 번 묻는다.
            if (!AppMessageBox.Confirm(this, Strings.MainForm_LogoutConfirm))
                return;

            _context.ApiClients.TokenProvider.Clear();
            Close();
        }

        // ────────────────────────────── 메뉴 ──────────────────────────────

        private void Accordion_ElementClick(object sender, ElementClickEventArgs e)
        {
            if (e.Element.Tag is MenuNode node && node.ScreenFactory != null)
                OpenOrFocusTab(node);
        }

        private void BuildMenu()
        {
            var root = MenuCatalog.BuildRoot();
            _breadcrumbs.Clear();
            _menuElements.Clear();

            accordion.Elements.Clear();
            accordion.Elements.Add(BuildAccordionElement(root, Strings.MainForm_Home, 0));
        }

        /// <summary>
        /// 메뉴 항목 하나를 만든다. 깊이(<paramref name="level"/>)에 따라 계층 표현이 달라진다:
        ///  - 루트/대분류 : 분류 아이콘 + 굵은 글씨(펼침 화살표는 AccordionControl이 그린다)
        ///  - 화면(리프)  : 화면별 색 점(●) + 옅은 글씨 + 살짝 어두운 배경 밴드
        /// 들여쓰기·행 높이·글꼴·색은 <see cref="AppTheme.MenuElement"/>가 한 곳에서 정한다.
        /// </summary>
        private AccordionControlElement BuildAccordionElement(MenuNode node, string parentPath, int level, bool lastChild = true)
        {
            var isLeaf = node.Children.Count == 0;

            var element = new AccordionControlElement();
            element.Text = node.Text;
            element.Tag = node;
            element.Expanded = true;
            element.Style = isLeaf ? ElementStyle.Item : ElementStyle.Group;

            AppTheme.MenuElement(element, level, isLeaf);

            if (isLeaf)
            {
                // 리프는 "트리 가지(└) + 화면별 색 점"을 쓴다 — 상위 분류에 매달린 계층이 눈에 보인다.
                element.ImageOptions.SvgImage = AppIcons.TreeLeaf(node.Accent, AppTheme.SidebarGuide, lastChild);
                element.ImageOptions.SvgImageSize = new Size(24, 34);
            }
            else
            {
                var iconColor = level == 0 ? Color.White : AppTheme.SidebarText;
                var iconSize = level == 0 ? 18 : 16;
                element.ImageOptions.SvgImage = AppIcons.Get(node.Icon ?? AppIcons.Dot, iconColor);
                element.ImageOptions.SvgImageSize = new Size(iconSize, iconSize);
            }

            var path = parentPath + " / " + node.Text;
            _breadcrumbs[node.Key] = path;
            _menuElements[node.Key] = element;

            for (var i = 0; i < node.Children.Count; i++)
                element.Elements.Add(BuildAccordionElement(node.Children[i], path, level + 1, i == node.Children.Count - 1));

            return element;
        }

        /// <summary>현재 열린 화면의 메뉴 항목만 보라색으로 강조한다.</summary>
        private void RefreshMenuHighlight()
        {
            foreach (var pair in _menuElements)
            {
                if (pair.Value.Style != ElementStyle.Item)
                    continue;
                AppTheme.MenuElementSelected(pair.Value, pair.Key == _activeKey);
            }
        }

        // ────────────────────────────── 탭 / 화면 ──────────────────────────────

        private void OpenOrFocusTab(MenuNode node)
        {
            if (_openScreens.ContainsKey(node.Key))
            {
                ActivateTab(node.Key);
                return;
            }

            var content = node.ScreenFactory(_context);
            content.Dock = DockStyle.Fill;
            content.Visible = false;
            contentHost.Controls.Add(content);
            WireNavigation(node.Key, content);

            var screen = BuildTabChip(node.Key, node.Text);
            screen.Content = content;
            tabStrip.Controls.Add(screen.Chip);

            _openScreens[node.Key] = screen;
            ActivateTab(node.Key);
        }

        /// <summary>
        /// 화면이 "같은 탭에서 다른 화면으로 넘어가겠다"고 요청하면(<see cref="INavigatingScreen"/>) 내용을 교체한다.
        /// 예: POP 진입 화면에서 [이동] → 같은 탭이 POP 화면으로 바뀐다.
        /// </summary>
        private void WireNavigation(string key, Control content)
        {
            if (!(content is INavigatingScreen navigator))
                return;

            navigator.NavigateRequested += (s, e) =>
            {
                // 요청을 보낸 화면을 그 화면의 이벤트 처리 도중에 Dispose하지 않도록 한 박자 뒤로 미룬다.
                BeginInvoke(new Action(() => SwapTabContent(key, e.Screen, e.Title)));
            };
        }

        /// <summary>탭은 그대로 두고 안에 든 화면만 바꾼다(탭 제목도 함께 갱신).</summary>
        private void SwapTabContent(string key, Control newContent, string title)
        {
            if (IsDisposed || newContent == null)
                return;

            if (!_openScreens.TryGetValue(key, out var screen))
                return;

            var old = screen.Content;

            newContent.Dock = DockStyle.Fill;
            newContent.Visible = false;
            contentHost.Controls.Add(newContent);
            WireNavigation(key, newContent);
            screen.Content = newContent;

            if (old != null)
            {
                contentHost.Controls.Remove(old);
                old.Dispose();
            }

            // 탭 제목이 바뀌면 TabChip이 폭을 알아서 다시 계산한다.
            if (!string.IsNullOrEmpty(title))
                screen.Chip.Text = title;

            ActivateTab(key);
        }

        /// <summary>열린 화면 1개를 나타내는 탭을 만든다(모양/색은 TabChip이 직접 그린다).</summary>
        /// <summary><paramref name="closable"/>가 false면 ✕ 없이 그려진다(홈 탭).</summary>
        private OpenedScreen BuildTabChip(string key, string text, bool closable = true)
        {
            var chip = new TabChip(key, text, closable);
            chip.Activated += (s, e) => ActivateTab(key);
            chip.CloseRequested += (s, e) => CloseTab(key);

            var screen = new OpenedScreen();
            screen.Key = key;
            screen.Chip = chip;
            return screen;
        }

        private void ActivateTab(string key)
        {
            if (!_openScreens.ContainsKey(key))
                return;

            _activeKey = key;

            foreach (var pair in _openScreens)
            {
                var isActive = pair.Key == key;
                pair.Value.Content.Visible = isActive;
                pair.Value.Chip.Active = isActive;
            }

            lblBreadcrumb.Text = _breadcrumbs.ContainsKey(key) ? _breadcrumbs[key] : Strings.MainForm_Home;
            RefreshMenuHighlight();
        }

        private void CloseTab(string key)
        {
            // 홈 탭은 닫지 않는다. 칩에 ✕가 없어 사용자가 부를 일은 없지만,
            // 코드에서 실수로 닫으면 다시 열 수단이 없어(메뉴에 없다) 여기서 막아 둔다.
            if (key == HomeTabKey)
                return;

            if (!_openScreens.TryGetValue(key, out var screen))
                return;

            tabStrip.Controls.Remove(screen.Chip);
            contentHost.Controls.Remove(screen.Content);
            screen.Content.Dispose();
            screen.Chip.Dispose();
            _openScreens.Remove(key);

            if (_activeKey != key)
                return;

            // 닫은 것이 보고 있던 탭이면 홈으로 돌아간다.
            // 홈은 항상 열려 있으므로 "열린 탭이 하나도 없는" 상태는 생기지 않는다.
            _activeKey = null;
            ActivateTab(HomeTabKey);
        }

        /// <summary>스트립 크기가 바뀌면 탭을 다시 그린다(하단 경계선까지 함께 갱신되도록).</summary>
        private void LayoutTabChips()
        {
            tabStrip.Invalidate();

            foreach (var screen in _openScreens.Values)
                screen.Chip.Invalidate();
        }

        // ────────────────────────────── 홈 탭 ──────────────────────────────

        /// <summary>
        /// 홈 탭을 만들어 첫 화면으로 띄운다(앱 시작 시 1회).
        ///
        /// 다른 화면과 달리 <b>메뉴에서 오지 않고 닫히지도 않는다.</b>
        /// 예전에는 "열린 탭이 없을 때만" 보이는 패널이라 화면을 하나라도 열면 홈 정보를 다시 볼 수 없었는데,
        /// 탭으로 만들어 두면 여러 화면을 띄운 상태에서도 홈 탭을 눌러 언제든 돌아올 수 있다.
        /// </summary>
        private void OpenHomeTab()
        {
            _homeScreen = new HomeScreen();
            _homeScreen.Initialize(_context);
            _homeScreen.Dock = DockStyle.Fill;
            _homeScreen.Visible = false;
            contentHost.Controls.Add(_homeScreen);

            var screen = BuildTabChip(HomeTabKey, Strings.MainForm_Home, closable: false);
            screen.Content = _homeScreen;
            tabStrip.Controls.Add(screen.Chip);

            _openScreens[HomeTabKey] = screen;
            _breadcrumbs[HomeTabKey] = Strings.MainForm_Home;

            ActivateTab(HomeTabKey);
        }

        /// <summary>
        /// 언어를 바꿨을 때 홈 탭 문구를 갈아 끼운다.
        /// 다른 화면은 닫았다 다시 열지만 홈은 다시 열 수단이 없어(메뉴에 없다) 제자리에서 고친다.
        /// </summary>
        private void RefreshHomeTabLanguage()
        {
            if (_homeScreen == null)
                return;

            _homeScreen.ApplyLanguage();
            _breadcrumbs[HomeTabKey] = Strings.MainForm_Home;

            if (_openScreens.TryGetValue(HomeTabKey, out var screen))
                screen.Chip.Text = Strings.MainForm_Home;
        }

        /// <summary>언어 콤보 항목 — 표시 문구와 문화권 코드 한 쌍.</summary>
        private class LanguageOption
        {
            public string Display { get; }
            public string Culture { get; }

            /// <summary>국기 파일 이름에 쓰는 두 글자 코드("ko-KR" → "ko").</summary>
            public string FlagCode { get; }

            public LanguageOption(string display, string culture)
            {
                Display = display;
                Culture = culture;
                FlagCode = culture.Substring(0, 2);
            }
        }

        private class OpenedScreen
        {
            public string Key;
            public Control Content;
            public TabChip Chip;
        }
    }
}
