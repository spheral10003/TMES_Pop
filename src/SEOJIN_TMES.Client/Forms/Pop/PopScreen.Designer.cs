namespace SEOJIN_TMES.Client.Forms.Pop
{
    partial class PopScreen
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
        /// 색·폰트·아이콘·다국어 문구는 PopScreen.cs의 Initialize()/ApplyTheme()에서 런타임에 입힌다.
        ///
        /// 화면 구성(화면 설계서 기준):
        ///   ┌ toolbarCard : 공정 제어 버튼 7개 + 우측 작업자/현재시각
        ///   ├ mainPanel   : 좌측 viewerCard(작업 표준/도면) + 우측 infoCard(진행 중 공정 정보 + 워크스테이션/상태 띠)
        ///   └ bottomCard  : 탭 헤더(투입자재/BOM 스팩/검사 스팩/연결 장비 + 수동 렌치 + 결과 저장) + 탭별 그리드 4개
        ///
        /// 도킹은 나중에 Add된 컨트롤이 먼저 처리되므로 Add 순서를 바꾸지 말 것
        /// (root: mainPanel → spacerBottom → bottomCard → spacerTop → toolbarCard,
        ///  mainPanel: viewerCard → spacerMain → infoCard).
        ///
        /// 하단 탭은 DevExpress XtraTab 대신 SimpleButton 4개 + 그리드 Visible 토글로 구현했다
        /// (이 개발 PC에 Tab &amp; Navigation 모듈이 설치돼 있지 않다 — README 참고).
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PopScreen));
            this.btnPrevProcess = new System.Windows.Forms.Button();
            this.spacerTop = new System.Windows.Forms.Panel();
            this.mainPanel = new DevExpress.XtraEditors.PanelControl();
            this.viewerCard = new DevExpress.XtraEditors.PanelControl();
            this.pdfViewer = new DevExpress.XtraPdfViewer.PdfViewer();
            this.viewerSurface = new System.Windows.Forms.Panel();
            this.spacerMain = new System.Windows.Forms.Panel();
            this.infoCard = new DevExpress.XtraEditors.PanelControl();
            this.fieldPanel = new DevExpress.XtraEditors.PanelControl();
            this.lblWorkOrderNo = new DevExpress.XtraEditors.LabelControl();
            this.txtWorkOrderNo = new DevExpress.XtraEditors.TextEdit();
            this.lblItemCode = new DevExpress.XtraEditors.LabelControl();
            this.txtItemCode = new DevExpress.XtraEditors.TextEdit();
            this.lblProductSerialNo = new DevExpress.XtraEditors.LabelControl();
            this.txtProductSerialNo = new DevExpress.XtraEditors.TextEdit();
            this.lblRoutingGroup = new DevExpress.XtraEditors.LabelControl();
            this.txtRoutingGroup = new DevExpress.XtraEditors.TextEdit();
            this.lblRoutingOperation = new DevExpress.XtraEditors.LabelControl();
            this.txtRoutingOperation = new DevExpress.XtraEditors.TextEdit();
            this.lblPageOperation = new DevExpress.XtraEditors.LabelControl();
            this.txtPageOperation = new DevExpress.XtraEditors.TextEdit();
            this.lblPageStartTime = new DevExpress.XtraEditors.LabelControl();
            this.txtPageStartTime = new DevExpress.XtraEditors.TextEdit();
            this.lblPageEndTime = new DevExpress.XtraEditors.LabelControl();
            this.txtPageEndTime = new DevExpress.XtraEditors.TextEdit();
            this.lblPageRunTime = new DevExpress.XtraEditors.LabelControl();
            this.txtPageRunTime = new DevExpress.XtraEditors.TextEdit();
            this.lblPageIdleTime = new DevExpress.XtraEditors.LabelControl();
            this.txtPageIdleTime = new DevExpress.XtraEditors.TextEdit();
            this.lblPageWorkTime = new DevExpress.XtraEditors.LabelControl();
            this.txtPageWorkTime = new DevExpress.XtraEditors.TextEdit();
            this.lblInfoTitle = new DevExpress.XtraEditors.LabelControl();
            this.btnInfoAccordion = new SEOJIN_TMES.Client.Forms.Common.AccordionHandle();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblProgressState = new System.Windows.Forms.Label();
            this.lblWorkstation = new System.Windows.Forms.Label();
            this.lblWorker = new System.Windows.Forms.Label();
            this.lblWorkerTile = new System.Windows.Forms.Label();
            this.btnAdminAlarm = new System.Windows.Forms.Button();
            this.btnProcessList = new System.Windows.Forms.Button();
            this.btnStartProcess = new System.Windows.Forms.Button();
            this.btnNextProcess = new System.Windows.Forms.Button();
            this.btnStopProcess = new System.Windows.Forms.Button();
            this.spacerBottom = new System.Windows.Forms.Panel();
            this.bottomCard = new DevExpress.XtraEditors.PanelControl();
            this.pnlTabHead = new DevExpress.XtraEditors.PanelControl();
            this.btnSaveResults = new System.Windows.Forms.Button();
            this.chkManualWrench = new DevExpress.XtraEditors.CheckEdit();
            this.btnTabsAccordion = new SEOJIN_TMES.Client.Forms.Common.AccordionHandle();
            this.tabSpec = new DevExpress.XtraTab.XtraTabControl();
            this.pageMaterial = new DevExpress.XtraTab.XtraTabPage();
            this.gridMaterial = new DevExpress.XtraGrid.GridControl();
            this.gridViewMaterial = new SEOJIN_TMES.Client.Forms.Common.MeisGridView();
            this.pageBom = new DevExpress.XtraTab.XtraTabPage();
            this.gridBomSpec = new DevExpress.XtraGrid.GridControl();
            this.gridViewBomSpec = new SEOJIN_TMES.Client.Forms.Common.MeisGridView();
            this.pageInspec = new DevExpress.XtraTab.XtraTabPage();
            this.gridInspection = new DevExpress.XtraGrid.GridControl();
            this.gridViewInspection = new SEOJIN_TMES.Client.Forms.Common.MeisGridView();
            this.pageEquip = new DevExpress.XtraTab.XtraTabPage();
            this.gridEquipment = new DevExpress.XtraGrid.GridControl();
            this.gridViewEquipment = new SEOJIN_TMES.Client.Forms.Common.MeisGridView();
            this.toolbarCard = new DevExpress.XtraEditors.PanelControl();
            ((System.ComponentModel.ISupportInitialize)(this.mainPanel)).BeginInit();
            this.mainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.viewerCard)).BeginInit();
            this.viewerCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.infoCard)).BeginInit();
            this.infoCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fieldPanel)).BeginInit();
            this.fieldPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtWorkOrderNo.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtItemCode.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtProductSerialNo.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRoutingGroup.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRoutingOperation.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPageOperation.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPageStartTime.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPageEndTime.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPageRunTime.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPageIdleTime.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPageWorkTime.Properties)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bottomCard)).BeginInit();
            this.bottomCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pnlTabHead)).BeginInit();
            this.pnlTabHead.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkManualWrench.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tabSpec)).BeginInit();
            this.tabSpec.SuspendLayout();
            this.pageMaterial.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridMaterial)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewMaterial)).BeginInit();
            this.pageBom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridBomSpec)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewBomSpec)).BeginInit();
            this.pageInspec.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridInspection)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewInspection)).BeginInit();
            this.pageEquip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridEquipment)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewEquipment)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.toolbarCard)).BeginInit();
            this.toolbarCard.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnPrevProcess
            // 
            this.btnPrevProcess.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(70)))), ((int)(((byte)(229)))));
            this.btnPrevProcess.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPrevProcess.Font = new System.Drawing.Font("Arial Narrow", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPrevProcess.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.btnPrevProcess.Image = ((System.Drawing.Image)(resources.GetObject("btnPrevProcess.Image")));
            this.btnPrevProcess.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnPrevProcess.Location = new System.Drawing.Point(13, 7);
            this.btnPrevProcess.Name = "btnPrevProcess";
            this.btnPrevProcess.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.btnPrevProcess.Size = new System.Drawing.Size(124, 31);
            this.btnPrevProcess.TabIndex = 11;
            this.btnPrevProcess.Text = "이전 공정";
            this.btnPrevProcess.UseVisualStyleBackColor = false;
            this.btnPrevProcess.Click += new System.EventHandler(this.btnPrevProcess_Click);
            // 
            // spacerTop
            // 
            this.spacerTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.spacerTop.Location = new System.Drawing.Point(0, 51);
            this.spacerTop.Name = "spacerTop";
            this.spacerTop.Size = new System.Drawing.Size(1509, 8);
            this.spacerTop.TabIndex = 1;
            // 
            // mainPanel
            // 
            this.mainPanel.Controls.Add(this.viewerCard);
            this.mainPanel.Controls.Add(this.spacerMain);
            this.mainPanel.Controls.Add(this.infoCard);
            this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainPanel.Location = new System.Drawing.Point(0, 59);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Size = new System.Drawing.Size(1509, 593);
            this.mainPanel.TabIndex = 2;
            // 
            // viewerCard
            // 
            this.viewerCard.Controls.Add(this.pdfViewer);
            this.viewerCard.Controls.Add(this.viewerSurface);
            this.viewerCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewerCard.Location = new System.Drawing.Point(2, 2);
            this.viewerCard.Name = "viewerCard";
            this.viewerCard.Padding = new System.Windows.Forms.Padding(1);
            this.viewerCard.Size = new System.Drawing.Size(1080, 589);
            this.viewerCard.TabIndex = 0;
            // 
            // pdfViewer
            // 
            this.pdfViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pdfViewer.Location = new System.Drawing.Point(3, 3);
            this.pdfViewer.Name = "pdfViewer";
            this.pdfViewer.NavigationPanePageVisibility = DevExpress.XtraPdfViewer.PdfNavigationPanePageVisibility.None;
            this.pdfViewer.Size = new System.Drawing.Size(1074, 583);
            this.pdfViewer.TabIndex = 0;
            this.pdfViewer.Visible = false;
            this.pdfViewer.ZoomMode = DevExpress.XtraPdfViewer.PdfZoomMode.FitToWidth;
            // 
            // viewerSurface
            // 
            this.viewerSurface.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewerSurface.Location = new System.Drawing.Point(3, 3);
            this.viewerSurface.Name = "viewerSurface";
            this.viewerSurface.Size = new System.Drawing.Size(1074, 583);
            this.viewerSurface.TabIndex = 0;
            this.viewerSurface.Paint += new System.Windows.Forms.PaintEventHandler(this.ViewerSurface_Paint);
            this.viewerSurface.Resize += new System.EventHandler(this.ViewerSurface_Resize);
            // 
            // spacerMain
            // 
            this.spacerMain.Dock = System.Windows.Forms.DockStyle.Right;
            this.spacerMain.Location = new System.Drawing.Point(1082, 2);
            this.spacerMain.Name = "spacerMain";
            this.spacerMain.Size = new System.Drawing.Size(8, 589);
            this.spacerMain.TabIndex = 1;
            // 
            // infoCard
            // 
            this.infoCard.Controls.Add(this.fieldPanel);
            this.infoCard.Controls.Add(this.lblInfoTitle);
            this.infoCard.Controls.Add(this.btnInfoAccordion);
            this.infoCard.Dock = System.Windows.Forms.DockStyle.Right;
            this.infoCard.Location = new System.Drawing.Point(1090, 2);
            this.infoCard.Name = "infoCard";
            this.infoCard.Padding = new System.Windows.Forms.Padding(1);
            this.infoCard.Size = new System.Drawing.Size(417, 589);
            this.infoCard.TabIndex = 2;
            // 
            // fieldPanel
            // 
            this.fieldPanel.Controls.Add(this.lblWorkOrderNo);
            this.fieldPanel.Controls.Add(this.txtWorkOrderNo);
            this.fieldPanel.Controls.Add(this.lblItemCode);
            this.fieldPanel.Controls.Add(this.txtItemCode);
            this.fieldPanel.Controls.Add(this.lblProductSerialNo);
            this.fieldPanel.Controls.Add(this.txtProductSerialNo);
            this.fieldPanel.Controls.Add(this.lblRoutingGroup);
            this.fieldPanel.Controls.Add(this.txtRoutingGroup);
            this.fieldPanel.Controls.Add(this.lblRoutingOperation);
            this.fieldPanel.Controls.Add(this.txtRoutingOperation);
            this.fieldPanel.Controls.Add(this.lblPageOperation);
            this.fieldPanel.Controls.Add(this.txtPageOperation);
            this.fieldPanel.Controls.Add(this.lblPageStartTime);
            this.fieldPanel.Controls.Add(this.txtPageStartTime);
            this.fieldPanel.Controls.Add(this.lblPageEndTime);
            this.fieldPanel.Controls.Add(this.txtPageEndTime);
            this.fieldPanel.Controls.Add(this.lblPageRunTime);
            this.fieldPanel.Controls.Add(this.txtPageRunTime);
            this.fieldPanel.Controls.Add(this.lblPageIdleTime);
            this.fieldPanel.Controls.Add(this.txtPageIdleTime);
            this.fieldPanel.Controls.Add(this.lblPageWorkTime);
            this.fieldPanel.Controls.Add(this.txtPageWorkTime);
            this.fieldPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fieldPanel.Location = new System.Drawing.Point(29, 57);
            this.fieldPanel.Name = "fieldPanel";
            this.fieldPanel.Size = new System.Drawing.Size(385, 529);
            this.fieldPanel.TabIndex = 1;
            // 
            // lblWorkOrderNo
            // 
            this.lblWorkOrderNo.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWorkOrderNo.Appearance.Options.UseFont = true;
            this.lblWorkOrderNo.Location = new System.Drawing.Point(14, 14);
            this.lblWorkOrderNo.Name = "lblWorkOrderNo";
            this.lblWorkOrderNo.Size = new System.Drawing.Size(88, 18);
            this.lblWorkOrderNo.TabIndex = 0;
            this.lblWorkOrderNo.Text = "· 작업지시번호";
            // 
            // txtWorkOrderNo
            // 
            this.txtWorkOrderNo.Location = new System.Drawing.Point(212, 11);
            this.txtWorkOrderNo.Name = "txtWorkOrderNo";
            this.txtWorkOrderNo.Properties.Appearance.BackColor = System.Drawing.Color.White;
            this.txtWorkOrderNo.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtWorkOrderNo.Properties.Appearance.Options.UseBackColor = true;
            this.txtWorkOrderNo.Properties.Appearance.Options.UseFont = true;
            this.txtWorkOrderNo.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.txtWorkOrderNo.Properties.ReadOnly = true;
            this.txtWorkOrderNo.Size = new System.Drawing.Size(160, 24);
            this.txtWorkOrderNo.TabIndex = 1;
            // 
            // lblItemCode
            // 
            this.lblItemCode.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblItemCode.Appearance.Options.UseFont = true;
            this.lblItemCode.Location = new System.Drawing.Point(14, 49);
            this.lblItemCode.Name = "lblItemCode";
            this.lblItemCode.Size = new System.Drawing.Size(88, 18);
            this.lblItemCode.TabIndex = 2;
            this.lblItemCode.Text = "· 생산품목코드";
            // 
            // txtItemCode
            // 
            this.txtItemCode.Location = new System.Drawing.Point(212, 46);
            this.txtItemCode.Name = "txtItemCode";
            this.txtItemCode.Properties.Appearance.BackColor = System.Drawing.Color.White;
            this.txtItemCode.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtItemCode.Properties.Appearance.Options.UseBackColor = true;
            this.txtItemCode.Properties.Appearance.Options.UseFont = true;
            this.txtItemCode.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.txtItemCode.Properties.ReadOnly = true;
            this.txtItemCode.Size = new System.Drawing.Size(160, 24);
            this.txtItemCode.TabIndex = 3;
            // 
            // lblProductSerialNo
            // 
            this.lblProductSerialNo.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProductSerialNo.Appearance.Options.UseFont = true;
            this.lblProductSerialNo.Location = new System.Drawing.Point(14, 84);
            this.lblProductSerialNo.Name = "lblProductSerialNo";
            this.lblProductSerialNo.Size = new System.Drawing.Size(132, 18);
            this.lblProductSerialNo.TabIndex = 4;
            this.lblProductSerialNo.Text = "· 생산품목 시리얼번호";
            // 
            // txtProductSerialNo
            // 
            this.txtProductSerialNo.Location = new System.Drawing.Point(212, 81);
            this.txtProductSerialNo.Name = "txtProductSerialNo";
            this.txtProductSerialNo.Properties.Appearance.BackColor = System.Drawing.Color.White;
            this.txtProductSerialNo.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtProductSerialNo.Properties.Appearance.Options.UseBackColor = true;
            this.txtProductSerialNo.Properties.Appearance.Options.UseFont = true;
            this.txtProductSerialNo.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.txtProductSerialNo.Properties.ReadOnly = true;
            this.txtProductSerialNo.Size = new System.Drawing.Size(160, 24);
            this.txtProductSerialNo.TabIndex = 5;
            // 
            // lblRoutingGroup
            // 
            this.lblRoutingGroup.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRoutingGroup.Appearance.Options.UseFont = true;
            this.lblRoutingGroup.Location = new System.Drawing.Point(14, 119);
            this.lblRoutingGroup.Name = "lblRoutingGroup";
            this.lblRoutingGroup.Size = new System.Drawing.Size(80, 18);
            this.lblRoutingGroup.TabIndex = 6;
            this.lblRoutingGroup.Text = "· 라우팅 그룹";
            // 
            // txtRoutingGroup
            // 
            this.txtRoutingGroup.Location = new System.Drawing.Point(212, 116);
            this.txtRoutingGroup.Name = "txtRoutingGroup";
            this.txtRoutingGroup.Properties.Appearance.BackColor = System.Drawing.Color.White;
            this.txtRoutingGroup.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtRoutingGroup.Properties.Appearance.Options.UseBackColor = true;
            this.txtRoutingGroup.Properties.Appearance.Options.UseFont = true;
            this.txtRoutingGroup.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.txtRoutingGroup.Properties.ReadOnly = true;
            this.txtRoutingGroup.Size = new System.Drawing.Size(160, 24);
            this.txtRoutingGroup.TabIndex = 7;
            // 
            // lblRoutingOperation
            // 
            this.lblRoutingOperation.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRoutingOperation.Appearance.Options.UseFont = true;
            this.lblRoutingOperation.Location = new System.Drawing.Point(14, 154);
            this.lblRoutingOperation.Name = "lblRoutingOperation";
            this.lblRoutingOperation.Size = new System.Drawing.Size(80, 18);
            this.lblRoutingOperation.TabIndex = 8;
            this.lblRoutingOperation.Text = "· 라우팅 공정";
            // 
            // txtRoutingOperation
            // 
            this.txtRoutingOperation.Location = new System.Drawing.Point(212, 151);
            this.txtRoutingOperation.Name = "txtRoutingOperation";
            this.txtRoutingOperation.Properties.Appearance.BackColor = System.Drawing.Color.White;
            this.txtRoutingOperation.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtRoutingOperation.Properties.Appearance.Options.UseBackColor = true;
            this.txtRoutingOperation.Properties.Appearance.Options.UseFont = true;
            this.txtRoutingOperation.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.txtRoutingOperation.Properties.ReadOnly = true;
            this.txtRoutingOperation.Size = new System.Drawing.Size(160, 24);
            this.txtRoutingOperation.TabIndex = 9;
            // 
            // lblPageOperation
            // 
            this.lblPageOperation.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPageOperation.Appearance.Options.UseFont = true;
            this.lblPageOperation.Location = new System.Drawing.Point(14, 189);
            this.lblPageOperation.Name = "lblPageOperation";
            this.lblPageOperation.Size = new System.Drawing.Size(80, 18);
            this.lblPageOperation.TabIndex = 10;
            this.lblPageOperation.Text = "· 페이지 공정";
            // 
            // txtPageOperation
            // 
            this.txtPageOperation.Location = new System.Drawing.Point(212, 186);
            this.txtPageOperation.Name = "txtPageOperation";
            this.txtPageOperation.Properties.Appearance.BackColor = System.Drawing.Color.White;
            this.txtPageOperation.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPageOperation.Properties.Appearance.Options.UseBackColor = true;
            this.txtPageOperation.Properties.Appearance.Options.UseFont = true;
            this.txtPageOperation.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.txtPageOperation.Properties.ReadOnly = true;
            this.txtPageOperation.Size = new System.Drawing.Size(160, 24);
            this.txtPageOperation.TabIndex = 11;
            // 
            // lblPageStartTime
            // 
            this.lblPageStartTime.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPageStartTime.Appearance.Options.UseFont = true;
            this.lblPageStartTime.Location = new System.Drawing.Point(14, 224);
            this.lblPageStartTime.Name = "lblPageStartTime";
            this.lblPageStartTime.Size = new System.Drawing.Size(137, 18);
            this.lblPageStartTime.TabIndex = 12;
            this.lblPageStartTime.Text = "· 페이지 공정 시작시각";
            // 
            // txtPageStartTime
            // 
            this.txtPageStartTime.Location = new System.Drawing.Point(212, 221);
            this.txtPageStartTime.Name = "txtPageStartTime";
            this.txtPageStartTime.Properties.Appearance.BackColor = System.Drawing.Color.White;
            this.txtPageStartTime.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPageStartTime.Properties.Appearance.Options.UseBackColor = true;
            this.txtPageStartTime.Properties.Appearance.Options.UseFont = true;
            this.txtPageStartTime.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.txtPageStartTime.Properties.ReadOnly = true;
            this.txtPageStartTime.Size = new System.Drawing.Size(160, 24);
            this.txtPageStartTime.TabIndex = 13;
            // 
            // lblPageEndTime
            // 
            this.lblPageEndTime.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPageEndTime.Appearance.Options.UseFont = true;
            this.lblPageEndTime.Location = new System.Drawing.Point(14, 259);
            this.lblPageEndTime.Name = "lblPageEndTime";
            this.lblPageEndTime.Size = new System.Drawing.Size(137, 18);
            this.lblPageEndTime.TabIndex = 14;
            this.lblPageEndTime.Text = "· 페이지 공정 종료시각";
            // 
            // txtPageEndTime
            // 
            this.txtPageEndTime.Location = new System.Drawing.Point(212, 256);
            this.txtPageEndTime.Name = "txtPageEndTime";
            this.txtPageEndTime.Properties.Appearance.BackColor = System.Drawing.Color.White;
            this.txtPageEndTime.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPageEndTime.Properties.Appearance.Options.UseBackColor = true;
            this.txtPageEndTime.Properties.Appearance.Options.UseFont = true;
            this.txtPageEndTime.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.txtPageEndTime.Properties.ReadOnly = true;
            this.txtPageEndTime.Size = new System.Drawing.Size(160, 24);
            this.txtPageEndTime.TabIndex = 15;
            // 
            // lblPageRunTime
            // 
            this.lblPageRunTime.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPageRunTime.Appearance.Options.UseFont = true;
            this.lblPageRunTime.Location = new System.Drawing.Point(14, 294);
            this.lblPageRunTime.Name = "lblPageRunTime";
            this.lblPageRunTime.Size = new System.Drawing.Size(137, 18);
            this.lblPageRunTime.TabIndex = 16;
            this.lblPageRunTime.Text = "· 페이지 공정 가동시간";
            // 
            // txtPageRunTime
            // 
            this.txtPageRunTime.Location = new System.Drawing.Point(212, 291);
            this.txtPageRunTime.Name = "txtPageRunTime";
            this.txtPageRunTime.Properties.Appearance.BackColor = System.Drawing.Color.White;
            this.txtPageRunTime.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPageRunTime.Properties.Appearance.Options.UseBackColor = true;
            this.txtPageRunTime.Properties.Appearance.Options.UseFont = true;
            this.txtPageRunTime.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.txtPageRunTime.Properties.ReadOnly = true;
            this.txtPageRunTime.Size = new System.Drawing.Size(160, 24);
            this.txtPageRunTime.TabIndex = 17;
            // 
            // lblPageIdleTime
            // 
            this.lblPageIdleTime.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPageIdleTime.Appearance.Options.UseFont = true;
            this.lblPageIdleTime.Location = new System.Drawing.Point(14, 329);
            this.lblPageIdleTime.Name = "lblPageIdleTime";
            this.lblPageIdleTime.Size = new System.Drawing.Size(150, 18);
            this.lblPageIdleTime.TabIndex = 18;
            this.lblPageIdleTime.Text = "· 페이지 공정 비가동시간";
            // 
            // txtPageIdleTime
            // 
            this.txtPageIdleTime.Location = new System.Drawing.Point(212, 326);
            this.txtPageIdleTime.Name = "txtPageIdleTime";
            this.txtPageIdleTime.Properties.Appearance.BackColor = System.Drawing.Color.White;
            this.txtPageIdleTime.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPageIdleTime.Properties.Appearance.Options.UseBackColor = true;
            this.txtPageIdleTime.Properties.Appearance.Options.UseFont = true;
            this.txtPageIdleTime.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.txtPageIdleTime.Properties.ReadOnly = true;
            this.txtPageIdleTime.Size = new System.Drawing.Size(160, 24);
            this.txtPageIdleTime.TabIndex = 19;
            // 
            // lblPageWorkTime
            // 
            this.lblPageWorkTime.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPageWorkTime.Appearance.Options.UseFont = true;
            this.lblPageWorkTime.Location = new System.Drawing.Point(14, 364);
            this.lblPageWorkTime.Name = "lblPageWorkTime";
            this.lblPageWorkTime.Size = new System.Drawing.Size(137, 18);
            this.lblPageWorkTime.TabIndex = 20;
            this.lblPageWorkTime.Text = "· 페이지 공정 작업시간";
            // 
            // txtPageWorkTime
            // 
            this.txtPageWorkTime.Location = new System.Drawing.Point(212, 361);
            this.txtPageWorkTime.Name = "txtPageWorkTime";
            this.txtPageWorkTime.Properties.Appearance.BackColor = System.Drawing.Color.White;
            this.txtPageWorkTime.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPageWorkTime.Properties.Appearance.Options.UseBackColor = true;
            this.txtPageWorkTime.Properties.Appearance.Options.UseFont = true;
            this.txtPageWorkTime.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.txtPageWorkTime.Properties.ReadOnly = true;
            this.txtPageWorkTime.Size = new System.Drawing.Size(160, 24);
            this.txtPageWorkTime.TabIndex = 21;
            // 
            // lblInfoTitle
            // 
            this.lblInfoTitle.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInfoTitle.Appearance.Options.UseFont = true;
            this.lblInfoTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblInfoTitle.ImageAlignToText = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            this.lblInfoTitle.ImageOptions.Alignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblInfoTitle.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("lblInfoTitle.ImageOptions.Image")));
            this.lblInfoTitle.Location = new System.Drawing.Point(29, 3);
            this.lblInfoTitle.Name = "lblInfoTitle";
            this.lblInfoTitle.Padding = new System.Windows.Forms.Padding(16, 10, 0, 8);
            this.lblInfoTitle.Size = new System.Drawing.Size(166, 54);
            this.lblInfoTitle.TabIndex = 0;
            this.lblInfoTitle.Text = "진행 중 공정 정보";
            // 
            // btnInfoAccordion
            // 
            this.btnInfoAccordion.Accent = System.Drawing.Color.FromArgb(((int)(((byte)(124)))), ((int)(((byte)(58)))), ((int)(((byte)(237)))));
            this.btnInfoAccordion.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnInfoAccordion.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnInfoAccordion.Edge = SEOJIN_TMES.Client.Forms.Common.AccordionEdge.Left;
            this.btnInfoAccordion.Expanded = true;
            this.btnInfoAccordion.Font = new System.Drawing.Font("맑은 고딕", 8.5F);
            this.btnInfoAccordion.Location = new System.Drawing.Point(3, 3);
            this.btnInfoAccordion.Name = "btnInfoAccordion";
            this.btnInfoAccordion.Size = new System.Drawing.Size(26, 583);
            this.btnInfoAccordion.TabIndex = 20;
            this.btnInfoAccordion.Click += new System.EventHandler(this.btnInfoAccordion_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblProgressState);
            this.panel1.Controls.Add(this.lblWorkstation);
            this.panel1.Controls.Add(this.lblWorker);
            this.panel1.Controls.Add(this.lblWorkerTile);
            this.panel1.Controls.Add(this.btnAdminAlarm);
            this.panel1.Controls.Add(this.btnProcessList);
            this.panel1.Controls.Add(this.btnStartProcess);
            this.panel1.Controls.Add(this.btnNextProcess);
            this.panel1.Controls.Add(this.btnStopProcess);
            this.panel1.Controls.Add(this.btnPrevProcess);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(2, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1505, 46);
            this.panel1.TabIndex = 14;
            // 
            // lblProgressState
            // 
            this.lblProgressState.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblProgressState.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(124)))), ((int)(((byte)(58)))), ((int)(((byte)(237)))));
            this.lblProgressState.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblProgressState.Font = new System.Drawing.Font("Arial", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProgressState.ForeColor = System.Drawing.Color.White;
            this.lblProgressState.Location = new System.Drawing.Point(768, 7);
            this.lblProgressState.Name = "lblProgressState";
            this.lblProgressState.Size = new System.Drawing.Size(205, 31);
            this.lblProgressState.TabIndex = 27;
            this.lblProgressState.Text = "공정 상태";
            this.lblProgressState.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblWorkstation
            // 
            this.lblWorkstation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWorkstation.BackColor = System.Drawing.Color.White;
            this.lblWorkstation.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblWorkstation.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWorkstation.Location = new System.Drawing.Point(1233, 7);
            this.lblWorkstation.Name = "lblWorkstation";
            this.lblWorkstation.Size = new System.Drawing.Size(270, 28);
            this.lblWorkstation.TabIndex = 26;
            this.lblWorkstation.Text = "Workstation";
            this.lblWorkstation.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblWorker
            // 
            this.lblWorker.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWorker.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblWorker.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWorker.Location = new System.Drawing.Point(1037, 7);
            this.lblWorker.Name = "lblWorker";
            this.lblWorker.Size = new System.Drawing.Size(195, 27);
            this.lblWorker.TabIndex = 18;
            this.lblWorker.Text = "작업자";
            this.lblWorker.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblWorkerTile
            // 
            this.lblWorkerTile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWorkerTile.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWorkerTile.Location = new System.Drawing.Point(977, 8);
            this.lblWorkerTile.Name = "lblWorkerTile";
            this.lblWorkerTile.Size = new System.Drawing.Size(65, 27);
            this.lblWorkerTile.TabIndex = 17;
            this.lblWorkerTile.Text = "작업자";
            this.lblWorkerTile.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnAdminAlarm
            // 
            this.btnAdminAlarm.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(158)))), ((int)(((byte)(11)))));
            this.btnAdminAlarm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAdminAlarm.Font = new System.Drawing.Font("Arial Narrow", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAdminAlarm.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.btnAdminAlarm.Image = ((System.Drawing.Image)(resources.GetObject("btnAdminAlarm.Image")));
            this.btnAdminAlarm.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnAdminAlarm.Location = new System.Drawing.Point(663, 7);
            this.btnAdminAlarm.Name = "btnAdminAlarm";
            this.btnAdminAlarm.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.btnAdminAlarm.Size = new System.Drawing.Size(124, 31);
            this.btnAdminAlarm.TabIndex = 16;
            this.btnAdminAlarm.Text = "알   림";
            this.btnAdminAlarm.UseVisualStyleBackColor = false;
            this.btnAdminAlarm.Click += new System.EventHandler(this.btnAdminAlarm_Click);
            // 
            // btnProcessList
            // 
            this.btnProcessList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(124)))), ((int)(((byte)(58)))), ((int)(((byte)(237)))));
            this.btnProcessList.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnProcessList.Font = new System.Drawing.Font("Arial Narrow", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnProcessList.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.btnProcessList.Image = ((System.Drawing.Image)(resources.GetObject("btnProcessList.Image")));
            this.btnProcessList.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnProcessList.Location = new System.Drawing.Point(533, 7);
            this.btnProcessList.Name = "btnProcessList";
            this.btnProcessList.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.btnProcessList.Size = new System.Drawing.Size(124, 31);
            this.btnProcessList.TabIndex = 15;
            this.btnProcessList.Text = "공정 목록";
            this.btnProcessList.UseVisualStyleBackColor = false;
            this.btnProcessList.Click += new System.EventHandler(this.btnProcessList_Click);
            // 
            // btnStartProcess
            // 
            this.btnStartProcess.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(185)))), ((int)(((byte)(129)))));
            this.btnStartProcess.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartProcess.Font = new System.Drawing.Font("Arial Narrow", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStartProcess.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.btnStartProcess.Image = ((System.Drawing.Image)(resources.GetObject("btnStartProcess.Image")));
            this.btnStartProcess.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnStartProcess.Location = new System.Drawing.Point(403, 7);
            this.btnStartProcess.Name = "btnStartProcess";
            this.btnStartProcess.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.btnStartProcess.Size = new System.Drawing.Size(124, 31);
            this.btnStartProcess.TabIndex = 14;
            this.btnStartProcess.Text = "공정 시작";
            this.btnStartProcess.UseVisualStyleBackColor = false;
            this.btnStartProcess.Click += new System.EventHandler(this.ProcessAction_Click);
            // 
            // btnNextProcess
            // 
            this.btnNextProcess.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(70)))), ((int)(((byte)(229)))));
            this.btnNextProcess.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNextProcess.Font = new System.Drawing.Font("Arial Narrow", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNextProcess.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.btnNextProcess.Image = ((System.Drawing.Image)(resources.GetObject("btnNextProcess.Image")));
            this.btnNextProcess.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNextProcess.Location = new System.Drawing.Point(143, 7);
            this.btnNextProcess.Name = "btnNextProcess";
            this.btnNextProcess.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.btnNextProcess.Size = new System.Drawing.Size(124, 31);
            this.btnNextProcess.TabIndex = 12;
            this.btnNextProcess.Text = "이전 공정";
            this.btnNextProcess.UseVisualStyleBackColor = false;
            this.btnNextProcess.Click += new System.EventHandler(this.btnNextProcess_Click);
            // 
            // btnStopProcess
            // 
            this.btnStopProcess.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(63)))), ((int)(((byte)(94)))));
            this.btnStopProcess.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStopProcess.Font = new System.Drawing.Font("Arial Narrow", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStopProcess.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.btnStopProcess.Image = ((System.Drawing.Image)(resources.GetObject("btnStopProcess.Image")));
            this.btnStopProcess.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnStopProcess.Location = new System.Drawing.Point(273, 7);
            this.btnStopProcess.Name = "btnStopProcess";
            this.btnStopProcess.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.btnStopProcess.Size = new System.Drawing.Size(124, 31);
            this.btnStopProcess.TabIndex = 13;
            this.btnStopProcess.Text = "공정 중지";
            this.btnStopProcess.UseVisualStyleBackColor = false;
            this.btnStopProcess.Click += new System.EventHandler(this.ProcessAction_Click);
            // 
            // spacerBottom
            // 
            this.spacerBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.spacerBottom.Location = new System.Drawing.Point(0, 652);
            this.spacerBottom.Name = "spacerBottom";
            this.spacerBottom.Size = new System.Drawing.Size(1509, 8);
            this.spacerBottom.TabIndex = 3;
            // 
            // bottomCard
            // 
            this.bottomCard.Controls.Add(this.pnlTabHead);
            this.bottomCard.Controls.Add(this.tabSpec);
            this.bottomCard.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bottomCard.Location = new System.Drawing.Point(0, 660);
            this.bottomCard.Name = "bottomCard";
            this.bottomCard.Padding = new System.Windows.Forms.Padding(1);
            this.bottomCard.Size = new System.Drawing.Size(1509, 200);
            this.bottomCard.TabIndex = 4;
            // 
            // pnlTabHead
            // 
            this.pnlTabHead.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlTabHead.Controls.Add(this.btnSaveResults);
            this.pnlTabHead.Controls.Add(this.chkManualWrench);
            this.pnlTabHead.Controls.Add(this.btnTabsAccordion);
            this.pnlTabHead.Location = new System.Drawing.Point(1090, 3);
            this.pnlTabHead.Name = "pnlTabHead";
            this.pnlTabHead.Size = new System.Drawing.Size(417, 28);
            this.pnlTabHead.TabIndex = 4;
            // 
            // btnSaveResults
            // 
            this.btnSaveResults.BackColor = System.Drawing.Color.Transparent;
            this.btnSaveResults.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveResults.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSaveResults.Location = new System.Drawing.Point(132, 3);
            this.btnSaveResults.Name = "btnSaveResults";
            this.btnSaveResults.Size = new System.Drawing.Size(104, 25);
            this.btnSaveResults.TabIndex = 25;
            this.btnSaveResults.Text = "결과 저장";
            this.btnSaveResults.UseVisualStyleBackColor = false;
            this.btnSaveResults.Click += new System.EventHandler(this.btnSaveResults_Click);
            // 
            // chkManualWrench
            // 
            this.chkManualWrench.Location = new System.Drawing.Point(16, 4);
            this.chkManualWrench.Name = "chkManualWrench";
            this.chkManualWrench.Properties.Appearance.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkManualWrench.Properties.Appearance.Options.UseFont = true;
            this.chkManualWrench.Properties.Caption = "수동 렌치";
            this.chkManualWrench.Size = new System.Drawing.Size(110, 20);
            this.chkManualWrench.TabIndex = 22;
            this.chkManualWrench.Visible = false;
            this.chkManualWrench.CheckedChanged += new System.EventHandler(this.chkManualWrench_CheckedChanged);
            // 
            // btnTabsAccordion
            // 
            this.btnTabsAccordion.Accent = System.Drawing.Color.FromArgb(((int)(((byte)(124)))), ((int)(((byte)(58)))), ((int)(((byte)(237)))));
            this.btnTabsAccordion.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTabsAccordion.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnTabsAccordion.Edge = SEOJIN_TMES.Client.Forms.Common.AccordionEdge.Left;
            this.btnTabsAccordion.Expanded = true;
            this.btnTabsAccordion.Font = new System.Drawing.Font("맑은 고딕", 8.5F);
            this.btnTabsAccordion.Location = new System.Drawing.Point(311, 2);
            this.btnTabsAccordion.Name = "btnTabsAccordion";
            this.btnTabsAccordion.Size = new System.Drawing.Size(104, 24);
            this.btnTabsAccordion.TabIndex = 24;
            this.btnTabsAccordion.Click += new System.EventHandler(this.btnTabsAccordion_Click);
            // 
            // tabSpec
            // 
            this.tabSpec.AppearancePage.Header.Font = new System.Drawing.Font("Arial Narrow", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabSpec.AppearancePage.Header.Options.UseFont = true;
            this.tabSpec.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabSpec.Location = new System.Drawing.Point(3, 3);
            this.tabSpec.Name = "tabSpec";
            this.tabSpec.SelectedTabPage = this.pageMaterial;
            this.tabSpec.Size = new System.Drawing.Size(1503, 194);
            this.tabSpec.TabIndex = 2;
            this.tabSpec.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            this.pageMaterial,
            this.pageBom,
            this.pageInspec,
            this.pageEquip});
            this.tabSpec.SelectedPageChanged += new DevExpress.XtraTab.TabPageChangedEventHandler(this.tabSpec_SelectedPageChanged);
            // 
            // pageMaterial
            // 
            this.pageMaterial.Controls.Add(this.gridMaterial);
            this.pageMaterial.Name = "pageMaterial";
            this.pageMaterial.Size = new System.Drawing.Size(1501, 162);
            this.pageMaterial.Text = "Material";
            // 
            // gridMaterial
            // 
            this.gridMaterial.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridMaterial.Location = new System.Drawing.Point(0, 0);
            this.gridMaterial.MainView = this.gridViewMaterial;
            this.gridMaterial.Name = "gridMaterial";
            this.gridMaterial.Size = new System.Drawing.Size(1501, 162);
            this.gridMaterial.TabIndex = 0;
            this.gridMaterial.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridViewMaterial});
            // 
            // gridViewMaterial
            // 
            this.gridViewMaterial.Accent = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(70)))), ((int)(((byte)(229)))));
            this.gridViewMaterial.GridControl = this.gridMaterial;
            this.gridViewMaterial.Name = "gridViewMaterial";
            this.gridViewMaterial.OptionsBehavior.Editable = false;
            this.gridViewMaterial.RowCellClick += new DevExpress.XtraGrid.Views.Grid.RowCellClickEventHandler(this.GridViewMaterial_RowCellClick);
            // 
            // pageBom
            // 
            this.pageBom.Controls.Add(this.gridBomSpec);
            this.pageBom.Name = "pageBom";
            this.pageBom.Size = new System.Drawing.Size(1501, 162);
            this.pageBom.Text = "Bom Spec";
            // 
            // gridBomSpec
            // 
            this.gridBomSpec.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridBomSpec.Location = new System.Drawing.Point(0, 0);
            this.gridBomSpec.MainView = this.gridViewBomSpec;
            this.gridBomSpec.Name = "gridBomSpec";
            this.gridBomSpec.Size = new System.Drawing.Size(1501, 162);
            this.gridBomSpec.TabIndex = 1;
            this.gridBomSpec.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridViewBomSpec});
            // 
            // gridViewBomSpec
            // 
            this.gridViewBomSpec.Accent = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(70)))), ((int)(((byte)(229)))));
            this.gridViewBomSpec.GridControl = this.gridBomSpec;
            this.gridViewBomSpec.Name = "gridViewBomSpec";
            this.gridViewBomSpec.RowClick += new DevExpress.XtraGrid.Views.Grid.RowClickEventHandler(this.GridViewBomSpec_RowClick);
            this.gridViewBomSpec.RowCellStyle += new DevExpress.XtraGrid.Views.Grid.RowCellStyleEventHandler(this.GridViewBomSpec_RowCellStyle);
            this.gridViewBomSpec.FocusedRowChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(this.GridViewBomSpec_FocusedRowChanged);
            this.gridViewBomSpec.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(this.GridViewBomSpec_CellValueChanged);
            this.gridViewBomSpec.ValidatingEditor += new DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventHandler(this.GridViewBomSpec_ValidatingEditor);
            this.gridViewBomSpec.InvalidValueException += new DevExpress.XtraEditors.Controls.InvalidValueExceptionEventHandler(this.GridViewBomSpec_InvalidValueException);
            // 
            // pageInspec
            // 
            this.pageInspec.Controls.Add(this.gridInspection);
            this.pageInspec.Name = "pageInspec";
            this.pageInspec.Size = new System.Drawing.Size(1501, 162);
            this.pageInspec.Text = "Inspection";
            // 
            // gridInspection
            // 
            this.gridInspection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridInspection.Location = new System.Drawing.Point(0, 0);
            this.gridInspection.MainView = this.gridViewInspection;
            this.gridInspection.Name = "gridInspection";
            this.gridInspection.Size = new System.Drawing.Size(1501, 162);
            this.gridInspection.TabIndex = 2;
            this.gridInspection.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridViewInspection});
            // 
            // gridViewInspection
            // 
            this.gridViewInspection.Accent = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(70)))), ((int)(((byte)(229)))));
            this.gridViewInspection.GridControl = this.gridInspection;
            this.gridViewInspection.Name = "gridViewInspection";
            this.gridViewInspection.ShowingEditor += this.GridViewInspection_ShowingEditor;
            // 
            // pageEquip
            // 
            this.pageEquip.Controls.Add(this.gridEquipment);
            this.pageEquip.Name = "pageEquip";
            this.pageEquip.Size = new System.Drawing.Size(1501, 162);
            this.pageEquip.Text = "Equipment";
            // 
            // gridEquipment
            // 
            this.gridEquipment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridEquipment.Location = new System.Drawing.Point(0, 0);
            this.gridEquipment.MainView = this.gridViewEquipment;
            this.gridEquipment.Name = "gridEquipment";
            this.gridEquipment.Size = new System.Drawing.Size(1501, 162);
            this.gridEquipment.TabIndex = 3;
            this.gridEquipment.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridViewEquipment});
            // 
            // gridViewEquipment
            // 
            this.gridViewEquipment.Accent = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(70)))), ((int)(((byte)(229)))));
            this.gridViewEquipment.GridControl = this.gridEquipment;
            this.gridViewEquipment.Name = "gridViewEquipment";
            this.gridViewEquipment.OptionsBehavior.Editable = false;
            this.gridViewEquipment.RowCellStyle += new DevExpress.XtraGrid.Views.Grid.RowCellStyleEventHandler(this.GridViewEquipment_RowCellStyle);
            // 
            // toolbarCard
            // 
            this.toolbarCard.Controls.Add(this.panel1);
            this.toolbarCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.toolbarCard.Location = new System.Drawing.Point(0, 0);
            this.toolbarCard.Name = "toolbarCard";
            this.toolbarCard.Size = new System.Drawing.Size(1509, 51);
            this.toolbarCard.TabIndex = 0;
            // 
            // PopScreen
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.mainPanel);
            this.Controls.Add(this.spacerBottom);
            this.Controls.Add(this.bottomCard);
            this.Controls.Add(this.spacerTop);
            this.Controls.Add(this.toolbarCard);
            this.Name = "PopScreen";
            this.Size = new System.Drawing.Size(1509, 860);
            this.Load += new System.EventHandler(this.PopScreen_Load);
            this.Disposed += new System.EventHandler(this.PopScreen_Disposed);
            ((System.ComponentModel.ISupportInitialize)(this.mainPanel)).EndInit();
            this.mainPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.viewerCard)).EndInit();
            this.viewerCard.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.infoCard)).EndInit();
            this.infoCard.ResumeLayout(false);
            this.infoCard.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fieldPanel)).EndInit();
            this.fieldPanel.ResumeLayout(false);
            this.fieldPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtWorkOrderNo.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtItemCode.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtProductSerialNo.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRoutingGroup.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRoutingOperation.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPageOperation.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPageStartTime.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPageEndTime.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPageRunTime.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPageIdleTime.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPageWorkTime.Properties)).EndInit();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.bottomCard)).EndInit();
            this.bottomCard.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pnlTabHead)).EndInit();
            this.pnlTabHead.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chkManualWrench.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tabSpec)).EndInit();
            this.tabSpec.ResumeLayout(false);
            this.pageMaterial.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridMaterial)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewMaterial)).EndInit();
            this.pageBom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridBomSpec)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewBomSpec)).EndInit();
            this.pageInspec.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridInspection)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewInspection)).EndInit();
            this.pageEquip.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridEquipment)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewEquipment)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.toolbarCard)).EndInit();
            this.toolbarCard.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel spacerTop;
        private DevExpress.XtraEditors.PanelControl mainPanel;
        private DevExpress.XtraEditors.PanelControl viewerCard;
        private DevExpress.XtraPdfViewer.PdfViewer pdfViewer;
        private System.Windows.Forms.Panel viewerSurface;
        private System.Windows.Forms.Panel spacerMain;
        private DevExpress.XtraEditors.PanelControl infoCard;
        private DevExpress.XtraEditors.LabelControl lblInfoTitle;
        private SEOJIN_TMES.Client.Forms.Common.AccordionHandle btnInfoAccordion;
        private DevExpress.XtraEditors.PanelControl fieldPanel;
        private DevExpress.XtraEditors.LabelControl lblWorkOrderNo;
        private DevExpress.XtraEditors.TextEdit txtWorkOrderNo;
        private DevExpress.XtraEditors.LabelControl lblItemCode;
        private DevExpress.XtraEditors.TextEdit txtItemCode;
        private DevExpress.XtraEditors.LabelControl lblProductSerialNo;
        private DevExpress.XtraEditors.TextEdit txtProductSerialNo;
        private DevExpress.XtraEditors.LabelControl lblRoutingGroup;
        private DevExpress.XtraEditors.TextEdit txtRoutingGroup;
        private DevExpress.XtraEditors.LabelControl lblRoutingOperation;
        private DevExpress.XtraEditors.TextEdit txtRoutingOperation;
        private DevExpress.XtraEditors.LabelControl lblPageOperation;
        private DevExpress.XtraEditors.TextEdit txtPageOperation;
        private DevExpress.XtraEditors.LabelControl lblPageStartTime;
        private DevExpress.XtraEditors.TextEdit txtPageStartTime;
        private DevExpress.XtraEditors.LabelControl lblPageEndTime;
        private DevExpress.XtraEditors.TextEdit txtPageEndTime;
        private DevExpress.XtraEditors.LabelControl lblPageRunTime;
        private DevExpress.XtraEditors.TextEdit txtPageRunTime;
        private DevExpress.XtraEditors.LabelControl lblPageIdleTime;
        private DevExpress.XtraEditors.TextEdit txtPageIdleTime;
        private DevExpress.XtraEditors.LabelControl lblPageWorkTime;
        private DevExpress.XtraEditors.TextEdit txtPageWorkTime;
        private System.Windows.Forms.Panel spacerBottom;
        private DevExpress.XtraEditors.PanelControl bottomCard;
        private DevExpress.XtraGrid.GridControl gridMaterial;
        private SEOJIN_TMES.Client.Forms.Common.MeisGridView gridViewMaterial;
        private DevExpress.XtraGrid.GridControl gridBomSpec;
        private SEOJIN_TMES.Client.Forms.Common.MeisGridView gridViewBomSpec;
        private DevExpress.XtraGrid.GridControl gridInspection;
        private SEOJIN_TMES.Client.Forms.Common.MeisGridView gridViewInspection;
        private DevExpress.XtraGrid.GridControl gridEquipment;
        private SEOJIN_TMES.Client.Forms.Common.MeisGridView gridViewEquipment;
        private DevExpress.XtraTab.XtraTabControl tabSpec;
        private DevExpress.XtraTab.XtraTabPage pageMaterial;
        private DevExpress.XtraTab.XtraTabPage pageBom;
        private DevExpress.XtraTab.XtraTabPage pageInspec;
        private DevExpress.XtraTab.XtraTabPage pageEquip;
        private System.Windows.Forms.Button btnPrevProcess;
        private System.Windows.Forms.Button btnStopProcess;
        private System.Windows.Forms.Button btnNextProcess;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnAdminAlarm;
        private System.Windows.Forms.Button btnProcessList;
        private System.Windows.Forms.Button btnStartProcess;
        private System.Windows.Forms.Label lblWorkerTile;
        private DevExpress.XtraEditors.PanelControl toolbarCard;
        private System.Windows.Forms.Label lblWorker;
        private DevExpress.XtraEditors.CheckEdit chkManualWrench;
        private Common.AccordionHandle btnTabsAccordion;
        private DevExpress.XtraEditors.PanelControl pnlTabHead;
        private System.Windows.Forms.Label lblProgressState;
        private System.Windows.Forms.Label lblWorkstation;
        private System.Windows.Forms.Button btnSaveResults;
    }
}
