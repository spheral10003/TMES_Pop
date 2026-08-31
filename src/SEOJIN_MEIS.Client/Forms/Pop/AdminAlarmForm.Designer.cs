namespace SEOJIN_MEIS.Client.Forms.Pop
{
    partial class AdminAlarmForm
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
        /// 색·폰트·아이콘·문구는 AdminAlarmForm.cs의 Initialize()에서 런타임에 넣는다.
        ///
        /// 구성: 상단 색 띠(아이콘+제목) + 알람 유형 콤보 + 「알람 상세 정보」 카드(대상 + 비고) + 하단 [닫기][전송].
        /// 대상 입력줄(lblTarget/cboTarget)은 알람 유형에 따라 보였다 숨었다 하므로 비고보다 위에 둔다.
        /// 도킹 순서 주의: bodyPanel(Fill) → footerPanel(Bottom) → headerPanel(Top).
        /// </summary>
        private void InitializeComponent()
        {
            this.headerPanel = new DevExpress.XtraEditors.PanelControl();
            this.headerIcon = new DevExpress.XtraEditors.SvgImageBox();
            this.lblTitle = new DevExpress.XtraEditors.LabelControl();
            this.bodyPanel = new DevExpress.XtraEditors.PanelControl();
            this.detailCard = new DevExpress.XtraEditors.PanelControl();
            this.lblDetailTitle = new DevExpress.XtraEditors.LabelControl();
            this.lblTarget = new DevExpress.XtraEditors.LabelControl();
            this.cboTarget = new DevExpress.XtraEditors.ComboBoxEdit();
            this.lblRemark = new DevExpress.XtraEditors.LabelControl();
            this.txtRemark = new DevExpress.XtraEditors.MemoEdit();
            this.spacer = new System.Windows.Forms.Panel();
            this.typeCard = new DevExpress.XtraEditors.PanelControl();
            this.lblAlarmType = new DevExpress.XtraEditors.LabelControl();
            this.cboAlarmType = new DevExpress.XtraEditors.ComboBoxEdit();
            this.footerPanel = new DevExpress.XtraEditors.PanelControl();
            this.btnSend = new DevExpress.XtraEditors.SimpleButton();
            this.btnClose = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.headerPanel)).BeginInit();
            this.headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.headerIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bodyPanel)).BeginInit();
            this.bodyPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.detailCard)).BeginInit();
            this.detailCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboTarget.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRemark.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.typeCard)).BeginInit();
            this.typeCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboAlarmType.Properties)).BeginInit();
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
            this.headerPanel.Size = new System.Drawing.Size(640, 56);
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
            this.lblTitle.Size = new System.Drawing.Size(150, 14);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "관리자 알람 유형 선택";
            //
            // bodyPanel
            //
            this.bodyPanel.Controls.Add(this.detailCard);
            this.bodyPanel.Controls.Add(this.spacer);
            this.bodyPanel.Controls.Add(this.typeCard);
            this.bodyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bodyPanel.Location = new System.Drawing.Point(0, 56);
            this.bodyPanel.Name = "bodyPanel";
            this.bodyPanel.Padding = new System.Windows.Forms.Padding(16, 12, 16, 8);
            this.bodyPanel.Size = new System.Drawing.Size(640, 336);
            this.bodyPanel.TabIndex = 1;
            //
            // typeCard
            //
            this.typeCard.Controls.Add(this.lblAlarmType);
            this.typeCard.Controls.Add(this.cboAlarmType);
            this.typeCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.typeCard.Location = new System.Drawing.Point(16, 12);
            this.typeCard.Name = "typeCard";
            this.typeCard.Padding = new System.Windows.Forms.Padding(1);
            this.typeCard.Size = new System.Drawing.Size(608, 62);
            this.typeCard.TabIndex = 0;
            //
            // lblAlarmType
            //
            this.lblAlarmType.Location = new System.Drawing.Point(18, 22);
            this.lblAlarmType.Name = "lblAlarmType";
            this.lblAlarmType.Size = new System.Drawing.Size(110, 20);
            this.lblAlarmType.TabIndex = 0;
            this.lblAlarmType.Text = "· 알람 유형";
            //
            // cboAlarmType
            //
            this.cboAlarmType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboAlarmType.Location = new System.Drawing.Point(136, 18);
            this.cboAlarmType.Name = "cboAlarmType";
            this.cboAlarmType.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboAlarmType.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cboAlarmType.Size = new System.Drawing.Size(452, 26);
            this.cboAlarmType.TabIndex = 1;
            this.cboAlarmType.SelectedIndexChanged += new System.EventHandler(this.cboAlarmType_SelectedIndexChanged);
            //
            // spacer
            //
            this.spacer.Dock = System.Windows.Forms.DockStyle.Top;
            this.spacer.Location = new System.Drawing.Point(16, 74);
            this.spacer.Name = "spacer";
            this.spacer.Size = new System.Drawing.Size(608, 10);
            this.spacer.TabIndex = 1;
            //
            // detailCard
            //
            this.detailCard.Controls.Add(this.lblDetailTitle);
            this.detailCard.Controls.Add(this.lblTarget);
            this.detailCard.Controls.Add(this.cboTarget);
            this.detailCard.Controls.Add(this.lblRemark);
            this.detailCard.Controls.Add(this.txtRemark);
            this.detailCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.detailCard.Location = new System.Drawing.Point(16, 84);
            this.detailCard.Name = "detailCard";
            this.detailCard.Padding = new System.Windows.Forms.Padding(1);
            this.detailCard.Size = new System.Drawing.Size(608, 244);
            this.detailCard.TabIndex = 2;
            //
            // lblDetailTitle
            //
            this.lblDetailTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDetailTitle.Location = new System.Drawing.Point(1, 1);
            this.lblDetailTitle.Name = "lblDetailTitle";
            this.lblDetailTitle.Padding = new System.Windows.Forms.Padding(16, 10, 0, 8);
            this.lblDetailTitle.Size = new System.Drawing.Size(606, 32);
            this.lblDetailTitle.TabIndex = 0;
            this.lblDetailTitle.Text = "알람 상세 정보";
            //
            // lblTarget
            //
            this.lblTarget.Location = new System.Drawing.Point(18, 50);
            this.lblTarget.Name = "lblTarget";
            this.lblTarget.Size = new System.Drawing.Size(110, 20);
            this.lblTarget.TabIndex = 1;
            this.lblTarget.Text = "· 결품 자재 코드";
            //
            // cboTarget
            //
            this.cboTarget.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboTarget.Location = new System.Drawing.Point(136, 46);
            this.cboTarget.Name = "cboTarget";
            this.cboTarget.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboTarget.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cboTarget.Size = new System.Drawing.Size(452, 26);
            this.cboTarget.TabIndex = 2;
            //
            // lblRemark
            //
            this.lblRemark.Location = new System.Drawing.Point(18, 88);
            this.lblRemark.Name = "lblRemark";
            this.lblRemark.Size = new System.Drawing.Size(110, 20);
            this.lblRemark.TabIndex = 3;
            this.lblRemark.Text = "· 비고";
            //
            // txtRemark
            //
            this.txtRemark.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRemark.Location = new System.Drawing.Point(136, 84);
            this.txtRemark.Name = "txtRemark";
            this.txtRemark.Size = new System.Drawing.Size(452, 142);
            this.txtRemark.TabIndex = 4;
            //
            // footerPanel
            //
            this.footerPanel.Controls.Add(this.btnSend);
            this.footerPanel.Controls.Add(this.btnClose);
            this.footerPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.footerPanel.Location = new System.Drawing.Point(0, 392);
            this.footerPanel.Name = "footerPanel";
            this.footerPanel.Size = new System.Drawing.Size(640, 64);
            this.footerPanel.TabIndex = 2;
            //
            // btnSend
            //
            this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSend.Location = new System.Drawing.Point(528, 16);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(96, 32);
            this.btnSend.TabIndex = 0;
            this.btnSend.Text = "전송";
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            //
            // btnClose
            //
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(424, 16);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(96, 32);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "닫기";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            //
            // AdminAlarmForm
            //
            this.Appearance.Options.UseFont = true;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(640, 456);
            this.Controls.Add(this.bodyPanel);
            this.Controls.Add(this.footerPanel);
            this.Controls.Add(this.headerPanel);
            this.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.IconOptions.ShowIcon = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AdminAlarmForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "관리자 알람 유형 선택";
            ((System.ComponentModel.ISupportInitialize)(this.footerPanel)).EndInit();
            this.footerPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cboAlarmType.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.typeCard)).EndInit();
            this.typeCard.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtRemark.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboTarget.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.detailCard)).EndInit();
            this.detailCard.ResumeLayout(false);
            this.detailCard.PerformLayout();
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
        private DevExpress.XtraEditors.PanelControl typeCard;
        private DevExpress.XtraEditors.LabelControl lblAlarmType;
        private DevExpress.XtraEditors.ComboBoxEdit cboAlarmType;
        private System.Windows.Forms.Panel spacer;
        private DevExpress.XtraEditors.PanelControl detailCard;
        private DevExpress.XtraEditors.LabelControl lblDetailTitle;
        private DevExpress.XtraEditors.LabelControl lblTarget;
        private DevExpress.XtraEditors.ComboBoxEdit cboTarget;
        private DevExpress.XtraEditors.LabelControl lblRemark;
        private DevExpress.XtraEditors.MemoEdit txtRemark;
        private DevExpress.XtraEditors.PanelControl footerPanel;
        private DevExpress.XtraEditors.SimpleButton btnSend;
        private DevExpress.XtraEditors.SimpleButton btnClose;
    }
}
