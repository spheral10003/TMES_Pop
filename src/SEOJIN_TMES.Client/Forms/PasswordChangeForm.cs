using SEOJIN_TMES.Client.ApiClient;
using SEOJIN_TMES.Client.Common.Errors;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Forms.Common;
using SEOJIN_TMES.Client.Theme;
using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace SEOJIN_TMES.Client.Forms
{
    /// <summary>
    /// &lt;비밀번호 변경&gt; 팝업 — 초기 비밀번호 계정(<c>pswdInitYn=Y</c>)으로 로그인했을 때 로그인 창이 띄운다.
    /// [적용]이면 <c>PATCH gm/users/{orgNo}/pswd-chg</c>로 바꾸고 OK로 닫힌다. [닫기] · Esc면 Cancel.
    ///
    /// 모양은 웹 MES의 비밀번호 변경 팝업을 따른다 — 흰 머리글(제목) + 가는 선 + 입력 두 줄 + 가는 선 + 버튼 줄.
    /// 버튼 자리는 공용 메시지 창(<see cref="AppMessagePanel"/>)과 같은 오른쪽 아래다.
    /// 컨트롤은 전부 .NET 기본 컨트롤(Label · TextBox · Button)이다.
    ///
    /// 비밀번호 규칙(길이 · 문자 조합 · 재사용 금지) 판정은 서버가 한다 — 여기서는 빈칸과 두 칸 일치만 확인하고,
    /// 서버가 거절하면 그 사유를 그대로 보여 준다. 오류는 메시지 창 대신 로그인 창처럼 본문 아래 붉은 상자에 띄운다.
    /// </summary>
    public partial class PasswordChangeForm : Form
    {
        /// <summary>오류 상자의 최소 높이(한 줄 문구 + 위아래 여백).</summary>
        private const int MinErrorHeight = 40;

        /// <summary>문구 높이에 더하는 상자 안쪽 위아래 여백 합.</summary>
        private const int ErrorTextPadding = 16;

        /// <summary>
        /// 오류 상자를 보일 때 창이 상자 높이에 더해 늘어나는 양. 디자이너의 본문 높이(108)는 두 번째 입력 줄 아래(92)까지에
        /// 여백 16을 둔 크기라, 상자를 100에 놓고 그 아래에 같은 여백 16을 두려면 8이 더 필요하다.
        /// </summary>
        private const int ErrorGap = 8;

        private ApiClientFactory _apiClient;
        private string _orgNo;

        /// <summary>적용 요청이 나가 있는 동안 true — 버튼을 연달아 눌러 요청이 겹치지 않게 막는다.</summary>
        private bool _busy;

        public PasswordChangeForm()
        {
            InitializeComponent();
        }

        /// <param name="apiClient">로그인 직후 토큰이 실린 API 클라이언트.</param>
        /// <param name="orgNo">비밀번호를 바꿀 사번(로그인 아이디와 같은 값).</param>
        public void Initialize(ApiClientFactory apiClient, string orgNo)
        {
            _apiClient = apiClient;
            _orgNo = orgNo;

            Text = Language.PasswordChange_Title;
            lblTitle.Text = Language.PasswordChange_Title;
            lblPassword.Text = Language.PasswordChange_Password;
            lblConfirm.Text = Language.PasswordChange_Confirm;
            btnApply.Text = Language.PasswordChange_Apply;
            btnClose.Text = Language.Common_Close;

            ApplyTheme();
            DialogChrome.Apply(this, pnlTitle);
        }

        private void ApplyTheme()
        {
            foreach (var panel in new[] { pnlTitle, pnlBody, pnlFooter })
                panel.BackColor = AppTheme.Surface;

            pnlTitleLine.BackColor = AppTheme.DialogDivider;
            pnlFooterLine.BackColor = AppTheme.DialogDivider;

            lblTitle.ForeColor = AppTheme.TextPrimary;
            lblTitle.Font = AppTheme.ChromeBoldFont;

            foreach (var label in new[] { lblPassword, lblConfirm })
            {
                label.ForeColor = AppTheme.TextPrimary;
                label.Font = AppTheme.BodyBoldFont;
            }

            // 필수 입력 표시 — 웹 MES 팝업과 같은 붉은 점.
            foreach (var mark in new[] { lblPasswordMark, lblConfirmMark })
                mark.ForeColor = AppTheme.Rose;

            foreach (var box in new[] { txtPassword, txtConfirm })
            {
                box.BackColor = AppTheme.Surface;
                box.ForeColor = AppTheme.TextPrimary;
                box.Font = AppTheme.BodyFont;
            }

            // 오류 상자 — 로그인 창의 오류 상자와 같은 색.
            pnlError.BackColor = AppTheme.LoginErrorBack;
            lblError.BackColor = AppTheme.LoginErrorBack;
            lblError.ForeColor = AppTheme.LoginErrorText;
            lblError.Font = AppTheme.BodyFont;

            AppTheme.PrimaryButton(btnApply);
            AppTheme.StandardButton(btnClose);
        }

        /// <summary>부모 창(없으면 화면) 가운데에 띄운다.</summary>
        public void CenterOn(IWin32Window owner)
        {
            DialogChrome.CenterOn(this, owner);   // 부모가 최소화돼 있어도 화면 안에 놓는다
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            txtPassword.Focus();
        }

        private async void btnApply_Click(object sender, EventArgs e)
        {
            if (_busy)
                return;

            var password = txtPassword.Text;
            var confirm = txtConfirm.Text;

            // 앞서 띄운 오류는 이번 시도와 무관하므로 지운다(로그인 창과 같은 흐름).
            SetError(string.Empty);

            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirm))
            {
                SetError(Language.PasswordChange_Empty);
                (string.IsNullOrEmpty(password) ? txtPassword : txtConfirm).Focus();
                return;
            }

            // 두 칸이 같은지는 해시(SHA256)를 만들기 전에 평문으로 확인한다 — 다르면 서버로 보내지 않는다.
            if (!string.Equals(password, confirm, StringComparison.Ordinal))
            {
                AppLog.Warn($"비밀번호 변경 - 비밀번호 확인 불일치, OrgNo={_orgNo}");
                SetError(Language.PasswordChange_Mismatch);
                txtConfirm.SelectAll();
                txtConfirm.Focus();
                return;
            }

            SetBusy(true);
            try
            {
                var result = await _apiClient.UserProfileApiClient.SetPassword(_orgNo, password);
                if (IsDisposed || Disposing) return;

                // 2xx여도 성공 여부는 서버가 본문(status · message)으로 알려 준 값을 따른다.
                // 본문 없이 2xx만 오면(null) 상태코드로 이미 성공이 확인된 것이다.
                if (result != null && !result.IsSuccess)
                {
                    AppLog.Warn($"비밀번호 변경 실패(서버 응답) - OrgNo={_orgNo}, Status={result.Status}, Code={result.Code}, Message={result.Message}");
                    var reason = string.IsNullOrWhiteSpace(result.Message) ? Language.PasswordChange_Failed : result.Message;
                    SetError(string.IsNullOrWhiteSpace(result.Code) ? reason : $"{reason} ({result.Code})");
                    return;
                }

                // 성공 안내 창은 띄우지 않고 바로 닫는다 — 로그인 창이 이어서 메인 화면으로 넘어간다.
                AppLog.Step($"초기 비밀번호 변경 완료 - OrgNo={_orgNo}, Status={result?.Status}, Message={result?.Message}");
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (ApiException ex) when (ex.IsTransportFailure)
            {
                AppLog.Error($"비밀번호 변경 중 통신 오류 - OrgNo={_orgNo}", ex);
                if (IsDisposed || Disposing) return;

                SetError($"{Language.Login_ConnectionErrorMessage} ({FormatCode(ex)})");
            }
            catch (ApiException ex)
            {
                // 비밀번호 규칙 위반 등 서버가 거절한 사유가 작업자에게 가장 쓸모 있다 — 그대로 보여 주고 창은 남긴다.
                AppLog.Warn($"비밀번호 변경 실패 - OrgNo={_orgNo}, {ex.ToLogText()}");
                if (IsDisposed || Disposing) return;

                var reason = string.IsNullOrWhiteSpace(ex.ServerMessage)
                    ? $"HTTP {(int)ex.StatusCode}"
                    : ex.ServerMessage.Replace(Environment.NewLine, " ").Replace('\n', ' ');
                SetError($"{reason} ({FormatCode(ex)})");
            }
            catch (Exception ex)
            {
                var wrapped = ServerLog.Unexpected("비밀번호 변경", ex);
                if (IsDisposed || Disposing) return;

                var culture = CultureInfo.CurrentUICulture;
                SetError($"{Language.Common_UnexpectedError} ({ErrorText.CodeLine(ErrorText.CodeOf(wrapped), ErrorText.DescriptionOf(wrapped, culture))})");
            }
            finally
            {
                if (!IsDisposed && !Disposing)
                    SetBusy(false);
            }
        }

        /// <summary>
        /// 오류 상자 — 로그인 창의 오류 상자(<c>pnlError</c> · <c>lblMessage</c>)와 같은 붉은 상자에 문구를 띄운다.
        /// 평소엔 숨겨 창을 작게 두고, 문구가 생기면 문구 줄 수만큼 상자를 키우고 창도 그만큼 늘린다
        /// (빈 오류 칸을 늘 두면 아래 빈 공간이 커진다). null · 빈 문자열이면 상자를 숨기고 원래 크기로 되돌린다.
        /// </summary>
        private void SetError(string message)
        {
            var oldExtra = pnlError.Visible ? pnlError.Height + ErrorGap : 0;
            var show = !string.IsNullOrEmpty(message);

            lblError.Text = message ?? string.Empty;
            if (show)
            {
                var textWidth = pnlError.Width - pnlError.Padding.Horizontal - lblError.Padding.Horizontal;
                var text = TextRenderer.MeasureText(message, lblError.Font, new Size(textWidth, int.MaxValue),
                    TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
                pnlError.Height = Math.Max(MinErrorHeight, text.Height + ErrorTextPadding);
            }
            pnlError.Visible = show;

            var newExtra = show ? pnlError.Height + ErrorGap : 0;
            if (newExtra != oldExtra)
                Height += newExtra - oldExtra;
        }

        private void pnlError_Paint(object sender, PaintEventArgs e)
        {
            using (var pen = new Pen(AppTheme.LoginErrorBorder))
                e.Graphics.DrawRectangle(pen, 0, 0, pnlError.Width - 1, pnlError.Height - 1);
        }

        /// <summary>
        /// 오류 문구 끝에 붙이는 "오류 코드 - 설명" — 로그인 창과 같은 형식.
        /// 서버 업무 코드면 설명이 서버 사유라, 앞쪽에 이미 적은 사유와 겹치면 코드만 남긴다.
        /// </summary>
        private static string FormatCode(ApiException ex) =>
            ErrorText.CodeLine(ex.Code, ex.DescriptionFor(CultureInfo.CurrentUICulture), ex.ServerMessage);

        /// <summary>요청 중에는 입력과 두 버튼을 잠근다 — [닫기]도 잠가야 Esc로 응답 도중 창이 닫히지 않는다.</summary>
        private void SetBusy(bool busy)
        {
            _busy = busy;
            txtPassword.Enabled = !busy;
            txtConfirm.Enabled = !busy;
            btnApply.Enabled = !busy;
            btnClose.Enabled = !busy;
            UseWaitCursor = busy;
        }
    }
}
