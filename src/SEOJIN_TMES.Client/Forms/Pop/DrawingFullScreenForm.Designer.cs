namespace SEOJIN_TMES.Client.Forms.Pop
{
    partial class DrawingFullScreenForm
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
        /// 색 · 문구 · 창 위치(모니터 전체)는 DrawingFullScreenForm.cs의 Initialize()에서 런타임에 넣는다.
        ///
        /// 일반 .NET Form — 제목 줄 없이(FormBorderStyle.None) 모니터 전체를 덮는다.
        /// 도킹 순서 주의: picView(Fill) → pnlBar(Top). 나중에 Add한 pnlBar가 위 띠를 먼저 차지한다.
        /// pnlBar 안의 flowButtons는 오른쪽 정렬, 버튼은 [Zoom In][Zoom Out][Fit][닫기] 순서(PopScreen 머리줄과 같은 순서).
        /// </summary>
        private void InitializeComponent()
        {
            this.picView = new ImageBoxExtensions.ImageBoxEx();
            this.pnlBar = new System.Windows.Forms.Panel();
            this.flowButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnZoomIn = new System.Windows.Forms.Button();
            this.btnZoomOut = new System.Windows.Forms.Button();
            this.btnFit = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.pnlBar.SuspendLayout();
            this.flowButtons.SuspendLayout();
            this.SuspendLayout();
            //
            // picView
            //
            this.picView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.picView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picView.Location = new System.Drawing.Point(0, 40);
            this.picView.Name = "picView";
            this.picView.Size = new System.Drawing.Size(1024, 728);
            this.picView.TabIndex = 0;
            //
            // pnlBar
            //
            this.pnlBar.Controls.Add(this.flowButtons);
            this.pnlBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlBar.Location = new System.Drawing.Point(0, 0);
            this.pnlBar.Name = "pnlBar";
            this.pnlBar.Padding = new System.Windows.Forms.Padding(4);
            this.pnlBar.Size = new System.Drawing.Size(1024, 40);
            this.pnlBar.TabIndex = 1;
            //
            // flowButtons
            //
            this.flowButtons.AutoSize = true;
            this.flowButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowButtons.Controls.Add(this.btnZoomIn);
            this.flowButtons.Controls.Add(this.btnZoomOut);
            this.flowButtons.Controls.Add(this.btnFit);
            this.flowButtons.Controls.Add(this.btnClose);
            this.flowButtons.Dock = System.Windows.Forms.DockStyle.Right;
            this.flowButtons.Location = new System.Drawing.Point(620, 4);
            this.flowButtons.Name = "flowButtons";
            this.flowButtons.Size = new System.Drawing.Size(400, 32);
            this.flowButtons.TabIndex = 0;
            this.flowButtons.WrapContents = false;
            //
            // btnZoomIn
            //
            this.btnZoomIn.Font = new System.Drawing.Font("Noto Sans KR", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnZoomIn.Margin = new System.Windows.Forms.Padding(2);
            this.btnZoomIn.Name = "btnZoomIn";
            this.btnZoomIn.Size = new System.Drawing.Size(96, 28);
            this.btnZoomIn.TabIndex = 0;
            this.btnZoomIn.TabStop = false;
            this.btnZoomIn.Text = "Zoom In";
            //
            // btnZoomOut
            //
            this.btnZoomOut.Font = new System.Drawing.Font("Noto Sans KR", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnZoomOut.Margin = new System.Windows.Forms.Padding(2);
            this.btnZoomOut.Name = "btnZoomOut";
            this.btnZoomOut.Size = new System.Drawing.Size(96, 28);
            this.btnZoomOut.TabIndex = 1;
            this.btnZoomOut.TabStop = false;
            this.btnZoomOut.Text = "Zoom Out";
            //
            // btnFit
            //
            this.btnFit.Font = new System.Drawing.Font("Noto Sans KR", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnFit.Margin = new System.Windows.Forms.Padding(2);
            this.btnFit.Name = "btnFit";
            this.btnFit.Size = new System.Drawing.Size(96, 28);
            this.btnFit.TabIndex = 2;
            this.btnFit.TabStop = false;
            this.btnFit.Text = "Fit";
            //
            // btnClose
            //
            this.btnClose.Font = new System.Drawing.Font("Noto Sans KR", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnClose.Margin = new System.Windows.Forms.Padding(12, 2, 2, 2);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(96, 28);
            this.btnClose.TabIndex = 3;
            this.btnClose.TabStop = false;
            this.btnClose.Text = "Close";
            //
            // DrawingFullScreenForm
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1024, 768);
            this.Controls.Add(this.picView);
            this.Controls.Add(this.pnlBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "DrawingFullScreenForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "DrawingFullScreenForm";
            this.flowButtons.ResumeLayout(false);
            this.pnlBar.ResumeLayout(false);
            this.pnlBar.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private ImageBoxExtensions.ImageBoxEx picView;
        private System.Windows.Forms.Panel pnlBar;
        private System.Windows.Forms.FlowLayoutPanel flowButtons;
        private System.Windows.Forms.Button btnZoomIn;
        private System.Windows.Forms.Button btnZoomOut;
        private System.Windows.Forms.Button btnFit;
        private System.Windows.Forms.Button btnClose;
    }
}
