namespace SEOJIN_TMES.Client.Forms.Pop
{
    partial class FileUploadForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.pnlCenter = new System.Windows.Forms.Panel();
            this.listFileName = new System.Windows.Forms.ListBox();
            this.picPreView = new System.Windows.Forms.PictureBox();
            this.pnlTitle = new System.Windows.Forms.Panel();
            this.lblClose = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.btnFileOpen = new System.Windows.Forms.Button();
            this.btnCancle = new System.Windows.Forms.Button();
            this.btnUpload = new System.Windows.Forms.Button();
            this.tlpMain.SuspendLayout();
            this.pnlCenter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreView)).BeginInit();
            this.pnlTitle.SuspendLayout();
            this.pnlBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpMain
            // 
            this.tlpMain.ColumnCount = 1;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.pnlCenter, 0, 1);
            this.tlpMain.Controls.Add(this.pnlTitle, 0, 0);
            this.tlpMain.Controls.Add(this.pnlBottom, 0, 2);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 3;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 8F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 77F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tlpMain.Size = new System.Drawing.Size(648, 442);
            this.tlpMain.TabIndex = 1;
            // 
            // pnlCenter
            // 
            this.pnlCenter.BackColor = System.Drawing.Color.Lavender;
            this.pnlCenter.Controls.Add(this.listFileName);
            this.pnlCenter.Controls.Add(this.picPreView);
            this.pnlCenter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCenter.Location = new System.Drawing.Point(1, 35);
            this.pnlCenter.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.pnlCenter.Name = "pnlCenter";
            this.pnlCenter.Size = new System.Drawing.Size(646, 340);
            this.pnlCenter.TabIndex = 4;
            // 
            // listFileName
            // 
            this.listFileName.BackColor = System.Drawing.Color.WhiteSmoke;
            this.listFileName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listFileName.Font = new System.Drawing.Font("Noto Sans KR Medium", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.listFileName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.listFileName.FormattingEnabled = true;
            this.listFileName.ItemHeight = 19;
            this.listFileName.Location = new System.Drawing.Point(390, 14);
            this.listFileName.Name = "listFileName";
            this.listFileName.Size = new System.Drawing.Size(227, 306);
            this.listFileName.TabIndex = 1;
            this.listFileName.SelectedIndexChanged += new System.EventHandler(this.listFileName_SelectedIndexChanged);
            // 
            // picPreView
            // 
            this.picPreView.BackColor = System.Drawing.Color.WhiteSmoke;
            this.picPreView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picPreView.Location = new System.Drawing.Point(25, 11);
            this.picPreView.Name = "picPreView";
            this.picPreView.Size = new System.Drawing.Size(341, 314);
            this.picPreView.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picPreView.TabIndex = 0;
            this.picPreView.TabStop = false;
            // 
            // pnlTitle
            // 
            this.pnlTitle.BackColor = System.Drawing.Color.Transparent;
            this.pnlTitle.Controls.Add(this.lblClose);
            this.pnlTitle.Controls.Add(this.lblTitle);
            this.pnlTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTitle.Location = new System.Drawing.Point(1, 1);
            this.pnlTitle.Margin = new System.Windows.Forms.Padding(1, 1, 1, 0);
            this.pnlTitle.Name = "pnlTitle";
            this.pnlTitle.Size = new System.Drawing.Size(646, 34);
            this.pnlTitle.TabIndex = 2;
            // 
            // lblClose
            // 
            this.lblClose.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblClose.AutoSize = true;
            this.lblClose.BackColor = System.Drawing.Color.SteelBlue;
            this.lblClose.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblClose.Font = new System.Drawing.Font("Noto Sans KR", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblClose.ForeColor = System.Drawing.Color.White;
            this.lblClose.Location = new System.Drawing.Point(616, 0);
            this.lblClose.Name = "lblClose";
            this.lblClose.Size = new System.Drawing.Size(26, 29);
            this.lblClose.TabIndex = 3;
            this.lblClose.Text = "X";
            this.lblClose.Click += new System.EventHandler(this.Close_Click);
            // 
            // lblTitle
            // 
            this.lblTitle.BackColor = System.Drawing.Color.SteelBlue;
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.lblTitle.Font = new System.Drawing.Font("Noto Sans KR Medium", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(646, 34);
            this.lblTitle.TabIndex = 2;
            this.lblTitle.Text = "File Upload";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblTitle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblTitle_MouseDown);
            this.lblTitle.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lblTitle_MouseMove);
            // 
            // pnlBottom
            // 
            this.pnlBottom.BackColor = System.Drawing.Color.Lavender;
            this.pnlBottom.Controls.Add(this.btnFileOpen);
            this.pnlBottom.Controls.Add(this.btnCancle);
            this.pnlBottom.Controls.Add(this.btnUpload);
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBottom.Location = new System.Drawing.Point(1, 375);
            this.pnlBottom.Margin = new System.Windows.Forms.Padding(1, 0, 1, 1);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Size = new System.Drawing.Size(646, 66);
            this.pnlBottom.TabIndex = 3;
            // 
            // btnFileOpen
            // 
            this.btnFileOpen.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnFileOpen.FlatAppearance.MouseOverBackColor = System.Drawing.Color.MediumSpringGreen;
            this.btnFileOpen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFileOpen.Font = new System.Drawing.Font("Noto Sans KR Medium", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnFileOpen.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.btnFileOpen.Location = new System.Drawing.Point(166, 14);
            this.btnFileOpen.Name = "btnFileOpen";
            this.btnFileOpen.Size = new System.Drawing.Size(138, 39);
            this.btnFileOpen.TabIndex = 2;
            this.btnFileOpen.Text = "File Open";
            this.btnFileOpen.UseVisualStyleBackColor = false;
            this.btnFileOpen.Click += new System.EventHandler(this.btnFileOpen_Click);
            // 
            // btnCancle
            // 
            this.btnCancle.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnCancle.FlatAppearance.MouseOverBackColor = System.Drawing.Color.OrangeRed;
            this.btnCancle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancle.Font = new System.Drawing.Font("Noto Sans KR Medium", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnCancle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.btnCancle.Location = new System.Drawing.Point(496, 14);
            this.btnCancle.Name = "btnCancle";
            this.btnCancle.Size = new System.Drawing.Size(138, 39);
            this.btnCancle.TabIndex = 1;
            this.btnCancle.Text = "Cancle";
            this.btnCancle.UseVisualStyleBackColor = false;
            this.btnCancle.Click += new System.EventHandler(this.Close_Click);
            // 
            // btnUpload
            // 
            this.btnUpload.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnUpload.FlatAppearance.MouseOverBackColor = System.Drawing.Color.MediumSpringGreen;
            this.btnUpload.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUpload.Font = new System.Drawing.Font("Noto Sans KR Medium", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnUpload.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.btnUpload.Location = new System.Drawing.Point(352, 14);
            this.btnUpload.Name = "btnUpload";
            this.btnUpload.Size = new System.Drawing.Size(138, 39);
            this.btnUpload.TabIndex = 0;
            this.btnUpload.Text = "Upload";
            this.btnUpload.UseVisualStyleBackColor = false;
            this.btnUpload.Click += new System.EventHandler(this.btnUpload_Click);
            // 
            // FileUploadForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.Cyan;
            this.ClientSize = new System.Drawing.Size(648, 442);
            this.Controls.Add(this.tlpMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FileUploadForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "FileUploadForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FileUploadForm_FormClosing);
            this.Load += new System.EventHandler(this.FileUploadForm_Load);
            this.tlpMain.ResumeLayout(false);
            this.pnlCenter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picPreView)).EndInit();
            this.pnlTitle.ResumeLayout(false);
            this.pnlTitle.PerformLayout();
            this.pnlBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.Panel pnlTitle;
        private System.Windows.Forms.Label lblClose;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.Panel pnlCenter;
        private System.Windows.Forms.PictureBox picPreView;
        private System.Windows.Forms.Button btnFileOpen;
        private System.Windows.Forms.Button btnCancle;
        private System.Windows.Forms.Button btnUpload;
        private System.Windows.Forms.ListBox listFileName;
    }
}