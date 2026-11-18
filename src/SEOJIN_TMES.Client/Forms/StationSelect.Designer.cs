namespace SEOJIN_TMES.Client.Forms
{
    partial class StationSelect
    {
        /// <summary>디자이너에서 사용하는 필수 변수입니다.</summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>사용 중인 모든 리소스를 정리합니다.</summary>
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
        /// 디자이너 지원에 필요한 메서드입니다. (람다식/삼항연산자/문자열보간/객체이니셜라이저 사용 금지 —
        /// 실제 문구/색/이벤트 로직은 StationSelect.cs에 둘 것)
        ///
        /// Add 순서: flowStations(Fill) → lblHint(Top) → lblTitle(Top) → pnlFooter(Bottom).
        /// 나중에 Add된 컨트롤이 먼저 도킹되므로 이 순서를 바꾸지 말 것.
        /// 카드를 누르면 바로 골라지므로 [확인] 버튼은 없다. [취소]는 Anchor=None으로 두어 폭이 바뀌어도 가운데에 남는다.
        /// </summary>
        private void InitializeComponent()
        {
            this.flowStations = new System.Windows.Forms.FlowLayoutPanel();
            this.lblHint = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowStations
            // 
            this.flowStations.AutoScroll = true;
            this.flowStations.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowStations.Location = new System.Drawing.Point(16, 72);
            this.flowStations.Name = "flowStations";
            this.flowStations.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.flowStations.Size = new System.Drawing.Size(1166, 556);
            this.flowStations.TabIndex = 0;
            // 
            // lblHint
            // 
            this.lblHint.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblHint.Font = new System.Drawing.Font("Tahoma", 12F);
            this.lblHint.Location = new System.Drawing.Point(16, 48);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new System.Drawing.Size(1166, 24);
            this.lblHint.TabIndex = 1;
            this.lblHint.Text = "접속할 워크스테이션을 누르세요.";
            this.lblHint.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Font = new System.Drawing.Font("Tahoma", 12F);
            this.lblTitle.Location = new System.Drawing.Point(16, 16);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(1166, 32);
            this.lblTitle.TabIndex = 2;
            this.lblTitle.Text = "워크스테이션 선택";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlFooter
            // 
            this.pnlFooter.Controls.Add(this.btnCancel);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(16, 628);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Size = new System.Drawing.Size(1166, 70);
            this.pnlFooter.TabIndex = 3;
            // 
            // btnCancel
            // 
            this.btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Tomato;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnCancel.Location = new System.Drawing.Point(502, 18);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(161, 40);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "취소";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            //
            // StationSelect
            // 
            this.Controls.Add(this.flowStations);
            this.Controls.Add(this.lblHint);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.pnlFooter);
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Name = "StationSelect";
            this.Padding = new System.Windows.Forms.Padding(16, 16, 16, 0);
            this.Size = new System.Drawing.Size(1200, 700);
            this.pnlFooter.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowStations;
        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel pnlFooter;
        private System.Windows.Forms.Button btnCancel;
    }
}
