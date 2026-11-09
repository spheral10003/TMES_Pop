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
        /// 구성: 제목 띠(오른쪽 X 닫기) + [알람 유형 타일] + [알람 코드 타일] + [비고 입력란] + 하단 [닫기][등록] + 우하단 크기 조절 손잡이(pnlGrip).
        /// 크기 가변: 유형 타일(flowTypes)은 AutoSize라 폭이 줄면 다음 줄로 넘어가며 높이가 늘고,
        ///            알람 코드 타일 크기는 flowCodes_SizeChanged에서 다시 계산한다. 최소 크기는 AdminAlarmForm.cs에서 정한다.
        /// lblClose는 lblTitle(Fill)보다 먼저 Add해 위에 그려지게 둔다(FileUploadForm과 같은 구성).
        /// 도킹 순서 주의: pnlBody(Fill) → pnlFooter(Bottom) → pnlTitle(Top).
        ///                 pnlBody 안은 flowCodes(Fill) → lblRemarkTitle(Bottom) → txtRemark(Bottom)
        ///                 → lblCodeTitle → flowTypes → lblTypeTitle(Top).
        ///                 나중에 Add한 것이 바깥쪽에 놓인다 — 비고 입력란이 맨 아래, 그 위에 비고 제목(StopIssueForm과 같은 구성).
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlTitle = new System.Windows.Forms.Panel();
            this.lblClose = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pnlBody = new System.Windows.Forms.Panel();
            this.flowCodes = new System.Windows.Forms.FlowLayoutPanel();
            this.lblRemarkTitle = new System.Windows.Forms.Label();
            this.txtRemark = new System.Windows.Forms.RichTextBox();
            this.lblCodeTitle = new System.Windows.Forms.Label();
            this.flowTypes = new System.Windows.Forms.FlowLayoutPanel();
            this.lblTypeTitle = new System.Windows.Forms.Label();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.pnlGrip = new System.Windows.Forms.Panel();
            this.btnSend = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
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
            this.pnlBody.Controls.Add(this.lblRemarkTitle);
            this.pnlBody.Controls.Add(this.txtRemark);
            this.pnlBody.Controls.Add(this.lblCodeTitle);
            this.pnlBody.Controls.Add(this.flowTypes);
            this.pnlBody.Controls.Add(this.lblTypeTitle);
            this.pnlBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBody.Location = new System.Drawing.Point(0, 48);
            this.pnlBody.Name = "pnlBody";
            this.pnlBody.Padding = new System.Windows.Forms.Padding(16, 4, 16, 8);
            this.pnlBody.Size = new System.Drawing.Size(1200, 588);
            this.pnlBody.TabIndex = 1;
            // 
            // flowCodes
            // 
            this.flowCodes.AutoScroll = true;
            this.flowCodes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowCodes.Location = new System.Drawing.Point(16, 68);
            this.flowCodes.Name = "flowCodes";
            this.flowCodes.Size = new System.Drawing.Size(1168, 400);
            this.flowCodes.TabIndex = 3;
            this.flowCodes.SizeChanged += new System.EventHandler(this.flowCodes_SizeChanged);
            //
            // lblRemarkTitle
            //
            this.lblRemarkTitle.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblRemarkTitle.Location = new System.Drawing.Point(16, 468);
            this.lblRemarkTitle.Name = "lblRemarkTitle";
            this.lblRemarkTitle.Size = new System.Drawing.Size(1168, 32);
            this.lblRemarkTitle.TabIndex = 4;
            this.lblRemarkTitle.Text = "비고";
            this.lblRemarkTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // txtRemark
            //
            this.txtRemark.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtRemark.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txtRemark.Location = new System.Drawing.Point(16, 500);
            this.txtRemark.MaxLength = 5000;
            this.txtRemark.Name = "txtRemark";
            this.txtRemark.Size = new System.Drawing.Size(1168, 80);
            this.txtRemark.TabIndex = 5;
            this.txtRemark.Text = "";
            //
            // lblCodeTitle
            // 
            this.lblCodeTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblCodeTitle.Location = new System.Drawing.Point(16, 36);
            this.lblCodeTitle.Name = "lblCodeTitle";
            this.lblCodeTitle.Size = new System.Drawing.Size(1168, 32);
            this.lblCodeTitle.TabIndex = 2;
            this.lblCodeTitle.Text = "알람 코드";
            this.lblCodeTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // flowTypes
            // 
            this.flowTypes.AutoSize = true;
            this.flowTypes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowTypes.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowTypes.Location = new System.Drawing.Point(16, 36);
            this.flowTypes.Name = "flowTypes";
            this.flowTypes.Size = new System.Drawing.Size(1168, 0);
            this.flowTypes.TabIndex = 1;
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
            this.pnlFooter.Controls.Add(this.pnlGrip);
            this.pnlFooter.Controls.Add(this.btnSend);
            this.pnlFooter.Controls.Add(this.btnClose);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 636);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Size = new System.Drawing.Size(1200, 64);
            this.pnlFooter.TabIndex = 2;
            // 
            // pnlGrip
            // 
            this.pnlGrip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlGrip.Location = new System.Drawing.Point(1184, 48);
            this.pnlGrip.Name = "pnlGrip";
            this.pnlGrip.Size = new System.Drawing.Size(16, 16);
            this.pnlGrip.TabIndex = 2;
            // 
            // btnSend
            // 
            this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSend.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSend.Enabled = false;
            this.btnSend.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSend.Location = new System.Drawing.Point(900, 12);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(138, 39);
            this.btnSend.TabIndex = 0;
            this.btnSend.Text = "등록";
            this.btnSend.UseVisualStyleBackColor = false;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Location = new System.Drawing.Point(1046, 12);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(138, 39);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "닫기";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // AdminAlarmForm
            // 
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(1200, 700);
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
            this.pnlBody.PerformLayout();
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
        private System.Windows.Forms.Label lblRemarkTitle;
        private System.Windows.Forms.RichTextBox txtRemark;
        private System.Windows.Forms.Panel pnlFooter;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Panel pnlGrip;
    }
}
