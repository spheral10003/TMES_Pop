using DevExpress.Drawing;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using SEOJIN_TMES.Client.ApiClient;
using SEOJIN_TMES.Client.Common.Configuration;
using SEOJIN_TMES.Client.Common.Errors;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Composition;
using SEOJIN_TMES.Client.Core;
using SEOJIN_TMES.Client.Theme;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
//using static DevExpress.Utils.Frames.FrameHelper;


namespace SEOJIN_TMES.Client.Forms
{
    /// <summary>
    /// RestAPI Token 기반 로그인 화면.
    ///
    /// 서버가 nonce 방식이라 로그인은 두 번의 호출로 이뤄진다(순서는 <see cref="AuthApiClient"/>가 담당한다):
    ///  1) GET  api/v1.0/common/my-salt  → salt 수신
    ///  2) POST api/v1.0/auth/login      → { corpCd, orgNo, password = SHA256(SHA256(비밀번호) + salt), salt }
    /// 화면은 평문 비밀번호만 넘기고, 해시/salt 처리는 API 클라이언트 안에서 끝난다(평문은 서버로 나가지 않는다).
    ///
    /// 응답 바디는 AccessToken/RefreshToken만 받는다.
    /// 성공/실패 판정은 HTTP 상태코드로 하고(서버가 401 등으로 응답), 클라이언트는 그 결과만 화면에 반영한다.
    /// 접속서버는 사업장 코드(corpCd)로 로그인 바디에 실어 보내고, 워크스테이션은 메인 화면이 뜬 뒤 고른다(<see cref="MainForm"/>).
    ///
    /// "아이디 저장"을 켜면 로그인 성공 시 아이디만 <see cref="UserPreferences"/>에 남긴다(비밀번호는 저장하지 않는다).
    ///
    /// VS 디자이너에서 열 수 있도록 매개변수 없는 생성자 + InitializeComponent()만 사용하고,
    /// 실제 의존성(ClientContext)은 생성 직후 Initialize()로 주입한다.
    /// </summary>
    public partial class LoginForm : XtraForm
    {
        /// <summary>언어 콤보 국기 아이콘 한 변(px) — 메인 화면 상단 바와 같은 크기.</summary>
        private const int LanguageFlagSize = 20;

        private Setting _setting;
        private ApiClientFactory _apiClient;
        private UserSession _currentUser;
        private UserPreferences _preferences;
        /// <summary>로그인 창 문구에만 쓰는 문화권. 스레드 문화권과는 따로 움직인다.</summary>
        private CultureInfo _loginCulture;

        /// <summary>국기 아이콘 모음. 폼이 살아 있는 동안 콤보가 참조한다.</summary>
        private ImageCollection _languageFlags;

        /// <summary>콤보를 코드로 채우는 동안 SelectedIndexChanged가 문구를 다시 입히지 않도록 막는다.</summary>
        private bool _suppressLanguageChange;

        /// <summary>콤보 오른쪽 그림 화살표(v)의 폭(px) — 이 자리를 누르면 콤보를 연다.</summary>
        private const int ComboArrowWidth = 18;

        /// <summary>제목 줄이 없어 창을 끌어 옮길 때 쓰는 마우스 누른 위치(창 기준). null이면 끄는 중이 아니다.</summary>
        private Point? _dragOffset;

        public LoginForm()
        {
            InitializeComponent();
        }

        /// <summary>디자이너가 만든 UI에 실행 시점 의존성과 다국어 문구를 적용한다.</summary>
        //public void Initialize(ClientContext context)
        public void Init()
        {
            //_context = context;
            _setting = Global.Instance.Settings;
            _apiClient = Global.Instance.Settings.ApiClients;
            _currentUser = Global.Instance.Settings.CurrentUser;
            _loginCulture = GetCulture(_setting.AppSet.DefaultCulture);

            // 로그인 창이 앱의 첫 창이라, 작업 표시줄에 처음 뜨는 아이콘이 이 값이다.
            Icon = AppImages.AppIcon(_setting.AppSet.IsTestServer);
            ShowIcon = true;   // FixedDialog는 기본으로 제목 줄 아이콘을 감춘다

            SetLanguageList();
            LoadUser();
            SetTexts();
            SetTheme();

            // 상태 줄의 네트워크 표시는 윈도우가 알려 주는 연결 변화를 따라간다(창이 닫히면 FormClosed에서 끊는다).
            NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;
        }

        /// <summary>설정 값이 비었거나 잘못됐으면 목록의 첫 언어(한국어)로 대신한다.</summary>
        private CultureInfo GetCulture(string cultureName)
        {
            var option = LanguageOption.All[LanguageOption.IndexOf(cultureName)];
            return CultureInfo.GetCultureInfo(option.Culture);
        }


        // <summary>
        /// 화면 문구를 <see cref="_loginCulture"/>로 입힌다.
        /// 처음 열 때와 언어 콤보를 바꿀 때 모두 이 메서드를 다시 부른다.
        /// </summary>
        private void SetTexts()
        {
            // ID · Password 항목 이름, 제목 줄(TMES Client · TEST/PROD · ✕)은 배경 그림의 글자 그대로라 언어와 상관없이 둔다.
            lblLanguage.Text = Language.Login_Language;
            lblCorp.Text = Language.Login_Business;
            btnConnect.Text = Language.Login_Connect;
            btnExit.Text = Language.Login_Exit;
            chkRememberId.Text = Language.Login_RememberId;
            lblKeyHint.Text = Language.Login_KeyHint;
            lblVersion.Text = string.Format(Language.MainForm_VersionFormat, AppVersion.Current);
            SetCorpList();
            SetNetworkStatus(NetworkInterface.GetIsNetworkAvailable());
            // 앞서 띄운 오류 문구는 이전 언어이므로 지운다.
            lblMessage.Text = string.Empty;
        }

        /// <summary>
        /// 로그인 화면은 배경 그림(<see cref="AppImages.LoginBackground"/>) 한 장 위에 컨트롤을 얹는다.
        /// App.config의 IsTestServer로 그림(TEST/PROD)과 제목 줄 아이콘 · 배지를 고른다.
        /// 얹은 컨트롤은 그림과 이어져 보여야 하므로 그림에서 뽑은 색(<see cref="AppTheme"/>의 Login*)을 입힌다 —
        /// 오른쪽 흰 영역의 글자는 전부 컨트롤로 덮어 다국어로 바꿔 쓰고, 입력 칸 · 콤보는 테두리 없이 그림의 칸 안에 넣는다.
        /// </summary>
        private void SetTheme()
        {
            var testServer = _setting.AppSet.IsTestServer;

            BackgroundImage = AppImages.LoginBackground(testServer);
            BackgroundImageLayout = ImageLayout.None;
            BackColor = AppTheme.LoginBack;

            // 제목 줄 — 그림의 제목 줄을 덮어 다시 만든다(최소화 버튼은 두지 않는다).
            pnlTitle.BackColor = AppTheme.LoginTitleBar;
            picTitleIcon.Image = AppImages.AppLogo(testServer, picTitleIcon.Width);
            lblBadge.Text = testServer ? "TEST" : "PROD";
            lblBadge.BackColor = testServer ? AppTheme.LoginBadgeTest : AppTheme.LoginBadgeProd;
            btnClose.BackColor = AppTheme.LoginTitleBar;
            btnClose.FlatAppearance.MouseOverBackColor = AppTheme.LoginTitleCloseHover;

            foreach (var label in new[] { lblLanguage, lblCorp, lblUserId, lblPassword })
            {
                label.Appearance.BackColor = AppTheme.LoginBack;
                label.Appearance.ForeColor = AppTheme.LoginText;
            }

            foreach (var box in new[] { pnlLanguage, pnlCorp })
                box.BackColor = AppTheme.LoginBack;

            foreach (var combo in new[] { cboLanguage, cboCorp })
                combo.BackColor = AppTheme.LoginBack;

            FitComboInBox(cboLanguage, pnlLanguage);
            FitComboInBox(cboCorp, pnlCorp);

            txtUserId.BackColor = AppTheme.LoginBack;
            txtPassword.BackColor = AppTheme.LoginBack;

            chkRememberId.BackColor = AppTheme.LoginBack;
            chkRememberId.ForeColor = AppTheme.LoginSubText;

            // 접속 버튼은 AcceptButton(Enter)이라 기본 버튼 테두리가 그려진다 — 테두리 색을 바탕과 같게 해 감춘다.
            btnConnect.BackColor = AppTheme.LoginButton;
            btnConnect.FlatAppearance.BorderColor = AppTheme.LoginButton;
            btnConnect.FlatAppearance.MouseOverBackColor = AppTheme.LoginButton;
            btnConnect.FlatAppearance.MouseDownBackColor = AppTheme.LoginButtonPressed;
            btnConnect.ForeColor = Color.White;

            btnExit.BackColor = AppTheme.LoginBack;
            btnExit.ForeColor = AppTheme.LoginText;
            btnExit.FlatAppearance.BorderColor = AppTheme.LoginLine;
            btnExit.FlatAppearance.MouseOverBackColor = AppTheme.LoginStatusBack;

            lblKeyHint.BackColor = AppTheme.LoginBack;
            lblKeyHint.ForeColor = AppTheme.LoginHint;

            pnlStatus.BackColor = AppTheme.LoginStatusBack;
            lblNetwork.ForeColor = AppTheme.LoginSubText;
            lblVersion.ForeColor = AppTheme.LoginSubText;

            lblMessage.Appearance.ForeColor = AppTheme.LoginErrorText;
            lblMessage.Appearance.Options.UseForeColor = true;
            lblMessage.Text = string.Empty;
            SetErrorBox();
        }

        /// <summary>
        /// 오류 상자 — 문구가 있을 때만 붉은 상자로 보인다.
        /// 문구가 없으면 흰색으로 칠해, 운용 그림(login_PROD.png)에 그려진 붉은 상자도 함께 가린다.
        /// lblMessage.Text가 바뀔 때마다(lblMessage_TextChanged) 다시 맞춘다.
        /// </summary>
        private void SetErrorBox()
        {
            pnlError.BackColor = string.IsNullOrEmpty(lblMessage.Text) ? AppTheme.LoginBack : AppTheme.LoginErrorBack;
            pnlError.Invalidate();
        }

        private void lblMessage_TextChanged(object sender, EventArgs e) => SetErrorBox();

        private void pnlError_Paint(object sender, PaintEventArgs e)
        {
            if (string.IsNullOrEmpty(lblMessage.Text))
                return;

            using (var pen = new Pen(AppTheme.LoginErrorBorder))
                e.Graphics.DrawRectangle(pen, 0, 0, pnlError.Width - 1, pnlError.Height - 1);
        }

        /// <summary>접속 중(Enabled=false)에는 그림의 비활성 LOGIN처럼 회색으로 바꾼다.</summary>
        private void btnConnect_EnabledChanged(object sender, EventArgs e)
        {
            var back = btnConnect.Enabled ? AppTheme.LoginButton : AppTheme.LoginButtonDisabled;
            btnConnect.BackColor = back;
            btnConnect.FlatAppearance.BorderColor = back;
        }

        /// <summary>
        /// 상태 줄 왼쪽 — 이 PC의 네트워크 연결 여부(윈도우가 알려 주는 값)다. 서버 응답 여부는 아니다.
        /// </summary>
        private void SetNetworkStatus(bool online)
        {
            lblNetworkDot.ForeColor = online ? AppTheme.LoginOnline : AppTheme.LoginOffline;
            lblNetwork.Text = online ? Language.Login_Online : Language.Login_Offline;
        }

        /// <summary>연결 변화 알림은 작업 스레드에서 오므로 화면 스레드로 넘겨서 바꾼다.</summary>
        private void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            try
            {
                if (IsDisposed || Disposing || !IsHandleCreated)
                    return;

                BeginInvoke(new Action<bool>(SetNetworkStatus), e.IsAvailable);
            }
            catch (Exception ex)
            {
                // 창이 닫히는 순간과 겹치면 BeginInvoke가 실패할 수 있다 — 표시만 못 할 뿐이라 로그만 남긴다.
                AppLog.Warn($"로그인 창 네트워크 상태 표시 실패 - {ex.Message}");
            }
        }

        /// <summary>NetworkChange는 정적 이벤트라 끊지 않으면 닫힌 창이 계속 붙잡혀 있다.</summary>
        private void LoginForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            NetworkChange.NetworkAvailabilityChanged -= NetworkChange_NetworkAvailabilityChanged;
        }

        /// <summary>
        /// 언어 콤보를 [국기 + 각 나라 말로 적은 언어 이름]으로 채우고 <see cref="_loginCulture"/>를 선택해 둔다.
        /// 목록은 메인 화면과 같은 <see cref="LanguageOption.All"/>을 쓴다.
        /// </summary>
        private void SetLanguageList()
        {
            _suppressLanguageChange = true;

            _languageFlags = new ImageCollection();
            _languageFlags.ImageSize = new Size(LanguageFlagSize, LanguageFlagSize);

            foreach (var language in LanguageOption.All)
            {
                _languageFlags.AddImage(AppImages.Flag(language.FlagCode, LanguageFlagSize));
            }
                
            // 국기는 콤보가 직접 그린다(cboItem_DrawItem) — 순서는 LanguageOption.All과 같다.
            cboLanguage.Items.Clear();

            for (var i = 0; i < LanguageOption.All.Length; i++)
            {
                var option = LanguageOption.All[i];
                cboLanguage.Items.Add(option.Display);
            }

            cboLanguage.SelectedIndex = LanguageOption.IndexOf(_loginCulture.Name);

            Thread.CurrentThread.CurrentUICulture = _loginCulture;

            _suppressLanguageChange = false;
        }

        /// <summary>
        /// 저장해 둔 아이디가 있으면 채워 넣고 커서를 비밀번호로 옮긴다(바로 비밀번호만 치면 되도록).
        /// </summary>
        private void LoadUser()
        {
            _preferences = UserPreferences.Load();

            chkRememberId.Checked = _preferences.RememberUserId;

            if (!_preferences.RememberUserId || string.IsNullOrEmpty(_preferences.UserId))
                return;

            txtUserId.Text = _preferences.UserId;
            ActiveControl = txtPassword;
        }

        /// <summary>
        /// 로그인 성공 시점에만 아이디 저장 상태를 갱신한다(실패한 아이디를 남기지 않기 위함).
        /// 체크를 끄면 이전에 저장해 둔 아이디도 함께 지운다. 비밀번호는 어떤 경우에도 저장하지 않는다.
        /// </summary>
        private void SaveUserIdPreference(string userId)
        {
            _preferences.RememberUserId = chkRememberId.Checked;
            _preferences.UserId = chkRememberId.Checked ? userId : null;
            _preferences.Save();
        }

        /// <summary>
        /// 사업부 콤보 — 항목은 하나다. App.config의 ServerName(GUM 구미 · HS 화성 · YI 용인)에 맞는
        /// 이름("Texon 구미" 등)을 보여 주고, 보이는 글자만 다국어로 바꾼다.
        /// 서버로 보내는 사업장 코드는 콤보 값이 아니라 ServerName을 그대로 쓴다(btnConnect_Click).
        /// </summary>
        private void SetCorpList()
        {
            cboCorp.Items.Clear();
            cboCorp.Items.Add(GetCorpName(_setting.AppSet.ServerName));
            cboCorp.SelectedIndex = 0;
        }

        /// <summary>
        /// 사업장 코드 → 표시 이름. 모르는 코드는 구미로 바꿔 보이지 않도록 코드를 그대로 보여 준다
        /// (설정이 틀렸을 때 화면에서 바로 알아볼 수 있게).
        /// </summary>
        private static string GetCorpName(string serverName)
        {
            switch ((serverName ?? string.Empty).Trim().ToUpperInvariant())
            {
                case "GUM": return Language.Login_CorpGumi;
                case "HS": return Language.Login_CorpHwaseong;
                case "YI": return Language.Login_CorpYongin;
                default: return serverName;
            }
        }

        /// <summary>
        /// .NET ComboBox는 테두리를 끌 수 없어(Flat도 1px 테두리 + 회색 화살표 버튼이 남는다),
        /// 칸 안쪽 크기의 패널에 콤보를 조금 크게 넣어 테두리와 화살표 버튼을 패널 밖으로 잘라 낸다.
        /// 화살표는 배경 그림의 것(v)이 그대로 보인다. 패널은 원래 세로 가운데를 유지한다.
        /// </summary>
        private void FitComboInBox(System.Windows.Forms.ComboBox combo, Panel box)
        {
            var centerY = box.Top + box.Height / 2;
            const int border = 3;

            box.Height = combo.Height - border * 2;
            box.Top = centerY - box.Height / 2;
            combo.Location = new Point(-border, -border);
            combo.Width = box.Width + border + SystemInformation.VerticalScrollBarWidth + border;
            combo.DropDownWidth = box.Width;
        }

        /// <summary>
        /// 콤보 항목을 그린다 — 칸 안쪽 색 바탕 + (언어 콤보면) 국기 + 이름.
        /// 펼친 목록에서 고른 줄만 접속 버튼 색으로 칠한다.
        /// </summary>
        private void cboItem_DrawItem(object sender, DrawItemEventArgs e)
        {
            var combo = (System.Windows.Forms.ComboBox)sender;
            if (e.Index < 0)
                return;

            var inList = (e.State & DrawItemState.ComboBoxEdit) == 0;
            var highlighted = inList && (e.State & DrawItemState.Selected) != 0;
            var back = highlighted ? AppTheme.LoginButton : AppTheme.LoginBack;
            var fore = highlighted ? Color.White : AppTheme.LoginText;

            using (var brush = new SolidBrush(back))
                e.Graphics.FillRectangle(brush, e.Bounds);

            var textLeft = e.Bounds.Left + 8;
            if (combo == cboLanguage && _languageFlags != null && e.Index < _languageFlags.Images.Count)
            {
                var flagTop = e.Bounds.Top + (e.Bounds.Height - LanguageFlagSize) / 2;
                e.Graphics.DrawImage(_languageFlags.Images[e.Index], textLeft, flagTop, LanguageFlagSize, LanguageFlagSize);
                textLeft += LanguageFlagSize + 8;
            }

            var textBounds = new Rectangle(textLeft, e.Bounds.Top, e.Bounds.Right - textLeft, e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, combo.GetItemText(combo.Items[e.Index]), combo.Font, textBounds, fore,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        /// <summary>콤보 오른쪽, 배경 그림의 화살표(v) 자리.</summary>
        private static Rectangle ComboArrowArea(Panel box) =>
            Rectangle.FromLTRB(box.Right, box.Top - 4, box.Right + ComboArrowWidth, box.Bottom + 4);

        /// <summary>
        /// 제목 줄이 없으므로 빈 바탕을 끌어서 창을 옮긴다.
        /// 그림 화살표(v) 자리를 누르면 끌지 않고 그 콤보를 연다 — 화살표는 그림이라 스스로 클릭을 받지 못한다.
        /// </summary>
        private void LoginForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if (ComboArrowArea(pnlLanguage).Contains(e.Location))
            {
                cboLanguage.Focus();
                cboLanguage.DroppedDown = true;
                return;
            }

            if (ComboArrowArea(pnlCorp).Contains(e.Location))
            {
                cboCorp.Focus();
                cboCorp.DroppedDown = true;
                return;
            }

            _dragOffset = e.Location;
        }

        private void LoginForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragOffset == null)
                return;

            var offset = _dragOffset.Value;
            Location = new Point(Location.X + e.X - offset.X, Location.Y + e.Y - offset.Y);
        }

        private void LoginForm_MouseUp(object sender, MouseEventArgs e)
        {
            _dragOffset = null;
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            var userId = txtUserId.Text?.Trim();
            var password = txtPassword.Text;

            

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password))
            {
                lblMessage.Text = Language.Login_ValidationMessage;
                return;
            }

            // 사업장 코드(corpCd) — 서버가 이 값으로 계정을 찾는다.
            // 사업부 콤보는 표시("Texon 구미" 등)만 하고, 코드는 App.config의 ServerName(GUM/HS/YI)을 쓴다.
            //var corpCd = cboServer.Text?.Trim();
            var corpCd = _setting.AppSet.ServerName;
            btnConnect.Enabled = false;
            lblMessage.Text = string.Empty;
            AppLog.Step($"로그인 시도 - CorpCd={corpCd}, UserId={userId}");

            try
            {
                // salt 조회 → SHA256(비밀번호) → SHA256(그 값 + salt) → 로그인. 네 단계 모두 AuthApiClient 안에서 이어진다.
                LoginResponse result = await _apiClient.AuthApiClient.LoginAsync(corpCd, userId, password);

                _apiClient.TokenProvider.SetTokens(result.AccessToken, result.RefreshToken);
                SaveUserIdPreference(userId);

                var userProfile = await _apiClient.UserProfileApiClient.GetProfileAsync(userId);
                if (IsDisposed || Disposing) return;

                // 초기 비밀번호(pswdInitYn=Y)인 계정은 POP에 들이지 않는다 — 웹에서 비밀번호를 바꾼 뒤 다시 로그인하게 한다.
                // 받은 토큰은 버린다(남겨 두면 로그인 창 뒤에서 인증된 호출이 가능한 상태로 남는다).
                // 아이디 저장은 그대로 둔다 — 비밀번호를 바꾸고 돌아와 바로 다시 로그인하기 편하게.
                if (userProfile?.IsInitialPassword == true)
                {
                    _apiClient.TokenProvider.Clear();
                    AppLog.Warn($"초기 비밀번호 계정이라 로그인 차단 - UserId={userId}");
                    // 다른 로그인 실패와 같은 자리(오류 상자)에 띄운다 — 문구는 라벨이 줄바꿈한다.
                    lblMessage.Text = Language.Login_InitialPasswordMessage;
                    return;
                }

                // 워크스테이션은 여기서 고르지 않는다 — 메인 화면이 뜨면서 선택 화면(LoginStationControl)을 덮어 띄운다.
                // 워크스테이션이 없어도 로그인은 통과시키고, 공정 시작(작업 진입) 단계에서 막는다.
                _currentUser = new UserSession
                {
                    UserId = userId,
                    Server = corpCd
                };

                _setting.SaveCulture(_loginCulture.Name);
                Global.Instance.Settings.CurrentUser = _currentUser;

                ServerLog.LoggedIn(userId, corpCd);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                AppLog.Warn($"로그인 실패(401 Unauthorized) - UserId={userId}");
                lblMessage.Text = Language.Login_FailedMessage;
            }
            catch (ApiException ex) when (ex.IsTransportFailure)
            {
                // 서버 미기동/네트워크 단절/타임아웃 — 상세 사유는 로그로 남기고, 라벨에는 한 줄 안내만 띄운다.
                AppLog.Error($"로그인 중 통신 오류 - UserId={userId}", ex);
                lblMessage.Text = $"{Language.Login_ConnectionErrorMessage} ({FormatCode(ex)})";
            }
            catch (ApiException ex)
            {
                AppLog.Error($"로그인 실패 - UserId={userId}, {ex.ToLogText()}");

                // 서버가 사유를 주었으면(계정 잠김 등) 그 사유가 작업자에게 가장 쓸모 있다 — 한 줄 라벨이라 줄바꿈은 편다.
                var reason = string.IsNullOrWhiteSpace(ex.ServerMessage)
                    ? $"HTTP {(int)ex.StatusCode}"
                    : ex.ServerMessage.Replace(Environment.NewLine, " ").Replace('\n', ' ');
                lblMessage.Text = $"{Language.Login_FailedMessage} - {reason} ({FormatCode(ex)})";
            }
            catch (Exception ex)
            {
                var wrapped = ServerLog.Unexpected("로그인", ex);
                lblMessage.Text = $"{Language.Login_ConnectionErrorMessage}: {ex.Message} ({ErrorText.CodeLine(ErrorText.CodeOf(wrapped), ErrorText.DescriptionOf(wrapped, CultureInfo.CurrentUICulture))})";
            }
            finally
            {
                if (!IsDisposed && !Disposing)
                    btnConnect.Enabled = true;
            }
        }

        /// <summary>
        /// 안내 라벨 끝에 붙이는 "오류 코드: XXX - 설명" — 라벨은 한 줄이라 ErrorText.WithCode(빈 줄 + 코드 줄) 대신 괄호로 잇는다.
        /// 서버 업무 코드면 설명이 서버 사유라, 라벨 앞쪽에 이미 적은 사유와 겹치면 코드만 남긴다.
        /// </summary>
        private string FormatCode(ApiException ex) =>
            ErrorText.CodeLine(ex.Code, ex.DescriptionFor(CultureInfo.CurrentUICulture), ex.ServerMessage);

        private void btnClose_Click(object sender, EventArgs e) => Close();

        private void cboLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.SuspendLayout();

            var index = cboLanguage.SelectedIndex;
            if (index < 0 || index >= LanguageOption.All.Length)
                return;
            
            _loginCulture = CultureInfo.GetCultureInfo(LanguageOption.All[index].Culture);
            Thread.CurrentThread.CurrentUICulture = _loginCulture;
            SetTexts();

            this.ResumeLayout(false);
            
        }


    }
}
