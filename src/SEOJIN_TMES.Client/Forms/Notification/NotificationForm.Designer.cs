namespace SEOJIN_TMES.Client.Forms.Notification
{
    partial class NotificationForm
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
        /// 색·폰트·아이콘·문구는 NotificationForm.cs의 Initialize()에서 런타임에 넣는다.
        ///
        /// 구성: 상단 색 띠(아이콘 + 제목 + [조회][닫기]) + 조회조건 카드 + 본문 좌우 분할
        ///       (좌: 알림 목록 그리드 + [읽음] / 우: 선택한 알림 상세).
        /// 도킹 순서 주의: 나중에 Add한 컨트롤이 먼저 자리를 잡는다 —
        /// filterCard(Top) → listCard(Left) → detailCard(Fill) 순으로 배치되도록 역순으로 Add한다.
        /// </summary>
        private void InitializeComponent()
        {
            this.headerPanel = new DevExpress.XtraEditors.PanelControl();
            this.headerIcon = new DevExpress.XtraEditors.SvgImageBox();
            this.lblTitle = new DevExpress.XtraEditors.LabelControl();
            this.btnSearch = new SEOJIN_TMES.Client.Forms.Common.MeisButton();
            this.btnClose = new SEOJIN_TMES.Client.Forms.Common.MeisButton();
            this.bodyPanel = new DevExpress.XtraEditors.PanelControl();
            this.detailCard = new DevExpress.XtraEditors.PanelControl();
            this.lblSenderId = new DevExpress.XtraEditors.LabelControl();
            this.txtSenderId = new DevExpress.XtraEditors.TextEdit();
            this.lblDetailSubject = new DevExpress.XtraEditors.LabelControl();
            this.txtDetailSubject = new DevExpress.XtraEditors.TextEdit();
            this.lblContent = new DevExpress.XtraEditors.LabelControl();
            this.memoContent = new DevExpress.XtraEditors.MemoEdit();
            this.spacerMiddle = new System.Windows.Forms.Panel();
            this.listCard = new DevExpress.XtraEditors.PanelControl();
            this.grid = new DevExpress.XtraGrid.GridControl();
            this.gridView = new SEOJIN_TMES.Client.Forms.Common.MeisGridView();
            this.listHeader = new DevExpress.XtraEditors.PanelControl();
            this.lblListTitle = new DevExpress.XtraEditors.LabelControl();
            this.btnMarkRead = new SEOJIN_TMES.Client.Forms.Common.MeisButton();
            this.spacerFilter = new System.Windows.Forms.Panel();
            this.filterCard = new DevExpress.XtraEditors.PanelControl();
            this.lblUserName = new DevExpress.XtraEditors.LabelControl();
            this.txtUserName = new DevExpress.XtraEditors.TextEdit();
            this.lblSubject = new DevExpress.XtraEditors.LabelControl();
            this.txtSubject = new DevExpress.XtraEditors.TextEdit();
            this.lblReadState = new DevExpress.XtraEditors.LabelControl();
            this.cboReadState = new DevExpress.XtraEditors.ComboBoxEdit();
            ((System.ComponentModel.ISupportInitialize)(this.headerPanel)).BeginInit();
            this.headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.headerIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bodyPanel)).BeginInit();
            this.bodyPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.detailCard)).BeginInit();
            this.detailCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtSenderId.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDetailSubject.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.memoContent.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.listCard)).BeginInit();
            this.listCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.listHeader)).BeginInit();
            this.listHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.filterCard)).BeginInit();
            this.filterCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtUserName.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSubject.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboReadState.Properties)).BeginInit();
            this.SuspendLayout();
            //
            // headerPanel
            //
            this.headerPanel.Controls.Add(this.headerIcon);
            this.headerPanel.Controls.Add(this.lblTitle);
            this.headerPanel.Controls.Add(this.btnSearch);
            this.headerPanel.Controls.Add(this.btnClose);
            this.headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.headerPanel.Location = new System.Drawing.Point(0, 0);
            this.headerPanel.Name = "headerPanel";
            this.headerPanel.Size = new System.Drawing.Size(1220, 56);
            this.headerPanel.TabIndex = 0;
            //
            // headerIcon
            //
            this.headerIcon.Location = new System.Drawing.Point(22, 15);
            this.headerIcon.Name = "headerIcon";
            this.headerIcon.Size = new System.Drawing.Size(26, 26);
            this.headerIcon.TabIndex = 0;
            //
            // lblTitle
            //
            this.lblTitle.Location = new System.Drawing.Point(58, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(28, 14);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "알림";
            //
            // btnSearch
            //
            this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSearch.Location = new System.Drawing.Point(1020, 14);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(88, 28);
            this.btnSearch.TabIndex = 2;
            this.btnSearch.Text = "조회";
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            //
            // btnClose
            //
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(1116, 14);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(88, 28);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "닫기";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            //
            // bodyPanel
            //
            this.bodyPanel.Controls.Add(this.detailCard);
            this.bodyPanel.Controls.Add(this.spacerMiddle);
            this.bodyPanel.Controls.Add(this.listCard);
            this.bodyPanel.Controls.Add(this.spacerFilter);
            this.bodyPanel.Controls.Add(this.filterCard);
            this.bodyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bodyPanel.Location = new System.Drawing.Point(0, 56);
            this.bodyPanel.Name = "bodyPanel";
            this.bodyPanel.Padding = new System.Windows.Forms.Padding(16, 12, 16, 14);
            this.bodyPanel.Size = new System.Drawing.Size(1220, 604);
            this.bodyPanel.TabIndex = 1;
            //
            // detailCard
            //
            this.detailCard.Controls.Add(this.lblSenderId);
            this.detailCard.Controls.Add(this.txtSenderId);
            this.detailCard.Controls.Add(this.lblDetailSubject);
            this.detailCard.Controls.Add(this.txtDetailSubject);
            this.detailCard.Controls.Add(this.lblContent);
            this.detailCard.Controls.Add(this.memoContent);
            this.detailCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.detailCard.Location = new System.Drawing.Point(750, 88);
            this.detailCard.Name = "detailCard";
            this.detailCard.Size = new System.Drawing.Size(452, 502);
            this.detailCard.TabIndex = 4;
            //
            // lblSenderId
            //
            this.lblSenderId.Location = new System.Drawing.Point(20, 24);
            this.lblSenderId.Name = "lblSenderId";
            this.lblSenderId.Size = new System.Drawing.Size(70, 14);
            this.lblSenderId.TabIndex = 0;
            this.lblSenderId.Text = "발신자아이디";
            //
            // txtSenderId
            //
            this.txtSenderId.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSenderId.Location = new System.Drawing.Point(110, 20);
            this.txtSenderId.Name = "txtSenderId";
            this.txtSenderId.Properties.ReadOnly = true;
            this.txtSenderId.Size = new System.Drawing.Size(320, 20);
            this.txtSenderId.TabIndex = 1;
            //
            // lblDetailSubject
            //
            this.lblDetailSubject.Location = new System.Drawing.Point(20, 62);
            this.lblDetailSubject.Name = "lblDetailSubject";
            this.lblDetailSubject.Size = new System.Drawing.Size(28, 14);
            this.lblDetailSubject.TabIndex = 2;
            this.lblDetailSubject.Text = "제목";
            //
            // txtDetailSubject
            //
            this.txtDetailSubject.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDetailSubject.Location = new System.Drawing.Point(110, 58);
            this.txtDetailSubject.Name = "txtDetailSubject";
            this.txtDetailSubject.Properties.ReadOnly = true;
            this.txtDetailSubject.Size = new System.Drawing.Size(320, 20);
            this.txtDetailSubject.TabIndex = 3;
            //
            // lblContent
            //
            this.lblContent.Location = new System.Drawing.Point(20, 100);
            this.lblContent.Name = "lblContent";
            this.lblContent.Size = new System.Drawing.Size(28, 14);
            this.lblContent.TabIndex = 4;
            this.lblContent.Text = "내용";
            //
            // memoContent
            //
            this.memoContent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.memoContent.Location = new System.Drawing.Point(110, 96);
            this.memoContent.Name = "memoContent";
            this.memoContent.Properties.ReadOnly = true;
            this.memoContent.Size = new System.Drawing.Size(320, 382);
            this.memoContent.TabIndex = 5;
            //
            // spacerMiddle
            //
            this.spacerMiddle.Dock = System.Windows.Forms.DockStyle.Left;
            this.spacerMiddle.Location = new System.Drawing.Point(738, 88);
            this.spacerMiddle.Name = "spacerMiddle";
            this.spacerMiddle.Size = new System.Drawing.Size(12, 502);
            this.spacerMiddle.TabIndex = 3;
            //
            // listCard
            //
            this.listCard.Controls.Add(this.grid);
            this.listCard.Controls.Add(this.listHeader);
            this.listCard.Dock = System.Windows.Forms.DockStyle.Left;
            this.listCard.Location = new System.Drawing.Point(18, 88);
            this.listCard.Name = "listCard";
            this.listCard.Padding = new System.Windows.Forms.Padding(1);
            this.listCard.Size = new System.Drawing.Size(720, 502);
            this.listCard.TabIndex = 2;
            //
            // grid
            //
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.Location = new System.Drawing.Point(1, 41);
            this.grid.MainView = this.gridView;
            this.grid.Name = "grid";
            this.grid.Size = new System.Drawing.Size(718, 460);
            this.grid.TabIndex = 1;
            this.grid.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView});
            //
            // gridView
            //
            this.gridView.GridControl = this.grid;
            this.gridView.Name = "gridView";
            this.gridView.OptionsBehavior.Editable = false;
            //
            // listHeader
            //
            this.listHeader.Controls.Add(this.lblListTitle);
            this.listHeader.Controls.Add(this.btnMarkRead);
            this.listHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.listHeader.Location = new System.Drawing.Point(1, 1);
            this.listHeader.Name = "listHeader";
            this.listHeader.Size = new System.Drawing.Size(718, 40);
            this.listHeader.TabIndex = 0;
            //
            // lblListTitle
            //
            this.lblListTitle.Location = new System.Drawing.Point(16, 13);
            this.lblListTitle.Name = "lblListTitle";
            this.lblListTitle.Size = new System.Drawing.Size(28, 14);
            this.lblListTitle.TabIndex = 0;
            this.lblListTitle.Text = "알림";
            //
            // btnMarkRead
            //
            this.btnMarkRead.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            // 폭 120 — 아이콘(16) + 간격 + "Mark read"(영어가 가장 길다)가 잘리지 않는 크기.
            // 우측 고정(Anchor Right)이라 왼쪽으로 자라며, 왼쪽에는 제목 라벨뿐이라 부딪히지 않는다.
            this.btnMarkRead.Location = new System.Drawing.Point(584, 7);
            this.btnMarkRead.Name = "btnMarkRead";
            this.btnMarkRead.Size = new System.Drawing.Size(120, 26);
            this.btnMarkRead.TabIndex = 1;
            this.btnMarkRead.Text = "읽음";
            this.btnMarkRead.Click += new System.EventHandler(this.btnMarkRead_Click);
            //
            // spacerFilter
            //
            this.spacerFilter.Dock = System.Windows.Forms.DockStyle.Top;
            this.spacerFilter.Location = new System.Drawing.Point(18, 76);
            this.spacerFilter.Name = "spacerFilter";
            this.spacerFilter.Size = new System.Drawing.Size(1184, 12);
            this.spacerFilter.TabIndex = 1;
            //
            // filterCard
            //
            this.filterCard.Controls.Add(this.lblUserName);
            this.filterCard.Controls.Add(this.txtUserName);
            this.filterCard.Controls.Add(this.lblSubject);
            this.filterCard.Controls.Add(this.txtSubject);
            this.filterCard.Controls.Add(this.lblReadState);
            this.filterCard.Controls.Add(this.cboReadState);
            this.filterCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.filterCard.Location = new System.Drawing.Point(18, 14);
            this.filterCard.Name = "filterCard";
            this.filterCard.Size = new System.Drawing.Size(1184, 62);
            this.filterCard.TabIndex = 0;
            //
            // lblUserName
            //
            this.lblUserName.Location = new System.Drawing.Point(20, 24);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.Size = new System.Drawing.Size(50, 14);
            this.lblUserName.TabIndex = 0;
            this.lblUserName.Text = "사용자명";
            //
            // txtUserName
            //
            this.txtUserName.Location = new System.Drawing.Point(100, 20);
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.Size = new System.Drawing.Size(200, 20);
            this.txtUserName.TabIndex = 1;
            //
            // lblSubject
            //
            this.lblSubject.Location = new System.Drawing.Point(330, 24);
            this.lblSubject.Name = "lblSubject";
            this.lblSubject.Size = new System.Drawing.Size(28, 14);
            this.lblSubject.TabIndex = 2;
            this.lblSubject.Text = "제목";
            //
            // txtSubject
            //
            this.txtSubject.Location = new System.Drawing.Point(390, 20);
            this.txtSubject.Name = "txtSubject";
            this.txtSubject.Size = new System.Drawing.Size(200, 20);
            this.txtSubject.TabIndex = 3;
            //
            // lblReadState
            //
            this.lblReadState.Location = new System.Drawing.Point(620, 24);
            this.lblReadState.Name = "lblReadState";
            this.lblReadState.Size = new System.Drawing.Size(50, 14);
            this.lblReadState.TabIndex = 4;
            this.lblReadState.Text = "읽음여부";
            //
            // cboReadState
            //
            this.cboReadState.Location = new System.Drawing.Point(700, 20);
            this.cboReadState.Name = "cboReadState";
            this.cboReadState.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cboReadState.Size = new System.Drawing.Size(140, 20);
            this.cboReadState.TabIndex = 5;
            //
            // NotificationForm
            //
            this.Appearance.Options.UseFont = true;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(1220, 660);
            this.Controls.Add(this.bodyPanel);
            this.Controls.Add(this.headerPanel);
            this.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.IconOptions.ShowIcon = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(940, 560);
            this.Name = "NotificationForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "알림";
            ((System.ComponentModel.ISupportInitialize)(this.headerPanel)).EndInit();
            this.headerPanel.ResumeLayout(false);
            this.headerPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.headerIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bodyPanel)).EndInit();
            this.bodyPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.detailCard)).EndInit();
            this.detailCard.ResumeLayout(false);
            this.detailCard.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtSenderId.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDetailSubject.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.memoContent.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.listCard)).EndInit();
            this.listCard.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.listHeader)).EndInit();
            this.listHeader.ResumeLayout(false);
            this.listHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.filterCard)).EndInit();
            this.filterCard.ResumeLayout(false);
            this.filterCard.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtUserName.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSubject.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboReadState.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.PanelControl headerPanel;
        private DevExpress.XtraEditors.SvgImageBox headerIcon;
        private DevExpress.XtraEditors.LabelControl lblTitle;
        private SEOJIN_TMES.Client.Forms.Common.MeisButton btnSearch;
        private SEOJIN_TMES.Client.Forms.Common.MeisButton btnClose;
        private DevExpress.XtraEditors.PanelControl bodyPanel;
        private DevExpress.XtraEditors.PanelControl filterCard;
        private DevExpress.XtraEditors.LabelControl lblUserName;
        private DevExpress.XtraEditors.TextEdit txtUserName;
        private DevExpress.XtraEditors.LabelControl lblSubject;
        private DevExpress.XtraEditors.TextEdit txtSubject;
        private DevExpress.XtraEditors.LabelControl lblReadState;
        private DevExpress.XtraEditors.ComboBoxEdit cboReadState;
        private System.Windows.Forms.Panel spacerFilter;
        private DevExpress.XtraEditors.PanelControl listCard;
        private DevExpress.XtraEditors.PanelControl listHeader;
        private DevExpress.XtraEditors.LabelControl lblListTitle;
        private SEOJIN_TMES.Client.Forms.Common.MeisButton btnMarkRead;
        private DevExpress.XtraGrid.GridControl grid;
        private SEOJIN_TMES.Client.Forms.Common.MeisGridView gridView;
        private System.Windows.Forms.Panel spacerMiddle;
        private DevExpress.XtraEditors.PanelControl detailCard;
        private DevExpress.XtraEditors.LabelControl lblSenderId;
        private DevExpress.XtraEditors.TextEdit txtSenderId;
        private DevExpress.XtraEditors.LabelControl lblDetailSubject;
        private DevExpress.XtraEditors.TextEdit txtDetailSubject;
        private DevExpress.XtraEditors.LabelControl lblContent;
        private DevExpress.XtraEditors.MemoEdit memoContent;
    }
}
