using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using SEOJIN_TMES.Client.ApiClient;
using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Composition;
using SEOJIN_TMES.Client.Forms.Common;
using SEOJIN_TMES.Client.Forms.Pop;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Mock;
using SEOJIN_TMES.Client.Theme;
using SEOJIN_TMES.Client.Core;

namespace SEOJIN_TMES.Client.Forms.WorkOrderEntry
{
    /// <summary>
    /// POP 진입(도입) 화면 — MEIS &gt; 작업관리 &gt; POP을 열면 이 화면이 먼저 보인다.
    ///
    /// 흐름:
    ///  1) 작업지시번호 / 생산품목 시리얼번호로 [조회] → 작업 지시 정보 목록 표시
    ///  2) 목록에서 한 건 선택 후 [작업] → 선택한 작업지시의 라우팅(공정) 목록을 받아 &lt;작업 진입 확인&gt; 팝업 표시
    ///  3) 팝업에서 이동할 페이지 공정을 선택하고 [이동] → 같은 탭이 POP 화면으로 바뀐다(<see cref="INavigatingScreen"/>)
    ///
    /// VS 디자이너 호환을 위해 매개변수 없는 생성자만 두고, 실제 의존성은 Initialize()로 주입한다.
    /// </summary>
    public partial class WorkOrderEntryScreen : XtraUserControl, IThemedScreen, INavigatingScreen
    {
        //private ClientContext _context;
        private Setting _appSet;
        /// <summary>[이동] 선택 시 같은 탭에 띄울 화면을 MainForm에 요청한다.</summary>
        public event EventHandler<ScreenNavigationEventArgs> NavigateRequested;

        public WorkOrderEntryScreen()
        {
            InitializeComponent();
        }

        public void Init()
        {
            //_context = context;
            _appSet = Global.Instance.Settings;

            lblGridTitle.Text = Language.WorkEntry_ListTitle;
            lblWorkOrderNo.Text = "· " + Language.WorkEntry_WorkOrderNo + "(*)";
            lblSerialNo.Text = "· " + Language.WorkEntry_SerialNo + "(*)";
            btnSearch.Text = Language.WorkEntry_Search;
            btnWork.Text = Language.WorkEntry_Work;

            ApplyTheme();
            ConfigureGrid();

            // 바코드 스캐너처럼 Enter로 끝나는 입력을 그대로 조회로 연결한다.
            txtWorkOrderNo.KeyDown += Condition_KeyDown;
            txtSerialNo.KeyDown += Condition_KeyDown;

            // 목록에서 더블클릭하면 [작업]과 같게 동작한다.
            gridView.DoubleClick += async (s, e) => await StartWorkAsync();
        }

        /// <summary>현재 테마 색을 화면에 입힌다(포인트 컬러: Indigo).</summary>
        public void ApplyTheme()
        {
            BackColor = AppTheme.Surface;
            spacer.BackColor = AppTheme.Surface;

            AppTheme.CardPanel(topCard);
            AppTheme.CardPanel(gridCard);
            AppTheme.SectionTitle(lblGridTitle, AppTheme.Indigo);

            AppTheme.BodyText(lblWorkOrderNo);
            AppTheme.BodyText(lblSerialNo);
            AppTheme.CodeEditor(txtWorkOrderNo);
            AppTheme.CodeEditor(txtSerialNo);

            btnSearch.Icon = AppIcons.Search;
            btnWork.Icon = AppIcons.Play;

            AppTheme.Grid(grid, gridView, AppTheme.Indigo);
        }

        /// <summary>목록 컬럼 구성(순서·헤더 문구·서식). 헤더 문구는 다국어 리소스에서 가져온다.</summary>
        private void ConfigureGrid()
        {
            grid.DataSource = new List<WorkOrderEntryDto>();
            GridColumns.Apply(gridView, Language.Col_No,
                GridColumns.Text("WorkOrderNo", Language.Col_WorkOrderNo, 140),
                GridColumns.Text("ItemCode", Language.Col_ItemCode, 100),
                GridColumns.Text("SerialNo", Language.Col_SerialNo, 110),
                GridColumns.Number("OrderQty", Language.Col_OrderQty, 110),
                GridColumns.Text("SalesOrderNo", Language.Col_SalesOrderNo, 110),
                GridColumns.Text("CustomerPoNo", Language.Col_CustomerPoNo, 130),
                GridColumns.Number("WorkQty", Language.Col_WorkQty, 110),
                GridColumns.Text("Fcid", Language.Col_Fcid, 110),
                GridColumns.Date("PlanStartDate", Language.Col_PlanStartDate, 120),
                GridColumns.Date("PlanEndDate", Language.Col_PlanEndDate, 120),
                GridColumns.Date("WorkStartDate", Language.Col_ActualStartDate, 120),
                GridColumns.Date("WorkEndDate", Language.Col_ActualEndDate, 120),
                GridColumns.Text("StatusName", Language.Col_StatusName, 110),
                GridColumns.Left("Remark", Language.Col_Remark, 200));

            //GridColumns.Apply(gridView, Language.Col_No,
            //    GridColumns.Text("woNo", Language.Col_WorkOrderNo, 140),
            //    GridColumns.Text("itemCd", Language.Col_ItemCode, 100),
            //    GridColumns.Text("sn", Language.Col_SerialNo, 110),
            //    GridColumns.Number("qty", Language.Col_OrderQty, 110),
            //    GridColumns.Text("soNo", Language.Col_SalesOrderNo, 110),
            //    GridColumns.Text("custPoNo", Language.Col_CustomerPoNo, 130),
            //    GridColumns.Text("fcid", Language.Col_Fcid, 110),
            //    GridColumns.Date("ppStartDt", Language.Col_PlanStartDate, 120),
            //    GridColumns.Date("ppEndDt", Language.Col_PlanEndDate, 120),
            //    GridColumns.Date("workStartDt", Language.Col_ActualStartDate, 120),
            //    GridColumns.Date("workEndDt", Language.Col_ActualEndDate, 120),
            //    GridColumns.Text("status", Language.Col_StatusName, 110),
            //    GridColumns.Left("remark", Language.Col_Remark, 200));
        }

        private void Condition_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            e.Handled = true;
            _ = SearchAsync();
        }

        private async void btnSearch_Click(object sender, EventArgs e) => await SearchAsync();
        private async void btnWork_Click(object sender, EventArgs e) => await StartWorkAsync();

        /// <summary>
        /// 작업지시 조회. 두 조건 중 최소 하나는 입력해야 한다
        /// (둘 다 비우고 전체를 긁어오면 POP 화면에서 의미가 없으므로 막는다 — 실제 운영 규칙 확정 시 조정).
        /// </summary>
        private async Task SearchAsync()
        {
            var workOrderNo = txtWorkOrderNo.Text?.Trim();
            var serialNo = txtSerialNo.Text?.Trim();

            //if (string.IsNullOrEmpty(workOrderNo) && string.IsNullOrEmpty(serialNo))
            //{
            //    AppMessageBox.Warn(this, Language.WorkEntry_ConditionRequired, Language.WorkEntry_Title);
            //    txtWorkOrderNo.Focus();
            //    return;
            //}

            using (var busy = BusyOverlay.Show(this, cancelable: true))
            {
                try
                {
                    btnSearch.Enabled = false;
                    List<WorkOrderEntryDto> result;

                    if (string.IsNullOrEmpty(workOrderNo) && string.IsNullOrEmpty(serialNo))
                    {
                        result = await FetchWorkOrdersAsync(busy.Token);
                    }
                    else
                    {
                        result = await FetchWorkOrdersAsync(workOrderNo, serialNo, busy.Token);
                    }
                    //var result = await FetchWorkOrdersAsync(workOrderNo, serialNo, busy.Token);
                    //var result = await FetchWorkOrdersAsync(busy.Token);
                    if (IsDisposed || Disposing) return;   // 조회 중 탭을 닫았다

                    grid.DataSource = result;
                    gridView.BestFitColumns();

                    if (result.Count == 0)
                        AppMessageBox.Info(this, Language.WorkEntry_NoResult, Language.WorkEntry_Title);
                    else
                        gridView.FocusedRowHandle = 0;
                }
                catch (OperationCanceledException)
                {
                    // 사용자가 [취소]를 눌렀다 — 오류가 아니므로 조용히 끝낸다.
                }
                catch (ApiException ex)
                {
                    if (IsDisposed || Disposing) return;
                    AppMessageBox.Warn(this, ex.Message, Language.WorkEntry_Title);
                }
                finally
                {
                    if (!IsDisposed && !Disposing)
                        btnSearch.Enabled = true;
                }
            }
        }

        /// <summary>
        /// 조회 결과를 가져온다. 목업 모드(App.config의 UseMockData=true)면 서버를 호출하지 않고
        /// 화면 설계서 샘플 데이터를 돌려준다 — 서버 없이도 화면/컬럼/서식을 그대로 확인하기 위한 임시 경로다.
        /// </summary>
        private async Task<List<WorkOrderEntryDto>> FetchWorkOrdersAsync(string workOrderNo, string serialNo, CancellationToken cancellationToken)
        {
            //if (_appSet.AppSet.UseMockData)
            //{
            //    // 서버 왕복처럼 보이도록 한 틱 양보한다(UI가 먼저 그려지고 버튼 비활성 상태가 보이도록).
            //    await Task.Yield();
            //    return MockWorkOrderData.SearchWorkOrders(workOrderNo, serialNo);
            //}

            return await _appSet.ApiClients.WorkOrderApiClient.SearchWorkOrdersAsync(_appSet.CurrentUser.Server, workOrderNo, serialNo, cancellationToken)
                   ?? new List<WorkOrderEntryDto>();
        }

        private async Task<List<WorkOrderEntryDto>> FetchWorkOrdersAsync(CancellationToken cancellationToken)
        {
            //if (_appSet.AppSet.UseMockData)
            //{
            //    // 서버 왕복처럼 보이도록 한 틱 양보한다(UI가 먼저 그려지고 버튼 비활성 상태가 보이도록).
            //    await Task.Yield();
            //    return MockWorkOrderData.SearchWorkOrders(workOrderNo, serialNo);
            //}

            return await _appSet.ApiClients.WorkOrderApiClient.SearchWorkOrdersAsync(_appSet.CurrentUser.Server, cancellationToken)
                   ?? new List<WorkOrderEntryDto>();
        }

        /// <summary>선택한 작업지시의 공정 목록을 받아 &lt;작업 진입 확인&gt; 팝업을 띄운다.</summary>
        private async Task StartWorkAsync()
        {
            var selected = gridView.GetFocusedRow() as WorkOrderEntryDto;
            if (selected == null)
            {
                AppMessageBox.Warn(this, Language.WorkEntry_SelectWorkOrder, Language.WorkEntry_Title);
                return;
            }

            List<RoutingProcessDto> processes;

            // 공정 목록을 받아오는 것뿐이라(아직 작업을 시작하지 않는다) 취소해도 남는 것이 없다.
            using (var busy = BusyOverlay.Show(this, cancelable: true))
            {
                try
                {
                    var workStationRef = _appSet.CurrentUser?.WorkstationInfo?.WorkstationRef;

                    btnWork.Enabled = false;
                    processes = await FetchRoutingProcessesAsync(selected.CorpCode, workStationRef, selected.SerialNo, busy.Token);

                    if (IsDisposed || Disposing) return;
                }
                catch (OperationCanceledException)
                {
                    return;   // 사용자가 [취소]를 눌렀다
                }
                catch (ApiException ex)
                {
                    if (IsDisposed || Disposing) return;
                    AppMessageBox.Warn(this, ex.Message, Language.WorkEntryConfirm_Title);
                    return;
                }
                finally
                {
                    if (!IsDisposed && !Disposing)
                        btnWork.Enabled = true;
                }
            }

            if (processes.Count == 0)
            {
                AppMessageBox.Warn(this, Language.WorkEntry_NoRouting, Language.WorkEntryConfirm_Title);
                return;
            }

            RoutingProcessDto process;
            using (var confirm = new WorkEntryConfirmForm())
            {
                confirm.Initialize(selected, processes);
                confirm.CenterOn(this);
                if (confirm.ShowDialog(this) != DialogResult.OK)
                    return;

                process = confirm.SelectedProcess;
            }

            if (process == null)
                return;

            OpenPopScreen(new WorkEntryContext(selected, process));
        }

        /// <summary>
        /// 선택한 작업지시의 공정(라우팅) 목록을 가져온다.
        /// 목업 모드면 서버 대신 <see cref="MockWorkOrderData"/>의 샘플 공정을 돌려준다.
        /// </summary>
        private async Task<List<RoutingProcessDto>> FetchRoutingProcessesAsync(string corpCode, string workStationRef, string serialNo, CancellationToken cancellationToken)
        {
            //if (_appSet.AppSet.UseMockData)
            //{
            //    await Task.Yield();
            //    return MockWorkOrderData.GetRoutingProcesses(workOrderNo, serialNo);
            //}

            return await _appSet.ApiClients.WorkOrderApiClient.GetRoutingProcessesAsync(corpCode, workStationRef, serialNo, cancellationToken)
                   ?? new List<RoutingProcessDto>();
        }

        /// <summary>선택 결과를 POP 화면에 넘기고, 같은 탭을 POP 화면으로 바꿔 달라고 요청한다.</summary>
        private void OpenPopScreen(WorkEntryContext entry)
        {
            var pop = new PopScreen(entry);
            var title = $"{Language.Menu_POP} - {entry.WorkOrder.WorkOrderNo}";
            NavigateRequested?.Invoke(this, new ScreenNavigationEventArgs(pop, title));
        }
    }
}
