namespace SEOJIN_TMES.Client.Forms.WorkOrderEntry
{
    partial class WorkOrderEntryScreen
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
        /// 색·폰트·아이콘·문구는 WorkOrderEntryScreen.cs의 Initialize()/ApplyTheme()에서 런타임에 넣는다.
        ///
        /// 구성: 상단 카드([조회][작업] + 조회 조건 2개) + 아래 카드(작업 지시 정보 그리드).
        /// 도킹은 나중에 Add된 컨트롤이 먼저 처리되므로 Add 순서(gridCard → spacer → topCard)를 바꾸지 말 것.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WorkOrderEntryScreen));
            this.topCard = new DevExpress.XtraEditors.PanelControl();
            this.chkWorkStationEnable = new System.Windows.Forms.CheckBox();
            this.btnItemPick = new System.Windows.Forms.Button();
            this.txtItemName = new DevExpress.XtraEditors.TextEdit();
            this.lblItemCode = new DevExpress.XtraEditors.LabelControl();
            this.txtItemCode = new DevExpress.XtraEditors.TextEdit();
            this.btnSearch = new System.Windows.Forms.Button();
            this.btnWork = new System.Windows.Forms.Button();
            this.lblWorkOrderNo = new DevExpress.XtraEditors.LabelControl();
            this.txtWorkOrderNo = new DevExpress.XtraEditors.TextEdit();
            this.lblSerialNo = new DevExpress.XtraEditors.LabelControl();
            this.txtSerialNo = new DevExpress.XtraEditors.TextEdit();
            this.spacer = new System.Windows.Forms.Panel();
            this.gridCard = new DevExpress.XtraEditors.PanelControl();
            this.grid = new DevExpress.XtraGrid.GridControl();
            this.gridView = new SEOJIN_TMES.Client.Forms.Common.MeisGridView();
            this.lblGridTitle = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.topCard)).BeginInit();
            this.topCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtItemName.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtItemCode.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtWorkOrderNo.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSerialNo.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridCard)).BeginInit();
            this.gridCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView)).BeginInit();
            this.SuspendLayout();
            // 
            // topCard
            // 
            this.topCard.Controls.Add(this.chkWorkStationEnable);
            this.topCard.Controls.Add(this.btnItemPick);
            this.topCard.Controls.Add(this.txtItemName);
            this.topCard.Controls.Add(this.lblItemCode);
            this.topCard.Controls.Add(this.txtItemCode);
            this.topCard.Controls.Add(this.btnSearch);
            this.topCard.Controls.Add(this.btnWork);
            this.topCard.Controls.Add(this.lblWorkOrderNo);
            this.topCard.Controls.Add(this.txtWorkOrderNo);
            this.topCard.Controls.Add(this.lblSerialNo);
            this.topCard.Controls.Add(this.txtSerialNo);
            this.topCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.topCard.Location = new System.Drawing.Point(0, 0);
            this.topCard.Name = "topCard";
            this.topCard.Size = new System.Drawing.Size(1509, 88);
            this.topCard.TabIndex = 0;
            // 
            // chkWorkStationEnable
            // 
            this.chkWorkStationEnable.AutoSize = true;
            this.chkWorkStationEnable.Checked = true;
            this.chkWorkStationEnable.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkWorkStationEnable.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkWorkStationEnable.Location = new System.Drawing.Point(1413, 35);
            this.chkWorkStationEnable.Name = "chkWorkStationEnable";
            this.chkWorkStationEnable.Size = new System.Drawing.Size(91, 18);
            this.chkWorkStationEnable.TabIndex = 11;
            this.chkWorkStationEnable.Text = "WorkStation";
            this.chkWorkStationEnable.UseVisualStyleBackColor = true;
            // 
            // btnItemPick
            // 
            this.btnItemPick.BackColor = System.Drawing.Color.Transparent;
            this.btnItemPick.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnItemPick.BackgroundImage")));
            this.btnItemPick.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnItemPick.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.btnItemPick.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnItemPick.Location = new System.Drawing.Point(1374, 32);
            this.btnItemPick.Margin = new System.Windows.Forms.Padding(1);
            this.btnItemPick.Name = "btnItemPick";
            this.btnItemPick.Size = new System.Drawing.Size(23, 20);
            this.btnItemPick.TabIndex = 10;
            this.btnItemPick.UseVisualStyleBackColor = false;
            this.btnItemPick.Click += new System.EventHandler(this.btnItemPick_Click);
            // 
            // txtItemName
            // 
            this.txtItemName.Location = new System.Drawing.Point(1272, 33);
            this.txtItemName.Name = "txtItemName";
            this.txtItemName.Size = new System.Drawing.Size(98, 20);
            this.txtItemName.TabIndex = 9;
            // 
            // lblItemCode
            // 
            this.lblItemCode.Location = new System.Drawing.Point(1034, 36);
            this.lblItemCode.Name = "lblItemCode";
            this.lblItemCode.Size = new System.Drawing.Size(75, 14);
            this.lblItemCode.TabIndex = 6;
            this.lblItemCode.Text = "품목코드/품목명";
            // 
            // txtItemCode
            // 
            this.txtItemCode.Location = new System.Drawing.Point(1168, 33);
            this.txtItemCode.Name = "txtItemCode";
            this.txtItemCode.Size = new System.Drawing.Size(98, 20);
            this.txtItemCode.TabIndex = 7;
            // 
            // btnSearch
            // 
            this.btnSearch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSearch.Location = new System.Drawing.Point(16, 25);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(104, 36);
            this.btnSearch.TabIndex = 0;
            this.btnSearch.Text = "조회";
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // btnWork
            // 
            this.btnWork.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnWork.Location = new System.Drawing.Point(126, 25);
            this.btnWork.Name = "btnWork";
            this.btnWork.Size = new System.Drawing.Size(104, 36);
            this.btnWork.TabIndex = 1;
            this.btnWork.Text = "작업";
            this.btnWork.Click += new System.EventHandler(this.btnWork_Click);
            // 
            // lblWorkOrderNo
            // 
            this.lblWorkOrderNo.Location = new System.Drawing.Point(282, 36);
            this.lblWorkOrderNo.Name = "lblWorkOrderNo";
            this.lblWorkOrderNo.Size = new System.Drawing.Size(60, 14);
            this.lblWorkOrderNo.TabIndex = 2;
            this.lblWorkOrderNo.Text = "작업지시번호";
            // 
            // txtWorkOrderNo
            // 
            this.txtWorkOrderNo.Location = new System.Drawing.Point(400, 32);
            this.txtWorkOrderNo.Name = "txtWorkOrderNo";
            this.txtWorkOrderNo.Size = new System.Drawing.Size(200, 20);
            this.txtWorkOrderNo.TabIndex = 3;
            // 
            // lblSerialNo
            // 
            this.lblSerialNo.Location = new System.Drawing.Point(636, 36);
            this.lblSerialNo.Name = "lblSerialNo";
            this.lblSerialNo.Size = new System.Drawing.Size(94, 14);
            this.lblSerialNo.TabIndex = 4;
            this.lblSerialNo.Text = "작업지시 시리얼번호";
            // 
            // txtSerialNo
            // 
            this.txtSerialNo.Location = new System.Drawing.Point(796, 32);
            this.txtSerialNo.Name = "txtSerialNo";
            this.txtSerialNo.Size = new System.Drawing.Size(200, 20);
            this.txtSerialNo.TabIndex = 5;
            // 
            // spacer
            // 
            this.spacer.Dock = System.Windows.Forms.DockStyle.Top;
            this.spacer.Location = new System.Drawing.Point(0, 88);
            this.spacer.Name = "spacer";
            this.spacer.Size = new System.Drawing.Size(1509, 10);
            this.spacer.TabIndex = 1;
            // 
            // gridCard
            // 
            this.gridCard.Controls.Add(this.grid);
            this.gridCard.Controls.Add(this.lblGridTitle);
            this.gridCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridCard.Location = new System.Drawing.Point(0, 98);
            this.gridCard.Name = "gridCard";
            this.gridCard.Padding = new System.Windows.Forms.Padding(1);
            this.gridCard.Size = new System.Drawing.Size(1509, 555);
            this.gridCard.TabIndex = 2;
            // 
            // grid
            // 
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.Location = new System.Drawing.Point(3, 39);
            this.grid.MainView = this.gridView;
            this.grid.Name = "grid";
            this.grid.Size = new System.Drawing.Size(1503, 513);
            this.grid.TabIndex = 1;
            this.grid.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView});
            // 
            // gridView
            // 
            this.gridView.Accent = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(70)))), ((int)(((byte)(229)))));
            this.gridView.GridControl = this.grid;
            this.gridView.Name = "gridView";
            this.gridView.OptionsBehavior.Editable = false;
            // 
            // lblGridTitle
            // 
            this.lblGridTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblGridTitle.Location = new System.Drawing.Point(3, 3);
            this.lblGridTitle.Name = "lblGridTitle";
            this.lblGridTitle.Padding = new System.Windows.Forms.Padding(18, 12, 0, 10);
            this.lblGridTitle.Size = new System.Drawing.Size(86, 36);
            this.lblGridTitle.TabIndex = 0;
            this.lblGridTitle.Text = "작업 지시 정보";
            // 
            // WorkOrderEntryScreen
            // 
            this.Controls.Add(this.gridCard);
            this.Controls.Add(this.spacer);
            this.Controls.Add(this.topCard);
            this.Name = "WorkOrderEntryScreen";
            this.Size = new System.Drawing.Size(1509, 653);
            ((System.ComponentModel.ISupportInitialize)(this.topCard)).EndInit();
            this.topCard.ResumeLayout(false);
            this.topCard.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtItemName.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtItemCode.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtWorkOrderNo.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSerialNo.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridCard)).EndInit();
            this.gridCard.ResumeLayout(false);
            this.gridCard.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.PanelControl topCard;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnWork;
        private DevExpress.XtraEditors.LabelControl lblWorkOrderNo;
        private DevExpress.XtraEditors.TextEdit txtWorkOrderNo;
        private DevExpress.XtraEditors.LabelControl lblSerialNo;
        private DevExpress.XtraEditors.TextEdit txtSerialNo;
        private System.Windows.Forms.Panel spacer;
        private DevExpress.XtraEditors.PanelControl gridCard;
        private DevExpress.XtraEditors.LabelControl lblGridTitle;
        private DevExpress.XtraGrid.GridControl grid;
        private SEOJIN_TMES.Client.Forms.Common.MeisGridView gridView;
        private DevExpress.XtraEditors.LabelControl lblItemCode;
        private DevExpress.XtraEditors.TextEdit txtItemCode;
        private DevExpress.XtraEditors.TextEdit txtItemName;
        private System.Windows.Forms.Button btnItemPick;
        private System.Windows.Forms.CheckBox chkWorkStationEnable;
    }
}
