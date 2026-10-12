namespace SEOJIN_TMES.Client.Forms
{
    partial class UserProfileForm
    {
        private System.ComponentModel.IContainer components = null;

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
        /// 디자이너 지원 메서드. 람다식/삼항연산자/문자열보간/객체이니셜라이저/커스텀 static 호출 사용 금지.
        /// 색·폰트·아이콘(AppTheme/AppIcons)과 실제 값 채우기는 UserProfileForm.cs의 Initialize()에서 수행한다.
        ///
        /// 도킹 순서 주의: 나중에 Add된 컨트롤이 먼저 도킹된다.
        /// bodyPanel(Fill) → footerPanel(Bottom) → headerPanel(Top) 순서를 바꾸지 말 것.
        /// </summary>
        private void InitializeComponent()
        {
            this.headerPanel = new DevExpress.XtraEditors.PanelControl();
            this.lblAvatar = new DevExpress.XtraEditors.LabelControl();
            this.lblName = new DevExpress.XtraEditors.LabelControl();
            this.lblUserId = new DevExpress.XtraEditors.LabelControl();
            this.bodyPanel = new DevExpress.XtraEditors.PanelControl();
            this.lblEmployeeNoCaption = new DevExpress.XtraEditors.LabelControl();
            this.lblEmployeeNo = new DevExpress.XtraEditors.LabelControl();
            this.lblUserNameCaption = new DevExpress.XtraEditors.LabelControl();
            this.lblUserName = new DevExpress.XtraEditors.LabelControl();
            this.lblPhoneCaption = new DevExpress.XtraEditors.LabelControl();
            this.lblPhone = new DevExpress.XtraEditors.LabelControl();
            this.lblDepartmentCaption = new DevExpress.XtraEditors.LabelControl();
            this.lblDepartment = new DevExpress.XtraEditors.LabelControl();
            this.lblWorkstationCaption = new DevExpress.XtraEditors.LabelControl();
            this.lblWorkstation = new DevExpress.XtraEditors.LabelControl();
            this.lblMessage = new DevExpress.XtraEditors.LabelControl();
            this.footerPanel = new DevExpress.XtraEditors.PanelControl();
            this.btnClose = new SEOJIN_TMES.Client.Forms.Common.MeisButton();
            ((System.ComponentModel.ISupportInitialize)(this.headerPanel)).BeginInit();
            this.headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bodyPanel)).BeginInit();
            this.bodyPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.footerPanel)).BeginInit();
            this.footerPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // headerPanel
            //
            this.headerPanel.Controls.Add(this.lblAvatar);
            this.headerPanel.Controls.Add(this.lblName);
            this.headerPanel.Controls.Add(this.lblUserId);
            this.headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.headerPanel.Location = new System.Drawing.Point(0, 0);
            this.headerPanel.Name = "headerPanel";
            this.headerPanel.Size = new System.Drawing.Size(440, 96);
            this.headerPanel.TabIndex = 0;
            //
            // lblAvatar
            //
            this.lblAvatar.Location = new System.Drawing.Point(24, 22);
            this.lblAvatar.Name = "lblAvatar";
            this.lblAvatar.Size = new System.Drawing.Size(52, 52);
            this.lblAvatar.TabIndex = 0;
            //
            // lblName
            //
            this.lblName.Location = new System.Drawing.Point(92, 30);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(200, 22);
            this.lblName.TabIndex = 1;
            //
            // lblUserId
            //
            this.lblUserId.Location = new System.Drawing.Point(94, 56);
            this.lblUserId.Name = "lblUserId";
            this.lblUserId.Size = new System.Drawing.Size(200, 16);
            this.lblUserId.TabIndex = 2;
            //
            // bodyPanel
            //
            this.bodyPanel.Controls.Add(this.lblEmployeeNoCaption);
            this.bodyPanel.Controls.Add(this.lblEmployeeNo);
            this.bodyPanel.Controls.Add(this.lblUserNameCaption);
            this.bodyPanel.Controls.Add(this.lblUserName);
            this.bodyPanel.Controls.Add(this.lblPhoneCaption);
            this.bodyPanel.Controls.Add(this.lblPhone);
            this.bodyPanel.Controls.Add(this.lblDepartmentCaption);
            this.bodyPanel.Controls.Add(this.lblDepartment);
            this.bodyPanel.Controls.Add(this.lblWorkstationCaption);
            this.bodyPanel.Controls.Add(this.lblWorkstation);
            this.bodyPanel.Controls.Add(this.lblMessage);
            this.bodyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bodyPanel.Location = new System.Drawing.Point(0, 96);
            this.bodyPanel.Name = "bodyPanel";
            this.bodyPanel.Size = new System.Drawing.Size(440, 252);
            this.bodyPanel.TabIndex = 1;
            //
            // lblEmployeeNoCaption
            //
            this.lblEmployeeNoCaption.Location = new System.Drawing.Point(28, 24);
            this.lblEmployeeNoCaption.Name = "lblEmployeeNoCaption";
            this.lblEmployeeNoCaption.Size = new System.Drawing.Size(90, 16);
            this.lblEmployeeNoCaption.TabIndex = 0;
            this.lblEmployeeNoCaption.Text = "사번";
            //
            // lblEmployeeNo
            //
            this.lblEmployeeNo.Location = new System.Drawing.Point(150, 24);
            this.lblEmployeeNo.Name = "lblEmployeeNo";
            this.lblEmployeeNo.Size = new System.Drawing.Size(260, 16);
            this.lblEmployeeNo.TabIndex = 1;
            //
            // lblUserNameCaption
            //
            this.lblUserNameCaption.Location = new System.Drawing.Point(28, 62);
            this.lblUserNameCaption.Name = "lblUserNameCaption";
            this.lblUserNameCaption.Size = new System.Drawing.Size(90, 16);
            this.lblUserNameCaption.TabIndex = 2;
            this.lblUserNameCaption.Text = "이름";
            //
            // lblUserName
            //
            this.lblUserName.Location = new System.Drawing.Point(150, 62);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.Size = new System.Drawing.Size(260, 16);
            this.lblUserName.TabIndex = 3;
            //
            // lblPhoneCaption
            //
            this.lblPhoneCaption.Location = new System.Drawing.Point(28, 100);
            this.lblPhoneCaption.Name = "lblPhoneCaption";
            this.lblPhoneCaption.Size = new System.Drawing.Size(90, 16);
            this.lblPhoneCaption.TabIndex = 4;
            this.lblPhoneCaption.Text = "연락처";
            //
            // lblPhone
            //
            this.lblPhone.Location = new System.Drawing.Point(150, 100);
            this.lblPhone.Name = "lblPhone";
            this.lblPhone.Size = new System.Drawing.Size(260, 16);
            this.lblPhone.TabIndex = 5;
            //
            // lblDepartmentCaption
            //
            this.lblDepartmentCaption.Location = new System.Drawing.Point(28, 138);
            this.lblDepartmentCaption.Name = "lblDepartmentCaption";
            this.lblDepartmentCaption.Size = new System.Drawing.Size(90, 16);
            this.lblDepartmentCaption.TabIndex = 6;
            this.lblDepartmentCaption.Text = "부서";
            //
            // lblDepartment
            //
            this.lblDepartment.Location = new System.Drawing.Point(150, 138);
            this.lblDepartment.Name = "lblDepartment";
            this.lblDepartment.Size = new System.Drawing.Size(260, 16);
            this.lblDepartment.TabIndex = 7;
            //
            // lblWorkstationCaption
            //
            this.lblWorkstationCaption.Location = new System.Drawing.Point(28, 176);
            this.lblWorkstationCaption.Name = "lblWorkstationCaption";
            this.lblWorkstationCaption.Size = new System.Drawing.Size(90, 16);
            this.lblWorkstationCaption.TabIndex = 8;
            this.lblWorkstationCaption.Text = "워크스테이션";
            //
            // lblWorkstation
            //
            this.lblWorkstation.Location = new System.Drawing.Point(150, 176);
            this.lblWorkstation.Name = "lblWorkstation";
            this.lblWorkstation.Size = new System.Drawing.Size(260, 16);
            this.lblWorkstation.TabIndex = 9;
            //
            // lblMessage
            //
            this.lblMessage.Location = new System.Drawing.Point(28, 214);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(382, 16);
            this.lblMessage.TabIndex = 10;
            //
            // footerPanel
            //
            this.footerPanel.Controls.Add(this.btnClose);
            this.footerPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.footerPanel.Location = new System.Drawing.Point(0, 348);
            this.footerPanel.Name = "footerPanel";
            this.footerPanel.Size = new System.Drawing.Size(440, 58);
            this.footerPanel.TabIndex = 2;
            //
            // btnClose
            //
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(310, 14);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(104, 30);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "닫기";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            //
            // UserProfileForm
            //
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(440, 406);
            this.Controls.Add(this.bodyPanel);
            this.Controls.Add(this.footerPanel);
            this.Controls.Add(this.headerPanel);
            this.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UserProfileForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "사용자 정보";
            ((System.ComponentModel.ISupportInitialize)(this.headerPanel)).EndInit();
            this.headerPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.bodyPanel)).EndInit();
            this.bodyPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.footerPanel)).EndInit();
            this.footerPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private DevExpress.XtraEditors.PanelControl headerPanel;
        private DevExpress.XtraEditors.LabelControl lblAvatar;
        private DevExpress.XtraEditors.LabelControl lblName;
        private DevExpress.XtraEditors.LabelControl lblUserId;
        private DevExpress.XtraEditors.PanelControl bodyPanel;
        private DevExpress.XtraEditors.LabelControl lblEmployeeNoCaption;
        private DevExpress.XtraEditors.LabelControl lblEmployeeNo;
        private DevExpress.XtraEditors.LabelControl lblUserNameCaption;
        private DevExpress.XtraEditors.LabelControl lblUserName;
        private DevExpress.XtraEditors.LabelControl lblPhoneCaption;
        private DevExpress.XtraEditors.LabelControl lblPhone;
        private DevExpress.XtraEditors.LabelControl lblDepartmentCaption;
        private DevExpress.XtraEditors.LabelControl lblDepartment;
        private DevExpress.XtraEditors.LabelControl lblWorkstationCaption;
        private DevExpress.XtraEditors.LabelControl lblWorkstation;
        private DevExpress.XtraEditors.LabelControl lblMessage;
        private DevExpress.XtraEditors.PanelControl footerPanel;
        private SEOJIN_TMES.Client.Forms.Common.MeisButton btnClose;
    }
}
