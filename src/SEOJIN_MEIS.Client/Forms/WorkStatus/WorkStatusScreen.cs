using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using SEOJIN_MEIS.Client.ApiClient;
using SEOJIN_MEIS.Client.Common.Models;
using SEOJIN_MEIS.Client.Composition;
using SEOJIN_MEIS.Client.Forms.Common;
using SEOJIN_MEIS.Client.Localization;
using SEOJIN_MEIS.Client.Mock;
using SEOJIN_MEIS.Client.Theme;

namespace SEOJIN_MEIS.Client.Forms.WorkStatus
{
    /// <summary>
    /// MEIS &gt; 작업 현황 조회 &gt; 작업 조회 현황.
    ///
    /// 흐름:
    ///  1) 조회 조건(지시일자 범위 · 작업지시번호 · 생산품목 · 시리얼번호 · 작업지시 상태)으로 [조회]
    ///     → [작업 지시 정보] 목록
    ///  2) 목록에서 한 건을 고르면 → [작업 진행 현황]이 그 작업지시의 공정·스팩별 진행 상태로 바뀐다
    ///  3) 진행 현황은 [진행 상태] / [스팩 결과 상태] 필터로 화면에서 바로 좁혀 볼 수 있다
    ///
    /// 조회·집계·판정은 전부 서버가 하고 화면은 결과 표시와 조건 전달만 한다.
    /// (필터 두 개는 이미 받아 둔 목록을 화면에서 좁히는 것이라 서버를 다시 부르지 않는다.)
    ///
    /// VS 디자이너 호환을 위해 매개변수 없는 생성자만 두고, 실제 의존성은 Initialize()로 주입한다.
    /// </summary>
    public partial class WorkStatusScreen : XtraUserControl, IThemedScreen
    {
        private ClientContext _context;

        /// <summary>선택한 작업지시의 진행 현황 원본(필터 전). 필터는 이 목록을 걸러 그리드에 다시 물린다.</summary>
        private List<WorkProgressDto> _progress = new List<WorkProgressDto>();

        /// <summary>
        /// 진행 현황을 이미 읽어 둔 작업지시번호.
        /// 조회 직후에는 포커스 이동 이벤트와 조회 코드가 같은 지시를 가리켜 두 번 읽힐 수 있어,
        /// 같은 지시면 다시 읽지 않도록 이 값으로 걸러 준다.
        /// </summary>
        private string _loadedWorkOrderNo;

        public WorkStatusScreen()
        {
            InitializeComponent();
        }

        public void Initialize(ClientContext context)
        {
            _context = context;

            ApplyTexts();
            ApplyTheme();
            ConfigureGrids();
            FillFilterCombos();

            // 지시일자 기본값: 최근 한 달.
            dateFrom.EditValue = DateTime.Today.AddMonths(-1);
            dateTo.EditValue = DateTime.Today;

            // 바코드 스캐너처럼 Enter로 끝나는 입력을 그대로 조회로 연결한다.
            txtWorkOrderNo.KeyDown += Condition_KeyDown;
            txtSerialNo.KeyDown += Condition_KeyDown;
            txtItemCode.KeyDown += Condition_KeyDown;

            // 품목코드를 직접 고쳐 쓰면 옆의 품목명은 더 이상 맞지 않으므로 비운다(팝업으로 고르면 다시 채워진다).
            txtItemCode.EditValueChanged += (s, e) => txtItemName.Text = string.Empty;

            // 작업지시를 고를 때마다 아래 진행 현황을 그 지시 것으로 바꾼다.
            gridViewOrders.FocusedRowChanged += async (s, e) => await LoadProgressAsync();
        }

        private void ApplyTexts()
        {
            btnSearch.Text = Strings.Common_Search;
            lblOrderDate.Text = "· " + Strings.WorkStatus_OrderDate + "(*)";
            lblDateTilde.Text = "~";
            lblWorkOrderNo.Text = "· " + Strings.WorkEntry_WorkOrderNo;
            lblItem.Text = "· " + Strings.WorkStatus_Item;
            lblSerialNo.Text = "· " + Strings.WorkEntry_SerialNo;
            lblStatus.Text = "· " + Strings.Col_StatusName;

            lblOrderTitle.Text = Strings.WorkStatus_OrderListTitle;
            lblProgressTitle.Text = Strings.WorkStatus_ProgressTitle;
            lblProgressState.Text = "· " + Strings.Col_ProgressState;
            lblSpecResultState.Text = "· " + Strings.Col_SpecResultState;
        }

        /// <summary>현재 테마 색을 화면에 입힌다(포인트 컬러: Cyan).</summary>
        public void ApplyTheme()
        {
            BackColor = AppTheme.Surface;
            spacerTop.BackColor = AppTheme.Surface;
            spacerBottom.BackColor = AppTheme.Surface;

            AppTheme.CardPanel(topCard);
            AppTheme.CardPanel(orderCard);
            AppTheme.CardPanel(progressCard);
            AppTheme.PlainPanel(filterPanel);

            AppTheme.SectionTitle(lblOrderTitle, AppTheme.Cyan);
            AppTheme.SectionTitle(lblProgressTitle, AppTheme.Violet);

            AppTheme.BodyText(lblOrderDate);
            AppTheme.BodyText(lblDateTilde);
            AppTheme.BodyText(lblWorkOrderNo);
            AppTheme.BodyText(lblItem);
            AppTheme.BodyText(lblSerialNo);
            AppTheme.BodyText(lblStatus);
            AppTheme.BodyText(lblProgressState);
            AppTheme.BodyText(lblSpecResultState);

            AppTheme.Editor(dateFrom);
            AppTheme.Editor(dateTo);
            AppTheme.Editor(txtWorkOrderNo);
            AppTheme.Editor(txtItemCode);
            AppTheme.Editor(txtItemName);
            AppTheme.Editor(txtSerialNo);
            AppTheme.Editor(cboStatus);
            AppTheme.Editor(cboProgressState);
            AppTheme.Editor(cboSpecResultState);

            AppTheme.AccentButton(btnSearch, AppTheme.Cyan, AppIcons.Search);
            AppTheme.IconButton(btnItemPick, AppIcons.Search, AppTheme.Cyan, 16);

            AppTheme.Grid(gridOrders, gridViewOrders, AppTheme.Cyan);
            AppTheme.Grid(gridProgress, gridViewProgress, AppTheme.Violet);
        }

        /// <summary>목록 컬럼 구성(순서·헤더 문구·서식). 헤더 문구는 다국어 리소스에서 가져온다.</summary>
        private void ConfigureGrids()
        {
            gridOrders.DataSource = new List<WorkOrderEntryDto>();
            GridColumns.Apply(gridViewOrders, Strings.Col_No,
                GridColumns.Text("WorkOrderNo", Strings.Col_WorkOrderNo, 140),
                GridColumns.Text("ItemCode", Strings.Col_ItemCode, 100),
                GridColumns.Text("SerialNo", Strings.Col_SerialNo, 110),
                GridColumns.Number("OrderQty", Strings.Col_OrderQty, 110),
                GridColumns.Text("SalesOrderNo", Strings.Col_SalesOrderNo, 110),
                GridColumns.Text("CustomerPoNo", Strings.Col_CustomerPoNo, 130),
                GridColumns.Text("Fcid", Strings.Col_Fcid, 110),
                GridColumns.Date("PlanStartDate", Strings.Col_PlanStartDate, 120),
                GridColumns.Date("PlanEndDate", Strings.Col_PlanEndDate, 120),
                GridColumns.Date("ActualStartDate", Strings.Col_ActualStartDate, 120),
                GridColumns.Date("ActualEndDate", Strings.Col_ActualEndDate, 120),
                GridColumns.Text("StatusName", Strings.Col_StatusName, 110),
                GridColumns.Left("Remark", Strings.Col_Remark, 200));

            gridProgress.DataSource = new List<WorkProgressDto>();
            GridColumns.Apply(gridViewProgress, Strings.Col_No,
                GridColumns.Text("RoutingGroup", Strings.Col_RoutingGroup, 150),
                GridColumns.Text("RoutingOperation", Strings.Col_RoutingOperation, 150),
                GridColumns.Text("PageOperationName", Strings.Col_PageOperationName, 150),
                GridColumns.Text("SpecName", Strings.Col_SpecName, 150),
                GridColumns.Text("SpecType", Strings.Col_SpecTypeName, 130),
                GridColumns.Text("ProgressState", Strings.Col_ProgressState, 130),
                GridColumns.Text("SpecResultState", Strings.Col_SpecResultState, 140));

            gridViewProgress.OptionsView.ColumnAutoWidth = true;

            // 결과가 아직 없는 줄은 빈칸 대신 "-"로 보여 준다(POP 화면과 같은 표기).
            gridViewProgress.CustomColumnDisplayText += (s, e) =>
            {
                if (e.Column.FieldName == "SpecResultState" && string.IsNullOrEmpty(e.Value as string))
                    e.DisplayText = "-";
            };
        }

        /// <summary>조회 조건/필터 콤보를 채운다. 첫 항목은 항상 "전체"(조건 없음)다.</summary>
        private void FillFilterCombos()
        {
            FillCombo(cboStatus, Strings.WorkStatus_StatusWaiting, Strings.WorkStatus_StatusRunning,
                Strings.WorkStatus_StatusDone, Strings.WorkStatus_StatusHold);

            FillCombo(cboProgressState, Strings.WorkStatus_StatusWaiting, Strings.WorkStatus_StatusRunning,
                Strings.WorkStatus_StatusDone);

            FillCombo(cboSpecResultState, Strings.Pop_ResultPass, Strings.Pop_ResultFail);
        }

        private static void FillCombo(ComboBoxEdit combo, params string[] values)
        {
            combo.Properties.Items.Clear();
            combo.Properties.Items.Add(Strings.Common_All);
            foreach (var value in values)
                combo.Properties.Items.Add(value);

            combo.SelectedIndex = 0;
        }

        private void Condition_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            e.Handled = true;
            _ = SearchAsync();
        }

        private async void btnSearch_Click(object sender, EventArgs e) => await SearchAsync();

        /// <summary>
        /// 작업 지시 정보를 조회한다. 지시일자는 필수이며, 시작이 종료보다 늦으면 안내 후 중단한다.
        /// </summary>
        private async Task SearchAsync()
        {
            var from = dateFrom.EditValue as DateTime?;
            var to = dateTo.EditValue as DateTime?;

            if (from == null || to == null)
            {
                AppMessageBox.Warn(this, Strings.WorkStatus_DateRequired, Strings.WorkStatus_Title);
                dateFrom.Focus();
                return;
            }

            if (from > to)
            {
                AppMessageBox.Warn(this, Strings.WorkStatus_DateReversed, Strings.WorkStatus_Title);
                dateFrom.Focus();
                return;
            }

            var query = new WorkStatusQuery();
            query.OrderDateFrom = from;
            query.OrderDateTo = to;
            query.WorkOrderNo = txtWorkOrderNo.Text?.Trim();
            query.ItemCode = txtItemCode.Text?.Trim();
            query.SerialNo = txtSerialNo.Text?.Trim();
            query.StatusName = SelectedFilterValue(cboStatus);

            using (var busy = BusyOverlay.Show(this, cancelable: true))
            {
                try
                {
                    btnSearch.Enabled = false;
                    var result = await FetchWorkOrdersAsync(query, busy.Token);

                    if (IsDisposed || Disposing) return;   // 조회 중 탭을 닫았다

                    // 새로 조회한 결과이므로 이전에 읽어 둔 진행 현황은 버린다(같은 지시라도 다시 읽는다).
                    ClearProgress();
                    gridOrders.DataSource = result;

                    if (result.Count == 0)
                    {
                        AppMessageBox.Info(this, Strings.WorkEntry_NoResult, Strings.WorkStatus_Title);
                        return;
                    }

                    // 첫 행을 고르면 FocusedRowChanged가 걸려 진행 현황도 함께 채워진다.
                    // 이미 0번 행에 포커스가 있어 이벤트가 안 걸리는 경우를 위해 한 번 더 호출한다(중복은 위에서 걸러진다).
                    gridViewOrders.FocusedRowHandle = 0;
                    await LoadProgressAsync();
                }
                catch (OperationCanceledException)
                {
                    // 사용자가 [취소]를 눌렀다 — 오류가 아니므로 조용히 끝낸다.
                }
                catch (ApiException ex)
                {
                    if (IsDisposed || Disposing) return;
                    AppMessageBox.Warn(this, ex.Message, Strings.WorkStatus_Title);
                }
                finally
                {
                    if (!IsDisposed && !Disposing)
                        btnSearch.Enabled = true;
                }
            }
        }

        private async Task<List<WorkOrderEntryDto>> FetchWorkOrdersAsync(WorkStatusQuery query, CancellationToken cancellationToken)
        {
            if (_context.Settings.UseMockData)
            {
                await Task.Yield();
                return MockWorkStatusData.SearchWorkOrders(query);
            }

            return await _context.ApiClients.WorkStatusApiClient.SearchWorkOrdersAsync(query, cancellationToken)
                   ?? new List<WorkOrderEntryDto>();
        }

        /// <summary>선택한 작업지시의 진행 현황을 읽어 아래 목록에 채운다(선택이 없으면 비운다).</summary>
        private async Task LoadProgressAsync()
        {
            var selected = gridViewOrders.GetFocusedRow() as WorkOrderEntryDto;
            if (selected == null)
            {
                ClearProgress();
                return;
            }

            // 같은 작업지시를 이미 읽어 뒀으면 건너뛴다(조회 직후 중복 호출 방지).
            if (string.Equals(_loadedWorkOrderNo, selected.WorkOrderNo, StringComparison.OrdinalIgnoreCase))
                return;

            _loadedWorkOrderNo = selected.WorkOrderNo;

            try
            {
                _progress = await FetchProgressAsync(selected.WorkOrderNo, selected.SerialNo);
                ApplyProgressFilter();
            }
            catch (ApiException ex)
            {
                ClearProgress();
                AppMessageBox.Warn(this, ex.Message, Strings.WorkStatus_ProgressTitle);
            }
        }

        private async Task<List<WorkProgressDto>> FetchProgressAsync(string workOrderNo, string serialNo)
        {
            if (_context.Settings.UseMockData)
            {
                await Task.Yield();
                return MockWorkStatusData.GetWorkProgress(workOrderNo, serialNo);
            }

            return await _context.ApiClients.WorkStatusApiClient.GetWorkProgressAsync(workOrderNo, serialNo)
                   ?? new List<WorkProgressDto>();
        }

        private void ClearProgress()
        {
            _loadedWorkOrderNo = null;
            _progress = new List<WorkProgressDto>();
            gridProgress.DataSource = _progress;
        }

        private void Filter_SelectedIndexChanged(object sender, EventArgs e) => ApplyProgressFilter();

        /// <summary>
        /// 두 필터(진행 상태 / 스팩 결과 상태)로 진행 현황 목록을 좁혀 다시 물린다.
        /// 이미 받아 둔 목록을 거르는 것이라 서버를 다시 부르지 않는다.
        /// </summary>
        private void ApplyProgressFilter()
        {
            var progressState = SelectedFilterValue(cboProgressState);
            var resultState = SelectedFilterValue(cboSpecResultState);

            var filtered = _progress
                .Where(row => progressState == null || string.Equals(row.ProgressState, progressState, StringComparison.OrdinalIgnoreCase))
                .Where(row => resultState == null || string.Equals(row.SpecResultState, resultState, StringComparison.OrdinalIgnoreCase))
                .ToList();

            gridProgress.DataSource = filtered;
        }

        /// <summary>콤보에서 고른 값. "전체"(첫 항목)면 조건 없음을 뜻하는 null을 돌려준다.</summary>
        private static string SelectedFilterValue(ComboBoxEdit combo)
        {
            var value = combo.SelectedItem as string;
            return string.IsNullOrEmpty(value) || value == Strings.Common_All ? null : value;
        }

        /// <summary>생산품목 돋보기 — &lt;생산품목 선택&gt; 팝업에서 고른 품목을 조건에 채운다.</summary>
        private void btnItemPick_Click(object sender, EventArgs e)
        {
            using (var picker = new ItemPickerForm())
            {
                picker.Initialize(_context, txtItemCode.Text?.Trim());
                picker.CenterOn(this);
                if (picker.ShowDialog(this) != DialogResult.OK || picker.SelectedItem == null)
                    return;

                txtItemCode.Text = picker.SelectedItem.ItemCode;
                txtItemName.Text = picker.SelectedItem.ItemName;
            }
        }
    }
}
