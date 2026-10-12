namespace SEOJIN_TMES.Client.Forms.ProductionStatus
{
    partial class ProductionStatusScreen
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
        /// 색·폰트·아이콘은 ProductionStatusScreen.cs의 ApplyTheme()에서 런타임에 입힌다.
        /// 도킹은 나중에 Add된 컨트롤이 먼저 처리되므로 Add 순서(gridCard → spacer → topCard)를 바꾸지 말 것.
        /// </summary>
        private void InitializeComponent()
        {
            this.topCard = new DevExpress.XtraEditors.PanelControl();
            this.lblFilterTitle = new DevExpress.XtraEditors.LabelControl();
            this.lblDate = new DevExpress.XtraEditors.LabelControl();
            this.dateStandard = new DevExpress.XtraEditors.DateEdit();
            this.btnSearch = new SEOJIN_TMES.Client.Forms.Common.MeisButton();
            this.spacer = new System.Windows.Forms.Panel();
            this.gridCard = new DevExpress.XtraEditors.PanelControl();
            this.grid = new DevExpress.XtraGrid.GridControl();
            this.gridView = new SEOJIN_TMES.Client.Forms.Common.MeisGridView();
            this.lblGridTitle = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.topCard)).BeginInit();
            this.topCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dateStandard.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateStandard.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridCard)).BeginInit();
            this.gridCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView)).BeginInit();
            this.SuspendLayout();
            // 
            // topCard
            // 
            this.topCard.Controls.Add(this.lblFilterTitle);
            this.topCard.Controls.Add(this.lblDate);
            this.topCard.Controls.Add(this.dateStandard);
            this.topCard.Controls.Add(this.btnSearch);
            this.topCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.topCard.Location = new System.Drawing.Point(0, 0);
            this.topCard.Name = "topCard";
            this.topCard.Size = new System.Drawing.Size(1509, 96);
            this.topCard.TabIndex = 0;
            // 
            // lblFilterTitle
            // 
            this.lblFilterTitle.Location = new System.Drawing.Point(20, 18);
            this.lblFilterTitle.Name = "lblFilterTitle";
            this.lblFilterTitle.Size = new System.Drawing.Size(44, 14);
            this.lblFilterTitle.TabIndex = 0;
            this.lblFilterTitle.Text = "조회 조건";
            // 
            // lblDate
            // 
            this.lblDate.Location = new System.Drawing.Point(22, 52);
            this.lblDate.Name = "lblDate";
            this.lblDate.Size = new System.Drawing.Size(40, 14);
            this.lblDate.TabIndex = 1;
            this.lblDate.Text = "기준일자";
            // 
            // dateStandard
            // 
            this.dateStandard.EditValue = null;
            this.dateStandard.Location = new System.Drawing.Point(96, 48);
            this.dateStandard.Name = "dateStandard";
            this.dateStandard.Size = new System.Drawing.Size(150, 20);
            this.dateStandard.TabIndex = 2;
            // 
            // btnSearch
            // 
            this.btnSearch.Accent = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(70)))), ((int)(((byte)(229)))));
            this.btnSearch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSearch.Icon = null;
            this.btnSearch.Location = new System.Drawing.Point(262, 47);
            this.btnSearch.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.PaintStyle = DevExpress.XtraEditors.Controls.PaintStyles.Light;
            this.btnSearch.Selected = false;
            this.btnSearch.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            this.btnSearch.Size = new System.Drawing.Size(104, 28);
            this.btnSearch.TabIndex = 3;
            this.btnSearch.TextAfterIcon = false;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // spacer
            // 
            this.spacer.Dock = System.Windows.Forms.DockStyle.Top;
            this.spacer.Location = new System.Drawing.Point(0, 96);
            this.spacer.Name = "spacer";
            this.spacer.Size = new System.Drawing.Size(1509, 10);
            this.spacer.TabIndex = 1;
            // 
            // gridCard
            // 
            this.gridCard.Controls.Add(this.grid);
            this.gridCard.Controls.Add(this.lblGridTitle);
            this.gridCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridCard.Location = new System.Drawing.Point(0, 106);
            this.gridCard.Name = "gridCard";
            this.gridCard.Padding = new System.Windows.Forms.Padding(1);
            this.gridCard.Size = new System.Drawing.Size(1509, 547);
            this.gridCard.TabIndex = 2;
            // 
            // grid
            // 
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.Location = new System.Drawing.Point(3, 39);
            this.grid.MainView = this.gridView;
            this.grid.Name = "grid";
            this.grid.Size = new System.Drawing.Size(1503, 505);
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
            this.lblGridTitle.Size = new System.Drawing.Size(62, 36);
            this.lblGridTitle.TabIndex = 0;
            this.lblGridTitle.Text = "생산 현황";
            // 
            // ProductionStatusScreen
            // 
            this.Controls.Add(this.gridCard);
            this.Controls.Add(this.spacer);
            this.Controls.Add(this.topCard);
            this.Name = "ProductionStatusScreen";
            this.Size = new System.Drawing.Size(1509, 653);
            ((System.ComponentModel.ISupportInitialize)(this.topCard)).EndInit();
            this.topCard.ResumeLayout(false);
            this.topCard.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dateStandard.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateStandard.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridCard)).EndInit();
            this.gridCard.ResumeLayout(false);
            this.gridCard.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.PanelControl topCard;
        private DevExpress.XtraEditors.LabelControl lblFilterTitle;
        private DevExpress.XtraEditors.LabelControl lblDate;
        private DevExpress.XtraEditors.DateEdit dateStandard;
        private SEOJIN_TMES.Client.Forms.Common.MeisButton btnSearch;
        private System.Windows.Forms.Panel spacer;
        private DevExpress.XtraEditors.PanelControl gridCard;
        private DevExpress.XtraEditors.LabelControl lblGridTitle;
        private DevExpress.XtraGrid.GridControl grid;
        private SEOJIN_TMES.Client.Forms.Common.MeisGridView gridView;
    }
}
