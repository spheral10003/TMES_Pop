namespace SEOJIN_MEIS.Client.Forms.WorkOrderEntry
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
            this.topCard = new DevExpress.XtraEditors.PanelControl();
            this.btnSearch = new SEOJIN_MEIS.Client.Forms.Common.MeisButton();
            this.btnWork = new SEOJIN_MEIS.Client.Forms.Common.MeisButton();
            this.lblWorkOrderNo = new DevExpress.XtraEditors.LabelControl();
            this.txtWorkOrderNo = new DevExpress.XtraEditors.TextEdit();
            this.lblSerialNo = new DevExpress.XtraEditors.LabelControl();
            this.txtSerialNo = new DevExpress.XtraEditors.TextEdit();
            this.spacer = new System.Windows.Forms.Panel();
            this.gridCard = new DevExpress.XtraEditors.PanelControl();
            this.grid = new DevExpress.XtraGrid.GridControl();
            this.gridView = new SEOJIN_MEIS.Client.Forms.Common.MeisGridView();
            this.lblGridTitle = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.topCard)).BeginInit();
            this.topCard.SuspendLayout();
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
            // btnSearch
            // 
            this.btnSearch.Accent = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(70)))), ((int)(((byte)(229)))));
            this.btnSearch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSearch.Icon = null;
            this.btnSearch.Location = new System.Drawing.Point(20, 26);
            this.btnSearch.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.PaintStyle = DevExpress.XtraEditors.Controls.PaintStyles.Light;
            this.btnSearch.Selected = false;
            this.btnSearch.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            this.btnSearch.Size = new System.Drawing.Size(104, 36);
            this.btnSearch.TabIndex = 0;
            this.btnSearch.Text = "조회";
            this.btnSearch.TextAfterIcon = false;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // btnWork
            // 
            this.btnWork.Accent = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(70)))), ((int)(((byte)(229)))));
            this.btnWork.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnWork.Icon = null;
            this.btnWork.Location = new System.Drawing.Point(132, 26);
            this.btnWork.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
            this.btnWork.Name = "btnWork";
            this.btnWork.PaintStyle = DevExpress.XtraEditors.Controls.PaintStyles.Light;
            this.btnWork.Selected = false;
            this.btnWork.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            this.btnWork.Size = new System.Drawing.Size(104, 36);
            this.btnWork.TabIndex = 1;
            this.btnWork.Text = "작업";
            this.btnWork.TextAfterIcon = false;
            this.btnWork.Click += new System.EventHandler(this.btnWork_Click);
            // 
            // lblWorkOrderNo
            // 
            this.lblWorkOrderNo.Location = new System.Drawing.Point(280, 36);
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
            this.lblSerialNo.Text = "생산품목 시리얼번호";
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
        private SEOJIN_MEIS.Client.Forms.Common.MeisButton btnSearch;
        private SEOJIN_MEIS.Client.Forms.Common.MeisButton btnWork;
        private DevExpress.XtraEditors.LabelControl lblWorkOrderNo;
        private DevExpress.XtraEditors.TextEdit txtWorkOrderNo;
        private DevExpress.XtraEditors.LabelControl lblSerialNo;
        private DevExpress.XtraEditors.TextEdit txtSerialNo;
        private System.Windows.Forms.Panel spacer;
        private DevExpress.XtraEditors.PanelControl gridCard;
        private DevExpress.XtraEditors.LabelControl lblGridTitle;
        private DevExpress.XtraGrid.GridControl grid;
        private SEOJIN_MEIS.Client.Forms.Common.MeisGridView gridView;
    }
}
