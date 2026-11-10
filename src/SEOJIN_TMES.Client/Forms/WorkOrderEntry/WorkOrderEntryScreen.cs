using DevExpress.Xpo.DB.Helpers;
using DevExpress.XtraEditors;
using SEOJIN_TMES.Client.ApiClient;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Composition;
using SEOJIN_TMES.Client.Core;
using SEOJIN_TMES.Client.Forms.Common;
using SEOJIN_TMES.Client.Forms.Pop;
using SEOJIN_TMES.Client.Forms.WorkStatus;
using SEOJIN_TMES.Client.Theme;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        //private WorkOrderQuery _query;

        /// <summary>조회 조건 statusList에 보내는 작업지시 상태 코드(서버 값).</summary>
        private const string StatusReady = "Ready";
        private const string StatusProgress = "Progress";
        private const string StatusCompleted = "Completed";

        /// <summary>상태 앞의 상태 구슬 컬럼(이벤트를 걸어 두므로 화면과 수명을 같이한다).</summary>
        private StateImageColumn _statusImage;

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
            //_query = new WorkOrderQuery();

            lblGridTitle.Text = Language.WorkEntry_ListTitle;
            lblWorkOrderNo.Text = "· " + Language.WorkEntry_WorkOrderNo + "(*)";
            lblSerialNo.Text = "· " + Language.WorkEntry_SerialNo + "(*)";
            lblItemCode.Text = "· " + Language.ItemPicker_Keyword;
            btnSearch.Text = Language.WorkEntry_Search;
            btnWork.Text = Language.WorkEntry_Work;
            chkWorkStationEnable.Text = Language.WorkEntry_WorkStation;
            chkReady.Text = Language.ProgressState_Ready;
            chkProgress.Text = Language.ProgressState_Progress;
            chkCompleted.Text = Language.ProgressState_Finished;
            LayoutStatusChecks();

            ApplyTheme();
            ConfigureGrid();

            // 바코드 스캐너처럼 Enter로 끝나는 입력을 그대로 조회로 연결한다.
            txtWorkOrderNo.KeyDown += Condition_KeyDown;
            txtSerialNo.KeyDown += Condition_KeyDown;

            // 목록에서 더블클릭하면 [작업]과 같게 동작한다.
            // 헤더 · 빈 영역 더블클릭에는 반응하지 않도록 RowDoubleClick을 쓴다(MeisGridView 참고).
            gridView.RowDoubleClick += gridView_RowDoubleClick;
            gridView.CustomColumnDisplayText += gridView_CustomColumnDisplayText;
        }

        /// <summary>
        /// [워크스테이션] 옆으로 [대기] · [진행중] · [완료]를 이어 붙인다. 체크박스 글자 폭이 언어마다 달라
        /// 디자이너의 고정 위치로 두면 긴 문구(베트남어 등)가 다음 체크박스와 겹친다.
        /// </summary>
        private void LayoutStatusChecks()
        {
            const int gap = 12;
            chkWorkStationEnable.Size = chkWorkStationEnable.PreferredSize;
            chkReady.Size = chkReady.PreferredSize;
            chkProgress.Size = chkProgress.PreferredSize;
            chkCompleted.Size = chkCompleted.PreferredSize;
            chkReady.Left = chkWorkStationEnable.Right + gap;
            chkProgress.Left = chkReady.Right + gap;
            chkCompleted.Left = chkProgress.Right + gap;
        }

        /// <summary>작업지시 상태(서버 코드 Ready/Progress/Completed 등)를 화면 언어의 문구로 보여 준다. 값 자체는 바꾸지 않는다.</summary>
        private void gridView_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column == null || e.Column.FieldName != nameof(WorkOrderEntryDto.StatusName))
                return;

            var value = e.Value as string;
            if (string.IsNullOrEmpty(value))
                return;

            e.DisplayText = Language.ProgressStateText(value);
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
            AppTheme.BodyText(lblItemCode);
            AppTheme.CodeEditor(txtWorkOrderNo);
            AppTheme.CodeEditor(txtSerialNo);
            AppTheme.CodeEditor(txtItemCode);
            AppTheme.CodeEditor(txtItemName);
            AppTheme.StandardButton(btnSearch);
            AppTheme.StandardButton(btnWork);

            AppTheme.Grid(grid, gridView, AppTheme.Indigo);
        }

        /// <summary>목록 컬럼 구성(순서·헤더 문구·서식). 헤더 문구는 다국어 리소스에서 가져온다.</summary>
        private void ConfigureGrid()
        {
            grid.DataSource = new List<WorkOrderEntryDto>();
            GridColumns.Apply(gridView, Language.Col_No,
                GridColumns.Text("ItemCode", Language.Col_ItemCode, 100),
                GridColumns.Text("WorkOrderNo", Language.Col_WorkOrderNo, 140),
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

            // 상태 앞에 상태 구슬 그림 — 헤더는 상태와 한 칸으로 합쳐 보인다.
            _statusImage = new StateImageColumn(gridView, nameof(WorkOrderEntryDto.StatusName));
            _statusImage.Attach();
        }

        /// <summary>
        /// 조회 조건의 작업지시 상태 목록 — [대기] · [진행중] · [완료] 체크로 고른다.
        /// 체크한 상태만 그대로 statusList로 보낸다(statusList=Ready&amp;statusList=Progress&amp;statusList=Completed).
        /// 아무것도 체크하지 않았을 때 대신 채워 넣지 않는다 — 빈 조건을 어떻게 볼지는 서버가 정한다.
        /// [완료]는 지난 작업을 찾아볼 때만 쓰므로 기본은 꺼 둔다(Designer).
        /// </summary>
        private List<string> BuildStatusList()
        {
            var statusList = new List<string>();
            if (chkReady.Checked)
                statusList.Add(StatusReady);
            if (chkProgress.Checked)
                statusList.Add(StatusProgress);
            if (chkCompleted.Checked)
                statusList.Add(StatusCompleted);

            return statusList;
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

        private async void gridView_RowDoubleClick(object sender, EventArgs e)
        {
            try
            {
                await StartWorkAsync();
            }
            catch (Exception ex)
            {
                // async void에서 예외가 새면 앱이 그대로 종료된다.
                AppLog.Error("작업 진입 화면 - 목록 더블클릭 처리 실패", ex);
            }
        }

        /// <summary>
        /// 작업지시 조회. 두 조건 중 최소 하나는 입력해야 한다
        /// (둘 다 비우고 전체를 긁어오면 POP 화면에서 의미가 없으므로 막는다 — 실제 운영 규칙 확정 시 조정).
        /// </summary>
        private async Task SearchAsync()
        {
            var workOrderNo = txtWorkOrderNo.Text?.Trim();
            var serialNo = txtSerialNo.Text?.Trim();

            
            try
            {
                btnSearch.Enabled = false;
                List<WorkOrderEntryDto> result;
                WorkOrderQuery _query = new WorkOrderQuery
                {
                    CorpCode = _appSet.CurrentUser?.Server,
                    WorkOrderNo = workOrderNo,
                    SerialNo = serialNo,
                    ItemCode = txtItemCode.Text?.Trim(),
                    ItemName = txtItemName.Text?.Trim(),
                    WorkStationRef = _appSet.CurrentUser?.WorkstationInfo?.WorkstationRef,
                    StatusList = BuildStatusList()
                };
                
                result = await FetchWorkOrdersAsync(_query, chkWorkStationEnable.Checked);
                
                if (IsDisposed || Disposing) return;   // 조회 중 탭을 닫았다

                grid.DataSource = result;
                gridView.BestFitColumns();
                _statusImage.Attach();   // 맞춤 폭 계산이 빈 그림 컬럼을 줄여 놓으므로 폭 · 위치를 다시 잡는다

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

        /// <summary>조회 결과를 서버에서 가져온다.</summary>
        private async Task<List<WorkOrderEntryDto>> FetchWorkOrdersAsync(WorkOrderQuery query, bool workStationEnable, CancellationToken cancellationToken = default)
        {
            return await _appSet.ApiClients.WorkOrderApiClient.SearchWorkOrdersAsync(query, workStationEnable, cancellationToken)
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

            // 워크스테이션은 메인 화면이 뜰 때 고른다 — 목록이 없었거나 [취소]했으면 비어 있다.
            // 공정 조회 · POP 화면이 모두 워크스테이션 기준이라 작업 진입 확인 창을 띄우지 않고 여기서 멈춘다.
            if (string.IsNullOrEmpty(_appSet.CurrentUser?.WorkstationInfo?.WorkstationRef))
            {
                AppLog.Warn($"작업 진입 불가(워크스테이션 없음) - WorkOrderNo={selected.WorkOrderNo}");
                AppMessageBox.Warn(this, Language.WorkEntry_NoWorkstation, Language.WorkEntryConfirm_Title);
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

        /// <summary>선택한 작업지시의 공정(라우팅) 목록을 서버에서 가져온다.</summary>
        private async Task<List<RoutingProcessDto>> FetchRoutingProcessesAsync(string corpCode, string workStationRef, string serialNo, CancellationToken cancellationToken)
        {
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

        private void btnItemPick_Click(object sender, EventArgs e)
        {
            using (var picker = new ItemPickerForm())
            {
                //picker.Init(txtItemCode.Text?.Trim());
                picker.CenterOn(this);
                if (picker.ShowDialog(this) != DialogResult.OK || picker.SelectedItem == null)
                    return;

                txtItemCode.Text = picker.SelectedItem.ItemCode;
                txtItemName.Text = picker.SelectedItem.ItemName;
            }
        }
    }
}
