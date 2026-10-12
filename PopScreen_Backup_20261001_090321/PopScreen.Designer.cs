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
            DevExpress.XtraEditors.TileItemElement tileItemElement1 = new DevExpress.XtraEditors.TileItemElement();
            DevExpress.XtraEditors.TileItemElement tileItemElement2 = new DevExpress.XtraEditors.TileItemElement();
            DevExpress.XtraEditors.TileItemElement tileItemElement3 = new DevExpress.XtraEditors.TileItemElement();
            DevExpress.XtraEditors.TileItemElement tileItemElement4 = new DevExpress.XtraEditors.TileItemElement();
            DevExpress.XtraEditors.TileItemElement tileItemElement5 = new DevExpress.XtraEditors.TileItemElement();
            DevExpress.XtraEditors.TileItemElement tileItemElement6 = new DevExpress.XtraEditors.TileItemElement();
            DevExpress.XtraEditors.TileItemElement tileItemElement7 = new DevExpress.XtraEditors.TileItemElement();
            this.toolbarCard = new DevExpress.XtraEditors.PanelControl();
            this.tileProcess = new DevExpress.XtraBars.Navigation.TileBar();
            this.tileGroupProcess = new DevExpress.XtraBars.Navigation.TileBarGroup();
            this.tilePrevProcess = new DevExpress.XtraBars.Navigation.TileBarItem();
            this.tileNextProcess = new DevExpress.XtraBars.Navigation.TileBarItem();
            this.tileSkipProcess = new DevExpress.XtraBars.Navigation.TileBarItem();
            this.tileStopProcess = new DevExpress.XtraBars.Navigation.TileBarItem();
            this.tileStartProcess = new DevExpress.XtraBars.Navigation.TileBarItem();
            this.tileProcessList = new DevExpress.XtraBars.Navigation.TileBarItem();
            this.tileAdminAlarm = new DevExpress.XtraBars.Navigation.TileBarItem();
            this.lblWorkerCaption = new DevExpress.XtraEditors.LabelControl();
            this.lblWorker = new DevExpress.XtraEditors.LabelControl();
            this.lblTimeCaption = new DevExpress.XtraEditors.LabelControl();
            this.lblCurrentTime = new DevExpress.XtraEditors.LabelControl();
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
            this.bannerPanel = new DevExpress.XtraEditors.PanelControl();
            this.lblProgressState = new DevExpress.XtraEditors.LabelControl();
            this.lblWorkstation = new DevExpress.XtraEditors.LabelControl();
            this.lblInfoTitle = new DevExpress.XtraEditors.LabelControl();
            this.btnInfoAccordion = new SEOJIN_TMES.Client.Forms.Common.AccordionHandle();
            this.spacerBottom = new System.Windows.Forms.Panel();
            this.bottomCard = new DevExpress.XtraEditors.PanelControl();
            this.tabBodyPanel = new DevExpress.XtraEditors.PanelControl();
            this.gridMaterial = new DevExpress.XtraGrid.GridControl();
            this.gridViewMaterial = new SEOJIN_TMES.Client.Forms.Common.MeisGridView();
            this.gridBomSpec = new DevExpress.XtraGrid.GridControl();
            this.gridViewBomSpec = new SEOJIN_TMES.Client.Forms.Common.MeisGridView();
            this.gridInspection = new DevExpress.XtraGrid.GridControl();
            this.gridViewInspection = new SEOJIN_TMES.Client.Forms.Common.MeisGridView();
            this.gridEquipment = new DevExpress.XtraGrid.GridControl();
            this.gridViewEquipment = new SEOJIN_TMES.Client.Forms.Common.MeisGridView();
            this.tabHeaderPanel = new DevExpress.XtraEditors.PanelControl();
            this.btnTabMaterial = new SEOJIN_TMES.Client.Forms.Common.MeisButton();
            this.btnTabBomSpec = new SEOJIN_TMES.Client.Forms.Common.MeisButton();
            this.btnTabInspection = new SEOJIN_TMES.Client.Forms.Common.MeisButton();
            this.btnTabEquipment = new SEOJIN_TMES.Client.Forms.Common.MeisButton();
            this.chkManualWrench = new DevExpress.XtraEditors.CheckEdit();
            this.btnSaveResults = new SEOJIN_TMES.Client.Forms.Common.MeisButton();
            this.btnTabsAccordion = new SEOJIN_TMES.Client.Forms.Common.AccordionHandle();
            ((System.ComponentModel.ISupportInitialize)(this.toolbarCard)).BeginInit();
            this.toolbarCard.SuspendLayout();
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
            ((System.ComponentModel.ISupportInitialize)(this.bannerPanel)).BeginInit();
            this.bannerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bottomCard)).BeginInit();
            this.bottomCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tabBodyPanel)).BeginInit();
            this.tabBodyPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridMaterial)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewMaterial)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridBomSpec)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewBomSpec)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridInspection)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewInspection)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridEquipment)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewEquipment)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tabHeaderPanel)).BeginInit();
            this.tabHeaderPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkManualWrench.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // toolbarCard
            // 
            this.toolbarCard.Controls.Add(this.tileProcess);
            this.toolbarCard.Controls.Add(this.lblWorkerCaption);
            this.toolbarCard.Controls.Add(this.lblWorker);
            this.toolbarCard.Controls.Add(this.lblTimeCaption);
            this.toolbarCard.Controls.Add(this.lblCurrentTime);
            this.toolbarCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.toolbarCard.Location = new System.Drawing.Point(0, 0);
            this.toolbarCard.Name = "toolbarCard";
            this.toolbarCard.Size = new System.Drawing.Size(1509, 104);
            this.toolbarCard.TabIndex = 0;
            // 
            // tileProcess
            // 
            this.tileProcess.DropDownOptions.BeakColor = System.Drawing.Color.Empty;
            this.tileProcess.Groups.Add(this.tileGroupProcess);
            this.tileProcess.Location = new System.Drawing.Point(14, 8);
            this.tileProcess.Name = "tileProcess";
            this.tileProcess.ScrollMode = DevExpress.XtraEditors.TileControlScrollMode.ScrollButtons;
            this.tileProcess.Size = new System.Drawing.Size(800, 88);
            this.tileProcess.TabIndex = 0;
            // 
            // tileGroupProcess
            // 
            this.tileGroupProcess.Items.Add(this.tilePrevProcess);
            this.tileGroupProcess.Items.Add(this.tileNextProcess);
            this.tileGroupProcess.Items.Add(this.tileSkipProcess);
            this.tileGroupProcess.Items.Add(this.tileStopProcess);
            this.tileGroupProcess.Items.Add(this.tileStartProcess);
            this.tileGroupProcess.Items.Add(this.tileProcessList);
            this.tileGroupProcess.Items.Add(this.tileAdminAlarm);
            this.tileGroupProcess.Name = "tileGroupProcess";
            // 
            // tilePrevProcess
            // 
            this.tilePrevProcess.DropDownOptions.BeakColor = System.Drawing.Color.Empty;
            tileItemElement1.Text = "이전 공정";
            this.tilePrevProcess.Elements.Add(tileItemElement1);
            this.tilePrevProcess.Id = 0;
            this.tilePrevProcess.Name = "tilePrevProcess";
            this.tilePrevProcess.ItemClick += new DevExpress.XtraEditors.TileItemClickEventHandler(this.tilePrevProcess_ItemClick);
            // 
            // tileNextProcess
            // 
            this.tileNextProcess.DropDownOptions.BeakColor = System.Drawing.Color.Empty;
            tileItemElement2.Text = "다음 공정";
            this.tileNextProcess.Elements.Add(tileItemElement2);
            this.tileNextProcess.Id = 1;
            this.tileNextProcess.Name = "tileNextProcess";
            this.tileNextProcess.ItemClick += new DevExpress.XtraEditors.TileItemClickEventHandler(this.tileNextProcess_ItemClick);
            // 
            // tileSkipProcess
            // 
            this.tileSkipProcess.DropDownOptions.BeakColor = System.Drawing.Color.Empty;
            tileItemElement3.Text = "공정 생략";
            this.tileSkipProcess.Elements.Add(tileItemElement3);
            this.tileSkipProcess.Id = 2;
            this.tileSkipProcess.Name = "tileSkipProcess";
            this.tileSkipProcess.ItemClick += new DevExpress.XtraEditors.TileItemClickEventHandler(this.ProcessActionTile_ItemClick);
            // 
            // tileStopProcess
            // 
            this.tileStopProcess.DropDownOptions.BeakColor = System.Drawing.Color.Empty;
            tileItemElement4.Text = "공정 중지";
            this.tileStopProcess.Elements.Add(tileItemElement4);
            this.tileStopProcess.Id = 3;
            this.tileStopProcess.Name = "tileStopProcess";
            this.tileStopProcess.ItemClick += new DevExpress.XtraEditors.TileItemClickEventHandler(this.ProcessActionTile_ItemClick);
            // 
            // tileStartProcess
            // 
            this.tileStartProcess.DropDownOptions.BeakColor = System.Drawing.Color.Empty;
            tileItemElement5.Text = "공정 시작";
            this.tileStartProcess.Elements.Add(tileItemElement5);
            this.tileStartProcess.Id = 4;
            this.tileStartProcess.Name = "tileStartProcess";
            this.tileStartProcess.ItemClick += new DevExpress.XtraEditors.TileItemClickEventHandler(this.ProcessActionTile_ItemClick);
            // 
            // tileProcessList
            // 
            this.tileProcessList.DropDownOptions.BeakColor = System.Drawing.Color.Empty;
            tileItemElement6.Text = "공정 목록";
            this.tileProcessList.Elements.Add(tileItemElement6);
            this.tileProcessList.Id = 5;
            this.tileProcessList.Name = "tileProcessList";
            this.tileProcessList.ItemClick += new DevExpress.XtraEditors.TileItemClickEventHandler(this.tileProcessList_ItemClick);
            // 
            // tileAdminAlarm
            // 
            this.tileAdminAlarm.DropDownOptions.BeakColor = System.Drawing.Color.Empty;
            tileItemElement7.Text = "관리자 알람";
            this.tileAdminAlarm.Elements.Add(tileItemElement7);
            this.tileAdminAlarm.Id = 6;
            this.tileAdminAlarm.Name = "tileAdminAlarm";
            this.tileAdminAlarm.ItemClick += new DevExpress.XtraEditors.TileItemClickEventHandler(this.tileAdminAlarm_ItemClick);
            // 
            // lblWorkerCaption
            // 
            this.lblWorkerCaption.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWorkerCaption.Location = new System.Drawing.Point(1269, 24);
            this.lblWorkerCaption.Name = "lblWorkerCaption";
            this.lblWorkerCaption.Size = new System.Drawing.Size(30, 14);
            this.lblWorkerCaption.TabIndex = 7;
            this.lblWorkerCaption.Text = "작업자";
            // 
            // lblWorker
            // 
            this.lblWorker.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWorker.Location = new System.Drawing.Point(1269, 50);
            this.lblWorker.Name = "lblWorker";
            this.lblWorker.Size = new System.Drawing.Size(0, 14);
            this.lblWorker.TabIndex = 8;
            // 
            // lblTimeCaption
            // 
            this.lblTimeCaption.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTimeCaption.Location = new System.Drawing.Point(1381, 24);
            this.lblTimeCaption.Name = "lblTimeCaption";
            this.lblTimeCaption.Size = new System.Drawing.Size(40, 14);
            this.lblTimeCaption.TabIndex = 9;
            this.lblTimeCaption.Text = "현재시각";
            // 
            // lblCurrentTime
            // 
            this.lblCurrentTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCurrentTime.Location = new System.Drawing.Point(1381, 50);
            this.lblCurrentTime.Name = "lblCurrentTime";
            this.lblCurrentTime.Size = new System.Drawing.Size(0, 14);
            this.lblCurrentTime.TabIndex = 10;
            // 
            // spacerTop
            // 
            this.spacerTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.spacerTop.Location = new System.Drawing.Point(0, 104);
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
            this.mainPanel.Location = new System.Drawing.Point(0, 112);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Size = new System.Drawing.Size(1509, 420);
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
            this.viewerCard.Size = new System.Drawing.Size(911, 416);
            this.viewerCard.TabIndex = 0;
            // 
            // pdfViewer
            // 
            this.pdfViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pdfViewer.Location = new System.Drawing.Point(3, 3);
            this.pdfViewer.Name = "pdfViewer";
            this.pdfViewer.NavigationPanePageVisibility = DevExpress.XtraPdfViewer.PdfNavigationPanePageVisibility.None;
            this.pdfViewer.Size = new System.Drawing.Size(905, 410);
            this.pdfViewer.TabIndex = 0;
            this.pdfViewer.Visible = false;
            this.pdfViewer.ZoomMode = DevExpress.XtraPdfViewer.PdfZoomMode.FitToWidth;
            // 
            // viewerSurface
            // 
            this.viewerSurface.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewerSurface.Location = new System.Drawing.Point(3, 3);
            this.viewerSurface.Name = "viewerSurface";
            this.viewerSurface.Size = new System.Drawing.Size(905, 410);
            this.viewerSurface.TabIndex = 0;
            this.viewerSurface.Paint += new System.Windows.Forms.PaintEventHandler(this.ViewerSurface_Paint);
            this.viewerSurface.Resize += new System.EventHandler(this.ViewerSurface_Resize);
            // 
            // spacerMain
            // 
            this.spacerMain.Dock = System.Windows.Forms.DockStyle.Right;
            this.spacerMain.Location = new System.Drawing.Point(913, 2);
            this.spacerMain.Name = "spacerMain";
            this.spacerMain.Size = new System.Drawing.Size(8, 416);
            this.spacerMain.TabIndex = 1;
            // 
            // infoCard
            // 
            this.infoCard.Controls.Add(this.fieldPanel);
            this.infoCard.Controls.Add(this.bannerPanel);
            this.infoCard.Controls.Add(this.lblInfoTitle);
            this.infoCard.Controls.Add(this.btnInfoAccordion);
            this.infoCard.Dock = System.Windows.Forms.DockStyle.Right;
            this.infoCard.Location = new System.Drawing.Point(921, 2);
            this.infoCard.Name = "infoCard";
            this.infoCard.Padding = new System.Windows.Forms.Padding(1);
            this.infoCard.Size = new System.Drawing.Size(586, 416);
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
            this.fieldPanel.Location = new System.Drawing.Point(29, 35);
            this.fieldPanel.Name = "fieldPanel";
            this.fieldPanel.Size = new System.Drawing.Size(554, 260);
            this.fieldPanel.TabIndex = 1;
            // 
            // lblWorkOrderNo
            // 
            this.lblWorkOrderNo.Location = new System.Drawing.Point(14, 14);
            this.lblWorkOrderNo.Name = "lblWorkOrderNo";
            this.lblWorkOrderNo.Size = new System.Drawing.Size(68, 14);
            this.lblWorkOrderNo.TabIndex = 0;
            this.lblWorkOrderNo.Text = "· 작업지시번호";
            // 
            // txtWorkOrderNo
            // 
            this.txtWorkOrderNo.Location = new System.Drawing.Point(152, 10);
            this.txtWorkOrderNo.Name = "txtWorkOrderNo";
            this.txtWorkOrderNo.Properties.ReadOnly = true;
            this.txtWorkOrderNo.Size = new System.Drawing.Size(112, 20);
            this.txtWorkOrderNo.TabIndex = 1;
            // 
            // lblItemCode
            // 
            this.lblItemCode.Location = new System.Drawing.Point(280, 14);
            this.lblItemCode.Name = "lblItemCode";
            this.lblItemCode.Size = new System.Drawing.Size(68, 14);
            this.lblItemCode.TabIndex = 2;
            this.lblItemCode.Text = "· 생산품목코드";
            // 
            // txtItemCode
            // 
            this.txtItemCode.Location = new System.Drawing.Point(418, 10);
            this.txtItemCode.Name = "txtItemCode";
            this.txtItemCode.Properties.ReadOnly = true;
            this.txtItemCode.Size = new System.Drawing.Size(112, 20);
            this.txtItemCode.TabIndex = 3;
            // 
            // lblProductSerialNo
            // 
            this.lblProductSerialNo.Location = new System.Drawing.Point(14, 46);
            this.lblProductSerialNo.Name = "lblProductSerialNo";
            this.lblProductSerialNo.Size = new System.Drawing.Size(102, 14);
            this.lblProductSerialNo.TabIndex = 4;
            this.lblProductSerialNo.Text = "· 생산품목 시리얼번호";
            // 
            // txtProductSerialNo
            // 
            this.txtProductSerialNo.Location = new System.Drawing.Point(152, 42);
            this.txtProductSerialNo.Name = "txtProductSerialNo";
            this.txtProductSerialNo.Properties.ReadOnly = true;
            this.txtProductSerialNo.Size = new System.Drawing.Size(112, 20);
            this.txtProductSerialNo.TabIndex = 5;
            // 
            // lblRoutingGroup
            // 
            this.lblRoutingGroup.Location = new System.Drawing.Point(280, 46);
            this.lblRoutingGroup.Name = "lblRoutingGroup";
            this.lblRoutingGroup.Size = new System.Drawing.Size(62, 14);
            this.lblRoutingGroup.TabIndex = 6;
            this.lblRoutingGroup.Text = "· 라우팅 그룹";
            // 
            // txtRoutingGroup
            // 
            this.txtRoutingGroup.Location = new System.Drawing.Point(418, 42);
            this.txtRoutingGroup.Name = "txtRoutingGroup";
            this.txtRoutingGroup.Properties.ReadOnly = true;
            this.txtRoutingGroup.Size = new System.Drawing.Size(112, 20);
            this.txtRoutingGroup.TabIndex = 7;
            // 
            // lblRoutingOperation
            // 
            this.lblRoutingOperation.Location = new System.Drawing.Point(14, 78);
            this.lblRoutingOperation.Name = "lblRoutingOperation";
            this.lblRoutingOperation.Size = new System.Drawing.Size(62, 14);
            this.lblRoutingOperation.TabIndex = 8;
            this.lblRoutingOperation.Text = "· 라우팅 공정";
            // 
            // txtRoutingOperation
            // 
            this.txtRoutingOperation.Location = new System.Drawing.Point(152, 74);
            this.txtRoutingOperation.Name = "txtRoutingOperation";
            this.txtRoutingOperation.Properties.ReadOnly = true;
            this.txtRoutingOperation.Size = new System.Drawing.Size(112, 20);
            this.txtRoutingOperation.TabIndex = 9;
            // 
            // lblPageOperation
            // 
            this.lblPageOperation.Location = new System.Drawing.Point(280, 78);
            this.lblPageOperation.Name = "lblPageOperation";
            this.lblPageOperation.Size = new System.Drawing.Size(62, 14);
            this.lblPageOperation.TabIndex = 10;
            this.lblPageOperation.Text = "· 페이지 공정";
            // 
            // txtPageOperation
            // 
            this.txtPageOperation.Location = new System.Drawing.Point(418, 74);
            this.txtPageOperation.Name = "txtPageOperation";
            this.txtPageOperation.Properties.ReadOnly = true;
            this.txtPageOperation.Size = new System.Drawing.Size(112, 20);
            this.txtPageOperation.TabIndex = 11;
            // 
            // lblPageStartTime
            // 
            this.lblPageStartTime.Location = new System.Drawing.Point(14, 110);
            this.lblPageStartTime.Name = "lblPageStartTime";
            this.lblPageStartTime.Size = new System.Drawing.Size(106, 14);
            this.lblPageStartTime.TabIndex = 12;
            this.lblPageStartTime.Text = "· 페이지 공정 시작시각";
            // 
            // txtPageStartTime
            // 
            this.txtPageStartTime.Location = new System.Drawing.Point(152, 106);
            this.txtPageStartTime.Name = "txtPageStartTime";
            this.txtPageStartTime.Properties.ReadOnly = true;
            this.txtPageStartTime.Size = new System.Drawing.Size(112, 20);
            this.txtPageStartTime.TabIndex = 13;
            // 
            // lblPageEndTime
            // 
            this.lblPageEndTime.Location = new System.Drawing.Point(280, 110);
            this.lblPageEndTime.Name = "lblPageEndTime";
            this.lblPageEndTime.Size = new System.Drawing.Size(106, 14);
            this.lblPageEndTime.TabIndex = 14;
            this.lblPageEndTime.Text = "· 페이지 공정 종료시각";
            // 
            // txtPageEndTime
            // 
            this.txtPageEndTime.Location = new System.Drawing.Point(418, 106);
            this.txtPageEndTime.Name = "txtPageEndTime";
            this.txtPageEndTime.Properties.ReadOnly = true;
            this.txtPageEndTime.Size = new System.Drawing.Size(112, 20);
            this.txtPageEndTime.TabIndex = 15;
            // 
            // lblPageRunTime
            // 
            this.lblPageRunTime.Location = new System.Drawing.Point(14, 142);
            this.lblPageRunTime.Name = "lblPageRunTime";
            this.lblPageRunTime.Size = new System.Drawing.Size(106, 14);
            this.lblPageRunTime.TabIndex = 16;
            this.lblPageRunTime.Text = "· 페이지 공정 가동시간";
            // 
            // txtPageRunTime
            // 
            this.txtPageRunTime.Location = new System.Drawing.Point(152, 138);
            this.txtPageRunTime.Name = "txtPageRunTime";
            this.txtPageRunTime.Properties.ReadOnly = true;
            this.txtPageRunTime.Size = new System.Drawing.Size(112, 20);
            this.txtPageRunTime.TabIndex = 17;
            // 
            // lblPageIdleTime
            // 
            this.lblPageIdleTime.Location = new System.Drawing.Point(280, 142);
            this.lblPageIdleTime.Name = "lblPageIdleTime";
            this.lblPageIdleTime.Size = new System.Drawing.Size(116, 14);
            this.lblPageIdleTime.TabIndex = 18;
            this.lblPageIdleTime.Text = "· 페이지 공정 비가동시간";
            // 
            // txtPageIdleTime
            // 
            this.txtPageIdleTime.Location = new System.Drawing.Point(418, 138);
            this.txtPageIdleTime.Name = "txtPageIdleTime";
            this.txtPageIdleTime.Properties.ReadOnly = true;
            this.txtPageIdleTime.Size = new System.Drawing.Size(112, 20);
            this.txtPageIdleTime.TabIndex = 19;
            // 
            // lblPageWorkTime
            // 
            this.lblPageWorkTime.Location = new System.Drawing.Point(14, 174);
            this.lblPageWorkTime.Name = "lblPageWorkTime";
            this.lblPageWorkTime.Size = new System.Drawing.Size(106, 14);
            this.lblPageWorkTime.TabIndex = 20;
            this.lblPageWorkTime.Text = "· 페이지 공정 작업시간";
            // 
            // txtPageWorkTime
            // 
            this.txtPageWorkTime.Location = new System.Drawing.Point(152, 170);
            this.txtPageWorkTime.Name = "txtPageWorkTime";
            this.txtPageWorkTime.Properties.ReadOnly = true;
            this.txtPageWorkTime.Size = new System.Drawing.Size(112, 20);
            this.txtPageWorkTime.TabIndex = 21;
            // 
            // bannerPanel
            // 
            this.bannerPanel.Controls.Add(this.lblProgressState);
            this.bannerPanel.Controls.Add(this.lblWorkstation);
            this.bannerPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bannerPanel.Location = new System.Drawing.Point(29, 295);
            this.bannerPanel.Name = "bannerPanel";
            this.bannerPanel.Padding = new System.Windows.Forms.Padding(14, 0, 14, 14);
            this.bannerPanel.Size = new System.Drawing.Size(554, 118);
            this.bannerPanel.TabIndex = 2;
            // 
            // lblProgressState
            // 
            this.lblProgressState.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblProgressState.Location = new System.Drawing.Point(16, 88);
            this.lblProgressState.Name = "lblProgressState";
            this.lblProgressState.Size = new System.Drawing.Size(0, 14);
            this.lblProgressState.TabIndex = 1;
            // 
            // lblWorkstation
            // 
            this.lblWorkstation.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblWorkstation.Location = new System.Drawing.Point(16, 2);
            this.lblWorkstation.Name = "lblWorkstation";
            this.lblWorkstation.Size = new System.Drawing.Size(0, 14);
            this.lblWorkstation.TabIndex = 0;
            // 
            // lblInfoTitle
            // 
            this.lblInfoTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblInfoTitle.Location = new System.Drawing.Point(29, 3);
            this.lblInfoTitle.Name = "lblInfoTitle";
            this.lblInfoTitle.Padding = new System.Windows.Forms.Padding(16, 10, 0, 8);
            this.lblInfoTitle.Size = new System.Drawing.Size(98, 32);
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
            this.btnInfoAccordion.Size = new System.Drawing.Size(26, 410);
            this.btnInfoAccordion.TabIndex = 20;
            this.btnInfoAccordion.Click += new System.EventHandler(this.btnInfoAccordion_Click);
            // 
            // spacerBottom
            // 
            this.spacerBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.spacerBottom.Location = new System.Drawing.Point(0, 532);
            this.spacerBottom.Name = "spacerBottom";
            this.spacerBottom.Size = new System.Drawing.Size(1509, 8);
            this.spacerBottom.TabIndex = 3;
            // 
            // bottomCard
            // 
            this.bottomCard.Controls.Add(this.tabBodyPanel);
            this.bottomCard.Controls.Add(this.tabHeaderPanel);
            this.bottomCard.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bottomCard.Location = new System.Drawing.Point(0, 540);
            this.bottomCard.Name = "bottomCard";
            this.bottomCard.Padding = new System.Windows.Forms.Padding(1);
            this.bottomCard.Size = new System.Drawing.Size(1509, 320);
            this.bottomCard.TabIndex = 4;
            // 
            // tabBodyPanel
            // 
            this.tabBodyPanel.Controls.Add(this.gridMaterial);
            this.tabBodyPanel.Controls.Add(this.gridBomSpec);
            this.tabBodyPanel.Controls.Add(this.gridInspection);
            this.tabBodyPanel.Controls.Add(this.gridEquipment);
            this.tabBodyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabBodyPanel.Location = new System.Drawing.Point(3, 49);
            this.tabBodyPanel.Name = "tabBodyPanel";
            this.tabBodyPanel.Size = new System.Drawing.Size(1503, 268);
            this.tabBodyPanel.TabIndex = 1;
            // 
            // gridMaterial
            // 
            this.gridMaterial.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridMaterial.Location = new System.Drawing.Point(2, 2);
            this.gridMaterial.MainView = this.gridViewMaterial;
            this.gridMaterial.Name = "gridMaterial";
            this.gridMaterial.Size = new System.Drawing.Size(1499, 264);
            this.gridMaterial.TabIndex = 0;
            this.gridMaterial.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridViewMaterial});
            // 
            // gridViewMaterial
            // 
            this.gridViewMaterial.Accent = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(70)))), ((int)(((byte)(229)))));
            this.gridViewMaterial.GridControl = this.gridMaterial;
            this.gridViewMaterial.Name = "gridViewMaterial";
            this.gridViewMaterial.RowCellClick += new DevExpress.XtraGrid.Views.Grid.RowCellClickEventHandler(this.GridViewMaterial_RowCellClick);
            this.gridViewMaterial.OptionsBehavior.Editable = false;
            // 
            // gridBomSpec
            // 
            this.gridBomSpec.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridBomSpec.Location = new System.Drawing.Point(2, 2);
            this.gridBomSpec.MainView = this.gridViewBomSpec;
            this.gridBomSpec.Name = "gridBomSpec";
            this.gridBomSpec.Size = new System.Drawing.Size(1499, 264);
            this.gridBomSpec.TabIndex = 1;
            this.gridBomSpec.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridViewBomSpec});
            this.gridBomSpec.Visible = false;
            // 
            // gridViewBomSpec
            // 
            this.gridViewBomSpec.Accent = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(70)))), ((int)(((byte)(229)))));
            this.gridViewBomSpec.GridControl = this.gridBomSpec;
            this.gridViewBomSpec.Name = "gridViewBomSpec";
            this.gridViewBomSpec.CustomColumnDisplayText += this.Numeric_CustomColumnDisplayText;
            this.gridViewBomSpec.RowCellStyle += this.GridViewBomSpec_RowCellStyle;
            this.gridViewBomSpec.ValidatingEditor += this.GridViewBomSpec_ValidatingEditor;
            this.gridViewBomSpec.InvalidValueException += this.GridViewBomSpec_InvalidValueException;
            this.gridViewBomSpec.CellValueChanged += this.GridViewBomSpec_CellValueChanged;
            this.gridViewBomSpec.RowClick += this.GridViewBomSpec_RowClick;
            // 
            // gridInspection
            // 
            this.gridInspection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridInspection.Location = new System.Drawing.Point(2, 2);
            this.gridInspection.MainView = this.gridViewInspection;
            this.gridInspection.Name = "gridInspection";
            this.gridInspection.Size = new System.Drawing.Size(1499, 264);
            this.gridInspection.TabIndex = 2;
            this.gridInspection.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridViewInspection});
            this.gridInspection.Visible = false;
            // 
            // gridViewInspection
            // 
            this.gridViewInspection.Accent = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(70)))), ((int)(((byte)(229)))));
            this.gridViewInspection.GridControl = this.gridInspection;
            this.gridViewInspection.Name = "gridViewInspection";
            this.gridViewInspection.ShowingEditor += this.GridViewInspection_ShowingEditor;
            // 
            // gridEquipment
            // 
            this.gridEquipment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridEquipment.Location = new System.Drawing.Point(2, 2);
            this.gridEquipment.MainView = this.gridViewEquipment;
            this.gridEquipment.Name = "gridEquipment";
            this.gridEquipment.Size = new System.Drawing.Size(1499, 264);
            this.gridEquipment.TabIndex = 3;
            this.gridEquipment.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridViewEquipment});
            this.gridEquipment.Visible = false;
            // 
            // gridViewEquipment
            // 
            this.gridViewEquipment.Accent = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(70)))), ((int)(((byte)(229)))));
            this.gridViewEquipment.GridControl = this.gridEquipment;
            this.gridViewEquipment.Name = "gridViewEquipment";
            this.gridViewEquipment.CustomColumnDisplayText += this.Numeric_CustomColumnDisplayText;
            this.gridViewEquipment.RowCellStyle += this.GridViewEquipment_RowCellStyle;
            this.gridViewEquipment.OptionsBehavior.Editable = false;
            // 
            // tabHeaderPanel
            // 
            this.tabHeaderPanel.Controls.Add(this.btnTabMaterial);
            this.tabHeaderPanel.Controls.Add(this.btnTabBomSpec);
            this.tabHeaderPanel.Controls.Add(this.btnTabInspection);
            this.tabHeaderPanel.Controls.Add(this.btnTabEquipment);
            this.tabHeaderPanel.Controls.Add(this.chkManualWrench);
            this.tabHeaderPanel.Controls.Add(this.btnSaveResults);
            this.tabHeaderPanel.Controls.Add(this.btnTabsAccordion);
            this.tabHeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabHeaderPanel.Location = new System.Drawing.Point(3, 3);
            this.tabHeaderPanel.Name = "tabHeaderPanel";
            this.tabHeaderPanel.Size = new System.Drawing.Size(1503, 46);
            this.tabHeaderPanel.TabIndex = 0;
            // 
            // btnTabMaterial
            // 
            this.btnTabMaterial.Accent = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(70)))), ((int)(((byte)(229)))));
            this.btnTabMaterial.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTabMaterial.Icon = null;
            this.btnTabMaterial.Location = new System.Drawing.Point(14, 7);
            this.btnTabMaterial.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
            this.btnTabMaterial.Name = "btnTabMaterial";
            this.btnTabMaterial.PaintStyle = DevExpress.XtraEditors.Controls.PaintStyles.Light;
            this.btnTabMaterial.Selected = false;
            this.btnTabMaterial.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            this.btnTabMaterial.Size = new System.Drawing.Size(120, 32);
            this.btnTabMaterial.TabIndex = 0;
            this.btnTabMaterial.Text = "투입자재";
            this.btnTabMaterial.TextAfterIcon = false;
            this.btnTabMaterial.Click += new System.EventHandler(this.TabButton_Click);
            // 
            // btnTabBomSpec
            // 
            this.btnTabBomSpec.Accent = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(70)))), ((int)(((byte)(229)))));
            this.btnTabBomSpec.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTabBomSpec.Icon = null;
            this.btnTabBomSpec.Location = new System.Drawing.Point(138, 7);
            this.btnTabBomSpec.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
            this.btnTabBomSpec.Name = "btnTabBomSpec";
            this.btnTabBomSpec.PaintStyle = DevExpress.XtraEditors.Controls.PaintStyles.Light;
            this.btnTabBomSpec.Selected = false;
            this.btnTabBomSpec.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            this.btnTabBomSpec.Size = new System.Drawing.Size(120, 32);
            this.btnTabBomSpec.TabIndex = 1;
            this.btnTabBomSpec.Text = "BOM 스팩";
            this.btnTabBomSpec.TextAfterIcon = false;
            this.btnTabBomSpec.Click += new System.EventHandler(this.TabButton_Click);
            // 
            // btnTabInspection
            // 
            this.btnTabInspection.Accent = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(70)))), ((int)(((byte)(229)))));
            this.btnTabInspection.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTabInspection.Icon = null;
            this.btnTabInspection.Location = new System.Drawing.Point(262, 7);
            this.btnTabInspection.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
            this.btnTabInspection.Name = "btnTabInspection";
            this.btnTabInspection.PaintStyle = DevExpress.XtraEditors.Controls.PaintStyles.Light;
            this.btnTabInspection.Selected = false;
            this.btnTabInspection.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            this.btnTabInspection.Size = new System.Drawing.Size(120, 32);
            this.btnTabInspection.TabIndex = 2;
            this.btnTabInspection.Text = "검사 스팩";
            this.btnTabInspection.TextAfterIcon = false;
            this.btnTabInspection.Click += new System.EventHandler(this.TabButton_Click);
            // 
            // btnTabEquipment
            // 
            this.btnTabEquipment.Accent = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(70)))), ((int)(((byte)(229)))));
            this.btnTabEquipment.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTabEquipment.Icon = null;
            this.btnTabEquipment.Location = new System.Drawing.Point(386, 7);
            this.btnTabEquipment.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
            this.btnTabEquipment.Name = "btnTabEquipment";
            this.btnTabEquipment.PaintStyle = DevExpress.XtraEditors.Controls.PaintStyles.Light;
            this.btnTabEquipment.Selected = false;
            this.btnTabEquipment.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            this.btnTabEquipment.Size = new System.Drawing.Size(120, 32);
            this.btnTabEquipment.TabIndex = 3;
            this.btnTabEquipment.Text = "연결 장비";
            this.btnTabEquipment.TextAfterIcon = false;
            this.btnTabEquipment.Click += new System.EventHandler(this.TabButton_Click);
            // 
            // chkManualWrench
            // 
            this.chkManualWrench.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkManualWrench.Location = new System.Drawing.Point(1069, 11);
            this.chkManualWrench.Name = "chkManualWrench";
            this.chkManualWrench.Properties.Caption = "수동 렌치";
            this.chkManualWrench.Size = new System.Drawing.Size(110, 20);
            this.chkManualWrench.TabIndex = 4;
            this.chkManualWrench.CheckedChanged += new System.EventHandler(this.chkManualWrench_CheckedChanged);
            // 
            // btnSaveResults
            // 
            this.btnSaveResults.Accent = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(70)))), ((int)(((byte)(229)))));
            this.btnSaveResults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveResults.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSaveResults.Icon = null;
            this.btnSaveResults.Location = new System.Drawing.Point(1197, 7);
            this.btnSaveResults.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
            this.btnSaveResults.Name = "btnSaveResults";
            this.btnSaveResults.PaintStyle = DevExpress.XtraEditors.Controls.PaintStyles.Light;
            this.btnSaveResults.Selected = false;
            this.btnSaveResults.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            this.btnSaveResults.Size = new System.Drawing.Size(134, 32);
            this.btnSaveResults.TabIndex = 5;
            this.btnSaveResults.Text = "결과 저장";
            this.btnSaveResults.TextAfterIcon = false;
            this.btnSaveResults.Click += new System.EventHandler(this.btnSaveResults_Click);
            // 
            // btnTabsAccordion
            // 
            this.btnTabsAccordion.Accent = System.Drawing.Color.FromArgb(((int)(((byte)(124)))), ((int)(((byte)(58)))), ((int)(((byte)(237)))));
            this.btnTabsAccordion.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTabsAccordion.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnTabsAccordion.Edge = SEOJIN_TMES.Client.Forms.Common.AccordionEdge.Left;
            this.btnTabsAccordion.Expanded = true;
            this.btnTabsAccordion.Font = new System.Drawing.Font("맑은 고딕", 8.5F);
            this.btnTabsAccordion.Location = new System.Drawing.Point(1405, 2);
            this.btnTabsAccordion.Name = "btnTabsAccordion";
            this.btnTabsAccordion.Size = new System.Drawing.Size(96, 42);
            this.btnTabsAccordion.TabIndex = 21;
            this.btnTabsAccordion.Click += new System.EventHandler(this.btnTabsAccordion_Click);
            // 
            // PopScreen
            // 
            this.Controls.Add(this.mainPanel);
            this.Controls.Add(this.spacerBottom);
            this.Controls.Add(this.bottomCard);
            this.Controls.Add(this.spacerTop);
            this.Controls.Add(this.toolbarCard);
            this.Name = "PopScreen";
            this.Size = new System.Drawing.Size(1509, 860);
            this.Load += new System.EventHandler(this.PopScreen_Load);
            this.Disposed += new System.EventHandler(this.PopScreen_Disposed);
            ((System.ComponentModel.ISupportInitialize)(this.toolbarCard)).EndInit();
            this.toolbarCard.ResumeLayout(false);
            this.toolbarCard.PerformLayout();
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
            ((System.ComponentModel.ISupportInitialize)(this.bannerPanel)).EndInit();
            this.bannerPanel.ResumeLayout(false);
            this.bannerPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bottomCard)).EndInit();
            this.bottomCard.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tabBodyPanel)).EndInit();
            this.tabBodyPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridMaterial)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewMaterial)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridBomSpec)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewBomSpec)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridInspection)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewInspection)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridEquipment)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewEquipment)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tabHeaderPanel)).EndInit();
            this.tabHeaderPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chkManualWrench.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.PanelControl toolbarCard;
        private DevExpress.XtraBars.Navigation.TileBar tileProcess;
        private DevExpress.XtraBars.Navigation.TileBarGroup tileGroupProcess;
        private DevExpress.XtraBars.Navigation.TileBarItem tilePrevProcess;
        private DevExpress.XtraBars.Navigation.TileBarItem tileNextProcess;
        private DevExpress.XtraBars.Navigation.TileBarItem tileSkipProcess;
        private DevExpress.XtraBars.Navigation.TileBarItem tileStopProcess;
        private DevExpress.XtraBars.Navigation.TileBarItem tileStartProcess;
        private DevExpress.XtraBars.Navigation.TileBarItem tileProcessList;
        private DevExpress.XtraBars.Navigation.TileBarItem tileAdminAlarm;
        private DevExpress.XtraEditors.LabelControl lblWorkerCaption;
        private DevExpress.XtraEditors.LabelControl lblWorker;
        private DevExpress.XtraEditors.LabelControl lblTimeCaption;
        private DevExpress.XtraEditors.LabelControl lblCurrentTime;
        private System.Windows.Forms.Panel spacerTop;
        private DevExpress.XtraEditors.PanelControl mainPanel;
        private DevExpress.XtraEditors.PanelControl viewerCard;
        private DevExpress.XtraPdfViewer.PdfViewer pdfViewer;
        private System.Windows.Forms.Panel viewerSurface;
        private System.Windows.Forms.Panel spacerMain;
        private DevExpress.XtraEditors.PanelControl infoCard;
        private DevExpress.XtraEditors.LabelControl lblInfoTitle;
        private SEOJIN_TMES.Client.Forms.Common.AccordionHandle btnInfoAccordion;
        private SEOJIN_TMES.Client.Forms.Common.AccordionHandle btnTabsAccordion;
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
        private DevExpress.XtraEditors.PanelControl bannerPanel;
        private DevExpress.XtraEditors.LabelControl lblWorkstation;
        private DevExpress.XtraEditors.LabelControl lblProgressState;
        private System.Windows.Forms.Panel spacerBottom;
        private DevExpress.XtraEditors.PanelControl bottomCard;
        private DevExpress.XtraEditors.PanelControl tabHeaderPanel;
        private SEOJIN_TMES.Client.Forms.Common.MeisButton btnTabMaterial;
        private SEOJIN_TMES.Client.Forms.Common.MeisButton btnTabBomSpec;
        private SEOJIN_TMES.Client.Forms.Common.MeisButton btnTabInspection;
        private SEOJIN_TMES.Client.Forms.Common.MeisButton btnTabEquipment;
        private DevExpress.XtraEditors.CheckEdit chkManualWrench;
        private SEOJIN_TMES.Client.Forms.Common.MeisButton btnSaveResults;
        private DevExpress.XtraEditors.PanelControl tabBodyPanel;
        private DevExpress.XtraGrid.GridControl gridMaterial;
        private SEOJIN_TMES.Client.Forms.Common.MeisGridView gridViewMaterial;
        private DevExpress.XtraGrid.GridControl gridBomSpec;
        private SEOJIN_TMES.Client.Forms.Common.MeisGridView gridViewBomSpec;
        private DevExpress.XtraGrid.GridControl gridInspection;
        private SEOJIN_TMES.Client.Forms.Common.MeisGridView gridViewInspection;
        private DevExpress.XtraGrid.GridControl gridEquipment;
        private SEOJIN_TMES.Client.Forms.Common.MeisGridView gridViewEquipment;
    }
}
