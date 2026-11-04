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
using System.Net;
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

        // 아래 폭 · 높이는 디자이너(배율 1.0) 기준이다 — 넣을 때 _scaler.Scale()로 지금 배율에 맞춘다.

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

        /// <summary>[이전 공정] · [다음 공정](PageUp · PageDown 포함) 이동이 진행 중인지.</summary>
        private bool _processMoving;

        /// <summary>
        /// 지금 공정을 [리웍]으로 다시 연 상태인지. 리웍 중에는 자동 완료 · 자동 이동을 하지 않고
        /// 작업자가 공정을 옮길 때 완료 여부를 판단한다(<see cref="EndCurrentProcess"/>). 다른 공정으로 옮기면 풀린다.
        /// </summary>
        private bool _reworking;

        /// <summary>
        /// 서버에 보내는 중인 실적 저장(시리얼 · 토크 · 검사) 개수.
        /// 화면 값은 저장 응답 전에 이미 채워져 있으므로, 응답을 기다리지 않고 완료를 보내면
        /// 서버에는 마지막 실적이 없는 채로 공정이 끝날 수 있다 — 0일 때만 완료를 보낸다.
        /// </summary>
        private int _pendingResultSaves;

        /// <summary>지금 화면에 띄운 도면의 전체 경로. 같은 도면을 다시 읽어 깜빡이지 않게 비교한다.</summary>
        private string _drawingPath;

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
            SetProcessInfo();

            // 도면은 공유 폴더를 거쳐 늦게 올 수 있으므로 공정 정보를 먼저 채운 뒤 읽는다.
            await LoadDrawing();
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
        /// 공정 상태 전이(시작 · 중지 · 재시작 · 종료)를 서버에 요청하고, 서버가 돌려준 상태 · 시각을 화면에 옮긴다.
        /// 전이 가능 여부와 시간 집계는 서버가 판단한다.
        ///
        /// 화면 전체(공정 정보 조회 → 탭 4개 + 도면)를 다시 읽지 않는 이유: 전이로 바뀌는 것은 상태와 시각뿐인데,
        /// 다시 읽으면 도면이 지워졌다 다시 그려져 공정 이동(이동 → 자동 시작)마다 화면이 깜빡였다.
        /// 응답이 비어 오면 그때만 다시 조회한다.
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

            PopProcessInfoDto changed;
            try
            {
                changed = await _apiClient.PopApiClient.ChangeProcessStateAsync(_key, action, request);
            }
            catch (ApiException ex)
            {
                AppLog.Warn($"[POP] 공정 상태 전이 실패 - {action} : {ex.Message}");
                if (IsDisposed || Disposing) return false;

                AppMessageBox.Warn(this, ex.Message, Language.Pop_Title);
                return false;
            }

            if (IsDisposed || Disposing) return false;

            if (_info != null && !string.IsNullOrEmpty(changed?.ProgressState))
            {
                SetStateChange(changed);
            }
            else
            {
                await GetProcessInfo();
                if (IsDisposed || Disposing) return false;
            }

            AppLog.Info($"[POP] 공정 상태 전이 - {action} → {_progressState}");
            return true;
        }

        /// <summary>
        /// [리웍] — 서버에 rework를 요청해 공정을 진행 중으로 되돌리고 값을 다시 고칠 수 있게 한다(요청 경로 · 조건은 resume과 같다).
        /// 리웍은 자동 완료 · 자동 이동을 하지 않는다. 작업자가 값을 고친 뒤 공정을 옮기면, 그때 모든 항목이 OK면 완료를 보내고
        /// NG나 입력하지 않은 항목이 남아 있으면 완료하지 않고 옮긴다(<see cref="EndCurrentProcess"/>).
        /// 리웍이 가능한 상태인지는 서버가 판단한다(거절되면 사유를 창으로 알린다).
        /// </summary>
        public async Task StartRework()
        {
            if (!await SetProcessAction(PopProcessAction.rework))
                return;

            _reworking = true;
            AppLog.Step($"[POP] 공정 리웍 - {_key.RoutingOperation}/{_key.PageOperation} → {_progressState}");
            ShowMessage(Language.Pop_ReworkStarted);
        }

        /// <summary>상태 전이 응답의 상태 · 시각 · 시간 집계만 지금 공정 정보에 옮겨 우측 카드를 다시 채운다(탭 · 도면은 그대로).</summary>
        private void SetStateChange(PopProcessInfoDto changed)
        {
            _info.ProgressState = changed.ProgressState;
            _info.PageStartTime = changed.PageStartTime;
            _info.PageEndTime = changed.PageEndTime;
            _info.RunTime = changed.RunTime;
            _info.IdleTime = changed.IdleTime;
            _info.WorkTime = changed.WorkTime;

            SetProcessInfo();
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
        /// [이전 공정] / [다음 공정] — 지금 공정을 완료 처리하고, 공정 목록에서 <paramref name="offset"/>칸 옆으로 옮긴다.
        /// 목록에 없는 공정이거나 목록의 끝이면 옮기지 않는다.
        /// 단, 마지막 공정에서 [다음 공정]을 누르면 옮길 곳은 없어도 완료는 보낸다 —
        /// 입력할 데이터가 없는 공정은 자동 완료되지 않으므로, 이 클릭 말고는 끝낼 길이 없다.
        /// </summary>
        public async Task MoveProcess(int offset)
        {
            if (_key == null)
            {
                AppMessageBox.Warn(this, Language.Pop_NoTargetProcess, Language.Pop_Title);
                return;
            }

            // PageUp/PageDown을 누르고 있으면 키가 반복 입력된다 — 이동 하나가 끝나기 전의 입력은 버린다.
            if (_processMoving)
                return;

            _processMoving = true;
            try
            {
                await MoveProcessBy(offset);
            }
            finally
            {
                _processMoving = false;
            }
        }

        private async Task MoveProcessBy(int offset)
        {
            if (!await GetProcesses())
                return;

            var current = GetProcessIndex();
            var target = current + offset;
            if (current < 0 || target < 0 || target >= _processes.Count)
            {
                AppLog.Info($"[POP] 공정 이동 불가 - 현재 {current}, 이동 {offset}, 목록 {_processes.Count}건");

                if (current >= 0 && offset > 0 && await EndCurrentProcess())
                    AppMessageBox.Info(this, Language.Pop_LastProcess, Language.Pop_Title);
                return;
            }

            if (!ConfirmIncompleteMove())
                return;

            if (!await EndCurrentProcess())
                return;

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

            // 지금 공정을 다시 고른 것은 공정을 바꾼 것이 아니므로 완료하지 않는다.
            var isSameProcess = string.Equals(selected.SubRoutingReference, _key.SubRoutingRef, StringComparison.OrdinalIgnoreCase);
            if (!isSameProcess && !ConfirmIncompleteMove())
                return;

            if (!isSameProcess && !await EndCurrentProcess())
                return;

            await MoveToProcess(selected);
            if (IsDisposed || Disposing) return;

            //AppMessageBox.Info(this, Language.Pop_ProcessMoved, Language.Pop_Title);
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

        /// <summary>
        /// 지금 공정이 끝나지 않은 채 다른 공정으로 옮기려 하면 경고하고 계속할지 묻는다.
        /// 남은 작업을 두고 실수로 넘어가는 것을 막으려는 것이라, 막지는 않고 작업자가 고르게 한다.
        /// 완료(Finished)된 공정, 중지(Stopped)된 공정(작업자가 사유를 남기고 일부러 멈춘 것이다),
        /// 이동하면서 <see cref="EndCurrentProcess"/>가 완료를 보내는 공정(진행 중이고 입력할 항목이 없거나 모두 OK인 공정 — 리웍 등)은 묻지 않는다.
        /// </summary>
        /// <returns>이동을 계속하면 true.</returns>
        private bool ConfirmIncompleteMove()
        {
            if (IsProgressState(StateFinished) || IsProgressState(StateStopped))
                return true;

            if (IsProcessRunning && (!HasInputItems() || IsAllTabsCompleted()))
                return true;

            AppLog.Info($"[POP] 미완료 공정 이동 확인 - {_key.RoutingOperation}/{_key.PageOperation} ({_progressState})");
            return AppMessageBox.Show(this, Language.Pop_ProcessIncompleteMoveConfirm, Language.Pop_Title,
                                      AppMessageKind.Warning, AppMessageButtons.OkCancel) == DialogResult.OK;
        }

        /// <summary>
        /// 작업자가 공정을 바꿀 때([이전 공정] · [다음 공정] · [공정 목록]) 지금 공정을 완료 처리한다.
        /// 진행 중(Progress)이고 <b>입력할 항목이 없는</b> 공정에만 보낸다 — 자동 완료되지 않는 공정이 그것뿐이다.
        /// 입력할 항목이 있는 공정은 완료 없이 그대로 옮긴다: 할 일이 남아 있으면 서버가 완료를 400(WORK_005)으로
        /// 거절하고, 그 거절로 이동까지 막히면 작업자가 그 공정을 벗어날 수 없었다. 입력을 다 마치면 자동 완료된다.
        /// 대기 · 중지 · 완료 상태도 그대로 두고 옮긴다.
        ///
        /// 진행 중이고 탭 3개가 <b>모두 OK</b>(<see cref="IsAllTabsCompleted"/>)인 공정도 완료를 보낸다 —
        /// 보통은 자동 완료로 이미 끝나 있지만, [리웍]은 자동 완료를 하지 않으므로 이 이동이 완료할 유일한 계기다.
        /// NG나 입력하지 않은 항목이 남은 리웍 공정은 완료하지 않고 옮긴다.
        ///
        /// 서버가 완료를 <b>업무 사유로 거절(400)</b>해도 이동은 계속한다. "입력할 항목이 없다"는 것은 화면이 받은 탭 기준의
        /// 추정일 뿐이라 서버의 할 일(WORK_005)과 어긋날 수 있는데, 그 거절로 이동까지 막으면 작업자가 공정을 벗어날 수 없었다.
        /// 완료 여부는 서버 판단을 그대로 따르고(공정은 진행 중으로 남는다) 안내 칸에만 남긴다.
        ///
        /// 완료 응답으로 지금 화면을 다시 그리지 않는다 — 곧바로 다른 공정을 읽으므로 그 갱신(응답이 비면 재조회까지)은 낭비다.
        /// </summary>
        /// <returns>이동을 계속해도 되면 true. 통신 실패 · 서버 장애로 완료 결과를 알 수 없거나 화면이 닫혔으면 false.</returns>
        private async Task<bool> EndCurrentProcess()
        {
            if (!IsProcessRunning || (HasInputItems() && !IsAllTabsCompleted()))
            {
                if (_reworking)
                    AppLog.Info($"[POP] 리웍 공정 미완료(NG · 미입력 남음) - 완료하지 않고 이동 ({_key.RoutingOperation}/{_key.PageOperation})");
                return true;
            }

            AppLog.Step($"[POP] 공정 변경으로 완료{(_reworking ? "(리웍)" : string.Empty)} - {_key.RoutingOperation}/{_key.PageOperation}");
            try
            {
                await _apiClient.PopApiClient.ChangeProcessStateAsync(_key, PopProcessAction.end);
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                AppLog.Warn($"[POP] 공정 완료 거절 - 완료하지 않고 이동 ({_key.RoutingOperation}/{_key.PageOperation}) : {ex.Message}");
                if (IsDisposed || Disposing) return false;

                ShowMessage(Language.Pop_ProcessMovedUnfinished);
                return true;
            }
            catch (ApiException ex)
            {
                AppLog.Warn($"[POP] 공정 상태 전이 실패 - {PopProcessAction.end} : {ex.Message}");
                if (IsDisposed || Disposing) return false;

                AppMessageBox.Warn(this, ex.Message, Language.Pop_Title);
                return false;
            }

            if (IsDisposed || Disposing) return false;

            ShowMessage(Language.Pop_ProcessEnded);
            return true;
        }

        /// <summary>고른 공정으로 조회 키를 옮기고 화면 전체(공정 정보 + 탭 4개 + 도면)를 다시 읽는다.</summary>
        private async Task MoveToProcess(RoutingProcessDto process)
        {
            _entry.Process = process;
            _key = BuildKey();
            _reworking = false;   // 리웍은 그 공정에 머무는 동안만이다

            AppLog.Step($"[POP] 공정 이동 - {_key.RoutingOperation}/{_key.PageOperation}");
            await GetProcessInfo();
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  공정 자동 진행
        //
        //  탭 3개(투입자재 / BOM 스팩 / 검사 스팩)의 입력이 모두 끝나면 서버에 [공정 종료]를 보내고,
        //  다음 공정으로 옮겨 [공정 시작]까지 자동으로 이어 간다.
        //   - 입력할 항목이 없는 공정(HasInputItems)은 자동으로 끝내지 않는다. 시작 상태로 두고, 작업자가
        //     [이전 공정] · [다음 공정] · [공정 목록]으로 공정을 바꿀 때 완료를 보낸다(EndCurrentProcess).
        //   - [공정 시작] 버튼은 없다. 화면 진입 · [이전 공정] · [다음 공정] · [공정 목록]으로 공정에 들어와도
        //     자동으로 시작하고, 중지돼 있던 공정이면 재시작한다(StartEnteredProcess).
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// 공정에 들어온 뒤의 공통 처리(진입 · 이전/다음 · 공정 목록) — 자동 시작 → 남은 시리얼 입력 → 완료 검사.
        /// 완료 검사를 마지막에 두는 이유: 이미 입력이 다 끝나 있는 공정에 다시 들어온 경우엔 저장이 일어나지 않으므로,
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
        /// 다음 공정도 입력이 이미 다 끝나 있으면 같은 자리에서 연달아 넘어가고, 입력할 데이터가 없는 공정에서는 멈춘다.
        /// </summary>
        public async Task TryAutoProgress()
        {
            // 리웍은 자동 완료 · 이동을 하지 않는다 — 작업자가 공정을 옮길 때 EndCurrentProcess가 완료를 판단한다.
            if (_autoProgressing || _pendingResultSaves > 0 || _reworking)
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

        /// <summary>
        /// 작업자가 입력할 항목이 있는 공정인지 — 시리얼 또는 품목번호 확인이 필요한 자재 · BOM 스팩 · 검사 항목 중 하나라도 있으면 true.
        /// 탭의 행 수로 보면 안 된다: 확인이 필요 없는 자재(serialVrfcYn=N · itemCdVrfcYn=N)는 행이 있어도 입력할 것이 없고
        /// 처음부터 "입력 완료"로 셈해져, 그런 자재만 있는 공정이 들어가자마자 자동 완료됐다.
        /// 품목번호 확인(itemCdVrfcYn=Y)도 서버는 할 일로 센다 — 빼고 세면 "입력할 것 없음"으로 보고 이동 때 완료를 보내
        /// WORK_005로 거절됐다(SUB20/0019).
        /// </summary>
        private bool HasInputItems()
        {
            return _materials.Any(m => m.HasInputItem) || _bomSpecs.Count > 0 || _inspections.Count > 0;
        }

        /// <summary>
        /// 탭 3개의 입력이 모두 끝났는지. 입력할 항목이 하나도 없는 공정(<see cref="HasInputItems"/>)은 false —
        /// 그런 공정은 자동 완료하지 않고 작업자가 공정을 바꿀 때 완료한다.
        /// </summary>
        private bool IsAllTabsCompleted()
        {
            if (!HasInputItems())
                return false;

            var materialsDone = _materials.All(m => m.IsInputCompleted);
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

        /// <summary>
        /// 그리드 한 줄의 &lt;투입자재 정보 입력&gt; 팝업을 띄우고, 입력한 값을 서버에 보낸다.
        /// 시리얼 확인 자재는 시리얼번호를, 품목코드 확인(itemCdVrfcYn=Y) 자재는 품목코드를 받는다(둘 다면 둘 다).
        /// </summary>
        public async Task InputMaterialSerial(int rowHandle)
        {
            var material = gridViewMaterial.GetRow(rowHandle) as PopInputMaterialDto;
            if (material == null)
                return;

            if (!material.HasInputItem)
            {
                AppMessageBox.Info(this, Language.Pop_SerialNotRequired, Language.PopSerial_Title);
                return;
            }

            // 중지된 공정은 팝업에서 다 입력한 뒤에 거절되지 않도록 열기 전에 막는다(BOM · 검사 입력과 같은 조건).
            // 그 밖의 상태에서 서버가 거절하면(409) SetMaterialInput이 사유를 알리고 서버 상태로 다시 그린다.
            if (!IsInputAllowed)
            {
                AppLog.Info($"[POP] 공정 중지 - 시리얼 입력 생략 ({material.MaterialCode}, 상태 {_progressState})");
                AppMessageBox.Info(this, Language.Pop_SerialNotRunning, Language.PopSerial_Title);
                return;
            }

            List<string> serials;
            string itemCode;
            using (var form = new MaterialSerialForm())
            {
                form.Initialize(material);
                form.CenterOn(this);
                if (form.ShowDialog(this) != DialogResult.OK)
                    return;

                serials = form.SerialNumbers;
                itemCode = form.InputtedItemCode;
            }

            // 품목코드는 화면에 따로 담아 두지 않으므로 실패해도 되돌릴 값이 없다(완료 표시는 저장 성공 때만 바뀐다).
            if (material.IsItemCodeInputOnly)
            {
                await SetMaterialItemCode(material, itemCode);
                return;
            }

            // 저장이 실패하면 입력 전 값으로 되돌린다 — 그대로 두면 서버에는 없는 시리얼로 "완료"가 표시되고,
            // 그 화면 값을 믿은 자동 완료가 서버에서 WORK_005(할 일 남음)로 거절됐다.
            var previous = material.SerialNumbers;
            material.SerialNumbers = serials;
            gridViewMaterial.RefreshData();

            if (await SetMaterialSerials(material, itemCode))
                return;

            if (IsDisposed || Disposing) return;

            material.SerialNumbers = previous;
            gridViewMaterial.RefreshData();
        }

        /// <summary>
        /// 시리얼 입력이 남은 자재마다 차례로 입력 팝업을 띄우고, 모두 끝났으면 [BOM 스팩] 탭으로 넘긴다.
        /// 중간에 자동 진행으로 공정이 바뀌면(자재 목록이 새로 바뀌면) 남은 팝업은 띄우지 않는다.
        /// 공정에 들어올 때마다(진입 · 이동 · 자동 진행) 여기를 지나므로, BOM 대상 행 선택과 목표 토크도 여기서 한다.
        ///
        /// 진행 중이 아닌 공정(완료 · 대기 · 중지, 자동 시작 실패 포함)에서는 팝업을 띄우지 않는다 — 저장해도 서버가 409로 거절하고,
        /// 완료된 공정에 남은 미입력 자재가 있으면 들어올 때마다 팝업과 거절이 되풀이됐다. 행 선택은 그대로 한다.
        /// </summary>
        public async Task CheckMaterialSerials()
        {
            if (IsProcessRunning)
            {
                var materials = _materials;
                for (int i = 0; i < materials.Count; i++)
                {
                    if (IsMaterialPopupDone(materials[i]))
                        continue;

                    int rowHandle = gridViewMaterial.GetRowHandle(i);   // DataSource 인덱스 → rowHandle
                    gridViewMaterial.FocusedRowHandle = rowHandle;
                    gridViewMaterial.FocusedColumn = gridViewMaterial.Columns[SerialInputStateField];

                    await InputMaterialSerial(rowHandle);
                    if (IsDisposed || Disposing || !ReferenceEquals(materials, _materials))
                        return;
                }
            }
            else if (!_materials.All(IsMaterialPopupDone))
            {
                AppLog.Info($"[POP] 공정 미진행 - 투입자재 입력 팝업 생략 ({_key?.RoutingOperation}/{_key?.PageOperation}, 상태 {_progressState})");
            }

            if (!ShowBomSpecTab())
                SelectBomSpecRow();
        }

        /// <summary>
        /// 입력 팝업으로 받을 것이 끝난 자재인지 — 품목코드만 받는 자재는 품목코드, 그 밖에는 시리얼 기준.
        /// 시리얼 자재의 품목코드 완료는 시리얼 저장에 함께 실리므로 여기서 따로 보지 않는다
        /// (서버 값이 늦게 오는 경우 시리얼을 다 넣은 자재에 팝업이 되풀이되지 않게).
        /// </summary>
        private static bool IsMaterialPopupDone(PopInputMaterialDto material)
        {
            return material.IsItemCodeInputOnly ? material.ItemCodeInputCompleted : material.IsSerialInputCompleted;
        }

        /// <summary>
        /// 투입자재 입력(시리얼 · 품목코드)이 모두 끝났으면 [BOM 스팩] 탭으로 넘기고 체결할 행 · 목표 토크를 맞춘다.
        /// 투입자재가 없으면 탭을 옮기지 않는다(작업자가 보던 탭을 그대로 둔다).
        /// </summary>
        /// <returns>탭을 옮겼으면 true.</returns>
        public bool ShowBomSpecTab()
        {
            if (_materials.Count == 0 || !_materials.All(IsMaterialPopupDone))
                return false;

            tabSpec.SelectedTabPage = pageBom;
            SelectBomSpecRow();
            return true;
        }

        /// <summary>
        /// 시리얼 확인 자재의 시리얼번호를 서버에 보낸다. 품목코드 확인 자재면 팝업에서 입력한 품목코드를 함께 싣고,
        /// 아니면 전처럼 투입자재 코드를 그대로 싣는다.
        /// </summary>
        /// <returns>서버에 저장됐으면 true. 실패(사유는 창으로 알린다) · 화면 닫힘이면 false.</returns>
        private async Task<bool> SetMaterialSerials(PopInputMaterialDto material, string itemCode)
        {
            // 공란이 섞인 목록은 보내지 않는다. 입력 팝업이 [저장]에서 이미 막지만, 서버로 나가는 곳은 여기 하나뿐이라
            // 팝업을 거치지 않는 경로가 생겨도 빈 시리얼이 실적으로 남지 않도록 마지막으로 한 번 더 막는다.
            var serials = material.SerialNumbers ?? new List<string>();
            if (serials.Count < Math.Max(material.InputQty, 1) || serials.Any(string.IsNullOrWhiteSpace))
            {
                AppLog.Info($"[POP] 시리얼 공란 - 저장 안 함 ({material.MaterialCode}, 입력 {serials.Count(s => !string.IsNullOrWhiteSpace(s))}/{material.InputQty})");
                if (IsDisposed || Disposing) return false;

                AppMessageBox.Warn(this, Language.PopSerial_Incomplete, Language.PopSerial_Title);
                return false;
            }

            var request = new PopMaterialSerialSaveRequest();
            request.inputtedItemCd = string.IsNullOrWhiteSpace(itemCode) ? material.MaterialCode : itemCode.Trim();
            request.inputtedSerialList = material.SerialNumbers;

            return await SetMaterialInput(material, request);
        }

        /// <summary>
        /// 품목코드만 확인하는 자재(시리얼 확인 N · 품목코드 확인 Y)의 품목코드를 서버에 보낸다.
        /// 시리얼 저장과 같은 API에 시리얼 목록을 비워 보낸다. 투입자재 코드와 같은지는 팝업이 먼저 확인하고,
        /// 최종 판정은 서버가 한다.
        /// </summary>
        /// <returns>서버에 저장됐으면 true. 실패(사유는 창으로 알린다) · 화면 닫힘이면 false.</returns>
        private async Task<bool> SetMaterialItemCode(PopInputMaterialDto material, string itemCode)
        {
            if (string.IsNullOrWhiteSpace(itemCode))
            {
                AppLog.Info($"[POP] 품목코드 공란 - 저장 안 함 ({material.MaterialCode})");
                if (IsDisposed || Disposing) return false;

                AppMessageBox.Warn(this, Language.PopSerial_ItemCodeIncomplete, Language.PopSerial_Title);
                return false;
            }

            var request = new PopMaterialSerialSaveRequest();
            request.inputtedItemCd = itemCode.Trim();

            return await SetMaterialInput(material, request);
        }

        /// <summary>투입자재 입력(시리얼 · 품목코드) 저장 요청을 보내고, 성공하면 품목코드 완료로 두고 자동 진행을 시도한다.</summary>
        /// <returns>서버에 저장됐으면 true. 실패(사유는 창으로 알린다) · 화면 닫힘이면 false.</returns>
        private async Task<bool> SetMaterialInput(PopInputMaterialDto material, PopMaterialSerialSaveRequest request)
        {
            var saveFailed = false;
            var isConflict = false;

            BeginResultSave();
            try
            {
                await _apiClient.PopApiClient.SaveMaterialSerialsAsync(request, _key, material.MaterialCode);
            }
            catch (ApiException ex)
            {
                AppLog.Warn($"[POP] 투입자재 입력 저장 실패 -{material.MaterialCode} : {ex.Message}");
                if (IsDisposed || Disposing) return false;

                AppMessageBox.Warn(this, ex.Message, Language.PopSerial_Title);
                saveFailed = true;
                isConflict = ex.StatusCode == HttpStatusCode.Conflict;
            }
            finally
            {
                EndResultSave();
            }

            // 409는 서버가 지금 상태로는 받을 수 없다는 뜻이다 — 공정이 진행 중이 아니거나(WORK_001/002),
            // 요청 안에 중복 시리얼 · 품목번호가 있는 경우. 대부분 화면이 아는 공정 상태가 서버와 달라서이므로
            // 서버 상태로 다시 그려 작업자가 같은 입력을 되풀이하지 않게 한다(다시 읽으면 자재 목록도 새로 바뀐다).
            if (isConflict)
            {
                await GetProcessInfo();
                return false;
            }

            if (saveFailed) return false;

            if (IsDisposed || Disposing) return false;

            // 시리얼 저장 · 품목코드 저장 모두 품목코드(inputtedItemCd)를 보낸다 — 서버가 받았으므로 다시 조회하지 않고 완료로 옮겨 둔다.
            material.ItemCodeInputCompleted = true;

            AppMessageBox.Success(this, Language.PopSerial_Saved, Language.PopSerial_Title);
            await TryAutoProgress();
            return true;
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
        /// 공정이 중지 상태면 보내지 않는다 — 편집 도중 중지된 경우를 막는다.
        /// </summary>
        public async Task SetInspectionResult(PopInspectionSpecDto spec, string fieldName)
        {
            if (!IsInputAllowed)
            {
                AppLog.Info($"[POP] 공정 중지 - 검사 결과 전송 생략 (상태 {_progressState})");
                return;
            }

            var itemCode = spec.InspectionItemCode ?? string.Empty;

            var result = new PopInspectionResult();
            result.jgmtRslt = spec.ResultState ?? string.Empty;

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
            {
                AppLog.Info($"[POP] {Language.Pop_TorqueNoPendingRow} (설비 {equipmentId}, 토크 {torqueValue}, 각도 {angleValue})");
                return;
            }

            var bomSpecs = _bomSpecs;
            target.TorqueResult = torqueValue;
            target.AngleResult = angleValue;
            target.IsManualInput = false;
            target.EquipmentCode = equipmentId;
            RefreshBomSpecGrid();

            AppLog.Info($"[POP] 설비 토크 결과값 수신 - {equipmentId} → {target.BomSpecName}/{target.InputPosition} = {torqueValue}");

            if (!await SetBomSpecResult(target, torqueValue, angleValue, equipmentId))
                return;

            // 저장을 기다리는 사이 공정이 바뀌었으면 새 공정의 행은 SetGridData가 이미 골랐다.
            if (!ReferenceEquals(bomSpecs, _bomSpecs))
                return;

            // 합격이면 이 결과로 바로 다음 행에 넘어가 목표 토크까지 써 둔다. 다음 체결이 들어올 때 넘기면
            // 그 체결 값이 이미 합격한 행을 덮어써, 다음 행으로 가려면 한 번 더 조여야 했다.
            // 다음 행은 결과 유무와 상관없이 그리드 순서상 바로 아래 행이다 — 작업자가 위에서부터 차례로 조이므로.
            // NG면 같은 행을 다시 조이도록 그대로 둔다. 마지막 행이면 자리를 지킨다(자동 진행이 공정을 넘긴다).
            if (IsBomSpecPassed(target) && _bomSpecSelectedRow + 1 < gridViewBomSpec.RowCount)
            {
                FocusBomSpecRow(_bomSpecSelectedRow + 1);
                WriteBomSpecTorque();
            }

            await TryAutoProgress();
        }

        /// <summary>
        /// 지금 고른 [BOM 스팩] 행의 목표 토크를 너트러너에 쓴다(공정 이동 · 행 자동 선택 · 장비 연결 시).
        /// 공정 상태(대기 · 진행 · 중지 · 완료)와 상관없이 쓴다 — 장비는 작업자가 조이기 전에 이미 맞춰져 있어야 하고,
        /// 상태를 따지면 자동 시작이 늦거나 실패한 공정에서 장비가 앞 공정의 토크로 남았다(체결 실적 반영은 따로 진행 중일 때만).
        /// 연결된 너트러너가 없으면 쓰지 않는다 — 자동으로 부르는 곳이라 창도 띄우지 않는다.
        /// 작업자가 행을 직접 고를 때는 <see cref="GridViewBomSpec_FocusedRowChanged"/>가 써 주고 실패도 알린다.
        /// </summary>
        public void WriteBomSpecTorque()
        {
            if (!HasConnectedNutrunner())
                return;

            var spec = gridViewBomSpec.GetRow(_bomSpecSelectedRow) as PopBomSpecDto;
            if (spec?.TorqueTarget == null)
                return;

            // 목표 토크는 서버 기준정보 값을 그대로 쓴다(클라이언트가 계산하지 않는다).
            WriteTorqueToEquipment(spec.TorqueTarget.Value);
        }

        /// <summary>이 공정의 너트러너 중 작업에 쓸 수 있는(<see cref="INutrunner.IsReady"/>) 것이 하나라도 있는지.</summary>
        private bool HasConnectedNutrunner()
        {
            if (_equip?.Manager == null)
                return false;

            foreach (var equipment in _equipments)
            {
                if (_equip.Manager.TryGet(equipment.EquipmentCode, out INutrunner nutrunner) && nutrunner.IsReady)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 목표 토크(<paramref name="torqueTarget"/>)를 받을 수 있는 너트러너 <b>전부</b>에 토크를 설정하고 잠금을 푼다.
        /// 범위 밖의 연결된 너트러너는 잠가서, 작업자가 맞지 않는 공구로 체결하지 못하게 한다.
        ///
        ///  - 대상: 작업에 쓸 수 있는(<see cref="INutrunner.IsReady"/>) 장비만 — 그리드의 "사용 가능"과 같은 기준.
        ///          연결됐지만 이벤트 수신 확인 전인 장비는 확인되는 순간(HandleReadyChanged) 써 준다.
        ///  - 활성: 서버 기준정보의 가동범위(RangeLower ~ RangeUpper, 경계 포함) 안에 목표 토크가 드는 모든 장비
        ///          → Torque = 목표 토크, PresetChange = 1, DriverLock = 0(해제)
        ///  - 그 외(범위 밖 · 범위가 비어 있음 · 토크 쓰기 실패) → DriverLock = 3(잠금)
        /// </summary>
        /// <returns>활성화한 장비코드 목록. 범위에 맞는 연결 장비가 없으면 빈 목록(이때 연결된 장비는 전부 잠긴다).</returns>
        public IReadOnlyList<string> WriteTorqueToEquipment(decimal torqueTarget)
        {
            var activated = new List<string>();
            if (_equip?.Manager == null)
                return activated;

            foreach (var equipment in _equipments)
            {
                var code = equipment.EquipmentCode;

                // 아직 쓸 수 없는 장비는 건너뛴다 — 핸드셰이크(EventDataEnable)보다 먼저 줄을 서지 않게.
                if (!_equip.Manager.TryGet(code, out INutrunner nutrunner) || !nutrunner.IsReady)
                    continue;

                if (IsInTorqueRange(equipment, torqueTarget))
                {
                    // 토크를 먼저 쓰고 잠금을 푼다 — 해제 직후 이전 토크로 체결되는 틈을 없앤다.
                    if (_equip.Manager.SetTorque(code, torqueTarget)
                        && _equip.Manager.SetDriverLock(code, false))
                    {
                        activated.Add(code);
                        AppLog.Info($"[POP] 토크 설정 · 잠금 해제 - {code} : {torqueTarget} " +
                                    $"(범위 {equipment.RangeLower} ~ {equipment.RangeUpper})");
                        continue;
                    }

                    // 쓰기 실패(레지스터 범위 초과 등) — 이 장비는 잠근다.
                    AppLog.Warn($"[POP] 토크 설정 실패 - {code} : {torqueTarget}");
                }

                _equip.Manager.SetDriverLock(code, true);
                AppLog.Info($"[POP] 너트러너 잠금 - {code} (범위 {equipment.RangeLower} ~ {equipment.RangeUpper}, 목표 {torqueTarget})");
            }

            if (activated.Count == 0)
                AppLog.Warn($"[POP] 토크 설정 실패 - 목표 {torqueTarget} 를 받을 수 있는 연결된 장비가 없음");

            return activated;
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
        ///
        /// 파일은 반드시 DocShare(DocumentShareClient)로 읽는다 — App.config의 공유 계정(UserName/Password)을
        /// 세션에 붙이는 일(WNetAddConnection2)이 그 안에서 이루어진다. File.ReadAllBytes로 바로 읽으면
        /// 현재 윈도우 계정으로 접근하게 되어 "사용자 이름 또는 암호가 올바르지 않습니다"로 실패한다.
        /// </summary>
        public async Task LoadDrawing()
        {
            string root = _appSet.AppSet.DocumentShare.RootPath;
            if (string.IsNullOrEmpty(root))
            {
                ClearImage();
                ShowViewerMessage(Language.Pop_DrawingShareNotConfigured);
                return;
            }

            if (string.IsNullOrEmpty(_info?.DrawingFilePath))
            {
                ClearImage();
                ShowViewerMessage(Language.Pop_DrawingNone);
                return;
            }

            // 서버 경로는 "/GUM/..."처럼 '/'로 시작한다. DocShare.Resolve에 그대로 넘기면 "\GUM\..."이
            // 루트 경로로 판정되어 공유 루트와 합쳐지지 않으므로, 여기서 전체 UNC 경로를 만들어 넘긴다.
            string path = Path.Combine(root, _info.DrawingFilePath.Replace('/', '\\').TrimStart('\\'));

            // 같은 도면이 이미 떠 있으면 다시 읽지 않는다 — 지웠다 다시 그리면 깜빡이고, 보던 배율 · 위치도 풀린다.
            if (_viewer != null && _viewer.HasImage && string.Equals(path, _drawingPath, StringComparison.OrdinalIgnoreCase))
                return;

            ClearImage();
            try
            {
                byte[] raw = await _appSet.DocShare.ReadAllBytesAsync(path);
                if (IsDisposed || Disposing) return;

                // Base64 텍스트로 저장된 도면은 먼저 디코딩한다.
                if (_imgCtrl.TryDecodeBase64(raw, out byte[] decoded))
                    raw = decoded;

                ShowImage(raw, path);
            }
            catch (Exception ex)
            {
                AppLog.Warn($"[POP] 도면 읽기 실패 - {path} : {ex.Message}");
                if (IsDisposed || Disposing) return;
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
            _drawingPath = path;
        }

        private void ClearImage()
        {
            // Init 전에 불릴 수 있다(도면 읽기 실패 경로) — 그때는 띄운 그림도 없다.
            _viewer?.Clear();
            _drawingPath = null;
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

                infoCard.Width = _scaler.Scale(expanded ? InfoExpandedWidth : InfoCollapsedWidth);
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
                    ? _scaler.Scale(BottomExpandedHeight)
                    : bottomCard.Padding.Vertical + _scaler.Scale(4) + pnlTabHead.Height;
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
