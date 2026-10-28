namespace SEOJIN_TMES.Client.Forms
{
    partial class LoginForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 이 메서드의 내용을
        /// 코드 편집기로 수정하지 마세요. (람다식/삼항연산자/문자열보간/객체이니셜라이저 사용 금지 —
        /// 사용 시 VS 디자이너 로드 오류가 재발합니다. 실제 문구/이벤트 로직은 LoginForm.cs에 둘 것)
        ///
        /// 배경 그림(Images\tmes_login_card.png, 1200×650)의 입력 칸 좌표에 맞춰 컨트롤을 얹는다 — 도킹을 쓰지 않는다.
        /// 그림 좌표(px): 입력 칸 x=700~1099 / 언어 y=216~255 · 사업부 292~331 · ID 368~407 · Password 444~483,
        /// 아이디 저장 y=506~518, 접속 버튼 y=529~583. 항목 이름 글자는 x=718부터, 앞쪽 아이콘(x=700~712)은 가리지 않는다.
        /// 그림을 바꾸면 이 좌표도 함께 다시 잴 것.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginForm));
            this.btnClose = new System.Windows.Forms.Button();
            this.lblLanguage = new DevExpress.XtraEditors.LabelControl();
            this.pnlLanguage = new System.Windows.Forms.Panel();
            this.cboLanguage = new System.Windows.Forms.ComboBox();
            this.lblCorp = new DevExpress.XtraEditors.LabelControl();
            this.pnlCorp = new System.Windows.Forms.Panel();
            this.cboCorp = new System.Windows.Forms.ComboBox();
            this.lblUserId = new DevExpress.XtraEditors.LabelControl();
            this.txtUserId = new System.Windows.Forms.TextBox();
            this.lblPassword = new DevExpress.XtraEditors.LabelControl();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.chkRememberId = new System.Windows.Forms.CheckBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.lblMessage = new DevExpress.XtraEditors.LabelControl();
            this.pnlLanguage.SuspendLayout();
            this.pnlCorp.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.White;
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Tomato;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnClose.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(85)))), ((int)(((byte)(99)))));
            this.btnClose.Location = new System.Drawing.Point(1152, 12);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(36, 32);
            this.btnClose.TabIndex = 9;
            this.btnClose.TabStop = false;
            this.btnClose.Text = "✕";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lblLanguage
            // 
            this.lblLanguage.Appearance.BackColor = System.Drawing.Color.White;
            this.lblLanguage.Appearance.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblLanguage.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(61)))), ((int)(((byte)(67)))), ((int)(((byte)(84)))));
            this.lblLanguage.Appearance.Options.UseBackColor = true;
            this.lblLanguage.Appearance.Options.UseFont = true;
            this.lblLanguage.Appearance.Options.UseForeColor = true;
            this.lblLanguage.Appearance.Options.UseTextOptions = true;
            this.lblLanguage.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.lblLanguage.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblLanguage.Location = new System.Drawing.Point(716, 194);
            this.lblLanguage.Name = "lblLanguage";
            this.lblLanguage.Size = new System.Drawing.Size(220, 22);
            this.lblLanguage.TabIndex = 10;
            this.lblLanguage.Text = "언어";
            // 
            // pnlLanguage
            // 
            this.pnlLanguage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(251)))), ((int)(((byte)(252)))), ((int)(((byte)(253)))));
            this.pnlLanguage.Controls.Add(this.cboLanguage);
            this.pnlLanguage.Location = new System.Drawing.Point(704, 221);
            this.pnlLanguage.Name = "pnlLanguage";
            this.pnlLanguage.Size = new System.Drawing.Size(378, 30);
            this.pnlLanguage.TabIndex = 0;
            // 
            // cboLanguage
            // 
            this.cboLanguage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(251)))), ((int)(((byte)(252)))), ((int)(((byte)(253)))));
            this.cboLanguage.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cboLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLanguage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cboLanguage.Font = new System.Drawing.Font("맑은 고딕", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.cboLanguage.ItemHeight = 26;
            this.cboLanguage.Location = new System.Drawing.Point(-3, -3);
            this.cboLanguage.Name = "cboLanguage";
            this.cboLanguage.Size = new System.Drawing.Size(408, 32);
            this.cboLanguage.TabIndex = 0;
            this.cboLanguage.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.cboItem_DrawItem);
            this.cboLanguage.SelectedIndexChanged += new System.EventHandler(this.cboLanguage_SelectedIndexChanged);
            // 
            // lblCorp
            // 
            this.lblCorp.Appearance.BackColor = System.Drawing.Color.White;
            this.lblCorp.Appearance.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblCorp.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(61)))), ((int)(((byte)(67)))), ((int)(((byte)(84)))));
            this.lblCorp.Appearance.Options.UseBackColor = true;
            this.lblCorp.Appearance.Options.UseFont = true;
            this.lblCorp.Appearance.Options.UseForeColor = true;
            this.lblCorp.Appearance.Options.UseTextOptions = true;
            this.lblCorp.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.lblCorp.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblCorp.Location = new System.Drawing.Point(716, 270);
            this.lblCorp.Name = "lblCorp";
            this.lblCorp.Size = new System.Drawing.Size(220, 22);
            this.lblCorp.TabIndex = 11;
            this.lblCorp.Text = "사업부";
            // 
            // pnlCorp
            // 
            this.pnlCorp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(251)))), ((int)(((byte)(252)))), ((int)(((byte)(253)))));
            this.pnlCorp.Controls.Add(this.cboCorp);
            this.pnlCorp.Location = new System.Drawing.Point(704, 297);
            this.pnlCorp.Name = "pnlCorp";
            this.pnlCorp.Size = new System.Drawing.Size(378, 30);
            this.pnlCorp.TabIndex = 1;
            // 
            // cboCorp
            // 
            this.cboCorp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(251)))), ((int)(((byte)(252)))), ((int)(((byte)(253)))));
            this.cboCorp.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cboCorp.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCorp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cboCorp.Font = new System.Drawing.Font("맑은 고딕", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.cboCorp.ItemHeight = 26;
            this.cboCorp.Location = new System.Drawing.Point(-3, -3);
            this.cboCorp.Name = "cboCorp";
            this.cboCorp.Size = new System.Drawing.Size(408, 32);
            this.cboCorp.TabIndex = 0;
            this.cboCorp.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.cboItem_DrawItem);
            // 
            // lblUserId
            // 
            this.lblUserId.Appearance.BackColor = System.Drawing.Color.White;
            this.lblUserId.Appearance.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblUserId.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(61)))), ((int)(((byte)(67)))), ((int)(((byte)(84)))));
            this.lblUserId.Appearance.Options.UseBackColor = true;
            this.lblUserId.Appearance.Options.UseFont = true;
            this.lblUserId.Appearance.Options.UseForeColor = true;
            this.lblUserId.Appearance.Options.UseTextOptions = true;
            this.lblUserId.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.lblUserId.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblUserId.Location = new System.Drawing.Point(716, 345);
            this.lblUserId.Name = "lblUserId";
            this.lblUserId.Size = new System.Drawing.Size(220, 22);
            this.lblUserId.TabIndex = 12;
            this.lblUserId.Text = "ID";
            // 
            // txtUserId
            // 
            this.txtUserId.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(251)))), ((int)(((byte)(252)))), ((int)(((byte)(253)))));
            this.txtUserId.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtUserId.Font = new System.Drawing.Font("맑은 고딕", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txtUserId.Location = new System.Drawing.Point(712, 377);
            this.txtUserId.Name = "txtUserId";
            this.txtUserId.Size = new System.Drawing.Size(376, 20);
            this.txtUserId.TabIndex = 2;
            // 
            // lblPassword
            // 
            this.lblPassword.Appearance.BackColor = System.Drawing.Color.White;
            this.lblPassword.Appearance.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblPassword.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(61)))), ((int)(((byte)(67)))), ((int)(((byte)(84)))));
            this.lblPassword.Appearance.Options.UseBackColor = true;
            this.lblPassword.Appearance.Options.UseFont = true;
            this.lblPassword.Appearance.Options.UseForeColor = true;
            this.lblPassword.Appearance.Options.UseTextOptions = true;
            this.lblPassword.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.lblPassword.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblPassword.Location = new System.Drawing.Point(716, 421);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(220, 22);
            this.lblPassword.TabIndex = 13;
            this.lblPassword.Text = "Password";
            // 
            // txtPassword
            // 
            this.txtPassword.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(251)))), ((int)(((byte)(252)))), ((int)(((byte)(253)))));
            this.txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtPassword.Font = new System.Drawing.Font("맑은 고딕", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txtPassword.Location = new System.Drawing.Point(712, 453);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(376, 20);
            this.txtPassword.TabIndex = 3;
            this.txtPassword.UseSystemPasswordChar = true;
            // 
            // chkRememberId
            // 
            this.chkRememberId.BackColor = System.Drawing.Color.White;
            this.chkRememberId.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.chkRememberId.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(136)))), ((int)(((byte)(136)))), ((int)(((byte)(136)))));
            this.chkRememberId.Location = new System.Drawing.Point(699, 502);
            this.chkRememberId.Name = "chkRememberId";
            this.chkRememberId.Size = new System.Drawing.Size(240, 22);
            this.chkRememberId.TabIndex = 4;
            this.chkRememberId.Text = "아이디 저장";
            this.chkRememberId.UseVisualStyleBackColor = false;
            // 
            // btnConnect
            // 
            this.btnConnect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(91)))), ((int)(((byte)(120)))));
            this.btnConnect.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnConnect.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(91)))), ((int)(((byte)(120)))));
            this.btnConnect.FlatAppearance.BorderSize = 0;
            this.btnConnect.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(72)))), ((int)(((byte)(96)))));
            this.btnConnect.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(91)))), ((int)(((byte)(120)))));
            this.btnConnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConnect.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnConnect.ForeColor = System.Drawing.Color.White;
            this.btnConnect.Location = new System.Drawing.Point(703, 532);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(394, 49);
            this.btnConnect.TabIndex = 5;
            this.btnConnect.Text = "접속";
            this.btnConnect.UseVisualStyleBackColor = false;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // lblMessage
            // 
            this.lblMessage.Appearance.ForeColor = System.Drawing.Color.Firebrick;
            this.lblMessage.Appearance.Options.UseForeColor = true;
            this.lblMessage.Appearance.Options.UseTextOptions = true;
            this.lblMessage.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.lblMessage.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
            this.lblMessage.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.lblMessage.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblMessage.Location = new System.Drawing.Point(700, 592);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(400, 44);
            this.lblMessage.TabIndex = 14;
            this.lblMessage.Text = "Error";
            // 
            // LoginForm
            // 
            this.AcceptButton = this.btnConnect;
            this.Appearance.Options.UseFont = true;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackgroundImageLayoutStore = System.Windows.Forms.ImageLayout.Tile;
            this.BackgroundImageStore = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImageStore")));
            this.ClientSize = new System.Drawing.Size(1200, 650);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lblLanguage);
            this.Controls.Add(this.pnlLanguage);
            this.Controls.Add(this.lblCorp);
            this.Controls.Add(this.pnlCorp);
            this.Controls.Add(this.lblUserId);
            this.Controls.Add(this.txtUserId);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.chkRememberId);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.lblMessage);
            this.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.IconOptions.Image = ((System.Drawing.Image)(resources.GetObject("LoginForm.IconOptions.Image")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LoginForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TMES POP 로그인";
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LoginForm_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.LoginForm_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.LoginForm_MouseUp);
            this.pnlLanguage.ResumeLayout(false);
            this.pnlCorp.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnClose;
        private DevExpress.XtraEditors.LabelControl lblLanguage;
        private System.Windows.Forms.Panel pnlLanguage;
        private System.Windows.Forms.ComboBox cboLanguage;
        private DevExpress.XtraEditors.LabelControl lblCorp;
        private System.Windows.Forms.Panel pnlCorp;
        private System.Windows.Forms.ComboBox cboCorp;
        private DevExpress.XtraEditors.LabelControl lblUserId;
        private System.Windows.Forms.TextBox txtUserId;
        private DevExpress.XtraEditors.LabelControl lblPassword;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.CheckBox chkRememberId;
        private System.Windows.Forms.Button btnConnect;
        private DevExpress.XtraEditors.LabelControl lblMessage;
    }
}
