using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using SEOJIN_MEIS.Client.ApiClient;
using SEOJIN_MEIS.Client.Common.Logging;
using SEOJIN_MEIS.Client.Common.Models;
using SEOJIN_MEIS.Client.Common.Storage;
using SEOJIN_MEIS.Client.Composition;
using SEOJIN_MEIS.Client.Equipment;
using SEOJIN_MEIS.Client.Equipment.Protocol;
using SEOJIN_MEIS.Client.Forms.Common;
using SEOJIN_MEIS.Client.Forms.WorkOrderEntry;
using SEOJIN_MEIS.Client.Localization;
using SEOJIN_MEIS.Client.Mock;
using SEOJIN_MEIS.Client.Theme;

namespace SEOJIN_MEIS.Client.Forms.Pop
{
    /// <summary>
    /// MEIS &gt; 작업관리 &gt; POP 본 화면 — 진입 화면에서 고른 페이지 공정 하나를 실제로 작업하는 화면.
    ///
    /// 구성:
    ///  - 상단  : 공정 제어 버튼 7개(이전/다음/생략/중지/시작/공정 목록/관리자 알람) + 작업자 · 현재시각
    ///  - 좌측  : 작업 표준/도면 표시 영역(연결할 뷰어가 정해지기 전까지는 자리표시자)
    ///  - 우측  : 진행 중 공정 정보 + 워크스테이션 띠 + 공정 진행 상태 띠
    ///  - 하단  : 탭 4개 — 투입자재 / BOM 스팩 / 검사 스팩 / 연결 장비
    ///
    /// 탭별 동작:
    ///  1) 투입자재  : "시리얼번호 입력상태" 셀 클릭 → &lt;투입자재 정보 입력&gt; 팝업에서 시리얼번호 입력
    ///  2) BOM 스팩  : 기준정보의 토크 스펙을 표시하고, 연결 장비에서 올라온 값을 토크 결과값에 채운다.
    ///                 [수동 렌치]를 켜면 "결과값 입력" 컬럼이 나타나 작업자가 직접 입력할 수 있다.
    ///  3) 검사 스팩 : 기준정보의 검사 항목을 표시하고 결과상태/결과값을 직접 입력한다.
    ///  4) 연결 장비 : 현재 워크스테이션에 물린 장비와 Modbus 연결 상태를 실시간으로 보여 준다.
    ///
    /// 업무 판정(합·불, 상태 전이 가능 여부 등)은 전부 서버가 하고, 이 화면은 표시와 입력 전달만 한다.
    /// VS 디자이너 호환을 위해 매개변수 없는 생성자만 두고, 실제 의존성은 Initialize()로 주입한다.
    /// </summary>
    public partial class PopScreen : XtraUserControl, IThemedScreen
    {
        /// <summary>[BOM 스팩] 탭의 "결과값 입력" 컬럼(데이터에 없는 화면 전용 컬럼)의 필드명.</summary>
        private const string ResultInputField = "__ResultInput";

        /// <summary>
        /// 워크스테이션 / 공정 진행 상태 띠 하나의 높이.
        /// 디자이너의 bannerPanel 높이(<c>= BannerHeight * 2 + 간격 8 + 아래 여백 14</c>)와 함께 맞춰 둘 것.
        /// </summary>
        private const int BannerHeight = 48;

        /// <summary>공정 제어 타일 하나의 폭. 디자이너의 tileProcess 크기와 함께 맞춰 둘 것(7개 × 폭 + 간격 8).</summary>
        private const int ProcessTileWidth = 104;

        /// <summary>공정 제어 타일 높이. 디자이너의 tileProcess 높이와 같게 둔다.</summary>
        private const int ProcessTileHeight = 64;

        private ClientContext _context;
        private WorkEntryContext _entry;
        private PopProcessKey _key;
        private PopProcessInfoDto _info;

        private List<PopInputMaterialDto> _materials = new List<PopInputMaterialDto>();
        private List<PopBomSpecDto> _bomSpecs = new List<PopBomSpecDto>();
        private List<PopInspectionSpecDto> _inspections = new List<PopInspectionSpecDto>();
        private List<PopEquipmentDto> _equipments = new List<PopEquipmentDto>();

        private RepositoryItemButtonEdit _resultInputEditor;
        private RepositoryItemComboBox _resultStateEditor;
        private RepositoryItemTextEdit _resultValueEditor;

        private System.Windows.Forms.Timer _clock;
        private PopTab _activeTab = PopTab.Material;

        /// <summary>도면 자리표시자에 그릴 안내 문구(불러오는 중 / 도면 없음 / 실패 사유).</summary>
        private string _viewerMessage;

        /// <summary>
        /// 화면에 떠 있는 PDF의 원본 바이트. PdfViewer가 문서를 닫을 때까지 스트림을 잡고 있으므로
        /// 필드로 들고 있다가 다음 도면을 열기 전/화면을 닫을 때 함께 정리한다.
        /// </summary>
        private MemoryStream _drawingStream;

        /// <summary>진행 중인 도면 읽기. 공정을 빠르게 옮기면 이전 요청은 취소한다.</summary>
        private CancellationTokenSource _drawingCancellation;

        public PopScreen()
        {
            InitializeComponent();
        }

        public void Initialize(ClientContext context)
        {
            Initialize(context, null);
        }

        /// <summary>
        /// POP 진입 화면에서 선택한 작업 대상(<paramref name="entry"/>)을 받아 화면을 구성한다.
        /// 메뉴에서 바로 열린 경우처럼 대상이 없으면 조회 없이 안내만 하고 빈 화면으로 남는다.
        /// </summary>
        public void Initialize(ClientContext context, WorkEntryContext entry)
        {
            _context = context;
            _entry = entry;
            _key = BuildKey(entry);

            ApplyTexts();
            ApplyTheme();
            ConfigureGrids();
            SetActiveTab(PopTab.Material);

            viewerSurface.Paint += ViewerSurface_Paint;

            // Panel은 크기가 커져도 <b>새로 드러난 부분만</b> 다시 그린다.
            // 이 표면은 전체를 채우는 X와 가운데 문구를 직접 그리므로, 그대로 두면
            // 아코디언으로 넓어질 때 예전 그림이 남아 X가 겹치고 문구 조각이 떠 있는다(실제로 그랬다).
            viewerSurface.Resize += (s, e) => viewerSurface.Invalidate();
            _viewerMessage = Strings.Pop_DrawingNone;
            gridViewMaterial.RowCellClick += GridViewMaterial_RowCellClick;

            StartClock();
            WireEquipmentEvents();

            Load += PopScreen_Load;
            Disposed += PopScreen_Disposed;
        }

        /// <summary>화면의 모든 고정 문구를 현재 언어로 채운다(언어 전환 시 화면을 다시 열면 반영된다).</summary>
        private void ApplyTexts()
        {
            tilePrevProcess.Text = Strings.Pop_PrevProcess;
            tileNextProcess.Text = Strings.Pop_NextProcess;
            tileSkipProcess.Text = Strings.Pop_SkipProcess;
            tileStopProcess.Text = Strings.Pop_StopProcess;
            tileStartProcess.Text = Strings.Pop_StartProcess;
            tileProcessList.Text = Strings.Pop_ProcessList;
            tileAdminAlarm.Text = Strings.Pop_AdminAlarm;

            lblWorkerCaption.Text = Strings.Pop_Worker;
            lblTimeCaption.Text = Strings.Pop_CurrentTime;
            lblWorker.Text = ResolveWorkerName();

            lblInfoTitle.Text = Strings.Pop_ProcessInfoTitle;
            lblWorkOrderNo.Text = "· " + Strings.Pop_WorkOrderNo;
            lblItemCode.Text = "· " + Strings.Pop_ItemCode;
            lblProductSerialNo.Text = "· " + Strings.Pop_ProductSerialNo;
            lblRoutingGroup.Text = "· " + Strings.Pop_RoutingGroup;
            lblRoutingOperation.Text = "· " + Strings.Pop_RoutingOperation;
            lblPageOperation.Text = "· " + Strings.Pop_PageOperation;
            lblPageStartTime.Text = "· " + Strings.Pop_PageStartTime;
            lblPageEndTime.Text = "· " + Strings.Pop_PageEndTime;
            lblPageRunTime.Text = "· " + Strings.Pop_PageRunTime;
            lblPageIdleTime.Text = "· " + Strings.Pop_PageIdleTime;
            lblPageWorkTime.Text = "· " + Strings.Pop_PageWorkTime;

            btnTabMaterial.Text = Strings.Pop_TabMaterial;
            btnTabBomSpec.Text = Strings.Pop_TabBomSpec;
            btnTabInspection.Text = Strings.Pop_TabInspection;
            btnTabEquipment.Text = Strings.Pop_TabEquipment;
            chkManualWrench.Text = Strings.Pop_ManualWrench;
            btnSaveResults.Text = Strings.Pop_SaveResult;
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
            AppTheme.PlainPanel(bannerPanel);
            AppTheme.PlainPanel(tabHeaderPanel);
            AppTheme.PlainPanel(tabBodyPanel);

            // 헤더(도킹된 제목/탭 줄) 위에 얹혀 보이도록 맨 앞으로 올린다.
            btnInfoAccordion.BringToFront();
            btnTabsAccordion.BringToFront();
            RefreshAccordionButtons();
            viewerSurface.BackColor = AppTheme.Card;

            // 공정 제어는 TileBar 타일 — 색은 동작의 성격을 따른다
            // (시작=초록, 중지=빨강, 이동=남보라, 생략=회색, 목록=보라, 알람=주황).
            AppTheme.TileStrip(tileProcess, ProcessTileHeight, ProcessTileWidth);
            AppTheme.Tile(tilePrevProcess, AppTheme.Indigo, AppIcons.Reset);
            AppTheme.Tile(tileNextProcess, AppTheme.Indigo, AppIcons.Refresh);
            AppTheme.Tile(tileSkipProcess, AppTheme.Slate, AppIcons.Close);
            AppTheme.Tile(tileStopProcess, AppTheme.Rose, AppIcons.Stop);
            AppTheme.Tile(tileStartProcess, AppTheme.Emerald, AppIcons.Play);
            AppTheme.Tile(tileProcessList, AppTheme.Violet, AppIcons.Layers);
            AppTheme.Tile(tileAdminAlarm, AppTheme.Amber, AppIcons.Bell);

            AppTheme.Caption(lblWorkerCaption);
            AppTheme.Caption(lblTimeCaption);
            AppTheme.StatePill(lblWorker, AppTheme.Indigo, new Size(100, 32));
            AppTheme.StatePill(lblCurrentTime, AppTheme.Slate, new Size(112, 32));
            lblCurrentTime.Appearance.Font = new Font("Consolas", 11F, FontStyle.Bold);

            AppTheme.SectionTitle(lblInfoTitle, AppTheme.Violet);
            ThemeFieldLabels();
            ThemeFieldEditors();

            AppTheme.Banner(lblWorkstation, AppTheme.Amber, BannerHeight);
            AppTheme.Banner(lblProgressState, AppTheme.Violet, BannerHeight);

            AppTheme.CheckBox(chkManualWrench);
            AppTheme.AccentButton(btnSaveResults, AppTheme.Indigo);

            AppTheme.Grid(gridMaterial, gridViewMaterial, AppTheme.Emerald);
            AppTheme.Grid(gridBomSpec, gridViewBomSpec, AppTheme.Indigo);
            AppTheme.Grid(gridInspection, gridViewInspection, AppTheme.Violet);
            AppTheme.Grid(gridEquipment, gridViewEquipment, AppTheme.Cyan);

            // AppTheme.Grid는 모든 그리드를 읽기 전용으로 되돌리므로, 입력이 필요한 두 탭만 다시 열어 준다.
            gridViewBomSpec.OptionsBehavior.Editable = true;
            gridViewInspection.OptionsBehavior.Editable = true;

            StyleTabButtons();
            viewerSurface.Invalidate();
        }

        private void ThemeFieldLabels()
        {
            AppTheme.Caption(lblWorkOrderNo);
            AppTheme.Caption(lblItemCode);
            AppTheme.Caption(lblProductSerialNo);
            AppTheme.Caption(lblRoutingGroup);
            AppTheme.Caption(lblRoutingOperation);
            AppTheme.Caption(lblPageOperation);
            AppTheme.Caption(lblPageStartTime);
            AppTheme.Caption(lblPageEndTime);
            AppTheme.Caption(lblPageRunTime);
            AppTheme.Caption(lblPageIdleTime);
            AppTheme.Caption(lblPageWorkTime);
        }

        private void ThemeFieldEditors()
        {
            AppTheme.Editor(txtWorkOrderNo);
            AppTheme.Editor(txtItemCode);
            AppTheme.Editor(txtProductSerialNo);
            AppTheme.Editor(txtRoutingGroup);
            AppTheme.Editor(txtRoutingOperation);
            AppTheme.Editor(txtPageOperation);
            AppTheme.Editor(txtPageStartTime);
            AppTheme.Editor(txtPageEndTime);
            AppTheme.Editor(txtPageRunTime);
            AppTheme.Editor(txtPageIdleTime);
            AppTheme.Editor(txtPageWorkTime);
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  조회
        // ─────────────────────────────────────────────────────────────────────────

        private async void PopScreen_Load(object sender, EventArgs e)
        {
            if (_key == null)
            {
                AppMessageBox.Warn(this, Strings.Pop_NoTargetProcess, Strings.Pop_Title);
                return;
            }

            await LoadAllAsync();
        }

        /// <summary>진행 중 공정 정보 + 탭 4개를 한 번에 다시 읽는다(진입 시 / 공정 이동 후).</summary>
        private async Task LoadAllAsync()
        {
            try
            {
                _info = await FetchProcessInfoAsync();
                _materials = await FetchMaterialsAsync();
                _bomSpecs = await FetchBomSpecsAsync();
                _inspections = await FetchInspectionsAsync();
                _equipments = await FetchEquipmentsAsync();
            }
            catch (ApiException ex)
            {
                AppMessageBox.Warn(this, ex.Message, Strings.Pop_Title);
                return;
            }

            BindProcessInfo();

            gridMaterial.DataSource = _materials;
            gridBomSpec.DataSource = _bomSpecs;
            gridInspection.DataSource = _inspections;
            gridEquipment.DataSource = _equipments;

            RefreshEquipmentStates();
            await LoadDrawingAsync();
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  작업 표준 / 도면 (SMB 공유 폴더의 PDF)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// 서버가 내려준 도면 경로(<see cref="PopProcessInfoDto.DrawingFilePath"/>)의 PDF를
        /// SMB 공유 폴더에서 읽어 좌측 뷰어에 띄운다.
        ///
        /// 공유 폴더가 아직 안 잡혔거나 파일이 없어도 화면은 계속 쓸 수 있어야 하므로,
        /// 실패는 예외로 올리지 않고 도면 영역에 사유만 적어 둔다.
        /// </summary>
        private async Task LoadDrawingAsync()
        {
            CloseDrawing();

            var path = _info?.DrawingFilePath;
            if (string.IsNullOrWhiteSpace(path))
            {
                ShowViewerMessage(Strings.Pop_DrawingNone);
                return;
            }

            var share = _context?.DocumentShare;
            if (share == null || !share.IsConfigured)
            {
                ShowViewerMessage(Strings.Pop_DrawingShareNotConfigured);
                return;
            }

            _drawingCancellation = new CancellationTokenSource();
            var cancellation = _drawingCancellation;

            ShowViewerMessage(Strings.Pop_DrawingLoading);

            try
            {
                var bytes = await share.ReadAllBytesAsync(path, cancellation.Token);

                // 읽는 동안 공정을 옮겼다면(= 다른 요청이 시작됐다면) 이 결과는 버린다.
                if (cancellation.IsCancellationRequested || cancellation != _drawingCancellation)
                    return;

                _drawingStream = new MemoryStream(bytes, false);
                pdfViewer.LoadDocument(_drawingStream);
                pdfViewer.Visible = true;
                _viewerMessage = null;

                AppLog.Info($"[POP] 도면 표시 - {path} ({bytes.Length:N0} bytes)");
            }
            catch (OperationCanceledException)
            {
                // 공정을 옮기면서 취소된 경우 — 안내할 것이 없다.
            }
            catch (DocumentShareException ex)
            {
                ShowViewerMessage($"{Strings.Pop_DrawingLoadFailed}  {ex.Message}");
                AppLog.Info($"[POP] 도면 표시 실패 - {path} : {ex.Message}");
            }
            catch (Exception ex)
            {
                // PDF가 깨졌거나 암호가 걸린 경우 등 뷰어가 던지는 예외까지 여기서 삼킨다.
                ShowViewerMessage($"{Strings.Pop_DrawingLoadFailed}  {ex.Message}");
                AppLog.Info($"[POP] 도면 표시 실패(뷰어) - {path} : {ex.Message}");
            }
        }

        /// <summary>열려 있던 도면을 닫고 자리표시자로 되돌린다(진행 중인 읽기도 취소).</summary>
        private void CloseDrawing()
        {
            if (_drawingCancellation != null)
            {
                _drawingCancellation.Cancel();
                _drawingCancellation.Dispose();
                _drawingCancellation = null;
            }

            if (pdfViewer.Visible)
            {
                pdfViewer.CloseDocument();
                pdfViewer.Visible = false;
            }

            if (_drawingStream != null)
            {
                _drawingStream.Dispose();
                _drawingStream = null;
            }
        }

        /// <summary>도면 자리표시자에 안내 문구를 띄운다.</summary>
        private void ShowViewerMessage(string message)
        {
            _viewerMessage = message;
            viewerSurface.Invalidate();
        }

        /// <summary>&lt;진행 중 공정 정보&gt;를 우측 카드에 채운다. 워크스테이션은 로그인 세션 값으로 보완한다.</summary>
        private void BindProcessInfo()
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

            // 서버가 워크스테이션을 내려주지 않으면 로그인할 때 고른 워크스테이션을 그대로 보여 준다.
            var workstation = info.WorkstationDisplay;
            if (string.IsNullOrWhiteSpace(workstation) || workstation == "()")
                workstation = _context?.CurrentUser?.Workstation;

            lblWorkstation.Text = workstation;
            lblProgressState.Text = info.ProgressState;
        }

        private async Task<PopProcessInfoDto> FetchProcessInfoAsync()
        {
            if (UseMock)
            {
                await Task.Yield();
                return MockPopData.GetProcessInfo(_key);
            }

            return await _context.ApiClients.PopApiClient.GetProcessInfoAsync(_key);
        }

        private async Task<List<PopInputMaterialDto>> FetchMaterialsAsync()
        {
            if (UseMock)
            {
                await Task.Yield();
                return MockPopData.GetInputMaterials(_key);
            }

            return await _context.ApiClients.PopApiClient.GetInputMaterialsAsync(_key)
                   ?? new List<PopInputMaterialDto>();
        }

        private async Task<List<PopBomSpecDto>> FetchBomSpecsAsync()
        {
            if (UseMock)
            {
                await Task.Yield();
                return MockPopData.GetBomSpecs(_key);
            }

            return await _context.ApiClients.PopApiClient.GetBomSpecsAsync(_key)
                   ?? new List<PopBomSpecDto>();
        }

        private async Task<List<PopInspectionSpecDto>> FetchInspectionsAsync()
        {
            if (UseMock)
            {
                await Task.Yield();
                return MockPopData.GetInspectionSpecs(_key);
            }

            return await _context.ApiClients.PopApiClient.GetInspectionSpecsAsync(_key)
                   ?? new List<PopInspectionSpecDto>();
        }

        private async Task<List<PopEquipmentDto>> FetchEquipmentsAsync()
        {
            var workstation = _context?.CurrentUser?.Workstation;

            if (UseMock)
            {
                await Task.Yield();
                return MockPopData.GetWorkstationEquipments(workstation);
            }

            return await _context.ApiClients.PopApiClient.GetWorkstationEquipmentsAsync(workstation)
                   ?? new List<PopEquipmentDto>();
        }

        private bool UseMock => _context != null && _context.Settings.UseMockData;

        // ─────────────────────────────────────────────────────────────────────────
        //  그리드 구성
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// 탭 4개의 컬럼을 구성한다.
        /// 컬럼은 DTO 형식에서 만들어지므로 빈 목록을 먼저 물린 뒤 구성하고, 이후에는 DataSource만 바꾼다.
        /// </summary>
        private void ConfigureGrids()
        {
            ConfigureMaterialGrid();
            ConfigureBomSpecGrid();
            ConfigureInspectionGrid();
            ConfigureEquipmentGrid();
        }

        private void ConfigureMaterialGrid()
        {
            // PopInputMaterialDto.SerialNumbers(컬렉션 속성)를 그리드가 마스터-디테일로 오인해
            // 행 왼쪽에 [+] 펼침 버튼을 만든다. 이 화면에는 하위 목록이 없으므로 꺼 둔다.
            gridViewMaterial.OptionsDetail.EnableMasterViewMode = false;

            gridMaterial.DataSource = new List<PopInputMaterialDto>();
            GridColumns.Apply(gridViewMaterial, Strings.Col_No,
                GridColumns.Text("MaterialCode", Strings.Col_MaterialCode, 200),
                GridColumns.Number("InputQty", Strings.Col_InputQty, 110),
                GridColumns.Text("InputUnit", Strings.Col_InputUnit, 100),
                GridColumns.Text("InputPosition", Strings.Col_InputPosition, 110),
                GridColumns.Text("SerialCheckText", Strings.Col_SerialCheck, 170),
                GridColumns.Text("SerialInputStateText", Strings.Col_SerialInputState, 180));

            gridViewMaterial.OptionsView.ColumnAutoWidth = true;

            // 클릭할 수 있는 셀임을 알 수 있게 손가락 커서로 바꾼다.
            var stateColumn = gridViewMaterial.Columns.ColumnByFieldName("SerialInputStateText");
            if (stateColumn != null)
                stateColumn.OptionsColumn.AllowFocus = true;
        }

        /// <summary>
        /// [BOM 스팩] 탭. 마지막의 "결과값 입력" 컬럼은 데이터에 없는 화면 전용 컬럼이며,
        /// 수동 렌치를 켰을 때만 보인다([입력] 버튼 → &lt;토크 결과값 입력&gt; 팝업).
        /// </summary>
        private void ConfigureBomSpecGrid()
        {
            gridBomSpec.DataSource = new List<PopBomSpecDto>();
            EnsureResultInputColumn();

            GridColumns.Apply(gridViewBomSpec, Strings.Col_No,
                GridColumns.Text("MaterialCode", Strings.Col_MaterialCode, 180),
                GridColumns.Text("InputPosition", Strings.Col_InputPosition, 100),
                GridColumns.Text("BomSpecName", Strings.Col_BomSpecName, 140),
                GridColumns.Text("SpecType", Strings.Col_SpecType, 120),
                GridColumns.Text("TorqueLower", Strings.Col_TorqueLower, 120),
                GridColumns.Text("TorqueUpper", Strings.Col_TorqueUpper, 120),
                GridColumns.Text("TorqueUnit", Strings.Col_TorqueUnit, 100),
                GridColumns.Text("TorqueResult", Strings.Col_TorqueResult, 130),
                GridColumns.Text(ResultInputField, Strings.Pop_ResultInputColumn, 120));

            gridViewBomSpec.OptionsView.ColumnAutoWidth = true;

            // 셀을 클릭해 포커스를 주기 전에도 [입력] 버튼이 모든 줄에 보이도록 한다
            // (버튼 에디터를 쓰는 컬럼이 "결과값 입력" 하나뿐이라 뷰 전체에 걸어도 무방하다).
            gridViewBomSpec.OptionsView.ShowButtonMode = DevExpress.XtraGrid.Views.Base.ShowButtonModeEnum.ShowAlways;

            NumericFormat(gridViewBomSpec, "TorqueLower", "TorqueUpper", "TorqueResult");

            // 값이 없는 토크 칸은 빈칸 대신 "-"로 보여 준다(목업과 같은 표기).
            gridViewBomSpec.CustomColumnDisplayText += Numeric_CustomColumnDisplayText;

            foreach (DevExpress.XtraGrid.Columns.GridColumn column in gridViewBomSpec.Columns)
                column.OptionsColumn.AllowEdit = column.FieldName == ResultInputField;

            SetResultInputColumnVisible(false);
        }

        /// <summary>"결과값 입력" 컬럼과 [입력] 버튼 에디터를 준비한다(이미 있으면 재사용).</summary>
        private void EnsureResultInputColumn()
        {
            if (_resultInputEditor == null)
            {
                _resultInputEditor = new RepositoryItemButtonEdit();
                _resultInputEditor.TextEditStyle = TextEditStyles.HideTextEditor;
                _resultInputEditor.Buttons.Clear();

                var button = new EditorButton(ButtonPredefines.Glyph);
                button.Caption = Strings.Pop_InputButton;
                _resultInputEditor.Buttons.Add(button);
                _resultInputEditor.ButtonClick += ResultInput_ButtonClick;

                gridBomSpec.RepositoryItems.Add(_resultInputEditor);
            }

            var column = gridViewBomSpec.Columns.ColumnByFieldName(ResultInputField);
            if (column == null)
            {
                column = gridViewBomSpec.Columns.AddField(ResultInputField);
                column.UnboundType = DevExpress.Data.UnboundColumnType.String;
                column.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.False;
            }

            column.ColumnEdit = _resultInputEditor;
        }

        private void ConfigureInspectionGrid()
        {
            gridInspection.DataSource = new List<PopInspectionSpecDto>();
            GridColumns.Apply(gridViewInspection, Strings.Col_No,
                GridColumns.Text("InspectionSpecName", Strings.Col_InspectionSpecName, 220),
                GridColumns.Left("InspectionRemark", Strings.Col_InspectionRemark, 340),
                GridColumns.Text("InspectionMethod", Strings.Col_InspectionMethod, 120),
                GridColumns.Text("ResultState", Strings.Col_ResultState, 130),
                GridColumns.Text("ResultValue", Strings.Col_ResultValue, 130));

            gridViewInspection.OptionsView.ColumnAutoWidth = true;

            _resultStateEditor = new RepositoryItemComboBox();
            _resultStateEditor.TextEditStyle = TextEditStyles.DisableTextEditor;
            _resultStateEditor.Items.Add(Strings.Pop_ResultPass);
            _resultStateEditor.Items.Add(Strings.Pop_ResultFail);
            gridInspection.RepositoryItems.Add(_resultStateEditor);

            _resultValueEditor = new RepositoryItemTextEdit();
            gridInspection.RepositoryItems.Add(_resultValueEditor);

            foreach (DevExpress.XtraGrid.Columns.GridColumn column in gridViewInspection.Columns)
                column.OptionsColumn.AllowEdit = false;

            var stateColumn = gridViewInspection.Columns.ColumnByFieldName("ResultState");
            if (stateColumn != null)
            {
                stateColumn.OptionsColumn.AllowEdit = true;
                stateColumn.ColumnEdit = _resultStateEditor;
            }

            var valueColumn = gridViewInspection.Columns.ColumnByFieldName("ResultValue");
            if (valueColumn != null)
            {
                valueColumn.OptionsColumn.AllowEdit = true;
                valueColumn.ColumnEdit = _resultValueEditor;
            }

            // 정성 검사는 결과값을 받지 않으므로 그 줄만 입력을 막는다.
            gridViewInspection.ShowingEditor += GridViewInspection_ShowingEditor;
        }

        private void ConfigureEquipmentGrid()
        {
            gridEquipment.DataSource = new List<PopEquipmentDto>();
            GridColumns.Apply(gridViewEquipment, Strings.Col_No,
                GridColumns.Text("EquipmentCode", Strings.Col_EquipmentCode, 160),
                GridColumns.Text("EquipmentName", Strings.Col_EquipmentName, 150),
                GridColumns.Left("EquipmentRemark", Strings.Col_EquipmentRemark, 260),
                GridColumns.Text("EquipmentType", Strings.Col_EquipmentType, 130),
                GridColumns.Text("CommunicationState", Strings.Col_CommunicationState, 130),
                GridColumns.Text("EquipmentState", Strings.Col_EquipmentState, 130),
                GridColumns.Text("RangeLower", Strings.Col_RangeLower, 150),
                GridColumns.Text("RangeUpper", Strings.Col_RangeUpper, 150));

            gridViewEquipment.OptionsView.ColumnAutoWidth = true;
            NumericFormat(gridViewEquipment, "RangeLower", "RangeUpper");
            gridViewEquipment.CustomColumnDisplayText += Numeric_CustomColumnDisplayText;
            gridViewEquipment.RowCellStyle += GridViewEquipment_RowCellStyle;
        }

        /// <summary>소수 한 자리 기준(1.0 / 4.25)으로 보여 주는 숫자 컬럼 서식.</summary>
        private static void NumericFormat(GridView view, params string[] fieldNames)
        {
            foreach (var fieldName in fieldNames)
            {
                var column = view.Columns.ColumnByFieldName(fieldName);
                if (column == null)
                    continue;

                column.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                column.DisplayFormat.FormatString = "0.0##";
                column.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
                column.AppearanceCell.Options.UseTextOptions = true;
            }
        }

        /// <summary>값이 비어 있는 숫자 칸은 "-"로 표시한다(목업 표기와 동일).</summary>
        private void Numeric_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.Value == null && e.Column.FieldName != ResultInputField)
                e.DisplayText = "-";
        }

        /// <summary>연결 실패/사용불가 상태는 붉은 글씨로 눈에 띄게 한다(상태 표시일 뿐 판정은 아니다).</summary>
        private void GridViewEquipment_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            if (e.Column.FieldName != "CommunicationState" && e.Column.FieldName != "EquipmentState")
                return;

            var equipment = gridViewEquipment.GetRow(e.RowHandle) as PopEquipmentDto;
            if (equipment == null)
                return;

            var connected = equipment.CommunicationState == Strings.Pop_CommConnected;
            e.Appearance.ForeColor = connected ? AppTheme.Emerald : AppTheme.Rose;
        }

        /// <summary>정성 검사 줄에서는 결과값 칸을 열지 않는다(결과상태만 고른다).</summary>
        private void GridViewInspection_ShowingEditor(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (gridViewInspection.FocusedColumn?.FieldName != "ResultValue")
                return;

            var spec = gridViewInspection.GetFocusedRow() as PopInspectionSpecDto;
            if (spec != null && !spec.IsQuantitative)
                e.Cancel = true;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  하단 탭 전환
        // ─────────────────────────────────────────────────────────────────────────

        private void btnTabMaterial_Click(object sender, EventArgs e) => SetActiveTab(PopTab.Material);
        private void btnTabBomSpec_Click(object sender, EventArgs e) => SetActiveTab(PopTab.BomSpec);
        private void btnTabInspection_Click(object sender, EventArgs e) => SetActiveTab(PopTab.Inspection);
        private void btnTabEquipment_Click(object sender, EventArgs e) => SetActiveTab(PopTab.Equipment);

        /// <summary>
        /// 탭을 바꾼다 — 그리드 하나만 보이게 하고, 그 탭에서만 쓰는 도구(수동 렌치/결과 저장)를 함께 켠다.
        /// </summary>
        private void SetActiveTab(PopTab tab)
        {
            _activeTab = tab;

            gridMaterial.Visible = tab == PopTab.Material;
            gridBomSpec.Visible = tab == PopTab.BomSpec;
            gridInspection.Visible = tab == PopTab.Inspection;
            gridEquipment.Visible = tab == PopTab.Equipment;

            chkManualWrench.Visible = tab == PopTab.BomSpec;
            btnSaveResults.Visible = tab == PopTab.BomSpec || tab == PopTab.Inspection;

            StyleTabButtons();
        }

        private void StyleTabButtons()
        {
            StyleTabButton(btnTabMaterial, _activeTab == PopTab.Material);
            StyleTabButton(btnTabBomSpec, _activeTab == PopTab.BomSpec);
            StyleTabButton(btnTabInspection, _activeTab == PopTab.Inspection);
            StyleTabButton(btnTabEquipment, _activeTab == PopTab.Equipment);
        }

        /// <summary>선택된 탭은 연한 초록 칩, 나머지는 회색 외곽선 버튼으로 그린다.</summary>
        private static void StyleTabButton(SimpleButton button, bool active)
        {
            if (!active)
            {
                AppTheme.QuietButton(button, AppTheme.Slate);
                return;
            }

            AppTheme.QuietButton(button, AppTheme.Emerald);
            button.Appearance.BackColor = AppTheme.Blend(AppTheme.Emerald, AppTheme.Card, 0.18f);
            button.Appearance.Options.UseBackColor = true;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  탭 1 — 투입자재 시리얼번호 입력
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>"시리얼번호 입력상태" 칸을 클릭하면 &lt;투입자재 정보 입력&gt; 팝업을 연다.</summary>
        private async void GridViewMaterial_RowCellClick(object sender, RowCellClickEventArgs e)
        {
            if (e.Column == null || e.Column.FieldName != "SerialInputStateText")
                return;

            var material = gridViewMaterial.GetRow(e.RowHandle) as PopInputMaterialDto;
            if (material == null)
                return;

            if (!material.SerialCheckRequired)
            {
                AppMessageBox.Info(this, Strings.Pop_SerialNotRequired, Strings.PopSerial_Title);
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

            await SaveMaterialSerialsAsync(material);
        }

        private async Task SaveMaterialSerialsAsync(PopInputMaterialDto material)
        {
            try
            {
                if (!UseMock)
                {
                    var request = new PopMaterialSerialSaveRequest
                    {
                        WorkOrderNo = _key.WorkOrderNo,
                        SerialNo = _key.SerialNo,
                        RoutingGroup = _key.RoutingGroup,
                        RoutingOperation = _key.RoutingOperation,
                        PageOperation = _key.PageOperation,
                        MaterialCode = material.MaterialCode,
                        SerialNumbers = material.SerialNumbers
                    };

                    await _context.ApiClients.PopApiClient.SaveMaterialSerialsAsync(request);
                }

                AppMessageBox.Success(this, Strings.PopSerial_Saved, Strings.PopSerial_Title);
            }
            catch (ApiException ex)
            {
                AppMessageBox.Warn(this, ex.Message, Strings.PopSerial_Title);
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  탭 2 — BOM 스팩 / 수동 렌치
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>수동 렌치를 켜면 "결과값 입력" 컬럼([입력] 버튼)이 나타난다.</summary>
        private void chkManualWrench_CheckedChanged(object sender, EventArgs e)
        {
            SetResultInputColumnVisible(chkManualWrench.Checked);
        }

        private void SetResultInputColumnVisible(bool visible)
        {
            var column = gridViewBomSpec.Columns.ColumnByFieldName(ResultInputField);
            if (column == null)
                return;

            column.Visible = visible;
            if (visible)
                column.VisibleIndex = gridViewBomSpec.VisibleColumns.Count;
        }

        /// <summary>[입력] 버튼 → &lt;토크 결과값 입력&gt; 팝업에서 받은 값을 그 줄의 토크 결과값에 넣는다.</summary>
        private void ResultInput_ButtonClick(object sender, ButtonPressedEventArgs e)
        {
            if (!chkManualWrench.Checked)
            {
                AppMessageBox.Info(this, Strings.Pop_ManualWrenchOnly, Strings.PopTorque_Title);
                return;
            }

            var spec = gridViewBomSpec.GetFocusedRow() as PopBomSpecDto;
            if (spec == null)
                return;

            using (var form = new TorqueInputForm())
            {
                form.Initialize(spec);
                form.CenterOn(this);
                if (form.ShowDialog(this) != DialogResult.OK)
                    return;

                spec.TorqueResult = form.Value;
                spec.IsManualInput = true;
            }

            gridViewBomSpec.RefreshData();
            AppLog.Info($"[POP] 수동 렌치 입력 - {spec.BomSpecName}/{spec.InputPosition} = {spec.TorqueResult}");
        }

        /// <summary>
        /// 연결 장비에서 올라온 토크 값을 아직 비어 있는 첫 줄에 채운다.
        /// 수동 렌치를 켠 동안에는 작업자 입력이 우선이므로 자동 반영하지 않는다.
        /// </summary>
        private void ApplyTorqueFromEquipment(string equipmentId, decimal value)
        {
            if (chkManualWrench.Checked)
                return;

            var target = _bomSpecs.FirstOrDefault(spec => spec.TorqueResult == null);
            if (target == null)
            {
                AppLog.Info($"[POP] {Strings.Pop_TorqueNoPendingRow} (설비 {equipmentId}, 값 {value})");
                return;
            }

            target.TorqueResult = value;
            target.IsManualInput = false;
            gridViewBomSpec.RefreshData();

            AppLog.Info($"[POP] 설비 토크 결과값 수신 - {equipmentId} → {target.BomSpecName}/{target.InputPosition} = {value}");
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  결과 저장 (탭 2 / 탭 3)
        // ─────────────────────────────────────────────────────────────────────────

        private async void btnSaveResults_Click(object sender, EventArgs e)
        {
            gridViewBomSpec.CloseEditor();
            gridViewBomSpec.UpdateCurrentRow();
            gridViewInspection.CloseEditor();
            gridViewInspection.UpdateCurrentRow();

            // 저장은 서버 상태를 바꾸는 처리라 [취소]를 주지 않는다 —
            // 중간에 끊으면 무엇이 반영되고 무엇이 안 됐는지 화면에서 알 수 없다.
            using (BusyOverlay.Show(this))
            {
                try
                {
                    btnSaveResults.Enabled = false;

                    if (!UseMock)
                    {
                        if (_activeTab == PopTab.BomSpec)
                            await _context.ApiClients.PopApiClient.SaveBomSpecResultsAsync(_key, _bomSpecs);
                        else if (_activeTab == PopTab.Inspection)
                            await _context.ApiClients.PopApiClient.SaveInspectionResultsAsync(_key, _inspections);
                    }

                    if (IsDisposed || Disposing) return;   // 저장 중 탭을 닫았다
                    AppMessageBox.Success(this, Strings.Pop_SavedResult, Strings.Pop_Title);
                }
                catch (ApiException ex)
                {
                    if (IsDisposed || Disposing) return;
                    AppMessageBox.Warn(this, ex.Message, Strings.Pop_Title);
                }
                finally
                {
                    if (!IsDisposed && !Disposing)
                        btnSaveResults.Enabled = true;
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  아코디언 — 우측 공정 정보 / 하단 탭을 접어 도면을 크게 본다
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>접었을 때 남기는 우측 패널 폭. 손잡이 버튼만 보일 만큼만 남긴다.</summary>
        private const int InfoCollapsedWidth = 40;

        /// <summary>펼쳤을 때 우측 패널 폭(디자이너의 infoCard 폭과 같게 둘 것).</summary>
        private const int InfoExpandedWidth = 560;

        /// <summary>펼쳤을 때 하단 영역 높이(디자이너의 bottomCard 높이와 같게 둘 것).</summary>
        private const int BottomExpandedHeight = 320;

        private bool _infoExpanded = true;
        private bool _bottomExpanded = true;

        private void btnInfoAccordion_Click(object sender, EventArgs e) => SetInfoExpanded(!_infoExpanded);

        private void btnTabsAccordion_Click(object sender, EventArgs e) => SetBottomExpanded(!_bottomExpanded);

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
                bannerPanel.Visible = expanded;

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

        /// <summary>
        /// 하단 탭 영역을 접었다 편다. 접어도 <b>탭 줄(tabHeaderPanel)은 그대로 남긴다</b> —
        /// 어떤 탭을 보고 있었는지 유지되고, 다시 펴는 손잡이도 그 줄에 있다.
        /// </summary>
        private void SetBottomExpanded(bool expanded)
        {
            _bottomExpanded = expanded;

            SuspendLayout();
            try
            {
                tabBodyPanel.Visible = expanded;

                // 접었을 때 높이 = 탭 줄 + 카드 안쪽 여백(위/아래).
                bottomCard.Height = expanded
                    ? BottomExpandedHeight
                    : tabHeaderPanel.Height + bottomCard.Padding.Vertical + 4;
            }
            finally
            {
                ResumeLayout(true);
            }

            Invalidate(true);
            RefreshAccordionButtons();
        }

        /// <summary>
        /// 손잡이 버튼의 화살표와 색을 지금 상태에 맞춘다.
        /// 화살표는 <b>누르면 일어날 일</b>을 가리킨다 — 펼쳐져 있으면 접는 방향(▶ / ▼)이다.
        /// </summary>
        private void RefreshAccordionButtons()
        {
            StyleAccordionHandle(btnInfoAccordion, _infoExpanded ? "›" : "‹", _infoExpanded, AppTheme.Violet);
            StyleAccordionHandle(btnTabsAccordion, _bottomExpanded ? "⌄" : "⌃", _bottomExpanded, AppTheme.Cyan);
        }

        /// <summary>
        /// 손잡이 모양 — 시안처럼 가장자리에 붙은 <b>얇은 띠</b>다.
        /// 화살표는 누르면 일어날 일을 가리킨다(펼쳐져 있으면 접는 방향).
        /// 글자가 화살표 하나뿐이라 본문 글꼴은 커서 잘리므로 작은 글꼴을 쓴다.
        /// </summary>
        private static void StyleAccordionHandle(SimpleButton button, string glyph, bool expanded, Color accent)
        {
            AppTheme.QuietButton(button, accent);
            button.Text = glyph;
            button.Appearance.Font = AppTheme.SmallFont;
            button.Appearance.Options.UseFont = true;

            // 접혀 있을 때는 "여기 뭔가 숨어 있다"가 드러나야 하므로 포인트 색으로 남겨 둔다.
            if (!expanded)
            {
                button.Appearance.ForeColor = accent;
                button.Appearance.BorderColor = accent;
                button.Appearance.Options.UseForeColor = true;
                button.Appearance.Options.UseBorderColor = true;
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  상단 공정 제어 버튼
        // ─────────────────────────────────────────────────────────────────────────

        private async void tileStartProcess_ItemClick(object sender, TileItemEventArgs e) =>
            await RequestProcessActionAsync(PopProcessAction.Start, Strings.Pop_ConfirmStart);

        private async void tileStopProcess_ItemClick(object sender, TileItemEventArgs e) =>
            await RequestProcessActionAsync(PopProcessAction.Stop, Strings.Pop_ConfirmStop);

        private async void tileSkipProcess_ItemClick(object sender, TileItemEventArgs e) =>
            await RequestProcessActionAsync(PopProcessAction.Skip, Strings.Pop_ConfirmSkip);

        private async void tilePrevProcess_ItemClick(object sender, TileItemEventArgs e) =>
            await RequestProcessActionAsync(PopProcessAction.Previous, Strings.Pop_ConfirmPrevious);

        private async void tileNextProcess_ItemClick(object sender, TileItemEventArgs e) =>
            await RequestProcessActionAsync(PopProcessAction.Next, Strings.Pop_ConfirmNext);

        /// <summary>
        /// 공정 상태 전이를 서버에 요청한다. 전이 가능 여부는 서버가 판단하고,
        /// 클라이언트는 응답으로 받은 &lt;진행 중 공정 정보&gt;로 화면을 다시 그린다.
        /// 목업 모드에서는 서버 대신 화면만 다시 읽어 흐름을 확인할 수 있게 한다.
        /// </summary>
        private async Task RequestProcessActionAsync(PopProcessAction action, string confirmMessage)
        {
            if (_key == null)
            {
                AppMessageBox.Warn(this, Strings.Pop_NoTargetProcess, Strings.Pop_Title);
                return;
            }

            if (!AppMessageBox.Confirm(this, confirmMessage, Strings.Pop_Title))
                return;

            try
            {
                if (UseMock)
                {
                    AppLog.Info($"[POP] (목업) 공정 상태 전이 요청 - {action}");
                    await LoadAllAsync();
                    return;
                }

                _info = await _context.ApiClients.PopApiClient.ChangeProcessStateAsync(_key, action);
                BindProcessInfo();

                // 이전/다음 공정으로 옮겨 갔다면 하단 탭 내용도 그 공정 것으로 바뀌어야 한다.
                if (action == PopProcessAction.Previous || action == PopProcessAction.Next)
                {
                    ApplyKeyFromInfo();
                    await LoadAllAsync();
                }
            }
            catch (ApiException ex)
            {
                AppMessageBox.Warn(this, ex.Message, Strings.Pop_Title);
            }
        }

        /// <summary>서버가 돌려준 공정 정보로 조회 키를 갱신한다(이전/다음 공정 이동 후).</summary>
        private void ApplyKeyFromInfo()
        {
            if (_info == null)
                return;

            _key.RoutingGroup = _info.RoutingGroup;
            _key.RoutingOperation = _info.RoutingOperation;
            _key.PageOperation = _info.PageOperation;
        }

        /// <summary>
        /// [공정 목록] — 화면에 보이는 작업지시번호를 키로 공정(라우팅) 목록을 조회해
        /// 기존 &lt;작업 진입 확인&gt; 팝업을 그대로 띄우고, [이동]으로 고른 공정으로 화면을 다시 읽는다.
        /// </summary>
        private async void tileProcessList_ItemClick(object sender, TileItemEventArgs e)
        {
            if (_key == null)
            {
                AppMessageBox.Warn(this, Strings.Pop_NoTargetProcess, Strings.Pop_Title);
                return;
            }

            List<RoutingProcessDto> processes;
            try
            {
                tileProcessList.Enabled = false;
                processes = await FetchRoutingProcessesAsync();
            }
            catch (ApiException ex)
            {
                AppMessageBox.Warn(this, ex.Message, Strings.WorkEntryConfirm_Title);
                return;
            }
            finally
            {
                tileProcessList.Enabled = true;
            }

            if (processes.Count == 0)
            {
                AppMessageBox.Warn(this, Strings.Pop_NoProcessList, Strings.WorkEntryConfirm_Title);
                return;
            }

            RoutingProcessDto selected;
            using (var confirm = new WorkEntryConfirmForm())
            {
                confirm.Initialize(ResolveWorkOrder(), processes);
                confirm.CenterOn(this);
                if (confirm.ShowDialog(this) != DialogResult.OK)
                    return;

                selected = confirm.SelectedProcess;
            }

            if (selected == null)
                return;

            _key.RoutingGroup = selected.RoutingGroup;
            _key.RoutingOperation = selected.RoutingOperation;
            _key.PageOperation = selected.PageOperation;

            await LoadAllAsync();
            AppMessageBox.Info(this, Strings.Pop_ProcessMoved, Strings.Pop_Title);
        }

        private async Task<List<RoutingProcessDto>> FetchRoutingProcessesAsync()
        {
            if (UseMock)
            {
                await Task.Yield();
                return MockWorkOrderData.GetRoutingProcesses(_key.WorkOrderNo, _key.SerialNo);
            }

            return await _context.ApiClients.WorkOrderApiClient.GetRoutingProcessesAsync(_key.WorkOrderNo, _key.SerialNo)
                   ?? new List<RoutingProcessDto>();
        }

        /// <summary>
        /// &lt;작업 진입 확인&gt; 팝업 제목에 쓸 작업지시.
        /// 진입 화면을 거쳐 왔으면 그 값을 쓰고, 아니면 화면에 보이는 작업지시번호로 최소한만 채운다.
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

        /// <summary>
        /// [관리자 알람] — &lt;관리자 알람 유형 선택&gt; 팝업을 띄우고, 고른 내용에
        /// 현재 공정/작업자 정보를 붙여 서버로 보낸다.
        /// 대상(결품 자재/이상 장비)은 지금 화면에 떠 있는 투입자재·연결 장비 목록에서 고르게 한다.
        /// </summary>
        private async void tileAdminAlarm_ItemClick(object sender, TileItemEventArgs e)
        {
            List<AdminAlarmTypeDto> alarmTypes;
            try
            {
                tileAdminAlarm.Enabled = false;
                alarmTypes = await FetchAdminAlarmTypesAsync();
            }
            catch (ApiException ex)
            {
                AppMessageBox.Warn(this, ex.Message, Strings.AdminAlarm_Title);
                return;
            }
            finally
            {
                tileAdminAlarm.Enabled = true;
            }

            if (alarmTypes.Count == 0)
            {
                AppMessageBox.Warn(this, Strings.AdminAlarm_NoTypes, Strings.AdminAlarm_Title);
                return;
            }

            PopAdminAlarmRequest request;
            using (var form = new AdminAlarmForm())
            {
                form.Initialize(alarmTypes, _materials, _equipments);
                form.CenterOn(this);
                if (form.ShowDialog(this) != DialogResult.OK)
                    return;

                request = form.Request;
            }

            if (request == null)
                return;

            request.Key = _key;
            request.UserId = _context.CurrentUser?.UserId;

            try
            {
                if (!UseMock)
                    await _context.ApiClients.PopApiClient.NotifyAdministratorAsync(request);

                AppLog.Info($"[POP] 관리자 알람 전송 - {request.AlarmTypeName}/{request.TargetCode} : {request.Remark}");
                AppMessageBox.Success(this, Strings.Pop_AdminAlarmSent, Strings.AdminAlarm_Title);
            }
            catch (ApiException ex)
            {
                AppMessageBox.Warn(this, ex.Message, Strings.AdminAlarm_Title);
            }
        }

        private async Task<List<AdminAlarmTypeDto>> FetchAdminAlarmTypesAsync()
        {
            if (UseMock)
            {
                await Task.Yield();
                return MockPopData.GetAdminAlarmTypes();
            }

            return await _context.ApiClients.PopApiClient.GetAdminAlarmTypesAsync()
                   ?? new List<AdminAlarmTypeDto>();
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  탭 4 — 연결 장비 / 설비 이벤트
        // ─────────────────────────────────────────────────────────────────────────

        private void WireEquipmentEvents()
        {
            _context.EquipmentManager.DataReceived += EquipmentManager_DataReceived;
            _context.EquipmentManager.ConnectionChanged += EquipmentManager_ConnectionChanged;
            _context.EquipmentManager.ErrorOccurred += EquipmentManager_ErrorOccurred;
        }

        private void UnwireEquipmentEvents()
        {
            _context.EquipmentManager.DataReceived -= EquipmentManager_DataReceived;
            _context.EquipmentManager.ConnectionChanged -= EquipmentManager_ConnectionChanged;
            _context.EquipmentManager.ErrorOccurred -= EquipmentManager_ErrorOccurred;
        }

        /// <summary>
        /// 설비에서 올라온 프레임 처리 — [연결 장비] 탭 상태를 갱신하고,
        /// 토크 값으로 보이는 레지스터를 [BOM 스팩] 탭의 토크 결과값으로 옮긴다.
        /// </summary>
        private void EquipmentManager_DataReceived(object sender, EquipmentDataReceivedEventArgs e)
        {
            InvokeIfRequired(() =>
            {
                var registers = e.Frame.GetRegisters();
                AppLog.Info($"[POP] [{e.EquipmentId}/{e.Channel}] 프레임 수신: {e.Frame}");

                // TODO(장비 레지스터 맵 확정 전 임시): 첫 레지스터를 1/100 스케일의 토크 값으로 본다.
                //      실제 맵을 받으면 어떤 주소가 토크 결과값인지 App.config의 RegisterMap으로 지정할 것.
                if (registers.Length > 0)
                    ApplyTorqueFromEquipment(e.EquipmentId, registers[0] / 100m);

                var status = new EquipmentStatusDto
                {
                    EquipmentId = e.EquipmentId,
                    RunState = registers.Length > 0 ? registers[0].ToString() : $"FC0x{e.Frame.FunctionCode:X2}",
                    ReportedAtUtc = DateTime.UtcNow
                };
                _ = _context.ApiClients.EquipmentStatusApiClient.ReportStatusAsync(status);
            });
        }

        private void EquipmentManager_ConnectionChanged(object sender, EquipmentConnectionChangedEventArgs e)
        {
            InvokeIfRequired(() =>
            {
                AppLog.Info($"[POP] [{e.EquipmentId}/{e.Channel}] 연결상태: {e.State} {e.Message}");
                RefreshEquipmentStates();
            });
        }

        private void EquipmentManager_ErrorOccurred(object sender, EquipmentErrorEventArgs e)
        {
            InvokeIfRequired(() =>
            {
                AppLog.Info($"[POP] [{e.EquipmentId}/{e.Channel}] 오류: {e.Exception.Message}");
                RefreshEquipmentStates();
            });
        }

        /// <summary>
        /// [연결 장비] 탭의 통신상태/장비상태를 현재 Modbus 연결 상태로 덮어쓴다.
        /// 장비코드(EquipmentCode)가 App.config의 EquipmentId와 같아야 연결 상태가 붙는다.
        /// 표준/이벤트 두 Connection 중 하나라도 연결돼 있으면 "연결 성공"으로 본다.
        /// </summary>
        private void RefreshEquipmentStates()
        {
            if (_context?.EquipmentManager == null)
                return;

            var manager = _context.EquipmentManager;

            foreach (var equipment in _equipments)
            {
                var id = equipment.EquipmentCode;
                var hasStandard = manager.HasChannel(id, EquipmentChannel.Standard);
                var hasEvent = manager.HasChannel(id, EquipmentChannel.Event);

                if (!hasStandard && !hasEvent)
                {
                    equipment.CommunicationState = Strings.Pop_CommUnused;
                    equipment.EquipmentState = Strings.Pop_EquipmentUnusable;
                    continue;
                }

                var standard = hasStandard ? manager.GetState(id, EquipmentChannel.Standard) : EquipmentConnectionState.Disconnected;
                var eventState = hasEvent ? manager.GetState(id, EquipmentChannel.Event) : EquipmentConnectionState.Disconnected;

                var connected = standard == EquipmentConnectionState.Connected || eventState == EquipmentConnectionState.Connected;
                var connecting = standard == EquipmentConnectionState.Connecting || eventState == EquipmentConnectionState.Connecting;

                if (connected)
                {
                    equipment.CommunicationState = Strings.Pop_CommConnected;
                    equipment.EquipmentState = Strings.Pop_EquipmentUsable;
                }
                else if (connecting)
                {
                    equipment.CommunicationState = Strings.Pop_CommConnecting;
                    equipment.EquipmentState = Strings.Pop_EquipmentUnusable;
                }
                else
                {
                    equipment.CommunicationState = Strings.Pop_CommFailed;
                    equipment.EquipmentState = Strings.Pop_EquipmentUnusable;
                }
            }

            gridViewEquipment.RefreshData();
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  기타 (시계 / 도면 자리표시자 / 유틸)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>상단 우측 현재시각을 1초마다 갱신한다.</summary>
        private void StartClock()
        {
            lblCurrentTime.Text = DateTime.Now.ToString("HH:mm:ss");

            _clock = new System.Windows.Forms.Timer();
            _clock.Interval = 1000;
            _clock.Tick += Clock_Tick;
            _clock.Start();
        }

        private void Clock_Tick(object sender, EventArgs e)
        {
            lblCurrentTime.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        /// <summary>
        /// 도면이 떠 있지 않을 때 보이는 자리표시자 — 화면 설계서와 같은 대각선 상자를 그리고,
        /// 가운데에 현재 상태(<see cref="_viewerMessage"/>: 불러오는 중 / 도면 없음 / 실패 사유)를 적는다.
        /// PDF가 열리면 <c>pdfViewer</c>가 이 면을 덮으므로 이 그림은 보이지 않는다.
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
                var title = Strings.Pop_Drawing;
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

        /// <summary>진입 화면에서 받은 작업 대상을 조회 키로 바꾼다. 대상이 없으면 null.</summary>
        private static PopProcessKey BuildKey(WorkEntryContext entry)
        {
            if (entry?.WorkOrder == null || entry.Process == null)
                return null;

            var key = new PopProcessKey();
            key.WorkOrderNo = entry.WorkOrder.WorkOrderNo;
            key.SerialNo = entry.WorkOrder.SerialNo;
            key.RoutingGroup = entry.Process.RoutingGroup;
            key.RoutingOperation = entry.Process.RoutingOperation;
            key.PageOperation = entry.Process.PageOperation;
            return key;
        }

        /// <summary>상단에 표시할 작업자 — 프로필 이름이 없으면 로그인 아이디를 쓴다.</summary>
        private string ResolveWorkerName()
        {
            var user = _context?.CurrentUser;
            if (user == null)
                return string.Empty;

            return string.IsNullOrWhiteSpace(user.UserName) ? user.UserId : user.UserName;
        }

        private static string FormatTime(DateTime? value) =>
            value?.ToString("HH:mm:ss") ?? string.Empty;

        private static string FormatDuration(TimeSpan? value) =>
            value?.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture) ?? string.Empty;

        private void PopScreen_Disposed(object sender, EventArgs e)
        {
            UnwireEquipmentEvents();
            CloseDrawing();

            if (_clock != null)
            {
                _clock.Stop();
                _clock.Tick -= Clock_Tick;
                _clock.Dispose();
                _clock = null;
            }
        }

        private void InvokeIfRequired(Action action)
        {
            if (IsDisposed) return;
            if (InvokeRequired) BeginInvoke(action);
            else action();
        }
    }

    /// <summary>POP 본 화면 하단의 탭.</summary>
    internal enum PopTab
    {
        Material,
        BomSpec,
        Inspection,
        Equipment
    }
}
