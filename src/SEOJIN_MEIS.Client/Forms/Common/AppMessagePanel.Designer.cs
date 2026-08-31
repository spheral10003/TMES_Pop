namespace SEOJIN_MEIS.Client.Forms.Common
{
    partial class AppMessagePanel
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

        #region Component Designer generated code

        /// <summary>
        /// 디자이너 지원 메서드. 람다식/삼항연산자/문자열보간/객체이니셜라이저/커스텀 static 호출 사용 금지.
        /// 색·폰트·아이콘과 문구는 AppMessagePanel.cs의 Initialize()에서 런타임에 넣는다.
        ///
        /// 도킹 순서 주의: 나중에 Add된 컨트롤이 먼저 도킹된다.
        /// bodyPanel(Fill) → footerPanel(Bottom) → headerPanel(Top) 순서를 바꾸지 말 것.
        /// </summary>
        private void InitializeComponent()
        {
            this.headerPanel = new DevExpress.XtraEditors.PanelControl();
            this.headerIcon = new DevExpress.XtraEditors.SvgImageBox();
            this.lblTitle = new DevExpress.XtraEditors.LabelControl();
            this.btnClose = new DevExpress.XtraEditors.SimpleButton();
            this.bodyPanel = new DevExpress.XtraEditors.PanelControl();
            this.lblMessage = new DevExpress.XtraEditors.LabelControl();
            this.footerPanel = new DevExpress.XtraEditors.PanelControl();
            this.btnOk = new DevExpress.XtraEditors.SimpleButton();
            this.btnCancel = new DevExpress.XtraEditors.SimpleButton();
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
            this.headerPanel.Controls.Add(this.headerIcon);
            this.headerPanel.Controls.Add(this.lblTitle);
            this.headerPanel.Controls.Add(this.btnClose);
            this.headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.headerPanel.Location = new System.Drawing.Point(0, 0);
            this.headerPanel.Name = "headerPanel";
            this.headerPanel.Size = new System.Drawing.Size(420, 56);
            this.headerPanel.TabIndex = 0;
            //
            // headerIcon
            //
            this.headerIcon.Location = new System.Drawing.Point(22, 15);
            this.headerIcon.Name = "headerIcon";
            this.headerIcon.Size = new System.Drawing.Size(26, 26);
            this.headerIcon.TabIndex = 0;
            //
            // lblTitle
            //
            this.lblTitle.Location = new System.Drawing.Point(58, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(280, 18);
            this.lblTitle.TabIndex = 1;
            //
            // btnClose
            //
            this.btnClose.Location = new System.Drawing.Point(378, 13);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(30, 30);
            this.btnClose.TabIndex = 2;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            //
            // bodyPanel
            //
            this.bodyPanel.Controls.Add(this.lblMessage);
            this.bodyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bodyPanel.Location = new System.Drawing.Point(0, 56);
            this.bodyPanel.Name = "bodyPanel";
            this.bodyPanel.Padding = new System.Windows.Forms.Padding(24, 8, 24, 8);
            this.bodyPanel.Size = new System.Drawing.Size(420, 70);
            this.bodyPanel.TabIndex = 1;
            //
            // lblMessage
            //
            this.lblMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMessage.Location = new System.Drawing.Point(24, 0);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(372, 62);
            this.lblMessage.TabIndex = 0;
            //
            // footerPanel
            //
            this.footerPanel.Controls.Add(this.btnOk);
            this.footerPanel.Controls.Add(this.btnCancel);
            this.footerPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.footerPanel.Location = new System.Drawing.Point(0, 116);
            this.footerPanel.Name = "footerPanel";
            this.footerPanel.Size = new System.Drawing.Size(420, 64);
            this.footerPanel.TabIndex = 2;
            //
            // btnOk
            //
            this.btnOk.Location = new System.Drawing.Point(244, 16);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(76, 32);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "확인";
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            //
            // btnCancel
            //
            this.btnCancel.Location = new System.Drawing.Point(328, 16);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(76, 32);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "취소";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            //
            // AppMessagePanel
            //
            this.Controls.Add(this.bodyPanel);
            this.Controls.Add(this.footerPanel);
            this.Controls.Add(this.headerPanel);
            this.Name = "AppMessagePanel";
            this.Size = new System.Drawing.Size(420, 180);
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
        private DevExpress.XtraEditors.SvgImageBox headerIcon;
        private DevExpress.XtraEditors.LabelControl lblTitle;
        private DevExpress.XtraEditors.SimpleButton btnClose;
        private DevExpress.XtraEditors.PanelControl bodyPanel;
        private DevExpress.XtraEditors.LabelControl lblMessage;
        private DevExpress.XtraEditors.PanelControl footerPanel;
        private DevExpress.XtraEditors.SimpleButton btnOk;
        private DevExpress.XtraEditors.SimpleButton btnCancel;
    }
}
