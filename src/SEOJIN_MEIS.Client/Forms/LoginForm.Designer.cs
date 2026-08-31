namespace SEOJIN_MEIS.Client.Forms
{
    partial class LoginForm
    {
        /// <summary>디자이너에서 사용하는 필수 변수입니다.</summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>사용 중인 모든 리소스를 정리합니다.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 이 메서드의 내용을
        /// 코드 편집기로 수정하지 마세요. (람다식/삼항연산자/문자열보간/객체이니셜라이저 사용 금지 —
        /// 사용 시 VS 디자이너 로드 오류가 재발합니다. 실제 문구/이벤트 로직은 LoginForm.cs에 둘 것)
        /// </summary>
        private void InitializeComponent()
        {
            this.panelLeft = new DevExpress.XtraEditors.PanelControl();
            this.picLogo = new DevExpress.XtraEditors.PictureEdit();
            this.panelRight = new DevExpress.XtraEditors.PanelControl();
            this.groupUserInfo = new DevExpress.XtraEditors.GroupControl();
            this.lblServer = new DevExpress.XtraEditors.LabelControl();
            this.cboServer = new DevExpress.XtraEditors.ComboBoxEdit();
            this.lblUserId = new DevExpress.XtraEditors.LabelControl();
            this.txtUserId = new DevExpress.XtraEditors.TextEdit();
            this.lblPassword = new DevExpress.XtraEditors.LabelControl();
            this.txtPassword = new DevExpress.XtraEditors.TextEdit();
            this.lblWorkstation = new DevExpress.XtraEditors.LabelControl();
            this.cboWorkstation = new DevExpress.XtraEditors.ComboBoxEdit();
            this.chkRememberId = new DevExpress.XtraEditors.CheckEdit();
            this.panelBottom = new DevExpress.XtraEditors.PanelControl();
            this.lblMessage = new DevExpress.XtraEditors.LabelControl();
            this.btnConnect = new DevExpress.XtraEditors.SimpleButton();
            this.btnClose = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.panelLeft)).BeginInit();
            this.panelLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelRight)).BeginInit();
            this.panelRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupUserInfo)).BeginInit();
            this.groupUserInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboServer.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtUserId.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPassword.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboWorkstation.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkRememberId.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelBottom)).BeginInit();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelLeft
            // 
            this.panelLeft.Controls.Add(this.picLogo);
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelLeft.Location = new System.Drawing.Point(0, 0);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Padding = new System.Windows.Forms.Padding(20);
            this.panelLeft.Size = new System.Drawing.Size(260, 260);
            this.panelLeft.TabIndex = 0;
            // 
            // picLogo
            // 
            this.picLogo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picLogo.Location = new System.Drawing.Point(22, 22);
            this.picLogo.Name = "picLogo";
            this.picLogo.Properties.AllowFocused = false;
            this.picLogo.Properties.ShowMenu = false;
            this.picLogo.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Squeeze;
            this.picLogo.Size = new System.Drawing.Size(216, 216);
            this.picLogo.TabIndex = 0;
            // 
            // panelRight
            // 
            this.panelRight.Controls.Add(this.groupUserInfo);
            this.panelRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRight.Location = new System.Drawing.Point(260, 0);
            this.panelRight.Name = "panelRight";
            this.panelRight.Padding = new System.Windows.Forms.Padding(12, 20, 20, 0);
            this.panelRight.Size = new System.Drawing.Size(460, 260);
            this.panelRight.TabIndex = 1;
            // 
            // groupUserInfo
            // 
            this.groupUserInfo.AppearanceCaption.Font = new System.Drawing.Font("맑은 고딕", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupUserInfo.AppearanceCaption.Options.UseFont = true;
            this.groupUserInfo.Controls.Add(this.lblServer);
            this.groupUserInfo.Controls.Add(this.cboServer);
            this.groupUserInfo.Controls.Add(this.lblUserId);
            this.groupUserInfo.Controls.Add(this.txtUserId);
            this.groupUserInfo.Controls.Add(this.lblPassword);
            this.groupUserInfo.Controls.Add(this.txtPassword);
            this.groupUserInfo.Controls.Add(this.lblWorkstation);
            this.groupUserInfo.Controls.Add(this.cboWorkstation);
            this.groupUserInfo.Controls.Add(this.chkRememberId);
            this.groupUserInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupUserInfo.Location = new System.Drawing.Point(14, 22);
            this.groupUserInfo.Name = "groupUserInfo";
            this.groupUserInfo.Size = new System.Drawing.Size(424, 236);
            this.groupUserInfo.TabIndex = 0;
            this.groupUserInfo.Text = "접속 사용자 정보";
            // 
            // lblServer
            // 
            this.lblServer.Location = new System.Drawing.Point(24, 44);
            this.lblServer.Name = "lblServer";
            this.lblServer.Size = new System.Drawing.Size(40, 14);
            this.lblServer.TabIndex = 0;
            this.lblServer.Text = "접속서버";
            // 
            // cboServer
            // 
            this.cboServer.Location = new System.Drawing.Point(140, 40);
            this.cboServer.Name = "cboServer";
            this.cboServer.Properties.Items.AddRange(new object[] {
            "TEST-SERVER",
            "PRD-SERVER"});
            this.cboServer.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cboServer.Size = new System.Drawing.Size(260, 20);
            this.cboServer.TabIndex = 1;
            // 
            // lblUserId
            // 
            this.lblUserId.Location = new System.Drawing.Point(24, 84);
            this.lblUserId.Name = "lblUserId";
            this.lblUserId.Size = new System.Drawing.Size(30, 14);
            this.lblUserId.TabIndex = 2;
            this.lblUserId.Text = "아이디";
            // 
            // txtUserId
            // 
            this.txtUserId.Location = new System.Drawing.Point(140, 80);
            this.txtUserId.Name = "txtUserId";
            this.txtUserId.Size = new System.Drawing.Size(260, 20);
            this.txtUserId.TabIndex = 3;
            // 
            // lblPassword
            // 
            this.lblPassword.Location = new System.Drawing.Point(24, 124);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(40, 14);
            this.lblPassword.TabIndex = 4;
            this.lblPassword.Text = "비밀번호";
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(140, 120);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Properties.UseSystemPasswordChar = true;
            this.txtPassword.Size = new System.Drawing.Size(260, 20);
            this.txtPassword.TabIndex = 5;
            // 
            // lblWorkstation
            // 
            this.lblWorkstation.Location = new System.Drawing.Point(24, 164);
            this.lblWorkstation.Name = "lblWorkstation";
            this.lblWorkstation.Size = new System.Drawing.Size(60, 14);
            this.lblWorkstation.TabIndex = 6;
            this.lblWorkstation.Text = "워크스테이션";
            // 
            // cboWorkstation
            // 
            this.cboWorkstation.Location = new System.Drawing.Point(140, 160);
            this.cboWorkstation.Name = "cboWorkstation";
            this.cboWorkstation.Properties.Items.AddRange(new object[] {
            "Workstation01(워크스테이션01)",
            "Workstation02(워크스테이션02)"});
            this.cboWorkstation.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cboWorkstation.Size = new System.Drawing.Size(260, 20);
            this.cboWorkstation.TabIndex = 7;
            // 
            // chkRememberId
            // 
            this.chkRememberId.Location = new System.Drawing.Point(138, 194);
            this.chkRememberId.Name = "chkRememberId";
            this.chkRememberId.Properties.Caption = "아이디 저장";
            this.chkRememberId.Size = new System.Drawing.Size(160, 20);
            this.chkRememberId.TabIndex = 8;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.lblMessage);
            this.panelBottom.Controls.Add(this.btnConnect);
            this.panelBottom.Controls.Add(this.btnClose);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 260);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(720, 70);
            this.panelBottom.TabIndex = 2;
            // 
            // lblMessage
            // 
            this.lblMessage.Appearance.ForeColor = System.Drawing.Color.Firebrick;
            this.lblMessage.Appearance.Options.UseForeColor = true;
            this.lblMessage.Appearance.Options.UseTextOptions = true;
            this.lblMessage.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.lblMessage.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblMessage.Location = new System.Drawing.Point(2, 2);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(0, 14);
            this.lblMessage.TabIndex = 0;
            // 
            // btnConnect
            // 
            this.btnConnect.Appearance.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnConnect.Appearance.Options.UseFont = true;
            this.btnConnect.Location = new System.Drawing.Point(368, 22);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(130, 40);
            this.btnConnect.TabIndex = 1;
            this.btnConnect.Text = "접속";
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnClose
            // 
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(222, 22);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(130, 40);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "닫기";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // LoginForm
            // 
            this.AcceptButton = this.btnConnect;
            this.Appearance.Options.UseFont = true;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(720, 330);
            this.Controls.Add(this.panelRight);
            this.Controls.Add(this.panelLeft);
            this.Controls.Add(this.panelBottom);
            this.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LoginForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MEIS POP 로그인";
            ((System.ComponentModel.ISupportInitialize)(this.panelLeft)).EndInit();
            this.panelLeft.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picLogo.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelRight)).EndInit();
            this.panelRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.groupUserInfo)).EndInit();
            this.groupUserInfo.ResumeLayout(false);
            this.groupUserInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboServer.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtUserId.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPassword.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboWorkstation.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkRememberId.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelBottom)).EndInit();
            this.panelBottom.ResumeLayout(false);
            this.panelBottom.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.PanelControl panelLeft;
        private DevExpress.XtraEditors.PictureEdit picLogo;
        private DevExpress.XtraEditors.PanelControl panelRight;
        private DevExpress.XtraEditors.GroupControl groupUserInfo;
        private DevExpress.XtraEditors.LabelControl lblServer;
        private DevExpress.XtraEditors.ComboBoxEdit cboServer;
        private DevExpress.XtraEditors.LabelControl lblUserId;
        private DevExpress.XtraEditors.TextEdit txtUserId;
        private DevExpress.XtraEditors.LabelControl lblPassword;
        private DevExpress.XtraEditors.TextEdit txtPassword;
        private DevExpress.XtraEditors.LabelControl lblWorkstation;
        private DevExpress.XtraEditors.ComboBoxEdit cboWorkstation;
        private DevExpress.XtraEditors.CheckEdit chkRememberId;
        private DevExpress.XtraEditors.PanelControl panelBottom;
        private DevExpress.XtraEditors.LabelControl lblMessage;
        private DevExpress.XtraEditors.SimpleButton btnConnect;
        private DevExpress.XtraEditors.SimpleButton btnClose;
    }
}
