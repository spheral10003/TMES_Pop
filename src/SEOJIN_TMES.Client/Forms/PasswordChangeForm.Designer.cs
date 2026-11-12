namespace SEOJIN_TMES.Client.Forms
{
    partial class PasswordChangeForm
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
        /// 색 · 글꼴 · 다국어 문구는 PasswordChangeForm.cs의 Initialize()에서 입힌다.
        ///
        /// Add 순서(나중에 Add한 것이 먼저 도킹된다):
        ///   pnlBody(Fill) → pnlFooterLine(Bottom) → pnlFooter(Bottom) → pnlTitleLine(Top) → pnlTitle(Top)
        /// 그래서 위에서부터 제목 → 선 → 본문 → 선 → 버튼 줄 순서로 놓인다. 순서를 바꾸면 선 위치가 어긋난다.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlTitle = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pnlTitleLine = new System.Windows.Forms.Panel();
            this.pnlBody = new System.Windows.Forms.Panel();
            this.lblPasswordMark = new System.Windows.Forms.Label();
            this.lblPassword = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.lblConfirmMark = new System.Windows.Forms.Label();
            this.lblConfirm = new System.Windows.Forms.Label();
            this.txtConfirm = new System.Windows.Forms.TextBox();
            this.pnlError = new System.Windows.Forms.Panel();
            this.lblError = new System.Windows.Forms.Label();
            this.pnlFooterLine = new System.Windows.Forms.Panel();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.btnApply = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.pnlTitle.SuspendLayout();
            this.pnlBody.SuspendLayout();
            this.pnlError.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlTitle
            //
            this.pnlTitle.Controls.Add(this.lblTitle);
            this.pnlTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTitle.Location = new System.Drawing.Point(0, 0);
            this.pnlTitle.Name = "pnlTitle";
            this.pnlTitle.Size = new System.Drawing.Size(480, 52);
            this.pnlTitle.TabIndex = 0;
            //
            // lblTitle
            //
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.lblTitle.Size = new System.Drawing.Size(480, 52);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "비밀번호 변경";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // pnlTitleLine
            //
            this.pnlTitleLine.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTitleLine.Location = new System.Drawing.Point(0, 52);
            this.pnlTitleLine.Name = "pnlTitleLine";
            this.pnlTitleLine.Size = new System.Drawing.Size(480, 1);
            this.pnlTitleLine.TabIndex = 1;
            //
            // pnlBody
            //
            this.pnlBody.Controls.Add(this.lblPasswordMark);
            this.pnlBody.Controls.Add(this.lblPassword);
            this.pnlBody.Controls.Add(this.txtPassword);
            this.pnlBody.Controls.Add(this.lblConfirmMark);
            this.pnlBody.Controls.Add(this.lblConfirm);
            this.pnlBody.Controls.Add(this.txtConfirm);
            this.pnlBody.Controls.Add(this.pnlError);
            this.pnlBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBody.Location = new System.Drawing.Point(0, 53);
            this.pnlBody.Name = "pnlBody";
            this.pnlBody.Size = new System.Drawing.Size(480, 108);
            this.pnlBody.TabIndex = 2;
            //
            // lblPasswordMark — 필수 입력 표시(붉은 점)
            //
            this.lblPasswordMark.Location = new System.Drawing.Point(36, 18);
            this.lblPasswordMark.Name = "lblPasswordMark";
            this.lblPasswordMark.Size = new System.Drawing.Size(14, 30);
            this.lblPasswordMark.TabIndex = 10;
            this.lblPasswordMark.Text = "•";
            this.lblPasswordMark.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lblPassword
            //
            this.lblPassword.Location = new System.Drawing.Point(50, 18);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(120, 30);
            this.lblPassword.TabIndex = 11;
            this.lblPassword.Text = "비밀번호";
            this.lblPassword.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // txtPassword
            //
            this.txtPassword.Location = new System.Drawing.Point(176, 21);
            this.txtPassword.MaxLength = 50;
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(268, 24);
            this.txtPassword.TabIndex = 0;
            this.txtPassword.UseSystemPasswordChar = true;
            //
            // lblConfirmMark
            //
            this.lblConfirmMark.Location = new System.Drawing.Point(36, 58);
            this.lblConfirmMark.Name = "lblConfirmMark";
            this.lblConfirmMark.Size = new System.Drawing.Size(14, 30);
            this.lblConfirmMark.TabIndex = 12;
            this.lblConfirmMark.Text = "•";
            this.lblConfirmMark.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lblConfirm
            //
            this.lblConfirm.Location = new System.Drawing.Point(50, 58);
            this.lblConfirm.Name = "lblConfirm";
            this.lblConfirm.Size = new System.Drawing.Size(120, 30);
            this.lblConfirm.TabIndex = 13;
            this.lblConfirm.Text = "비밀번호 확인";
            this.lblConfirm.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // txtConfirm
            //
            this.txtConfirm.Location = new System.Drawing.Point(176, 61);
            this.txtConfirm.MaxLength = 50;
            this.txtConfirm.Name = "txtConfirm";
            this.txtConfirm.Size = new System.Drawing.Size(268, 24);
            this.txtConfirm.TabIndex = 1;
            this.txtConfirm.UseSystemPasswordChar = true;
            //
            // pnlError — 로그인 창의 오류 상자와 같은 붉은 상자. 평소엔 숨기고, 문구가 생기면 창을 그만큼 늘려 보인다.
            //
            this.pnlError.Controls.Add(this.lblError);
            this.pnlError.Location = new System.Drawing.Point(36, 100);
            this.pnlError.Name = "pnlError";
            this.pnlError.Padding = new System.Windows.Forms.Padding(1);
            this.pnlError.Size = new System.Drawing.Size(408, 40);
            this.pnlError.TabIndex = 14;
            this.pnlError.Visible = false;
            this.pnlError.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlError_Paint);
            //
            // lblError
            //
            this.lblError.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblError.Location = new System.Drawing.Point(1, 1);
            this.lblError.Name = "lblError";
            this.lblError.Padding = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.lblError.Size = new System.Drawing.Size(406, 38);
            this.lblError.TabIndex = 0;
            this.lblError.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // pnlFooterLine
            //
            this.pnlFooterLine.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooterLine.Location = new System.Drawing.Point(0, 161);
            this.pnlFooterLine.Name = "pnlFooterLine";
            this.pnlFooterLine.Size = new System.Drawing.Size(480, 1);
            this.pnlFooterLine.TabIndex = 3;
            //
            // pnlFooter — 버튼은 메시지 창(AppMessagePanel)과 같은 자리: 오른쪽 아래, 88×36, 간격 8, 오른쪽 여백 16.
            //
            this.pnlFooter.Controls.Add(this.btnApply);
            this.pnlFooter.Controls.Add(this.btnClose);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 162);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Size = new System.Drawing.Size(480, 60);
            this.pnlFooter.TabIndex = 4;
            //
            // btnApply
            //
            this.btnApply.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnApply.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnApply.Location = new System.Drawing.Point(280, 12);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(88, 36);
            this.btnApply.TabIndex = 2;
            this.btnApply.Text = "적용";
            this.btnApply.UseVisualStyleBackColor = false;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            //
            // btnClose
            //
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Location = new System.Drawing.Point(376, 12);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(88, 36);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "닫기";
            this.btnClose.UseVisualStyleBackColor = false;
            //
            // PasswordChangeForm
            //
            this.AcceptButton = this.btnApply;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(480, 222);
            this.Controls.Add(this.pnlBody);
            this.Controls.Add(this.pnlFooterLine);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.pnlTitleLine);
            this.Controls.Add(this.pnlTitle);
            this.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PasswordChangeForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "비밀번호 변경";
            this.pnlTitle.ResumeLayout(false);
            this.pnlBody.ResumeLayout(false);
            this.pnlBody.PerformLayout();
            this.pnlError.ResumeLayout(false);
            this.pnlFooter.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlTitle;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel pnlTitleLine;
        private System.Windows.Forms.Panel pnlBody;
        private System.Windows.Forms.Label lblPasswordMark;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label lblConfirmMark;
        private System.Windows.Forms.Label lblConfirm;
        private System.Windows.Forms.TextBox txtConfirm;
        private System.Windows.Forms.Panel pnlError;
        private System.Windows.Forms.Label lblError;
        private System.Windows.Forms.Panel pnlFooterLine;
        private System.Windows.Forms.Panel pnlFooter;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnClose;
    }
}
