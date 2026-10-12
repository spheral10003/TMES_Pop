namespace SEOJIN_TMES.Client.Forms.WorkOrderEntry
{
    partial class WorkEntryConfirmForm
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
        /// 색·폰트·아이콘·문구는 WorkEntryConfirmForm.cs의 Initialize()에서 런타임에 넣는다.
        ///
        /// 구성(다른 팝업과 동일): 상단 색 띠(아이콘+제목) + 옅은 본문 안의 카드(공정 목록 그리드) + 하단 [닫기][이동].
        /// 도킹 순서 주의: bodyPanel(Fill) → footerPanel(Bottom) → headerPanel(Top).
        /// </summary>
        private void InitializeComponent()
        {
            this.headerPanel = new DevExpress.XtraEditors.PanelControl();
            this.headerIcon = new DevExpress.XtraEditors.SvgImageBox();
            this.lblTitle = new DevExpress.XtraEditors.LabelControl();
            this.bodyPanel = new DevExpress.XtraEditors.PanelControl();
            this.listCard = new DevExpress.XtraEditors.PanelControl();
            this.grid = new DevExpress.XtraGrid.GridControl();
            this.gridView = new SEOJIN_TMES.Client.Forms.Common.MeisGridView();
            this.lblListTitle = new DevExpress.XtraEditors.LabelControl();
            this.footerPanel = new DevExpress.XtraEditors.PanelControl();
            this.btnMove = new SEOJIN_TMES.Client.Forms.Common.MeisButton();
            this.btnClose = new SEOJIN_TMES.Client.Forms.Common.MeisButton();
            ((System.ComponentModel.ISupportInitialize)(this.headerPanel)).BeginInit();
            this.headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.headerIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bodyPanel)).BeginInit();
            this.bodyPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.listCard)).BeginInit();
            this.listCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView)).BeginInit();
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
            this.headerPanel.Size = new System.Drawing.Size(660, 56);
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
            this.lblTitle.Size = new System.Drawing.Size(68, 14);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "작업 진입 확인";
            // 
            // bodyPanel
            // 
            this.bodyPanel.Controls.Add(this.listCard);
            this.bodyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bodyPanel.Location = new System.Drawing.Point(0, 56);
            this.bodyPanel.Name = "bodyPanel";
            this.bodyPanel.Padding = new System.Windows.Forms.Padding(16, 12, 16, 8);
            this.bodyPanel.Size = new System.Drawing.Size(660, 452);
            this.bodyPanel.TabIndex = 1;
            // 
            // listCard
            // 
            this.listCard.Controls.Add(this.grid);
            this.listCard.Controls.Add(this.lblListTitle);
            this.listCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listCard.Location = new System.Drawing.Point(18, 14);
            this.listCard.Name = "listCard";
            this.listCard.Padding = new System.Windows.Forms.Padding(1);
            this.listCard.Size = new System.Drawing.Size(624, 428);
            this.listCard.TabIndex = 0;
            // 
            // grid
            // 
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.Location = new System.Drawing.Point(3, 35);
            this.grid.MainView = this.gridView;
            this.grid.Name = "grid";
            this.grid.Size = new System.Drawing.Size(618, 390);
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
            // lblListTitle
            // 
            this.lblListTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblListTitle.Location = new System.Drawing.Point(3, 3);
            this.lblListTitle.Name = "lblListTitle";
            this.lblListTitle.Padding = new System.Windows.Forms.Padding(16, 10, 0, 8);
            this.lblListTitle.Size = new System.Drawing.Size(60, 32);
            this.lblListTitle.TabIndex = 0;
            this.lblListTitle.Text = "공정 목록";
            // 
            // footerPanel
            // 
            this.footerPanel.Controls.Add(this.btnMove);
            this.footerPanel.Controls.Add(this.btnClose);
            this.footerPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.footerPanel.Location = new System.Drawing.Point(0, 508);
            this.footerPanel.Name = "footerPanel";
            this.footerPanel.Size = new System.Drawing.Size(660, 64);
            this.footerPanel.TabIndex = 2;
            // 
            // btnMove
            // 
            this.btnMove.Accent = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(70)))), ((int)(((byte)(229)))));
            this.btnMove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMove.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMove.Icon = null;
            this.btnMove.Location = new System.Drawing.Point(548, 16);
            this.btnMove.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
            this.btnMove.Name = "btnMove";
            this.btnMove.PaintStyle = DevExpress.XtraEditors.Controls.PaintStyles.Light;
            this.btnMove.Selected = false;
            this.btnMove.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            this.btnMove.Size = new System.Drawing.Size(96, 32);
            this.btnMove.TabIndex = 0;
            this.btnMove.Text = "이동";
            this.btnMove.TextAfterIcon = false;
            this.btnMove.Click += new System.EventHandler(this.btnMove_Click);
            // 
            // btnClose
            // 
            this.btnClose.Accent = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(70)))), ((int)(((byte)(229)))));
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Icon = null;
            this.btnClose.Location = new System.Drawing.Point(444, 16);
            this.btnClose.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
            this.btnClose.Name = "btnClose";
            this.btnClose.PaintStyle = DevExpress.XtraEditors.Controls.PaintStyles.Light;
            this.btnClose.Selected = false;
            this.btnClose.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            this.btnClose.Size = new System.Drawing.Size(96, 32);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "닫기";
            this.btnClose.TextAfterIcon = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // WorkEntryConfirmForm
            // 
            this.Appearance.Options.UseFont = true;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(660, 572);
            this.Controls.Add(this.bodyPanel);
            this.Controls.Add(this.footerPanel);
            this.Controls.Add(this.headerPanel);
            this.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.IconOptions.ShowIcon = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WorkEntryConfirmForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "작업 진입 확인";
            ((System.ComponentModel.ISupportInitialize)(this.headerPanel)).EndInit();
            this.headerPanel.ResumeLayout(false);
            this.headerPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.headerIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bodyPanel)).EndInit();
            this.bodyPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.listCard)).EndInit();
            this.listCard.ResumeLayout(false);
            this.listCard.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.footerPanel)).EndInit();
            this.footerPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.PanelControl headerPanel;
        private DevExpress.XtraEditors.SvgImageBox headerIcon;
        private DevExpress.XtraEditors.LabelControl lblTitle;
        private DevExpress.XtraEditors.PanelControl bodyPanel;
        private DevExpress.XtraEditors.PanelControl listCard;
        private DevExpress.XtraEditors.LabelControl lblListTitle;
        private DevExpress.XtraGrid.GridControl grid;
        private SEOJIN_TMES.Client.Forms.Common.MeisGridView gridView;
        private DevExpress.XtraEditors.PanelControl footerPanel;
        private SEOJIN_TMES.Client.Forms.Common.MeisButton btnMove;
        private SEOJIN_TMES.Client.Forms.Common.MeisButton btnClose;
    }
}
