using System;
using System.Threading.Tasks;
using DevExpress.XtraEditors;
using SEOJIN_MEIS.Client.ApiClient;
using SEOJIN_MEIS.Client.Common.Logging;
using SEOJIN_MEIS.Client.Composition;
using SEOJIN_MEIS.Client.Forms.Common;
using SEOJIN_MEIS.Client.Localization;
using SEOJIN_MEIS.Client.Theme;

namespace SEOJIN_MEIS.Client.Forms
{
    /// <summary>
    /// 사용자 정보 팝업 — 메인 화면 상단바의 아바타/아이디를 클릭하면 열린다.
    ///
    /// 표시 항목의 출처가 두 가지다:
    ///  - 워크스테이션 : 로그인 화면에서 선택한 값(<see cref="SEOJIN_MEIS.Client.Common.Models.UserSession.Workstation"/>) — 클라이언트가 이미 알고 있다.
    ///  - 사번/이름/연락처/부서 : 서버 프로필 조회 API(<see cref="IUserProfileApiClient"/>) 응답.
    ///    팝업이 열릴 때 조회하고, 조회 전에는 "조회 중...", 실패하면 안내 문구를 남기고 값은 "-"로 표시한다.
    ///
    /// VS 디자이너 호환을 위해 매개변수 없는 생성자만 두고, 실제 의존성은 Initialize()로 주입한다.
    /// </summary>
    public partial class UserProfileForm : XtraForm
    {
        private const string EmptyValue = "-";

        private ClientContext _context;

        public UserProfileForm()
        {
            InitializeComponent();
        }

        public void Initialize(ClientContext context)
        {
            _context = context;

            Text = Strings.UserProfile_Title;
            lblEmployeeNoCaption.Text = Strings.UserProfile_EmployeeNo;
            lblUserNameCaption.Text = Strings.UserProfile_UserName;
            lblPhoneCaption.Text = Strings.UserProfile_Phone;
            lblDepartmentCaption.Text = Strings.UserProfile_Department;
            lblWorkstationCaption.Text = Strings.UserProfile_Workstation;
            btnClose.Text = Strings.Common_Close;

            ApplyTheme();
            FillFromSession();

            Shown += async (s, e) => await LoadProfileAsync();
        }

        /// <summary>메인 화면과 같은 톤: 상단은 남보라 그라데이션 헤더, 본문은 흰 카드.</summary>
        private void ApplyTheme()
        {
            AppTheme.Sidebar(headerPanel);
            DialogChrome.Apply(this, headerPanel);
            AppTheme.Avatar(lblAvatar, GetInitial(), 52);
            lblAvatar.Appearance.Font = AppTheme.TitleFont;

            AppTheme.BrandTitle(lblName);
            AppTheme.BrandSubTitle(lblUserId);

            AppTheme.ContentSurface(bodyPanel);
            AppTheme.ContentSurface(footerPanel);

            AppTheme.Caption(lblEmployeeNoCaption);
            AppTheme.Caption(lblUserNameCaption);
            AppTheme.Caption(lblPhoneCaption);
            AppTheme.Caption(lblDepartmentCaption);
            AppTheme.Caption(lblWorkstationCaption);

            AppTheme.BodyText(lblEmployeeNo, true);
            AppTheme.BodyText(lblUserName, true);
            AppTheme.BodyText(lblPhone, true);
            AppTheme.BodyText(lblDepartment, true);
            AppTheme.BodyText(lblWorkstation, true);

            AppTheme.Caption(lblMessage);
        }

        private string GetInitial()
        {
            var user = _context.CurrentUser;
            var source = user != null && !string.IsNullOrEmpty(user.UserName) ? user.UserName : user?.UserId;
            return string.IsNullOrEmpty(source) ? "U" : source.Substring(0, 1).ToUpperInvariant();
        }

        /// <summary>세션에 들어있는 값으로 화면을 채운다(프로필 조회 전에도 워크스테이션/아이디는 바로 보인다).</summary>
        private void FillFromSession()
        {
            var user = _context.CurrentUser;

            lblName.Text = user != null && !string.IsNullOrEmpty(user.UserName) ? user.UserName : (user?.UserId ?? EmptyValue);
            lblUserId.Text = user?.UserId ?? string.Empty;

            lblEmployeeNo.Text = Display(user?.EmployeeNo);
            lblUserName.Text = Display(user?.UserName);
            lblPhone.Text = Display(user?.PhoneNumber);
            lblDepartment.Text = Display(user?.DepartmentName);
            lblWorkstation.Text = Display(user?.Workstation);

            AppTheme.Avatar(lblAvatar, GetInitial(), 52);
            lblAvatar.Appearance.Font = AppTheme.TitleFont;
        }

        /// <summary>
        /// 서버에서 프로필(사번/이름/연락처/부서)을 조회해 세션과 화면에 반영한다.
        /// 서버가 없거나 API가 아직 준비되지 않은 환경에서도 팝업 자체는 정상 동작해야 하므로,
        /// 실패는 예외를 띄우지 않고 안내 문구로만 표시한다.
        /// </summary>
        private async Task LoadProfileAsync()
        {
            // 조회 대상은 로그인한 본인이다(경로에 아이디가 들어간다).
            // 세션이 없으면 부를 수 있는 대상이 없으므로 조회 자체를 건너뛴다.
            var userId = _context.CurrentUser?.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                ShowLoadFailed(Strings.UserProfile_LoadFailed);
                return;
            }

            lblMessage.Text = Strings.UserProfile_Loading;
            try
            {
                var profile = await _context.ApiClients.UserProfileApiClient.GetProfileAsync(userId);
                if (IsDisposed || Disposing) return;   // 응답을 기다리는 사이 사용자가 팝업을 닫았다

                _context.CurrentUser?.ApplyProfile(profile);
                FillFromSession();
                lblMessage.Text = string.Empty;
            }
            catch (ApiException ex)
            {
                AppLog.Warn($"사용자 프로필 조회 실패 - StatusCode={(int)ex.StatusCode}");
                ShowLoadFailed($"{Strings.UserProfile_LoadFailed} ({(int)ex.StatusCode})");
            }
            catch (Exception ex)
            {
                AppLog.Warn($"사용자 프로필 조회 실패 - {ex.Message}");
                ShowLoadFailed(Strings.UserProfile_LoadFailed);
            }
        }

        private void ShowLoadFailed(string message)
        {
            // 조회 실패가 늦게 도착할 수 있다 — 그 사이 팝업이 닫혔으면 만지지 않는다(CLAUDE.md 6번).
            if (IsDisposed || Disposing)
                return;

            lblMessage.Text = message;
            lblMessage.Appearance.ForeColor = AppTheme.Rose;
            lblMessage.Appearance.Options.UseForeColor = true;
        }

        private static string Display(string value) => string.IsNullOrWhiteSpace(value) ? EmptyValue : value;

        private void btnClose_Click(object sender, EventArgs e) => Close();
    }
}
