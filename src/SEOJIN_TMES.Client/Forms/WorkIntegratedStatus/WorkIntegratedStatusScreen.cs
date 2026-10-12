using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraTreeList.Nodes;
using SEOJIN_TMES.Client.ApiClient;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Composition;
using SEOJIN_TMES.Client.Core;
using SEOJIN_TMES.Client.Export;
using SEOJIN_TMES.Client.Forms.Common;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Mock;
using SEOJIN_TMES.Client.Theme;
using SEOJIN_TMES.Client.Common.Configuration;

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
        /// <summary>현재 트리에 물려 있는 결과. 엑셀 내보내기 전에 비어 있는지 판단하는 데 쓴다.</summary>
        private List<WorkIntegratedNodeDto> _nodes = new List<WorkIntegratedNodeDto>();

        public WorkIntegratedStatusScreen()
        {
            InitializeComponent();
        }

        public void Init()
        {
            _appSet = Global.Instance.Settings.AppSet;
            _apiClients = Global.Instance.Settings.ApiClients;

            lblOverviewTitle.Text = Language.WorkIntegrated_OverviewTitle;
            lblFilterTitle.Text = Language.ProductionStatus_FilterTitle;
            lblSerialNo.Text = Language.WorkEntry_SerialNo + "(*)";
            btnSearch.Text = Language.Common_Search;
            btnExcelExport.Text = Language.WorkIntegrated_ExcelExport;

            ApplyTheme();
            ConfigureTree();

            // 바코드 스캐너처럼 Enter로 끝나는 입력을 그대로 조회로 연결한다.
            txtSerialNo.KeyDown += SerialNo_KeyDown;
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

            btnSearch.Icon = AppIcons.Search;
            btnExcelExport.Icon = AppIcons.Clipboard;

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

                // 폭 합계(220 + 250 + 130*8 = 1510)를 화면 기본 폭에 맞춰, 값 컬럼은 고정 폭 + 가로 스크롤로 둔다
                // (앵글 컬럼까지 억지로 줄여 넣지 않고 화면 설계서처럼 옆으로 밀어 본다).
                AddColumn("NodeCode", Language.Col_NodeCode, 220, HorzAlignment.Near);
                AddColumn("NodeDescription", Language.Col_NodeDescription, 250, HorzAlignment.Near);

                AddValueColumn("TorqueResult", Language.Col_TorqueResult, 130);
                AddValueColumn("TorqueTarget", Language.Col_TorqueTarget, 130);
                AddValueColumn("TorqueLower", Language.Col_TorqueLower, 130);
                AddValueColumn("TorqueMax", Language.Col_TorqueMax, 130);
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
            column.Format.FormatString = "0.000";
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
            if (e.KeyCode != Keys.Enter)
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

            using (var busy = BusyOverlay.Show(this, cancelable: true))
            {
                try
                {
                    btnSearch.Enabled = false;
                    _nodes = await FetchOverviewAsync(serialNo, busy.Token);

                    if (IsDisposed || Disposing) return;   // 조회 중 탭을 닫았다

                    treeOverview.DataSource = _nodes;
                    treeOverview.ExpandAll();

                    if (_nodes.Count == 0)
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

        private async Task<List<WorkIntegratedNodeDto>> FetchOverviewAsync(string serialNo, CancellationToken cancellationToken)
        {
            if (_appSet.UseMockData)
            {
                await Task.Yield();
                return MockWorkIntegratedData.GetOverview(serialNo);
            }

            return await _apiClients.ProductionApiClient.GetWorkIntegratedOverviewAsync(serialNo, cancellationToken)
                   ?? new List<WorkIntegratedNodeDto>();
        }

        /// <summary>
        /// [엑셀 내보내기] — 화면에 보이는 트리를 그대로 xlsx로 저장한다(계층/컬럼 구성 유지).
        /// 저장 위치는 사용자가 고르며, 기본 파일명에 시리얼번호와 시각을 넣어 덮어쓰기를 피한다.
        /// </summary>
        private void btnExcelExport_Click(object sender, EventArgs e)
        {
            if (_nodes.Count == 0)
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
                    AppMessageBox.Warn(this, $"{Language.WorkIntegrated_ExportFailed}\n{ex.Message}", Language.WorkIntegrated_ExcelExport);
                }
            }
        }

        // ────────────────────────── 엑셀 내보내기 (아웃라인 계층) ──────────────────────────

        /// <summary>
        /// 트리를 xlsx로 쓴다. 계층은 <b>Excel 아웃라인(그룹)</b>이 되어 엑셀에서 접었다 펼 수 있다.
        ///
        /// <c>treeOverview.ExportToXlsx(...)</c>를 쓰지 않는 이유는
        /// <see cref="XlsxOutlineWriter"/> 주석에 적어 두었다(요약: 그 API는 아웃라인을 만들지 못한다).
        /// </summary>
        private void ExportOverviewToXlsx(string fileName)
        {
            var writer = new XlsxOutlineWriter(Language.WorkIntegrated_OverviewTitle, BuildExportColumns());
            AppendNodes(writer, treeOverview.Nodes);
            writer.Save(fileName);
        }

        /// <summary>내보낼 컬럼 구성 — 화면 컬럼의 제목·폭·정렬을 그대로 옮긴다(언어를 바꾸면 제목도 함께 바뀐다).</summary>
        private List<XlsxColumn> BuildExportColumns()
        {
            var columns = new List<XlsxColumn>();
            foreach (TreeListColumn column in treeOverview.Columns)
            {
                var align = column.AppearanceCell.TextOptions.HAlignment == HorzAlignment.Center
                    ? XlsxAlign.Center
                    : XlsxAlign.Left;
                columns.Add(new XlsxColumn(column.Caption, column.Width, align));
            }
            return columns;
        }

        /// <summary>
        /// 트리를 화면에 보이는 순서 그대로 재귀로 훑어 한 줄씩 넘긴다.
        /// 노드의 <see cref="TreeListNode.Level"/>이 그대로 엑셀 아웃라인 레벨이 된다.
        /// </summary>
        private void AppendNodes(XlsxOutlineWriter writer, TreeListNodes nodes)
        {
            foreach (TreeListNode node in nodes)
            {
                var dto = treeOverview.GetDataRecordByNode(node) as WorkIntegratedNodeDto;
                if (dto != null)
                    writer.AddRow(node.Level, BuildExportValues(dto));

                if (node.Nodes.Count > 0)
                    AppendNodes(writer, node.Nodes);
            }
        }

        /// <summary>
        /// 한 줄의 값. 값이 있는 칸은 <b>숫자 그대로</b> 넘겨 엑셀에서 계산·비교가 되게 하고,
        /// 값이 없는 칸은 화면과 같이 ".."로 채운다.
        /// </summary>
        private static object[] BuildExportValues(WorkIntegratedNodeDto dto)
        {
            return new object[]
            {
                dto.NodeCode ?? string.Empty,
                string.IsNullOrEmpty(dto.NodeDescription) ? EmptyValueText : dto.NodeDescription,
                ValueOrPlaceholder(dto.TorqueResult),
                ValueOrPlaceholder(dto.TorqueTarget),
                ValueOrPlaceholder(dto.TorqueLower),
                ValueOrPlaceholder(dto.TorqueMax),
                ValueOrPlaceholder(dto.AngleTarget),
                ValueOrPlaceholder(dto.AngleLower),
                ValueOrPlaceholder(dto.AngleUpper),
                ValueOrPlaceholder(dto.AngleResult)
            };
        }

        /// <summary>값이 있으면 <see cref="decimal"/>(숫자 칸), 없으면 ".." 문자열.</summary>
        private static object ValueOrPlaceholder(decimal? value) =>
            value.HasValue ? (object)value.Value : EmptyValueText;
    }
}
