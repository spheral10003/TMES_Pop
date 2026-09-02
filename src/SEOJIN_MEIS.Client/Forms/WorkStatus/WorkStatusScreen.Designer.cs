namespace SEOJIN_MEIS.Client.Forms.WorkStatus
{
    partial class WorkStatusScreen
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
        /// 색·폰트·아이콘·다국어 문구는 WorkStatusScreen.cs의 Initialize()/ApplyTheme()에서 런타임에 입힌다.
        ///
        /// 화면 구성(화면 설계서 기준):
        ///   ┌ topCard       : [조회] + 조회 조건 6개(지시일자 범위 / 작업지시번호 / 생산품목 / 시리얼번호 / 작업지시 상태)
        ///   ├ orderCard     : 작업 지시 정보 목록(행을 고르면 아래 진행 현황이 그 지시로 바뀐다)
        ///   └ progressCard  : 작업 진행 현황 — 필터(진행 상태 / 스팩 결과 상태) + 공정·스팩별 목록
        ///
        /// 도킹은 나중에 Add된 컨트롤이 먼저 처리되므로 Add 순서를 바꾸지 말 것
        /// (root: orderCard → spacerBottom → progressCard → spacerTop → topCard,
        ///  progressCard: gridProgress → filterPanel → lblProgressTitle).
        /// </summary>
        private void InitializeComponent()
        {
            this.topCard = new DevExpress.XtraEditors.PanelControl();
            this.btnSearch = new SEOJIN_MEIS.Client.Forms.Common.MeisButton();
            this.lblOrderDate = new DevExpress.XtraEditors.LabelControl();
            this.dateFrom = new DevExpress.XtraEditors.DateEdit();
            this.lblDateTilde = new DevExpress.XtraEditors.LabelControl();
            this.dateTo = new DevExpress.XtraEditors.DateEdit();
            this.lblWorkOrderNo = new DevExpress.XtraEditors.LabelControl();
            this.txtWorkOrderNo = new DevExpress.XtraEditors.TextEdit();
            this.lblItem = new DevExpress.XtraEditors.LabelControl();
            this.txtItemCode = new DevExpress.XtraEditors.TextEdit();
            this.txtItemName = new DevExpress.XtraEditors.TextEdit();
            this.btnItemPick = new DevExpress.XtraEditors.SimpleButton();
            this.lblSerialNo = new DevExpress.XtraEditors.LabelControl();
            this.txtSerialNo = new DevExpress.XtraEditors.TextEdit();
            this.lblStatus = new DevExpress.XtraEditors.LabelControl();
            this.cboStatus = new DevExpress.XtraEditors.ComboBoxEdit();
            this.spacerTop = new System.Windows.Forms.Panel();
            this.orderCard = new DevExpress.XtraEditors.PanelControl();
            this.gridOrders = new DevExpress.XtraGrid.GridControl();
            this.gridViewOrders = new SEOJIN_MEIS.Client.Forms.Common.MeisGridView();
            this.lblOrderTitle = new DevExpress.XtraEditors.LabelControl();
            this.spacerBottom = new System.Windows.Forms.Panel();
            this.progressCard = new DevExpress.XtraEditors.PanelControl();
            this.gridProgress = new DevExpress.XtraGrid.GridControl();
            this.gridViewProgress = new SEOJIN_MEIS.Client.Forms.Common.MeisGridView();
            this.filterPanel = new DevExpress.XtraEditors.PanelControl();
            this.lblProgressState = new DevExpress.XtraEditors.LabelControl();
            this.cboProgressState = new DevExpress.XtraEditors.ComboBoxEdit();
            this.lblSpecResultState = new DevExpress.XtraEditors.LabelControl();
            this.cboSpecResultState = new DevExpress.XtraEditors.ComboBoxEdit();
            this.lblProgressTitle = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.topCard)).BeginInit();
            this.topCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dateFrom.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateFrom.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateTo.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateTo.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtWorkOrderNo.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtItemCode.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtItemName.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSerialNo.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboStatus.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.orderCard)).BeginInit();
            this.orderCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridOrders)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewOrders)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressCard)).BeginInit();
            this.progressCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridProgress)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewProgress)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.filterPanel)).BeginInit();
            this.filterPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboProgressState.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboSpecResultState.Properties)).BeginInit();
            this.SuspendLayout();
            //
            // topCard
            //
            this.topCard.Controls.Add(this.btnSearch);
            this.topCard.Controls.Add(this.lblOrderDate);
            this.topCard.Controls.Add(this.dateFrom);
            this.topCard.Controls.Add(this.lblDateTilde);
            this.topCard.Controls.Add(this.dateTo);
            this.topCard.Controls.Add(this.lblWorkOrderNo);
            this.topCard.Controls.Add(this.txtWorkOrderNo);
            this.topCard.Controls.Add(this.lblItem);
            this.topCard.Controls.Add(this.txtItemCode);
            this.topCard.Controls.Add(this.txtItemName);
            this.topCard.Controls.Add(this.btnItemPick);
            this.topCard.Controls.Add(this.lblSerialNo);
            this.topCard.Controls.Add(this.txtSerialNo);
            this.topCard.Controls.Add(this.lblStatus);
            this.topCard.Controls.Add(this.cboStatus);
            this.topCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.topCard.Location = new System.Drawing.Point(0, 0);
            this.topCard.Name = "topCard";
            this.topCard.Size = new System.Drawing.Size(1509, 104);
            this.topCard.TabIndex = 0;
            //
            // btnSearch
            //
            this.btnSearch.Location = new System.Drawing.Point(16, 20);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(100, 64);
            this.btnSearch.TabIndex = 0;
            this.btnSearch.Text = "조회";
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            //
            // lblOrderDate
            //
            this.lblOrderDate.Location = new System.Drawing.Point(140, 24);
            this.lblOrderDate.Name = "lblOrderDate";
            this.lblOrderDate.Size = new System.Drawing.Size(96, 20);
            this.lblOrderDate.TabIndex = 1;
            this.lblOrderDate.Text = "· 지시일자(*)";
            //
            // dateFrom
            //
            this.dateFrom.EditValue = null;
            this.dateFrom.Location = new System.Drawing.Point(244, 20);
            this.dateFrom.Name = "dateFrom";
            this.dateFrom.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dateFrom.Size = new System.Drawing.Size(126, 26);
            this.dateFrom.TabIndex = 2;
            //
            // lblDateTilde
            //
            this.lblDateTilde.Location = new System.Drawing.Point(376, 24);
            this.lblDateTilde.Name = "lblDateTilde";
            this.lblDateTilde.Size = new System.Drawing.Size(12, 20);
            this.lblDateTilde.TabIndex = 3;
            this.lblDateTilde.Text = "~";
            //
            // dateTo
            //
            this.dateTo.EditValue = null;
            this.dateTo.Location = new System.Drawing.Point(394, 20);
            this.dateTo.Name = "dateTo";
            this.dateTo.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dateTo.Size = new System.Drawing.Size(126, 26);
            this.dateTo.TabIndex = 4;
            //
            // lblWorkOrderNo
            //
            this.lblWorkOrderNo.Location = new System.Drawing.Point(556, 24);
            this.lblWorkOrderNo.Name = "lblWorkOrderNo";
            this.lblWorkOrderNo.Size = new System.Drawing.Size(96, 20);
            this.lblWorkOrderNo.TabIndex = 5;
            this.lblWorkOrderNo.Text = "· 작업지시번호";
            //
            // txtWorkOrderNo
            //
            this.txtWorkOrderNo.Location = new System.Drawing.Point(660, 20);
            this.txtWorkOrderNo.Name = "txtWorkOrderNo";
            this.txtWorkOrderNo.Size = new System.Drawing.Size(170, 26);
            this.txtWorkOrderNo.TabIndex = 6;
            //
            // lblItem
            //
            this.lblItem.Location = new System.Drawing.Point(866, 24);
            this.lblItem.Name = "lblItem";
            this.lblItem.Size = new System.Drawing.Size(72, 20);
            this.lblItem.TabIndex = 7;
            this.lblItem.Text = "· 생산품목";
            //
            // txtItemCode
            //
            this.txtItemCode.Location = new System.Drawing.Point(946, 20);
            this.txtItemCode.Name = "txtItemCode";
            this.txtItemCode.Size = new System.Drawing.Size(110, 26);
            this.txtItemCode.TabIndex = 8;
            //
            // txtItemName
            //
            this.txtItemName.Location = new System.Drawing.Point(1062, 20);
            this.txtItemName.Name = "txtItemName";
            this.txtItemName.Properties.ReadOnly = true;
            this.txtItemName.Size = new System.Drawing.Size(140, 26);
            this.txtItemName.TabIndex = 9;
            //
            // btnItemPick
            //
            this.btnItemPick.Location = new System.Drawing.Point(1206, 19);
            this.btnItemPick.Name = "btnItemPick";
            this.btnItemPick.Size = new System.Drawing.Size(34, 28);
            this.btnItemPick.TabIndex = 10;
            this.btnItemPick.Click += new System.EventHandler(this.btnItemPick_Click);
            //
            // lblSerialNo
            //
            this.lblSerialNo.Location = new System.Drawing.Point(140, 62);
            this.lblSerialNo.Name = "lblSerialNo";
            this.lblSerialNo.Size = new System.Drawing.Size(140, 20);
            this.lblSerialNo.TabIndex = 11;
            this.lblSerialNo.Text = "· 생산품목 시리얼번호";
            //
            // txtSerialNo
            //
            this.txtSerialNo.Location = new System.Drawing.Point(288, 58);
            this.txtSerialNo.Name = "txtSerialNo";
            this.txtSerialNo.Size = new System.Drawing.Size(232, 26);
            this.txtSerialNo.TabIndex = 12;
            //
            // lblStatus
            //
            this.lblStatus.Location = new System.Drawing.Point(556, 62);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(96, 20);
            this.lblStatus.TabIndex = 13;
            this.lblStatus.Text = "· 작업지시 상태";
            //
            // cboStatus
            //
            this.cboStatus.Location = new System.Drawing.Point(660, 58);
            this.cboStatus.Name = "cboStatus";
            this.cboStatus.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboStatus.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cboStatus.Size = new System.Drawing.Size(170, 26);
            this.cboStatus.TabIndex = 14;
            //
            // spacerTop
            //
            this.spacerTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.spacerTop.Location = new System.Drawing.Point(0, 104);
            this.spacerTop.Name = "spacerTop";
            this.spacerTop.Size = new System.Drawing.Size(1509, 8);
            this.spacerTop.TabIndex = 1;
            //
            // orderCard
            //
            this.orderCard.Controls.Add(this.gridOrders);
            this.orderCard.Controls.Add(this.lblOrderTitle);
            this.orderCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.orderCard.Location = new System.Drawing.Point(0, 112);
            this.orderCard.Name = "orderCard";
            this.orderCard.Padding = new System.Windows.Forms.Padding(1);
            this.orderCard.Size = new System.Drawing.Size(1509, 292);
            this.orderCard.TabIndex = 2;
            //
            // lblOrderTitle
            //
            this.lblOrderTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblOrderTitle.Location = new System.Drawing.Point(1, 1);
            this.lblOrderTitle.Name = "lblOrderTitle";
            this.lblOrderTitle.Padding = new System.Windows.Forms.Padding(18, 12, 0, 10);
            this.lblOrderTitle.Size = new System.Drawing.Size(1507, 38);
            this.lblOrderTitle.TabIndex = 0;
            this.lblOrderTitle.Text = "작업 지시 정보";
            //
            // gridOrders
            //
            this.gridOrders.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridOrders.Location = new System.Drawing.Point(1, 39);
            this.gridOrders.MainView = this.gridViewOrders;
            this.gridOrders.Name = "gridOrders";
            this.gridOrders.Size = new System.Drawing.Size(1507, 252);
            this.gridOrders.TabIndex = 1;
            this.gridOrders.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridViewOrders});
            //
            // gridViewOrders
            //
            this.gridViewOrders.GridControl = this.gridOrders;
            this.gridViewOrders.Name = "gridViewOrders";
            this.gridViewOrders.OptionsBehavior.Editable = false;
            //
            // spacerBottom
            //
            this.spacerBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.spacerBottom.Location = new System.Drawing.Point(0, 404);
            this.spacerBottom.Name = "spacerBottom";
            this.spacerBottom.Size = new System.Drawing.Size(1509, 8);
            this.spacerBottom.TabIndex = 3;
            //
            // progressCard
            //
            this.progressCard.Controls.Add(this.gridProgress);
            this.progressCard.Controls.Add(this.filterPanel);
            this.progressCard.Controls.Add(this.lblProgressTitle);
            this.progressCard.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressCard.Location = new System.Drawing.Point(0, 412);
            this.progressCard.Name = "progressCard";
            this.progressCard.Padding = new System.Windows.Forms.Padding(1);
            this.progressCard.Size = new System.Drawing.Size(1509, 330);
            this.progressCard.TabIndex = 4;
            //
            // lblProgressTitle
            //
            this.lblProgressTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblProgressTitle.Location = new System.Drawing.Point(1, 1);
            this.lblProgressTitle.Name = "lblProgressTitle";
            this.lblProgressTitle.Padding = new System.Windows.Forms.Padding(18, 12, 0, 10);
            this.lblProgressTitle.Size = new System.Drawing.Size(1507, 38);
            this.lblProgressTitle.TabIndex = 0;
            this.lblProgressTitle.Text = "작업 진행 현황";
            //
            // filterPanel
            //
            this.filterPanel.Controls.Add(this.lblProgressState);
            this.filterPanel.Controls.Add(this.cboProgressState);
            this.filterPanel.Controls.Add(this.lblSpecResultState);
            this.filterPanel.Controls.Add(this.cboSpecResultState);
            this.filterPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.filterPanel.Location = new System.Drawing.Point(1, 39);
            this.filterPanel.Name = "filterPanel";
            this.filterPanel.Size = new System.Drawing.Size(1507, 50);
            this.filterPanel.TabIndex = 1;
            //
            // lblProgressState
            //
            this.lblProgressState.Location = new System.Drawing.Point(18, 15);
            this.lblProgressState.Name = "lblProgressState";
            this.lblProgressState.Size = new System.Drawing.Size(80, 20);
            this.lblProgressState.TabIndex = 0;
            this.lblProgressState.Text = "· 진행 상태";
            //
            // cboProgressState
            //
            this.cboProgressState.Location = new System.Drawing.Point(106, 11);
            this.cboProgressState.Name = "cboProgressState";
            this.cboProgressState.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboProgressState.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cboProgressState.Size = new System.Drawing.Size(150, 26);
            this.cboProgressState.TabIndex = 1;
            this.cboProgressState.SelectedIndexChanged += new System.EventHandler(this.Filter_SelectedIndexChanged);
            //
            // lblSpecResultState
            //
            this.lblSpecResultState.Location = new System.Drawing.Point(292, 15);
            this.lblSpecResultState.Name = "lblSpecResultState";
            this.lblSpecResultState.Size = new System.Drawing.Size(110, 20);
            this.lblSpecResultState.TabIndex = 2;
            this.lblSpecResultState.Text = "· 스팩 결과 상태";
            //
            // cboSpecResultState
            //
            this.cboSpecResultState.Location = new System.Drawing.Point(410, 11);
            this.cboSpecResultState.Name = "cboSpecResultState";
            this.cboSpecResultState.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboSpecResultState.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cboSpecResultState.Size = new System.Drawing.Size(150, 26);
            this.cboSpecResultState.TabIndex = 3;
            this.cboSpecResultState.SelectedIndexChanged += new System.EventHandler(this.Filter_SelectedIndexChanged);
            //
            // gridProgress
            //
            this.gridProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridProgress.Location = new System.Drawing.Point(1, 89);
            this.gridProgress.MainView = this.gridViewProgress;
            this.gridProgress.Name = "gridProgress";
            this.gridProgress.Size = new System.Drawing.Size(1507, 240);
            this.gridProgress.TabIndex = 2;
            this.gridProgress.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridViewProgress});
            //
            // gridViewProgress
            //
            this.gridViewProgress.GridControl = this.gridProgress;
            this.gridViewProgress.Name = "gridViewProgress";
            this.gridViewProgress.OptionsBehavior.Editable = false;
            //
            // WorkStatusScreen
            //
            this.Controls.Add(this.orderCard);
            this.Controls.Add(this.spacerBottom);
            this.Controls.Add(this.progressCard);
            this.Controls.Add(this.spacerTop);
            this.Controls.Add(this.topCard);
            this.Name = "WorkStatusScreen";
            this.Size = new System.Drawing.Size(1509, 742);
            ((System.ComponentModel.ISupportInitialize)(this.cboSpecResultState.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboProgressState.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.filterPanel)).EndInit();
            this.filterPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridViewProgress)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridProgress)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressCard)).EndInit();
            this.progressCard.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridViewOrders)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridOrders)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.orderCard)).EndInit();
            this.orderCard.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cboStatus.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSerialNo.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtItemName.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtItemCode.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtWorkOrderNo.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateTo.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateTo.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateFrom.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateFrom.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.topCard)).EndInit();
            this.topCard.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.PanelControl topCard;
        private SEOJIN_MEIS.Client.Forms.Common.MeisButton btnSearch;
        private DevExpress.XtraEditors.LabelControl lblOrderDate;
        private DevExpress.XtraEditors.DateEdit dateFrom;
        private DevExpress.XtraEditors.LabelControl lblDateTilde;
        private DevExpress.XtraEditors.DateEdit dateTo;
        private DevExpress.XtraEditors.LabelControl lblWorkOrderNo;
        private DevExpress.XtraEditors.TextEdit txtWorkOrderNo;
        private DevExpress.XtraEditors.LabelControl lblItem;
        private DevExpress.XtraEditors.TextEdit txtItemCode;
        private DevExpress.XtraEditors.TextEdit txtItemName;
        private DevExpress.XtraEditors.SimpleButton btnItemPick;
        private DevExpress.XtraEditors.LabelControl lblSerialNo;
        private DevExpress.XtraEditors.TextEdit txtSerialNo;
        private DevExpress.XtraEditors.LabelControl lblStatus;
        private DevExpress.XtraEditors.ComboBoxEdit cboStatus;
        private System.Windows.Forms.Panel spacerTop;
        private DevExpress.XtraEditors.PanelControl orderCard;
        private DevExpress.XtraEditors.LabelControl lblOrderTitle;
        private DevExpress.XtraGrid.GridControl gridOrders;
        private SEOJIN_MEIS.Client.Forms.Common.MeisGridView gridViewOrders;
        private System.Windows.Forms.Panel spacerBottom;
        private DevExpress.XtraEditors.PanelControl progressCard;
        private DevExpress.XtraEditors.LabelControl lblProgressTitle;
        private DevExpress.XtraEditors.PanelControl filterPanel;
        private DevExpress.XtraEditors.LabelControl lblProgressState;
        private DevExpress.XtraEditors.ComboBoxEdit cboProgressState;
        private DevExpress.XtraEditors.LabelControl lblSpecResultState;
        private DevExpress.XtraEditors.ComboBoxEdit cboSpecResultState;
        private DevExpress.XtraGrid.GridControl gridProgress;
        private SEOJIN_MEIS.Client.Forms.Common.MeisGridView gridViewProgress;
    }
}
