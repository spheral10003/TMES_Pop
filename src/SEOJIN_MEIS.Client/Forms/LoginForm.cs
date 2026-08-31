using System;
using System.Drawing;
using System.Net;
using System.Windows.Forms;
using SEOJIN_MEIS.Client.ApiClient;
using SEOJIN_MEIS.Client.Common.Configuration;
using SEOJIN_MEIS.Client.Common.Logging;
using SEOJIN_MEIS.Client.Common.Models;
using SEOJIN_MEIS.Client.Composition;
using SEOJIN_MEIS.Client.Localization;
using SEOJIN_MEIS.Client.Theme;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;

namespace SEOJIN_MEIS.Client.Forms
{
    /// <summary>
    /// RestAPI Token 기반 로그인 화면.
    ///
    /// 서버가 nonce 방식이라 로그인은 두 번의 호출로 이뤄진다(순서는 <see cref="AuthApiClient"/>가 담당한다):
    ///  1) GET  api/v1.0/common/my-salt  → salt 수신
    ///  2) POST api/v1.0/auth/login      → { orgNo, password = SHA256(비밀번호 + salt), salt }
    /// 화면은 평문 비밀번호만 넘기고, 해시/salt 처리는 API 클라이언트 안에서 끝난다(평문은 서버로 나가지 않는다).
    ///
    /// 응답 바디는 AccessToken/RefreshToken만 받는다.
    /// 성공/실패 판정은 HTTP 상태코드로 하고(서버가 401 등으로 응답), 클라이언트는 그 결과만 화면에 반영한다.
    /// 접속서버/워크스테이션 항목은 로그인 API 인자에는 포함하지 않는다(TODO: 용도 확정 후 연결).
    ///
    /// "아이디 저장"을 켜면 로그인 성공 시 아이디만 <see cref="UserPreferences"/>에 남긴다(비밀번호는 저장하지 않는다).
    ///
    /// VS 디자이너에서 열 수 있도록 매개변수 없는 생성자 + InitializeComponent()만 사용하고,
    /// 실제 의존성(ClientContext)은 생성 직후 Initialize()로 주입한다.
    /// </summary>
    public partial class LoginForm : DevExpress.XtraEditors.XtraForm
    {
        private ClientContext _context;
        private UserPreferences _preferences;

        public LoginForm()
        {
            InitializeComponent();
        }

        /// <summary>디자이너가 만든 UI에 실행 시점 의존성과 다국어 문구를 적용한다.</summary>
        public void Initialize(ClientContext context)
        {
            _context = context;

            Text = Strings.Login_Title;
            groupUserInfo.Text = Strings.Login_UserInfoGroup;
            lblServer.Text = Strings.Login_Server;
            lblUserId.Text = Strings.Login_UserId;
            lblPassword.Text = Strings.Login_Password;
            lblWorkstation.Text = Strings.Login_Workstation;
            btnConnect.Text = Strings.Login_Connect;
            btnClose.Text = Strings.Login_Close;
            chkRememberId.Text = Strings.Login_RememberId;

            if (cboServer.Properties.Items.Count > 0) cboServer.SelectedIndex = 0;
            if (cboWorkstation.Properties.Items.Count > 0) cboWorkstation.SelectedIndex = 0;

            RestoreSavedUserId();
            ApplyTheme();
        }

        /// <summary>
        /// 로그인 화면에 공통 테마를 입힌다 — 좌측은 메인 화면과 같은 남보라 그라데이션 브랜드 블록,
        /// 우측은 흰 카드 + 포인트 컬러 버튼. 색 값은 전부 <see cref="AppTheme"/>에서 가져온다.
        /// </summary>
        private void ApplyTheme()
        {
            AppTheme.Sidebar(panelLeft);
            BuildBrandBlock();

            AppTheme.ContentSurface(panelRight);
            AppTheme.ContentSurface(panelBottom);
            AppTheme.CardGroup(groupUserInfo, AppTheme.Violet);

            AppTheme.BodyText(lblServer);
            AppTheme.BodyText(lblUserId);
            AppTheme.BodyText(lblPassword);
            AppTheme.BodyText(lblWorkstation);

            AppTheme.Editor(cboServer);
            AppTheme.Editor(txtUserId);
            AppTheme.Editor(txtPassword);
            AppTheme.Editor(cboWorkstation);
            AppTheme.CheckBox(chkRememberId);

            AppTheme.AccentButton(btnConnect, AppTheme.Indigo);
            AppTheme.QuietButton(btnClose, AppTheme.Slate);

            lblMessage.Appearance.ForeColor = AppTheme.Rose;
            lblMessage.Appearance.Font = AppTheme.BodyBoldFont;
            lblMessage.Appearance.Options.UseForeColor = true;
            lblMessage.Appearance.Options.UseFont = true;
        }

        /// <summary>
        /// 저장해 둔 아이디가 있으면 채워 넣고 커서를 비밀번호로 옮긴다(바로 비밀번호만 치면 되도록).
        /// </summary>
        private void RestoreSavedUserId()
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
        /// 좌측 패널을 [일러스트 + 제품명] 블록으로 채운다.
        /// 위쪽은 Images\login_icon.png(배경 투명 스마트팩토리 일러스트), 아래는 제품명 한 줄이다.
        /// 라벨은 글자 길이에 따라 폭이 달라지므로 좌표를 박아 두지 않고 패널 기준으로 가운데에 맞춘다.
        /// </summary>
        private void BuildBrandBlock()
        {
            picLogo.Dock = DockStyle.None;
            picLogo.Bounds = new Rectangle(20, 10, 220, 188);
            picLogo.Properties.SizeMode = PictureSizeMode.Squeeze;   // 원본 비율 유지 + 넘치면 줄여서 맞춘다
            picLogo.Properties.BorderStyle = BorderStyles.NoBorder;
            picLogo.BackColor = Color.Transparent;
            picLogo.Properties.Appearance.BackColor = Color.Transparent;
            picLogo.Properties.Appearance.Options.UseBackColor = true;
            picLogo.Image = AppImages.LoginIllustration();
            picLogo.Visible = true;

            var title = new LabelControl();
            title.Text = "MEIS POP";
            title.Appearance.Font = new Font("맑은 고딕", 15F, FontStyle.Bold);
            title.Appearance.ForeColor = Color.White;
            title.Appearance.Options.UseFont = true;
            title.Appearance.Options.UseForeColor = true;

            panelLeft.Controls.Add(title);

            // 라벨은 자동 크기라서 부모에 붙인 뒤에야 실제 폭을 알 수 있다 — 그 다음에 가운데로 옮긴다.
            CenterInPanelLeft(title, 206);
        }

        private void CenterInPanelLeft(Control control, int top) =>
            control.Location = new Point((panelLeft.Width - control.Width) / 2, top);

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            var userId = txtUserId.Text?.Trim();
            var password = txtPassword.Text;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password))
            {
                lblMessage.Text = Strings.Login_ValidationMessage;
                return;
            }

            btnConnect.Enabled = false;
            lblMessage.Text = string.Empty;
            AppLog.Step($"로그인 시도 - UserId={userId}");
            try
            {
                // salt 조회 → SHA256(비밀번호 + salt) → 로그인. 세 단계 모두 AuthApiClient 안에서 이어진다.
                //LoginResponse result = await _context.ApiClients.AuthApiClient.LoginAsync(userId, password);

                //_context.ApiClients.TokenProvider.SetTokens(result.AccessToken, result.RefreshToken);
                //SaveUserIdPreference(userId);

                // 로그인 화면에서 선택한 접속서버/워크스테이션은 로그인 API 인자에는 없지만
                // 화면 표시용으로 세션에 담아둔다(사용자 정보 팝업의 "워크스테이션" 항목이 이 값을 쓴다).
                _context.CurrentUser = new UserSession
                {
                    UserId = userId,
                    Server = cboServer.Text,
                    Workstation = cboWorkstation.Text
                };
                AppLog.Step($"로그인 성공 - UserId={userId}");

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                AppLog.Warn($"로그인 실패(401 Unauthorized) - UserId={userId}");
                lblMessage.Text = Strings.Login_FailedMessage;
            }
            catch (ApiException ex) when (ex.IsTransportFailure)
            {
                // 서버 미기동/네트워크 단절/타임아웃 — 상세 사유는 로그로 남기고, 라벨에는 한 줄 안내만 띄운다.
                AppLog.Error($"로그인 중 통신 오류 - UserId={userId}", ex);
                lblMessage.Text = Strings.Login_ConnectionErrorMessage;
            }
            catch (ApiException ex)
            {
                AppLog.Error($"로그인 실패 - UserId={userId}, StatusCode={(int)ex.StatusCode}");
                lblMessage.Text = $"{Strings.Login_FailedMessage} ({(int)ex.StatusCode})";
            }
            catch (Exception ex)
            {
                AppLog.Error($"로그인 중 예외 - UserId={userId}", ex);
                lblMessage.Text = $"{Strings.Login_ConnectionErrorMessage}: {ex.Message}";
            }
            finally
            {
                btnConnect.Enabled = true;
            }
        }

        private void btnClose_Click(object sender, EventArgs e) => Close();
    }
}
