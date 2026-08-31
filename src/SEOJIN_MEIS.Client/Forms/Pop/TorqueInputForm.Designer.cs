namespace SEOJIN_MEIS.Client.Forms.Pop
{
    partial class TorqueInputForm
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
        /// 색·폰트·아이콘·문구는 TorqueInputForm.cs의 Initialize()에서 런타임에 넣는다.
        ///
        /// 구성: 상단 색 띠(아이콘+제목) + 본문 카드(스팩명 / 허용 범위 / 결과값 입력) + 하단 [닫기][저장].
        /// 도킹 순서 주의: bodyPanel(Fill) → footerPanel(Bottom) → headerPanel(Top).
        /// </summary>
        private void InitializeComponent()
        {
            this.headerPanel = new DevExpress.XtraEditors.PanelControl();
            this.headerIcon = new DevExpress.XtraEditors.SvgImageBox();
            this.lblTitle = new DevExpress.XtraEditors.LabelControl();
            this.bodyPanel = new DevExpress.XtraEditors.PanelControl();
            this.card = new DevExpress.XtraEditors.PanelControl();
            this.lblSpecName = new DevExpress.XtraEditors.LabelControl();
            this.lblRange = new DevExpress.XtraEditors.LabelControl();
            this.lblValueCaption = new DevExpress.XtraEditors.LabelControl();
            this.txtValue = new DevExpress.XtraEditors.TextEdit();
            this.footerPanel = new DevExpress.XtraEditors.PanelControl();
            this.btnSave = new DevExpress.XtraEditors.SimpleButton();
            this.btnClose = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.headerPanel)).BeginInit();
            this.headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.headerIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bodyPanel)).BeginInit();
            this.bodyPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.card)).BeginInit();
            this.card.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtValue.Properties)).BeginInit();
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
            this.headerPanel.Size = new System.Drawing.Size(460, 56);
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
            this.lblTitle.Text = "토크 결과값 입력";
            //
            // bodyPanel
            //
            this.bodyPanel.Controls.Add(this.card);
            this.bodyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bodyPanel.Location = new System.Drawing.Point(0, 56);
            this.bodyPanel.Name = "bodyPanel";
            this.bodyPanel.Padding = new System.Windows.Forms.Padding(16, 12, 16, 8);
            this.bodyPanel.Size = new System.Drawing.Size(460, 180);
            this.bodyPanel.TabIndex = 1;
            //
            // card
            //
            this.card.Controls.Add(this.lblSpecName);
            this.card.Controls.Add(this.lblRange);
            this.card.Controls.Add(this.lblValueCaption);
            this.card.Controls.Add(this.txtValue);
            this.card.Dock = System.Windows.Forms.DockStyle.Fill;
            this.card.Location = new System.Drawing.Point(16, 12);
            this.card.Name = "card";
            this.card.Padding = new System.Windows.Forms.Padding(1);
            this.card.Size = new System.Drawing.Size(428, 160);
            this.card.TabIndex = 0;
            //
            // lblSpecName
            //
            this.lblSpecName.Location = new System.Drawing.Point(20, 20);
            this.lblSpecName.Name = "lblSpecName";
            this.lblSpecName.Size = new System.Drawing.Size(380, 20);
            this.lblSpecName.TabIndex = 0;
            //
            // lblRange
            //
            this.lblRange.Location = new System.Drawing.Point(20, 50);
            this.lblRange.Name = "lblRange";
            this.lblRange.Size = new System.Drawing.Size(380, 20);
            this.lblRange.TabIndex = 1;
            //
            // lblValueCaption
            //
            this.lblValueCaption.Location = new System.Drawing.Point(20, 100);
            this.lblValueCaption.Name = "lblValueCaption";
            this.lblValueCaption.Size = new System.Drawing.Size(120, 20);
            this.lblValueCaption.TabIndex = 2;
            this.lblValueCaption.Text = "· 토크 결과값";
            //
            // txtValue
            //
            this.txtValue.Location = new System.Drawing.Point(150, 96);
            this.txtValue.Name = "txtValue";
            this.txtValue.Size = new System.Drawing.Size(180, 26);
            this.txtValue.TabIndex = 3;
            //
            // footerPanel
            //
            this.footerPanel.Controls.Add(this.btnSave);
            this.footerPanel.Controls.Add(this.btnClose);
            this.footerPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.footerPanel.Location = new System.Drawing.Point(0, 236);
            this.footerPanel.Name = "footerPanel";
            this.footerPanel.Size = new System.Drawing.Size(460, 64);
            this.footerPanel.TabIndex = 2;
            //
            // btnSave
            //
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(348, 16);
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
            this.btnClose.Location = new System.Drawing.Point(244, 16);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(96, 32);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "닫기";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            //
            // TorqueInputForm
            //
            this.Appearance.Options.UseFont = true;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(460, 300);
            this.Controls.Add(this.bodyPanel);
            this.Controls.Add(this.footerPanel);
            this.Controls.Add(this.headerPanel);
            this.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.IconOptions.ShowIcon = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TorqueInputForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "토크 결과값 입력";
            ((System.ComponentModel.ISupportInitialize)(this.footerPanel)).EndInit();
            this.footerPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtValue.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.card)).EndInit();
            this.card.ResumeLayout(false);
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
        private DevExpress.XtraEditors.PanelControl card;
        private DevExpress.XtraEditors.LabelControl lblSpecName;
        private DevExpress.XtraEditors.LabelControl lblRange;
        private DevExpress.XtraEditors.LabelControl lblValueCaption;
        private DevExpress.XtraEditors.TextEdit txtValue;
        private DevExpress.XtraEditors.PanelControl footerPanel;
        private DevExpress.XtraEditors.SimpleButton btnSave;
        private DevExpress.XtraEditors.SimpleButton btnClose;
    }
}
