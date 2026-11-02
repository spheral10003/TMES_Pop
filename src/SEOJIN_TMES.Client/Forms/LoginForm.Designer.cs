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
        /// 배경 그림(Images\login_TEST.png · login_PROD.png, 880×560)의 칸 좌표에 맞춰 컨트롤을 얹는다 — 도킹을 쓰지 않는다.
        /// 그림 좌표(px): 제목 줄 y=0~36 / 입력 칸 x=451~834, 언어 y=129~162 · 사업부 173~206 · ID 217~250 · Password 261~294,
        /// 아이디 저장 y=308~320, 오류 상자 x=335~834 y=338~411, LOGIN x=335~662 · EXIT x=671~834 (y=448~489),
        /// 키 안내 y=502~513, 상태 줄 y=533~558. 항목 이름 글자는 x=335부터.
        /// 그림을 바꾸면 이 좌표도 함께 다시 잴 것.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginForm));
            this.pnlTitle = new System.Windows.Forms.Panel();
            this.picTitleIcon = new System.Windows.Forms.PictureBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblBadge = new System.Windows.Forms.Label();
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
            this.pnlError = new System.Windows.Forms.Panel();
            this.lblMessage = new DevExpress.XtraEditors.LabelControl();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.lblKeyHint = new System.Windows.Forms.Label();
            this.pnlStatus = new System.Windows.Forms.Panel();
            this.lblNetworkDot = new System.Windows.Forms.Label();
            this.lblNetwork = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.pnlTitle.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picTitleIcon)).BeginInit();
            this.pnlLanguage.SuspendLayout();
            this.pnlCorp.SuspendLayout();
            this.pnlError.SuspendLayout();
            this.pnlStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlTitle
            // 
            this.pnlTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(58)))), ((int)(((byte)(82)))));
            this.pnlTitle.Controls.Add(this.picTitleIcon);
            this.pnlTitle.Controls.Add(this.lblTitle);
            this.pnlTitle.Controls.Add(this.lblBadge);
            this.pnlTitle.Controls.Add(this.btnClose);
            this.pnlTitle.Location = new System.Drawing.Point(0, 0);
            this.pnlTitle.Name = "pnlTitle";
            this.pnlTitle.Size = new System.Drawing.Size(880, 37);
            this.pnlTitle.TabIndex = 20;
            this.pnlTitle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LoginForm_MouseDown);
            this.pnlTitle.MouseMove += new System.Windows.Forms.MouseEventHandler(this.LoginForm_MouseMove);
            this.pnlTitle.MouseUp += new System.Windows.Forms.MouseEventHandler(this.LoginForm_MouseUp);
            // 
            // picTitleIcon
            // 
            this.picTitleIcon.Location = new System.Drawing.Point(11, 9);
            this.picTitleIcon.Name = "picTitleIcon";
            this.picTitleIcon.Size = new System.Drawing.Size(20, 20);
            this.picTitleIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picTitleIcon.TabIndex = 0;
            this.picTitleIcon.TabStop = false;
            // 
            // lblTitle
            // 
            this.lblTitle.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(36, 7);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(91, 24);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "TMES Client";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblTitle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LoginForm_MouseDown);
            this.lblTitle.MouseMove += new System.Windows.Forms.MouseEventHandler(this.LoginForm_MouseMove);
            this.lblTitle.MouseUp += new System.Windows.Forms.MouseEventHandler(this.LoginForm_MouseUp);
            // 
            // lblBadge
            // 
            this.lblBadge.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(194)))), ((int)(((byte)(65)))), ((int)(((byte)(12)))));
            this.lblBadge.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblBadge.ForeColor = System.Drawing.Color.White;
            this.lblBadge.Location = new System.Drawing.Point(127, 9);
            this.lblBadge.Name = "lblBadge";
            this.lblBadge.Size = new System.Drawing.Size(50, 20);
            this.lblBadge.TabIndex = 2;
            this.lblBadge.Text = "TEST";
            this.lblBadge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(58)))), ((int)(((byte)(82)))));
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(43)))), ((int)(((byte)(28)))));
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("맑은 고딕", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(834, 0);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(46, 37);
            this.btnClose.TabIndex = 3;
            this.btnClose.TabStop = false;
            this.btnClose.Text = "✕";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lblLanguage
            // 
            this.lblLanguage.Appearance.BackColor = System.Drawing.Color.White;
            this.lblLanguage.Appearance.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblLanguage.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(55)))), ((int)(((byte)(67)))));
            this.lblLanguage.Appearance.Options.UseBackColor = true;
            this.lblLanguage.Appearance.Options.UseFont = true;
            this.lblLanguage.Appearance.Options.UseForeColor = true;
            this.lblLanguage.Appearance.Options.UseTextOptions = true;
            this.lblLanguage.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.lblLanguage.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblLanguage.Location = new System.Drawing.Point(333, 133);
            this.lblLanguage.Name = "lblLanguage";
            this.lblLanguage.Size = new System.Drawing.Size(114, 26);
            this.lblLanguage.TabIndex = 10;
            this.lblLanguage.Text = "언어";
            // 
            // pnlLanguage
            // 
            this.pnlLanguage.BackColor = System.Drawing.Color.White;
            this.pnlLanguage.Controls.Add(this.cboLanguage);
            this.pnlLanguage.Location = new System.Drawing.Point(456, 133);
            this.pnlLanguage.Name = "pnlLanguage";
            this.pnlLanguage.Size = new System.Drawing.Size(360, 26);
            this.pnlLanguage.TabIndex = 0;
            // 
            // cboLanguage
            // 
            this.cboLanguage.BackColor = System.Drawing.Color.White;
            this.cboLanguage.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cboLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLanguage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cboLanguage.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.cboLanguage.ItemHeight = 24;
            this.cboLanguage.Location = new System.Drawing.Point(-3, -3);
            this.cboLanguage.Name = "cboLanguage";
            this.cboLanguage.Size = new System.Drawing.Size(390, 30);
            this.cboLanguage.TabIndex = 0;
            this.cboLanguage.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.cboItem_DrawItem);
            this.cboLanguage.SelectedIndexChanged += new System.EventHandler(this.cboLanguage_SelectedIndexChanged);
            // 
            // lblCorp
            // 
            this.lblCorp.Appearance.BackColor = System.Drawing.Color.White;
            this.lblCorp.Appearance.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblCorp.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(55)))), ((int)(((byte)(67)))));
            this.lblCorp.Appearance.Options.UseBackColor = true;
            this.lblCorp.Appearance.Options.UseFont = true;
            this.lblCorp.Appearance.Options.UseForeColor = true;
            this.lblCorp.Appearance.Options.UseTextOptions = true;
            this.lblCorp.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.lblCorp.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblCorp.Location = new System.Drawing.Point(333, 177);
            this.lblCorp.Name = "lblCorp";
            this.lblCorp.Size = new System.Drawing.Size(114, 26);
            this.lblCorp.TabIndex = 11;
            this.lblCorp.Text = "사업부";
            // 
            // pnlCorp
            // 
            this.pnlCorp.BackColor = System.Drawing.Color.White;
            this.pnlCorp.Controls.Add(this.cboCorp);
            this.pnlCorp.Location = new System.Drawing.Point(456, 177);
            this.pnlCorp.Name = "pnlCorp";
            this.pnlCorp.Size = new System.Drawing.Size(360, 26);
            this.pnlCorp.TabIndex = 1;
            // 
            // cboCorp
            // 
            this.cboCorp.BackColor = System.Drawing.Color.White;
            this.cboCorp.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cboCorp.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCorp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cboCorp.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.cboCorp.ItemHeight = 24;
            this.cboCorp.Location = new System.Drawing.Point(-3, -3);
            this.cboCorp.Name = "cboCorp";
            this.cboCorp.Size = new System.Drawing.Size(390, 30);
            this.cboCorp.TabIndex = 0;
            this.cboCorp.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.cboItem_DrawItem);
            // 
            // lblUserId
            // 
            this.lblUserId.Appearance.BackColor = System.Drawing.Color.White;
            this.lblUserId.Appearance.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblUserId.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(55)))), ((int)(((byte)(67)))));
            this.lblUserId.Appearance.Options.UseBackColor = true;
            this.lblUserId.Appearance.Options.UseFont = true;
            this.lblUserId.Appearance.Options.UseForeColor = true;
            this.lblUserId.Appearance.Options.UseTextOptions = true;
            this.lblUserId.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.lblUserId.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblUserId.Location = new System.Drawing.Point(333, 221);
            this.lblUserId.Name = "lblUserId";
            this.lblUserId.Size = new System.Drawing.Size(114, 26);
            this.lblUserId.TabIndex = 12;
            this.lblUserId.Text = "ID";
            // 
            // txtUserId
            // 
            this.txtUserId.BackColor = System.Drawing.Color.White;
            this.txtUserId.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtUserId.Font = new System.Drawing.Font("맑은 고딕", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txtUserId.Location = new System.Drawing.Point(462, 224);
            this.txtUserId.Name = "txtUserId";
            this.txtUserId.Size = new System.Drawing.Size(366, 19);
            this.txtUserId.TabIndex = 2;
            // 
            // lblPassword
            // 
            this.lblPassword.Appearance.BackColor = System.Drawing.Color.White;
            this.lblPassword.Appearance.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblPassword.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(55)))), ((int)(((byte)(67)))));
            this.lblPassword.Appearance.Options.UseBackColor = true;
            this.lblPassword.Appearance.Options.UseFont = true;
            this.lblPassword.Appearance.Options.UseForeColor = true;
            this.lblPassword.Appearance.Options.UseTextOptions = true;
            this.lblPassword.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.lblPassword.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblPassword.Location = new System.Drawing.Point(333, 265);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(114, 26);
            this.lblPassword.TabIndex = 13;
            this.lblPassword.Text = "Password";
            // 
            // txtPassword
            // 
            this.txtPassword.BackColor = System.Drawing.Color.White;
            this.txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtPassword.Font = new System.Drawing.Font("맑은 고딕", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txtPassword.Location = new System.Drawing.Point(462, 268);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(366, 19);
            this.txtPassword.TabIndex = 3;
            this.txtPassword.UseSystemPasswordChar = true;
            // 
            // chkRememberId
            // 
            this.chkRememberId.BackColor = System.Drawing.Color.White;
            this.chkRememberId.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.chkRememberId.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(71)))), ((int)(((byte)(84)))));
            this.chkRememberId.Location = new System.Drawing.Point(450, 303);
            this.chkRememberId.Name = "chkRememberId";
            this.chkRememberId.Size = new System.Drawing.Size(300, 23);
            this.chkRememberId.TabIndex = 4;
            this.chkRememberId.Text = "아이디 저장";
            this.chkRememberId.UseVisualStyleBackColor = false;
            // 
            // pnlError
            // 
            this.pnlError.BackColor = System.Drawing.Color.White;
            this.pnlError.Controls.Add(this.lblMessage);
            this.pnlError.Location = new System.Drawing.Point(335, 338);
            this.pnlError.Name = "pnlError";
            this.pnlError.Size = new System.Drawing.Size(500, 74);
            this.pnlError.TabIndex = 14;
            this.pnlError.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlError_Paint);
            // 
            // lblMessage
            // 
            this.lblMessage.Appearance.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblMessage.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(41)))), ((int)(((byte)(20)))));
            this.lblMessage.Appearance.Options.UseFont = true;
            this.lblMessage.Appearance.Options.UseForeColor = true;
            this.lblMessage.Appearance.Options.UseTextOptions = true;
            this.lblMessage.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
            this.lblMessage.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.lblMessage.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblMessage.Location = new System.Drawing.Point(12, 11);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(476, 54);
            this.lblMessage.TabIndex = 0;
            this.lblMessage.Text = "Error";
            this.lblMessage.TextChanged += new System.EventHandler(this.lblMessage_TextChanged);
            // 
            // btnConnect
            // 
            this.btnConnect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(93)))), ((int)(((byte)(123)))));
            this.btnConnect.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnConnect.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(93)))), ((int)(((byte)(123)))));
            this.btnConnect.FlatAppearance.BorderSize = 0;
            this.btnConnect.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(72)))), ((int)(((byte)(96)))));
            this.btnConnect.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(93)))), ((int)(((byte)(123)))));
            this.btnConnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConnect.Font = new System.Drawing.Font("맑은 고딕", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnConnect.ForeColor = System.Drawing.Color.White;
            this.btnConnect.Location = new System.Drawing.Point(335, 448);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(328, 42);
            this.btnConnect.TabIndex = 5;
            this.btnConnect.Text = "접속";
            this.btnConnect.UseVisualStyleBackColor = false;
            this.btnConnect.EnabledChanged += new System.EventHandler(this.btnConnect_EnabledChanged);
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnExit
            // 
            this.btnExit.BackColor = System.Drawing.Color.White;
            this.btnExit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExit.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnExit.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(169)))), ((int)(((byte)(182)))), ((int)(((byte)(194)))));
            this.btnExit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(242)))), ((int)(((byte)(246)))));
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExit.Font = new System.Drawing.Font("맑은 고딕", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnExit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(55)))), ((int)(((byte)(67)))));
            this.btnExit.Location = new System.Drawing.Point(671, 448);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(164, 42);
            this.btnExit.TabIndex = 6;
            this.btnExit.Text = "종료";
            this.btnExit.UseVisualStyleBackColor = false;
            this.btnExit.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lblKeyHint
            // 
            this.lblKeyHint.BackColor = System.Drawing.Color.White;
            this.lblKeyHint.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblKeyHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(101)))), ((int)(((byte)(104)))), ((int)(((byte)(117)))));
            this.lblKeyHint.Location = new System.Drawing.Point(335, 497);
            this.lblKeyHint.Name = "lblKeyHint";
            this.lblKeyHint.Size = new System.Drawing.Size(500, 21);
            this.lblKeyHint.TabIndex = 15;
            this.lblKeyHint.Text = "Enter: 로그인 · Esc: 종료";
            this.lblKeyHint.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlStatus
            // 
            this.pnlStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(242)))), ((int)(((byte)(246)))));
            this.pnlStatus.Controls.Add(this.lblNetworkDot);
            this.pnlStatus.Controls.Add(this.lblNetwork);
            this.pnlStatus.Controls.Add(this.lblVersion);
            this.pnlStatus.Location = new System.Drawing.Point(1, 533);
            this.pnlStatus.Name = "pnlStatus";
            this.pnlStatus.Size = new System.Drawing.Size(878, 26);
            this.pnlStatus.TabIndex = 16;
            // 
            // lblNetworkDot
            // 
            this.lblNetworkDot.Font = new System.Drawing.Font("맑은 고딕", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblNetworkDot.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(158)))), ((int)(((byte)(79)))));
            this.lblNetworkDot.Location = new System.Drawing.Point(8, 3);
            this.lblNetworkDot.Name = "lblNetworkDot";
            this.lblNetworkDot.Size = new System.Drawing.Size(16, 20);
            this.lblNetworkDot.TabIndex = 0;
            this.lblNetworkDot.Text = "●";
            this.lblNetworkDot.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblNetwork
            // 
            this.lblNetwork.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblNetwork.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(71)))), ((int)(((byte)(84)))));
            this.lblNetwork.Location = new System.Drawing.Point(24, 3);
            this.lblNetwork.Name = "lblNetwork";
            this.lblNetwork.Size = new System.Drawing.Size(300, 20);
            this.lblNetwork.TabIndex = 1;
            this.lblNetwork.Text = "온라인";
            this.lblNetwork.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblVersion
            // 
            this.lblVersion.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblVersion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(71)))), ((int)(((byte)(84)))));
            this.lblVersion.Location = new System.Drawing.Point(478, 3);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(388, 20);
            this.lblVersion.TabIndex = 2;
            this.lblVersion.Text = "Ver. 1.0.0.0";
            this.lblVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // LoginForm
            // 
            this.AcceptButton = this.btnConnect;
            this.Appearance.Options.UseFont = true;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackgroundImageLayoutStore = System.Windows.Forms.ImageLayout.Tile;
            this.BackgroundImageStore = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImageStore")));
            this.CancelButton = this.btnExit;
            this.ClientSize = new System.Drawing.Size(880, 560);
            this.Controls.Add(this.pnlTitle);
            this.Controls.Add(this.lblLanguage);
            this.Controls.Add(this.pnlLanguage);
            this.Controls.Add(this.lblCorp);
            this.Controls.Add(this.pnlCorp);
            this.Controls.Add(this.lblUserId);
            this.Controls.Add(this.txtUserId);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.chkRememberId);
            this.Controls.Add(this.pnlError);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.lblKeyHint);
            this.Controls.Add(this.pnlStatus);
            this.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.IconOptions.Image = ((System.Drawing.Image)(resources.GetObject("LoginForm.IconOptions.Image")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LoginForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TMES Client";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.LoginForm_FormClosed);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LoginForm_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.LoginForm_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.LoginForm_MouseUp);
            this.pnlTitle.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picTitleIcon)).EndInit();
            this.pnlLanguage.ResumeLayout(false);
            this.pnlCorp.ResumeLayout(false);
            this.pnlError.ResumeLayout(false);
            this.pnlStatus.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel pnlTitle;
        private System.Windows.Forms.PictureBox picTitleIcon;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblBadge;
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
        private System.Windows.Forms.Panel pnlError;
        private DevExpress.XtraEditors.LabelControl lblMessage;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Label lblKeyHint;
        private System.Windows.Forms.Panel pnlStatus;
        private System.Windows.Forms.Label lblNetworkDot;
        private System.Windows.Forms.Label lblNetwork;
        private System.Windows.Forms.Label lblVersion;
    }
}
