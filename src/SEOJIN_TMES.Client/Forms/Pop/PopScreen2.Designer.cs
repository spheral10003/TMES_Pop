namespace SEOJIN_TMES.Client.Forms.Pop
{
    partial class PopScreen2
    {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PopScreen2));
            this.pnlTop = new System.Windows.Forms.Panel();
            this.lblWorker = new System.Windows.Forms.Label();
            this.lblWorkerTile = new System.Windows.Forms.Label();
            this.btnAdminAlarm = new System.Windows.Forms.Button();
            this.btnPrevProcess = new System.Windows.Forms.Button();
            this.btnProcessList = new System.Windows.Forms.Button();
            this.btnNextProcess = new System.Windows.Forms.Button();
            this.btnStartProcess = new System.Windows.Forms.Button();
            this.btnStopProcess = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.btnSaveResults = new System.Windows.Forms.Button();
            this.btnTabsAccordion = new SEOJIN_TMES.Client.Forms.Common.AccordionHandle();
            this.chkManualWrench = new System.Windows.Forms.CheckBox();
            this.tabGrid = new System.Windows.Forms.TabControl();
            this.pageMaterial = new System.Windows.Forms.TabPage();
            this.gridMaterial = new DevExpress.XtraGrid.GridControl();
            this.gridViewMaterial = new SEOJIN_TMES.Client.Forms.Common.MeisGridView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.gridBomSpec = new DevExpress.XtraGrid.GridControl();
            this.gridViewBomSpec = new SEOJIN_TMES.Client.Forms.Common.MeisGridView();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.gridInspection = new DevExpress.XtraGrid.GridControl();
            this.gridViewInspection = new SEOJIN_TMES.Client.Forms.Common.MeisGridView();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.gridEquipment = new DevExpress.XtraGrid.GridControl();
            this.gridViewEquipment = new SEOJIN_TMES.Client.Forms.Common.MeisGridView();
            this.panel2 = new System.Windows.Forms.Panel();
            this.txtPageWorkTime = new System.Windows.Forms.Label();
            this.txtPageIdleTime = new System.Windows.Forms.Label();
            this.txtPageRunTime = new System.Windows.Forms.Label();
            this.txtPageEndTime = new System.Windows.Forms.Label();
            this.txtPageStartTime = new System.Windows.Forms.Label();
            this.txtPageOperation = new System.Windows.Forms.Label();
            this.txtRoutingOperation = new System.Windows.Forms.Label();
            this.txtRoutingGroup = new System.Windows.Forms.Label();
            this.txtProductSerialNo = new System.Windows.Forms.Label();
            this.txtItemCode = new System.Windows.Forms.Label();
            this.txtWorkOrderNo = new System.Windows.Forms.Label();
            this.lblInfoTitle = new System.Windows.Forms.Label();
            this.btnInfoAccordion = new SEOJIN_TMES.Client.Forms.Common.AccordionHandle();
            this.lblPageWorkTime = new System.Windows.Forms.Label();
            this.lblPageIdleTime = new System.Windows.Forms.Label();
            this.lblPageRunTime = new System.Windows.Forms.Label();
            this.lblPageEndTime = new System.Windows.Forms.Label();
            this.lblPageStartTime = new System.Windows.Forms.Label();
            this.lblPageOperation = new System.Windows.Forms.Label();
            this.lblRoutingOperation = new System.Windows.Forms.Label();
            this.lblRoutingGroup = new System.Windows.Forms.Label();
            this.lblProductSerialNo = new System.Windows.Forms.Label();
            this.lblItemCode = new System.Windows.Forms.Label();
            this.lblWorkOrderNo = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.lblWorkstation = new System.Windows.Forms.Label();
            this.lblProgressState = new System.Windows.Forms.Label();
            this.pnlTop.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel4.SuspendLayout();
            this.tabGrid.SuspendLayout();
            this.pageMaterial.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridMaterial)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewMaterial)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridBomSpec)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewBomSpec)).BeginInit();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridInspection)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewInspection)).BeginInit();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridEquipment)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewEquipment)).BeginInit();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlTop
            // 
            this.pnlTop.Controls.Add(this.lblProgressState);
            this.pnlTop.Controls.Add(this.lblWorkstation);
            this.pnlTop.Controls.Add(this.lblWorker);
            this.pnlTop.Controls.Add(this.lblWorkerTile);
            this.pnlTop.Controls.Add(this.btnAdminAlarm);
            this.pnlTop.Controls.Add(this.btnPrevProcess);
            this.pnlTop.Controls.Add(this.btnProcessList);
            this.pnlTop.Controls.Add(this.btnNextProcess);
            this.pnlTop.Controls.Add(this.btnStartProcess);
            this.pnlTop.Controls.Add(this.btnStopProcess);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(1365, 49);
            this.pnlTop.TabIndex = 0;
            // 
            // lblWorker
            // 
            this.lblWorker.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWorker.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblWorker.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWorker.Location = new System.Drawing.Point(1106, 8);
            this.lblWorker.Name = "lblWorker";
            this.lblWorker.Size = new System.Drawing.Size(256, 27);
            this.lblWorker.TabIndex = 23;
            this.lblWorker.Text = "작업자";
            this.lblWorker.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblWorkerTile
            // 
            this.lblWorkerTile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWorkerTile.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWorkerTile.Location = new System.Drawing.Point(1024, 8);
            this.lblWorkerTile.Name = "lblWorkerTile";
            this.lblWorkerTile.Size = new System.Drawing.Size(87, 27);
            this.lblWorkerTile.TabIndex = 22;
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
            this.btnAdminAlarm.Location = new System.Drawing.Point(663, 8);
            this.btnAdminAlarm.Name = "btnAdminAlarm";
            this.btnAdminAlarm.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.btnAdminAlarm.Size = new System.Drawing.Size(124, 31);
            this.btnAdminAlarm.TabIndex = 21;
            this.btnAdminAlarm.Text = "알   림";
            this.btnAdminAlarm.UseVisualStyleBackColor = false;
            // 
            // btnPrevProcess
            // 
            this.btnPrevProcess.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(70)))), ((int)(((byte)(229)))));
            this.btnPrevProcess.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPrevProcess.Font = new System.Drawing.Font("Arial Narrow", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPrevProcess.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.btnPrevProcess.Image = ((System.Drawing.Image)(resources.GetObject("btnPrevProcess.Image")));
            this.btnPrevProcess.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnPrevProcess.Location = new System.Drawing.Point(13, 8);
            this.btnPrevProcess.Name = "btnPrevProcess";
            this.btnPrevProcess.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.btnPrevProcess.Size = new System.Drawing.Size(124, 31);
            this.btnPrevProcess.TabIndex = 12;
            this.btnPrevProcess.Text = "이전 공정";
            this.btnPrevProcess.UseVisualStyleBackColor = false;
            // 
            // btnProcessList
            // 
            this.btnProcessList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(124)))), ((int)(((byte)(58)))), ((int)(((byte)(237)))));
            this.btnProcessList.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnProcessList.Font = new System.Drawing.Font("Arial Narrow", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnProcessList.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.btnProcessList.Image = ((System.Drawing.Image)(resources.GetObject("btnProcessList.Image")));
            this.btnProcessList.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnProcessList.Location = new System.Drawing.Point(533, 8);
            this.btnProcessList.Name = "btnProcessList";
            this.btnProcessList.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.btnProcessList.Size = new System.Drawing.Size(124, 31);
            this.btnProcessList.TabIndex = 20;
            this.btnProcessList.Text = "공정 목록";
            this.btnProcessList.UseVisualStyleBackColor = false;
            // 
            // btnNextProcess
            // 
            this.btnNextProcess.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(70)))), ((int)(((byte)(229)))));
            this.btnNextProcess.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNextProcess.Font = new System.Drawing.Font("Arial Narrow", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNextProcess.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.btnNextProcess.Image = ((System.Drawing.Image)(resources.GetObject("btnNextProcess.Image")));
            this.btnNextProcess.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNextProcess.Location = new System.Drawing.Point(143, 8);
            this.btnNextProcess.Name = "btnNextProcess";
            this.btnNextProcess.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.btnNextProcess.Size = new System.Drawing.Size(124, 31);
            this.btnNextProcess.TabIndex = 17;
            this.btnNextProcess.Text = "다음 공정";
            this.btnNextProcess.UseVisualStyleBackColor = false;
            // 
            // btnStartProcess
            // 
            this.btnStartProcess.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(185)))), ((int)(((byte)(129)))));
            this.btnStartProcess.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartProcess.Font = new System.Drawing.Font("Arial Narrow", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStartProcess.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.btnStartProcess.Image = ((System.Drawing.Image)(resources.GetObject("btnStartProcess.Image")));
            this.btnStartProcess.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnStartProcess.Location = new System.Drawing.Point(403, 8);
            this.btnStartProcess.Name = "btnStartProcess";
            this.btnStartProcess.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.btnStartProcess.Size = new System.Drawing.Size(124, 31);
            this.btnStartProcess.TabIndex = 19;
            this.btnStartProcess.Text = "공정 시작";
            this.btnStartProcess.UseVisualStyleBackColor = false;
            // 
            // btnStopProcess
            // 
            this.btnStopProcess.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(63)))), ((int)(((byte)(94)))));
            this.btnStopProcess.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStopProcess.Font = new System.Drawing.Font("Arial Narrow", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStopProcess.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.btnStopProcess.Image = ((System.Drawing.Image)(resources.GetObject("btnStopProcess.Image")));
            this.btnStopProcess.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnStopProcess.Location = new System.Drawing.Point(273, 8);
            this.btnStopProcess.Name = "btnStopProcess";
            this.btnStopProcess.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.btnStopProcess.Size = new System.Drawing.Size(124, 31);
            this.btnStopProcess.TabIndex = 18;
            this.btnStopProcess.Text = "지공정 중지";
            this.btnStopProcess.UseVisualStyleBackColor = false;
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.panel4);
            this.panel1.Controls.Add(this.tabGrid);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 481);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1365, 175);
            this.panel1.TabIndex = 1;
            // 
            // panel4
            // 
            this.panel4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panel4.Controls.Add(this.btnSaveResults);
            this.panel4.Controls.Add(this.btnTabsAccordion);
            this.panel4.Controls.Add(this.chkManualWrench);
            this.panel4.Location = new System.Drawing.Point(1019, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(344, 29);
            this.panel4.TabIndex = 1;
            // 
            // btnSaveResults
            // 
            this.btnSaveResults.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.btnSaveResults.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveResults.Font = new System.Drawing.Font("Arial Narrow", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSaveResults.ForeColor = System.Drawing.Color.Black;
            this.btnSaveResults.Image = ((System.Drawing.Image)(resources.GetObject("btnSaveResults.Image")));
            this.btnSaveResults.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSaveResults.Location = new System.Drawing.Point(132, 3);
            this.btnSaveResults.Name = "btnSaveResults";
            this.btnSaveResults.Padding = new System.Windows.Forms.Padding(2, 0, 0, 0);
            this.btnSaveResults.Size = new System.Drawing.Size(124, 23);
            this.btnSaveResults.TabIndex = 26;
            this.btnSaveResults.Text = "결과 저장";
            this.btnSaveResults.UseVisualStyleBackColor = false;
            // 
            // btnTabsAccordion
            // 
            this.btnTabsAccordion.Accent = System.Drawing.Color.FromArgb(((int)(((byte)(124)))), ((int)(((byte)(58)))), ((int)(((byte)(237)))));
            this.btnTabsAccordion.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTabsAccordion.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnTabsAccordion.Edge = SEOJIN_TMES.Client.Forms.Common.AccordionEdge.Left;
            this.btnTabsAccordion.Expanded = true;
            this.btnTabsAccordion.Font = new System.Drawing.Font("맑은 고딕", 8.5F);
            this.btnTabsAccordion.Location = new System.Drawing.Point(273, 0);
            this.btnTabsAccordion.Name = "btnTabsAccordion";
            this.btnTabsAccordion.Size = new System.Drawing.Size(71, 29);
            this.btnTabsAccordion.TabIndex = 25;
            // 
            // chkManualWrench
            // 
            this.chkManualWrench.AutoSize = true;
            this.chkManualWrench.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkManualWrench.Location = new System.Drawing.Point(30, 5);
            this.chkManualWrench.Name = "chkManualWrench";
            this.chkManualWrench.Size = new System.Drawing.Size(74, 20);
            this.chkManualWrench.TabIndex = 0;
            this.chkManualWrench.Text = "수동 렌치";
            this.chkManualWrench.UseVisualStyleBackColor = true;
            // 
            // tabGrid
            // 
            this.tabGrid.Controls.Add(this.pageMaterial);
            this.tabGrid.Controls.Add(this.tabPage2);
            this.tabGrid.Controls.Add(this.tabPage1);
            this.tabGrid.Controls.Add(this.tabPage3);
            this.tabGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabGrid.ItemSize = new System.Drawing.Size(96, 25);
            this.tabGrid.Location = new System.Drawing.Point(0, 0);
            this.tabGrid.Name = "tabGrid";
            this.tabGrid.SelectedIndex = 0;
            this.tabGrid.Size = new System.Drawing.Size(1363, 173);
            this.tabGrid.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabGrid.TabIndex = 0;
            // 
            // pageMaterial
            // 
            this.pageMaterial.Controls.Add(this.gridMaterial);
            this.pageMaterial.Location = new System.Drawing.Point(4, 29);
            this.pageMaterial.Name = "pageMaterial";
            this.pageMaterial.Padding = new System.Windows.Forms.Padding(3);
            this.pageMaterial.Size = new System.Drawing.Size(1355, 140);
            this.pageMaterial.TabIndex = 0;
            this.pageMaterial.Text = "Material";
            this.pageMaterial.UseVisualStyleBackColor = true;
            // 
            // gridMaterial
            // 
            this.gridMaterial.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridMaterial.Location = new System.Drawing.Point(3, 3);
            this.gridMaterial.MainView = this.gridViewMaterial;
            this.gridMaterial.Name = "gridMaterial";
            this.gridMaterial.Size = new System.Drawing.Size(1349, 134);
            this.gridMaterial.TabIndex = 1;
            this.gridMaterial.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridViewMaterial});
            // 
            // gridViewMaterial
            // 
            this.gridViewMaterial.Accent = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(70)))), ((int)(((byte)(229)))));
            this.gridViewMaterial.GridControl = this.gridMaterial;
            this.gridViewMaterial.Name = "gridViewMaterial";
            this.gridViewMaterial.OptionsBehavior.Editable = false;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.gridBomSpec);
            this.tabPage2.Location = new System.Drawing.Point(4, 29);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1357, 142);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // gridBomSpec
            // 
            this.gridBomSpec.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridBomSpec.Location = new System.Drawing.Point(3, 3);
            this.gridBomSpec.MainView = this.gridViewBomSpec;
            this.gridBomSpec.Name = "gridBomSpec";
            this.gridBomSpec.Size = new System.Drawing.Size(1351, 136);
            this.gridBomSpec.TabIndex = 2;
            this.gridBomSpec.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridViewBomSpec});
            // 
            // gridViewBomSpec
            // 
            this.gridViewBomSpec.Accent = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(70)))), ((int)(((byte)(229)))));
            this.gridViewBomSpec.GridControl = this.gridBomSpec;
            this.gridViewBomSpec.Name = "gridViewBomSpec";
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.gridInspection);
            this.tabPage1.Location = new System.Drawing.Point(4, 29);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1357, 142);
            this.tabPage1.TabIndex = 2;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // gridInspection
            // 
            this.gridInspection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridInspection.Location = new System.Drawing.Point(3, 3);
            this.gridInspection.MainView = this.gridViewInspection;
            this.gridInspection.Name = "gridInspection";
            this.gridInspection.Size = new System.Drawing.Size(1351, 136);
            this.gridInspection.TabIndex = 3;
            this.gridInspection.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridViewInspection});
            // 
            // gridViewInspection
            // 
            this.gridViewInspection.Accent = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(70)))), ((int)(((byte)(229)))));
            this.gridViewInspection.GridControl = this.gridInspection;
            this.gridViewInspection.Name = "gridViewInspection";
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.gridEquipment);
            this.tabPage3.Location = new System.Drawing.Point(4, 29);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(1357, 142);
            this.tabPage3.TabIndex = 3;
            this.tabPage3.Text = "tabPage3";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // gridEquipment
            // 
            this.gridEquipment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridEquipment.Location = new System.Drawing.Point(3, 3);
            this.gridEquipment.MainView = this.gridViewEquipment;
            this.gridEquipment.Name = "gridEquipment";
            this.gridEquipment.Size = new System.Drawing.Size(1351, 136);
            this.gridEquipment.TabIndex = 4;
            this.gridEquipment.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridViewEquipment});
            // 
            // gridViewEquipment
            // 
            this.gridViewEquipment.Accent = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(70)))), ((int)(((byte)(229)))));
            this.gridViewEquipment.GridControl = this.gridEquipment;
            this.gridViewEquipment.Name = "gridViewEquipment";
            this.gridViewEquipment.OptionsBehavior.Editable = false;
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.txtPageWorkTime);
            this.panel2.Controls.Add(this.txtPageIdleTime);
            this.panel2.Controls.Add(this.txtPageRunTime);
            this.panel2.Controls.Add(this.txtPageEndTime);
            this.panel2.Controls.Add(this.txtPageStartTime);
            this.panel2.Controls.Add(this.txtPageOperation);
            this.panel2.Controls.Add(this.txtRoutingOperation);
            this.panel2.Controls.Add(this.txtRoutingGroup);
            this.panel2.Controls.Add(this.txtProductSerialNo);
            this.panel2.Controls.Add(this.txtItemCode);
            this.panel2.Controls.Add(this.txtWorkOrderNo);
            this.panel2.Controls.Add(this.lblInfoTitle);
            this.panel2.Controls.Add(this.btnInfoAccordion);
            this.panel2.Controls.Add(this.lblPageWorkTime);
            this.panel2.Controls.Add(this.lblPageIdleTime);
            this.panel2.Controls.Add(this.lblPageRunTime);
            this.panel2.Controls.Add(this.lblPageEndTime);
            this.panel2.Controls.Add(this.lblPageStartTime);
            this.panel2.Controls.Add(this.lblPageOperation);
            this.panel2.Controls.Add(this.lblRoutingOperation);
            this.panel2.Controls.Add(this.lblRoutingGroup);
            this.panel2.Controls.Add(this.lblProductSerialNo);
            this.panel2.Controls.Add(this.lblItemCode);
            this.panel2.Controls.Add(this.lblWorkOrderNo);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(1021, 49);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(344, 432);
            this.panel2.TabIndex = 2;
            // 
            // txtPageWorkTime
            // 
            this.txtPageWorkTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPageWorkTime.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPageWorkTime.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPageWorkTime.Location = new System.Drawing.Point(179, 325);
            this.txtPageWorkTime.Name = "txtPageWorkTime";
            this.txtPageWorkTime.Size = new System.Drawing.Size(144, 21);
            this.txtPageWorkTime.TabIndex = 57;
            this.txtPageWorkTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtPageIdleTime
            // 
            this.txtPageIdleTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPageIdleTime.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPageIdleTime.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPageIdleTime.Location = new System.Drawing.Point(179, 298);
            this.txtPageIdleTime.Name = "txtPageIdleTime";
            this.txtPageIdleTime.Size = new System.Drawing.Size(144, 21);
            this.txtPageIdleTime.TabIndex = 56;
            this.txtPageIdleTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtPageRunTime
            // 
            this.txtPageRunTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPageRunTime.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPageRunTime.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPageRunTime.Location = new System.Drawing.Point(179, 271);
            this.txtPageRunTime.Name = "txtPageRunTime";
            this.txtPageRunTime.Size = new System.Drawing.Size(144, 21);
            this.txtPageRunTime.TabIndex = 55;
            this.txtPageRunTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtPageEndTime
            // 
            this.txtPageEndTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPageEndTime.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPageEndTime.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPageEndTime.Location = new System.Drawing.Point(179, 244);
            this.txtPageEndTime.Name = "txtPageEndTime";
            this.txtPageEndTime.Size = new System.Drawing.Size(144, 21);
            this.txtPageEndTime.TabIndex = 54;
            this.txtPageEndTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtPageStartTime
            // 
            this.txtPageStartTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPageStartTime.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPageStartTime.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPageStartTime.Location = new System.Drawing.Point(179, 217);
            this.txtPageStartTime.Name = "txtPageStartTime";
            this.txtPageStartTime.Size = new System.Drawing.Size(144, 21);
            this.txtPageStartTime.TabIndex = 53;
            this.txtPageStartTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtPageOperation
            // 
            this.txtPageOperation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPageOperation.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPageOperation.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPageOperation.Location = new System.Drawing.Point(179, 190);
            this.txtPageOperation.Name = "txtPageOperation";
            this.txtPageOperation.Size = new System.Drawing.Size(144, 21);
            this.txtPageOperation.TabIndex = 52;
            this.txtPageOperation.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtRoutingOperation
            // 
            this.txtRoutingOperation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRoutingOperation.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtRoutingOperation.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtRoutingOperation.Location = new System.Drawing.Point(179, 163);
            this.txtRoutingOperation.Name = "txtRoutingOperation";
            this.txtRoutingOperation.Size = new System.Drawing.Size(144, 21);
            this.txtRoutingOperation.TabIndex = 51;
            this.txtRoutingOperation.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtRoutingGroup
            // 
            this.txtRoutingGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRoutingGroup.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtRoutingGroup.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtRoutingGroup.Location = new System.Drawing.Point(179, 136);
            this.txtRoutingGroup.Name = "txtRoutingGroup";
            this.txtRoutingGroup.Size = new System.Drawing.Size(144, 21);
            this.txtRoutingGroup.TabIndex = 50;
            this.txtRoutingGroup.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtProductSerialNo
            // 
            this.txtProductSerialNo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtProductSerialNo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtProductSerialNo.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtProductSerialNo.Location = new System.Drawing.Point(179, 109);
            this.txtProductSerialNo.Name = "txtProductSerialNo";
            this.txtProductSerialNo.Size = new System.Drawing.Size(144, 21);
            this.txtProductSerialNo.TabIndex = 49;
            this.txtProductSerialNo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtItemCode
            // 
            this.txtItemCode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtItemCode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtItemCode.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtItemCode.Location = new System.Drawing.Point(179, 82);
            this.txtItemCode.Name = "txtItemCode";
            this.txtItemCode.Size = new System.Drawing.Size(144, 21);
            this.txtItemCode.TabIndex = 48;
            this.txtItemCode.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtWorkOrderNo
            // 
            this.txtWorkOrderNo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtWorkOrderNo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtWorkOrderNo.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtWorkOrderNo.Location = new System.Drawing.Point(179, 52);
            this.txtWorkOrderNo.Name = "txtWorkOrderNo";
            this.txtWorkOrderNo.Size = new System.Drawing.Size(144, 21);
            this.txtWorkOrderNo.TabIndex = 47;
            this.txtWorkOrderNo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblInfoTitle
            // 
            this.lblInfoTitle.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInfoTitle.Location = new System.Drawing.Point(36, 10);
            this.lblInfoTitle.Name = "lblInfoTitle";
            this.lblInfoTitle.Size = new System.Drawing.Size(101, 21);
            this.lblInfoTitle.TabIndex = 46;
            this.lblInfoTitle.Text = "진행 중 공정 정보";
            this.lblInfoTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnInfoAccordion
            // 
            this.btnInfoAccordion.Accent = System.Drawing.Color.FromArgb(((int)(((byte)(124)))), ((int)(((byte)(58)))), ((int)(((byte)(237)))));
            this.btnInfoAccordion.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnInfoAccordion.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnInfoAccordion.Edge = SEOJIN_TMES.Client.Forms.Common.AccordionEdge.Left;
            this.btnInfoAccordion.Expanded = true;
            this.btnInfoAccordion.Font = new System.Drawing.Font("맑은 고딕", 8.5F);
            this.btnInfoAccordion.Location = new System.Drawing.Point(0, 0);
            this.btnInfoAccordion.Name = "btnInfoAccordion";
            this.btnInfoAccordion.Size = new System.Drawing.Size(26, 430);
            this.btnInfoAccordion.TabIndex = 45;
            // 
            // lblPageWorkTime
            // 
            this.lblPageWorkTime.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPageWorkTime.Location = new System.Drawing.Point(41, 325);
            this.lblPageWorkTime.Name = "lblPageWorkTime";
            this.lblPageWorkTime.Size = new System.Drawing.Size(101, 21);
            this.lblPageWorkTime.TabIndex = 43;
            this.lblPageWorkTime.Text = "· 페이지 공정 작업시간";
            this.lblPageWorkTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblPageIdleTime
            // 
            this.lblPageIdleTime.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPageIdleTime.Location = new System.Drawing.Point(41, 298);
            this.lblPageIdleTime.Name = "lblPageIdleTime";
            this.lblPageIdleTime.Size = new System.Drawing.Size(101, 21);
            this.lblPageIdleTime.TabIndex = 41;
            this.lblPageIdleTime.Text = "· 페이지 공정 비가동시간";
            this.lblPageIdleTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblPageRunTime
            // 
            this.lblPageRunTime.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPageRunTime.Location = new System.Drawing.Point(41, 271);
            this.lblPageRunTime.Name = "lblPageRunTime";
            this.lblPageRunTime.Size = new System.Drawing.Size(101, 21);
            this.lblPageRunTime.TabIndex = 39;
            this.lblPageRunTime.Text = "· 페이지 공정 가동시간";
            this.lblPageRunTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblPageEndTime
            // 
            this.lblPageEndTime.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPageEndTime.Location = new System.Drawing.Point(41, 244);
            this.lblPageEndTime.Name = "lblPageEndTime";
            this.lblPageEndTime.Size = new System.Drawing.Size(101, 21);
            this.lblPageEndTime.TabIndex = 37;
            this.lblPageEndTime.Text = "· 페이지 공정 종료시각";
            this.lblPageEndTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblPageStartTime
            // 
            this.lblPageStartTime.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPageStartTime.Location = new System.Drawing.Point(41, 217);
            this.lblPageStartTime.Name = "lblPageStartTime";
            this.lblPageStartTime.Size = new System.Drawing.Size(101, 21);
            this.lblPageStartTime.TabIndex = 35;
            this.lblPageStartTime.Text = "· 페이지 공정 시작시각";
            this.lblPageStartTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblPageOperation
            // 
            this.lblPageOperation.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPageOperation.Location = new System.Drawing.Point(41, 190);
            this.lblPageOperation.Name = "lblPageOperation";
            this.lblPageOperation.Size = new System.Drawing.Size(101, 21);
            this.lblPageOperation.TabIndex = 33;
            this.lblPageOperation.Text = "· 페이지 공정";
            this.lblPageOperation.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblRoutingOperation
            // 
            this.lblRoutingOperation.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRoutingOperation.Location = new System.Drawing.Point(41, 163);
            this.lblRoutingOperation.Name = "lblRoutingOperation";
            this.lblRoutingOperation.Size = new System.Drawing.Size(101, 21);
            this.lblRoutingOperation.TabIndex = 31;
            this.lblRoutingOperation.Text = "· 라우팅 공정";
            this.lblRoutingOperation.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblRoutingGroup
            // 
            this.lblRoutingGroup.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRoutingGroup.Location = new System.Drawing.Point(41, 136);
            this.lblRoutingGroup.Name = "lblRoutingGroup";
            this.lblRoutingGroup.Size = new System.Drawing.Size(101, 21);
            this.lblRoutingGroup.TabIndex = 29;
            this.lblRoutingGroup.Text = "· 라우팅 그룹";
            this.lblRoutingGroup.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblProductSerialNo
            // 
            this.lblProductSerialNo.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProductSerialNo.Location = new System.Drawing.Point(41, 109);
            this.lblProductSerialNo.Name = "lblProductSerialNo";
            this.lblProductSerialNo.Size = new System.Drawing.Size(101, 21);
            this.lblProductSerialNo.TabIndex = 27;
            this.lblProductSerialNo.Text = "· 생산품목 시리얼번호";
            this.lblProductSerialNo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblItemCode
            // 
            this.lblItemCode.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblItemCode.Location = new System.Drawing.Point(41, 82);
            this.lblItemCode.Name = "lblItemCode";
            this.lblItemCode.Size = new System.Drawing.Size(101, 21);
            this.lblItemCode.TabIndex = 25;
            this.lblItemCode.Text = "· 생산품목코드";
            this.lblItemCode.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblWorkOrderNo
            // 
            this.lblWorkOrderNo.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWorkOrderNo.Location = new System.Drawing.Point(41, 55);
            this.lblWorkOrderNo.Name = "lblWorkOrderNo";
            this.lblWorkOrderNo.Size = new System.Drawing.Size(101, 21);
            this.lblWorkOrderNo.TabIndex = 23;
            this.lblWorkOrderNo.Text = "· 작업지시번호";
            this.lblWorkOrderNo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel3
            // 
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 49);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(1021, 432);
            this.panel3.TabIndex = 3;
            // 
            // lblWorkstation
            // 
            this.lblWorkstation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWorkstation.BackColor = System.Drawing.Color.White;
            this.lblWorkstation.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblWorkstation.Font = new System.Drawing.Font("Arial", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWorkstation.Location = new System.Drawing.Point(827, 8);
            this.lblWorkstation.Name = "lblWorkstation";
            this.lblWorkstation.Size = new System.Drawing.Size(205, 31);
            this.lblWorkstation.TabIndex = 24;
            this.lblWorkstation.Text = "Workstation";
            this.lblWorkstation.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblProgressState
            // 
            this.lblProgressState.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblProgressState.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(124)))), ((int)(((byte)(58)))), ((int)(((byte)(237)))));
            this.lblProgressState.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblProgressState.Font = new System.Drawing.Font("Arial", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProgressState.ForeColor = System.Drawing.Color.White;
            this.lblProgressState.Location = new System.Drawing.Point(615, 7);
            this.lblProgressState.Name = "lblProgressState";
            this.lblProgressState.Size = new System.Drawing.Size(205, 31);
            this.lblProgressState.TabIndex = 25;
            this.lblProgressState.Text = "공정 상태";
            this.lblProgressState.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // PopScreen2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.pnlTop);
            this.Name = "PopScreen2";
            this.Size = new System.Drawing.Size(1365, 656);
            this.Load += new System.EventHandler(this.PopScreen2_Load);
            this.pnlTop.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.tabGrid.ResumeLayout(false);
            this.pageMaterial.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridMaterial)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewMaterial)).EndInit();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridBomSpec)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewBomSpec)).EndInit();
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridInspection)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewInspection)).EndInit();
            this.tabPage3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridEquipment)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewEquipment)).EndInit();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Button btnPrevProcess;
        private System.Windows.Forms.Button btnAdminAlarm;
        private System.Windows.Forms.Button btnProcessList;
        private System.Windows.Forms.Button btnNextProcess;
        private System.Windows.Forms.Button btnStartProcess;
        private System.Windows.Forms.Button btnStopProcess;
        private System.Windows.Forms.Label lblWorker;
        private System.Windows.Forms.Label lblWorkerTile;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label lblWorkOrderNo;
        private System.Windows.Forms.TabControl tabGrid;
        private System.Windows.Forms.TabPage pageMaterial;
        private System.Windows.Forms.TabPage tabPage2;
        private DevExpress.XtraGrid.GridControl gridMaterial;
        private Common.MeisGridView gridViewMaterial;
        private DevExpress.XtraGrid.GridControl gridBomSpec;
        private Common.MeisGridView gridViewBomSpec;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage3;
        private DevExpress.XtraGrid.GridControl gridInspection;
        private Common.MeisGridView gridViewInspection;
        private DevExpress.XtraGrid.GridControl gridEquipment;
        private Common.MeisGridView gridViewEquipment;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label lblPageWorkTime;
        private System.Windows.Forms.Label lblPageIdleTime;
        private System.Windows.Forms.Label lblPageRunTime;
        private System.Windows.Forms.Label lblPageEndTime;
        private System.Windows.Forms.Label lblPageStartTime;
        private System.Windows.Forms.Label lblPageOperation;
        private System.Windows.Forms.Label lblRoutingOperation;
        private System.Windows.Forms.Label lblRoutingGroup;
        private System.Windows.Forms.Label lblProductSerialNo;
        private System.Windows.Forms.Label lblItemCode;
        private Common.AccordionHandle btnInfoAccordion;
        private System.Windows.Forms.Label lblInfoTitle;
        private System.Windows.Forms.Label txtPageWorkTime;
        private System.Windows.Forms.Label txtPageIdleTime;
        private System.Windows.Forms.Label txtPageRunTime;
        private System.Windows.Forms.Label txtPageEndTime;
        private System.Windows.Forms.Label txtPageStartTime;
        private System.Windows.Forms.Label txtPageOperation;
        private System.Windows.Forms.Label txtRoutingOperation;
        private System.Windows.Forms.Label txtRoutingGroup;
        private System.Windows.Forms.Label txtProductSerialNo;
        private System.Windows.Forms.Label txtItemCode;
        private System.Windows.Forms.Label txtWorkOrderNo;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.CheckBox chkManualWrench;
        private Common.AccordionHandle btnTabsAccordion;
        private System.Windows.Forms.Button btnSaveResults;
        private System.Windows.Forms.Label lblWorkstation;
        private System.Windows.Forms.Label lblProgressState;
    }
}
