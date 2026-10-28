using SEOJIN_TMES.Client.ApiClient;
using SEOJIN_TMES.Client.Common.Imaging;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Equipment.Nutrunner;
using SEOJIN_TMES.Client.Forms.Common;
using SEOJIN_TMES.Client.Forms.WorkOrderEntry;
using SEOJIN_TMES.Client.Theme;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SEOJIN_TMES.Client.Forms.Pop
{
    /// <summary>
    /// 공정 상태 전이 · 공정 이동 · 자동 진행 · 실적 전송 · 설비 · 도면 · 아코디언 처리.
    /// 이벤트 핸들러(PopScreen.cs / PopScreen.Grid.cs)가 <see cref="IPopScreen"/>을 거쳐 부른다.
    /// </summary>
    public partial class PopScreen
    {
        // 서버가 내려주는 공정 진행 상태(WorkStatus). 판정은 서버가 하고 화면은 이 값으로 표시만 바꾼다.
        private const string StateReady = "Ready";
        private const string StateProgress = "Progress";
        private const string StateStopped = "Stopped";
        private const string StateFinished = "Finished";

        /// <summary>기준정보에 설비 포트가 없거나 숫자가 아닐 때 쓰는 Modbus 포트(이벤트 포트는 +1).</summary>
        private const int DefaultEquipmentPort = 5000;

        /// <summary>접었을 때 남기는 우측 패널 폭 = 세로 손잡이 띠의 폭.</summary>
        private const int InfoCollapsedWidth = 26;

        /// <summary>펼쳤을 때 우측 패널 폭(디자이너의 infoCard 폭과 같게 둘 것).</summary>
        private const int InfoExpandedWidth = 417;

        /// <summary>펼쳤을 때 하단 영역 높이(디자이너의 bottomCard 높이와 같게 둘 것).</summary>
        private const int BottomExpandedHeight = 192;

        private readonly ByteToImage _imgCtrl = new ByteToImage();

        /// <summary>
        /// [공정 목록]에 뜨는 라우팅 공정들. [이전 공정] · [다음 공정] · 자동 진행도 <b>이 순서</b>를 따라 움직이므로
        /// 목록과 버튼이 서로 다른 곳으로 가는 일이 없다.
        /// </summary>
        private List<RoutingProcessDto> _processes = new List<RoutingProcessDto>();

        /// <summary>자동 완료 · 이동 · 시작이 진행 중인지. 저장 응답이 겹쳐 들어와도 한 번만 돌게 막는다.</summary>
        private bool _autoProgressing;

        /// <summary>
        /// 서버에 보내는 중인 실적 저장(시리얼 · 토크 · 검사) 개수.
        /// 화면 값은 저장 응답 전에 이미 채워져 있으므로, 응답을 기다리지 않고 완료를 보내면
        /// 서버에는 마지막 실적이 없는 채로 공정이 끝날 수 있다 — 0일 때만 완료를 보낸다.
        /// </summary>
        private int _pendingResultSaves;

        private bool _infoExpanded = true;
        private bool _bottomExpanded = true;

        // ─────────────────────────────────────────────────────────────────────────
        //  공통
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// async void 이벤트 핸들러의 공통 예외 처리.
        /// async void에서 예외가 새면 앱이 그대로 종료되므로 핸들러마다 잡아서 여기로 넘긴다.
        /// 서버 오류(<see cref="ApiException"/>)는 사유만 알리고, 그 밖의 예외는 스택까지 로그로 남긴다.
        /// </summary>
        private void HandleError(string actionName, Exception ex)
        {
            var isApiError = ex is ApiException;
            if (isApiError)
                AppLog.Warn($"[POP] {actionName} 실패 - {ex.Message}");
            else
                ServerLog.Unexpected($"POP {actionName}", ex);   // API 오류가 아닌 것은 예기치 못한 오류 — 서버로도 보낸다

            if (IsDisposed || Disposing)
                return;

            if (isApiError)
                AppMessageBox.Warn(this, ex.Message, Language.Pop_Title);
            else
                AppMessageBox.Error(this, ex.Message, Language.Pop_Title);
        }

        private bool IsProgressState(string state) =>
            string.Equals(_progressState, state, StringComparison.OrdinalIgnoreCase);

        /// <summary>진입 화면에서 받은 작업 대상을 조회 키로 바꾼다. 대상이 없으면 null.</summary>
        private PopProcessKey BuildKey()
        {
            if (_entry?.WorkOrder == null || _entry.Process == null)
                return null;

            var key = new PopProcessKey();
            key.WorkOrderNo = _entry.WorkOrder.WorkOrderNo;
            key.SerialNo = _entry.WorkOrder.SerialNo;
            key.RoutingGroup = _entry.Process.RoutingGroup;
            key.RoutingGroupRef = _entry.Process.RoutingGroupRef;
            key.RoutingOperation = _entry.Process.RoutingName;
            key.PageOperation = _entry.Process.PageName;
            key.SubRoutingRef = _entry.Process.SubRoutingReference;
            key.CorpCode = _entry.Process.CorpCode;
            // 워크스테이션 없이는 작업 진입에서 막히지만(WorkOrderEntryScreen), 여기서 죽지 않도록 null을 견딘다.
            key.WorkstationRef = _appSet.CurrentUser?.WorkstationInfo?.WorkstationRef;
            return key;
        }

        /// <summary>상단에 표시할 작업자 — 프로필 이름이 없으면 로그인 아이디를 쓴다.</summary>
        private string ResolveWorkerName()
        {
            var user = _appSet?.CurrentUser;
            if (user == null)
                return string.Empty;

            return string.IsNullOrWhiteSpace(user.UserName) ? user.UserId : user.UserName;
        }

        /// <summary>
        /// &lt;작업 진입 확인&gt; 팝업 제목에 쓸 작업지시.
        /// 진입 화면을 거쳐 왔으면 그 값을 쓰고, 아니면 조회 키의 작업지시번호로 최소한만 채운다.
        /// </summary>
        private WorkOrderEntryDto ResolveWorkOrder()
        {
            if (_entry?.WorkOrder != null)
                return _entry.WorkOrder;

            var stub = new WorkOrderEntryDto();
            stub.WorkOrderNo = _key.WorkOrderNo;
            stub.SerialNo = _key.SerialNo;
            return stub;
        }

        private string FormatTime(DateTime? value) =>
            value?.ToString("HH:mm:ss") ?? string.Empty;

        private string FormatDuration(TimeSpan? value) =>
            value?.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture) ?? string.Empty;

        // ─────────────────────────────────────────────────────────────────────────
        //  공정 정보
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// 진행 중 공정 정보 · 탭 4개 데이터 · 도면을 다시 읽는다(진입 시 / 상태 전이 후 / 공정 이동 후).
        /// 서버 오류는 알리기만 하고 화면은 이전 값 그대로 둔다.
        /// </summary>
        public async Task GetProcessInfo()
        {
            PopScreenData screen;
            try
            {
                screen = await _apiClient.PopApiClient.GetProcessInfoAsync(_key);
            }
            catch (ApiException ex)
            {
                AppLog.Warn($"[POP] 공정 정보 조회 실패 - {ex.Message}");
                if (IsDisposed || Disposing) return;

                AppMessageBox.Warn(this, ex.Message, Language.Pop_Title);
                return;
            }

            if (IsDisposed || Disposing) return;

            if (screen == null)
            {
                AppLog.Warn("[POP] 공정 정보 조회 - 응답 없음");
                return;
            }

            _info = screen.Process;
            _bomManualEnabled = screen.BomManualEnabled;

            SetGridData(screen);
            LoadDrawing();
            SetProcessInfo();
        }

        /// <summary>&lt;진행 중 공정 정보&gt;를 우측 카드에 채운다. 워크스테이션은 로그인 세션 값으로 보완한다.</summary>
        private void SetProcessInfo()
        {
            var info = _info ?? new PopProcessInfoDto();

            txtWorkOrderNo.Text = info.WorkOrderNo;
            txtItemCode.Text = info.ItemCode;
            txtProductSerialNo.Text = info.SerialNo;
            txtRoutingGroup.Text = info.RoutingGroup;
            txtRoutingOperation.Text = info.RoutingOperation;
            txtPageOperation.Text = info.PageOperation;
            txtPageStartTime.Text = FormatTime(info.PageStartTime);
            txtPageEndTime.Text = FormatTime(info.PageEndTime);
            txtPageRunTime.Text = FormatDuration(info.RunTime);
            txtPageIdleTime.Text = FormatDuration(info.IdleTime);
            txtPageWorkTime.Text = FormatDuration(info.WorkTime);

            // 서버가 워크스테이션을 내려주지 않으면 메인 화면에서 고른 워크스테이션을 그대로 보여 준다.
            var workstation = info.WorkstationDisplay;
            if (string.IsNullOrWhiteSpace(workstation) || workstation == "()")
                workstation = _appSet?.CurrentUser?.Workstation;

            lblWorkstation.Text = workstation;
            SetProgressState(info.ProgressState);
        }

        /// <summary>우측 카드 아래 안내 칸에 문구를 띄운다. 언제 일어난 일인지 알 수 있게 시각을 붙인다.</summary>
        private void ShowMessage(string message)
        {
            if (IsDisposed || Disposing)
                return;

            lblMessage.Text = $"[{DateTime.Now:HH:mm:ss}] {message}";
        }

        /// <summary>
        /// 서버가 준 진행 상태로 상태 띠의 색과 중지 버튼을 바꾼다.
        /// 상태 판정은 서버 몫이고, 여기서는 받은 값을 화면에 옮기기만 한다 —
        /// 모르는 값이 오면 기본색(보라)으로 두고 글자만 보여 준다.
        /// </summary>
        private void SetProgressState(string progressState)
        {
            _progressState = progressState;
            lblProgressState.Text = progressState;

            if (IsProgressState(StateReady))
            {
                lblProgressState.BackColor = AppTheme.Slate;
                btnStopProcess.Text = Language.Pop_StopProcess;
                btnStopProcess.Tag = PopProcessAction.stop;
            }
            else if (IsProgressState(StateProgress))
            {
                lblProgressState.BackColor = AppTheme.Emerald;
                btnStopProcess.Text = Language.Pop_StopProcess;
                btnStopProcess.Tag = PopProcessAction.stop;
            }
            else if (IsProgressState(StateStopped))
            {
                lblProgressState.BackColor = AppTheme.Rose;
                btnStopProcess.Text = Language.Pop_RestartProcess;
                btnStopProcess.Tag = PopProcessAction.resume;
            }
            else if (IsProgressState(StateFinished))
            {
                lblProgressState.BackColor = AppTheme.Amber;
            }
            else
            {
                lblProgressState.BackColor = AppTheme.Violet;
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  공정 상태 전이
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// 공정 상태 전이(시작 · 중지 · 재시작 · 종료)를 서버에 요청하고, 서버가 바꾼 상태를 다시 읽어 그린다.
        /// 전이 가능 여부와 시간 집계는 서버가 판단한다.
        /// </summary>
        /// <returns>요청이 받아들여지고 화면을 다시 읽었으면 true. 취소 · 서버 거절 · 화면 닫힘이면 false.</returns>
        public async Task<bool> SetProcessAction(PopProcessAction action)
        {
            if (_key == null)
            {
                AppMessageBox.Warn(this, Language.Pop_NoTargetProcess, Language.Pop_Title);
                return false;
            }

            PopProcessActionRequest request = null;
            if (action == PopProcessAction.stop)
            {
                request = GetStopReason();
                if (request == null)
                    return false;
            }

            try
            {
                await _apiClient.PopApiClient.ChangeProcessStateAsync(_key, action, request);
            }
            catch (ApiException ex)
            {
                AppLog.Warn($"[POP] 공정 상태 전이 실패 - {action} : {ex.Message}");
                if (IsDisposed || Disposing) return false;

                AppMessageBox.Warn(this, ex.Message, Language.Pop_Title);
                return false;
            }

            if (IsDisposed || Disposing) return false;

            await GetProcessInfo();
            if (IsDisposed || Disposing) return false;

            AppLog.Info($"[POP] 공정 상태 전이 - {action} → {_progressState}");
            return true;
        }

        /// <summary>중지 사유 선택 창. 취소하면 null — 중지 요청을 보내지 않는다.</summary>
        private PopProcessActionRequest GetStopReason()
        {
            using (var form = new StopIssueForm())
            {
                form.Initialize();
                form.CenterOn(this);
                if (form.ShowDialog(this) != DialogResult.OK)
                {
                    AppLog.Info("[POP] 공정 중지 취소 - 사유 선택 창에서 취소");
                    return null;
                }

                // using을 벗어나면 Dispose되므로 블록 안에서 값을 꺼내 둔다.
                var request = new PopProcessActionRequest();
                request.dttmCd = form.StopType.ToString();   // "PD01" ~ "PD09"
                request.remark = form.Remark;

                AppLog.Info($"[POP] 공정 중지 사유 - [{form.SelectedIndex}] {request.dttmCd} : {request.remark}");
                return request;
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  공정 이동 (이전 / 다음 / 공정 목록)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// [이전 공정] / [다음 공정] — 공정 목록에서 지금 공정의 <paramref name="offset"/>칸 옆으로 옮긴다.
        /// 이동은 상태 전이가 아니라 <b>보고 있는 공정을 바꾸는 것</b>이라 조회 키만 바꾸고 화면을 다시 읽는다.
        /// 목록에 없는 공정이거나 목록의 끝이면 옮기지 않는다.
        /// </summary>
        public async Task MoveProcess(int offset)
        {
            if (_key == null)
            {
                AppMessageBox.Warn(this, Language.Pop_NoTargetProcess, Language.Pop_Title);
                return;
            }

            if (!await GetProcesses())
                return;

            var current = GetProcessIndex();
            var target = current + offset;
            if (current < 0 || target < 0 || target >= _processes.Count)
            {
                AppLog.Info($"[POP] 공정 이동 불가 - 현재 {current}, 이동 {offset}, 목록 {_processes.Count}건");
                return;
            }

            await MoveToProcess(_processes[target]);
            if (IsDisposed || Disposing) return;

            await StartEnteredProcess();
        }

        /// <summary>
        /// [공정 목록] — 공정(라우팅) 목록을 조회해 &lt;작업 진입 확인&gt; 팝업을 띄우고,
        /// [이동]으로 고른 공정으로 화면을 다시 읽는다.
        /// </summary>
        public async Task ShowProcessList()
        {
            if (_key == null)
            {
                AppMessageBox.Warn(this, Language.Pop_NoTargetProcess, Language.Pop_Title);
                return;
            }

            if (!await GetProcesses())
                return;

            RoutingProcessDto selected;
            using (var confirm = new WorkEntryConfirmForm())
            {
                confirm.Initialize(ResolveWorkOrder(), _processes);
                confirm.CenterOn(this);
                if (confirm.ShowDialog(this) != DialogResult.OK)
                    return;

                selected = confirm.SelectedProcess;
            }

            if (selected == null)
                return;

            await MoveToProcess(selected);
            if (IsDisposed || Disposing) return;

            AppMessageBox.Info(this, Language.Pop_ProcessMoved, Language.Pop_Title);
            if (IsDisposed || Disposing) return;

            await StartEnteredProcess();
        }

        /// <summary>
        /// 공정 목록을 확보한다(이미 읽어 뒀으면 그대로 쓴다).
        /// 조회에 실패하거나 목록이 비면 사유를 알리고 false를 돌려준다.
        /// </summary>
        private async Task<bool> GetProcesses()
        {
            if (_processes.Count > 0)
                return true;

            btnPrevProcess.Enabled = false;
            btnNextProcess.Enabled = false;
            btnProcessList.Enabled = false;
            try
            {
                string corpCode = _entry?.WorkOrder?.CorpCode;
                string workstationRef = _appSet?.CurrentUser?.WorkstationInfo?.WorkstationRef;
                _processes = await _apiClient.WorkOrderApiClient.GetRoutingProcessesAsync(corpCode, workstationRef, _key.SerialNo)
                             ?? new List<RoutingProcessDto>();
            }
            catch (ApiException ex)
            {
                AppLog.Warn($"[POP] 공정 목록 조회 실패 - {ex.Message}");
                if (IsDisposed || Disposing) return false;

                AppMessageBox.Warn(this, ex.Message, Language.WorkEntryConfirm_Title);
                return false;
            }
            finally
            {
                if (!IsDisposed && !Disposing)
                {
                    btnPrevProcess.Enabled = true;
                    btnNextProcess.Enabled = true;
                    btnProcessList.Enabled = true;
                }
            }

            if (IsDisposed || Disposing) return false;

            if (_processes.Count == 0)
            {
                AppMessageBox.Warn(this, Language.Pop_NoProcessList, Language.WorkEntryConfirm_Title);
                return false;
            }

            return true;
        }

        /// <summary>공정 목록에서 지금 보고 있는 공정의 위치. 없으면 -1.</summary>
        private int GetProcessIndex()
        {
            for (int i = 0; i < _processes.Count; i++)
            {
                if (string.Equals(_processes[i].SubRoutingReference, _key.SubRoutingRef, StringComparison.OrdinalIgnoreCase))
                    return i;
            }

            return -1;
        }

        /// <summary>고른 공정으로 조회 키를 옮기고 화면 전체(공정 정보 + 탭 4개 + 도면)를 다시 읽는다.</summary>
        private async Task MoveToProcess(RoutingProcessDto process)
        {
            _entry.Process = process;
            _key = BuildKey();

            AppLog.Step($"[POP] 공정 이동 - {_key.RoutingOperation}/{_key.PageOperation}");
            await GetProcessInfo();
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  공정 자동 진행
        //
        //  탭 3개(투입자재 / BOM 스팩 / 검사 스팩)의 입력이 모두 끝나면 서버에 [공정 종료]를 보내고,
        //  다음 공정으로 옮겨 [공정 시작]까지 자동으로 이어 간다.
        //   - 세 탭 모두 데이터가 없는 공정도 "끝난 공정"으로 보고 그대로 종료 → 다음 공정으로 넘어간다.
        //   - [공정 시작] 버튼은 없다. 화면 진입 · [이전 공정] · [다음 공정] · [공정 목록]으로 공정에 들어와도
        //     자동으로 시작하고, 중지돼 있던 공정이면 재시작한다(StartEnteredProcess).
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// 공정에 들어온 뒤의 공통 처리(진입 · 이전/다음 · 공정 목록) — 자동 시작 → 남은 시리얼 입력 → 완료 검사.
        /// 완료 검사를 마지막에 두는 이유: 세 탭이 모두 비어 있는 공정은 입력할 것이 없어 저장이 일어나지 않으므로,
        /// 여기서 한 번 보지 않으면 다음 공정으로 넘어갈 계기가 없다.
        /// </summary>
        public async Task StartEnteredProcess()
        {
            await AutoStartProcess();
            if (IsDisposed || Disposing) return;

            await CheckMaterialSerials();
            if (IsDisposed || Disposing) return;

            await TryAutoProgress();
        }

        /// <summary>
        /// 지금 공정을 자동으로 시작한다 — 대기(Ready)면 시작, 중지(Stopped)면 재시작.
        /// 이미 진행 중이거나 완료된 공정에는 요청하지 않는다(전이 가능 여부의 최종 판단은 서버가 한다).
        /// 자동 처리라 확인 창은 띄우지 않고, 결과는 우측 안내 칸에 남긴다.
        /// </summary>
        /// <returns>요청 뒤(또는 원래부터) 공정이 진행 중이면 true.</returns>
        private async Task<bool> AutoStartProcess()
        {
            if (_key == null)
                return false;

            PopProcessAction action;
            if (IsProgressState(StateReady))
                action = PopProcessAction.start;
            else if (IsProgressState(StateStopped))
                action = PopProcessAction.resume;
            else
                return IsProcessRunning;

            // 실패 사유는 SetProcessAction이 창으로 알린다 — 안내 칸에는 실패 사실만 남긴다.
            if (!await SetProcessAction(action))
            {
                ShowMessage(Language.Pop_AutoStartFailed);
                return false;
            }

            var restarted = action == PopProcessAction.resume;
            AppLog.Step($"[POP] 공정 자동 {(restarted ? "재시작" : "시작")} - {_key.RoutingOperation}/{_key.PageOperation} → {_progressState}");
            ShowMessage(restarted ? Language.Pop_AutoRestarted : Language.Pop_AutoStarted);
            return IsProcessRunning;
        }

        private void BeginResultSave()
        {
            _pendingResultSaves++;
        }

        private void EndResultSave()
        {
            if (_pendingResultSaves > 0)
                _pendingResultSaves--;
        }

        /// <summary>
        /// 지금 공정의 탭 3개가 모두 끝났으면 자동으로 완료 → 다음 공정 이동 → 공정 시작을 진행한다.
        /// 실적을 저장한 직후 · 공정을 시작한 직후에 부른다.
        /// 다음 공정도 데이터가 없으면 같은 자리에서 연달아 넘어간다.
        /// </summary>
        public async Task TryAutoProgress()
        {
            if (_autoProgressing || _pendingResultSaves > 0)
                return;

            _autoProgressing = true;
            var moved = false;
            try
            {
                while (!IsDisposed && !Disposing && IsProcessRunning && IsAllTabsCompleted())
                {
                    AppLog.Step($"[POP] 공정 자동 완료 - {_key.RoutingOperation}/{_key.PageOperation}");
                    if (!await SetProcessAction(PopProcessAction.end))
                        return;

                    ShowMessage(Language.Pop_AutoEnded);

                    if (!await MoveNextAndStart())
                        return;

                    moved = true;
                }
            }
            finally
            {
                _autoProgressing = false;
            }

            // 시리얼 입력 팝업은 자동 진행이 끝난 뒤에 띄운다 — 팝업에서 저장한 결과로
            // 다시 이 메서드가 불려야 하는데, 진행 중 플래그가 걸려 있으면 무시되기 때문이다.
            if (moved && !IsDisposed && !Disposing && IsProcessRunning)
                await CheckMaterialSerials();
        }

        /// <summary>탭 3개의 입력이 모두 끝났는지. 세 탭 모두 비어 있으면 할 일이 없는 공정이므로 끝난 것으로 본다.</summary>
        private bool IsAllTabsCompleted()
        {
            var materialsDone = _materials.All(m => m.IsSerialInputCompleted);
            var bomSpecsDone = _bomSpecs.All(IsBomSpecPassed);
            var inspectionsDone = _inspections.All(IsInspectionEntered);

            return materialsDone && bomSpecsDone && inspectionsDone;
        }

        /// <summary>
        /// 공정 목록의 다음 공정으로 옮기고 자동으로 시작(중지 상태면 재시작)한다.
        /// 마지막 공정이었으면 알리고 false. 옮긴 공정이 이미 진행 · 완료 상태면
        /// 시작 요청은 보내지 않고 서버 상태를 그대로 따른다.
        /// </summary>
        private async Task<bool> MoveNextAndStart()
        {
            if (!await GetProcesses())
                return false;

            var current = GetProcessIndex();
            if (current < 0 || current + 1 >= _processes.Count)
            {
                AppMessageBox.Info(this, Language.Pop_LastProcess, Language.Pop_Title);
                return false;
            }

            await MoveToProcess(_processes[current + 1]);
            if (IsDisposed || Disposing) return false;

            return await AutoStartProcess();
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  실적 — 투입자재 시리얼
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>그리드 한 줄의 &lt;투입자재 정보 입력&gt; 팝업을 띄우고, 입력한 시리얼번호를 서버에 보낸다.</summary>
        public async Task InputMaterialSerial(int rowHandle)
        {
            var material = gridViewMaterial.GetRow(rowHandle) as PopInputMaterialDto;
            if (material == null)
                return;

            if (!material.SerialCheckRequired)
            {
                AppMessageBox.Info(this, Language.Pop_SerialNotRequired, Language.PopSerial_Title);
                return;
            }

            List<string> serials;
            using (var form = new MaterialSerialForm())
            {
                form.Initialize(material);
                form.CenterOn(this);
                if (form.ShowDialog(this) != DialogResult.OK)
                    return;

                serials = form.SerialNumbers;
            }

            material.SerialNumbers = serials;
            gridViewMaterial.RefreshData();

            await SetMaterialSerials(material);
        }

        /// <summary>
        /// 시리얼 입력이 남은 자재마다 차례로 입력 팝업을 띄우고, 모두 끝났으면 [BOM 스팩] 탭으로 넘긴다.
        /// 중간에 자동 진행으로 공정이 바뀌면(자재 목록이 새로 바뀌면) 남은 팝업은 띄우지 않는다.
        /// </summary>
        public async Task CheckMaterialSerials()
        {
            var materials = _materials;
            for (int i = 0; i < materials.Count; i++)
            {
                if (materials[i].IsSerialInputCompleted)
                    continue;

                int rowHandle = gridViewMaterial.GetRowHandle(i);   // DataSource 인덱스 → rowHandle
                gridViewMaterial.FocusedRowHandle = rowHandle;
                gridViewMaterial.FocusedColumn = gridViewMaterial.Columns[SerialInputStateField];

                await InputMaterialSerial(rowHandle);
                if (IsDisposed || Disposing || !ReferenceEquals(materials, _materials))
                    return;
            }

            if (materials.Count > 0 && materials.All(m => m.IsSerialInputCompleted))
                tabSpec.SelectedTabPage = pageBom;
        }

        private async Task SetMaterialSerials(PopInputMaterialDto material)
        {
            var request = new PopMaterialSerialSaveRequest();
            request.inputtedItemCd = material.MaterialCode;
            request.inputtedSerialList = material.SerialNumbers;

            BeginResultSave();
            try
            {
                await _apiClient.PopApiClient.SaveMaterialSerialsAsync(request, _key, material.MaterialCode);
            }
            catch (ApiException ex)
            {
                AppLog.Warn($"[POP] 투입자재 시리얼 저장 실패 - {material.MaterialCode} : {ex.Message}");
                if (IsDisposed || Disposing) return;

                AppMessageBox.Warn(this, ex.Message, Language.PopSerial_Title);
                return;
            }
            finally
            {
                EndResultSave();
            }

            if (IsDisposed || Disposing) return;

            AppMessageBox.Success(this, Language.PopSerial_Saved, Language.PopSerial_Title);
            await TryAutoProgress();
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  실적 — BOM 스팩 · 검사 스팩
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// BOM 스팩 한 줄의 토크 · 각도 결과를 서버에 보낸다(설비 자동 입력 · 작업자 직접 입력 공용).
        /// <paramref name="equipmentCode"/>는 결과를 낸 설비코드, 사람이 넣었으면 <see cref="ManualEquipmentCode"/>.
        /// </summary>
        /// <returns>저장됐고 화면이 아직 열려 있으면 true.</returns>
        public async Task<bool> SetBomSpecResult(PopBomSpecDto spec, decimal? torque, decimal? angle, string equipmentCode)
        {
            var result = new PopBomSpecResultDto();
            result.eqpCd = equipmentCode;
            result.torqueRsltVl = torque;
            result.angleRsltVl = angle;

            BeginResultSave();
            try
            {
                await _apiClient.PopApiClient.SaveBomSpecResultsAsync(_key, result, spec);
            }
            catch (ApiException ex)
            {
                AppLog.Warn($"[POP] BOM 결과 저장 실패 - {spec.BomSpecName}/{spec.InputPosition} ({equipmentCode}) : {ex.Message}");
                if (IsDisposed || Disposing) return false;

                AppMessageBox.Warn(this, ex.Message, Language.Pop_Title);
                return false;
            }
            finally
            {
                EndResultSave();
            }

            return !IsDisposed && !Disposing;
        }

        /// <summary>
        /// 검사 스팩 한 줄의 결과를 서버에 보낸다 — 정량 검사는 판정 · 결과값 · 비고, 정성 검사는 판정 · 비고.
        /// 검사 항목 코드는 URL 경로로만 보내고 Body에는 담지 않는다.
        /// 공정이 진행 중이 아니면 보내지 않는다 — 편집 도중 서버 상태가 바뀐 경우를 막는다.
        /// </summary>
        public async Task SetInspectionResult(PopInspectionSpecDto spec, string fieldName)
        {
            if (!IsProcessRunning)
            {
                AppLog.Info($"[POP] 공정 미시작 - 검사 결과 전송 생략 (상태 {_progressState})");
                return;
            }

            var itemCode = spec.InspectionItemCode ?? string.Empty;

            var result = new PopInspectionResult();
            result.JgmtRslt = spec.ResultState ?? string.Empty;

            // 정성 검사는 결과값이 없다 — null로 두면 Body에서 빠진다(PopInspectionResult 참고).
            result.inspRsltVl = spec.IsQuantitative ? spec.ResultValue ?? string.Empty : null;
            result.inspRemark = spec.InspectionResultRemark ?? string.Empty;

            BeginResultSave();
            try
            {
                await _apiClient.PopApiClient.SaveInspectionResultAsync(_key, itemCode, result);
                AppLog.Info($"[POP] 검사 결과 전송 - {itemCode} ({fieldName})");
            }
            catch (ApiException ex)
            {
                AppLog.Warn($"[POP] 검사 결과 전송 실패 - {itemCode}: {ex.Message}");
                if (IsDisposed || Disposing) return;

                AppMessageBox.Warn(this, ex.Message, Language.Pop_TabInspection);
                return;
            }
            finally
            {
                EndResultSave();
            }

            if (IsDisposed || Disposing) return;
            await TryAutoProgress();
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  설비
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// 연결 장비에서 올라온 토크 · 각도 값을 지금 선택된 [BOM 스팩] 줄에 채우고 서버에 보낸다.
        /// 수동 렌치를 켠 동안에는 작업자 입력이 우선이므로 자동 반영하지 않는다.
        /// 공정을 시작하지 않은 상태(대기 · 중지 · 완료)에서 올라온 체결 데이터는 실적이 아니므로
        /// 화면에도 반영하지 않고 서버로도 보내지 않는다(로그만 남기고 버린다).
        /// </summary>
        public async Task SetTorqueFromEquipment(string equipmentId, decimal torqueValue, decimal angleValue)
        {
            if (!IsProcessRunning)
            {
                AppLog.Info($"[POP] 공정 미시작 - 설비 데이터 버림 (설비 {equipmentId}, 상태 {_progressState}, " +
                            $"토크 {torqueValue}, 각도 {angleValue})");
                return;
            }

            if (chkManualWrench.Checked)
                return;

            var target = gridViewBomSpec.GetRow(_bomSpecSelectedRow) as PopBomSpecDto;
            if (target == null)
                return;

            if (target.TorqueResult == null && target.AngleResult == null)
                return;

            if (IsBomSpecPassed(target))
                _bomSpecSelectedRow++;

            AppLog.Info($"[POP] {Language.Pop_TorqueNoPendingRow} (설비 {equipmentId}, 값 {torqueValue}, 값 {angleValue}");

            target.TorqueResult = torqueValue;
            target.AngleResult = angleValue;
            target.IsManualInput = false;
            target.EquipmentCode = equipmentId;
            RefreshBomSpecGrid();

            if (!await SetBomSpecResult(target, torqueValue, angleValue, equipmentId))
                return;

            if (_bomSpecs.Count >= _bomSpecSelectedRow)
                gridViewBomSpec.FocusedRowHandle = _bomSpecSelectedRow;
            else
                _bomSpecSelectedRow = _bomSpecs.Count;

            AppLog.Info($"[POP] 설비 토크 결과값 수신 - {equipmentId} → {target.BomSpecName}/{target.InputPosition} = {torqueValue}");

            await TryAutoProgress();
        }

        /// <summary>
        /// 목표 토크(<paramref name="torqueTarget"/>)를 쓸 너트러너를 하나 골라 토크를 설정하고 잠금을 푼다.
        /// 나머지 연결된 너트러너는 잠가서, 작업자가 맞지 않는 공구로 체결하지 못하게 한다.
        ///
        ///  - 대상: 네트워크(Request 채널)에 연결된 장비만 — 끊긴 장비에는 어떤 값도 전달되지 않는다.
        ///  - 선택: 서버 기준정보의 가동범위(RangeLower ~ RangeUpper, 경계 포함) 안에 목표 토크가 드는 첫 장비
        ///          → Torque = 목표 토크, DriverLock = 0(해제)
        ///  - 그 외(범위 밖 · 범위가 비어 있음 · 두 번째 이후 범위 안 장비) → DriverLock = 3(잠금)
        /// </summary>
        /// <returns>선택된 장비코드. 범위에 맞는 연결 장비가 없으면 null(이때 연결된 장비는 전부 잠긴다).</returns>
        public string WriteTorqueToEquipment(decimal torqueTarget)
        {
            if (_equip?.Manager == null)
                return null;

            string selectedCode = null;

            foreach (var equipment in _equipments)
            {
                var code = equipment.EquipmentCode;

                // 연결 안 된 장비는 잠금도 보낼 수 없으므로 건너뛴다.
                if (!_equip.Manager.TryGet(code, out INutrunner nutrunner) || !nutrunner.IsConnected)
                    continue;

                if (selectedCode == null && IsInTorqueRange(equipment, torqueTarget))
                {
                    // 토크를 먼저 쓰고 잠금을 푼다 — 해제 직후 이전 토크로 체결되는 틈을 없앤다.
                    if (_equip.Manager.SetTorque(code, torqueTarget)
                        && _equip.Manager.SetDriverLock(code, false))
                    {
                        selectedCode = code;
                        AppLog.Info($"[POP] 토크 설정 · 잠금 해제 - {code} : {torqueTarget} " +
                                    $"(범위 {equipment.RangeLower} ~ {equipment.RangeUpper})");
                        continue;
                    }

                    // 쓰기 실패(레지스터 범위 초과 등) — 이 장비는 잠그고 다음 장비를 찾는다.
                    AppLog.Warn($"[POP] 토크 설정 실패 - {code} : {torqueTarget}");
                }

                _equip.Manager.SetDriverLock(code, true);
                AppLog.Info($"[POP] 너트러너 잠금 - {code} (범위 {equipment.RangeLower} ~ {equipment.RangeUpper}, 목표 {torqueTarget})");
            }

            if (selectedCode == null)
                AppLog.Warn($"[POP] 토크 설정 실패 - 목표 {torqueTarget} 를 받을 수 있는 연결된 장비가 없음");

            return selectedCode;
        }

        /// <summary>
        /// 서버가 내려준 장비 가동범위 안에 목표 토크가 드는지(경계 포함).
        /// 범위가 비어 있으면 어떤 토크를 감당하는지 알 수 없으므로 범위 밖으로 본다.
        /// </summary>
        private bool IsInTorqueRange(PopEquipmentDto equipment, decimal torqueTarget)
        {
            if (equipment.RangeLower == null || equipment.RangeUpper == null)
                return false;

            return torqueTarget >= equipment.RangeLower.Value
                && torqueTarget <= equipment.RangeUpper.Value;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  관리자 알람
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// [알림] — &lt;알람 조회 / 등록&gt; 팝업을 띄운다. 알람 조회와 등록은 팝업이 지금 공정 키로 직접 하고,
        /// 등록에 성공해 닫혔을 때만 완료를 알린다. 진행 중인 공정이 없으면 올릴 곳이 없으므로 열지 않는다.
        /// </summary>
        public Task ShowAdminAlarm()
        {
            if (_key == null)
            {
                AppMessageBox.Warn(this, Language.Pop_NoTargetProcess, Language.AdminAlarm_Title);
                return Task.CompletedTask;
            }

            using (var form = new AdminAlarmForm())
            {
                form.Initialize(_apiClient, _key);
                form.CenterOn(this);
                if (form.ShowDialog(this) != DialogResult.OK)
                    return Task.CompletedTask;
            }

            if (!IsDisposed && !Disposing)
                AppMessageBox.Success(this, Language.Pop_AdminAlarmSent, Language.AdminAlarm_Title);

            return Task.CompletedTask;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  도면 (공유 폴더의 도면 파일 → picView)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// 공유 폴더에서 지금 공정의 도면 파일을 읽어 띄운다.
        /// 읽지 못하면(경로 없음 · 권한 · 손상 등) 자리표시자에 사유를 적고 로그를 남긴다 —
        /// 도면 하나 때문에 공정 정보 조회 전체가 실패하면 안 된다.
        /// </summary>
        public void LoadDrawing()
        {
            ClearImage();

            string root = _appSet.AppSet.DocumentShare.RootPath;
            if (string.IsNullOrEmpty(root))
            {
                ShowViewerMessage(Language.Pop_DrawingShareNotConfigured);
                return;
            }

            if (string.IsNullOrEmpty(_info?.DrawingFilePath))
            {
                ShowViewerMessage(Language.Pop_DrawingNone);
                return;
            }

            string path = Path.Combine(root, _info.DrawingFilePath.Replace('/', '\\').TrimStart('\\'));
            try
            {
                byte[] raw = File.ReadAllBytes(path);

                // Base64 텍스트로 저장된 도면은 먼저 디코딩한다.
                if (_imgCtrl.TryDecodeBase64(raw, out byte[] decoded))
                    raw = decoded;

                ShowImage(raw, path);
            }
            catch (Exception ex)
            {
                AppLog.Warn($"[POP] 도면 읽기 실패 - {path} : {ex.Message}");
                ShowViewerMessage(Language.Pop_DrawingLoadFailed);
            }
        }

        private void ShowImage(byte[] data, string path)
        {
            // OLE 헤더 등이 앞에 붙어 있으면 실제 이미지 시작 위치를 찾아 잘라낸다.
            int offset = _imgCtrl.FindImageOffset(data);
            if (offset < 0)
            {
                AppLog.Warn($"[POP] 도면 이미지 데이터를 찾을 수 없음(손상되었거나 이미지가 아닌 파일) - {path}");
                ShowViewerMessage(Language.Pop_DrawingLoadFailed);
                return;
            }

            if (offset > 0)
            {
                byte[] trimmed = new byte[data.Length - offset];
                Buffer.BlockCopy(data, offset, trimmed, 0, trimmed.Length);
                data = trimmed;
            }

            Image img = _imgCtrl.BytesToImage(data);

            // 이전 그림 해제 + 화면 맞춤까지 ImageView가 처리한다(img는 이제 ImageView 소유).
            _viewer.SetImage(img);
        }

        private void ClearImage()
        {
            // Init 전에 불릴 수 있다(도면 읽기 실패 경로) — 그때는 띄운 그림도 없다.
            _viewer?.Clear();
        }

        /// <summary>도면 자리표시자에 안내 문구를 띄운다.</summary>
        private void ShowViewerMessage(string message)
        {
            _viewerMessage = message;
            viewerSurface.Invalidate();
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  아코디언 — 우측 공정 정보 / 하단 탭을 접어 도면을 크게 본다
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// 우측 &lt;진행 중 공정 정보&gt;를 접었다 편다.
        ///
        /// 도면(viewerCard)은 <c>Dock = Fill</c>이라 <b>옆 패널 폭만 줄이면 그만큼 자동으로 넓어진다</b> —
        /// 도면 크기를 직접 계산해 넣지 말 것(창 크기가 바뀌면 어긋난다).
        /// 접어도 손잡이는 남겨야 하므로 폭을 0으로 만들지 않고 <see cref="InfoCollapsedWidth"/>만 남긴다.
        /// </summary>
        private void SetInfoExpanded(bool expanded)
        {
            _infoExpanded = expanded;

            // 여러 컨트롤을 연달아 바꾸면 그때마다 다시 배치되어 화면이 깜빡인다.
            mainPanel.SuspendLayout();
            try
            {
                lblInfoTitle.Visible = expanded;
                fieldPanel.Visible = expanded;

                infoCard.Width = expanded ? InfoExpandedWidth : InfoCollapsedWidth;
                spacerMain.Visible = expanded;
            }
            finally
            {
                mainPanel.ResumeLayout(true);
            }

            // 자리를 옮기지 않은 컨트롤은 레이아웃만으로는 다시 그려지지 않아 잔상이 남는다.
            mainPanel.Invalidate(true);
            RefreshAccordionButtons();
        }

        /// <summary>하단 탭 영역을 접었다 편다. 접어도 탭 줄은 남긴다 — 다시 펴는 손잡이가 그 줄에 있다.</summary>
        private void SetBottomExpanded(bool expanded)
        {
            _bottomExpanded = expanded;

            SuspendLayout();
            try
            {
                // 접었을 때 높이 = 탭 줄 + 카드 안쪽 여백(위/아래).
                bottomCard.Height = expanded
                    ? BottomExpandedHeight
                    : bottomCard.Padding.Vertical + 4 + pnlTabHead.Height;
            }
            finally
            {
                ResumeLayout(true);
            }

            Invalidate(true);
            RefreshAccordionButtons();
        }

        /// <summary>
        /// 손잡이의 방향 · 이름 · 색을 지금 상태에 맞춘다.
        /// 이름을 함께 보여 주는 이유는, 화살표만 있으면 <b>무엇이 접혀 있는지</b> 알 수 없기 때문이다.
        /// </summary>
        private void RefreshAccordionButtons()
        {
            btnInfoAccordion.Edge = AccordionEdge.Left;
            btnInfoAccordion.Accent = AppTheme.Violet;
            btnInfoAccordion.Text = Language.Pop_ProcessInfoTitle;
            btnInfoAccordion.Expanded = _infoExpanded;

            btnTabsAccordion.Edge = AccordionEdge.Bottom;
            btnTabsAccordion.Accent = AppTheme.Cyan;
            btnTabsAccordion.Text = Language.Pop_ToggleTabs;
            btnTabsAccordion.Expanded = _bottomExpanded;
        }
    }
}
