using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Base;
using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Core;
using SEOJIN_TMES.Client.Forms.Pop;
using SEOJIN_TMES.Client.Forms.Common;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Theme;

namespace SEOJIN_TMES.Client.Forms.WorkOrderEntry
{
    /// <summary>
    /// &lt;작업 진입 확인&gt; 팝업 — 선택한 작업지시의 라우팅(공정) 목록을 보여주고,
    /// 사용자가 이동할 **페이지 공정**을 고르면 [이동]으로 확정한다.
    ///
    /// 탭 두 개로 보여 준다.
    /// - [라우팅 공정] : 라우팅 공정이 바뀔 때마다 한 줄만 보여 준다.
    ///   그 줄의 페이지는 진행 상태로 고른다(대기 → 작업 중 → 중지 → 마지막 페이지, <see cref="RoutingProcessGroups"/>) —
    ///   작업자가 라우팅 공정을 고르면 이어서 할 페이지로 바로 들어가게 하려는 것.
    /// - [전체 페이지 공정] : 예전 방식대로 페이지 공정을 전부 보여 준다.
    ///   상단 투입자재 검색 줄(<see cref="MaterialSearchBar"/>)로 투입자재 코드를 쓰는 페이지만 추릴 수 있다.
    ///
    /// 두 탭 모두 진행 상태 뒤에 [비고](서버 remark)를 보여 주고, 맨 뒤 [미리보기] 칸(사진 아이콘)에 마우스를 올리면 커서 옆에,
    /// 그 칸을 클릭하면 그 행 옆에 그 줄 페이지의 작업 지시 이미지(DocumentShare.RootPath + subRoutingProcImagePath)를
    /// 미리보기로 띄운다(<see cref="GridImagePreview"/>, POP [공정 목록] 팝업과 같은 방식).
    /// [라우팅 공정] 탭의 그림은 그 줄의 대표 페이지 — 곧 [이동]으로 들어갈 페이지의 것이다.
    ///
    /// 결과는 <see cref="SelectedProcess"/>로 받는다(DialogResult.OK일 때만 값이 있다).
    /// 데이터 조회는 호출하는 화면(<see cref="WorkOrderEntryScreen"/>)이 미리 해서 넘겨준다 —
    /// 공정이 하나도 없을 때 빈 팝업이 뜨지 않도록 하기 위함.
    ///
    /// VS 디자이너 호환을 위해 매개변수 없는 생성자만 두고, 실제 내용은 Initialize()로 넣는다.
    /// </summary>
    public partial class WorkEntryConfirmForm : XtraForm
    {
        /// <summary>창을 줄일 수 있는 한계 — 크기를 통일하기 전 원래 크기(테두리 포함). 이보다 작으면 하단 버튼이 겹친다.</summary>
        private readonly Size _minimumFormSize = new Size(662, 574);

        private WorkOrderEntryDto _workOrder;

        /// <summary>Initialize로 받은 페이지 공정 전체 — <see cref="FocusProcess"/>가 줄 위치를 찾는 데 쓴다.</summary>
        private List<RoutingProcessDto> _allPages = new List<RoutingProcessDto>();

        /// <summary>[이동]으로 확정한 페이지 공정. 취소/닫기면 null.</summary>
        public RoutingProcessDto SelectedProcess { get; private set; }

        /// <summary>진행 상태 앞의 상태 구슬 컬럼(이벤트를 걸어 두므로 폼과 수명을 같이한다).</summary>
        private StateImageColumn _stateImage;
        private StateImageColumn _stateImageAll;

        /// <summary>[미리보기] 칸의 작업 지시 이미지 미리보기(탭마다 하나). 폼이 닫힐 때(Disposed) 함께 해제한다.</summary>
        private GridImagePreview _preview;
        private GridImagePreview _previewAll;

        public WorkEntryConfirmForm()
        {
            InitializeComponent();
        }

        public void Initialize(WorkOrderEntryDto workOrder, List<RoutingProcessDto> processes)
        {
            _workOrder = workOrder;
            var allPages = processes ?? new List<RoutingProcessDto>();
            _allPages = allPages;

            Text = Language.WorkEntryConfirm_Title;
            lblTitle.Text = _workOrder != null
                ? $"{Language.WorkEntryConfirm_Title} - {_workOrder.WorkOrderNo}"
                : Language.WorkEntryConfirm_Title;
            lblListTitle.Text = Language.WorkEntryConfirm_ListTitle;
            lblAllTitle.Text = Language.WorkEntryConfirm_ListTitle;
            pageRouting.Text = Language.WorkEntryConfirm_TabRouting;
            pageAllPages.Text = Language.WorkEntryConfirm_TabAllPages;
            btnMove.Text = Language.WorkEntryConfirm_Move;
            btnClose.Text = Language.Common_Close;

            ApplyTheme();

            // 탭 1 — 라우팅 공정마다 한 줄
            grid.DataSource = RoutingProcessGroups.Representatives(allPages);
            _stateImage = ConfigureProcessGrid(gridView);

            // 탭 2 — 페이지 공정 전체(처음 열 때는 검색 없이 전부)
            gridAll.DataSource = allPages;
            _stateImageAll = ConfigureProcessGrid(gridViewAll);

            materialSearch.Initialize(_workOrder, allPages);
            materialSearch.SearchCompleted += materialSearch_SearchCompleted;

            // 작업 지시 이미지 미리보기 — 공유 폴더 계정 접속은 DocShare가 맡는다.
            // GridColumns.Apply가 목록에 없는 컬럼을 숨기므로 컬럼 구성(ConfigureProcessGrid) 뒤에 만든다.
            var settings = Global.Instance.Settings;
            _preview = new GridImagePreview(gridView, PreviewPathOf, settings.DocShare, settings.AppSet.DocumentShare.RootPath);
            _previewAll = new GridImagePreview(gridViewAll, PreviewPathOf, settings.DocShare, settings.AppSet.DocumentShare.RootPath);
            Disposed += WorkEntryConfirmForm_Disposed;
        }

        /// <summary>미리보기할 그림 경로 — 그 줄 페이지 공정의 작업 지시 이미지(subRoutingProcImagePath).</summary>
        private string PreviewPathOf(object row)
        {
            return (row as RoutingProcessDto)?.ProcImagePath;
        }

        private void WorkEntryConfirmForm_Disposed(object sender, EventArgs e)
        {
            _preview?.Dispose();
            _preview = null;
            _previewAll?.Dispose();
            _previewAll = null;
        }

        /// <summary>
        /// 처음 포커스를 <paramref name="subRoutingRef"/> 페이지에 둔다 — [라우팅 공정] 탭은 그 페이지가 속한 줄,
        /// [전체 페이지 공정] 탭은 그 페이지. <see cref="Initialize"/> 뒤에 부른다. 목록에 없으면 첫 줄 그대로 둔다.
        /// POP 화면이 라우팅 그룹 경계에서 이 창을 띄울 때, 이어서 할 공정을 작업자가 찾아 내려가지 않게 하려는 것이다.
        /// </summary>
        public void FocusProcess(string subRoutingRef)
        {
            if (string.IsNullOrEmpty(subRoutingRef))
                return;

            var routingIndex = RoutingProcessGroups.RepresentativeIndexOf(_allPages, subRoutingRef);
            if (routingIndex >= 0)
                gridView.FocusedRowHandle = gridView.GetRowHandle(routingIndex);

            for (int i = 0; i < _allPages.Count; i++)
            {
                if (_allPages[i] != null && string.Equals(_allPages[i].SubRoutingReference, subRoutingRef, StringComparison.OrdinalIgnoreCase))
                {
                    gridViewAll.FocusedRowHandle = gridViewAll.GetRowHandle(i);
                    break;
                }
            }
        }

        /// <summary>두 탭의 그리드 컬럼 · 상태 구슬 · 더블클릭을 같은 모양으로 맞춘다.</summary>
        private StateImageColumn ConfigureProcessGrid(MeisGridView view)
        {
            GridColumns.Apply(view, Language.Col_No,
                GridColumns.Text("RoutingGroup", Language.Col_RoutingGroup, 150),
                GridColumns.Text("RoutingName", Language.Col_RoutingOperation, 140),
                GridColumns.Text("PageName", Language.Col_PageOperation, 140),
                GridColumns.Text("ProgressState", Language.Col_ProgressState, 120),
                GridColumns.Left("Remark", Language.Col_Remark, 200));
            // 진행 상태 앞에 상태 구슬 그림 — 헤더는 진행 상태와 한 칸으로 합쳐 보인다.
            var stateImage = new StateImageColumn(view, nameof(RoutingProcessDto.ProgressState));
            stateImage.Attach();
            GridColumns.FitToWidth(view);

            if (view.RowCount > 0)
                view.FocusedRowHandle = 0;

            // 행을 더블클릭하면 [이동]과 같게 동작한다.
            // 헤더 · 빈 영역 더블클릭에는 반응하지 않도록 RowDoubleClick을 쓴다(MeisGridView 참고).
            view.RowDoubleClick += gridView_RowDoubleClick;
            view.CustomColumnDisplayText += gridView_CustomColumnDisplayText;
            return stateImage;
        }

        /// <summary>투입자재 검색 결과로 [전체 페이지 공정] 목록을 갈아 끼운다(조건이 없으면 전체).</summary>
        private void materialSearch_SearchCompleted(object sender, MaterialSearchResultEventArgs e)
        {
            gridAll.DataSource = e.Pages;
            lblAllTitle.Text = string.IsNullOrEmpty(e.Condition)
                ? Language.WorkEntryConfirm_ListTitle
                : $"{Language.WorkEntryConfirm_ListTitle} - {e.Condition}";
            if (gridViewAll.RowCount > 0)
                gridViewAll.FocusedRowHandle = 0;
        }

        /// <summary>진행 상태(서버 코드 Ready/Progress/Stopped/Finished)를 화면 언어의 문구로 보여 준다. 값 자체는 바꾸지 않는다.</summary>
        private void gridView_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column == null || e.Column.FieldName != nameof(RoutingProcessDto.ProgressState))
                return;

            var value = e.Value as string;
            if (string.IsNullOrEmpty(value))
                return;

            e.DisplayText = Language.ProgressStateText(value);
        }

        private void ApplyTheme()
        {
            AppTheme.DialogHeader(headerPanel, AppTheme.Indigo);
            DialogChrome.Apply(this, headerPanel);
            DialogChrome.FitPopupSize(this);
            DialogChrome.EnableResizeGrip(pnlGrip);
            MinimumSize = _minimumFormSize;
            AppTheme.DialogHeaderTitle(lblTitle);
            AppTheme.ContentSurface(bodyPanel);
            AppTheme.ContentSurface(footerPanel);
            pnlGrip.BackColor = AppTheme.Surface;
            AppTheme.StandardButton(btnMove);
            AppTheme.StandardButton(btnClose);

            pageRouting.BackColor = AppTheme.Surface;
            pageAllPages.BackColor = AppTheme.Surface;

            AppTheme.CardPanel(listCard);
            AppTheme.SectionTitle(lblListTitle, AppTheme.Indigo);
            AppTheme.Grid(grid, gridView, AppTheme.Indigo);

            AppTheme.CardPanel(allCard);
            AppTheme.SectionTitle(lblAllTitle, AppTheme.Indigo);
            AppTheme.Grid(gridAll, gridViewAll, AppTheme.Indigo);

            headerIcon.SvgImage = AppIcons.Get(AppIcons.Layers, Color.White);
            headerIcon.SizeMode = SvgImageSizeMode.Stretch;
            headerIcon.BackColor = Color.Transparent;

        }

        /// <summary>부모 창(없으면 화면) 가운데에 띄운다.</summary>
        public void CenterOn(IWin32Window owner)
        {
            DialogChrome.CenterOn(this, owner);   // 부모가 최소화돼 있어도 화면 안에 놓는다
        }

        private void btnMove_Click(object sender, EventArgs e) => MoveToSelectedProcess();

        private void gridView_RowDoubleClick(object sender, EventArgs e) => MoveToSelectedProcess();

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>선택한 페이지 공정을 확정한다(보고 있는 탭의 그리드 기준). 선택이 없으면 안내만 하고 창은 유지한다.</summary>
        private void MoveToSelectedProcess()
        {
            var view = tabProcess.SelectedTab == pageAllPages ? gridViewAll : gridView;
            var process = view.GetFocusedRow() as RoutingProcessDto;
            if (process == null)
            {
                AppMessageBox.Warn(this, Language.WorkEntryConfirm_SelectProcess, Language.WorkEntryConfirm_Title);
                return;
            }

            SelectedProcess = process;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
