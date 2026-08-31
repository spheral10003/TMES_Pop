using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using SEOJIN_MEIS.Client.Theme;

namespace SEOJIN_MEIS.Client.Forms.Common
{
    /// <summary>
    /// <see cref="AppMessagePanel"/>을 모달 대화상자로 띄우는 호스트 폼.
    /// 사용자 정보 팝업(<see cref="UserProfileForm"/>)과 같은 형태로 맞췄다 —
    /// 창 테두리가 있는 고정 크기 대화상자 + 상단 색 띠(제목/아이콘) + 옅은 본문.
    /// 이전에는 테두리 없는 흰 카드였는데, 흰 화면 위에서 창 경계가 잘 보이지 않아 이렇게 바꿨다.
    ///
    /// 화면 코드에서는 이 폼을 직접 만들지 말고 <see cref="AppMessageBox"/>를 사용한다.
    /// </summary>
    public partial class AppMessageForm : XtraForm
    {
        private const int DefaultWidth = 420;

        private AppMessagePanel _panel;

        public AppMessageForm()
        {
            InitializeComponent();
        }

        public void Initialize(string message, string title, AppMessageKind kind, AppMessageButtons buttons)
        {
            borderPanel.LookAndFeel.UseDefaultLookAndFeel = false;
            borderPanel.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
            borderPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            borderPanel.Appearance.BackColor = AppTheme.Surface;
            borderPanel.Appearance.Options.UseBackColor = true;
            borderPanel.Padding = new Padding(0);

            _panel = new AppMessagePanel();
            _panel.Dock = DockStyle.Fill;
            borderPanel.Controls.Add(_panel);
            _panel.Initialize(message, title, kind, buttons);

            // 창 제목줄에 이미 닫기(X)가 있으므로 헤더 안의 X는 감춘다(사용자 정보 팝업과 동일한 구성).
            _panel.ShowCloseButton = false;

            // 창 제목은 헤더와 같은 문구를 쓴다(제목을 안 주면 성격별 기본 제목이 들어간다).
            Text = _panel.Title;

            // 메시지 줄 수에 맞춰 창 높이를 잡는다(폭은 고정).
            ClientSize = new Size(DefaultWidth, AppMessagePanel.MeasureHeight(message, DefaultWidth));

            _panel.ResultSelected += Panel_ResultSelected;
            KeyDown += (s, e) => _panel.HandleKey(e.KeyCode);
        }

        /// <summary>부모 창(없으면 화면) 가운데에 띄운다.</summary>
        public void CenterOn(IWin32Window owner)
        {
            var ownerForm = (owner as Control)?.FindForm();
            var bounds = ownerForm != null ? ownerForm.Bounds : Screen.PrimaryScreen.WorkingArea;

            Location = new Point(
                bounds.Left + ((bounds.Width - Width) / 2),
                bounds.Top + ((bounds.Height - Height) / 2));
        }

        private void Panel_ResultSelected(object sender, AppMessageResultEventArgs e)
        {
            DialogResult = e.Result;
            Close();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _panel?.FocusDefault();
        }
    }
}
