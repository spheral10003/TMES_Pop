using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using SEOJIN_MEIS.Client.Localization;
using SEOJIN_MEIS.Client.Theme;

namespace SEOJIN_MEIS.Client.Forms.Common
{
    /// <summary>
    /// 공통 메시지/확인 창의 내용을 담은 유저컨트롤.
    /// 구성은 사용자 정보 팝업과 같은 형태 — 상단 색 띠(성격별 아이콘 + 제목 + X), 옅은 회청색 본문에 좌측 정렬 메시지,
    /// 우측 하단 [확인]/[취소]. 흰 배경만 있던 이전 형태는 흰 화면 위에서 창 경계가 잘 보이지 않아 바꿨다.
    ///
    /// 창(폼)에 대한 의존이 없어서 두 가지로 쓸 수 있다:
    ///  1) <see cref="AppMessageForm"/>이 호스트로 감싸 모달 대화상자로 띄운다(<see cref="AppMessageBox"/> 사용).
    ///  2) 화면 안쪽 패널에 직접 올려 인라인 확인 영역으로 쓴다 — 결과는 <see cref="ResultSelected"/> 이벤트로 받는다.
    ///
    /// VS 디자이너 호환을 위해 매개변수 없는 생성자만 두고, 실제 내용은 Initialize()로 넣는다.
    /// </summary>
    public partial class AppMessagePanel : XtraUserControl
    {
        private const int SidePadding = 24;
        private const int HeaderHeight = 56;
        private const int FooterHeight = 64;
        private const int ButtonGap = 8;

        private AppMessageButtons _buttons = AppMessageButtons.Ok;

        /// <summary>확인/취소/닫기 중 하나가 선택되면 발생한다(닫기 = Cancel).</summary>
        public event EventHandler<AppMessageResultEventArgs> ResultSelected;

        /// <summary>헤더에 표시된 제목(호스트 폼의 창 제목으로도 쓴다).</summary>
        public string Title => lblTitle.Text;

        /// <summary>
        /// 헤더 우측 X 버튼 표시 여부.
        /// 창 제목줄이 있는 호스트(AppMessageForm)에서는 제목줄의 닫기와 겹치므로 false로 둔다.
        /// 화면 안쪽에 인라인으로 올려 쓸 때는 true로 두면 닫기 수단이 된다.
        /// </summary>
        public bool ShowCloseButton
        {
            get { return btnClose.Visible; }
            set { btnClose.Visible = value; }
        }

        public AppMessagePanel()
        {
            InitializeComponent();
        }

        /// <summary>메시지 내용과 버튼 구성을 넣고 테마 색을 적용한다.</summary>
        public void Initialize(string message, string title, AppMessageKind kind, AppMessageButtons buttons)
        {
            _buttons = buttons;

            // 제목을 따로 주지 않아도 헤더는 항상 보여준다(성격별 기본 제목) — 흰 본문만 있으면 창 경계가 잘 안 보이기 때문.
            lblTitle.Text = string.IsNullOrEmpty(title) ? DefaultTitleOf(kind) : title;
            lblMessage.Text = message ?? string.Empty;

            btnOk.Text = Strings.Common_Confirm;
            btnCancel.Text = Strings.Common_Cancel;
            btnCancel.Visible = buttons == AppMessageButtons.OkCancel;

            // X 버튼에 포커스 사각형이 남지 않도록 탭 순서에서 빼고, 기본 포커스는 [확인]에 둔다.
            btnClose.TabStop = false;

            ApplyTheme(kind);
            LayoutButtons();
            Resize += (s, e) => LayoutButtons();
        }

        private void ApplyTheme(AppMessageKind kind)
        {
            var accent = AccentOf(kind);

            // 사용자 정보 팝업과 같은 구성: 상단 색 띠(제목+아이콘) + 옅은 회청색 본문 + 아래 버튼 영역.
            BackColor = AppTheme.Surface;
            AppTheme.DialogHeader(headerPanel, accent);
            AppTheme.DialogHeaderTitle(lblTitle);
            AppTheme.ContentSurface(bodyPanel);
            AppTheme.ContentSurface(footerPanel);

            headerIcon.SvgImage = AppIcons.Get(IconOf(kind), Color.White);
            headerIcon.SizeMode = SvgImageSizeMode.Stretch;
            headerIcon.BackColor = Color.Transparent;

            lblMessage.Appearance.Font = AppTheme.BodyFont;
            lblMessage.Appearance.ForeColor = AppTheme.TextPrimary;
            lblMessage.Appearance.Options.UseFont = true;
            lblMessage.Appearance.Options.UseForeColor = true;
            lblMessage.Appearance.TextOptions.WordWrap = WordWrap.Wrap;
            lblMessage.Appearance.TextOptions.VAlignment = VertAlignment.Top;
            lblMessage.Appearance.Options.UseTextOptions = true;
            lblMessage.AutoSizeMode = LabelAutoSizeMode.None;

            // 색 띠 위에 있으므로 X는 흰색으로 그리고, hover도 띠보다 진한 색으로 맞춘다.
            AppTheme.IconButton(btnClose, AppIcons.Close, Color.White, 14);
            btnClose.AppearanceHovered.BackColor = ControlPaint.Dark(accent, 0.2f);
            btnClose.AppearanceHovered.Options.UseBackColor = true;
        }

        /// <summary>성격별 기본 제목(제목을 지정하지 않았을 때).</summary>
        private static string DefaultTitleOf(AppMessageKind kind)
        {
            switch (kind)
            {
                case AppMessageKind.Question: return Strings.Message_TitleQuestion;
                case AppMessageKind.Success: return Strings.Message_TitleSuccess;
                case AppMessageKind.Warning: return Strings.Message_TitleWarning;
                case AppMessageKind.Error: return Strings.Message_TitleError;
                default: return Strings.Message_TitleInfo;
            }
        }

        /// <summary>성격별 헤더 아이콘.</summary>
        private static string IconOf(AppMessageKind kind)
        {
            switch (kind)
            {
                case AppMessageKind.Question: return AppIcons.Question;
                case AppMessageKind.Success: return AppIcons.Check;
                case AppMessageKind.Warning: return AppIcons.Warning;
                case AppMessageKind.Error: return AppIcons.ErrorMark;
                default: return AppIcons.Info;
            }
        }

        /// <summary>메시지 성격별 강조 색. 시안의 청록 버튼이 기본(Info/Question)이다.</summary>
        private static Color AccentOf(AppMessageKind kind)
        {
            switch (kind)
            {
                case AppMessageKind.Success: return AppTheme.Emerald;
                case AppMessageKind.Warning: return AppTheme.Amber;
                case AppMessageKind.Error: return AppTheme.Rose;
                default: return AppTheme.Teal;
            }
        }

        /// <summary>버튼을 우측 하단으로 정렬한다(취소가 가장 오른쪽, 그 왼쪽이 확인).</summary>
        private void LayoutButtons()
        {
            btnClose.Left = Width - SidePadding + 6 - btnClose.Width;
            btnClose.Top = (HeaderHeight - btnClose.Height) / 2;

            var top = (FooterHeight - btnOk.Height) / 2;
            if (_buttons == AppMessageButtons.OkCancel)
            {
                btnCancel.Top = top;
                btnCancel.Left = Width - SidePadding - btnCancel.Width;
                btnOk.Top = top;
                btnOk.Left = btnCancel.Left - ButtonGap - btnOk.Width;
            }
            else
            {
                btnOk.Top = top;
                btnOk.Left = Width - SidePadding - btnOk.Width;
            }
        }

        /// <summary>
        /// 주어진 폭에서 메시지를 모두 보여주기 위해 필요한 전체 높이.
        /// 호스트 폼이 창 크기를 잡을 때 사용한다(메시지 줄 수에 따라 창이 늘어난다).
        /// </summary>
        public static int MeasureHeight(string message, int width)
        {
            var textWidth = Math.Max(80, width - (SidePadding * 2));
            var textSize = TextRenderer.MeasureText(
                message ?? string.Empty,
                AppTheme.BodyFont,
                new Size(textWidth, int.MaxValue),
                TextFormatFlags.WordBreak);

            var bodyHeight = Math.Max(44, textSize.Height + 16);
            return HeaderHeight + bodyHeight + FooterHeight;
        }

        private void btnOk_Click(object sender, EventArgs e) => Raise(DialogResult.OK);
        private void btnCancel_Click(object sender, EventArgs e) => Raise(DialogResult.Cancel);
        private void btnClose_Click(object sender, EventArgs e) => Raise(DialogResult.Cancel);

        /// <summary>기본 포커스를 [확인] 버튼에 둔다(Enter로 바로 확인 가능).</summary>
        public void FocusDefault() => btnOk.Focus();

        /// <summary>키보드 처리 — Enter는 확인, Esc는 취소(닫기).</summary>
        public void HandleKey(Keys keyCode)
        {
            if (keyCode == Keys.Enter) Raise(DialogResult.OK);
            else if (keyCode == Keys.Escape) Raise(DialogResult.Cancel);
        }

        private void Raise(DialogResult result) =>
            ResultSelected?.Invoke(this, new AppMessageResultEventArgs(result));
    }

    /// <summary>공통 메시지 창에서 선택된 결과.</summary>
    public class AppMessageResultEventArgs : EventArgs
    {
        public DialogResult Result { get; }

        public AppMessageResultEventArgs(DialogResult result)
        {
            Result = result;
        }
    }
}
