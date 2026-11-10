using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Base;
using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Core;
using SEOJIN_TMES.Client.Forms.Common;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Theme;

namespace SEOJIN_TMES.Client.Forms.Pop
{
    /// <summary>
    /// POP [공정 목록] 팝업 — 지금 공정이 속한 **라우팅 그룹 안의 페이지 공정**을
    /// 라우팅 공정 · 페이지 공정 · 진행 상태로 보여 주고, 행을 더블클릭하거나 [이동]을 누르면 그 페이지 공정으로 옮긴다.
    ///
    /// &lt;작업 진입 확인&gt;(WorkEntryConfirmForm)은 라우팅 공정마다 한 줄로 줄여 보여 주므로,
    /// 페이지 단위로 골라 옮기는 일은 이 팝업이 맡는다.
    /// 목록 조회와 "어느 라우팅 그룹의 페이지인가"는 호출하는 POP 화면이 정해서 넘긴다.
    /// 그리드 위 투입자재 검색 줄(<see cref="MaterialSearchBar"/>)로 투입자재 코드를 쓰는 페이지만 추릴 수 있다.
    /// 같은 줄 우측에는 지금 목록의 공정 수(전체 · 완료 · 진행중 · 대기)를 보여 준다.
    /// 행 위에 마우스를 올리면 커서 옆에, 행을 클릭하면 그 행 옆에
    /// 그 페이지의 작업 지시 이미지(DocumentShare.RootPath + subRoutingProcImagePath)를 미리보기로 띄운다(<see cref="GridImagePreview"/>).
    ///
    /// VS 디자이너 호환을 위해 매개변수 없는 생성자만 두고, 실제 내용은 Initialize()로 넣는다.
    /// </summary>
    public partial class ProcessListForm : XtraForm
    {
        /// <summary>창을 줄일 수 있는 한계 — 이보다 작으면 하단 버튼이 겹친다(WorkEntryConfirmForm과 같은 값).</summary>
        private readonly Size _minimumFormSize = new Size(662, 574);

        /// <summary>[이동]으로 확정한 페이지 공정. 취소/닫기면 null.</summary>
        public RoutingProcessDto SelectedProcess { get; private set; }

        /// <summary>진행 상태 앞의 상태 구슬 컬럼(이벤트를 걸어 두므로 폼과 수명을 같이한다).</summary>
        private StateImageColumn _stateImage;

        /// <summary>행 마우스 오버 · 클릭 시 작업 지시 이미지 미리보기. 폼이 닫힐 때(Disposed) 함께 해제한다.</summary>
        private GridImagePreview _preview;

        public ProcessListForm()
        {
            InitializeComponent();
        }

        /// <param name="workOrder">제목의 작업지시번호, 투입자재 검색의 조회 키(작업지시번호 · 시리얼)에 쓴다. null이면 제목에서 생략.</param>
        /// <param name="pages">같은 라우팅 그룹의 페이지 공정들(순번 순).</param>
        /// <param name="currentSubRoutingRef">지금 보고 있는 페이지 — 처음 포커스를 여기에 둔다.</param>
        public void Initialize(WorkOrderEntryDto workOrder, List<RoutingProcessDto> pages, string currentSubRoutingRef)
        {
            pages = pages ?? new List<RoutingProcessDto>();

            Text = Language.Pop_ProcessList;
            lblTitle.Text = workOrder != null
                ? $"{Language.Pop_ProcessList} - {workOrder.WorkOrderNo}"
                : Language.Pop_ProcessList;
            lblListTitle.Text = BuildListTitle(pages);
            btnMove.Text = Language.WorkEntryConfirm_Move;
            btnClose.Text = Language.Common_Close;

            ApplyTheme();

            grid.DataSource = pages;
            GridColumns.Apply(gridView, Language.Col_No,
                GridColumns.Text("RoutingName", Language.Col_RoutingOperation, 160),
                GridColumns.Text("PageName", Language.Col_PageOperation, 200),
                GridColumns.Text("ProgressState", Language.Col_ProgressState, 140));
            // 진행 상태 앞에 상태 구슬 그림 — 헤더는 진행 상태와 한 칸으로 합쳐 보인다.
            _stateImage = new StateImageColumn(gridView, nameof(RoutingProcessDto.ProgressState));
            _stateImage.Attach();
            GridColumns.FitToWidth(gridView);

            gridView.FocusedRowHandle = FindRowIndex(pages, currentSubRoutingRef);
            ShowProcessStats(pages);

            // 헤더 · 빈 영역 더블클릭에는 반응하지 않도록 RowDoubleClick을 쓴다(MeisGridView 참고).
            gridView.RowDoubleClick += gridView_RowDoubleClick;
            gridView.CustomColumnDisplayText += gridView_CustomColumnDisplayText;

            // 그리드 위 투입자재 검색 — 이 라우팅 그룹의 페이지 중에서만 찾는다.
            _listTitle = lblListTitle.Text;
            materialSearch.Initialize(workOrder, pages);
            materialSearch.SearchCompleted += materialSearch_SearchCompleted;

            // 작업 지시 이미지 미리보기 — 공유 폴더 계정 접속은 DocShare가 맡는다.
            var settings = Global.Instance.Settings;
            _preview = new GridImagePreview(gridView, PreviewPathOf, settings.DocShare, settings.AppSet.DocumentShare.RootPath);
            Disposed += ProcessListForm_Disposed;
        }

        /// <summary>미리보기할 그림 경로 — 그 페이지 공정의 작업 지시 이미지(subRoutingProcImagePath).</summary>
        private string PreviewPathOf(object row)
        {
            return (row as RoutingProcessDto)?.ProcImagePath;
        }

        private void ProcessListForm_Disposed(object sender, EventArgs e)
        {
            _preview?.Dispose();
            _preview = null;
        }

        /// <summary>검색 전 목록 제목(라우팅 그룹 포함) — 검색 조건을 붙였다 뗄 때의 기준.</summary>
        private string _listTitle;

        /// <summary>투입자재 검색 결과로 목록을 갈아 끼운다(조건이 없으면 전체).</summary>
        private void materialSearch_SearchCompleted(object sender, MaterialSearchResultEventArgs e)
        {
            grid.DataSource = e.Pages;
            ShowProcessStats(e.Pages);
            lblListTitle.Text = string.IsNullOrEmpty(e.Condition)
                ? _listTitle
                : $"{_listTitle} - {e.Condition}";
            if (gridView.RowCount > 0)
                gridView.FocusedRowHandle = 0;
        }

        /// <summary>
        /// 그리드 바로 위 우측에 지금 목록의 페이지 공정 수를 보여 준다 — 전체 · 완료 · 진행중 · 대기.
        /// 서버가 내려준 진행 상태를 그대로 세기만 하고 판정은 하지 않는다(집계 API가 따로 없다).
        /// 투입자재 검색으로 목록이 바뀌면 그 목록 기준으로 다시 센다 — 그리드에 보이는 줄과 숫자가 맞도록.
        /// 중지(Stopped)는 따로 보이지 않으므로 전체에만 들어간다.
        /// </summary>
        private void ShowProcessStats(List<RoutingProcessDto> pages)
        {
            pages = pages ?? new List<RoutingProcessDto>();

            lblProcessStats.Text = string.Format("{0} {1}    {2} {3}    {4} {5}    {6} {7}",
                Language.Common_All, pages.Count,
                Language.ProgressState_Finished, CountInState(pages, "FINISHED"),
                Language.ProgressState_Progress, CountInState(pages, "PROGRESS"),
                Language.ProgressState_Ready, CountInState(pages, "READY"));
        }

        /// <summary>진행 상태가 state인 페이지 수. 대소문자 · 공백 차이는 같은 상태로 본다(Language.ProgressStateText와 같은 기준).</summary>
        private int CountInState(List<RoutingProcessDto> pages, string state)
        {
            int count = 0;
            foreach (var page in pages)
            {
                if (string.Equals(page.ProgressState?.Trim(), state, StringComparison.OrdinalIgnoreCase))
                    count++;
            }

            return count;
        }

        /// <summary>목록 제목 — 어느 라우팅 그룹의 공정인지 알 수 있게 라우팅 그룹을 붙인다.</summary>
        private string BuildListTitle(List<RoutingProcessDto> pages)
        {
            if (pages.Count == 0)
                return Language.Pop_ProcessList;

            return $"{Language.Pop_ProcessList} - {pages[0].RoutingGroup}";
        }

        /// <summary>지금 페이지의 행 위치. 못 찾으면 첫 행(목록이 비면 0이어도 그리드가 무시한다).</summary>
        private int FindRowIndex(List<RoutingProcessDto> pages, string subRoutingRef)
        {
            for (int i = 0; i < pages.Count; i++)
            {
                if (string.Equals(pages[i].SubRoutingReference, subRoutingRef, StringComparison.OrdinalIgnoreCase))
                    return i;
            }

            return 0;
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
            AppTheme.CardPanel(listCard);
            AppTheme.SectionTitle(lblListTitle, AppTheme.Indigo);
            // 검색 줄은 카드 안에 테두리 없이 얹으므로 카드와 같은 바탕색이어야 이어져 보인다.
            searchRow.Appearance.BackColor = AppTheme.Card;
            searchRow.Appearance.Options.UseBackColor = true;
            AppTheme.BodyText(lblProcessStats, true);
            AppTheme.Grid(grid, gridView, AppTheme.Indigo);

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

        /// <summary>선택한 페이지 공정을 확정한다. 선택이 없으면 안내만 하고 창은 유지한다.</summary>
        private void MoveToSelectedProcess()
        {
            var process = gridView.GetFocusedRow() as RoutingProcessDto;
            if (process == null)
            {
                AppMessageBox.Warn(this, Language.WorkEntryConfirm_SelectProcess, Language.Pop_ProcessList);
                return;
            }

            SelectedProcess = process;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
