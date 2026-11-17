using DevExpress.Export;
using DevExpress.Export.Xl;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraPrinting;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Columns;
using SEOJIN_TMES.Client.ApiClient;
using SEOJIN_TMES.Client.Common.Configuration;
using SEOJIN_TMES.Client.Common.Errors;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Core;
using SEOJIN_TMES.Client.Menu;
using SEOJIN_TMES.Client.Forms.Common;
using SEOJIN_TMES.Client.Theme;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SEOJIN_TMES.Client.Forms.WorkIntegratedStatus
{
    /// <summary>
    /// MEIS &gt; 작업통합 개요 &gt; 작업통합 현황 조회.
    ///
    /// 생산품목 시리얼번호 하나가 <b>거쳐 온 전 과정</b>을 한 트리로 펼쳐 본다:
    ///   품목 → 라우팅 그룹 → 라우팅 공정 → 페이지 공정 → 투입자재 → 스팩(토크/앵글 값)
    ///
    /// 값 컬럼(토크·앵글)은 스팩 줄에서만 채워지고, 상위 줄은 계층을 잡아 주는 역할만 한다.
    /// 조회·집계·판정은 전부 서버가 하고 화면은 결과 표시와 엑셀 내보내기만 담당한다.
    ///
    /// VS 디자이너 호환을 위해 매개변수 없는 생성자만 두고, 실제 의존성은 Initialize()로 주입한다.
    /// </summary>
    public partial class WorkIntegratedStatusScreen : XtraUserControl, IThemedScreen
    {
        private AppSettings _appSet;
        private ApiClientFactory _apiClients;
        private UserSession _user;
        /// <summary>현재 트리에 물려 있는 결과. 엑셀 내보내기 전에 비어 있는지 판단하는 데 쓴다.</summary>
        private List<WorkIntegratedDto> _workIntegrateds = new List<WorkIntegratedDto>();
        private List<WorkIntegratedNode> _node = new List<WorkIntegratedNode>();

        public WorkIntegratedStatusScreen()
        {
            InitializeComponent();
        }

        public void Init()
        {
            _appSet = Global.Instance.Settings.AppSet;
            _apiClients = Global.Instance.Settings.ApiClients;
            _user = Global.Instance.Settings.CurrentUser;

            lblOverviewTitle.Text = Language.WorkIntegrated_OverviewTitle;
            lblFilterTitle.Text = Language.ProductionStatus_FilterTitle;
            lblSerialNo.Text = Language.WorkEntry_SerialNo + "(*)";
            btnSearch.Text = Language.Common_Search;
            btnExcelExport.Text = Language.WorkIntegrated_ExcelExport;

            ApplyTheme();
            ConfigureTree();

            // 바코드 스캐너처럼 Enter로 끝나는 입력을 그대로 조회로 연결한다.
            txtSerialNo.KeyDown += SerialNo_KeyDown;

            SetPermission();
        }

        /// <summary>서버가 내려준 버튼 권한(POP03). 받기 전에는 모두 없음이라 [조회] · [엑셀 내보내기]가 숨어 있다.</summary>
        private ProgramPermissionDto _permission = ProgramPermissionDto.None();

        /// <summary>버튼 권한을 받아 [조회](srchAuthYn) · [엑셀 내보내기](prnotAuthYn)를 보이거나 숨긴다.</summary>
        private async void SetPermission()
        {
            _permission = ProgramPermissionDto.None();
            MenuPermission.SetVisible(false, btnSearch, btnExcelExport);

            var permission = await MenuPermission.Get(MenuCodes.WorkIntegratedStatus);
            if (IsDisposed || Disposing)
                return;

            _permission = permission;
            MenuPermission.SetVisible(_permission.CanSearch, btnSearch);
            MenuPermission.SetVisible(_permission.CanPrint, btnExcelExport);
        }

        /// <summary>현재 테마 색을 화면에 입힌다(포인트 컬러: Violet).</summary>
        public void ApplyTheme()
        {
            BackColor = AppTheme.Surface;
            spacer.BackColor = AppTheme.Surface;

            AppTheme.CardPanel(topCard);
            AppTheme.CardPanel(overviewCard);
            AppTheme.SectionTitle(lblFilterTitle, AppTheme.Violet);
            AppTheme.SectionTitle(lblOverviewTitle, AppTheme.Violet);

            AppTheme.BodyText(lblSerialNo);
            AppTheme.CodeEditor(txtSerialNo);

            AppTheme.StandardButton(btnSearch);
            AppTheme.StandardButton(btnExcelExport);

            AppTheme.Tree(treeOverview, AppTheme.Violet);
        }

        /// <summary>
        /// 트리 컬럼 구성. TreeList는 데이터 형식에서 컬럼을 자동 생성하면 DTO의 모든 속성이 나오므로
        /// (NodeId/ParentNodeId/NodeKind 같은 내부 필드까지) 필요한 컬럼만 직접 만들어 붙인다.
        /// </summary>
        private void ConfigureTree()
        {
            treeOverview.BeginUpdate();
            try
            {
                treeOverview.Columns.Clear();
                treeOverview.OptionsBehavior.PopulateServiceColumns = false;
                treeOverview.OptionsView.AutoWidth = false;
                //treeOverview.ChildListFieldName = nameof(WorkIntegratedNode.Children);

                treeOverview.OptionsBehavior.Editable = false;
                treeOverview.OptionsView.ShowIndicator = false;
                GridColumnGuard.Apply(treeOverview);   // 컬럼 지우기만 막는다
                // 코드·설명 열은 고정 폭, 값 컬럼은 130 고정 폭 + 가로 스크롤로 둔다
                // (앵글 컬럼까지 억지로 줄여 넣지 않고 화면 설계서처럼 옆으로 밀어 본다).
                AddColumn("Code", Language.Col_NodeCode, 220, HorzAlignment.Near);
                AddColumn("Name", Language.Col_NodeDescription, 250, HorzAlignment.Near);
                // 투입자재의 품목코드/시리얼번호(서버 wholInputMatlCd) — 서버가 만든 문자열을 그대로 보인다. 없으면 ".."
                AddColumn("Serial", Language.Col_WholeInputMaterialCode, 220, HorzAlignment.Near);

                AddValueColumn("UsageQty", Language.Col_WorkQty, 130);
                AddValueColumn("Unit", Language.Col_Unit, 130);
                AddValueColumn("TorqueResult", Language.Col_TorqueResult, 130);
                AddValueColumn("TorqueTarget", Language.Col_TorqueTarget, 130);
                AddValueColumn("TorqueLower", Language.Col_TorqueLower, 130);
                AddValueColumn("TorqueUpper", Language.Col_TorqueMax, 130);
                AddValueColumn("AngleTarget", Language.Col_AngleTarget, 130);
                AddValueColumn("AngleLower", Language.Col_AngleLower, 130);
                AddValueColumn("AngleUpper", Language.Col_AngleUpper, 130);
                AddValueColumn("AngleResult", Language.Col_AngleResult, 130);
            }
            finally
            {
                treeOverview.EndUpdate();
            }

            // 값이 없는 줄(계층만 잡아 주는 상위 줄)은 빈칸 대신 ".."로 표시한다 — 목업과 같은 표기.
            treeOverview.CustomColumnDisplayText += TreeOverview_CustomColumnDisplayText;
        }

        private TreeListColumn AddColumn(string fieldName, string caption, int width, HorzAlignment alignment)
        {
            var column = treeOverview.Columns.Add();
            column.FieldName = fieldName;
            column.Caption = caption;
            column.Width = width;
            column.Visible = true;
            column.VisibleIndex = treeOverview.Columns.Count - 1;
            column.OptionsColumn.AllowEdit = false;
            column.OptionsColumn.AllowSort = false;
            column.AppearanceCell.TextOptions.HAlignment = alignment;
            column.AppearanceCell.Options.UseTextOptions = true;
            return column;
        }

        /// <summary>토크/앵글 값 컬럼 — 소수 세 자리 고정으로 자릿수를 맞춰 읽기 쉽게 한다(6.180 / 5.400).</summary>
        private void AddValueColumn(string fieldName, string caption, int width)
        {
            var column = AddColumn(fieldName, caption, width, HorzAlignment.Center);
            column.Format.FormatType = FormatType.Numeric;
            column.Format.FormatString = "0.00##";
        }

        /// <summary>
        /// 값이 비어 있는 칸의 표기.
        /// 항목 설명은 서버가 안 내려줄 수 있고, 값 컬럼은 스팩 줄에서만 채워지므로
        /// 두 경우 모두 목업과 같이 ".."로 보여 준다(빈칸이면 어느 열인지 눈으로 따라가기 어렵다).
        /// </summary>
        private void TreeOverview_CustomColumnDisplayText(object sender, DevExpress.XtraTreeList.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Value == null || (e.Value as string) == string.Empty)
                e.DisplayText = EmptyValueText;
        }

        /// <summary>값이 없는 칸의 표기. 화면과 엑셀이 같은 표기를 쓰도록 한 곳에 둔다.</summary>
        private const string EmptyValueText = "..";

        private void SerialNo_KeyDown(object sender, KeyEventArgs e)
        {
            // 조회 권한이 없으면 [조회]를 숨겼으므로 Enter로도 조회하지 않는다.
            if (e.KeyCode != Keys.Enter || !_permission.CanSearch)
                return;

            e.Handled = true;
            _ = SearchAsync();
        }

        private async void btnSearch_Click(object sender, EventArgs e) => await SearchAsync();

        /// <summary>
        /// 생산품목 시리얼번호로 작업 통합 개요를 조회한다.
        /// 시리얼번호는 필수다 — 이 화면은 "한 개체가 어떻게 만들어졌는가"를 보는 화면이라
        /// 대상 없이 전체를 긁어오면 의미가 없다.
        /// </summary>
        private async Task SearchAsync()
        {
            var serialNo = txtSerialNo.Text?.Trim();
            if (string.IsNullOrEmpty(serialNo))
            {
                AppMessageBox.Warn(this, Language.WorkIntegrated_SerialRequired, Language.WorkIntegratedStatus_Title);
                txtSerialNo.Focus();
                return;
            }

            string corpCd = _user.Server; 

            using (var busy = BusyOverlay.Show(this, cancelable: true))
            {
                try
                {
                    btnSearch.Enabled = false;
                    _workIntegrateds = await FetchOverviewAsync(corpCd, serialNo, busy.Token);
                    _node = WorkIntegratedNode.BuildTree(_workIntegrateds);
                    if (IsDisposed || Disposing) return;   // 조회 중 탭을 닫았다

                    treeOverview.DataSource = _node;
                    treeOverview.ExpandAll();

                    if (_node.Count == 0)
                        AppMessageBox.Info(this, Language.WorkEntry_NoResult, Language.WorkIntegratedStatus_Title);
                }
                catch (OperationCanceledException)
                {
                    // 사용자가 [취소]를 눌렀다 — 오류가 아니므로 조용히 끝낸다.
                }
                catch (ApiException ex)
                {
                    if (IsDisposed || Disposing) return;
                    AppMessageBox.Warn(this, ex.Message, Language.WorkIntegratedStatus_Title);
                }
                finally
                {
                    if (!IsDisposed && !Disposing)
                        btnSearch.Enabled = true;
                }
            }
        }

        private async Task<List<WorkIntegratedDto>> FetchOverviewAsync(string corpCd, string serialNo, CancellationToken cancellationToken)
        {
            return await _apiClients.ProductionApiClient.GetWorkIntegratedOverviewAsync(corpCd, serialNo, cancellationToken)
                   ?? new List<WorkIntegratedDto>();
        }

        /// <summary>
        /// [엑셀 내보내기] — 화면에 보이는 트리를 그대로 xlsx로 저장한다(계층/컬럼 구성 유지).
        /// 저장 위치는 사용자가 고르며, 기본 파일명에 시리얼번호와 시각을 넣어 덮어쓰기를 피한다.
        /// </summary>
        private void btnExcelExport_Click(object sender, EventArgs e)
        {
            if (_workIntegrateds.Count == 0)
            {
                AppMessageBox.Info(this, Language.WorkIntegrated_ExportEmpty, Language.WorkIntegrated_ExcelExport);
                return;
            }

            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "Excel 통합 문서 (*.xlsx)|*.xlsx";
                dialog.FileName = $"작업통합개요_{txtSerialNo.Text?.Trim()}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                try
                {
                    ExportOverviewToXlsx(dialog.FileName);
                    AppLog.Info($"[작업통합] 엑셀 내보내기 완료 - {dialog.FileName}");
                    AppMessageBox.Success(this, Language.WorkIntegrated_ExportDone, Language.WorkIntegrated_ExcelExport);
                }
                catch (Exception ex)
                {
                    // 파일이 열려 있거나 쓰기 권한이 없는 경우 등 — 화면은 계속 쓸 수 있어야 한다.
                    AppLog.Info($"[작업통합] 엑셀 내보내기 실패 - {ex.Message}");
                    AppMessageBox.Warn(this,
                        ErrorText.WithCode($"{Language.WorkIntegrated_ExportFailed}\n{ex.Message}", ErrorText.CodeOf(ex, ErrorCodes.FileAccessFailed)),
                        Language.WorkIntegrated_ExcelExport);
                }
            }
        }

        // ────────────────────────── 엑셀 내보내기 (DevExpress DataAware) ──────────────────────────

        /// <summary>
        /// 트리를 DevExpress의 데이터 내보내기(<see cref="ExportType.DataAware"/>)로 xlsx에 쓴다.
        ///
        /// 이 방식을 쓰는 이유:
        ///   · 노드 계층이 그대로 <b>엑셀 행 그룹(접기/펼치기)</b>이 되고, 부모 행이 자식 위에 온다.
        ///   · 사용자가 화면에서 바꾼 <b>열 순서 · 숨김 · 폭 · 노드 접힘</b>이 그대로 따라간다.
        ///   · WinForms 라이선스에 포함된 기능이라 평가판 경고 시트가 붙지 않는다
        ///     (경고가 붙는 것은 Office File API·DevExpress.Export.Xl을 직접 쓸 때뿐 — 실측).
        /// 숫자는 숫자 칸으로, 열 서식(0.00##)도 함께 넘어간다.
        ///
        /// 예전 직접 생성기(Export/XlsxOutlineWriter.cs)는 빌드에서 빼고 파일만 남겨 두었다.
        /// </summary>
        private void ExportOverviewToXlsx(string fileName)
        {
            var options = new XlsxExportOptionsEx
            {
                ExportType = ExportType.DataAware,
                SheetName = Language.WorkIntegrated_OverviewTitle,
                // 화면에서 정렬을 막아 두었으므로(AllowSort=false) 엑셀에도 자동 필터 단추를 달지 않는다.
                AllowSortingAndFiltering = DefaultBoolean.False
            };
            options.CustomizeCell += ExportOptions_CustomizeCell;

            treeOverview.ExportToXlsx(fileName, options);
            AppLog.Info($"[작업통합] 엑셀 행 수 - {treeOverview.AllNodesCount}/{_workIntegrateds.Count}");
        }

        /// <summary>
        /// 내보내는 칸 하나하나의 모양.
        ///   · 제목행: 굵게 · 연보라 배경(<see cref="AppTheme.ExportHeaderBack"/>, 다크 테마와 무관한 고정색) · 가운데
        ///   · 값이 없는 칸: 화면(<see cref="TreeOverview_CustomColumnDisplayText"/>)과 같이 ".."
        ///     — DataAware는 표시 문구가 아니라 값을 내보내므로 화면의 ".."가 저절로 따라오지 않는다.
        /// <c>Handled = true</c>면 칸의 기본 서식 대신 여기서 준 서식만 쓰이므로, ".." 칸은 열 정렬을 다시 지정한다.
        /// </summary>
        private void ExportOptions_CustomizeCell(CustomizeCellEventArgs e)
        {
            if (e.AreaType == SheetAreaType.Header)
            {
                e.Formatting.Font = new XlCellFont { Bold = true };
                e.Formatting.BackColor = AppTheme.ExportHeaderBack;
                e.Formatting.Alignment = new XlCellAlignment
                {
                    HorizontalAlignment = XlHorizontalAlignment.Center,
                    VerticalAlignment = XlVerticalAlignment.Center
                };
                e.Handled = true;
                return;
            }

            if (e.AreaType != SheetAreaType.DataArea || !IsEmptyValue(e.Value))
                return;

            var column = treeOverview.Columns[e.ColumnFieldName];
            var center = column != null && column.AppearanceCell.TextOptions.HAlignment == HorzAlignment.Center;

            e.Value = EmptyValueText;
            e.Formatting.Alignment = new XlCellAlignment
            {
                HorizontalAlignment = center ? XlHorizontalAlignment.Center : XlHorizontalAlignment.Left
            };
            e.Handled = true;
        }

        /// <summary>화면에서 ".."로 보이는 값과 같은 기준(null · DBNull · 빈 문자열).</summary>
        private bool IsEmptyValue(object value) =>
            value == null || value is DBNull || (value as string) == string.Empty;
    }
}
