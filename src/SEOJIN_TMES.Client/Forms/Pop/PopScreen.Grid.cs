using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using SEOJIN_TMES.Client.ApiClient;
using SEOJIN_TMES.Client.Common.Errors;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Core;
using SEOJIN_TMES.Client.Equipment.Nutrunner;
using SEOJIN_TMES.Client.Forms.Common;
using SEOJIN_TMES.Client.Theme;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SEOJIN_TMES.Client.Forms.Pop
{
    /// <summary>
    /// 하단 탭 4개(투입자재 / BOM 스팩 / 검사 스팩 / 연결 장비)의 그리드 — 컬럼 구성 · 바인딩 · 그리드 이벤트.
    /// </summary>
    public partial class PopScreen
    {
        /// <summary>[BOM 스팩] 탭의 "결과값 입력" 컬럼(데이터에 없는 화면 전용 컬럼)의 필드명.</summary>
        private const string ResultInputField = "__ResultInput";

        /// <summary>[BOM 스팩] 탭의 "토크 결과값" 컬럼 필드명(<see cref="PopBomSpecDto.TorqueResult"/>).</summary>
        private const string TorqueResultField = "TorqueResult";
        private const string AngleResultField = "AngleResult";

        /// <summary>[연결 장비] 탭의 "재연결" 컬럼(데이터에 없는 화면 전용 컬럼)의 필드명.</summary>
        private const string ReconnectField = "__Reconnect";

        /// <summary>[투입자재] 탭에서 클릭하면 시리얼 입력 팝업이 열리는 칸.</summary>
        private const string SerialInputStateField = "SerialInputStateText";

        /// <summary>토크 결과값을 다루는 소수 자릿수 — 이 자리에서 올림해 저장한다.</summary>
        private const int TorqueDecimals = 2;

        /// <summary>작업자가 직접 넣은 결과의 설비코드 — 서버가 입력 경로(설비/사람)를 구분하는 값이다.</summary>
        private const string ManualEquipmentCode = "MAN";

        /// <summary>[검사 스팩] 판정 콤보의 펼침 버튼 폭 배율(기본 폭 대비).</summary>
        private const int ResultStateButtonScale = 2;

        /// <summary>클립보드가 다른 프로그램에 잡혀 있을 때 다시 시도하는 횟수와 간격(ms) — 원격 접속 · 클립보드 관리 프로그램이 잠깐씩 잡는다.</summary>
        private const int ClipboardRetryCount = 5;
        private const int ClipboardRetryDelayMs = 100;

        private List<PopInputMaterialDto> _materials = new List<PopInputMaterialDto>();
        private List<PopBomSpecDto> _bomSpecs = new List<PopBomSpecDto>();
        private List<PopInspectionSpecDto> _inspections = new List<PopInspectionSpecDto>();
        private List<PopEquipmentDto> _equipments = new List<PopEquipmentDto>();

        /// <summary>설비 체결 결과를 채울 [BOM 스팩] 행. 작업자가 행을 고르거나 합격 결과가 들어오면 옮겨 간다.</summary>
        private int _bomSpecSelectedRow;

        /// <summary>
        /// [BOM 스팩] 그리드를 다시 그리는 중(DataSource 바인딩 · RefreshData)인 구간의 깊이.
        /// 0보다 크면 <see cref="GridViewBomSpec_FocusedRowChanged"/>를 무시한다 —
        /// 다시 그릴 때 DevExpress가 포커스 행을 재설정하며 이벤트를 올리는데,
        /// 그때마다 너트러너에 목표 토크를 다시 쓰거나 "사용할 수 있는 너트러너가 없습니다" 창이 뜨면 안 된다.
        /// 바깥 구간 안에서 <see cref="RefreshBomSpecGrid"/>를 또 불러도 풀리지 않도록 bool이 아닌 카운터로 둔다.
        /// </summary>
        private int _bomSpecRedrawDepth;

        private RepositoryItemButtonEdit _resultInputEditor;
        private RepositoryItemTextEdit _torqueResultEditor;
        private RepositoryItemComboBox _resultStateEditor;
        private RepositoryItemTextEdit _resultValueEditor;
        private RepositoryItemTextEdit _resultRemarkEditor;
        private RepositoryItemButtonEdit _reconnectEditor;

        /// <summary>재연결 대상이 아닌 줄의 "재연결" 칸에 끼우는 빈 에디터(버튼을 그리지 않는다).</summary>
        private RepositoryItemTextEdit _reconnectBlankEditor;

        /// <summary>[검사 스팩] 탭에서 작업자가 입력할 수 있는 칸.</summary>
        private readonly HashSet<string> _inspectionInputFields = new HashSet<string>
        {
            "ResultState", "ResultValue", "InspectionResultRemark"
        };

        /// <summary>
        /// [검사 스팩]에서 고쳤지만 아직 서버에 보내지 않은 줄. 셀을 고칠 때마다 보내지 않고,
        /// 작업자가 그 줄에서 다른 줄로 포커스를 옮길 때 한 번에 보낸다(<see cref="GridViewInspection_FocusedRowChanged"/>).
        /// 줄을 떠나지 않은 채 공정을 옮기면 이동 전에 남은 줄을 보낸다(<see cref="SavePendingInspections"/>).
        /// 다시 바인딩하면 DTO가 새로 만들어지므로 <see cref="SetGridData"/>에서 비운다.
        /// </summary>
        private readonly HashSet<PopInspectionSpecDto> _pendingInspections = new HashSet<PopInspectionSpecDto>();

        private bool IsBomSpecRedrawing => _bomSpecRedrawDepth > 0;

        private void BeginBomSpecRedraw()
        {
            _bomSpecRedrawDepth++;
        }

        private void EndBomSpecRedraw()
        {
            if (_bomSpecRedrawDepth > 0)
                _bomSpecRedrawDepth--;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  그리드 구성
        // ─────────────────────────────────────────────────────────────────────────

        public void InitGrids()
        {
            InitMaterialGrid();
            InitBomSpecGrid();
            InitInspectionGrid();
            InitEquipmentGrid();

            foreach (var view in new GridView[] { gridViewMaterial, gridViewBomSpec, gridViewInspection, gridViewEquipment })
            {
                LockGridCustomization(view);

                // 다국어 · 작은 화면에서 머리글은 줄바꿈, 셀은 말줄임 + 힌트(AppTheme.GridTextFit 참고).
                AppTheme.GridTextFit(view);
            }

            // OK/NG 칸은 선택돼도 결과 색을 유지한다 — 선택 줄은 그 칸 안쪽 테두리로 표시한다.
            // 투입자재 그리드는 칠하는 칸이 없어 빠진다(참조 자재도 색을 바꾸지 않는다).
            foreach (var view in new GridView[] { gridViewBomSpec, gridViewInspection })
                AppTheme.ResultCellSelection(view);

            // 터치 POP에는 물리 키보드가 없다 — 값을 넣는 칸의 편집기가 열리면 가상 키보드를 띄운다.
            gridViewBomSpec.ShownEditor += GridView_ShownEditor;
            gridViewInspection.ShownEditor += GridView_ShownEditor;
        }

        /// <summary>
        /// 컬럼 머리글로 할 수 있는 조작을 <b>정렬만</b> 남기고 막는다.
        /// 터치 화면에서 머리글을 잘못 끌면 그룹으로 묶이거나 컬럼이 사라져, 작업자가 원래 화면으로 되돌릴 방법이 없었다.
        /// 우클릭 메뉴(컬럼 · 그룹 패널 · 합계 · 그룹 행)도 같은 이유로 띄우지 않는다. 컬럼 폭 조절은 그대로 둔다.
        /// </summary>
        private void LockGridCustomization(GridView view)
        {
            view.OptionsCustomization.AllowSort = true;
            view.OptionsCustomization.AllowColumnMoving = false;
            view.OptionsCustomization.AllowGroup = false;
            view.OptionsCustomization.AllowQuickHideColumns = false;
            view.OptionsCustomization.AllowFilter = false;
            view.OptionsView.ShowGroupPanel = false;
            view.OptionsFind.AllowFindPanel = false;

            view.OptionsMenu.EnableColumnMenu = false;
            view.OptionsMenu.EnableGroupPanelMenu = false;
            view.OptionsMenu.EnableFooterMenu = false;
            view.OptionsMenu.EnableGroupRowMenu = false;
        }

        private void InitMaterialGrid()
        {
            // PopInputMaterialDto.SerialNumbers(컬렉션 속성)를 그리드가 마스터-디테일로 오인해
            // 행 왼쪽에 [+] 펼침 버튼을 만든다. 이 화면에는 하위 목록이 없으므로 꺼 둔다.
            gridViewMaterial.OptionsDetail.EnableMasterViewMode = false;

            gridMaterial.DataSource = new List<PopInputMaterialDto>();
            GridColumns.Apply(gridViewMaterial, Language.Col_No,
                GridColumns.Text("MaterialCode", Language.Col_MaterialCode, 200),
                GridColumns.Text("MaterialName", Language.Col_MaterialName, 300),
                GridColumns.Text("PurchasePlanMethod", Language.Col_PurchasePlanMethod, 130),
                //GridColumns.Text("MaterialGroup", Language.Col_MaterialGroup, 100),
                GridColumns.Number("InputQty", Language.Col_InputQty, 110),
                GridColumns.Text("InputUnit", Language.Col_InputUnit, 100),
                GridColumns.Text("InputPosition", Language.Col_InputPosition, 110),
                GridColumns.Text("SerialCheckText", Language.Col_SerialCheck, 170),
                GridColumns.Text(SerialInputStateField, Language.Col_SerialInputState, 180));

            gridViewMaterial.OptionsView.ColumnAutoWidth = true;

            // 클릭해서 팝업을 여는 칸이라 포커스를 받을 수 있게 둔다.
            var stateColumn = gridViewMaterial.Columns.ColumnByFieldName(SerialInputStateField);
            if (stateColumn != null)
                stateColumn.OptionsColumn.AllowFocus = true;

            gridViewMaterial.FocusedRowChanged += GridViewMaterial_FocusedRowChanged;
            SetGridLineColor(gridViewMaterial);
        }

        /// <summary>
        /// [BOM 스팩] 탭. 마지막의 "결과값 입력" 컬럼은 데이터에 없는 화면 전용 컬럼이며,
        /// 수동 렌치를 켰을 때만 보인다([입력] 버튼 → 하한~상한 중간값을 그 줄에 채운다).
        /// "토크 결과값" · "각도 결과값" 컬럼은 작업자가 직접 고쳐 쓸 수 있다.
        /// </summary>
        private void InitBomSpecGrid()
        {
            gridBomSpec.DataSource = new List<PopBomSpecDto>();
            EnsureResultInputColumn();

            GridColumns.Apply(gridViewBomSpec, Language.Col_No,
                GridColumns.Text("MaterialCode", Language.Col_MaterialCode, 180),
                GridColumns.Text("MaterialName", Language.Col_MaterialName, 250),
                GridColumns.Text("PurchasePlanMethod", Language.Col_PurchasePlanMethod, 130),
                GridColumns.Text("InputPosition", Language.Col_InputPosition, 100),
                GridColumns.Text("BomSpecName", Language.Col_BomSpecName, 140),
                GridColumns.Text("SpecType", Language.Col_SpecType, 120),
                GridColumns.Text("TorqueLower", Language.Col_TorqueLower, 120),
                GridColumns.Text("TorqueUpper", Language.Col_TorqueUpper, 120),
                GridColumns.Text("TorqueTarget", Language.Col_TorqueTarget, 120),
                GridColumns.Text("TorqueUnit", Language.Col_TorqueUnit, 100),
                GridColumns.Text(TorqueResultField, Language.Col_TorqueResult, 130),
                GridColumns.Text("AngleLower", Language.Col_AngleLower, 120),
                GridColumns.Text("AngleUpper", Language.Col_AngleUpper, 120),
                GridColumns.Text("AngleTarget", Language.Col_AngleTarget, 120),
                GridColumns.Text("AngleUnit", Language.Col_AngleUnit, 100),
                GridColumns.Text(AngleResultField, Language.Col_AngleResult, 130),
                GridColumns.Text("EquipmentCode", Language.Col_ResultAction, 130),
                GridColumns.Text(ResultInputField, Language.Pop_ResultInputColumn, 120));

            gridViewBomSpec.OptionsView.ColumnAutoWidth = true;

            // 셀을 클릭해 포커스를 주기 전에도 [입력] 버튼이 모든 줄에 보이도록 한다
            // (버튼 에디터를 쓰는 컬럼이 "결과값 입력" 하나뿐이라 뷰 전체에 걸어도 무방하다).
            gridViewBomSpec.OptionsView.ShowButtonMode = ShowButtonModeEnum.ShowAlways;

            NumericFormat(gridViewBomSpec, "TorqueLower", "TorqueUpper", "TorqueTarget", TorqueResultField,
                "AngleLower", "AngleUpper", "AngleTarget", AngleResultField);

            EnableTorqueResultEditing();

            foreach (DevExpress.XtraGrid.Columns.GridColumn column in gridViewBomSpec.Columns)
            {
                column.OptionsColumn.AllowEdit =
                    column.FieldName == ResultInputField || column.FieldName == TorqueResultField || column.FieldName == AngleResultField;
            }

            gridViewBomSpec.OptionsBehavior.Editable = true;
            SetResultInputColumnVisible(false);

            // 복사(Ctrl+C) · 붙여넣기(Ctrl+V)는 GridViewBomSpec_KeyDown에서 직접 처리한다.
            // 그리드 자체의 붙여넣기(줄 추가 · 여러 칸 덮어쓰기)는 꺼 둔다 — 입력 칸이 아닌 곳의 값이 바뀌면 안 된다.
            gridViewBomSpec.OptionsClipboard.PasteMode = DevExpress.Export.PasteMode.None;
            gridViewBomSpec.KeyDown += GridViewBomSpec_KeyDown;

            SetGridLineColor(gridViewBomSpec);
        }

        private void InitInspectionGrid()
        {
            gridInspection.DataSource = new List<PopInspectionSpecDto>();
            GridColumns.Apply(gridViewInspection, Language.Col_No,
                GridColumns.Text("InspectionSpecName", Language.Col_InspectionSpecName, 300),
                GridColumns.Text("InputPosition", Language.Col_InputPosition, 80),
                GridColumns.Text("Description", Language.Col_Description, 300),
                GridColumns.Text("InspectionMethod", Language.Col_InspectionMethod, 120),
                GridColumns.Text("InspectionLower", Language.Col_InspectionLower, 90),
                GridColumns.Text("InspectionUpper", Language.Col_InspectionUpper, 90),
                GridColumns.Text("InspectionTarget", Language.Col_InspectionTarget, 90),
                GridColumns.Text("ResultState", Language.Col_ResultState, 80),
                GridColumns.Text("ResultValue", Language.Col_ResultValue, 130),
                GridColumns.Text("InspectionResultRemark", Language.Col_InspectionRemark, 120));

            gridViewInspection.OptionsView.ColumnAutoWidth = true;
            NumericFormat(gridViewInspection, "InspectionLower", "InspectionUpper", "InspectionTarget");

            // 검사방법은 서버 코드(QNT/QLT)로 들고 있고, 보여 줄 때만 화면 언어의 정량/정성으로 바꾼다.
            gridViewInspection.CustomColumnDisplayText += GridViewInspection_CustomColumnDisplayText;

            _resultStateEditor = new RepositoryItemComboBox();
            _resultStateEditor.TextEditStyle = TextEditStyles.DisableTextEditor;
            // 판정은 OK / NG 코드만 쓴다 — 고른 값이 그대로 서버로 나가므로 번역 문구를 넣지 않는다.
            _resultStateEditor.Items.Add(PopInspectionSpecDto.ResultOk);
            _resultStateEditor.Items.Add(PopInspectionSpecDto.ResultNg);
            // 현장 터치 화면에서 펼침 버튼이 작아 잘 안 눌린다 — 기본 폭(스크롤바 폭과 같다)의 두 배로 키운다.
            _resultStateEditor.Buttons[0].Width = SystemInformation.VerticalScrollBarWidth * ResultStateButtonScale;
            gridInspection.RepositoryItems.Add(_resultStateEditor);

            _resultValueEditor = new RepositoryItemTextEdit();
            gridInspection.RepositoryItems.Add(_resultValueEditor);

            _resultRemarkEditor = new RepositoryItemTextEdit();
            gridInspection.RepositoryItems.Add(_resultRemarkEditor);

            foreach (DevExpress.XtraGrid.Columns.GridColumn column in gridViewInspection.Columns)
                column.OptionsColumn.AllowEdit = false;

            SetEditableColumn(gridViewInspection, "ResultState", _resultStateEditor);
            SetEditableColumn(gridViewInspection, "ResultValue", _resultValueEditor);
            SetEditableColumn(gridViewInspection, "InspectionResultRemark", _resultRemarkEditor);

            gridViewInspection.OptionsBehavior.Editable = true;
            // 판정 칸은 누르기 전에도 펼침 버튼이 보여야 작업자가 고르는 칸인 줄 안다.
            // 버튼이 있는 편집기는 판정 콤보뿐이라 다른 칸에는 영향이 없다.
            gridViewInspection.OptionsView.ShowButtonMode = ShowButtonModeEnum.ShowAlways;

            gridViewInspection.RowCellStyle += GridViewInspection_RowCellStyle;
            gridViewInspection.FocusedRowChanged += GridViewInspection_FocusedRowChanged;
            SetGridLineColor(gridViewInspection);
        }

        /// <summary>
        /// [연결 장비] 탭. 마지막의 "재연결" 컬럼은 데이터에 없는 화면 전용 컬럼이며,
        /// 연결이 끊긴(연결 실패) 장비 줄에만 [재연결] 버튼이 보인다.
        /// </summary>
        private void InitEquipmentGrid()
        {
            gridEquipment.DataSource = new List<PopEquipmentDto>();
            EnsureReconnectColumn();

            GridColumns.Apply(gridViewEquipment, Language.Col_No,
                GridColumns.Text("EquipmentCode", Language.Col_EquipmentCode, 160),
                GridColumns.Text("EquipmentName", Language.Col_EquipmentName, 150),
                GridColumns.Left("EquipmentRemark", Language.Col_EquipmentRemark, 260),
                GridColumns.Text("EquipmentType", Language.Col_EquipmentType, 130),
                GridColumns.Text("CommunicationState", Language.Col_CommunicationState, 130),
                GridColumns.Text("EquipmentState", Language.Col_EquipmentState, 130),
                GridColumns.Text("RangeLower", Language.Col_RangeLower, 150),
                GridColumns.Text("RangeUpper", Language.Col_RangeUpper, 150),
                GridColumns.Text(ReconnectField, Language.Pop_ReconnectColumn, 120));

            gridViewEquipment.OptionsView.ColumnAutoWidth = true;
            gridViewEquipment.OptionsView.ShowButtonMode = ShowButtonModeEnum.ShowAlways;
            NumericFormat(gridViewEquipment, "RangeLower", "RangeUpper");

            // 값 입력은 없지만 [재연결] 버튼이 눌리려면 셀 에디터가 열려야 한다 — 편집 허용 컬럼은 "재연결" 하나뿐이다.
            foreach (DevExpress.XtraGrid.Columns.GridColumn column in gridViewEquipment.Columns)
                column.OptionsColumn.AllowEdit = column.FieldName == ReconnectField;

            gridViewEquipment.OptionsBehavior.Editable = true;

            // 재연결 대상인 줄에만 버튼을 끼우고, 그 외 줄은 에디터 자체를 열지 않는다.
            gridViewEquipment.CustomRowCellEdit += GridViewEquipment_CustomRowCellEdit;
            gridViewEquipment.ShowingEditor += GridViewEquipment_ShowingEditor;

            SetGridLineColor(gridViewEquipment);
        }

        /// <summary>"재연결" 컬럼과 [재연결] 버튼 에디터를 준비한다(이미 있으면 재사용).</summary>
        private void EnsureReconnectColumn()
        {
            if (_reconnectEditor == null)
            {
                _reconnectEditor = new RepositoryItemButtonEdit();
                _reconnectEditor.TextEditStyle = TextEditStyles.HideTextEditor;
                _reconnectEditor.Buttons.Clear();

                var button = new EditorButton(ButtonPredefines.Glyph);
                button.Caption = Language.Pop_ReconnectButton;
                _reconnectEditor.Buttons.Add(button);
                _reconnectEditor.ButtonClick += Reconnect_ButtonClick;

                gridEquipment.RepositoryItems.Add(_reconnectEditor);

                _reconnectBlankEditor = new RepositoryItemTextEdit();
                _reconnectBlankEditor.ReadOnly = true;
                gridEquipment.RepositoryItems.Add(_reconnectBlankEditor);
            }

            var column = gridViewEquipment.Columns.ColumnByFieldName(ReconnectField);
            if (column == null)
            {
                column = gridViewEquipment.Columns.AddField(ReconnectField);
                column.UnboundType = DevExpress.Data.UnboundColumnType.String;
                column.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.False;
            }

            column.ColumnEdit = _reconnectEditor;
        }

        /// <summary>탭 4개 그리드가 같은 행 · 열 구분선 색을 쓰게 한다.</summary>
        private void SetGridLineColor(GridView view)
        {
            view.Appearance.HorzLine.BackColor = AppTheme.GridLine;
            view.Appearance.HorzLine.Options.UseBackColor = true;
            view.Appearance.VertLine.BackColor = AppTheme.GridLine;
            view.Appearance.VertLine.Options.UseBackColor = true;
        }

        /// <summary>읽기 전용으로 잠근 그리드에서 한 컬럼만 편집할 수 있게 열고 편집기를 붙인다.</summary>
        private void SetEditableColumn(GridView view, string fieldName, RepositoryItem editor)
        {
            var column = view.Columns.ColumnByFieldName(fieldName);
            if (column == null)
                return;

            column.OptionsColumn.AllowEdit = true;
            column.ColumnEdit = editor;
        }

        /// <summary>
        /// "토크 결과값" 칸을 작업자가 직접 고쳐 쓸 수 있게 한다.
        ///
        /// 숫자 검사는 <see cref="GridViewBomSpec_ValidatingEditor"/>에서 하고,
        /// 마스크로 미리 막지는 않는다 — 붙여넣기·IME 같은 경로까지 한 곳에서 걸러야
        /// "무엇이 왜 거절됐는지"를 한 가지 문구로 안내할 수 있기 때문이다.
        /// </summary>
        private void EnableTorqueResultEditing()
        {
            if (_torqueResultEditor == null)
            {
                _torqueResultEditor = new RepositoryItemTextEdit();
                gridBomSpec.RepositoryItems.Add(_torqueResultEditor);
            }

            var column = gridViewBomSpec.Columns.ColumnByFieldName(TorqueResultField);
            if (column != null)
                column.ColumnEdit = _torqueResultEditor;
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
                button.Caption = Language.Pop_InputButton;
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

        private void SetResultInputColumnVisible(bool visible)
        {
            var column = gridViewBomSpec.Columns.ColumnByFieldName(ResultInputField);
            if (column == null)
                return;

            column.Visible = visible;
            if (visible)
                column.VisibleIndex = gridViewBomSpec.VisibleColumns.Count;
        }

        /// <summary>소수 한 자리 기준(1.0 / 4.25)으로 보여 주는 숫자 컬럼 서식.</summary>
        private void NumericFormat(GridView view, params string[] fieldNames)
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

        // ─────────────────────────────────────────────────────────────────────────
        //  바인딩
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// 기준정보에 IP · 포트가 없는(또는 포트가 1~65534 숫자가 아닌) 설비는 "설비 없음"으로 보고 목록에서 뺀다.
        /// 예전에는 0.0.0.0 · 5000으로 등록해 연결할 수 없는 대상에 재연결을 끝없이 되풀이했고,
        /// 그리드에는 "연결 실패"로 남아 작업자가 설비 고장으로 오해했다.
        /// 여기서 빼 두면 그리드 · 장비 등록(<see cref="WireEquipmentEvents"/>) · "너트러너 없음" 경고 판정이 모두 같은 목록을 본다.
        /// </summary>
        private List<PopEquipmentDto> SelectConnectableEquipments(List<PopEquipmentDto> equipments)
        {
            var result = new List<PopEquipmentDto>();
            if (equipments == null)
                return result;

            foreach (var equipment in equipments)
            {
                if (equipment == null)
                    continue;

                if (string.IsNullOrWhiteSpace(equipment.EquipmentIp) || !TryGetEquipmentPort(equipment, out _))
                {
                    AppLog.Warn($"[POP] 설비 IP · 포트 없음 - 목록에서 제외: {equipment.EquipmentCode} " +
                                $"(IP '{equipment.EquipmentIp}', 포트 '{equipment.EquipmentPort}')");
                    continue;
                }

                result.Add(equipment);
            }

            return result;
        }

        /// <summary>기준정보의 설비 포트(문자열)를 Modbus 포트로 읽는다. 비었거나 1~65534 밖이면 false.</summary>
        private bool TryGetEquipmentPort(PopEquipmentDto equipment, out int port)
        {
            // 이벤트 포트는 port + 1을 쓰므로 65535는 받을 수 없다.
            return int.TryParse(equipment.EquipmentPort?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out port)
                   && port > 0 && port < 65535;
        }

        /// <summary>서버가 한 번에 내려준 탭 4개 데이터를 그리드에 붙인다.</summary>
        public void SetGridData(PopScreenData screen)
        {
            _materials = screen.Materials ?? new List<PopInputMaterialDto>();
            _inspections = screen.InspectionSpecs ?? new List<PopInspectionSpecDto>();
            _pendingInspections.Clear();
            _equipments = SelectConnectableEquipments(screen.Equipments);

            gridMaterial.DataSource = _materials;
            gridInspection.DataSource = _inspections;
            gridEquipment.DataSource = _equipments;

            gridViewMaterial.RefreshData();
            gridViewInspection.RefreshData();
            gridViewEquipment.RefreshData();

            // 서버가 새로 준 장비 목록은 통신상태가 "Disconnected"로 비어 있다. 이미 연결된 장비는
            // ReadyChanged · ConnectionChanged가 다시 오지 않으므로, 재조회(상태 전이 · 공정 이동 · 409 재조회)마다
            // 현재 Modbus 상태로 바로 채운다 — 그러지 않으면 연결돼 있는데도 "연결 실패/사용불가"로 남는다.
            RefreshEquipmentStates();

            // 바인딩 중 포커스가 움직여도 너트러너에 토크를 쓰지 않는다.
            BeginBomSpecRedraw();
            try
            {
                _bomSpecs = screen.BomSpecs ?? new List<PopBomSpecDto>();
                gridBomSpec.DataSource = _bomSpecs;
                gridViewBomSpec.RefreshData();

                // 앞 공정에서 쓰던 행 위치를 그대로 들고 오면 새 공정의 마지막 행(또는 범위 밖)에 결과가 채워진다.
                FocusBomSpecRow(FindBomSpecTargetRow());
            }
            finally
            {
                EndBomSpecRedraw();
            }

            // 공정이 바뀌면 시작 여부와 상관없이 바로 새 목표 토크를 쓴다 — 자동 시작이 늦거나 실패해도
            // 장비가 앞 공정의 토크로 남지 않게. 시작 뒤 SelectBomSpecRow가 행을 다시 고르면 한 번 더 쓴다.
            Screen.WriteBomSpecTorque();
        }

        /// <summary>값만 바뀐 행을 다시 그린다 — 포커스 이벤트는 올리지 않는다.</summary>
        public void RefreshBomSpecGrid()
        {
            BeginBomSpecRedraw();
            try
            {
                gridViewBomSpec.RefreshData();
            }
            finally
            {
                EndBomSpecRedraw();
            }
        }

        /// <summary>
        /// 설비 결과를 채울 [BOM 스팩] 행을 골라 포커스를 옮기고, 그 행의 목표 토크를 연결된 너트러너에 쓴다.
        /// 공정에 들어올 때(진입 · 이동 · 자동 진행)와 투입자재 입력을 마쳐 [BOM 스팩] 탭으로 넘어올 때 부른다.
        /// </summary>
        public void SelectBomSpecRow()
        {
            FocusBomSpecRow(FindBomSpecTargetRow());
            Screen.WriteBomSpecTorque();
        }

        /// <summary>
        /// <paramref name="rowHandle"/> 바로 아래부터 찾은 첫 입력 대상 행(참조 줄 건너뜀). 없으면 InvalidRowHandle.
        /// 설비 합격 뒤 "바로 아래 행"으로 넘어갈 때 참조 줄에 멈추지 않게 한다.
        /// </summary>
        private int FindNextBomSpecInputRow(int rowHandle)
        {
            for (int next = rowHandle + 1; next < gridViewBomSpec.RowCount; next++)
            {
                if (gridViewBomSpec.GetRow(next) is PopBomSpecDto spec && !spec.IsReference)
                    return next;
            }

            return DevExpress.XtraGrid.GridControl.InvalidRowHandle;
        }

        /// <summary>
        /// 설비 결과를 채울 첫 행 — NG이거나 결과가 아직 없는 첫 행(참조 줄은 합격으로 보므로 건너뛴다).
        /// 그런 행이 없으면 참조 아닌 첫 행, 그것도 없으면(전부 참조 줄) 첫 행.
        /// 그리드에 보이는 순서(정렬 반영)대로 본다. 행이 하나도 없으면 InvalidRowHandle.
        /// </summary>
        private int FindBomSpecTargetRow()
        {
            for (int rowHandle = 0; rowHandle < gridViewBomSpec.RowCount; rowHandle++)
            {
                if (gridViewBomSpec.GetRow(rowHandle) is PopBomSpecDto spec && !IsBomSpecPassed(spec))
                    return rowHandle;
            }

            if (gridViewBomSpec.RowCount == 0)
                return DevExpress.XtraGrid.GridControl.InvalidRowHandle;

            var inputRow = FindInputRow(gridViewBomSpec, -1, 1);
            return inputRow != DevExpress.XtraGrid.GridControl.InvalidRowHandle ? inputRow : 0;
        }

        /// <summary>
        /// 행을 설비 결과를 채울 행으로 삼고 포커스를 옮긴다. 목표 토크는 쓰지 않는다 —
        /// 포커스 이벤트로 쓰게 두면 너트러너가 없을 때 경고 창이 뜨므로, 쓸 곳에서 한 번만 쓴다.
        /// </summary>
        private void FocusBomSpecRow(int rowHandle)
        {
            _bomSpecSelectedRow = rowHandle;
            if (rowHandle < 0)
                return;

            BeginBomSpecRedraw();
            try
            {
                gridViewBomSpec.FocusedRowHandle = rowHandle;
            }
            finally
            {
                EndBomSpecRedraw();
            }
        }

        /// <summary>
        /// [연결 장비] 탭의 통신상태/장비상태를 현재 Modbus 연결 상태로 덮어쓴다.
        ///  - 사용 가능(<see cref="INutrunner.IsReady"/>)         → 연결 성공 / 사용 가능
        ///  - Request 채널만 붙고 이벤트 수신 확인 전             → 연결 중 / 사용불가
        ///    (예전에는 이 구간도 "연결 실패"로 그려 연결 직후 빨갛게 보였다)
        ///  - 그 외                                              → 연결 실패 / 사용불가
        /// ReadyChanged · ConnectionChanged가 올 때 부른다.
        /// </summary>
        public void RefreshEquipmentStates()
        {
            if (_equip?.Manager == null)
                return;

            foreach (var equipment in _equipments)
            {
                if (!_equip.Manager.TryGet(equipment.EquipmentCode, out INutrunner nutrunner) || nutrunner == null)
                    continue;

                if (nutrunner.IsReady)
                {
                    equipment.CommunicationState = Language.Pop_CommConnected;
                    equipment.EquipmentState = Language.Pop_EquipmentUsable;
                }
                else if (nutrunner.IsConnected)
                {
                    equipment.CommunicationState = Language.Pop_CommConnecting;
                    equipment.EquipmentState = Language.Pop_EquipmentUnusable;
                }
                else
                {
                    equipment.CommunicationState = Language.Pop_CommFailed;
                    equipment.EquipmentState = Language.Pop_EquipmentUnusable;
                }
            }

            gridViewEquipment.RefreshData();
        }

        /// <summary>
        /// BOM 스팩 한 줄이 측정되고 NG가 아닌지 — 토크 · 각도가 모두 하한~상한 안이어야 한다
        /// (값이 비어 있으면 비교가 false라 통과하지 않는다).
        /// 그리드의 NG 표시 · 설비 결과의 다음 줄 이동 · 자동 진행이 모두 이 기준을 쓴다.
        /// 참조 줄(서버 refLtr=AR·REF)은 체결 대상이 아니므로 값 없이도 통과로 본다.
        /// 각도 하한 · 상한이 둘 다 없는 줄은 각도 관리 대상이 아니라서 토크만 본다
        /// (null 비교가 false라 예전에는 각도 값을 넣어도 영원히 NG로 남아 자동 진행이 막혔다).
        /// </summary>
        private bool IsBomSpecPassed(PopBomSpecDto spec)
        {
            if (spec.IsReference)
                return true;

            if (!IsAngleManaged(spec))
                return IsTorquePassed(spec);

            return IsTorquePassed(spec) && IsAnglePassed(spec);
        }

        /// <summary>토크 결과값이 하한~상한 안인지(값이 비어 있으면 false).</summary>
        private bool IsTorquePassed(PopBomSpecDto spec)
        {
            return spec.TorqueResult >= spec.TorqueLower && spec.TorqueResult <= spec.TorqueUpper;
        }

        /// <summary>각도 결과값이 하한~상한 안인지(값이 비어 있으면 false).</summary>
        private bool IsAnglePassed(PopBomSpecDto spec)
        {
            return spec.AngleResult >= spec.AngleLower && spec.AngleResult <= spec.AngleUpper;
        }

        /// <summary>각도 하한 · 상한 중 하나라도 있으면 각도도 판정 대상이다.</summary>
        private bool IsAngleManaged(PopBomSpecDto spec)
        {
            return spec.AngleLower != null || spec.AngleUpper != null;
        }

        /// <summary>검사 스팩 한 줄이 입력됐는지 — 결과상태는 필수이고, 정량 검사는 결과값까지 있어야 한다.</summary>
        private bool IsInspectionEntered(PopInspectionSpecDto spec)
        {
            if (string.IsNullOrWhiteSpace(spec.ResultState))
                return false;

            return !spec.IsQuantitative || !string.IsNullOrWhiteSpace(spec.ResultValue);
        }

        /// <summary>
        /// 토크 값을 소수 둘째 자리에서 <b>올림</b>한다(4.561 → 4.57, 2.0 → 2.00).
        /// 판정이 아니라 입력 자릿수를 맞추기 위한 것으로, 합·불 판정은 서버가 원본 값으로 한다.
        /// </summary>
        private decimal CeilTorque(decimal value)
        {
            var scale = 1m;
            for (var i = 0; i < TorqueDecimals; i++)
                scale *= 10m;

            // 음수도 "0에서 먼 쪽"이 아니라 수직선 위쪽으로 올린다(Math.Ceiling과 같은 방향).
            return Math.Ceiling(value * scale) / scale;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  [투입자재] 그리드 이벤트
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// 참조 자재(서버 refLtr=AR·REF)는 OK로 끝난 줄이라 색은 바꾸지 않고, 포커스가 오면 다음 입력 줄로 건너뛴다
        /// (<see cref="SkipReferenceRow"/>).
        /// </summary>
        private void GridViewMaterial_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            SkipReferenceRow(gridViewMaterial, e);
        }

        /// <summary>
        /// 참조 줄(서버 refLtr=AR·REF)에 포커스가 오면 움직인 방향(위 · 아래)의 첫 입력 줄로 넘긴다.
        /// 그 방향에 없으면 반대 방향, 양쪽 다 없으면(전부 참조 줄) 그대로 둔다.
        /// 참조 줄은 입력 대상이 아니라 그 줄에 머물면 작업자가 할 일이 없다 — 키보드 · 클릭 · 바인딩 직후 첫 행 모두 같다.
        /// 포커스를 옮기면 FocusedRowChanged가 새 줄로 다시 불리므로, 옮겼으면(true) 부른 쪽은 이번 호출을 끝낸다.
        /// </summary>
        private bool SkipReferenceRow(GridView view, FocusedRowChangedEventArgs e)
        {
            if (!view.IsDataRow(e.FocusedRowHandle) || !IsReferenceRow(view, e.FocusedRowHandle))
                return false;

            int step = view.IsDataRow(e.PrevFocusedRowHandle) && e.PrevFocusedRowHandle > e.FocusedRowHandle ? -1 : 1;
            int target = FindInputRow(view, e.FocusedRowHandle, step);
            if (target == DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                target = FindInputRow(view, e.FocusedRowHandle, -step);
            if (target == DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                return false;

            view.FocusedRowHandle = target;
            return true;
        }

        /// <summary><paramref name="rowHandle"/>에서 <paramref name="step"/> 방향으로 찾은 첫 참조 아닌 줄. 없으면 InvalidRowHandle.</summary>
        private static int FindInputRow(GridView view, int rowHandle, int step)
        {
            for (int next = rowHandle + step; next >= 0 && next < view.RowCount; next += step)
            {
                if (!IsReferenceRow(view, next))
                    return next;
            }

            return DevExpress.XtraGrid.GridControl.InvalidRowHandle;
        }

        /// <summary>투입자재 · BOM 스팩 줄이 참조 줄(서버 refLtr=AR·REF)인지.</summary>
        private static bool IsReferenceRow(GridView view, int rowHandle)
        {
            var row = view.GetRow(rowHandle);
            if (row is PopInputMaterialDto material)
                return material.IsReference;
            if (row is PopBomSpecDto spec)
                return spec.IsReference;
            return false;
        }

        /// <summary>"시리얼번호 입력상태" 칸을 클릭하면 &lt;투입자재 정보 입력&gt; 팝업을 연다.</summary>
        private async void GridViewMaterial_RowCellClick(object sender, RowCellClickEventArgs e)
        {
            if (e.Column == null || e.Column.FieldName != SerialInputStateField)
                return;

            try
            {
                var bomSpecs = _bomSpecs;
                await Screen.InputMaterialSerial(e.RowHandle);

                // 저장으로 자동 진행이 일어나 공정이 바뀌었으면 새 공정 쪽에서 이미 탭 · 행을 골랐다.
                if (IsDisposed || Disposing || !ReferenceEquals(bomSpecs, _bomSpecs))
                    return;

                // 자재 입력이 다 끝나 다음 탭으로 넘어갔을 때만 체결할 BOM 행 · 목표 토크를 맞춘다.
                if (Screen.SelectInputTab())
                    Screen.SelectBomSpecRow();
            }
            catch (Exception ex)
            {
                HandleError("투입자재 시리얼 입력", ex);
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  [BOM 스팩] 그리드 이벤트
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// 작업자가 고른 행을 설비 체결 결과를 채울 행으로 삼는다. 공정 상태와 상관없이 고른다 —
        /// 진행 중이 아닐 때 들어온 체결 결과는 <see cref="PopScreen.SetTorqueFromEquipment"/>가 따로 버린다.
        /// </summary>
        private void GridViewBomSpec_RowClick(object sender, RowClickEventArgs e)
        {
            if (e.RowHandle < 0)
                return;

            // 참조 줄은 체결 대상이 아니다 — 눌러도 설비 결과를 채울 행은 그대로 둔다.
            if (gridViewBomSpec.GetRow(e.RowHandle) is PopBomSpecDto spec && spec.IsReference)
                return;

            _bomSpecSelectedRow = e.RowHandle;
            AppLog.Info($"[POP] 선택한 Row - ( Row 값 {_bomSpecSelectedRow})");
        }

        /// <summary>
        /// 포커스가 옮겨 간 행의 목표 토크를 너트러너에 써 둔다. 공정 상태와 상관없이 쓴다
        /// (<see cref="PopScreen.WriteBomSpecTorque"/>와 같은 이유 — 장비는 조이기 전에 맞춰져 있어야 한다).
        /// </summary>
        private void GridViewBomSpec_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            // 바인딩 · RefreshData로 다시 그리는 중에 생긴 포커스 이동은 작업자가 고른 것이 아니다.
            if (IsBomSpecRedrawing)
                return;
            if (e.FocusedRowHandle < 0)
                return;

            // 참조 줄은 체결 대상이 아니다 — 다음 체결 줄로 넘기고, 목표 토크는 그 줄로 다시 불린 이 이벤트가 쓴다.
            if (SkipReferenceRow(gridViewBomSpec, e))
                return;

            var spec = gridViewBomSpec.GetRow(e.FocusedRowHandle) as PopBomSpecDto;
            if (spec?.TorqueTarget == null || spec.IsReference)
                return;

            _bomSpecSelectedRow = e.FocusedRowHandle;
            AppLog.Info($"[POP] FocusedRow - ( value {_bomSpecSelectedRow})");

            try
            {
                // 목표 토크는 서버 기준정보 값을 그대로 쓴다(클라이언트가 계산하지 않는다).
                var activated = Screen.WriteTorqueToEquipment(spec.TorqueTarget.Value);

                // 공정에 등록된 장비가 아예 없으면 수동 입력 공정이다 — 알릴 일이 아니다.
                // 장비가 등록돼 있는데 쓸 수 있는 것이 없을 때(연결 실패 · 범위 밖)만 알린다.
                if (activated.Count == 0 && _equipments.Count > 0)
                    AppMessageBox.Warn(this, "사용 할 수 있는 너트러너가 없습니다.", Language.Pop_Title);
            }
            catch (Exception ex)
            {
                HandleError("너트러너 토크 설정", ex);
            }
        }

        /// <summary>
        /// 공정이 완료 · 중지 상태가 아닐 때(<see cref="IsInputAllowed"/>) "토크 결과값" · "각도 결과값" 칸의 편집기를 연다.
        /// 완료 · 중지 상태에서는 입력 자체를 막는다([리웍]으로 진행 중이 되면 열린다) —
        /// 그래서 ValidatingEditor · CellValueChanged에서는 공정 상태를 다시 확인하지 않는다.
        /// [입력] 버튼 컬럼도 편집기가 열려야 눌리므로 같은 조건으로 막힌다.
        /// </summary>
        private void GridViewBomSpec_ShowingEditor(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var fieldName = gridViewBomSpec.FocusedColumn?.FieldName;
            if (fieldName != TorqueResultField && fieldName != AngleResultField && fieldName != ResultInputField)
            {
                e.Cancel = true;
                return;
            }

            // 참조 줄(서버 refLtr=AR·REF)은 입력 대상이 아니다 — 결과값 칸 · [입력] 버튼 모두 열지 않는다.
            if (gridViewBomSpec.GetFocusedRow() is PopBomSpecDto spec && spec.IsReference)
            {
                e.Cancel = true;
                return;
            }

            if (!IsInputAllowed)
            {
                e.Cancel = true;
                ShowMessage(Language.Pop_InputNotAllowed);
            }
        }

        /// <summary>
        /// "토크 결과값" · "각도 결과값" 칸에 들어온 값이 실수인지 확인하고, 통과하면 서버에 저장한다.
        /// 토크는 소수 둘째 자리에서 올려 저장하고, 각도는 입력한 값 그대로 저장한다.
        /// 빈칸(또는 "-")은 "아직 측정하지 않음"(null)으로 되돌린다 — 잘못 넣은 값을 지울 수 있어야 한다.
        /// </summary>
        private async void GridViewBomSpec_ValidatingEditor(object sender, BaseContainerValidateEditorEventArgs e)
        {
            var fieldName = gridViewBomSpec.FocusedColumn?.FieldName;
            if (fieldName != TorqueResultField && fieldName != AngleResultField)
                return;

            var text = Convert.ToString(e.Value, CultureInfo.CurrentCulture)?.Trim();

            if (string.IsNullOrEmpty(text) || text == "-")
            {
                e.Value = null;
                return;
            }

            // double 로 받아 보고 나서 decimal 로 옮긴다 — 화면 입력은 실수 표기(1e3 · 소수점)를 모두 허용하되,
            // 저장 값은 자릿수 오차가 없는 decimal 로 유지하기 위함이다(DTO 타입도 decimal?).
            if (!double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out var parsed)
                || double.IsNaN(parsed) || double.IsInfinity(parsed))
            {
                e.Valid = false;
                e.ErrorText = Language.PopTorque_Invalid;
                return;
            }

            bool isTorque = fieldName == TorqueResultField;
            decimal value;
            try
            {
                value = isTorque ? CeilTorque((decimal)parsed) : (decimal)parsed;
            }
            catch (OverflowException)
            {
                // decimal 범위를 넘는 값(1e30 등) — 토크 · 각도 값으로 의미가 없으므로 숫자 오류와 같이 다룬다.
                e.Valid = false;
                e.ErrorText = Language.PopTorque_Invalid;
                return;
            }

            var view = (GridView)sender;
            var spec = view.GetRow(view.FocusedRowHandle) as PopBomSpecDto;
            if (spec == null)
                return;

            // e.Value는 await 전에 정해야 한다 — 첫 await에서 이벤트가 끝나 그리드가 그 시점의 값을 셀에 반영한다.
            e.Value = value;

            // 고친 칸만 새 값으로 바꾸고 다른 쪽은 지금 들고 있는 값을 그대로 보낸다.
            // 토크만 고쳤는데 각도가 아직 비어 있으면 수동 렌치와 같이 목표 각도를 채운다.
            decimal? torque = isTorque ? value : spec.TorqueResult;
            decimal? angle = isTorque ? (spec.AngleResult ?? spec.AngleTarget) : value;

            if (isTorque)
                spec.AngleResult = angle;
            spec.IsManualInput = true;
            spec.EquipmentCode = ManualEquipmentCode;

            try
            {
                if (!await Screen.SetBomSpecResult(spec, torque, angle, ManualEquipmentCode))
                    return;

                AppLog.Info($"[POP] {(isTorque ? "토크" : "각도")} 결과값 직접 입력 저장 - {spec.BomSpecName}/{spec.InputPosition} " +
                            $"(토크 {torque}, 각도 {angle})");

                // 첫 await에서 그리드가 입력값을 행에 반영했으므로, 여기서는 고친 값까지 포함해 완료를 판단한다.
                Screen.ShowInspectionTabIfBomDone();
                await Screen.TryAutoProgress();
            }
            catch (Exception ex)
            {
                HandleError("BOM 결과 직접 입력", ex);
            }
        }

        /// <summary>
        /// Ctrl+C(Ctrl+Insert) = 포커스 칸 하나의 값 복사, Ctrl+V(Shift+Insert) = 입력 칸에만 붙여넣기.
        ///  - 복사: DevExpress 기본 동작은 <b>줄 전체</b>(머리글 포함)를 복사한다. 자재코드 하나만 옮겨 쓰려는
        ///    작업자에게는 쓸 수 없는 모양이라, 고른 칸의 값만 넣는다(<see cref="CopyBomSpecCell"/>).
        ///  - 붙여넣기: "토크 결과값" · "각도 결과값" 칸에서만 받는다(<see cref="PasteBomSpecCell"/>).
        ///    그 밖의 칸은 키를 여기서 삼켜 아무 일도 일어나지 않게 한다.
        /// 편집기가 열려 있으면 키가 편집기로 가서 편집기 안의 글자를 복사 · 붙여넣으므로 여기로 오지 않는다.
        /// </summary>
        private void GridViewBomSpec_KeyDown(object sender, KeyEventArgs e)
        {
            var isCopy = e.KeyData == (Keys.Control | Keys.C) || e.KeyData == (Keys.Control | Keys.Insert);
            var isPaste = e.KeyData == (Keys.Control | Keys.V) || e.KeyData == (Keys.Shift | Keys.Insert);
            if (!isCopy && !isPaste)
                return;

            // 편집 중인 칸의 복사 · 붙여넣기는 편집기 몫이다 — 혹시 여기로 와도 건드리지 않는다.
            if (gridViewBomSpec.IsEditing)
                return;

            // 그리드의 기본 처리(줄 전체 복사)를 막는다. 뒤따르는 문자 메시지(Ctrl+V = 0x16)도 함께 지운다 —
            // 남겨 두면 방금 연 편집기로 넘어가 한 번 더 붙여넣어진다.
            e.SuppressKeyPress = true;

            if (!gridViewBomSpec.IsDataRow(gridViewBomSpec.FocusedRowHandle))
                return;

            try
            {
                if (isCopy)
                    CopyBomSpecCell();
                else
                    PasteBomSpecCell();
            }
            catch (ExternalException ex)
            {
                // 다른 프로그램이 클립보드를 잡고 있으면 열지 못한다 — 작업을 막을 일이 아니라 기록만 남긴다.
                AppLog.Warn($"[POP] BOM 스팩 {(isCopy ? "복사" : "붙여넣기")} 실패 - 클립보드를 열 수 없음: {ex.Message}");
            }
        }

        /// <summary>
        /// 포커스 칸에 <b>보이는 글자</b>를 클립보드에 넣는다(숫자는 화면 서식 그대로).
        /// 빈 칸이면 클립보드를 비운다 — 앞서 복사해 둔 값이 남아 있으면 "빈 칸을 복사했다"고 생각한 작업자가
        /// 결과값 칸에 붙여넣을 때 엉뚱한 옛 값이 들어가 서버에 저장된다.
        /// [입력] 버튼 칸은 값이 있는 칸이 아니라 건드리지 않는다.
        /// </summary>
        private void CopyBomSpecCell()
        {
            var column = gridViewBomSpec.FocusedColumn;
            if (column == null || column.FieldName == ResultInputField)
                return;

            var text = gridViewBomSpec.GetFocusedDisplayText();
            if (string.IsNullOrEmpty(text))
                Clipboard.Clear();
            else
                Clipboard.SetDataObject(text, true, ClipboardRetryCount, ClipboardRetryDelayMs);
        }

        /// <summary>
        /// 입력 칸("토크 결과값" · "각도 결과값")에 클립보드 글자를 넣는다. 값을 칸에 바로 쓰지 않고
        /// <b>편집기를 열어</b> 넣는다 — 열 수 있는지(참조 줄 · 공정 상태)는 ShowingEditor가,
        /// 숫자 검사 · 서버 저장은 ValidatingEditor가 손으로 쳤을 때와 똑같이 맡는다.
        /// 입력 칸이 아니면 아무것도 하지 않는다.
        /// </summary>
        private void PasteBomSpecCell()
        {
            var fieldName = gridViewBomSpec.FocusedColumn?.FieldName;
            if (fieldName != TorqueResultField && fieldName != AngleResultField)
                return;

            if (!Clipboard.ContainsText())
                return;

            gridViewBomSpec.ShowEditor();

            // ShowingEditor가 거절했으면 편집기가 열리지 않는다(안내는 그쪽에서 이미 했다).
            var editor = gridViewBomSpec.ActiveEditor as DevExpress.XtraEditors.TextEdit;
            if (editor == null)
                return;

            // 들어 있던 값 뒤에 이어 붙지 않게 전체를 고른 뒤 바꿔 넣는다.
            editor.SelectAll();
            editor.Paste();
        }

        /// <summary>
        /// 값 변환에 실패했을 때 DevExpress 기본 오류 창 대신 셀 안의 오류 표시로 알린다
        /// (창을 띄우면 작업자가 확인을 누를 때까지 다음 줄로 넘어가지 못한다).
        /// </summary>
        private void GridViewBomSpec_InvalidValueException(object sender, InvalidValueExceptionEventArgs e)
        {
            var fieldName = gridViewBomSpec.FocusedColumn?.FieldName;
            if (fieldName != TorqueResultField && fieldName != AngleResultField)
                return;

            e.ExceptionMode = ExceptionMode.DisplayError;
            e.ErrorText = Language.PopTorque_Invalid;
        }

        /// <summary>작업자가 직접 고친 줄은 수동 입력으로 표시한다 — 서버가 입력 경로(설비/사람)를 구분해 저장한다.</summary>
        private void GridViewBomSpec_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName != TorqueResultField && e.Column.FieldName != AngleResultField)
                return;

            var spec = gridViewBomSpec.GetRow(e.RowHandle) as PopBomSpecDto;
            if (spec == null)
                return;

            spec.IsManualInput = true;
            spec.EquipmentCode = ManualEquipmentCode;
            AppLog.Info($"[POP] 토크 결과값 직접 입력 - {spec.BomSpecName}/{spec.InputPosition} = {spec.TorqueResult}");
        }

        /// <summary>
        /// 결과상태가 OK면 OK 색, NG면 NG 색을 <b>결과 칸에만</b> 칠한다(표시만 한다 — 판정은 서버).
        ///  - 정량: 결과값 · 결과상태 두 칸 — 결과값이 판정 근거라 함께 보여 준다.
        ///  - 정성: 결과상태 칸만 — 결과값 칸은 쓰지 않는다.
        /// 줄 전체를 칠하면 어느 칸이 판정 대상인지 구분이 안 돼서 결과 칸으로 좁혔다.
        /// 칸 정렬을 건드리지 않고, 선택 행도 결과 색을 유지하고 선택은 테두리로 표시한다(<see cref="AppTheme.ResultCellSelection"/> 참고).
        /// </summary>
        private void GridViewInspection_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            if (e.RowHandle < 0 || e.Column == null)
                return;

            var spec = gridViewInspection.GetRow(e.RowHandle) as PopInspectionSpecDto;
            if (spec == null)
                return;

            var fieldName = e.Column.FieldName;
            var isResultCell = fieldName == "ResultState" || (spec.IsQuantitative && fieldName == "ResultValue");
            if (!isResultCell)
                return;

            if (spec.ResultState == PopInspectionSpecDto.ResultNg)
                AppTheme.NgRowCell(gridViewInspection, e);
            else if (spec.ResultState == PopInspectionSpecDto.ResultOk)
                AppTheme.OkRowCell(gridViewInspection, e);
        }

        /// <summary>
        /// 측정값이 들어온 <b>결과값 칸만</b> 칠한다 — 토크 결과값은 토크 판정으로, 각도 결과값은 각도 판정으로
        /// (하한~상한 안이면 OK 색, 벗어나면 NG 색). 줄 전체를 칠하면 토크 · 각도 중 어느 쪽이 NG인지 보이지 않는다.
        ///  - 각도 하한 · 상한이 둘 다 없는 줄은 각도 판정 대상이 아니라 각도 칸을 칠하지 않는다(<see cref="IsBomSpecPassed"/>와 같은 기준).
        ///  - 참조 줄(서버 refLtr=AR·REF)은 측정값 없이 OK로 끝난 줄이라 값과 상관없이 결과값 칸만 OK 색으로 둔다(줄 전체는 칠하지 않는다).
        /// 칸 정렬을 건드리지 않고, 선택 행도 결과 색을 유지하고 선택은 테두리로 표시한다(<see cref="AppTheme.ResultCellSelection"/> 참고).
        /// </summary>
        private void GridViewBomSpec_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            if (e.RowHandle < 0 || e.Column == null)
                return;

            var spec = gridViewBomSpec.GetRow(e.RowHandle) as PopBomSpecDto;
            if (spec == null)
                return;

            var fieldName = e.Column.FieldName;
            if (spec.IsReference)
            {
                if (fieldName == TorqueResultField || (fieldName == AngleResultField && IsAngleManaged(spec)))
                    AppTheme.OkRowCell(gridViewBomSpec, e);
                return;
            }

            if (fieldName == TorqueResultField && spec.TorqueResult != null)
                ResultCell(gridViewBomSpec, e, IsTorquePassed(spec));
            else if (fieldName == AngleResultField && spec.AngleResult != null && IsAngleManaged(spec))
                ResultCell(gridViewBomSpec, e, IsAnglePassed(spec));
        }

        /// <summary>판정 결과에 따라 칸 하나를 OK / NG 색으로 칠한다.</summary>
        private void ResultCell(GridView view, RowCellStyleEventArgs e, bool passed)
        {
            if (passed)
                AppTheme.OkRowCell(view, e);
            else
                AppTheme.NgRowCell(view, e);
        }

        /// <summary>
        /// [입력] 버튼 → 누른 줄과 <b>같은 자재코드(MaterialCode)</b>인 줄을 한 번에 채운다(일괄 입력).
        /// 각 줄의 토크 결과값은 하한~상한의 중간값, 각도 결과값은 목표 각도 — 수동 렌치로 조립할 때는 규격 한가운데를
        /// 목표로 조이므로 같은 자재의 체결 점마다 숫자를 다시 치는 과정을 줄인 것이다.
        ///  - 참조 줄(서버 refLtr=AR·REF)은 입력 대상이 아니라 건너뛴다.
        ///  - 이미 값(토크 또는 각도)이 들어 있는 줄은 건너뛴다 — 설비 · 직접 입력으로 넣은 실측값을 덮어쓰지 않는다.
        ///  - 토크 하한/상한이 없는 줄도 중간값을 낼 수 없어 건너뛴다.
        ///  - 채운 값은 직접 입력과 같이 수동 입력(<see cref="ManualEquipmentCode"/>)으로 서버에 저장한다.
        ///  - 끝나면 그 자재의 마지막 줄 <b>바로 다음 줄</b>로 포커스를 옮긴다(다음 자재로 넘어가는 순서 — 마지막이면 그대로).
        /// 수동 렌치가 꺼져 있으면 동작하지 않는다(컬럼도 숨겨진다 — <see cref="SetResultInputColumnVisible"/>).
        /// 여기서 내는 값은 <b>입력 편의를 위한 기본값</b>일 뿐이고(합·불 판정은 서버 몫),
        /// 다르게 넣어야 하면 "토크 결과값" 칸을 눌러 직접 고쳐 쓰면 된다.
        /// </summary>
        private async void ResultInput_ButtonClick(object sender, ButtonPressedEventArgs e)
        {
            if (!chkManualWrench.Checked)
            {
                AppMessageBox.Info(this, Language.Pop_ManualWrenchOnly, Language.PopTorque_Title);
                return;
            }

            var focused = gridViewBomSpec.GetFocusedRow() as PopBomSpecDto;
            if (focused == null)
                return;

            // [입력] 칸의 버튼 에디터가 열린 채로 다른 줄을 고치면 그리드가 포커스를 엉뚱하게 되돌린다.
            gridViewBomSpec.CloseEditor();

            var group = GetSameMaterialRows(focused.MaterialCode);
            var targets = new List<PopBomSpecDto>();
            var rangeMissing = false;
            foreach (var rowHandle in group)
            {
                var spec = (PopBomSpecDto)gridViewBomSpec.GetRow(rowHandle);
                if (spec.IsReference || spec.TorqueResult != null || spec.AngleResult != null)
                    continue;

                if (spec.TorqueLower == null || spec.TorqueUpper == null)
                {
                    rangeMissing = true;
                    continue;
                }

                targets.Add(spec);
            }

            if (targets.Count == 0)
            {
                AppLog.Info($"[POP] 수동 렌치 일괄 입력 - 대상 없음 ({focused.MaterialCode}, 같은 자재 {group.Count}줄, 범위 없음 {rangeMissing})");
                if (rangeMissing)
                    AppMessageBox.Info(this, Language.Pop_TorqueRangeMissing, Language.PopTorque_Title);
                FocusBomSpecRow(GetRowAfterMaterial(group));
                return;
            }

            try
            {
                foreach (var spec in targets)
                {
                    var torque = CeilTorque((spec.TorqueLower.Value + spec.TorqueUpper.Value) / 2m);
                    var angle = spec.AngleTarget;

                    spec.TorqueResult = torque;
                    spec.AngleResult = angle;
                    spec.IsManualInput = true;
                    spec.EquipmentCode = ManualEquipmentCode;
                    RefreshBomSpecGrid();

                    // 실패하면 그 줄에서 멈춘다 — 사유는 SetBomSpecResult가 창으로 알린다.
                    if (!await Screen.SetBomSpecResult(spec, torque, angle, ManualEquipmentCode))
                        return;

                    AppLog.Info($"[POP] 수동 렌치 일괄 입력 - {spec.MaterialCode} {spec.BomSpecName}/{spec.InputPosition} = {torque} " +
                                $"(하한 {spec.TorqueLower} ~ 상한 {spec.TorqueUpper}, 각도 {angle})");
                }

                if (IsDisposed || Disposing) return;

                FocusBomSpecRow(GetRowAfterMaterial(GetSameMaterialRows(focused.MaterialCode)));
                Screen.ShowInspectionTabIfBomDone();
                await Screen.TryAutoProgress();
            }
            catch (Exception ex)
            {
                HandleError("수동 렌치 일괄 입력", ex);
            }
        }

        /// <summary>[BOM 스팩] 그리드에서 자재코드가 같은 줄의 행 핸들(화면 순서).</summary>
        private List<int> GetSameMaterialRows(string materialCode)
        {
            var rows = new List<int>();
            for (int rowHandle = 0; rowHandle < gridViewBomSpec.RowCount; rowHandle++)
            {
                if (gridViewBomSpec.GetRow(rowHandle) is PopBomSpecDto spec
                    && string.Equals(spec.MaterialCode, materialCode, StringComparison.OrdinalIgnoreCase))
                    rows.Add(rowHandle);
            }

            return rows;
        }

        /// <summary>
        /// 같은 자재 줄 중 마지막 줄 다음의 첫 입력 대상 줄(참조 줄은 건너뛴다). 그런 줄이 없으면 마지막 줄 그대로.
        /// </summary>
        private int GetRowAfterMaterial(List<int> sameMaterialRows)
        {
            if (sameMaterialRows.Count == 0)
                return gridViewBomSpec.FocusedRowHandle;

            var last = sameMaterialRows[sameMaterialRows.Count - 1];
            var next = FindNextBomSpecInputRow(last);
            return next >= 0 ? next : last;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  [검사 스팩] 그리드 이벤트
        // ─────────────────────────────────────────────────────────────────────────

        private void GridViewInspection_ShowingEditor(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var view = (GridView)sender;
            var fieldName = view.FocusedColumn?.FieldName;
            if (fieldName == null || !_inspectionInputFields.Contains(fieldName))
            {
                e.Cancel = true;
                return;
            }

            // 완료 · 중지된 공정은 입력을 막는다([리웍]으로 진행 중이 되면 열린다). 대기 상태에서 받을지는 서버가 판정한다.
            // 칸을 누를 때마다 불리므로 창 대신 안내 칸에만 남긴다.
            if (!IsInputAllowed)
            {
                e.Cancel = true;
                ShowMessage(Language.Pop_InputNotAllowed);
                return;
            }

            var spec = view.GetFocusedRow() as PopInspectionSpecDto;
            if (spec == null)
                return;

            // 정성 검사는 결과값이 없다 — 결과상태(OK/NG)만 받는다.
            if (fieldName == "ResultValue" && !spec.IsQuantitative)
                e.Cancel = true;

            // 정량 검사의 판정이 결과값으로 정해졌으면 작업자가 바꿀 수 없다 — 결과값을 고쳐야 판정이 바뀐다.
            if (fieldName == "ResultState" && GetAutoResultState(spec) != null)
                e.Cancel = true;
        }

        /// <summary>검사방법 코드를 화면 언어로 표시한다 — QNT면 정량, 그 외는 정성.</summary>
        private void GridViewInspection_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column?.FieldName != "InspectionMethod" || e.ListSourceRowIndex < 0)
                return;

            var quantitative = string.Equals(e.Value as string, PopInspectionSpecDto.MethodQuantitative, StringComparison.OrdinalIgnoreCase);
            e.DisplayText = quantitative ? Language.Pop_InspectionQuantitative : Language.Pop_InspectionQualitative;
        }

        /// <summary>
        /// 셀 값이 바뀌면 그 줄을 "보낼 줄"로 표시만 한다 — 서버 전송은 줄을 떠날 때(<see cref="GridViewInspection_FocusedRowChanged"/>).
        /// 정량 검사의 결과값이 바뀌면 하한 · 상한(또는 기준 문자)으로 판정을 다시 채운다.
        /// </summary>
        private void gridViewInspection_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            var fieldName = e.Column?.FieldName;
            if (fieldName == null || !_inspectionInputFields.Contains(fieldName))
                return;

            var view = (GridView)sender;
            var spec = view.GetRow(e.RowHandle) as PopInspectionSpecDto;
            if (spec == null)
                return;

            if (fieldName == "ResultValue" && spec.IsQuantitative)
                ApplyAutoResultState(view, e.RowHandle, spec);

            _pendingInspections.Add(spec);
        }

        /// <summary>
        /// 정량 검사의 판정을 결과값으로 채운다(<see cref="GetAutoResultState"/>).
        /// 판정 칸에 직접 넣는다 — SetRowCellValue로 넣으면 CellValueChanged가 한 번 더 올라온다.
        /// 기준(하한 · 상한)이 있는 항목에서 결과값을 지우거나 숫자가 아니게 고치면 판정도 비운다 —
        /// 앞서 결과값으로 정해진 판정이 그대로 남아 잠긴 채 서버로 나가지 않게 하기 위해서다.
        /// </summary>
        private void ApplyAutoResultState(GridView view, int rowHandle, PopInspectionSpecDto spec)
        {
            var auto = GetAutoResultState(spec);
            if (auto != null)
            {
                spec.ResultState = auto;
                AppLog.Info($"[POP] 검사 결과값으로 판정 - {auto} ({spec.InspectionItemCode}, {spec.ResultValue})");
            }
            else if (spec.InspectionLower.HasValue || spec.InspectionUpper.HasValue)
            {
                spec.ResultState = null;
            }

            view.RefreshRow(rowHandle);
        }

        /// <summary>
        /// 결과값으로 정해지는 정량 검사의 판정. 정할 수 없으면 null — 그때는 작업자가 판정을 직접 고른다.
        /// 1) 결과값이 기준 문자(stdLtr, 예: 저항 측정의 "OL")와 같으면 OK.
        /// 2) 결과값이 숫자이고 하한 · 상한 중 하나라도 있으면, 있는 기준 안(경계 포함)이면 OK · 벗어나면 NG.
        /// </summary>
        private string GetAutoResultState(PopInspectionSpecDto spec)
        {
            if (!spec.IsQuantitative || string.IsNullOrWhiteSpace(spec.ResultValue))
                return null;

            if (IsStdLtrMatched(spec))
                return PopInspectionSpecDto.ResultOk;

            if (!spec.InspectionLower.HasValue && !spec.InspectionUpper.HasValue)
                return null;

            if (!TryParseResultValue(spec.ResultValue, out var value))
                return null;

            var lowerOk = !spec.InspectionLower.HasValue || value >= spec.InspectionLower.Value;
            var upperOk = !spec.InspectionUpper.HasValue || value <= spec.InspectionUpper.Value;
            return lowerOk && upperOk ? PopInspectionSpecDto.ResultOk : PopInspectionSpecDto.ResultNg;
        }

        /// <summary>
        /// 결과값 문자열을 숫자로 읽는다. 가상 키패드는 소수점을 "."으로 넣으므로 고정 문화권으로 먼저 읽고,
        /// 안 되면 화면 문화권(베트남어는 소수점이 ",")으로 다시 읽는다.
        /// </summary>
        private bool TryParseResultValue(string text, out decimal value)
        {
            var trimmed = text.Trim();
            return decimal.TryParse(trimmed, NumberStyles.Number, CultureInfo.InvariantCulture, out value)
                || decimal.TryParse(trimmed, NumberStyles.Number, CultureInfo.CurrentCulture, out value);
        }

        /// <summary>
        /// 작업자가 줄을 옮기면 떠난 줄(이전 포커스 줄)에 고친 것이 있을 때 서버에 보낸다.
        /// 새로 선택된 줄이 아니라 <b>떠난 줄</b>을 보낸다.
        /// 다시 바인딩할 때 올라오는 이벤트는 <see cref="_pendingInspections"/>가 비어 있어 아무것도 보내지 않는다.
        /// </summary>
        private async void GridViewInspection_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            if (e.PrevFocusedRowHandle == e.FocusedRowHandle || !gridViewInspection.IsDataRow(e.PrevFocusedRowHandle))
                return;

            var spec = gridViewInspection.GetRow(e.PrevFocusedRowHandle) as PopInspectionSpecDto;
            if (spec == null || !_pendingInspections.Contains(spec))
                return;

            // 결과상태가 없으면 비고 · 결과값이 있어도 보내지 않는다. 정량 검사는 결과값까지 있어야 보낸다.
            // 보낼 줄 표시는 남겨 둔다 — 나중에 판정을 채우고 다시 떠날 때 함께 보낸다.
            if (!IsInspectionEntered(spec))
            {
                AppLog.Info($"[POP] 검사 결과상태/결과값 미입력 - 전송 안 함 ({spec.InspectionItemCode})");
                return;
            }

            _pendingInspections.Remove(spec);
            try
            {
                if (!await Screen.SetInspectionResult(spec, "row"))
                    KeepPendingInspection(spec);
            }
            catch (Exception ex)
            {
                KeepPendingInspection(spec);
                HandleError("검사 결과 전송", ex);
            }
        }

        /// <summary>
        /// 보내지 못한 검사 줄을 다시 "보낼 줄"로 남긴다 — 단 지금 검사 탭의 줄일 때만.
        /// 전송이 409로 거절되면 공정 정보를 다시 읽어(<see cref="SendInspectionResult"/>) 검사 탭이 새 줄로 바뀌는데,
        /// 옛 줄을 다시 넣으면 화면에 없는 줄이 남아 공정 이동 때(<see cref="SavePendingInspections"/>) 옛 값이 다시 나간다.
        /// </summary>
        private void KeepPendingInspection(PopInspectionSpecDto spec)
        {
            if (_inspections.Contains(spec))
                _pendingInspections.Add(spec);
        }

        /// <summary>검사 스팩 한 줄이 서버에 저장까지 끝났는지 — 입력됐어도 아직 보내지 않은 줄은 완료가 아니다.</summary>
        private bool IsInspectionSaved(PopInspectionSpecDto spec)
        {
            return IsInspectionEntered(spec) && !_pendingInspections.Contains(spec);
        }

        /// <summary>
        /// 결과값이 서버가 준 기준 문자(stdLtr)와 같은지 — 양쪽 다 앞뒤 공백을 빼고 대문자로 바꿔 비교한다
        /// (" ol"도 "OL"로 본다). 기준 문자가 없는 항목은 비교하지 않는다.
        /// </summary>
        private bool IsStdLtrMatched(PopInspectionSpecDto spec)
        {
            if (string.IsNullOrWhiteSpace(spec.InspectionStdLtr) || string.IsNullOrWhiteSpace(spec.ResultValue))
                return false;

            return string.Equals(
                spec.ResultValue.Trim().ToUpperInvariant(),
                spec.InspectionStdLtr.Trim().ToUpperInvariant(),
                StringComparison.Ordinal);
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  가상 키보드 — [BOM 스팩] · [검사 스팩] 값 입력 칸
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// 열린 셀 편집기에 맞는 자판 — 숫자 형식 셀이면 숫자 키패드, 문자열 셀이면 문자 키보드, 대상이 아니면 null.
        ///
        /// 글자를 치는 편집기(TextEdit)만 대상이다. [입력] 버튼 · 판정(OK/NG) 콤보처럼 누르기만 하면 되는 편집기는
        /// ButtonEdit 계열(콤보도 ButtonEdit를 상속한다)이라 여기서 빠진다.
        /// 편집기가 열렸다는 것 자체가 ShowingEditor의 입력 조건(공정 상태 · 정량/정성)을 통과했다는 뜻이라 여기서 다시 보지 않는다.
        /// </summary>
        private VirtualKeyboardMode? GetKeyboardMode(GridView view)
        {
            var editor = view.ActiveEditor;
            var column = view.FocusedColumn;
            if (column == null || !(editor is DevExpress.XtraEditors.TextEdit) || editor is DevExpress.XtraEditors.ButtonEdit || editor.Properties.ReadOnly)
                return null;

            return IsNumericCell(view, column) ? VirtualKeyboardMode.Numeric : VirtualKeyboardMode.Text;
        }

        /// <summary>
        /// 숫자를 넣는 셀인지 — 컬럼의 데이터 형식으로 판단한다(토크 · 각도 결과값은 decimal?).
        /// 검사 결과값은 서버 필드가 문자열이지만 정량 검사에서는 측정 숫자를 넣는 칸이라 숫자로 본다
        /// (정성 검사는 결과값 칸 자체가 막혀 있다 — <see cref="GridViewInspection_ShowingEditor"/>).
        /// </summary>
        private bool IsNumericCell(GridView view, DevExpress.XtraGrid.Columns.GridColumn column)
        {
            // 기준 문자(stdLtr, 예: "OL")가 있는 항목은 글자를 쳐야 하므로 문자 키보드를 띄운다.
            if (view == gridViewInspection && column.FieldName == "ResultValue")
            {
                var spec = view.GetFocusedRow() as PopInspectionSpecDto;
                return spec != null && spec.IsQuantitative && string.IsNullOrWhiteSpace(spec.InspectionStdLtr);
            }

            var type = Nullable.GetUnderlyingType(column.ColumnType) ?? column.ColumnType;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 글자를 치는 편집기가 열리면 Windows 화상 키보드(OSK)를 띄운다.
        /// OSK는 포커스를 가져가지 않으므로 열린 편집기에 바로 글자가 들어가고, 편집을 닫을 때
        /// ValidatingEditor(숫자 검사 · 서버 저장)와 CellValueChanged(검사 결과 전송)가 직접 타이핑했을 때와 똑같이 돈다.
        /// 자체 가상 키보드(<see cref="VirtualKeyboardForm"/>)는 코드로 남겨 두되 여기서 띄우지 않는다.
        /// </summary>
        private void GridView_ShownEditor(object sender, EventArgs e)
        {
            var view = (GridView)sender;
            if (GetKeyboardMode(view) == null)
                return;

            OnScreenKeyboard.Show();
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  [연결 장비] 그리드 이벤트
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// 연결 실패/사용불가 상태는 붉은 글씨로 눈에 띄게 한다(상태 표시일 뿐 판정은 아니다).
        /// 연결 중(이벤트 수신 확인 대기)은 곧 바뀔 상태라 실패와 구분해 주황으로 둔다.
        /// </summary>
        private void GridViewEquipment_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            if (e.Column.FieldName != "CommunicationState" && e.Column.FieldName != "EquipmentState")
                return;

            var equipment = gridViewEquipment.GetRow(e.RowHandle) as PopEquipmentDto;
            if (equipment == null)
                return;

            if (equipment.CommunicationState == Language.Pop_CommConnected)
                e.Appearance.ForeColor = AppTheme.Emerald;
            else if (equipment.CommunicationState == Language.Pop_CommConnecting)
                e.Appearance.ForeColor = AppTheme.Amber;
            else
                e.Appearance.ForeColor = AppTheme.Rose;
        }

        /// <summary>
        /// 재연결 대상 = 너트러너로 등록돼 있는데 연결이 끊긴(연결 실패) 장비.
        /// 등록되지 않은 장비는 <see cref="RefreshEquipmentStates"/>가 상태를 채우지 않으므로 여기에 걸리지 않는다.
        /// </summary>
        private bool CanReconnect(PopEquipmentDto? equipment)
        {
            return equipment != null && equipment.CommunicationState == Language.Pop_CommFailed;
        }

        private void GridViewEquipment_CustomRowCellEdit(object sender, CustomRowCellEditEventArgs e)
        {
            if (e.Column.FieldName != ReconnectField)
                return;

            var equipment = gridViewEquipment.GetRow(e.RowHandle) as PopEquipmentDto;
            if (CanReconnect(equipment))
                e.RepositoryItem = _reconnectEditor;
            else
                e.RepositoryItem = _reconnectBlankEditor;
        }

        private void GridViewEquipment_ShowingEditor(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (gridViewEquipment.FocusedColumn?.FieldName != ReconnectField)
            {
                e.Cancel = true;
                return;
            }

            if (!CanReconnect(gridViewEquipment.GetFocusedRow() as PopEquipmentDto))
                e.Cancel = true;
        }

        /// <summary>
        /// [재연결] → 그 장비의 통신 시퀀스를 다시 시작한다.
        /// 시퀀스는 재연결을 3회 실패하면 스스로 멈추므로, 끊긴 장비를 다시 붙이려면 시퀀스를 새로 띄워야 한다
        /// (<c>Start</c>가 남은 스레드를 정리하고 재시도 횟수를 0으로 돌린 뒤 바로 접속을 시도한다).
        /// 결과(연결 성공/실패)는 ConnectionChanged를 타고 <see cref="RefreshEquipmentStates"/>로 이 그리드에 반영된다.
        /// </summary>
        private void Reconnect_ButtonClick(object sender, ButtonPressedEventArgs e)
        {
            var equipment = gridViewEquipment.GetFocusedRow() as PopEquipmentDto;
            if (equipment == null || !CanReconnect(equipment) || _equip?.Manager == null)
                return;

            gridViewEquipment.CloseEditor();

            try
            {
                if (!_equip.Manager.TryGet(equipment.EquipmentCode, out INutrunner nutrunner) || nutrunner == null)
                {
                    AppMessageBox.Warn(this, ErrorText.WithCode(Language.Pop_ReconnectFailed, ErrorCodes.EquipmentReconnectFailed), Language.Pop_Title);
                    return;
                }

                AppLog.Info($"[POP] 장비 재연결 시도 - {equipment.EquipmentCode}");
                _equip.Manager.Start(nutrunner.Id);
            }
            catch (Exception ex)
            {
                HandleError("장비 재연결", ex);
            }

            RefreshEquipmentStates();
        }
    }
}
