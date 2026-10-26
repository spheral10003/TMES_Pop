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
        /// 색·폰트·아이콘·문구는 AdminAlarmForm.cs의 Initialize()에서 런타임에 넣는다.
        ///
        /// 일반 .NET Form — Windows 제목 줄은 없애고(FormBorderStyle.None) pnlTitle + lblTitle이 제목 줄 역할을 한다.
        /// 구성: 제목 띠 + 조회 조건(알람 구분/등급/알람 명 + [조회]) + 알람 타일 + 비고 + 하단 [닫기][등록].
        /// 도킹 순서 주의: pnlBody(Fill) → pnlFooter(Bottom) → pnlSearch(Top) → pnlTitle(Top).
        ///                 pnlBody 안은 tileAlarms(Fill) → pnlRemark(Bottom) → lblListTitle(Top).
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlTitle = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pnlSearch = new System.Windows.Forms.Panel();
            this.lblType = new System.Windows.Forms.Label();
            this.cboType = new DevExpress.XtraEditors.ComboBoxEdit();
            this.lblGrade = new System.Windows.Forms.Label();
            this.cboGrade = new DevExpress.XtraEditors.ComboBoxEdit();
            this.lblName = new System.Windows.Forms.Label();
            this.txtName = new DevExpress.XtraEditors.TextEdit();
            this.btnSearch = new SEOJIN_TMES.Client.Forms.Common.MeisButton();
            this.pnlBody = new System.Windows.Forms.Panel();
            this.tileAlarms = new DevExpress.XtraEditors.TileControl();
            this.tileGroupAlarms = new DevExpress.XtraEditors.TileGroup();
            this.pnlRemark = new System.Windows.Forms.Panel();
            this.txtRemark = new DevExpress.XtraEditors.MemoEdit();
            this.lblRemark = new System.Windows.Forms.Label();
            this.lblListTitle = new System.Windows.Forms.Label();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.btnSend = new SEOJIN_TMES.Client.Forms.Common.MeisButton();
            this.btnClose = new SEOJIN_TMES.Client.Forms.Common.MeisButton();
            this.pnlTitle.SuspendLayout();
            this.pnlSearch.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboType.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboGrade.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtName.Properties)).BeginInit();
            this.pnlBody.SuspendLayout();
            this.pnlRemark.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtRemark.Properties)).BeginInit();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlTitle
            //
            this.pnlTitle.Controls.Add(this.lblTitle);
            this.pnlTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTitle.Location = new System.Drawing.Point(0, 0);
            this.pnlTitle.Name = "pnlTitle";
            this.pnlTitle.Size = new System.Drawing.Size(900, 48);
            this.pnlTitle.TabIndex = 0;
            //
            // lblTitle
            //
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Padding = new System.Windows.Forms.Padding(16, 0, 0, 0);
            this.lblTitle.Size = new System.Drawing.Size(900, 48);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "알람 조회 / 등록";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // pnlSearch
            //
            this.pnlSearch.Controls.Add(this.lblType);
            this.pnlSearch.Controls.Add(this.cboType);
            this.pnlSearch.Controls.Add(this.lblGrade);
            this.pnlSearch.Controls.Add(this.cboGrade);
            this.pnlSearch.Controls.Add(this.lblName);
            this.pnlSearch.Controls.Add(this.txtName);
            this.pnlSearch.Controls.Add(this.btnSearch);
            this.pnlSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSearch.Location = new System.Drawing.Point(0, 48);
            this.pnlSearch.Name = "pnlSearch";
            this.pnlSearch.Size = new System.Drawing.Size(900, 60);
            this.pnlSearch.TabIndex = 1;
            //
            // lblType
            //
            this.lblType.Location = new System.Drawing.Point(16, 18);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(76, 24);
            this.lblType.TabIndex = 0;
            this.lblType.Text = "알람 구분";
            this.lblType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // cboType
            //
            this.cboType.Location = new System.Drawing.Point(94, 18);
            this.cboType.Name = "cboType";
            this.cboType.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboType.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cboType.Size = new System.Drawing.Size(140, 24);
            this.cboType.TabIndex = 1;
            //
            // lblGrade
            //
            this.lblGrade.Location = new System.Drawing.Point(252, 18);
            this.lblGrade.Name = "lblGrade";
            this.lblGrade.Size = new System.Drawing.Size(76, 24);
            this.lblGrade.TabIndex = 2;
            this.lblGrade.Text = "알람 등급";
            this.lblGrade.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // cboGrade
            //
            this.cboGrade.Location = new System.Drawing.Point(330, 18);
            this.cboGrade.Name = "cboGrade";
            this.cboGrade.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboGrade.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cboGrade.Size = new System.Drawing.Size(120, 24);
            this.cboGrade.TabIndex = 3;
            //
            // lblName
            //
            this.lblName.Location = new System.Drawing.Point(468, 18);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(64, 24);
            this.lblName.TabIndex = 4;
            this.lblName.Text = "알람 명";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // txtName
            //
            this.txtName.Location = new System.Drawing.Point(534, 18);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(220, 24);
            this.txtName.TabIndex = 5;
            this.txtName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtName_KeyDown);
            //
            // btnSearch
            //
            this.btnSearch.Accent = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(70)))), ((int)(((byte)(229)))));
            this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSearch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSearch.Icon = null;
            this.btnSearch.Location = new System.Drawing.Point(788, 14);
            this.btnSearch.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.PaintStyle = DevExpress.XtraEditors.Controls.PaintStyles.Light;
            this.btnSearch.Selected = false;
            this.btnSearch.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            this.btnSearch.Size = new System.Drawing.Size(96, 32);
            this.btnSearch.TabIndex = 6;
            this.btnSearch.Text = "조회";
            this.btnSearch.TextAfterIcon = false;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            //
            // pnlBody
            //
            this.pnlBody.Controls.Add(this.tileAlarms);
            this.pnlBody.Controls.Add(this.pnlRemark);
            this.pnlBody.Controls.Add(this.lblListTitle);
            this.pnlBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBody.Location = new System.Drawing.Point(0, 108);
            this.pnlBody.Name = "pnlBody";
            this.pnlBody.Padding = new System.Windows.Forms.Padding(16, 0, 16, 8);
            this.pnlBody.Size = new System.Drawing.Size(900, 468);
            this.pnlBody.TabIndex = 2;
            //
            // tileAlarms
            //
            this.tileAlarms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tileAlarms.Groups.Add(this.tileGroupAlarms);
            this.tileAlarms.Location = new System.Drawing.Point(16, 32);
            this.tileAlarms.Name = "tileAlarms";
            this.tileAlarms.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tileAlarms.ScrollMode = DevExpress.XtraEditors.TileControlScrollMode.ScrollBar;
            this.tileAlarms.Size = new System.Drawing.Size(868, 324);
            this.tileAlarms.TabIndex = 1;
            this.tileAlarms.ItemClick += new DevExpress.XtraEditors.TileItemClickEventHandler(this.tileAlarms_ItemClick);
            //
            // tileGroupAlarms
            //
            this.tileGroupAlarms.Name = "tileGroupAlarms";
            //
            // pnlRemark
            //
            this.pnlRemark.Controls.Add(this.txtRemark);
            this.pnlRemark.Controls.Add(this.lblRemark);
            this.pnlRemark.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlRemark.Location = new System.Drawing.Point(16, 356);
            this.pnlRemark.Name = "pnlRemark";
            this.pnlRemark.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.pnlRemark.Size = new System.Drawing.Size(868, 104);
            this.pnlRemark.TabIndex = 2;
            //
            // txtRemark
            //
            this.txtRemark.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtRemark.Location = new System.Drawing.Point(0, 32);
            this.txtRemark.Name = "txtRemark";
            this.txtRemark.Properties.MaxLength = 1000;
            this.txtRemark.Size = new System.Drawing.Size(868, 72);
            this.txtRemark.TabIndex = 1;
            //
            // lblRemark
            //
            this.lblRemark.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblRemark.Location = new System.Drawing.Point(0, 8);
            this.lblRemark.Name = "lblRemark";
            this.lblRemark.Size = new System.Drawing.Size(868, 24);
            this.lblRemark.TabIndex = 0;
            this.lblRemark.Text = "비고";
            this.lblRemark.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblListTitle
            //
            this.lblListTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblListTitle.Location = new System.Drawing.Point(16, 0);
            this.lblListTitle.Name = "lblListTitle";
            this.lblListTitle.Size = new System.Drawing.Size(868, 32);
            this.lblListTitle.TabIndex = 0;
            this.lblListTitle.Text = "알람 코드 / 조치 내용";
            this.lblListTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // pnlFooter
            //
            this.pnlFooter.Controls.Add(this.btnSend);
            this.pnlFooter.Controls.Add(this.btnClose);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 576);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Size = new System.Drawing.Size(900, 64);
            this.pnlFooter.TabIndex = 3;
            //
            // btnSend
            //
            this.btnSend.Accent = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(70)))), ((int)(((byte)(229)))));
            this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSend.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSend.Icon = null;
            this.btnSend.Location = new System.Drawing.Point(772, 14);
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
            this.btnClose.Location = new System.Drawing.Point(652, 14);
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
            this.ClientSize = new System.Drawing.Size(900, 640);
            this.Controls.Add(this.pnlBody);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.pnlSearch);
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
            this.pnlSearch.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cboType.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboGrade.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtName.Properties)).EndInit();
            this.pnlBody.ResumeLayout(false);
            this.pnlRemark.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtRemark.Properties)).EndInit();
            this.pnlFooter.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlTitle;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel pnlSearch;
        private System.Windows.Forms.Label lblType;
        private DevExpress.XtraEditors.ComboBoxEdit cboType;
        private System.Windows.Forms.Label lblGrade;
        private DevExpress.XtraEditors.ComboBoxEdit cboGrade;
        private System.Windows.Forms.Label lblName;
        private DevExpress.XtraEditors.TextEdit txtName;
        private SEOJIN_TMES.Client.Forms.Common.MeisButton btnSearch;
        private System.Windows.Forms.Panel pnlBody;
        private DevExpress.XtraEditors.TileControl tileAlarms;
        private DevExpress.XtraEditors.TileGroup tileGroupAlarms;
        private System.Windows.Forms.Panel pnlRemark;
        private DevExpress.XtraEditors.MemoEdit txtRemark;
        private System.Windows.Forms.Label lblRemark;
        private System.Windows.Forms.Label lblListTitle;
        private System.Windows.Forms.Panel pnlFooter;
        private SEOJIN_TMES.Client.Forms.Common.MeisButton btnSend;
        private SEOJIN_TMES.Client.Forms.Common.MeisButton btnClose;
    }
}
