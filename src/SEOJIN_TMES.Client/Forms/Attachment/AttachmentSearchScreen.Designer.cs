namespace SEOJIN_TMES.Client.Forms.Attachment
{
    partial class AttachmentSearchScreen
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
        /// 그리드만 DevExpress(GridControl + 기본 GridView)이고 나머지는 전부 .NET 기본 컨트롤이다.
        /// 색은 이 화면만 디자인 시안 그대로 여기에 직접 지정한다(AppTheme을 거치지 않음).
        ///
        /// 화면 구성:
        ///   ┌ pnlSearch     : 등록일자(필수) 범위 / 시리얼번호 / 작업지시번호 + [조회]
        ///   └ splitMain
        ///       ├ Panel1    : 목록 — 작업지시 검색 결과(행을 고르면 우측 첨부가 그 지시로 바뀐다)
        ///       └ Panel2    : 상세정보 — [다운로드][삭제][업로드] / 파일 상세목록 그리드
        ///
        /// 도킹은 나중에 Add된 컨트롤이 먼저 처리되므로 Add 순서를 바꾸지 말 것
        /// (root: splitMain → pnlSpacer → pnlSearch,
        ///  Panel1: gridOrders → pnlOrderLine → lblOrderTitle,
        ///  Panel2: gridFiles → pnlFileLine → lblFileTitle → pnlAttach → lblDetailTitle).
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlSearch = new System.Windows.Forms.Panel();
            this.btnSearch = new System.Windows.Forms.Button();
            this.txtWorkOrderNo = new System.Windows.Forms.TextBox();
            this.lblWorkOrderNo = new System.Windows.Forms.Label();
            this.txtSerialNo = new System.Windows.Forms.TextBox();
            this.lblSerialNo = new System.Windows.Forms.Label();
            this.dtpTo = new System.Windows.Forms.DateTimePicker();
            this.lblDateTilde = new System.Windows.Forms.Label();
            this.dtpFrom = new System.Windows.Forms.DateTimePicker();
            this.lblRegDate = new System.Windows.Forms.Label();
            this.pnlSpacer = new System.Windows.Forms.Panel();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.gridOrders = new DevExpress.XtraGrid.GridControl();
            this.gridViewOrders = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colOrderNo = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colWorkOrderNo = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colSerialNo = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colAttachmentCount = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colCreatedDate = new DevExpress.XtraGrid.Columns.GridColumn();
            this.pnlOrderLine = new System.Windows.Forms.Panel();
            this.lblOrderTitle = new System.Windows.Forms.Label();
            this.gridFiles = new DevExpress.XtraGrid.GridControl();
            this.gridViewFiles = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colFileNo = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colFileName = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colFileType = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repoFileTypeText = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.colFileSize = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colCreatedBy = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colCreatedAt = new DevExpress.XtraGrid.Columns.GridColumn();
            this.pnlFileLine = new System.Windows.Forms.Panel();
            this.lblFileTitle = new System.Windows.Forms.Label();
            this.pnlAttach = new System.Windows.Forms.Panel();
            this.btnUpload = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnDownload = new System.Windows.Forms.Button();
            this.lblAttach = new System.Windows.Forms.Label();
            this.lblDetailTitle = new System.Windows.Forms.Label();
            this.pnlSearch.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridOrders)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewOrders)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridFiles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewFiles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repoFileTypeText)).BeginInit();
            this.pnlAttach.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlSearch
            // 
            this.pnlSearch.BackColor = System.Drawing.Color.White;
            this.pnlSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlSearch.Controls.Add(this.btnSearch);
            this.pnlSearch.Controls.Add(this.txtWorkOrderNo);
            this.pnlSearch.Controls.Add(this.lblWorkOrderNo);
            this.pnlSearch.Controls.Add(this.txtSerialNo);
            this.pnlSearch.Controls.Add(this.lblSerialNo);
            this.pnlSearch.Controls.Add(this.dtpTo);
            this.pnlSearch.Controls.Add(this.lblDateTilde);
            this.pnlSearch.Controls.Add(this.dtpFrom);
            this.pnlSearch.Controls.Add(this.lblRegDate);
            this.pnlSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSearch.Location = new System.Drawing.Point(8, 8);
            this.pnlSearch.Name = "pnlSearch";
            this.pnlSearch.Size = new System.Drawing.Size(1424, 46);
            this.pnlSearch.TabIndex = 0;
            // 
            // btnSearch
            // 
            this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(85)))), ((int)(((byte)(120)))));
            this.btnSearch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSearch.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(85)))), ((int)(((byte)(120)))));
            this.btnSearch.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(108)))), ((int)(((byte)(145)))));
            this.btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearch.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.btnSearch.ForeColor = System.Drawing.Color.White;
            this.btnSearch.Location = new System.Drawing.Point(1336, 9);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(76, 26);
            this.btnSearch.TabIndex = 5;
            this.btnSearch.Text = "조회";
            this.btnSearch.UseVisualStyleBackColor = false;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // txtWorkOrderNo
            // 
            this.txtWorkOrderNo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtWorkOrderNo.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.txtWorkOrderNo.Location = new System.Drawing.Point(822, 11);
            this.txtWorkOrderNo.Name = "txtWorkOrderNo";
            this.txtWorkOrderNo.Size = new System.Drawing.Size(195, 23);
            this.txtWorkOrderNo.TabIndex = 4;
            this.txtWorkOrderNo.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Condition_KeyDown);
            // 
            // lblWorkOrderNo
            // 
            this.lblWorkOrderNo.AutoSize = true;
            this.lblWorkOrderNo.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.lblWorkOrderNo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.lblWorkOrderNo.Location = new System.Drawing.Point(732, 15);
            this.lblWorkOrderNo.Name = "lblWorkOrderNo";
            this.lblWorkOrderNo.Size = new System.Drawing.Size(86, 15);
            this.lblWorkOrderNo.TabIndex = 0;
            this.lblWorkOrderNo.Text = "· 작업지시번호";
            // 
            // txtSerialNo
            // 
            this.txtSerialNo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSerialNo.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.txtSerialNo.Location = new System.Drawing.Point(532, 11);
            this.txtSerialNo.Name = "txtSerialNo";
            this.txtSerialNo.Size = new System.Drawing.Size(195, 23);
            this.txtSerialNo.TabIndex = 3;
            this.txtSerialNo.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Condition_KeyDown);
            // 
            // lblSerialNo
            // 
            this.lblSerialNo.AutoSize = true;
            this.lblSerialNo.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.lblSerialNo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.lblSerialNo.Location = new System.Drawing.Point(444, 15);
            this.lblSerialNo.Name = "lblSerialNo";
            this.lblSerialNo.Size = new System.Drawing.Size(74, 15);
            this.lblSerialNo.TabIndex = 0;
            this.lblSerialNo.Text = "· 시리얼번호";
            // 
            // dtpTo
            // 
            this.dtpTo.CustomFormat = "yyyy-MM-dd";
            this.dtpTo.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.dtpTo.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpTo.Location = new System.Drawing.Point(246, 11);
            this.dtpTo.Name = "dtpTo";
            this.dtpTo.Size = new System.Drawing.Size(118, 23);
            this.dtpTo.TabIndex = 2;
            // 
            // lblDateTilde
            // 
            this.lblDateTilde.AutoSize = true;
            this.lblDateTilde.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.lblDateTilde.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.lblDateTilde.Location = new System.Drawing.Point(227, 15);
            this.lblDateTilde.Name = "lblDateTilde";
            this.lblDateTilde.Size = new System.Drawing.Size(15, 15);
            this.lblDateTilde.TabIndex = 0;
            this.lblDateTilde.Text = "~";
            // 
            // dtpFrom
            // 
            this.dtpFrom.CustomFormat = "yyyy-MM-dd";
            this.dtpFrom.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.dtpFrom.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpFrom.Location = new System.Drawing.Point(105, 11);
            this.dtpFrom.Name = "dtpFrom";
            this.dtpFrom.Size = new System.Drawing.Size(118, 23);
            this.dtpFrom.TabIndex = 1;
            // 
            // lblRegDate
            // 
            this.lblRegDate.AutoSize = true;
            this.lblRegDate.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.lblRegDate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.lblRegDate.Location = new System.Drawing.Point(12, 15);
            this.lblRegDate.Name = "lblRegDate";
            this.lblRegDate.Size = new System.Drawing.Size(75, 15);
            this.lblRegDate.TabIndex = 0;
            this.lblRegDate.Text = "· 등록일자(*)";
            // 
            // pnlSpacer
            // 
            this.pnlSpacer.BackColor = System.Drawing.Color.White;
            this.pnlSpacer.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSpacer.Location = new System.Drawing.Point(8, 54);
            this.pnlSpacer.Name = "pnlSpacer";
            this.pnlSpacer.Size = new System.Drawing.Size(1424, 8);
            this.pnlSpacer.TabIndex = 1;
            // 
            // splitMain
            // 
            this.splitMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.Location = new System.Drawing.Point(8, 62);
            this.splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            this.splitMain.Panel1.BackColor = System.Drawing.Color.White;
            this.splitMain.Panel1.Controls.Add(this.gridOrders);
            this.splitMain.Panel1.Controls.Add(this.pnlOrderLine);
            this.splitMain.Panel1.Controls.Add(this.lblOrderTitle);
            this.splitMain.Panel1.Padding = new System.Windows.Forms.Padding(0, 0, 8, 0);
            // 
            // splitMain.Panel2
            // 
            this.splitMain.Panel2.BackColor = System.Drawing.Color.White;
            this.splitMain.Panel2.Controls.Add(this.gridFiles);
            this.splitMain.Panel2.Controls.Add(this.pnlFileLine);
            this.splitMain.Panel2.Controls.Add(this.lblFileTitle);
            this.splitMain.Panel2.Controls.Add(this.pnlAttach);
            this.splitMain.Panel2.Controls.Add(this.lblDetailTitle);
            this.splitMain.Panel2.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.splitMain.Size = new System.Drawing.Size(1424, 450);
            this.splitMain.SplitterDistance = 708;
            this.splitMain.SplitterWidth = 3;
            this.splitMain.TabIndex = 2;
            // 
            // gridOrders
            // 
            this.gridOrders.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridOrders.Location = new System.Drawing.Point(0, 30);
            this.gridOrders.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
            this.gridOrders.LookAndFeel.UseDefaultLookAndFeel = false;
            this.gridOrders.MainView = this.gridViewOrders;
            this.gridOrders.Name = "gridOrders";
            this.gridOrders.Size = new System.Drawing.Size(700, 420);
            this.gridOrders.TabIndex = 2;
            this.gridOrders.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridViewOrders});
            // 
            // gridViewOrders
            // 
            this.gridViewOrders.Appearance.Empty.BackColor = System.Drawing.Color.White;
            this.gridViewOrders.Appearance.Empty.Options.UseBackColor = true;
            this.gridViewOrders.Appearance.FocusedRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.gridViewOrders.Appearance.FocusedRow.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.gridViewOrders.Appearance.FocusedRow.Options.UseBackColor = true;
            this.gridViewOrders.Appearance.FocusedRow.Options.UseForeColor = true;
            this.gridViewOrders.Appearance.HeaderPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(247)))), ((int)(((byte)(247)))));
            this.gridViewOrders.Appearance.HeaderPanel.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.gridViewOrders.Appearance.HeaderPanel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.gridViewOrders.Appearance.HeaderPanel.Options.UseBackColor = true;
            this.gridViewOrders.Appearance.HeaderPanel.Options.UseFont = true;
            this.gridViewOrders.Appearance.HeaderPanel.Options.UseForeColor = true;
            this.gridViewOrders.Appearance.HeaderPanel.Options.UseTextOptions = true;
            this.gridViewOrders.Appearance.HeaderPanel.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.gridViewOrders.Appearance.Row.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.gridViewOrders.Appearance.Row.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.gridViewOrders.Appearance.Row.Options.UseFont = true;
            this.gridViewOrders.Appearance.Row.Options.UseForeColor = true;
            this.gridViewOrders.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colOrderNo,
            this.colWorkOrderNo,
            this.colSerialNo,
            this.colAttachmentCount,
            this.colCreatedDate});
            this.gridViewOrders.GridControl = this.gridOrders;
            this.gridViewOrders.Name = "gridViewOrders";
            this.gridViewOrders.OptionsBehavior.Editable = false;
            this.gridViewOrders.OptionsCustomization.AllowGroup = false;
            this.gridViewOrders.OptionsMenu.EnableColumnMenu = false;
            this.gridViewOrders.OptionsView.ShowGroupPanel = false;
            this.gridViewOrders.OptionsView.ShowIndicator = false;
            this.gridViewOrders.RowHeight = 26;
            this.gridViewOrders.CustomDrawEmptyForeground += new DevExpress.XtraGrid.Views.Base.CustomDrawEventHandler(this.gridView_CustomDrawEmptyForeground);
            this.gridViewOrders.FocusedRowChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(this.gridViewOrders_FocusedRowChanged);
            this.gridViewOrders.CustomColumnDisplayText += new DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventHandler(this.gridView_RowNumberDisplayText);
            // 
            // colOrderNo
            // 
            this.colOrderNo.AppearanceCell.Options.UseTextOptions = true;
            this.colOrderNo.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.colOrderNo.Caption = "No.";
            this.colOrderNo.FieldName = "RowNo";
            this.colOrderNo.Name = "colOrderNo";
            this.colOrderNo.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.False;
            this.colOrderNo.UnboundDataType = typeof(int);
            this.colOrderNo.Visible = true;
            this.colOrderNo.VisibleIndex = 0;
            this.colOrderNo.Width = 50;
            // 
            // colWorkOrderNo
            // 
            this.colWorkOrderNo.Caption = "작업지시번호";
            this.colWorkOrderNo.FieldName = "WorkOrderNo";
            this.colWorkOrderNo.Name = "colWorkOrderNo";
            this.colWorkOrderNo.Visible = true;
            this.colWorkOrderNo.VisibleIndex = 1;
            this.colWorkOrderNo.Width = 170;
            // 
            // colSerialNo
            // 
            this.colSerialNo.Caption = "시리얼번호";
            this.colSerialNo.FieldName = "SerialNo";
            this.colSerialNo.Name = "colSerialNo";
            this.colSerialNo.Visible = true;
            this.colSerialNo.VisibleIndex = 2;
            this.colSerialNo.Width = 170;
            // 
            // colAttachmentCount
            // 
            this.colAttachmentCount.AppearanceCell.Options.UseTextOptions = true;
            this.colAttachmentCount.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.colAttachmentCount.Caption = "첨부파일";
            this.colAttachmentCount.FieldName = "AttachmentCount";
            this.colAttachmentCount.Name = "colAttachmentCount";
            this.colAttachmentCount.Visible = true;
            this.colAttachmentCount.VisibleIndex = 3;
            this.colAttachmentCount.Width = 80;
            // 
            // colCreatedDate
            // 
            this.colCreatedDate.AppearanceCell.Options.UseTextOptions = true;
            this.colCreatedDate.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.colCreatedDate.Caption = "생성일자";
            this.colCreatedDate.FieldName = "CreatedDate";
            this.colCreatedDate.Name = "colCreatedDate";
            this.colCreatedDate.Visible = true;
            this.colCreatedDate.VisibleIndex = 4;
            this.colCreatedDate.Width = 150;
            // 
            // pnlOrderLine
            // 
            this.pnlOrderLine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(85)))), ((int)(((byte)(120)))));
            this.pnlOrderLine.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlOrderLine.Location = new System.Drawing.Point(0, 28);
            this.pnlOrderLine.Name = "pnlOrderLine";
            this.pnlOrderLine.Size = new System.Drawing.Size(700, 2);
            this.pnlOrderLine.TabIndex = 1;
            // 
            // lblOrderTitle
            // 
            this.lblOrderTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblOrderTitle.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.lblOrderTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(34)))));
            this.lblOrderTitle.Location = new System.Drawing.Point(0, 0);
            this.lblOrderTitle.Name = "lblOrderTitle";
            this.lblOrderTitle.Size = new System.Drawing.Size(700, 28);
            this.lblOrderTitle.TabIndex = 0;
            this.lblOrderTitle.Text = "■ 목록";
            this.lblOrderTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // gridFiles
            // 
            this.gridFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridFiles.Location = new System.Drawing.Point(8, 250);
            this.gridFiles.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
            this.gridFiles.LookAndFeel.UseDefaultLookAndFeel = false;
            this.gridFiles.MainView = this.gridViewFiles;
            this.gridFiles.Name = "gridFiles";
            this.gridFiles.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repoFileTypeText});
            this.gridFiles.Size = new System.Drawing.Size(705, 200);
            this.gridFiles.TabIndex = 4;
            this.gridFiles.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridViewFiles});
            // 
            // gridViewFiles
            // 
            this.gridViewFiles.Appearance.Empty.BackColor = System.Drawing.Color.White;
            this.gridViewFiles.Appearance.Empty.Options.UseBackColor = true;
            this.gridViewFiles.Appearance.FocusedRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.gridViewFiles.Appearance.FocusedRow.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.gridViewFiles.Appearance.FocusedRow.Options.UseBackColor = true;
            this.gridViewFiles.Appearance.FocusedRow.Options.UseForeColor = true;
            this.gridViewFiles.Appearance.HeaderPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(247)))), ((int)(((byte)(247)))));
            this.gridViewFiles.Appearance.HeaderPanel.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.gridViewFiles.Appearance.HeaderPanel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.gridViewFiles.Appearance.HeaderPanel.Options.UseBackColor = true;
            this.gridViewFiles.Appearance.HeaderPanel.Options.UseFont = true;
            this.gridViewFiles.Appearance.HeaderPanel.Options.UseForeColor = true;
            this.gridViewFiles.Appearance.HeaderPanel.Options.UseTextOptions = true;
            this.gridViewFiles.Appearance.HeaderPanel.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.gridViewFiles.Appearance.Row.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.gridViewFiles.Appearance.Row.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.gridViewFiles.Appearance.Row.Options.UseFont = true;
            this.gridViewFiles.Appearance.Row.Options.UseForeColor = true;
            this.gridViewFiles.Appearance.SelectedRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.gridViewFiles.Appearance.SelectedRow.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.gridViewFiles.Appearance.SelectedRow.Options.UseBackColor = true;
            this.gridViewFiles.Appearance.SelectedRow.Options.UseForeColor = true;
            this.gridViewFiles.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colFileNo,
            this.colFileName,
            this.colFileType,
            this.colFileSize,
            this.colCreatedBy,
            this.colCreatedAt});
            this.gridViewFiles.GridControl = this.gridFiles;
            this.gridViewFiles.Name = "gridViewFiles";
            this.gridViewFiles.OptionsBehavior.Editable = false;
            this.gridViewFiles.OptionsCustomization.AllowGroup = false;
            this.gridViewFiles.OptionsMenu.EnableColumnMenu = false;
            this.gridViewFiles.OptionsSelection.MultiSelect = true;
            this.gridViewFiles.OptionsView.ShowGroupPanel = false;
            this.gridViewFiles.OptionsView.ShowIndicator = false;
            this.gridViewFiles.RowHeight = 26;
            this.gridViewFiles.SelectionChanged += new DevExpress.Data.SelectionChangedEventHandler(this.gridViewFiles_SelectionChanged);
            this.gridViewFiles.CustomDrawEmptyForeground += new DevExpress.XtraGrid.Views.Base.CustomDrawEventHandler(this.gridView_CustomDrawEmptyForeground);
            this.gridViewFiles.CustomColumnDisplayText += new DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventHandler(this.gridViewFiles_CustomColumnDisplayText);
            // 
            // colFileNo
            // 
            this.colFileNo.AppearanceCell.Options.UseTextOptions = true;
            this.colFileNo.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.colFileNo.Caption = "No.";
            this.colFileNo.FieldName = "RowNo";
            this.colFileNo.Name = "colFileNo";
            this.colFileNo.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.False;
            this.colFileNo.UnboundDataType = typeof(int);
            this.colFileNo.Visible = true;
            this.colFileNo.VisibleIndex = 0;
            this.colFileNo.Width = 45;
            // 
            // colFileName
            // 
            this.colFileName.Caption = "파일명";
            this.colFileName.FieldName = "DisplayName";
            this.colFileName.Name = "colFileName";
            this.colFileName.Visible = true;
            this.colFileName.VisibleIndex = 1;
            this.colFileName.Width = 260;
            // 
            // colFileType
            // 
            this.colFileType.AppearanceCell.Options.UseTextOptions = true;
            this.colFileType.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.colFileType.Caption = "첨부유형";
            this.colFileType.ColumnEdit = this.repoFileTypeText;
            this.colFileType.FieldName = "IsImage";
            this.colFileType.Name = "colFileType";
            this.colFileType.Visible = true;
            this.colFileType.VisibleIndex = 2;
            this.colFileType.Width = 70;
            // 
            // repoFileTypeText
            // 
            this.repoFileTypeText.AutoHeight = false;
            this.repoFileTypeText.Name = "repoFileTypeText";
            // 
            // colFileSize
            // 
            this.colFileSize.AppearanceCell.Options.UseTextOptions = true;
            this.colFileSize.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.colFileSize.Caption = "파일크기";
            this.colFileSize.FieldName = "FileSize";
            this.colFileSize.Name = "colFileSize";
            this.colFileSize.Visible = true;
            this.colFileSize.VisibleIndex = 3;
            this.colFileSize.Width = 80;
            // 
            // colCreatedBy
            // 
            this.colCreatedBy.AppearanceCell.Options.UseTextOptions = true;
            this.colCreatedBy.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.colCreatedBy.Caption = "작성자";
            this.colCreatedBy.FieldName = "CreatedBy";
            this.colCreatedBy.Name = "colCreatedBy";
            this.colCreatedBy.Visible = true;
            this.colCreatedBy.VisibleIndex = 4;
            this.colCreatedBy.Width = 80;
            // 
            // colCreatedAt
            // 
            this.colCreatedAt.AppearanceCell.Options.UseTextOptions = true;
            this.colCreatedAt.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.colCreatedAt.Caption = "작성일시";
            this.colCreatedAt.DisplayFormat.FormatString = "yyyy-MM-dd HH:mm";
            this.colCreatedAt.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.colCreatedAt.FieldName = "CreatedAt";
            this.colCreatedAt.Name = "colCreatedAt";
            this.colCreatedAt.Visible = true;
            this.colCreatedAt.VisibleIndex = 5;
            this.colCreatedAt.Width = 120;
            // 
            // pnlFileLine
            // 
            this.pnlFileLine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(85)))), ((int)(((byte)(120)))));
            this.pnlFileLine.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFileLine.Location = new System.Drawing.Point(8, 248);
            this.pnlFileLine.Name = "pnlFileLine";
            this.pnlFileLine.Size = new System.Drawing.Size(705, 2);
            this.pnlFileLine.TabIndex = 3;
            // 
            // lblFileTitle
            // 
            this.lblFileTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblFileTitle.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.lblFileTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(34)))));
            this.lblFileTitle.Location = new System.Drawing.Point(8, 220);
            this.lblFileTitle.Name = "lblFileTitle";
            this.lblFileTitle.Size = new System.Drawing.Size(705, 28);
            this.lblFileTitle.TabIndex = 2;
            this.lblFileTitle.Text = "■ 파일 상세목록";
            this.lblFileTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlAttach
            // 
            this.pnlAttach.BackColor = System.Drawing.Color.White;
            this.pnlAttach.Controls.Add(this.btnUpload);
            this.pnlAttach.Controls.Add(this.btnDelete);
            this.pnlAttach.Controls.Add(this.btnDownload);
            this.pnlAttach.Controls.Add(this.lblAttach);
            this.pnlAttach.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlAttach.Location = new System.Drawing.Point(8, 28);
            this.pnlAttach.Name = "pnlAttach";
            this.pnlAttach.Size = new System.Drawing.Size(705, 38);
            this.pnlAttach.TabIndex = 1;
            // 
            // btnUpload
            // 
            this.btnUpload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUpload.BackColor = System.Drawing.Color.White;
            this.btnUpload.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnUpload.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(85)))), ((int)(((byte)(120)))));
            this.btnUpload.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(241)))), ((int)(((byte)(248)))));
            this.btnUpload.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUpload.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.btnUpload.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.btnUpload.Location = new System.Drawing.Point(618, 6);
            this.btnUpload.Name = "btnUpload";
            this.btnUpload.Size = new System.Drawing.Size(76, 26);
            this.btnUpload.TabIndex = 2;
            this.btnUpload.Text = "업로드";
            this.btnUpload.UseVisualStyleBackColor = false;
            this.btnUpload.Click += new System.EventHandler(this.btnUpload_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDelete.BackColor = System.Drawing.Color.White;
            this.btnDelete.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDelete.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(85)))), ((int)(((byte)(120)))));
            this.btnDelete.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(241)))), ((int)(((byte)(248)))));
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.btnDelete.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.btnDelete.Location = new System.Drawing.Point(536, 6);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(76, 26);
            this.btnDelete.TabIndex = 1;
            this.btnDelete.Text = "삭제";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnDownload
            // 
            this.btnDownload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDownload.BackColor = System.Drawing.Color.White;
            this.btnDownload.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDownload.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(85)))), ((int)(((byte)(120)))));
            this.btnDownload.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(241)))), ((int)(((byte)(248)))));
            this.btnDownload.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDownload.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.btnDownload.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.btnDownload.Location = new System.Drawing.Point(454, 6);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(76, 26);
            this.btnDownload.TabIndex = 0;
            this.btnDownload.Text = "다운로드";
            this.btnDownload.UseVisualStyleBackColor = false;
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // lblAttach
            // 
            this.lblAttach.AutoSize = true;
            this.lblAttach.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.lblAttach.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.lblAttach.Location = new System.Drawing.Point(20, 11);
            this.lblAttach.Name = "lblAttach";
            this.lblAttach.Size = new System.Drawing.Size(64, 15);
            this.lblAttach.TabIndex = 0;
            this.lblAttach.Text = "• 첨부파일";
            // 
            // lblDetailTitle
            // 
            this.lblDetailTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDetailTitle.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.lblDetailTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(34)))));
            this.lblDetailTitle.Location = new System.Drawing.Point(8, 0);
            this.lblDetailTitle.Name = "lblDetailTitle";
            this.lblDetailTitle.Size = new System.Drawing.Size(705, 28);
            this.lblDetailTitle.TabIndex = 0;
            this.lblDetailTitle.Text = "■ 상세정보";
            this.lblDetailTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // AttachmentSearchScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.splitMain);
            this.Controls.Add(this.pnlSpacer);
            this.Controls.Add(this.pnlSearch);
            this.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.Name = "AttachmentSearchScreen";
            this.Padding = new System.Windows.Forms.Padding(8);
            this.Size = new System.Drawing.Size(1440, 520);
            this.pnlSearch.ResumeLayout(false);
            this.pnlSearch.PerformLayout();
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridOrders)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewOrders)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridFiles)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewFiles)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repoFileTypeText)).EndInit();
            this.pnlAttach.ResumeLayout(false);
            this.pnlAttach.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlSearch;
        private System.Windows.Forms.Label lblRegDate;
        private System.Windows.Forms.DateTimePicker dtpFrom;
        private System.Windows.Forms.Label lblDateTilde;
        private System.Windows.Forms.DateTimePicker dtpTo;
        private System.Windows.Forms.Label lblSerialNo;
        private System.Windows.Forms.TextBox txtSerialNo;
        private System.Windows.Forms.Label lblWorkOrderNo;
        private System.Windows.Forms.TextBox txtWorkOrderNo;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Panel pnlSpacer;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.Label lblOrderTitle;
        private System.Windows.Forms.Panel pnlOrderLine;
        private DevExpress.XtraGrid.GridControl gridOrders;
        private DevExpress.XtraGrid.Views.Grid.GridView gridViewOrders;
        private DevExpress.XtraGrid.Columns.GridColumn colOrderNo;
        private DevExpress.XtraGrid.Columns.GridColumn colWorkOrderNo;
        private DevExpress.XtraGrid.Columns.GridColumn colSerialNo;
        private DevExpress.XtraGrid.Columns.GridColumn colAttachmentCount;
        private DevExpress.XtraGrid.Columns.GridColumn colCreatedDate;
        private System.Windows.Forms.Label lblDetailTitle;
        private System.Windows.Forms.Panel pnlAttach;
        private System.Windows.Forms.Label lblAttach;
        private System.Windows.Forms.Button btnDownload;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnUpload;
        private System.Windows.Forms.Label lblFileTitle;
        private System.Windows.Forms.Panel pnlFileLine;
        private DevExpress.XtraGrid.GridControl gridFiles;
        private DevExpress.XtraGrid.Views.Grid.GridView gridViewFiles;
        private DevExpress.XtraGrid.Columns.GridColumn colFileNo;
        private DevExpress.XtraGrid.Columns.GridColumn colFileName;
        private DevExpress.XtraGrid.Columns.GridColumn colFileType;
        private DevExpress.XtraGrid.Columns.GridColumn colFileSize;
        private DevExpress.XtraGrid.Columns.GridColumn colCreatedBy;
        private DevExpress.XtraGrid.Columns.GridColumn colCreatedAt;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit repoFileTypeText;
    }
}
