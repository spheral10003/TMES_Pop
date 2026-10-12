namespace SEOJIN_TMES.Client.Forms.Home
{
    /// <summary>
    /// 홈 화면 구조.
    ///
    /// 내용(브랜드 머리글 · 정보 타일)은 로그인 사용자·설정에 따라 달라지므로
    /// 여기서는 <b>담을 자리만</b> 잡고 HomeScreen.cs의 Initialize()에서 채운다
    /// (디자이너에는 구조·도킹·크기만 둔다는 규칙 — 프로젝트 CLAUDE.md 1번).
    ///
    /// Controls.Add 순서: card(Top) 하나뿐이라 순서 제약은 없지만,
    /// 나중에 채움 컨트롤을 추가하면 Fill 을 먼저 Add 해야 한다.
    /// </summary>
    partial class HomeScreen
    {
        private System.ComponentModel.IContainer components = null;

        private DevExpress.XtraEditors.PanelControl card;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.card = new DevExpress.XtraEditors.PanelControl();
            ((System.ComponentModel.ISupportInitialize)(this.card)).BeginInit();
            this.SuspendLayout();
            //
            // card
            //
            this.card.Dock = System.Windows.Forms.DockStyle.Top;
            this.card.Height = 232;
            this.card.Location = new System.Drawing.Point(0, 0);
            this.card.Name = "card";
            this.card.Size = new System.Drawing.Size(1000, 232);
            this.card.TabIndex = 0;
            //
            // HomeScreen
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.card);
            this.Name = "HomeScreen";
            this.Size = new System.Drawing.Size(1000, 600);
            ((System.ComponentModel.ISupportInitialize)(this.card)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
