namespace SEOJIN_TMES.Client.Forms
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginForm));
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
            this.chkRememberId = new DevExpress.XtraEditors.CheckEdit();
            this.panelBottom = new DevExpress.XtraEditors.PanelControl();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnConnect = new System.Windows.Forms.Button();
            this.lblMessage = new DevExpress.XtraEditors.LabelControl();
            this.pnlStation = new System.Windows.Forms.Panel();
            this.lblWorkstationMsg = new System.Windows.Forms.Label();
            this.lblWorkstation = new DevExpress.XtraEditors.LabelControl();
            this.cboWorkstation = new DevExpress.XtraEditors.ComboBoxEdit();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
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
            ((System.ComponentModel.ISupportInitialize)(this.chkRememberId.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelBottom)).BeginInit();
            this.panelBottom.SuspendLayout();
            this.pnlStation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboWorkstation.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // panelLeft
            // 
            this.panelLeft.Controls.Add(this.picLogo);
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelLeft.Location = new System.Drawing.Point(0, 0);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Padding = new System.Windows.Forms.Padding(20);
            this.panelLeft.Size = new System.Drawing.Size(268, 241);
            this.panelLeft.TabIndex = 0;
            // 
            // picLogo
            // 
            this.picLogo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picLogo.Location = new System.Drawing.Point(22, 22);
            this.picLogo.Name = "picLogo";
            this.picLogo.Properties.AllowFocused = false;
            this.picLogo.Properties.Appearance.BackColor = System.Drawing.Color.Gray;
            this.picLogo.Properties.Appearance.Options.UseBackColor = true;
            this.picLogo.Properties.ShowMenu = false;
            this.picLogo.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Squeeze;
            this.picLogo.Size = new System.Drawing.Size(224, 197);
            this.picLogo.TabIndex = 0;
            // 
            // panelRight
            // 
            this.panelRight.Controls.Add(this.groupUserInfo);
            this.panelRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRight.Location = new System.Drawing.Point(268, 0);
            this.panelRight.Name = "panelRight";
            this.panelRight.Padding = new System.Windows.Forms.Padding(12, 20, 20, 20);
            this.panelRight.Size = new System.Drawing.Size(452, 241);
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
            this.groupUserInfo.Controls.Add(this.chkRememberId);
            this.groupUserInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupUserInfo.Location = new System.Drawing.Point(14, 22);
            this.groupUserInfo.Name = "groupUserInfo";
            this.groupUserInfo.Size = new System.Drawing.Size(416, 197);
            this.groupUserInfo.TabIndex = 0;
            this.groupUserInfo.Text = "접속 사용자 정보";
            // 
            // lblServer
            // 
            this.lblServer.Location = new System.Drawing.Point(24, 49);
            this.lblServer.Name = "lblServer";
            this.lblServer.Size = new System.Drawing.Size(40, 14);
            this.lblServer.TabIndex = 0;
            this.lblServer.Text = "접속서버";
            // 
            // cboServer
            // 
            this.cboServer.Location = new System.Drawing.Point(140, 45);
            this.cboServer.Name = "cboServer";
            this.cboServer.Properties.Items.AddRange(new object[] {
            "GUM"});
            this.cboServer.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cboServer.Size = new System.Drawing.Size(260, 20);
            this.cboServer.TabIndex = 1;
            // 
            // lblUserId
            // 
            this.lblUserId.Location = new System.Drawing.Point(24, 89);
            this.lblUserId.Name = "lblUserId";
            this.lblUserId.Size = new System.Drawing.Size(30, 14);
            this.lblUserId.TabIndex = 2;
            this.lblUserId.Text = "아이디";
            // 
            // txtUserId
            // 
            this.txtUserId.Location = new System.Drawing.Point(140, 85);
            this.txtUserId.Name = "txtUserId";
            this.txtUserId.Size = new System.Drawing.Size(260, 20);
            this.txtUserId.TabIndex = 3;
            // 
            // lblPassword
            // 
            this.lblPassword.Location = new System.Drawing.Point(24, 129);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(40, 14);
            this.lblPassword.TabIndex = 4;
            this.lblPassword.Text = "비밀번호";
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(140, 125);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Properties.UseSystemPasswordChar = true;
            this.txtPassword.Size = new System.Drawing.Size(260, 20);
            this.txtPassword.TabIndex = 5;
            // 
            // chkRememberId
            // 
            this.chkRememberId.Location = new System.Drawing.Point(140, 164);
            this.chkRememberId.Name = "chkRememberId";
            this.chkRememberId.Properties.Caption = "아이디 저장";
            this.chkRememberId.Size = new System.Drawing.Size(160, 20);
            this.chkRememberId.TabIndex = 8;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.btnClose);
            this.panelBottom.Controls.Add(this.btnConnect);
            this.panelBottom.Controls.Add(this.lblMessage);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 241);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(720, 91);
            this.panelBottom.TabIndex = 2;
            // 
            // btnClose
            // 
            this.btnClose.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Tomato;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Location = new System.Drawing.Point(177, 34);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(161, 40);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "닫기";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnConnect
            // 
            this.btnConnect.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnConnect.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DeepSkyBlue;
            this.btnConnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConnect.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConnect.Location = new System.Drawing.Point(390, 34);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(161, 40);
            this.btnConnect.TabIndex = 3;
            this.btnConnect.Text = "접속";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // lblMessage
            // 
            this.lblMessage.Appearance.ForeColor = System.Drawing.Color.Firebrick;
            this.lblMessage.Appearance.Options.UseForeColor = true;
            this.lblMessage.Appearance.Options.UseTextOptions = true;
            this.lblMessage.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.lblMessage.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblMessage.Location = new System.Drawing.Point(2, 2);
            this.lblMessage.MaximumSize = new System.Drawing.Size(716, 0);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(26, 14);
            this.lblMessage.TabIndex = 0;
            this.lblMessage.Text = "Error";
            // 
            // pnlStation
            // 
            this.pnlStation.BackColor = System.Drawing.Color.White;
            this.pnlStation.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlStation.Controls.Add(this.lblWorkstationMsg);
            this.pnlStation.Controls.Add(this.lblWorkstation);
            this.pnlStation.Controls.Add(this.cboWorkstation);
            this.pnlStation.Controls.Add(this.btnConfirm);
            this.pnlStation.Controls.Add(this.btnCancel);
            this.pnlStation.Location = new System.Drawing.Point(5, 5);
            this.pnlStation.Name = "pnlStation";
            this.pnlStation.Size = new System.Drawing.Size(710, 322);
            this.pnlStation.TabIndex = 3;
            // 
            // lblWorkstationMsg
            // 
            this.lblWorkstationMsg.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWorkstationMsg.Location = new System.Drawing.Point(148, 65);
            this.lblWorkstationMsg.Name = "lblWorkstationMsg";
            this.lblWorkstationMsg.Size = new System.Drawing.Size(426, 49);
            this.lblWorkstationMsg.TabIndex = 10;
            this.lblWorkstationMsg.Text = "워크스테이션을 선택 하세요";
            this.lblWorkstationMsg.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblWorkstation
            // 
            this.lblWorkstation.Appearance.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWorkstation.Appearance.Options.UseFont = true;
            this.lblWorkstation.Location = new System.Drawing.Point(153, 165);
            this.lblWorkstation.Name = "lblWorkstation";
            this.lblWorkstation.Size = new System.Drawing.Size(66, 16);
            this.lblWorkstation.TabIndex = 8;
            this.lblWorkstation.Text = "워크스테이션";
            // 
            // cboWorkstation
            // 
            this.cboWorkstation.Location = new System.Drawing.Point(269, 161);
            this.cboWorkstation.Name = "cboWorkstation";
            this.cboWorkstation.Properties.Appearance.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboWorkstation.Properties.Appearance.Options.UseFont = true;
            this.cboWorkstation.Properties.Items.AddRange(new object[] {
            "Workstation01(워크스테이션01)",
            "Workstation02(워크스테이션02)"});
            this.cboWorkstation.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cboWorkstation.Size = new System.Drawing.Size(282, 22);
            this.cboWorkstation.TabIndex = 9;
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(124, 241);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(184, 59);
            this.btnConfirm.TabIndex = 1;
            this.btnConfirm.Text = "확   인";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(390, 241);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(184, 59);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "취   소";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // LoginForm
            // 
            this.Appearance.Options.UseFont = true;
            this.ClientSize = new System.Drawing.Size(720, 332);
            this.Controls.Add(this.panelRight);
            this.Controls.Add(this.panelLeft);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.pnlStation);
            this.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.IconOptions.Image = ((System.Drawing.Image)(resources.GetObject("LoginForm.IconOptions.Image")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LoginForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TMES POP 로그인";
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
            ((System.ComponentModel.ISupportInitialize)(this.chkRememberId.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelBottom)).EndInit();
            this.panelBottom.ResumeLayout(false);
            this.panelBottom.PerformLayout();
            this.pnlStation.ResumeLayout(false);
            this.pnlStation.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboWorkstation.Properties)).EndInit();
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
        private DevExpress.XtraEditors.CheckEdit chkRememberId;
        private DevExpress.XtraEditors.PanelControl panelBottom;
        private DevExpress.XtraEditors.LabelControl lblMessage;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Panel pnlStation;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblWorkstationMsg;
        private DevExpress.XtraEditors.LabelControl lblWorkstation;
        private DevExpress.XtraEditors.ComboBoxEdit cboWorkstation;
    }
}
