namespace SEOJIN_MEIS.Client.Forms.Pop
{
    partial class MaterialSerialForm
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
        /// 색·폰트·아이콘·문구는 MaterialSerialForm.cs의 Initialize()에서 런타임에 넣는다.
        ///
        /// 구성(다른 팝업과 동일): 상단 색 띠(아이콘+제목) + 옅은 본문(자재 정보 카드 + 시리얼번호 그리드) + 하단 [닫기][저장].
        /// 도킹 순서 주의: bodyPanel(Fill) → footerPanel(Bottom) → headerPanel(Top),
        /// bodyPanel 안에서는 listCard(Fill) → spacer(Top) → fieldCard(Top).
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
            this.fieldCard = new DevExpress.XtraEditors.PanelControl();
            this.lblMaterialCode = new DevExpress.XtraEditors.LabelControl();
            this.txtMaterialCode = new DevExpress.XtraEditors.TextEdit();
            this.lblQty = new DevExpress.XtraEditors.LabelControl();
            this.txtQty = new DevExpress.XtraEditors.TextEdit();
            this.lblItemCode = new DevExpress.XtraEditors.LabelControl();
            this.txtItemCode = new DevExpress.XtraEditors.TextEdit();
            this.footerPanel = new DevExpress.XtraEditors.PanelControl();
            this.btnSave = new SEOJIN_MEIS.Client.Forms.Common.MeisButton();
            this.btnClose = new SEOJIN_MEIS.Client.Forms.Common.MeisButton();
            ((System.ComponentModel.ISupportInitialize)(this.headerPanel)).BeginInit();
            this.headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.headerIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bodyPanel)).BeginInit();
            this.bodyPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.listCard)).BeginInit();
            this.listCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fieldCard)).BeginInit();
            this.fieldCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtMaterialCode.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtQty.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtItemCode.Properties)).BeginInit();
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
            this.lblTitle.Text = "투입자재 정보 입력";
            //
            // bodyPanel
            //
            this.bodyPanel.Controls.Add(this.listCard);
            this.bodyPanel.Controls.Add(this.spacer);
            this.bodyPanel.Controls.Add(this.fieldCard);
            this.bodyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bodyPanel.Location = new System.Drawing.Point(0, 56);
            this.bodyPanel.Name = "bodyPanel";
            this.bodyPanel.Padding = new System.Windows.Forms.Padding(16, 12, 16, 8);
            this.bodyPanel.Size = new System.Drawing.Size(620, 400);
            this.bodyPanel.TabIndex = 1;
            //
            // fieldCard
            //
            this.fieldCard.Controls.Add(this.lblMaterialCode);
            this.fieldCard.Controls.Add(this.txtMaterialCode);
            this.fieldCard.Controls.Add(this.lblQty);
            this.fieldCard.Controls.Add(this.txtQty);
            this.fieldCard.Controls.Add(this.lblItemCode);
            this.fieldCard.Controls.Add(this.txtItemCode);
            this.fieldCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.fieldCard.Location = new System.Drawing.Point(16, 12);
            this.fieldCard.Name = "fieldCard";
            this.fieldCard.Padding = new System.Windows.Forms.Padding(1);
            this.fieldCard.Size = new System.Drawing.Size(588, 124);
            this.fieldCard.TabIndex = 0;
            //
            // lblMaterialCode
            //
            this.lblMaterialCode.Location = new System.Drawing.Point(18, 20);
            this.lblMaterialCode.Name = "lblMaterialCode";
            this.lblMaterialCode.Size = new System.Drawing.Size(120, 20);
            this.lblMaterialCode.TabIndex = 0;
            this.lblMaterialCode.Text = "· 투입자재 코드";
            //
            // txtMaterialCode
            //
            this.txtMaterialCode.Location = new System.Drawing.Point(146, 16);
            this.txtMaterialCode.Name = "txtMaterialCode";
            this.txtMaterialCode.Properties.ReadOnly = true;
            this.txtMaterialCode.Size = new System.Drawing.Size(180, 26);
            this.txtMaterialCode.TabIndex = 1;
            //
            // lblQty
            //
            this.lblQty.Location = new System.Drawing.Point(18, 54);
            this.lblQty.Name = "lblQty";
            this.lblQty.Size = new System.Drawing.Size(120, 20);
            this.lblQty.TabIndex = 2;
            this.lblQty.Text = "· 투입수량";
            //
            // txtQty
            //
            this.txtQty.Location = new System.Drawing.Point(146, 50);
            this.txtQty.Name = "txtQty";
            this.txtQty.Properties.ReadOnly = true;
            this.txtQty.Size = new System.Drawing.Size(180, 26);
            this.txtQty.TabIndex = 3;
            //
            // lblItemCode
            //
            this.lblItemCode.Location = new System.Drawing.Point(18, 88);
            this.lblItemCode.Name = "lblItemCode";
            this.lblItemCode.Size = new System.Drawing.Size(120, 20);
            this.lblItemCode.TabIndex = 4;
            this.lblItemCode.Text = "· 품목코드";
            //
            // txtItemCode
            //
            this.txtItemCode.Location = new System.Drawing.Point(146, 84);
            this.txtItemCode.Name = "txtItemCode";
            this.txtItemCode.Properties.ReadOnly = true;
            this.txtItemCode.Size = new System.Drawing.Size(180, 26);
            this.txtItemCode.TabIndex = 5;
            //
            // spacer
            //
            this.spacer.Dock = System.Windows.Forms.DockStyle.Top;
            this.spacer.Location = new System.Drawing.Point(16, 136);
            this.spacer.Name = "spacer";
            this.spacer.Size = new System.Drawing.Size(588, 10);
            this.spacer.TabIndex = 1;
            //
            // listCard
            //
            this.listCard.Controls.Add(this.grid);
            this.listCard.Controls.Add(this.lblListTitle);
            this.listCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listCard.Location = new System.Drawing.Point(16, 146);
            this.listCard.Name = "listCard";
            this.listCard.Padding = new System.Windows.Forms.Padding(1);
            this.listCard.Size = new System.Drawing.Size(588, 246);
            this.listCard.TabIndex = 2;
            //
            // grid
            //
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.Location = new System.Drawing.Point(1, 33);
            this.grid.MainView = this.gridView;
            this.grid.Name = "grid";
            this.grid.Size = new System.Drawing.Size(586, 212);
            this.grid.TabIndex = 1;
            this.grid.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView});
            //
            // gridView
            //
            this.gridView.GridControl = this.grid;
            this.gridView.Name = "gridView";
            //
            // lblListTitle
            //
            this.lblListTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblListTitle.Location = new System.Drawing.Point(1, 1);
            this.lblListTitle.Name = "lblListTitle";
            this.lblListTitle.Padding = new System.Windows.Forms.Padding(16, 10, 0, 8);
            this.lblListTitle.Size = new System.Drawing.Size(586, 32);
            this.lblListTitle.TabIndex = 0;
            this.lblListTitle.Text = "시리얼 번호";
            //
            // footerPanel
            //
            this.footerPanel.Controls.Add(this.btnSave);
            this.footerPanel.Controls.Add(this.btnClose);
            this.footerPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.footerPanel.Location = new System.Drawing.Point(0, 456);
            this.footerPanel.Name = "footerPanel";
            this.footerPanel.Size = new System.Drawing.Size(620, 64);
            this.footerPanel.TabIndex = 2;
            //
            // btnSave
            //
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(508, 16);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(96, 32);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "저장";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
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
            // MaterialSerialForm
            //
            this.Appearance.Options.UseFont = true;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(620, 520);
            this.Controls.Add(this.bodyPanel);
            this.Controls.Add(this.footerPanel);
            this.Controls.Add(this.headerPanel);
            this.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.IconOptions.ShowIcon = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MaterialSerialForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "투입자재 정보 입력";
            ((System.ComponentModel.ISupportInitialize)(this.footerPanel)).EndInit();
            this.footerPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtItemCode.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtQty.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtMaterialCode.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fieldCard)).EndInit();
            this.fieldCard.ResumeLayout(false);
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
        private DevExpress.XtraEditors.PanelControl fieldCard;
        private DevExpress.XtraEditors.LabelControl lblMaterialCode;
        private DevExpress.XtraEditors.TextEdit txtMaterialCode;
        private DevExpress.XtraEditors.LabelControl lblQty;
        private DevExpress.XtraEditors.TextEdit txtQty;
        private DevExpress.XtraEditors.LabelControl lblItemCode;
        private DevExpress.XtraEditors.TextEdit txtItemCode;
        private System.Windows.Forms.Panel spacer;
        private DevExpress.XtraEditors.PanelControl listCard;
        private DevExpress.XtraEditors.LabelControl lblListTitle;
        private DevExpress.XtraGrid.GridControl grid;
        private SEOJIN_MEIS.Client.Forms.Common.MeisGridView gridView;
        private DevExpress.XtraEditors.PanelControl footerPanel;
        private SEOJIN_MEIS.Client.Forms.Common.MeisButton btnSave;
        private SEOJIN_MEIS.Client.Forms.Common.MeisButton btnClose;
    }
}
