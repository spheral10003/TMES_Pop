namespace SEOJIN_TMES.Client.Forms
{
    partial class MainForm
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
        /// 여기에는 "무엇이 어디에 놓이는지"(구조/도킹/크기)만 두고,
        /// 색·폰트·아이콘(AppTheme/AppIcons)과 메뉴 트리 구성, 이벤트 배선은 MainForm.cs의 Initialize()에서 수행한다.
        ///
        /// 도킹 순서 주의: WinForms는 나중에 Add된 컨트롤을 먼저 도킹한다.
        /// 그래서 mainArea(Fill) → sidePanel(Left) 순서로 Add해야 좌측 사이드바가 화면 높이 전체를 차지한다.
        /// </summary>
        private void InitializeComponent()
        {
            this.sidePanel = new DevExpress.XtraEditors.PanelControl();
            this.accordion = new DevExpress.XtraBars.Navigation.AccordionControl();
            this.lblVersion = new DevExpress.XtraEditors.LabelControl();
            this.brandPanel = new DevExpress.XtraEditors.PanelControl();
            this.lblBrandSub = new DevExpress.XtraEditors.LabelControl();
            this.lblBrand = new DevExpress.XtraEditors.LabelControl();
            this.brandIcon = new DevExpress.XtraEditors.SvgImageBox();
            this.mainArea = new DevExpress.XtraEditors.PanelControl();
            this.rightPanel = new DevExpress.XtraEditors.PanelControl();
            this.contentHost = new DevExpress.XtraEditors.PanelControl();
            this.tabStrip = new System.Windows.Forms.FlowLayoutPanel();
            this.panelTop = new DevExpress.XtraEditors.PanelControl();
            this.btnMenuToggle = new DevExpress.XtraEditors.SimpleButton();
            this.lblBreadcrumb = new DevExpress.XtraEditors.LabelControl();
            this.btnAlerts = new DevExpress.XtraEditors.SimpleButton();
            this.lblAlertBadge = new DevExpress.XtraEditors.LabelControl();
            this.lblWelcome = new DevExpress.XtraEditors.LabelControl();
            this.lblSessionTime = new DevExpress.XtraEditors.LabelControl();
            this.cboLanguage = new DevExpress.XtraEditors.ImageComboBoxEdit();
            this.btnUser = new DevExpress.XtraEditors.SimpleButton();
            this.btnLogout = new DevExpress.XtraEditors.SimpleButton();
            this.btnMinimize = new DevExpress.XtraEditors.SimpleButton();
            this.btnMaximize = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.sidePanel)).BeginInit();
            this.sidePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.accordion)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.brandPanel)).BeginInit();
            this.brandPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.brandIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mainArea)).BeginInit();
            this.mainArea.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.rightPanel)).BeginInit();
            this.rightPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.contentHost)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelTop)).BeginInit();
            this.panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboLanguage.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // sidePanel
            // 
            this.sidePanel.Controls.Add(this.accordion);
            this.sidePanel.Controls.Add(this.lblVersion);
            this.sidePanel.Controls.Add(this.brandPanel);
            this.sidePanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.sidePanel.Location = new System.Drawing.Point(0, 0);
            this.sidePanel.Name = "sidePanel";
            this.sidePanel.Size = new System.Drawing.Size(240, 800);
            this.sidePanel.TabIndex = 0;
            // 
            // accordion
            // 
            this.accordion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.accordion.Location = new System.Drawing.Point(2, 76);
            this.accordion.Name = "accordion";
            this.accordion.Size = new System.Drawing.Size(236, 692);
            this.accordion.TabIndex = 1;
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblVersion.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblVersion.Location = new System.Drawing.Point(2, 768);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Padding = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.lblVersion.Size = new System.Drawing.Size(236, 30);
            this.lblVersion.TabIndex = 2;
            this.lblVersion.Text = "Ver.";
            // 
            // brandPanel
            // 
            this.brandPanel.Controls.Add(this.lblBrandSub);
            this.brandPanel.Controls.Add(this.lblBrand);
            this.brandPanel.Controls.Add(this.brandIcon);
            this.brandPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.brandPanel.Location = new System.Drawing.Point(2, 2);
            this.brandPanel.Name = "brandPanel";
            this.brandPanel.Size = new System.Drawing.Size(236, 74);
            this.brandPanel.TabIndex = 0;
            // 
            // lblBrandSub
            // 
            this.lblBrandSub.Location = new System.Drawing.Point(63, 43);
            this.lblBrandSub.Name = "lblBrandSub";
            this.lblBrandSub.Size = new System.Drawing.Size(147, 14);
            this.lblBrandSub.TabIndex = 2;
            this.lblBrandSub.Text = "MANUFACTURING SYSTEM";
            // 
            // lblBrand
            // 
            this.lblBrand.Location = new System.Drawing.Point(62, 20);
            this.lblBrand.Name = "lblBrand";
            this.lblBrand.Size = new System.Drawing.Size(54, 14);
            this.lblBrand.TabIndex = 1;
            this.lblBrand.Text = "MEIS POP";
            // 
            // brandIcon
            // 
            this.brandIcon.Location = new System.Drawing.Point(16, 19);
            this.brandIcon.Name = "brandIcon";
            this.brandIcon.Size = new System.Drawing.Size(36, 36);
            this.brandIcon.TabIndex = 0;
            // 
            // mainArea
            // 
            this.mainArea.Controls.Add(this.rightPanel);
            this.mainArea.Controls.Add(this.panelTop);
            this.mainArea.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainArea.Location = new System.Drawing.Point(240, 0);
            this.mainArea.Name = "mainArea";
            this.mainArea.Size = new System.Drawing.Size(1040, 800);
            this.mainArea.TabIndex = 1;
            // 
            // rightPanel
            // 
            this.rightPanel.Controls.Add(this.contentHost);
            this.rightPanel.Controls.Add(this.tabStrip);
            this.rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightPanel.Location = new System.Drawing.Point(2, 36);
            this.rightPanel.Name = "rightPanel";
            this.rightPanel.Size = new System.Drawing.Size(1036, 762);
            this.rightPanel.TabIndex = 1;
            // 
            // contentHost
            // 
            this.contentHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.contentHost.Location = new System.Drawing.Point(2, 40);
            this.contentHost.Name = "contentHost";
            this.contentHost.Padding = new System.Windows.Forms.Padding(12, 4, 12, 12);
            this.contentHost.Size = new System.Drawing.Size(1032, 720);
            this.contentHost.TabIndex = 1;
            // 
            // tabStrip
            // 
            this.tabStrip.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabStrip.Location = new System.Drawing.Point(2, 2);
            this.tabStrip.Name = "tabStrip";
            this.tabStrip.Size = new System.Drawing.Size(1032, 38);
            this.tabStrip.TabIndex = 0;
            this.tabStrip.WrapContents = false;
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.btnMenuToggle);
            this.panelTop.Controls.Add(this.lblBreadcrumb);
            this.panelTop.Controls.Add(this.btnAlerts);
            this.panelTop.Controls.Add(this.lblAlertBadge);
            this.panelTop.Controls.Add(this.lblWelcome);
            this.panelTop.Controls.Add(this.lblSessionTime);
            this.panelTop.Controls.Add(this.cboLanguage);
            this.panelTop.Controls.Add(this.btnUser);
            this.panelTop.Controls.Add(this.btnLogout);
            this.panelTop.Controls.Add(this.btnMinimize);
            this.panelTop.Controls.Add(this.btnMaximize);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(2, 2);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1036, 34);
            this.panelTop.TabIndex = 0;
            // 
            // btnMenuToggle
            // 
            this.btnMenuToggle.Location = new System.Drawing.Point(5, 5);
            this.btnMenuToggle.Name = "btnMenuToggle";
            this.btnMenuToggle.Size = new System.Drawing.Size(22, 22);
            this.btnMenuToggle.TabIndex = 0;
            this.btnMenuToggle.Click += new System.EventHandler(this.btnMenuToggle_Click);
            // 
            // lblBreadcrumb
            // 
            this.lblBreadcrumb.Location = new System.Drawing.Point(56, 21);
            this.lblBreadcrumb.Name = "lblBreadcrumb";
            this.lblBreadcrumb.Size = new System.Drawing.Size(0, 14);
            this.lblBreadcrumb.TabIndex = 1;
            // 
            // btnAlerts
            // 
            this.btnAlerts.Location = new System.Drawing.Point(802, 4);
            this.btnAlerts.Name = "btnAlerts";
            this.btnAlerts.Size = new System.Drawing.Size(24, 24);
            this.btnAlerts.TabIndex = 3;
            this.btnAlerts.Click += new System.EventHandler(this.btnAlerts_Click);
            // 
            // lblAlertBadge
            // 
            this.lblAlertBadge.Location = new System.Drawing.Point(775, 8);
            this.lblAlertBadge.Name = "lblAlertBadge";
            this.lblAlertBadge.Size = new System.Drawing.Size(21, 14);
            this.lblAlertBadge.TabIndex = 4;
            this.lblAlertBadge.Text = "000";
            // 
            // lblWelcome
            // 
            this.lblWelcome.Location = new System.Drawing.Point(832, 9);
            this.lblWelcome.Name = "lblWelcome";
            this.lblWelcome.Size = new System.Drawing.Size(51, 14);
            this.lblWelcome.TabIndex = 5;
            this.lblWelcome.Text = "Welcome";
            // 
            // lblSessionTime
            // 
            this.lblSessionTime.Location = new System.Drawing.Point(242, 10);
            this.lblSessionTime.Name = "lblSessionTime";
            this.lblSessionTime.Size = new System.Drawing.Size(50, 14);
            this.lblSessionTime.TabIndex = 6;
            this.lblSessionTime.Text = "00:00:00";
            // 
            // cboLanguage
            // 
            this.cboLanguage.Location = new System.Drawing.Point(651, 7);
            this.cboLanguage.Name = "cboLanguage";
            this.cboLanguage.Size = new System.Drawing.Size(118, 20);
            this.cboLanguage.TabIndex = 7;
            // 
            // btnUser
            // 
            this.btnUser.Location = new System.Drawing.Point(889, 4);
            this.btnUser.Name = "btnUser";
            this.btnUser.Size = new System.Drawing.Size(24, 24);
            this.btnUser.TabIndex = 8;
            this.btnUser.Click += new System.EventHandler(this.btnUser_Click);
            // 
            // btnLogout
            // 
            this.btnLogout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLogout.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.btnLogout.Appearance.BorderColor = System.Drawing.Color.White;
            this.btnLogout.Appearance.Options.UseBackColor = true;
            this.btnLogout.Appearance.Options.UseBorderColor = true;
            this.btnLogout.Location = new System.Drawing.Point(979, 3);
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.Size = new System.Drawing.Size(24, 24);
            this.btnLogout.TabIndex = 9;
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);
            // 
            // btnMinimize
            // 
            this.btnMinimize.Location = new System.Drawing.Point(919, 3);
            this.btnMinimize.Name = "btnMinimize";
            this.btnMinimize.Size = new System.Drawing.Size(24, 24);
            this.btnMinimize.TabIndex = 10;
            this.btnMinimize.Click += new System.EventHandler(this.btnMinimize_Click);
            // 
            // btnMaximize
            // 
            this.btnMaximize.Location = new System.Drawing.Point(949, 3);
            this.btnMaximize.Name = "btnMaximize";
            this.btnMaximize.Size = new System.Drawing.Size(24, 24);
            this.btnMaximize.TabIndex = 11;
            this.btnMaximize.Click += new System.EventHandler(this.btnMaximize_Click);
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(1280, 800);
            this.Controls.Add(this.mainArea);
            this.Controls.Add(this.sidePanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "MainForm";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.sidePanel)).EndInit();
            this.sidePanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.accordion)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.brandPanel)).EndInit();
            this.brandPanel.ResumeLayout(false);
            this.brandPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.brandIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.mainArea)).EndInit();
            this.mainArea.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.rightPanel)).EndInit();
            this.rightPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.contentHost)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelTop)).EndInit();
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboLanguage.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.PanelControl sidePanel;
        private DevExpress.XtraEditors.PanelControl brandPanel;
        private DevExpress.XtraEditors.SvgImageBox brandIcon;
        private DevExpress.XtraEditors.LabelControl lblBrand;
        private DevExpress.XtraEditors.LabelControl lblBrandSub;
        private DevExpress.XtraEditors.LabelControl lblVersion;
        private DevExpress.XtraBars.Navigation.AccordionControl accordion;
        private DevExpress.XtraEditors.PanelControl mainArea;
        private DevExpress.XtraEditors.PanelControl panelTop;
        private DevExpress.XtraEditors.SimpleButton btnMenuToggle;
        private DevExpress.XtraEditors.LabelControl lblBreadcrumb;
        private DevExpress.XtraEditors.SimpleButton btnAlerts;
        private DevExpress.XtraEditors.LabelControl lblAlertBadge;
        private DevExpress.XtraEditors.LabelControl lblWelcome;
        private DevExpress.XtraEditors.LabelControl lblSessionTime;
        private DevExpress.XtraEditors.ImageComboBoxEdit cboLanguage;
        private DevExpress.XtraEditors.SimpleButton btnUser;
        private DevExpress.XtraEditors.SimpleButton btnLogout;
        private DevExpress.XtraEditors.SimpleButton btnMinimize;
        private DevExpress.XtraEditors.SimpleButton btnMaximize;
        private DevExpress.XtraEditors.PanelControl rightPanel;
        private System.Windows.Forms.FlowLayoutPanel tabStrip;
        private DevExpress.XtraEditors.PanelControl contentHost;
    }
}
