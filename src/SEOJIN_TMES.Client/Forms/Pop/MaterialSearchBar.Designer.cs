namespace SEOJIN_TMES.Client.Forms.Pop
{
    partial class MaterialSearchBar
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

        #region Component Designer generated code

        /// <summary>
        /// 디자이너 지원 메서드. 람다식/삼항연산자/문자열보간/객체이니셜라이저/커스텀 static 호출 사용 금지.
        /// 색·폰트·문구는 MaterialSearchBar.cs의 Initialize() / ApplyTheme()에서 런타임에 넣는다.
        ///
        /// 구성(왼쪽부터): · 투입자재 코드 [입력] [검색] — 그리드 바로 위에 Dock=Top으로 붙여 쓴다.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblMaterialCode = new DevExpress.XtraEditors.LabelControl();
            this.txtMaterialCode = new DevExpress.XtraEditors.TextEdit();
            this.btnSearch = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.txtMaterialCode.Properties)).BeginInit();
            this.SuspendLayout();
            //
            // lblMaterialCode
            //
            this.lblMaterialCode.Location = new System.Drawing.Point(16, 14);
            this.lblMaterialCode.Name = "lblMaterialCode";
            this.lblMaterialCode.Size = new System.Drawing.Size(90, 17);
            this.lblMaterialCode.TabIndex = 0;
            this.lblMaterialCode.Text = "· 투입자재 코드";
            //
            // txtMaterialCode
            //
            this.txtMaterialCode.Location = new System.Drawing.Point(130, 10);
            this.txtMaterialCode.Name = "txtMaterialCode";
            this.txtMaterialCode.Size = new System.Drawing.Size(220, 24);
            this.txtMaterialCode.TabIndex = 1;
            this.txtMaterialCode.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtCode_KeyDown);
            //
            // btnSearch
            //
            this.btnSearch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSearch.Location = new System.Drawing.Point(358, 6);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(96, 32);
            this.btnSearch.TabIndex = 2;
            this.btnSearch.Text = "검색";
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            //
            // MaterialSearchBar
            //
            this.Controls.Add(this.lblMaterialCode);
            this.Controls.Add(this.txtMaterialCode);
            this.Controls.Add(this.btnSearch);
            this.Name = "MaterialSearchBar";
            this.Size = new System.Drawing.Size(1142, 44);
            ((System.ComponentModel.ISupportInitialize)(this.txtMaterialCode.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.LabelControl lblMaterialCode;
        private DevExpress.XtraEditors.TextEdit txtMaterialCode;
        private System.Windows.Forms.Button btnSearch;
    }
}
