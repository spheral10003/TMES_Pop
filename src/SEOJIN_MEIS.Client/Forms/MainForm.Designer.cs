namespace SEOJIN_MEIS.Client.Forms
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
            this.brandPanel = new DevExpress.XtraEditors.PanelControl();
            this.brandIcon = new DevExpress.XtraEditors.SvgImageBox();
            this.lblBrand = new DevExpress.XtraEditors.LabelControl();
            this.lblBrandSub = new DevExpress.XtraEditors.LabelControl();
            this.lblVersion = new DevExpress.XtraEditors.LabelControl();
            this.accordion = new DevExpress.XtraBars.Navigation.AccordionControl();
            this.mainArea = new DevExpress.XtraEditors.PanelControl();
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
            this.rightPanel = new DevExpress.XtraEditors.PanelControl();
            this.tabStrip = new System.Windows.Forms.FlowLayoutPanel();
            this.contentHost = new DevExpress.XtraEditors.PanelControl();
            ((System.ComponentModel.ISupportInitialize)(this.sidePanel)).BeginInit();
            this.sidePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.brandPanel)).BeginInit();
            this.brandPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.accordion)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mainArea)).BeginInit();
            this.mainArea.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelTop)).BeginInit();
            this.panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboLanguage.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rightPanel)).BeginInit();
            this.rightPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.contentHost)).BeginInit();
            this.SuspendLayout();
            //
            // sidePanel
            //
            // Add 순서 주의 — WinForms는 나중에 Add한 컨트롤을 먼저 도킹한다.
            // brandPanel(Top) → lblVersion(Bottom) → accordion(Fill) 순으로 자리를 잡아야 하므로
            // Add는 그 반대인 accordion → lblVersion → brandPanel 순서다. 바꾸면 레이아웃이 무너진다.
            this.sidePanel.Controls.Add(this.accordion);
            this.sidePanel.Controls.Add(this.lblVersion);
            this.sidePanel.Controls.Add(this.brandPanel);
            this.sidePanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.sidePanel.Location = new System.Drawing.Point(0, 0);
            this.sidePanel.Name = "sidePanel";
            this.sidePanel.Size = new System.Drawing.Size(240, 800);
            this.sidePanel.TabIndex = 0;
            //
            // lblVersion — 사이드바 맨 아래 배포 버전. 실제 문구는 MainForm.Initialize()에서 채운다.
            //
            this.lblVersion.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblVersion.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblVersion.Location = new System.Drawing.Point(0, 770);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Padding = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.lblVersion.Size = new System.Drawing.Size(240, 30);
            this.lblVersion.TabIndex = 2;
            this.lblVersion.Text = "Ver.";
            //
            // brandPanel
            //
            this.brandPanel.Controls.Add(this.lblBrandSub);
            this.brandPanel.Controls.Add(this.lblBrand);
            this.brandPanel.Controls.Add(this.brandIcon);
            this.brandPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.brandPanel.Location = new System.Drawing.Point(0, 0);
            this.brandPanel.Name = "brandPanel";
            this.brandPanel.Size = new System.Drawing.Size(240, 74);
            this.brandPanel.TabIndex = 0;
            //
            // brandIcon
            //
            this.brandIcon.Location = new System.Drawing.Point(16, 19);
            this.brandIcon.Name = "brandIcon";
            this.brandIcon.Size = new System.Drawing.Size(36, 36);
            this.brandIcon.TabIndex = 0;
            //
            // lblBrand
            //
            this.lblBrand.Location = new System.Drawing.Point(62, 20);
            this.lblBrand.Name = "lblBrand";
            this.lblBrand.Size = new System.Drawing.Size(100, 20);
            this.lblBrand.TabIndex = 1;
            this.lblBrand.Text = "MEIS POP";
            //
            // lblBrandSub
            //
            this.lblBrandSub.Location = new System.Drawing.Point(63, 43);
            this.lblBrandSub.Name = "lblBrandSub";
            this.lblBrandSub.Size = new System.Drawing.Size(150, 14);
            this.lblBrandSub.TabIndex = 2;
            this.lblBrandSub.Text = "MANUFACTURING SYSTEM";
            //
            // accordion
            //
            this.accordion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.accordion.Location = new System.Drawing.Point(0, 74);
            this.accordion.Name = "accordion";
            this.accordion.Size = new System.Drawing.Size(240, 726);
            this.accordion.TabIndex = 1;
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
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1040, 56);
            this.panelTop.TabIndex = 0;
            //
            // btnMenuToggle
            //
            this.btnMenuToggle.Location = new System.Drawing.Point(12, 11);
            this.btnMenuToggle.Name = "btnMenuToggle";
            this.btnMenuToggle.Size = new System.Drawing.Size(34, 34);
            this.btnMenuToggle.TabIndex = 0;
            this.btnMenuToggle.Click += new System.EventHandler(this.btnMenuToggle_Click);
            //
            // lblBreadcrumb
            //
            this.lblBreadcrumb.Location = new System.Drawing.Point(56, 21);
            this.lblBreadcrumb.Name = "lblBreadcrumb";
            this.lblBreadcrumb.Size = new System.Drawing.Size(300, 16);
            this.lblBreadcrumb.TabIndex = 1;
            //
            // btnAlerts
            //
            this.btnAlerts.Location = new System.Drawing.Point(800, 11);
            this.btnAlerts.Name = "btnAlerts";
            this.btnAlerts.Size = new System.Drawing.Size(34, 34);
            this.btnAlerts.TabIndex = 3;
            this.btnAlerts.Click += new System.EventHandler(this.btnAlerts_Click);
            //
            // lblAlertBadge
            //
            this.lblAlertBadge.Location = new System.Drawing.Point(824, 9);
            this.lblAlertBadge.Name = "lblAlertBadge";
            this.lblAlertBadge.Size = new System.Drawing.Size(16, 16);
            this.lblAlertBadge.TabIndex = 4;
            this.lblAlertBadge.Text = "0";
            //
            // lblWelcome
            //
            this.lblWelcome.Location = new System.Drawing.Point(700, 21);
            this.lblWelcome.Name = "lblWelcome";
            this.lblWelcome.Size = new System.Drawing.Size(160, 16);
            this.lblWelcome.TabIndex = 5;
            //
            // lblSessionTime
            //
            this.lblSessionTime.Location = new System.Drawing.Point(866, 21);
            this.lblSessionTime.Name = "lblSessionTime";
            this.lblSessionTime.Size = new System.Drawing.Size(66, 16);
            this.lblSessionTime.TabIndex = 6;
            this.lblSessionTime.Text = "00:00:00";
            //
            // cboLanguage
            //
            this.cboLanguage.Location = new System.Drawing.Point(898, 15);
            this.cboLanguage.Name = "cboLanguage";
            this.cboLanguage.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            // 국기 아이콘(20px) + 언어명 + 펼침 버튼이 들어가야 하므로 글자만 있을 때보다 넓고 높게 잡는다.
            this.cboLanguage.Size = new System.Drawing.Size(124, 28);
            this.cboLanguage.TabIndex = 7;
            //
            // btnUser
            //
            this.btnUser.Location = new System.Drawing.Point(950, 11);
            this.btnUser.Name = "btnUser";
            this.btnUser.Size = new System.Drawing.Size(34, 34);
            this.btnUser.TabIndex = 8;
            this.btnUser.Click += new System.EventHandler(this.btnUser_Click);
            //
            // btnLogout
            //
            this.btnLogout.Location = new System.Drawing.Point(990, 11);
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.Size = new System.Drawing.Size(34, 34);
            this.btnLogout.TabIndex = 9;
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);
            //
            // rightPanel
            //
            this.rightPanel.Controls.Add(this.contentHost);
            this.rightPanel.Controls.Add(this.tabStrip);
            this.rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightPanel.Location = new System.Drawing.Point(0, 56);
            this.rightPanel.Name = "rightPanel";
            this.rightPanel.Size = new System.Drawing.Size(1040, 744);
            this.rightPanel.TabIndex = 1;
            //
            // tabStrip
            //
            this.tabStrip.AutoScroll = false;
            this.tabStrip.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabStrip.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.tabStrip.Location = new System.Drawing.Point(0, 0);
            this.tabStrip.Name = "tabStrip";
            this.tabStrip.Size = new System.Drawing.Size(1040, 44);
            this.tabStrip.TabIndex = 0;
            this.tabStrip.WrapContents = false;
            //
            // contentHost
            //
            this.contentHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.contentHost.Location = new System.Drawing.Point(0, 44);
            this.contentHost.Name = "contentHost";
            this.contentHost.Padding = new System.Windows.Forms.Padding(12, 4, 12, 12);
            this.contentHost.Size = new System.Drawing.Size(1040, 700);
            this.contentHost.TabIndex = 1;
            //
            // MainForm
            //
            this.ClientSize = new System.Drawing.Size(1280, 800);
            this.Controls.Add(this.mainArea);
            this.Controls.Add(this.sidePanel);
            // 윈도우 제목 줄(캡션 + 최소화/최대화/닫기)을 없앤다 — POP 단말은 항상 최대화 상태로 쓰고
            // 창 조작은 상단 바(로그아웃)로만 하므로 제목 줄이 화면 높이만 잡아먹는다.
            // 최대화 영역 제한은 MainForm.cs 생성자에서 잡는다(작업 표시줄까지 덮지 않도록).
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "MainForm";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            ((System.ComponentModel.ISupportInitialize)(this.accordion)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.brandPanel)).EndInit();
            this.brandPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.sidePanel)).EndInit();
            this.sidePanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cboLanguage.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelTop)).EndInit();
            this.panelTop.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.contentHost)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rightPanel)).EndInit();
            this.rightPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mainArea)).EndInit();
            this.mainArea.ResumeLayout(false);
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
        private DevExpress.XtraEditors.PanelControl rightPanel;
        private System.Windows.Forms.FlowLayoutPanel tabStrip;
        private DevExpress.XtraEditors.PanelControl contentHost;
    }
}
