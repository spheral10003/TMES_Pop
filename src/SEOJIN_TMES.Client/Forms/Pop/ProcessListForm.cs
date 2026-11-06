using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Base;
using SEOJIN_TMES.Client.Common.Models;
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
    ///
    /// VS 디자이너 호환을 위해 매개변수 없는 생성자만 두고, 실제 내용은 Initialize()로 넣는다.
    /// </summary>
    public partial class ProcessListForm : XtraForm
    {
        /// <summary>창을 줄일 수 있는 한계 — 이보다 작으면 하단 버튼이 겹친다(WorkEntryConfirmForm과 같은 값).</summary>
        private readonly Size _minimumFormSize = new Size(662, 574);

        /// <summary>[이동]으로 확정한 페이지 공정. 취소/닫기면 null.</summary>
        public RoutingProcessDto SelectedProcess { get; private set; }

        public ProcessListForm()
        {
            InitializeComponent();
        }

        /// <param name="workOrder">제목에 작업지시번호를 붙이는 데만 쓴다. null이면 생략.</param>
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
            GridColumns.FitToWidth(gridView);

            gridView.FocusedRowHandle = FindRowIndex(pages, currentSubRoutingRef);

            // 헤더 · 빈 영역 더블클릭에는 반응하지 않도록 RowDoubleClick을 쓴다(MeisGridView 참고).
            gridView.RowDoubleClick += gridView_RowDoubleClick;
            gridView.CustomColumnDisplayText += gridView_CustomColumnDisplayText;
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
