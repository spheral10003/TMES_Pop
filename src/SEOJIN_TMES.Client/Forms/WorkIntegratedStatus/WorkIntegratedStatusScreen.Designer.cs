namespace SEOJIN_TMES.Client.Forms.WorkIntegratedStatus
{
    partial class WorkIntegratedStatusScreen
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
        /// 색·폰트·아이콘·다국어 문구는 WorkIntegratedStatusScreen.cs의 Initialize()/ApplyTheme()에서 런타임에 입힌다.
        ///
        /// 화면 구성(생산 현황조회와 같은 배치 규칙):
        ///   ┌ topCard      : 「조회 조건」 제목 + [생산품목 시리얼번호] 라벨·입력칸 + [조회]/[엑셀 내보내기] 버튼을
        ///   │                한 줄에 왼쪽부터 나란히 둔다(라벨 → 에디터 → 실행 버튼).
        ///   └ overviewCard : 「작업 통합 개요」 — 품목 → 라우팅 그룹 → 공정 → 페이지 공정 → 투입자재 → 스팩 계층 트리
        ///
        /// 조회 조건이 하나뿐이라 LayoutControl/TileBar 대신 생산 현황조회와 같은 고정 배치를 쓴다 —
        /// 조건 폼이 카드 오른쪽 끝까지 늘어나 입력칸이 화면 가운데로 밀리거나
        /// 실행 버튼이 타일 크기(132×68)로 과하게 커지는 문제를 없앤다.
        /// 조건이 늘어나면 작업 조회 현황(WorkStatusScreen)처럼 같은 좌표 규칙으로 줄을 추가한다.
        /// 도킹은 나중에 Add된 컨트롤이 먼저 처리되므로 Add 순서(overviewCard → spacer → topCard)를 바꾸지 말 것.
        /// </summary>
        private void InitializeComponent()
        {
            this.topCard = new DevExpress.XtraEditors.PanelControl();
            this.lblFilterTitle = new DevExpress.XtraEditors.LabelControl();
            this.lblSerialNo = new DevExpress.XtraEditors.LabelControl();
            this.txtSerialNo = new DevExpress.XtraEditors.TextEdit();
            this.btnSearch = new SEOJIN_TMES.Client.Forms.Common.MeisButton();
            this.btnExcelExport = new SEOJIN_TMES.Client.Forms.Common.MeisButton();
            this.spacer = new System.Windows.Forms.Panel();
            this.overviewCard = new DevExpress.XtraEditors.PanelControl();
            this.treeOverview = new DevExpress.XtraTreeList.TreeList();
            this.lblOverviewTitle = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.topCard)).BeginInit();
            this.topCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtSerialNo.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.overviewCard)).BeginInit();
            this.overviewCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.treeOverview)).BeginInit();
            this.SuspendLayout();
            // 
            // topCard
            // 
            this.topCard.Controls.Add(this.lblFilterTitle);
            this.topCard.Controls.Add(this.lblSerialNo);
            this.topCard.Controls.Add(this.txtSerialNo);
            this.topCard.Controls.Add(this.btnSearch);
            this.topCard.Controls.Add(this.btnExcelExport);
            this.topCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.topCard.Location = new System.Drawing.Point(0, 0);
            this.topCard.Name = "topCard";
            this.topCard.Size = new System.Drawing.Size(1509, 96);
            this.topCard.TabIndex = 0;
            // 
            // lblFilterTitle
            // 
            this.lblFilterTitle.Location = new System.Drawing.Point(20, 18);
            this.lblFilterTitle.Name = "lblFilterTitle";
            this.lblFilterTitle.Size = new System.Drawing.Size(44, 14);
            this.lblFilterTitle.TabIndex = 0;
            this.lblFilterTitle.Text = "조회 조건";
            // 
            // lblSerialNo
            // 
            this.lblSerialNo.Location = new System.Drawing.Point(22, 52);
            this.lblSerialNo.Name = "lblSerialNo";
            this.lblSerialNo.Size = new System.Drawing.Size(111, 14);
            this.lblSerialNo.TabIndex = 1;
            this.lblSerialNo.Text = "생산품목 시리얼번호(*)";
            // 
            // txtSerialNo
            // 
            this.txtSerialNo.Location = new System.Drawing.Point(180, 48);
            this.txtSerialNo.Name = "txtSerialNo";
            this.txtSerialNo.Size = new System.Drawing.Size(260, 20);
            this.txtSerialNo.TabIndex = 2;
            // 
            // btnSearch
            // 
            this.btnSearch.Accent = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(70)))), ((int)(((byte)(229)))));
            this.btnSearch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSearch.Icon = null;
            this.btnSearch.Location = new System.Drawing.Point(456, 47);
            this.btnSearch.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.PaintStyle = DevExpress.XtraEditors.Controls.PaintStyles.Light;
            this.btnSearch.Selected = false;
            this.btnSearch.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            this.btnSearch.Size = new System.Drawing.Size(104, 28);
            this.btnSearch.TabIndex = 3;
            this.btnSearch.Text = "조회";
            this.btnSearch.TextAfterIcon = false;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // btnExcelExport
            // 
            this.btnExcelExport.Accent = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(70)))), ((int)(((byte)(229)))));
            this.btnExcelExport.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExcelExport.Icon = null;
            this.btnExcelExport.Location = new System.Drawing.Point(568, 47);
            this.btnExcelExport.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
            this.btnExcelExport.Name = "btnExcelExport";
            this.btnExcelExport.PaintStyle = DevExpress.XtraEditors.Controls.PaintStyles.Light;
            this.btnExcelExport.Selected = false;
            this.btnExcelExport.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            this.btnExcelExport.Size = new System.Drawing.Size(136, 28);
            this.btnExcelExport.TabIndex = 4;
            this.btnExcelExport.Text = "엑셀 내보내기";
            this.btnExcelExport.TextAfterIcon = false;
            this.btnExcelExport.Click += new System.EventHandler(this.btnExcelExport_Click);
            // 
            // spacer
            // 
            this.spacer.Dock = System.Windows.Forms.DockStyle.Top;
            this.spacer.Location = new System.Drawing.Point(0, 96);
            this.spacer.Name = "spacer";
            this.spacer.Size = new System.Drawing.Size(1509, 10);
            this.spacer.TabIndex = 1;
            // 
            // overviewCard
            // 
            this.overviewCard.Controls.Add(this.treeOverview);
            this.overviewCard.Controls.Add(this.lblOverviewTitle);
            this.overviewCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.overviewCard.Location = new System.Drawing.Point(0, 106);
            this.overviewCard.Name = "overviewCard";
            this.overviewCard.Padding = new System.Windows.Forms.Padding(1);
            this.overviewCard.Size = new System.Drawing.Size(1509, 636);
            this.overviewCard.TabIndex = 2;
            // 
            // treeOverview
            // 
            this.treeOverview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeOverview.KeyFieldName = "NodeId";
            this.treeOverview.Location = new System.Drawing.Point(3, 39);
            this.treeOverview.Name = "treeOverview";
            this.treeOverview.OptionsBehavior.Editable = false;
            this.treeOverview.OptionsView.ShowIndicator = false;
            this.treeOverview.ParentFieldName = "ParentNodeId";
            this.treeOverview.Size = new System.Drawing.Size(1503, 594);
            this.treeOverview.TabIndex = 1;
            // 
            // lblOverviewTitle
            // 
            this.lblOverviewTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblOverviewTitle.Location = new System.Drawing.Point(3, 3);
            this.lblOverviewTitle.Name = "lblOverviewTitle";
            this.lblOverviewTitle.Padding = new System.Windows.Forms.Padding(18, 12, 0, 10);
            this.lblOverviewTitle.Size = new System.Drawing.Size(86, 36);
            this.lblOverviewTitle.TabIndex = 0;
            this.lblOverviewTitle.Text = "작업 통합 개요";
            // 
            // WorkIntegratedStatusScreen
            // 
            this.Controls.Add(this.overviewCard);
            this.Controls.Add(this.spacer);
            this.Controls.Add(this.topCard);
            this.Name = "WorkIntegratedStatusScreen";
            this.Size = new System.Drawing.Size(1509, 742);
            ((System.ComponentModel.ISupportInitialize)(this.topCard)).EndInit();
            this.topCard.ResumeLayout(false);
            this.topCard.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtSerialNo.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.overviewCard)).EndInit();
            this.overviewCard.ResumeLayout(false);
            this.overviewCard.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.treeOverview)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.PanelControl topCard;
        private DevExpress.XtraEditors.LabelControl lblFilterTitle;
        private DevExpress.XtraEditors.LabelControl lblSerialNo;
        private DevExpress.XtraEditors.TextEdit txtSerialNo;
        private SEOJIN_TMES.Client.Forms.Common.MeisButton btnSearch;
        private SEOJIN_TMES.Client.Forms.Common.MeisButton btnExcelExport;
        private System.Windows.Forms.Panel spacer;
        private DevExpress.XtraEditors.PanelControl overviewCard;
        private DevExpress.XtraEditors.LabelControl lblOverviewTitle;
        private DevExpress.XtraTreeList.TreeList treeOverview;
    }
}
