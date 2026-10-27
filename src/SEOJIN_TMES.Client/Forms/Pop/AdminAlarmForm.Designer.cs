namespace SEOJIN_TMES.Client.Forms.Pop
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
        /// 색·폰트·문구는 AdminAlarmForm.cs의 Initialize()에서 런타임에 넣고, 타일(CardTile)도 조회 결과로 런타임에 만든다.
        ///
        /// 일반 .NET Form — Windows 제목 줄은 없애고(FormBorderStyle.None) pnlTitle + lblTitle이 제목 줄 역할을 한다.
        /// 구성: 제목 띠(오른쪽 X 닫기) + [알람 유형 타일] + [알람 코드 타일] + 하단 [닫기][등록].
        /// lblClose는 lblTitle(Fill)보다 먼저 Add해 위에 그려지게 둔다(FileUploadForm과 같은 구성).
        /// 도킹 순서 주의: pnlBody(Fill) → pnlFooter(Bottom) → pnlTitle(Top).
        ///                 pnlBody 안은 flowCodes(Fill) → lblCodeTitle → flowTypes → lblTypeTitle(모두 Top).
        ///                 나중에 Add한 것이 위에 놓인다.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlTitle = new System.Windows.Forms.Panel();
            this.lblClose = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pnlBody = new System.Windows.Forms.Panel();
            this.flowCodes = new System.Windows.Forms.FlowLayoutPanel();
            this.lblCodeTitle = new System.Windows.Forms.Label();
            this.flowTypes = new System.Windows.Forms.FlowLayoutPanel();
            this.lblTypeTitle = new System.Windows.Forms.Label();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.btnSend = new SEOJIN_TMES.Client.Forms.Common.MeisButton();
            this.btnClose = new SEOJIN_TMES.Client.Forms.Common.MeisButton();
            this.pnlTitle.SuspendLayout();
            this.pnlBody.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlTitle
            //
            this.pnlTitle.Controls.Add(this.lblClose);
            this.pnlTitle.Controls.Add(this.lblTitle);
            this.pnlTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTitle.Location = new System.Drawing.Point(0, 0);
            this.pnlTitle.Name = "pnlTitle";
            this.pnlTitle.Size = new System.Drawing.Size(1200, 48);
            this.pnlTitle.TabIndex = 0;
            //
            // lblClose
            //
            this.lblClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblClose.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblClose.Location = new System.Drawing.Point(1156, 8);
            this.lblClose.Name = "lblClose";
            this.lblClose.Size = new System.Drawing.Size(32, 32);
            this.lblClose.TabIndex = 1;
            this.lblClose.Text = "X";
            this.lblClose.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblClose.Click += new System.EventHandler(this.btnClose_Click);
            this.lblClose.MouseEnter += new System.EventHandler(this.lblClose_MouseEnter);
            this.lblClose.MouseLeave += new System.EventHandler(this.lblClose_MouseLeave);
            //
            // lblTitle
            //
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Padding = new System.Windows.Forms.Padding(16, 0, 0, 0);
            this.lblTitle.Size = new System.Drawing.Size(1200, 48);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "알람 조회 / 등록";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // pnlBody
            //
            this.pnlBody.Controls.Add(this.flowCodes);
            this.pnlBody.Controls.Add(this.lblCodeTitle);
            this.pnlBody.Controls.Add(this.flowTypes);
            this.pnlBody.Controls.Add(this.lblTypeTitle);
            this.pnlBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBody.Location = new System.Drawing.Point(0, 48);
            this.pnlBody.Name = "pnlBody";
            this.pnlBody.Padding = new System.Windows.Forms.Padding(16, 4, 16, 8);
            this.pnlBody.Size = new System.Drawing.Size(1200, 488);
            this.pnlBody.TabIndex = 1;
            //
            // flowCodes
            //
            this.flowCodes.AutoScroll = true;
            this.flowCodes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowCodes.Location = new System.Drawing.Point(16, 188);
            this.flowCodes.Name = "flowCodes";
            this.flowCodes.Size = new System.Drawing.Size(1168, 292);
            this.flowCodes.TabIndex = 3;
            //
            // lblCodeTitle
            //
            this.lblCodeTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblCodeTitle.Location = new System.Drawing.Point(16, 156);
            this.lblCodeTitle.Name = "lblCodeTitle";
            this.lblCodeTitle.Size = new System.Drawing.Size(1168, 32);
            this.lblCodeTitle.TabIndex = 2;
            this.lblCodeTitle.Text = "알람 코드";
            this.lblCodeTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // flowTypes
            //
            this.flowTypes.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowTypes.Location = new System.Drawing.Point(16, 36);
            this.flowTypes.Name = "flowTypes";
            this.flowTypes.Size = new System.Drawing.Size(1168, 120);
            this.flowTypes.TabIndex = 1;
            this.flowTypes.WrapContents = false;
            //
            // lblTypeTitle
            //
            this.lblTypeTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTypeTitle.Location = new System.Drawing.Point(16, 4);
            this.lblTypeTitle.Name = "lblTypeTitle";
            this.lblTypeTitle.Size = new System.Drawing.Size(1168, 32);
            this.lblTypeTitle.TabIndex = 0;
            this.lblTypeTitle.Text = "알람 유형";
            this.lblTypeTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // pnlFooter
            //
            this.pnlFooter.Controls.Add(this.btnSend);
            this.pnlFooter.Controls.Add(this.btnClose);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 536);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Size = new System.Drawing.Size(1200, 64);
            this.pnlFooter.TabIndex = 2;
            //
            // btnSend
            //
            this.btnSend.Accent = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(70)))), ((int)(((byte)(229)))));
            this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSend.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSend.Enabled = false;
            this.btnSend.Icon = null;
            this.btnSend.Location = new System.Drawing.Point(1072, 14);
            this.btnSend.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
            this.btnSend.Name = "btnSend";
            this.btnSend.PaintStyle = DevExpress.XtraEditors.Controls.PaintStyles.Light;
            this.btnSend.Selected = false;
            this.btnSend.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            this.btnSend.Size = new System.Drawing.Size(112, 36);
            this.btnSend.TabIndex = 0;
            this.btnSend.Text = "등록";
            this.btnSend.TextAfterIcon = false;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            //
            // btnClose
            //
            this.btnClose.Accent = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(70)))), ((int)(((byte)(229)))));
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Icon = null;
            this.btnClose.Location = new System.Drawing.Point(952, 14);
            this.btnClose.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
            this.btnClose.Name = "btnClose";
            this.btnClose.PaintStyle = DevExpress.XtraEditors.Controls.PaintStyles.Light;
            this.btnClose.Selected = false;
            this.btnClose.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            this.btnClose.Size = new System.Drawing.Size(112, 36);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "닫기";
            this.btnClose.TextAfterIcon = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            //
            // AdminAlarmForm
            //
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(1200, 600);
            this.Controls.Add(this.pnlBody);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.pnlTitle);
            this.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AdminAlarmForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "알람 조회 / 등록";
            this.Shown += new System.EventHandler(this.AdminAlarmForm_Shown);
            this.pnlTitle.ResumeLayout(false);
            this.pnlBody.ResumeLayout(false);
            this.pnlFooter.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlTitle;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblClose;
        private System.Windows.Forms.Panel pnlBody;
        private System.Windows.Forms.Label lblTypeTitle;
        private System.Windows.Forms.FlowLayoutPanel flowTypes;
        private System.Windows.Forms.Label lblCodeTitle;
        private System.Windows.Forms.FlowLayoutPanel flowCodes;
        private System.Windows.Forms.Panel pnlFooter;
        private SEOJIN_TMES.Client.Forms.Common.MeisButton btnSend;
        private SEOJIN_TMES.Client.Forms.Common.MeisButton btnClose;
    }
}
