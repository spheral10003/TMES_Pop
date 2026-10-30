namespace SEOJIN_TMES.Client.Forms.ModbusTest
{
    partial class ModbusConnectionTestForm
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
        /// 색·폰트·문구는 ModbusConnectionTestForm.cs의 Initialize()에서 런타임에 넣는다.
        ///
        /// Windows 제목 줄은 없애고(FormBorderStyle.None) pnlTitle + lblTitle이 제목 줄 역할을 한다(StopIssueForm과 같은 구성).
        /// 구성: 제목 띠(오른쪽 X 닫기) + [반복 횟수 · 대상 편집 버튼 줄] + [테스트 대상 그리드] + [접속 기록]
        ///       + 하단 [닫기][중지][시작] + 우하단 크기 조절 손잡이(pnlGrip).
        /// lblClose는 lblTitle(Fill)보다 먼저 Add해 위에 그려지게 둔다.
        /// 도킹 순서 주의: pnlBody(Fill) → pnlFooter(Bottom) → pnlTitle(Top).
        ///                 pnlBody 안은 grid(Fill) → lblLogTitle(Bottom) → memoLog(Bottom) → lblListTitle(Top) → pnlOptions(Top).
        ///                 나중에 Add한 것이 바깥쪽에 놓인다 — 접속 기록이 맨 아래, 그 위에 기록 제목. 옵션 줄이 맨 위, 그 아래 목록 제목.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlTitle = new System.Windows.Forms.Panel();
            this.lblClose = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pnlBody = new System.Windows.Forms.Panel();
            this.grid = new DevExpress.XtraGrid.GridControl();
            this.gridView = new SEOJIN_TMES.Client.Forms.Common.MeisGridView();
            this.lblLogTitle = new System.Windows.Forms.Label();
            this.memoLog = new DevExpress.XtraEditors.MemoEdit();
            this.lblListTitle = new System.Windows.Forms.Label();
            this.pnlOptions = new System.Windows.Forms.Panel();
            this.lblRepeat = new System.Windows.Forms.Label();
            this.spinRepeat = new DevExpress.XtraEditors.SpinEdit();
            this.lblTimeoutInfo = new System.Windows.Forms.Label();
            this.btnLoadEquipments = new System.Windows.Forms.Button();
            this.btnAddRow = new System.Windows.Forms.Button();
            this.btnRemoveRow = new System.Windows.Forms.Button();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.pnlGrip = new System.Windows.Forms.Panel();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.pnlTitle.SuspendLayout();
            this.pnlBody.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.memoLog.Properties)).BeginInit();
            this.pnlOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spinRepeat.Properties)).BeginInit();
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
            this.lblTitle.Text = "Modbus TCP 접속 테스트";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // pnlBody
            //
            this.pnlBody.Controls.Add(this.grid);
            this.pnlBody.Controls.Add(this.lblLogTitle);
            this.pnlBody.Controls.Add(this.memoLog);
            this.pnlBody.Controls.Add(this.lblListTitle);
            this.pnlBody.Controls.Add(this.pnlOptions);
            this.pnlBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBody.Location = new System.Drawing.Point(0, 48);
            this.pnlBody.Name = "pnlBody";
            this.pnlBody.Padding = new System.Windows.Forms.Padding(16, 4, 16, 8);
            this.pnlBody.Size = new System.Drawing.Size(1200, 588);
            this.pnlBody.TabIndex = 1;
            //
            // pnlOptions
            //
            this.pnlOptions.Controls.Add(this.lblRepeat);
            this.pnlOptions.Controls.Add(this.spinRepeat);
            this.pnlOptions.Controls.Add(this.lblTimeoutInfo);
            this.pnlOptions.Controls.Add(this.btnLoadEquipments);
            this.pnlOptions.Controls.Add(this.btnAddRow);
            this.pnlOptions.Controls.Add(this.btnRemoveRow);
            this.pnlOptions.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlOptions.Location = new System.Drawing.Point(16, 4);
            this.pnlOptions.Name = "pnlOptions";
            this.pnlOptions.Size = new System.Drawing.Size(1168, 52);
            this.pnlOptions.TabIndex = 0;
            //
            // lblRepeat
            //
            this.lblRepeat.Location = new System.Drawing.Point(0, 12);
            this.lblRepeat.Name = "lblRepeat";
            this.lblRepeat.Size = new System.Drawing.Size(90, 28);
            this.lblRepeat.TabIndex = 0;
            this.lblRepeat.Text = "반복 횟수";
            this.lblRepeat.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // spinRepeat
            //
            this.spinRepeat.EditValue = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.spinRepeat.Location = new System.Drawing.Point(96, 13);
            this.spinRepeat.Name = "spinRepeat";
            this.spinRepeat.Properties.IsFloatValue = false;
            this.spinRepeat.Properties.MaxValue = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.spinRepeat.Properties.MinValue = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.spinRepeat.Size = new System.Drawing.Size(110, 26);
            this.spinRepeat.TabIndex = 1;
            //
            // lblTimeoutInfo
            //
            this.lblTimeoutInfo.Location = new System.Drawing.Point(222, 12);
            this.lblTimeoutInfo.Name = "lblTimeoutInfo";
            this.lblTimeoutInfo.Size = new System.Drawing.Size(360, 28);
            this.lblTimeoutInfo.TabIndex = 2;
            this.lblTimeoutInfo.Text = "접속 대기 2000ms · 재접속 간격 300ms";
            this.lblTimeoutInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // btnLoadEquipments
            //
            this.btnLoadEquipments.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLoadEquipments.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLoadEquipments.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLoadEquipments.Location = new System.Drawing.Point(738, 7);
            this.btnLoadEquipments.Name = "btnLoadEquipments";
            this.btnLoadEquipments.Size = new System.Drawing.Size(190, 39);
            this.btnLoadEquipments.TabIndex = 3;
            this.btnLoadEquipments.Text = "등록 장비 불러오기";
            this.btnLoadEquipments.UseVisualStyleBackColor = false;
            this.btnLoadEquipments.Click += new System.EventHandler(this.btnLoadEquipments_Click);
            //
            // btnAddRow
            //
            this.btnAddRow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddRow.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddRow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddRow.Location = new System.Drawing.Point(936, 7);
            this.btnAddRow.Name = "btnAddRow";
            this.btnAddRow.Size = new System.Drawing.Size(112, 39);
            this.btnAddRow.TabIndex = 4;
            this.btnAddRow.Text = "행 추가";
            this.btnAddRow.UseVisualStyleBackColor = false;
            this.btnAddRow.Click += new System.EventHandler(this.btnAddRow_Click);
            //
            // btnRemoveRow
            //
            this.btnRemoveRow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveRow.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRemoveRow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRemoveRow.Location = new System.Drawing.Point(1056, 7);
            this.btnRemoveRow.Name = "btnRemoveRow";
            this.btnRemoveRow.Size = new System.Drawing.Size(112, 39);
            this.btnRemoveRow.TabIndex = 5;
            this.btnRemoveRow.Text = "행 삭제";
            this.btnRemoveRow.UseVisualStyleBackColor = false;
            this.btnRemoveRow.Click += new System.EventHandler(this.btnRemoveRow_Click);
            //
            // lblListTitle
            //
            this.lblListTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblListTitle.Location = new System.Drawing.Point(16, 56);
            this.lblListTitle.Name = "lblListTitle";
            this.lblListTitle.Size = new System.Drawing.Size(1168, 32);
            this.lblListTitle.TabIndex = 1;
            this.lblListTitle.Text = "테스트 대상";
            this.lblListTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // grid
            //
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.Location = new System.Drawing.Point(16, 88);
            this.grid.MainView = this.gridView;
            this.grid.Name = "grid";
            this.grid.Size = new System.Drawing.Size(1168, 292);
            this.grid.TabIndex = 2;
            this.grid.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView});
            //
            // gridView
            //
            this.gridView.GridControl = this.grid;
            this.gridView.Name = "gridView";
            //
            // lblLogTitle
            //
            this.lblLogTitle.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblLogTitle.Location = new System.Drawing.Point(16, 380);
            this.lblLogTitle.Name = "lblLogTitle";
            this.lblLogTitle.Size = new System.Drawing.Size(1168, 32);
            this.lblLogTitle.TabIndex = 3;
            this.lblLogTitle.Text = "접속 기록";
            this.lblLogTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // memoLog
            //
            this.memoLog.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.memoLog.Location = new System.Drawing.Point(16, 412);
            this.memoLog.Name = "memoLog";
            this.memoLog.Properties.ReadOnly = true;
            this.memoLog.Properties.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.memoLog.Size = new System.Drawing.Size(1168, 168);
            this.memoLog.TabIndex = 4;
            //
            // pnlFooter
            //
            this.pnlFooter.Controls.Add(this.pnlGrip);
            this.pnlFooter.Controls.Add(this.btnStart);
            this.pnlFooter.Controls.Add(this.btnStop);
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
            this.pnlGrip.TabIndex = 3;
            //
            // btnStart
            //
            this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStart.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStart.Location = new System.Drawing.Point(1046, 12);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(138, 39);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "시작";
            this.btnStart.UseVisualStyleBackColor = false;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            //
            // btnStop
            //
            this.btnStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStop.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStop.Location = new System.Drawing.Point(900, 12);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(138, 39);
            this.btnStop.TabIndex = 1;
            this.btnStop.Text = "중지";
            this.btnStop.UseVisualStyleBackColor = false;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            //
            // btnClose
            //
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Location = new System.Drawing.Point(754, 12);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(138, 39);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "닫기";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            //
            // ModbusConnectionTestForm
            //
            this.Appearance.Options.UseFont = true;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Controls.Add(this.pnlBody);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.pnlTitle);
            this.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.IconOptions.ShowIcon = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ModbusConnectionTestForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Modbus TCP 접속 테스트";
            this.pnlTitle.ResumeLayout(false);
            this.pnlBody.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.memoLog.Properties)).EndInit();
            this.pnlOptions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spinRepeat.Properties)).EndInit();
            this.pnlFooter.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlTitle;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblClose;
        private System.Windows.Forms.Panel pnlBody;
        private System.Windows.Forms.Panel pnlOptions;
        private System.Windows.Forms.Label lblRepeat;
        private DevExpress.XtraEditors.SpinEdit spinRepeat;
        private System.Windows.Forms.Label lblTimeoutInfo;
        private System.Windows.Forms.Button btnLoadEquipments;
        private System.Windows.Forms.Button btnAddRow;
        private System.Windows.Forms.Button btnRemoveRow;
        private System.Windows.Forms.Label lblListTitle;
        private DevExpress.XtraGrid.GridControl grid;
        private SEOJIN_TMES.Client.Forms.Common.MeisGridView gridView;
        private System.Windows.Forms.Label lblLogTitle;
        private DevExpress.XtraEditors.MemoEdit memoLog;
        private System.Windows.Forms.Panel pnlFooter;
        private System.Windows.Forms.Panel pnlGrip;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnClose;
    }
}
