using DevExpress.XtraEditors;
using SEOJIN_TMES.Client.ApiClient;
using SEOJIN_TMES.Client.Common.Errors;
using SEOJIN_TMES.Client.Common.Imaging;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Core;
using SEOJIN_TMES.Client.Equipment.Core;
using SEOJIN_TMES.Client.Equipment.Nutrunner;
using SEOJIN_TMES.Client.Forms.Common;
using SEOJIN_TMES.Client.Forms.WorkOrderEntry;
using SEOJIN_TMES.Client.Theme;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SEOJIN_TMES.Client.Forms.Pop
{
    /// <summary>
    /// TMES &gt; 작업관리 &gt; POP 본 화면 — 진입 화면에서 고른 페이지 공정 하나를 실제로 작업하는 화면.
    ///
    /// 구성:
    ///  - 상단  : 공정 제어 버튼(이전/다음/중지/공정 목록/관리자 알람) + 작업자
    ///            · 중지      : 서버에 상태 전이를 요청하고, 돌아온 진행 상태를 우측 카드와 상단 띠에 반영한다.
    ///            · 이전/다음 : [공정 목록]과 같은 순서로 옆 공정으로 옮긴다(키보드 PageUp / PageDown도 같다).
    ///            · [공정 시작] 버튼은 없다 — 공정에 들어오면(진입 · 이전/다음 · 공정 목록) 자동으로 시작하고,
    ///              중지돼 있던 공정이면 자동으로 다시 시작한다. 종료는 탭 값이 모두 입력되면 자동으로 보낸다.
    ///  - 좌측  : 작업 표준/도면(공유 폴더의 도면 이미지)
    ///  - 우측  : 진행 중 공정 정보 + 안내 문구(lblMessage). 처음에는 접힌 채로 보여 준다.
    ///  - 하단  : 탭 4개 — 투입자재 / BOM 스팩 / 검사 스팩 / 연결 장비
    ///
    /// 파일 구성:
    ///  - PopScreen.cs        : 초기화 · 이벤트 핸들러만 둔다. 처리는 <see cref="IPopScreen"/>(<see cref="Screen"/>)로 부른다.
    ///  - PopScreen.Grid.cs   : 탭 4개 그리드의 구성 · 바인딩 · 그리드 이벤트
    ///  - PopScreen.Method.cs : 공정 상태 전이 · 공정 이동 · 자동 진행 · 실적 전송 · 설비 · 도면
    ///
    /// 업무 판정(합·불, 상태 전이 가능 여부 등)은 전부 서버가 하고, 이 화면은 표시와 입력 전달만 한다.
    /// </summary>
    public partial class PopScreen : XtraUserControl, IThemedScreen, IPopScreen
    {
        private Setting _appSet;
        private Devices _equip;
        private ApiClientFactory _apiClient;
        private WorkEntryContext _entry;
        private PopProcessKey _key;
        private PopProcessInfoDto _info;

        /// <summary>
        /// 연결 실패 창을 이미 띄운 장비. "사용 가능"이 될 때까지 다시 띄우지 않는다.
        /// 시퀀스는 TCP가 붙을 때마다 실패 알람 상태를 초기화하는데, 장비가 접속만 받고 응답하지 않으면
        /// 붙음 → 응답 없음 → 재연결이 반복되어 같은 창이 오전 내내 47번 떴다(2026-11-02 현장 로그).
        /// </summary>
        private readonly HashSet<string> _connectFailedShown = new HashSet<string>();

        /// <summary>
        /// 도면 이미지 뷰어(확대/축소 · 화면 맞춤 · 드래그 이동). 배율 계산과 그림의 소유 · 해제는 이쪽이 맡는다.
        /// <see cref="Init"/>에서 만든다.
        /// </summary>
        private ImageView _viewer;

        /// <summary>도면 자리표시자에 그릴 안내 문구(도면 없음 / 실패 사유).</summary>
        private string _viewerMessage;

        /// <summary>
        /// 서버가 마지막으로 내려준 공정 진행 상태(Ready / Progress / Stopped / Finished).
        /// 장비 데이터를 받을지 버릴지 판단할 때 쓴다 — 상태 판정은 서버 몫이고 여기서는 받은 값을 기억만 한다.
        /// </summary>
        private string _progressState;

        /// <summary>서버가 이 공정에서 [수동 렌치]를 허용했는지.</summary>
        private bool _bomManualEnabled;

        /// <summary>
        /// 기준 해상도(1920×1080) 대비 비율로 화면 전체를 확대 · 축소한다. 생성자에서 디자이너 값을 기준으로 잡고,
        /// Load에서 contentHost에 붙인다(<see cref="AttachScaling"/>).
        /// </summary>
        private readonly ScreenScaler _scaler;

        /// <summary>다국어 문구가 칸을 넘으면 글자를 줄인다(배율 적용 글꼴 기준).</summary>
        private readonly TextFitter _textFitter;

        /// <summary>이벤트 핸들러가 처리를 부르는 창구 — 구현이 Grid / Method 중 어디에 있는지 몰라도 되게 한다.</summary>
        private IPopScreen Screen => this;

        public PopProcessKey Key => _key;

        public bool IsProcessRunning => IsProgressState(StateProgress);

        /// <summary>
        /// 값을 입력할 수 있는 상태인지 — 완료(Finished) · 중지(Stopped)된 공정은 막는다.
        /// 완료된 공정을 고치려면 [리웍]으로 진행 중(Progress)으로 되돌린다 — 서버가 돌려준 상태가 진행 중이면 다시 열린다.
        /// </summary>
        public bool IsInputAllowed => !IsProgressState(StateStopped) && !IsProgressState(StateFinished);

        public PopScreen(WorkEntryContext entry = null)
        {
            InitializeComponent();
            _entry = entry;

            // 디자이너 값 그대로일 때 기준을 잡아야 한다 — contentHost에 붙어 늘어난 뒤에는 앵커 위치가 이미 움직여 있다.
            _scaler = new ScreenScaler(this);
            _scaler.Rescaled += Scaler_Rescaled;
            _textFitter = new TextFitter(_scaler);
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  초기화
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>전역 설정 · 설비 · API 클라이언트를 받고, 진입 대상으로 조회 키를 만든다.</summary>
        private void Init()
        {
            _appSet = Global.Instance.Settings;
            _equip = Global.Instance.Devices;
            _apiClient = Global.Instance.Settings.ApiClients;
            _key = BuildKey();

            _processes = new List<RoutingProcessDto>();
            _viewerMessage = Language.Pop_DrawingNone;

            if (_viewer == null)
            {
                _viewer = new ImageView(picView);
                btnZoomIn.Click += BtnZoomIn_Click;
                btnZoomOut.Click += BtnZoomOut_Click;
                btnFit.Click += BtnFit_Click;

                // ImageBox는 기본값(AllowDoubleClick=false)이면 DoubleClick 이벤트 자체를 올리지 않는다.
                picView.AllowDoubleClick = true;
                picView.DoubleClick += PicView_DoubleClick;
            }
        }

        /// <summary>화면의 모든 고정 문구를 현재 언어로 채운다(언어 전환 시 화면을 다시 열면 반영된다).</summary>
        private void ApplyLanguage()
        {
            btnPrevProcess.Text = Language.Pop_PrevProcess;
            btnNextProcess.Text = Language.Pop_NextProcess;
            btnStopProcess.Text = Language.Pop_StopProcess;
            btnProcessList.Text = Language.Pop_ProcessList;
            btnAdminAlarm.Text = Language.Pop_AdminAlarm;
            btnRework.Text = Language.Pop_Rework;

            lblWorkerTile.Text = Language.Pop_Worker;
            lblWorker.Text = ResolveWorkerName();

            lblInfoTitle.Text = Language.Pop_ProcessInfoTitle;
            lblWorkOrderNo.Text = "· " + Language.Pop_WorkOrderNo;
            lblItemCode.Text = "· " + Language.Pop_ItemCode;
            lblProductSerialNo.Text = "· " + Language.Pop_ProductSerialNo;
            lblRoutingGroup.Text = "· " + Language.Pop_RoutingGroup;
            lblRoutingOperation.Text = "· " + Language.Pop_RoutingOperation;
            lblPageOperation.Text = "· " + Language.Pop_PageOperation;
            lblPageStartTime.Text = "· " + Language.Pop_PageStartTime;
            lblPageEndTime.Text = "· " + Language.Pop_PageEndTime;
            lblPageRunTime.Text = "· " + Language.Pop_PageRunTime;
            lblPageIdleTime.Text = "· " + Language.Pop_PageIdleTime;
            lblPageWorkTime.Text = "· " + Language.Pop_PageWorkTime;
            lblMessage.Text = string.Empty;

            pageMaterial.Text = Language.Pop_TabMaterial;
            pageBom.Text = Language.Pop_TabBomSpec;
            pageInspec.Text = Language.Pop_TabInspection;
            pageEquip.Text = Language.Pop_TabEquipment;
            chkManualWrench.Text = Language.Pop_ManualWrench;

            mnuFit.Text = Language.Pop_ImageFit;
            mnuActual.Text = Language.Pop_ImageActualSize;
        }

        /// <summary>현재 테마 색을 화면 전체에 입힌다. 상단바에서 라이트/다크를 전환하면 다시 호출된다.</summary>
        public void ApplyTheme()
        {
            BackColor = AppTheme.Surface;
            spacerTop.BackColor = AppTheme.Surface;
            spacerBottom.BackColor = AppTheme.Surface;
            spacerMain.BackColor = AppTheme.Surface;

            AppTheme.CardPanel(toolbarCard);
            AppTheme.CardPanel(viewerCard);
            AppTheme.CardPanel(infoCard);
            AppTheme.CardPanel(bottomCard);
            AppTheme.PlainPanel(mainPanel);
            AppTheme.PlainPanel(fieldPanel);

            // ⚠️ 손잡이에 BringToFront()를 부르지 말 것.
            // 도킹은 z-순서의 <b>역순</b>으로 처리되므로, 앞으로 끌어올리면 도킹이 가장 늦게 잡힌다.
            // 그러면 Fill인 fieldPanel이 먼저 영역을 다 가져가고 손잡이가 그 위에 겹쳐 글자를 가린다
            // (실제로 그렇게 보였다). Designer의 Add 순서가 이미 올바른 도킹 순서를 만든다.
            RefreshAccordionButtons();
            viewerSurface.BackColor = AppTheme.Card;
            picView.BackColor = AppTheme.ImageViewerBack;

            // 버튼 색은 앱 공통(#6D7C92 + 흰 글자). 글꼴은 Designer에서 버튼마다 정해 둔 값을 그대로 쓴다.
            foreach (var button in new[] { btnPrevProcess, btnNextProcess, btnStopProcess, btnProcessList, btnAdminAlarm, btnRework,
                                            btnFit, btnZoomIn, btnZoomOut })
                AppTheme.StandardButton(button, keepFont: true);

            SetProgressState(_progressState);   // 테마를 다시 입혀도 상태 색이 보라로 되돌아가지 않도록
            viewerSurface.Invalidate();
        }

        /// <summary>
        /// 화면을 contentHost 크기에 맞춰 비율 스케일하고, 다국어 문구 맞춤 대상을 등록한다.
        /// 테마 · 그리드 구성 뒤에 불러야 한다 — 그때의 글꼴 · 행 높이가 배율 1.0의 기준이 된다.
        /// </summary>
        private void AttachScaling()
        {
            if (Parent == null)
                return;

            _scaler.Attach(Parent);

            // 한 줄 칸(버튼 · 상태 표시 · 항목 이름)은 글자 축소, 안내 문구 상자는 줄바꿈 우선.
            // 제목(lblInfoTitle)은 자동 크기 라벨이라 대상이 아니다 — 카드 폭에 비해 짧아 넘칠 일도 없다.
            _textFitter.Register(TextFitMode.Shrink,
                btnPrevProcess, btnNextProcess, btnStopProcess, btnProcessList, btnAdminAlarm, btnRework,
                lblWorkerTile, lblWorker, lblWorkstation, lblProgressState,
                lblWorkOrderNo, lblItemCode, lblProductSerialNo, lblRoutingGroup, lblRoutingOperation,
                lblPageOperation, lblPageStartTime, lblPageEndTime, lblPageRunTime, lblPageIdleTime, lblPageWorkTime,
                chkManualWrench);
            _textFitter.Register(TextFitMode.Wrap, lblMessage);
        }

        /// <summary>
        /// 배율을 다시 맞추면 크기가 디자이너 값(펼친 상태) × 배율로 돌아간다 — 접어 둔 패널은 다시 접는다.
        /// </summary>
        private void Scaler_Rescaled(object sender, EventArgs e)
        {
            if (!_infoExpanded)
                SetInfoExpanded(false);
            if (!_bottomExpanded)
                SetBottomExpanded(false);
        }

        /// <summary>중지 버튼이 서버에 요청할 동작. 서버 상태에 따라 stop ↔ resume으로 바뀐다.</summary>
        private void InitActionTiles()
        {
            btnStopProcess.Tag = PopProcessAction.stop;
        }

        /// <summary>
        /// 설비 목록(서버 기준정보)을 장비 관리자에 등록하고 이벤트를 구독한 뒤 연결을 시작한다.
        /// 설비 Id는 목록 순번으로 다시 매긴다 — 장비 관리자 안에서만 쓰는 식별자다.
        /// 등록 · 시작은 <see cref="Devices.Attach"/>가 한다 — 이전 POP가 아직 열려 있어도 이 화면이 장비를 넘겨받는다.
        /// </summary>
        private void WireEquipmentEvents()
        {
            var infos = new List<NutrunnerInfo>();
            int index = 0;
            foreach (var eq in _equipments)
            {
                // IP · 포트가 없는 설비는 SetGridData에서 이미 뺐다(SelectConnectableEquipments).
                // 여기서 다시 걸리는 것은 방어용이다 — 연결할 수 없는 대상을 등록하지 않는다.
                if (string.IsNullOrWhiteSpace(eq.EquipmentIp) || !TryGetEquipmentPort(eq, out var port))
                    continue;

                var info = new NutrunnerInfo();
                info.Id = eq.Id = index++;
                info.Code = eq.EquipmentCode;
                info.Ip = eq.EquipmentIp.Trim();
                info.Port = port;
                info.EventPort = port + 1;
                info.Name = eq.EquipmentName;
                info.ModelName = eq.EquipmentModel;
                info.ConnectTimeoutMs = _appSet.AppSet.ModbusConnectTimeoutMs;
                info.ResponseTimeoutMs = _appSet.AppSet.ModbusResponseTimeoutMs;
                info.ReconnectWaitMs = _appSet.AppSet.ModbusReconnectWaitMs;
                info.EventConnectDelayMs = _appSet.AppSet.ModbusEventConnectDelayMs;
                info.KeepAliveTimeMs = _appSet.AppSet.ModbusKeepAliveTimeMs;
                info.KeepAliveIntervalMs = _appSet.AppSet.ModbusKeepAliveIntervalMs;

                infos.Add(info);
            }

            // 시작 직후의 첫 연결 이벤트를 놓치지 않도록 구독을 먼저 한다.
            _equip.Manager.ConnectionChanged += OnConnectionChanged;
            _equip.Manager.ReadyChanged += OnReadyChanged;
            _equip.Manager.AlarmOccurred += OnErrorOccurred;
            _equip.Manager.RegisterDataReceived += OnRegisterDataReceived;
            _equip.Manager.FasteningReceived += OnFasteningReceived;
            _equip.Attach(this, infos);

            Screen.RefreshEquipmentStates();
        }

        /// <summary>
        /// 구독을 먼저 끊고 장비를 놓는다 — 멈추는 중에 올라온 이벤트가 사라진 화면을 건드리지 않게.
        /// 매니저는 폐기하지 않는다(앱 전역 공용, 종료 때 Global이 정리). 다른 POP가 이미 넘겨받았으면 장비는 그대로 둔다.
        /// </summary>
        private void UnwireEquipmentEvents()
        {
            _equip.Manager.ConnectionChanged -= OnConnectionChanged;
            _equip.Manager.ReadyChanged -= OnReadyChanged;
            _equip.Manager.AlarmOccurred -= OnErrorOccurred;
            _equip.Manager.RegisterDataReceived -= OnRegisterDataReceived;
            _equip.Manager.FasteningReceived -= OnFasteningReceived;
            _equip.Detach(this);
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  화면
        // ─────────────────────────────────────────────────────────────────────────

        private void PopScreen_Load(object sender, EventArgs e)
        {
            // Designer가 Load를 직접 구독하므로 VS 디자이너에서도 호출된다 — 런타임이 아니면 아무것도 하지 않는다.
            if (DesignMode)
                return;

            try
            {
                Init();
                ApplyLanguage();
                ApplyTheme();
                InitActionTiles();
                Screen.InitGrids();

                // 도면을 넓게 보는 것이 먼저라 우측 공정 정보는 접힌 채로 시작한다(손잡이로 펼친다).
                SetInfoExpanded(false);

                AttachScaling();

                // 화면 틀을 먼저 보여 준 뒤 조회한다(ScreenLoad 참고).
                ScreenLoad.AfterPaint(this, ShowInitialData);
            }
            catch (Exception ex)
            {
                HandleError("화면 초기화", ex);
            }
        }

        /// <summary>화면이 다 그려진 뒤 한 번 — 공정 정보를 읽고, 설비를 배선한 다음 진입 공정을 시작한다.</summary>
        private async Task ShowInitialData()
        {
            try
            {
                await Screen.GetProcessInfo();
                if (IsDisposed || Disposing) return;

                // 진입한 공정이 마지막 공정인지 — 완료하기 전에 작업자에게 확인하는 데 쓴다.
                await Screen.GetIsLastProcess();
                if (IsDisposed || Disposing) return;

                WireEquipmentEvents();

                // 진입도 공정 이동과 같다 — 시작 버튼이 없으므로 여기서 시작하지 않으면 첫 공정은 시작할 길이 없다.
                await Screen.StartEnteredProcess();
            }
            catch (Exception ex)
            {
                HandleError("화면 초기화", ex);
            }
        }

        private void PopScreen_Disposed(object sender, EventArgs e)
        {
            try
            {
                if (_equip != null)
                    UnwireEquipmentEvents();
            }
            catch (Exception ex)
            {
                // 닫히는 중이라 창은 띄우지 않는다.
                ServerLog.Unexpected("POP 설비 연결 해제", ex);
            }

            // 들고 있던 도면 이미지를 해제한다.
            _viewer?.Dispose();
            _viewer = null;

            // contentHost는 앱이 끝날 때까지 남으므로 Resize 구독을 끊어야 닫힌 화면이 붙들려 있지 않는다.
            _textFitter.Dispose();
            _scaler.Dispose();
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  상단 공정 제어 버튼
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// [공정 중지/재시작] 핸들러. 요청할 동작은 버튼 Tag에서 읽는다.
        /// 버튼 문구는 여기서 바꾸지 않는다 — 요청 뒤 서버가 다시 내려준 진행 상태로 <see cref="SetProgressState"/>가 그린다.
        /// </summary>
        private async void ProcessAction_Click(object sender, EventArgs e)
        {
            if (!((sender as Control)?.Tag is PopProcessAction action))
                return;

            try
            {
                if (!await Screen.SetProcessAction(action))
                    return;

                // 재시작 직후엔 공정 진입 때 건너뛴 일을 잇는다 — 중지 공정은 자동 재시작하지 않아
                // 진입 시 시리얼 입력 팝업이 뜨지 않았으므로, 여기서 남은 시리얼 입력 → 완료 검사를 한다.
                if (action == PopProcessAction.resume)
                {
                    await Screen.CheckMaterialSerials();
                    if (IsDisposed || Disposing) return;
                }

                // 시작 · 재시작 직후에도 완료 여부를 본다 — 탭 3개가 모두 비어 있는 공정은 바로 넘어간다.
                if (action == PopProcessAction.start || action == PopProcessAction.resume)
                    await Screen.TryAutoProgress();
            }
            catch (Exception ex)
            {
                HandleError("공정 상태 전이", ex);
            }
        }

        /// <summary>
        /// [리웍] — 공정을 진행 중으로 되돌려 값을 다시 고칠 수 있게 한다. 재시작과 달리 바로 완료 검사(TryAutoProgress)를 하지 않는다 —
        /// 리웍은 작업자가 값을 고친 뒤 직접 공정을 옮기고, 그때 완료 여부를 판단한다.
        /// </summary>
        private async void btnRework_Click(object sender, EventArgs e)
        {
            try
            {
                await Screen.StartRework();
            }
            catch (Exception ex)
            {
                HandleError("공정 리웍", ex);
            }
        }

        private async void btnPrevProcess_Click(object sender, EventArgs e)
        {
            try
            {
                await Screen.MoveProcess(-1);
            }
            catch (Exception ex)
            {
                HandleError("이전 공정 이동", ex);
            }
        }

        private async void btnNextProcess_Click(object sender, EventArgs e)
        {
            try
            {
                await Screen.MoveProcess(+1);
            }
            catch (Exception ex)
            {
                HandleError("다음 공정 이동", ex);
            }
        }

        /// <summary>
        /// PageUp = [이전 공정], PageDown = [다음 공정]. 그리드 · 입력칸에 포커스가 있어도 화면 안이면 먼저 가로챈다
        /// (그리드의 페이지 스크롤보다 공정 이동이 우선이다). 버튼과 같은 처리를 타도록 PerformClick으로 부른다 —
        /// 버튼이 꺼져 있으면(공정 목록 조회 중) 키도 먹지 않는다.
        /// Ctrl + '+' = 도면 [Zoom In], Ctrl + '-' = [Zoom Out]. 기본 자판의 +는 Shift를 같이 눌러야 나오고
        /// 숫자 키패드 +/-도 쓰이므로 세 경우를 모두 받는다.
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.PageUp:
                    btnPrevProcess.PerformClick();
                    return true;

                case Keys.PageDown:
                    btnNextProcess.PerformClick();
                    return true;

                case Keys.Control | Keys.Oemplus:
                case Keys.Control | Keys.Shift | Keys.Oemplus:
                case Keys.Control | Keys.Add:
                    btnZoomIn.PerformClick();
                    return true;

                case Keys.Control | Keys.OemMinus:
                case Keys.Control | Keys.Subtract:
                    btnZoomOut.PerformClick();
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private async void btnProcessList_Click(object sender, EventArgs e)
        {
            try
            {
                await Screen.ShowProcessList();
            }
            catch (Exception ex)
            {
                HandleError("공정 목록", ex);
            }
        }

        private async void btnAdminAlarm_Click(object sender, EventArgs e)
        {
            try
            {
                await Screen.ShowAdminAlarm();
            }
            catch (Exception ex)
            {
                HandleError("관리자 알람", ex);
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  하단 탭
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>[수동 렌치]는 BOM · 검사 탭에서, 서버가 허용한 공정일 때만 보인다.</summary>
        private void tabSpec_SelectedPageChanged(object sender, DevExpress.XtraTab.TabPageChangedEventArgs e)
        {
            switch (e.Page.Name)
            {
                case "pageInspec":
                case "pageBom":
                    chkManualWrench.Visible = _bomManualEnabled;
                    break;

                case "pageEquip":
                case "pageMaterial":
                    chkManualWrench.Visible = false;
                    break;
            }
        }

        /// <summary>수동 렌치를 켜면 "결과값 입력" 컬럼([입력] 버튼)이 나타난다.</summary>
        private void chkManualWrench_CheckedChanged(object sender, EventArgs e)
        {
            SetResultInputColumnVisible(chkManualWrench.Checked);
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  아코디언 — 우측 공정 정보 / 하단 탭을 접어 도면을 크게 본다
        // ─────────────────────────────────────────────────────────────────────────

        private void btnInfoAccordion_Click(object sender, EventArgs e)
        {
            SetInfoExpanded(!_infoExpanded);
        }

        private void btnTabsAccordion_Click(object sender, EventArgs e)
        {
            SetBottomExpanded(!_bottomExpanded);
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  도면 뷰어
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>크기가 바뀌면 자리표시자 전체를 다시 그린다(일부만 다시 그리면 잔상이 남는다).</summary>
        private void ViewerSurface_Resize(object sender, EventArgs e)
        {
            viewerSurface.Invalidate();
        }

        /// <summary>
        /// 도면이 떠 있지 않을 때 보이는 자리표시자 — 화면 설계서와 같은 대각선 상자를 그리고,
        /// 가운데에 현재 상태(<see cref="_viewerMessage"/>: 도면 없음 / 실패 사유)를 적는다.
        /// 도면 이미지가 열리면 <c>picView</c>가 이 면을 덮으므로 이 그림은 보이지 않는다.
        /// </summary>
        private void ViewerSurface_Paint(object sender, PaintEventArgs e)
        {
            var bounds = new Rectangle(1, 1, viewerSurface.Width - 3, viewerSurface.Height - 3);
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (var pen = new Pen(AppTheme.CardBorder))
            {
                e.Graphics.DrawRectangle(pen, bounds);
                e.Graphics.DrawLine(pen, bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
                e.Graphics.DrawLine(pen, bounds.Right, bounds.Top, bounds.Left, bounds.Bottom);
            }

            using (var brush = new SolidBrush(AppTheme.TextMuted))
            using (var font = AppTheme.SmallFont)
            {
                var title = Language.Pop_Drawing;
                var titleSize = e.Graphics.MeasureString(title, font);
                e.Graphics.DrawString(title, font, brush,
                    bounds.Left + ((bounds.Width - titleSize.Width) / 2),
                    bounds.Top + 10);
            }

            if (string.IsNullOrEmpty(_viewerMessage))
                return;

            // 실패 사유는 경로까지 들어가 길 수 있으므로 가운데 정렬로 줄바꿈해 그린다.
            using (var brush = new SolidBrush(AppTheme.TextMuted))
            using (var font = AppTheme.BodyFont)
            using (var format = new StringFormat())
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                var box = new RectangleF(
                    bounds.Left + (bounds.Width * 0.1f),
                    bounds.Top + (bounds.Height * 0.42f),
                    bounds.Width * 0.8f,
                    bounds.Height * 0.3f);

                e.Graphics.DrawString(_viewerMessage, font, brush, box, format);
            }
        }

        /// <summary>
        /// 이미지가 없으면 우클릭 메뉴를 띄우지 않는다 — 빈 화면에서 맞춤/원본 크기는 의미가 없다.
        /// 현재 맞춤 모드인지 체크 표시로 알려 준다.
        /// </summary>
        private void MenuView_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = _viewer == null || !_viewer.HasImage;
            if (!e.Cancel) mnuFit.Checked = _viewer.IsFitMode;
        }

        /// <summary>우클릭 → Image fit. 이후 창 크기가 바뀌어도 계속 맞춘다(ImageView.IsFitMode).</summary>
        private void MnuFit_Click(object sender, EventArgs e)
        {
            _viewer?.ZoomToFit();
        }

        /// <summary>우클릭 → 원본 크기(100%).</summary>
        private void MnuActual_Click(object sender, EventArgs e)
        {
            _viewer?.ActualSize();
        }

        private void BtnZoomIn_Click(object sender, EventArgs e)
        {
            _viewer?.ZoomIn();
        }

        private void BtnZoomOut_Click(object sender, EventArgs e)
        {
            _viewer?.ZoomOut();
        }

        private void BtnFit_Click(object sender, EventArgs e)
        {
            _viewer?.ZoomToFit();
        }

        /// <summary>
        /// 도면 더블클릭 → 도면이 떠 있는 모니터 전체로 크게 본다.
        /// 전체화면 창은 받은 그림을 소유·해제하므로 원본이 아닌 복사본을 넘긴다(<see cref="DrawingFullScreenForm"/> 참고).
        /// </summary>
        private void PicView_DoubleClick(object sender, EventArgs e)
        {
            if (_viewer == null || !_viewer.HasImage || picView.Image == null) return;

            Image copy = null;
            try
            {
                copy = new Bitmap(picView.Image);
                using (var form = new DrawingFullScreenForm())
                {
                    form.Initialize(copy, System.Windows.Forms.Screen.FromControl(picView));   // Screen은 이 화면의 IPopScreen 속성과 이름이 겹친다
                    copy = null;   // 이제 form 소유 — 닫힐 때 form이 해제한다
                    form.ShowDialog(FindForm());
                }
            }
            catch (Exception ex)
            {
                copy?.Dispose();
                AppLog.Error("[POP] 도면 전체화면 표시 실패", ex);
                if (IsDisposed || Disposing) return;
                AppMessageBox.Warn(this, Language.Pop_DrawingLoadFailed);
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  설비 이벤트 — 장비 관리자의 수신 스레드에서 올라오므로 화면 스레드로 넘겨 처리한다
        // ─────────────────────────────────────────────────────────────────────────

        private void OnFasteningReceived(object sender, NutrunnerFasteningEventArgs e)
        {
            InvokeIfRequired(HandleFastening, e);
        }

        private void OnRegisterDataReceived(object sender, NutrunnerRegisterEventArgs e)
        {
            InvokeIfRequired(HandleRegisterData, e);
        }

        private void OnConnectionChanged(object sender, EquipmentConnectionEventArgs e)
        {
            InvokeIfRequired(HandleConnectionChanged, e);
        }

        private void OnReadyChanged(object sender, EquipmentConnectionEventArgs e)
        {
            InvokeIfRequired(HandleReadyChanged, e);
        }

        /// <summary>
        /// 알람은 로그로 남긴다. 재연결을 3회 연속 실패한 알람(ConnectFailed)만 작업자에게 창으로 알린다 —
        /// 재연결은 뒤에서 계속되지만(NutrunnerSequence) 그동안 체결이 들어오지 않으므로 알아채야 한다.
        /// 창은 장비마다 "사용 가능"이 될 때까지 한 번만 띄운다(<see cref="_connectFailedShown"/>).
        /// </summary>
        private void OnErrorOccurred(object sender, AlarmEventArgs e)
        {
            AppLog.Warn($"[ALARM] [{e.OccurredAt} / {e.Code}] {e.Message}");

            if (e.Code == AlarmCode.ConnectFailed)
                InvokeIfRequired(ShowConnectFailed, e);
        }

        private void ShowConnectFailed(AlarmEventArgs e)
        {
            // AlarmEventArgs.Code는 알람 코드라 장비 코드는 기반 클래스에서 읽는다.
            string equipmentCode = ((EquipmentEventArgs)e).Code;
            if (!_connectFailedShown.Add(equipmentCode))
                return;

            AppMessageBox.Warn(this,
                ErrorText.WithCode(string.Format(Language.Pop_EquipmentConnectFailed, equipmentCode), ErrorCodes.EquipmentConnectFailed),
                Language.Pop_Title);
        }

        /// <summary>체결 결과 한 건 — 원본을 로그로 남기고 토크 · 각도(Angle3)를 BOM 스팩에 채운다.</summary>
        private async void HandleFastening(NutrunnerFasteningEventArgs e)
        {
            try
            {
                FasteningData data = e.Data;

                AppLog.Info("────────────────────────");
                AppLog.Info($"[ID {e.Code}] Fastening");
                AppLog.Info($"[ToolId] : {data.ToolId}");
                AppLog.Info($"[FastenTime] : {data.FastenTime}");
                AppLog.Info($"[Preset] : {data.Preset}");
                AppLog.Info($"[TargetTorque] : {data.TargetTorqueValue:0.00}");
                AppLog.Info($"[Torque] : {data.TorqueValue:0.00}");
                AppLog.Info($"[Speed] : {data.Speed}");
                AppLog.Info($"[Angle1] : {data.Angle1}");
                AppLog.Info($"[Angle2] : {data.Angle2}");
                AppLog.Info($"[Angle3] : {data.Angle3}");
                AppLog.Info($"[RemainScrew] : {data.RemainScrew}");
                AppLog.Info($"[Error] : {data.Error}");
                AppLog.Info($"[Direction] : {data.Direction}");
                AppLog.Info($"[EventStatus] : {data.EventStatus}");
                AppLog.Info($"[SnugAngle] : {data.SnugAngle}");
                AppLog.Info($"[Barcode] : {data.Barcode}");

                // 역방향(풀기) 동작 값은 체결 토크가 아니므로 BOM 스팩에 표기하지 않는다.
                if (data.IsReverse)
                {
                    AppLog.Info($"[ID {e.Code}] 역방향 동작 - 토크 값 무시");
                    return;
                }

                await Screen.SetTorqueFromEquipment(e.Code, (decimal)data.TorqueValue, (decimal)data.Angle3);
            }
            catch (Exception ex)
            {
                HandleError("설비 체결 결과 처리", ex);
            }
        }

        private void HandleRegisterData(NutrunnerRegisterEventArgs e)
        {
            if (!e.Success)
                return;

            // 장비 상태 표시는 ReadyChanged · ConnectionChanged로 갱신한다 — 응답마다 그리드를 다시 그리지 않는다.

            // Alive(상태 레지스터 읽기)는 5초마다 와서 로그 · 콘솔을 덮는다 — 연결 확인에만 쓰고 기록하지 않는다.
            if (e.StartAddress == (ushort)NutrunnerRegister.Status)
                return;

            ushort address = e.StartAddress;
            foreach (ushort value in e.Data)
                AppLog.Warn($"[ID {e.Code}][{address++:D4}] : {value}");
        }

        private void HandleConnectionChanged(EquipmentConnectionEventArgs e)
        {
            AppLog.Info($"[ID {e.Code}] {(e.IsConnected ? "연결 됨" : "연결 끊김")}");
            Screen.RefreshEquipmentStates();
        }

        /// <summary>
        /// 장비가 작업에 쓸 수 있게 되거나(이벤트 수신 확인) 쓸 수 없게 된 순간.
        /// 목표 토크는 연결 직후가 아니라 여기서 쓴다 — 연결 직후에 쓰면 토크 · 프리셋 · 잠금 쓰기 3건이
        /// EventDataEnable보다 먼저 줄을 서서, 장비가 하나라도 응답하지 않으면 "사용 가능" 표시가 그만큼(건당 응답 대기) 늦었다.
        /// </summary>
        private void HandleReadyChanged(EquipmentConnectionEventArgs e)
        {
            AppLog.Info($"[ID {e.Code}] {(e.IsConnected ? "사용 가능(이벤트 수신 확인)" : "사용 불가")}");
            Screen.RefreshEquipmentStates();

            if (e.IsConnected)
            {
                // 실제로 응답을 주고받았으니 다음 끊김에는 다시 알린다.
                _connectFailedShown.Remove(e.Code);
                Screen.WriteBomSpecTorque();
            }
        }

        /// <summary>설비 수신 스레드에서 올라온 처리를 화면 스레드로 넘긴다. 화면이 이미 닫혔으면 버린다.</summary>
        private void InvokeIfRequired<T>(Action<T> action, T args)
        {
            if (IsDisposed || Disposing)
                return;

            if (InvokeRequired)
                BeginInvoke(action, args);
            else
                action(args);
        }
    }
}
