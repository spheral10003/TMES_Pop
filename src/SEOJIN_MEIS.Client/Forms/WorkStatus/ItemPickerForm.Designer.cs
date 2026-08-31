namespace SEOJIN_MEIS.Client.Forms.WorkStatus
{
    partial class ItemPickerForm
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
        /// 색·폰트·아이콘·문구는 ItemPickerForm.cs의 Initialize()에서 런타임에 넣는다.
        ///
        /// 구성: 상단 색 띠 + 검색어 카드 + 품목 목록 그리드 + 하단 [닫기][선택].
        /// 도킹 순서 주의: bodyPanel(Fill) → footerPanel(Bottom) → headerPanel(Top),
        /// bodyPanel 안에서는 listCard(Fill) → spacer(Top) → searchCard(Top).
        /// </summary>
        private void InitializeComponent()
        {
            this.headerPanel = new DevExpress.XtraEditors.PanelControl();
            this.headerIcon = new DevExpress.XtraEditors.SvgImageBox();
            this.lblTitle = new DevExpress.XtraEditors.LabelControl();
            this.bodyPanel = new DevExpress.XtraEditors.PanelControl();
            this.listCard = new DevExpress.XtraEditors.PanelControl();
            this.grid = new DevExpress.XtraGrid.GridControl();
            this.gridView = new SEOJIN_MEIS.Client.Forms.Common.MeisGridView();
            this.lblListTitle = new DevExpress.XtraEditors.LabelControl();
            this.spacer = new System.Windows.Forms.Panel();
            this.searchCard = new DevExpress.XtraEditors.PanelControl();
            this.lblKeyword = new DevExpress.XtraEditors.LabelControl();
            this.txtKeyword = new DevExpress.XtraEditors.TextEdit();
            this.btnSearch = new DevExpress.XtraEditors.SimpleButton();
            this.footerPanel = new DevExpress.XtraEditors.PanelControl();
            this.btnSelect = new DevExpress.XtraEditors.SimpleButton();
            this.btnClose = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.headerPanel)).BeginInit();
            this.headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.headerIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bodyPanel)).BeginInit();
            this.bodyPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.listCard)).BeginInit();
            this.listCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.searchCard)).BeginInit();
            this.searchCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtKeyword.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.footerPanel)).BeginInit();
            this.footerPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // headerPanel
            //
            this.headerPanel.Controls.Add(this.headerIcon);
            this.headerPanel.Controls.Add(this.lblTitle);
            this.headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.headerPanel.Location = new System.Drawing.Point(0, 0);
            this.headerPanel.Name = "headerPanel";
            this.headerPanel.Size = new System.Drawing.Size(620, 56);
            this.headerPanel.TabIndex = 0;
            //
            // headerIcon
            //
            this.headerIcon.Location = new System.Drawing.Point(22, 15);
            this.headerIcon.Name = "headerIcon";
            this.headerIcon.Size = new System.Drawing.Size(26, 26);
            this.headerIcon.TabIndex = 0;
            //
            // lblTitle
            //
            this.lblTitle.Location = new System.Drawing.Point(58, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(120, 14);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "생산품목 선택";
            //
            // bodyPanel
            //
            this.bodyPanel.Controls.Add(this.listCard);
            this.bodyPanel.Controls.Add(this.spacer);
            this.bodyPanel.Controls.Add(this.searchCard);
            this.bodyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bodyPanel.Location = new System.Drawing.Point(0, 56);
            this.bodyPanel.Name = "bodyPanel";
            this.bodyPanel.Padding = new System.Windows.Forms.Padding(16, 12, 16, 8);
            this.bodyPanel.Size = new System.Drawing.Size(620, 380);
            this.bodyPanel.TabIndex = 1;
            //
            // searchCard
            //
            this.searchCard.Controls.Add(this.lblKeyword);
            this.searchCard.Controls.Add(this.txtKeyword);
            this.searchCard.Controls.Add(this.btnSearch);
            this.searchCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.searchCard.Location = new System.Drawing.Point(16, 12);
            this.searchCard.Name = "searchCard";
            this.searchCard.Padding = new System.Windows.Forms.Padding(1);
            this.searchCard.Size = new System.Drawing.Size(588, 62);
            this.searchCard.TabIndex = 0;
            //
            // lblKeyword
            //
            this.lblKeyword.Location = new System.Drawing.Point(18, 22);
            this.lblKeyword.Name = "lblKeyword";
            this.lblKeyword.Size = new System.Drawing.Size(130, 20);
            this.lblKeyword.TabIndex = 0;
            this.lblKeyword.Text = "· 품목코드 / 품목명";
            //
            // txtKeyword
            //
            this.txtKeyword.Location = new System.Drawing.Point(156, 18);
            this.txtKeyword.Name = "txtKeyword";
            this.txtKeyword.Size = new System.Drawing.Size(300, 26);
            this.txtKeyword.TabIndex = 1;
            //
            // btnSearch
            //
            this.btnSearch.Location = new System.Drawing.Point(466, 17);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(104, 28);
            this.btnSearch.TabIndex = 2;
            this.btnSearch.Text = "조회";
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            //
            // spacer
            //
            this.spacer.Dock = System.Windows.Forms.DockStyle.Top;
            this.spacer.Location = new System.Drawing.Point(16, 74);
            this.spacer.Name = "spacer";
            this.spacer.Size = new System.Drawing.Size(588, 10);
            this.spacer.TabIndex = 1;
            //
            // listCard
            //
            this.listCard.Controls.Add(this.grid);
            this.listCard.Controls.Add(this.lblListTitle);
            this.listCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listCard.Location = new System.Drawing.Point(16, 84);
            this.listCard.Name = "listCard";
            this.listCard.Padding = new System.Windows.Forms.Padding(1);
            this.listCard.Size = new System.Drawing.Size(588, 288);
            this.listCard.TabIndex = 2;
            //
            // grid
            //
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.Location = new System.Drawing.Point(1, 33);
            this.grid.MainView = this.gridView;
            this.grid.Name = "grid";
            this.grid.Size = new System.Drawing.Size(586, 254);
            this.grid.TabIndex = 1;
            this.grid.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView});
            //
            // gridView
            //
            this.gridView.GridControl = this.grid;
            this.gridView.Name = "gridView";
            this.gridView.OptionsBehavior.Editable = false;
            //
            // lblListTitle
            //
            this.lblListTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblListTitle.Location = new System.Drawing.Point(1, 1);
            this.lblListTitle.Name = "lblListTitle";
            this.lblListTitle.Padding = new System.Windows.Forms.Padding(16, 10, 0, 8);
            this.lblListTitle.Size = new System.Drawing.Size(586, 32);
            this.lblListTitle.TabIndex = 0;
            this.lblListTitle.Text = "품목 목록";
            //
            // footerPanel
            //
            this.footerPanel.Controls.Add(this.btnSelect);
            this.footerPanel.Controls.Add(this.btnClose);
            this.footerPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.footerPanel.Location = new System.Drawing.Point(0, 436);
            this.footerPanel.Name = "footerPanel";
            this.footerPanel.Size = new System.Drawing.Size(620, 64);
            this.footerPanel.TabIndex = 2;
            //
            // btnSelect
            //
            this.btnSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelect.Location = new System.Drawing.Point(508, 16);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(96, 32);
            this.btnSelect.TabIndex = 0;
            this.btnSelect.Text = "선택";
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            //
            // btnClose
            //
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(404, 16);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(96, 32);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "닫기";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            //
            // ItemPickerForm
            //
            this.Appearance.Options.UseFont = true;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(620, 500);
            this.Controls.Add(this.bodyPanel);
            this.Controls.Add(this.footerPanel);
            this.Controls.Add(this.headerPanel);
            this.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.IconOptions.ShowIcon = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ItemPickerForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "생산품목 선택";
            ((System.ComponentModel.ISupportInitialize)(this.footerPanel)).EndInit();
            this.footerPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtKeyword.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.searchCard)).EndInit();
            this.searchCard.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.listCard)).EndInit();
            this.listCard.ResumeLayout(false);
            this.listCard.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bodyPanel)).EndInit();
            this.bodyPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.headerIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.headerPanel)).EndInit();
            this.headerPanel.ResumeLayout(false);
            this.headerPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.PanelControl headerPanel;
        private DevExpress.XtraEditors.SvgImageBox headerIcon;
        private DevExpress.XtraEditors.LabelControl lblTitle;
        private DevExpress.XtraEditors.PanelControl bodyPanel;
        private DevExpress.XtraEditors.PanelControl searchCard;
        private DevExpress.XtraEditors.LabelControl lblKeyword;
        private DevExpress.XtraEditors.TextEdit txtKeyword;
        private DevExpress.XtraEditors.SimpleButton btnSearch;
        private System.Windows.Forms.Panel spacer;
        private DevExpress.XtraEditors.PanelControl listCard;
        private DevExpress.XtraEditors.LabelControl lblListTitle;
        private DevExpress.XtraGrid.GridControl grid;
        private SEOJIN_MEIS.Client.Forms.Common.MeisGridView gridView;
        private DevExpress.XtraEditors.PanelControl footerPanel;
        private DevExpress.XtraEditors.SimpleButton btnSelect;
        private DevExpress.XtraEditors.SimpleButton btnClose;
    }
}
