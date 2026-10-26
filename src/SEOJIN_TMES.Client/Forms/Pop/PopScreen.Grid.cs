using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using SEOJIN_TMES.Client.ApiClient;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Equipment.Nutrunner;
using SEOJIN_TMES.Client.Forms.Common;
using SEOJIN_TMES.Client.Theme;
using System;
using System.Collections.Generic;
using System.Globalization;

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
        }

        private void InitMaterialGrid()
        {
            // PopInputMaterialDto.SerialNumbers(컬렉션 속성)를 그리드가 마스터-디테일로 오인해
            // 행 왼쪽에 [+] 펼침 버튼을 만든다. 이 화면에는 하위 목록이 없으므로 꺼 둔다.
            gridViewMaterial.OptionsDetail.EnableMasterViewMode = false;

            gridMaterial.DataSource = new List<PopInputMaterialDto>();
            GridColumns.Apply(gridViewMaterial, Language.Col_No,
                GridColumns.Text("MaterialCode", Language.Col_MaterialCode, 200),
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
                GridColumns.Text("EquipmentCode", "결과동작", 130),
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

            SetGridLineColor(gridViewBomSpec);
        }

        private void InitInspectionGrid()
        {
            gridInspection.DataSource = new List<PopInspectionSpecDto>();
            GridColumns.Apply(gridViewInspection, Language.Col_No,
                GridColumns.Text("InspectionSpecName", Language.Col_InspectionSpecName, 220),
                GridColumns.Text("InputPosition", Language.Col_InputPosition, 110),
                GridColumns.Left("Description", Language.Col_Description, 300),
                GridColumns.Text("InspectionMethod", Language.Col_InspectionMethod, 120),
                GridColumns.Text("ResultState", Language.Col_ResultState, 130),
                GridColumns.Text("ResultValue", Language.Col_ResultValue, 130),
                GridColumns.Text("InspectionResultRemark", Language.Col_InspectionRemark, 300));

            gridViewInspection.OptionsView.ColumnAutoWidth = true;

            _resultStateEditor = new RepositoryItemComboBox();
            _resultStateEditor.TextEditStyle = TextEditStyles.DisableTextEditor;
            _resultStateEditor.Items.Add(Language.Pop_ResultPass);
            _resultStateEditor.Items.Add(Language.Pop_ResultFail);
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

        /// <summary>서버가 한 번에 내려준 탭 4개 데이터를 그리드에 붙인다.</summary>
        public void SetGridData(PopScreenData screen)
        {
            _materials = screen.Materials ?? new List<PopInputMaterialDto>();
            _inspections = screen.InspectionSpecs ?? new List<PopInspectionSpecDto>();
            _equipments = screen.Equipments ?? new List<PopEquipmentDto>();

            gridMaterial.DataSource = _materials;
            gridInspection.DataSource = _inspections;
            gridEquipment.DataSource = _equipments;

            gridViewMaterial.RefreshData();
            gridViewInspection.RefreshData();
            gridViewEquipment.RefreshData();

            // 바인딩 중 포커스가 움직여도 너트러너에 토크를 쓰지 않는다.
            BeginBomSpecRedraw();
            try
            {
                _bomSpecs = screen.BomSpecs ?? new List<PopBomSpecDto>();
                gridBomSpec.DataSource = _bomSpecs;
                gridViewBomSpec.RefreshData();
            }
            finally
            {
                EndBomSpecRedraw();
            }
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
        /// [연결 장비] 탭의 통신상태/장비상태를 현재 Modbus 연결 상태로 덮어쓴다.
        /// 표준 채널이 연결되고 이벤트 수신이 켜져 있어야 "연결 성공"으로 본다.
        /// </summary>
        public void RefreshEquipmentStates()
        {
            if (_equip?.Manager == null)
                return;

            foreach (var equipment in _equipments)
            {
                if (!_equip.Manager.TryGet(equipment.EquipmentCode, out INutrunner nutrunner) || nutrunner == null)
                    continue;

                if (nutrunner.IsConnected && nutrunner.IsEventEnabled)
                {
                    equipment.CommunicationState = Language.Pop_CommConnected;
                    equipment.EquipmentState = Language.Pop_EquipmentUsable;
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
        /// </summary>
        private bool IsBomSpecPassed(PopBomSpecDto spec)
        {
            var torqueOk = spec.TorqueResult >= spec.TorqueLower && spec.TorqueResult <= spec.TorqueUpper;
            var angleOk = spec.AngleResult >= spec.AngleLower && spec.AngleResult <= spec.AngleUpper;
            return torqueOk && angleOk;
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

        /// <summary>"시리얼번호 입력상태" 칸을 클릭하면 &lt;투입자재 정보 입력&gt; 팝업을 연다.</summary>
        private async void GridViewMaterial_RowCellClick(object sender, RowCellClickEventArgs e)
        {
            if (e.Column == null || e.Column.FieldName != SerialInputStateField)
                return;

            try
            {
                await Screen.InputMaterialSerial(e.RowHandle);
            }
            catch (Exception ex)
            {
                HandleError("투입자재 시리얼 입력", ex);
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  [BOM 스팩] 그리드 이벤트
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>작업자가 고른 행을 설비 체결 결과를 채울 행으로 삼는다.</summary>
        private void GridViewBomSpec_RowClick(object sender, RowClickEventArgs e)
        {
            if (!IsProcessRunning)
            {
                AppLog.Info($"[POP] 공정 미시작 - ( 상태 {_progressState})");
                return;
            }
            if (e.RowHandle < 0)
                return;

            _bomSpecSelectedRow = e.RowHandle;
            AppLog.Info($"[POP] 선택한 Row - ( Row 값 {_bomSpecSelectedRow})");
        }

        /// <summary>포커스가 옮겨 간 행의 목표 토크를 너트러너에 써 둔다.</summary>
        private void GridViewBomSpec_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            // 바인딩 · RefreshData로 다시 그리는 중에 생긴 포커스 이동은 작업자가 고른 것이 아니다.
            if (IsBomSpecRedrawing)
                return;
            if (!IsProcessRunning)
            {
                AppLog.Info($"[POP] 공정 미시작 - ( 상태 {_progressState})");
                return;
            }
            if (e.FocusedRowHandle < 0)
                return;

            var spec = gridViewBomSpec.GetRow(e.FocusedRowHandle) as PopBomSpecDto;
            if (spec?.TorqueTarget == null)
                return;

            _bomSpecSelectedRow = e.FocusedRowHandle;
            AppLog.Info($"[POP] FocusedRow - ( value {_bomSpecSelectedRow})");

            try
            {
                // 목표 토크는 서버 기준정보 값을 그대로 쓴다(클라이언트가 계산하지 않는다).
                if (Screen.WriteTorqueToEquipment(spec.TorqueTarget.Value) == null)
                    AppMessageBox.Warn(this, "사용 할 수 있는 너트러너가 없습니다.", Language.Pop_Title);
            }
            catch (Exception ex)
            {
                HandleError("너트러너 토크 설정", ex);
            }
        }

        /// <summary>
        /// 공정이 진행 중(<see cref="IsProcessRunning"/>)일 때만 "토크 결과값" · "각도 결과값" 칸의 편집기를 연다.
        /// 공정 전 · 중지 · 완료 상태에서 들어온 값은 실적이 아니므로 입력 자체를 막는다 —
        /// 그래서 ValidatingEditor · CellValueChanged에서는 공정 상태를 다시 확인하지 않는다.
        /// [입력] 버튼 컬럼도 편집기가 열려야 눌리므로 같은 조건으로 막힌다.
        /// </summary>
        private void GridViewBomSpec_ShowingEditor(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!IsProcessRunning)
            {
                e.Cancel = true;
                return;
            }

            var fieldName = gridViewBomSpec.FocusedColumn?.FieldName;
            if (fieldName != TorqueResultField && fieldName != AngleResultField && fieldName != ResultInputField)
                e.Cancel = true;
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
                await Screen.TryAutoProgress();
            }
            catch (Exception ex)
            {
                HandleError("BOM 결과 직접 입력", ex);
            }
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

        /// <summary>측정값이 있는데 하한~상한을 벗어난 줄은 붉게 칠한다(포커스 행 색보다 우선).</summary>
        private void GridViewBomSpec_RowStyle(object sender, RowStyleEventArgs e)
        {
            if (e.RowHandle < 0)
                return;

            var spec = gridViewBomSpec.GetRow(e.RowHandle) as PopBomSpecDto;
            if (spec == null || (spec.TorqueResult == null && spec.AngleResult == null))
                return;

            if (!IsBomSpecPassed(spec))
            {
                e.Appearance.BackColor = AppTheme.Rose;
                e.HighPriority = true;
            }
        }

        /// <summary>
        /// [입력] 버튼 → 그 줄의 토크 하한값과 상한값의 중간값을 토크 결과값에, 목표 각도를 각도 결과값에 채운다.
        /// 수동 렌치로 조립할 때는 규격 한가운데를 목표로 조이므로 숫자를 다시 치는 과정을 줄인 것이다.
        /// 여기서 내는 값은 <b>입력 편의를 위한 기본값</b>일 뿐이고(합·불 판정은 서버 몫),
        /// 다르게 넣어야 하면 "토크 결과값" 칸을 눌러 직접 고쳐 쓰면 된다.
        /// </summary>
        private void ResultInput_ButtonClick(object sender, ButtonPressedEventArgs e)
        {
            if (!chkManualWrench.Checked)
            {
                AppMessageBox.Info(this, Language.Pop_ManualWrenchOnly, Language.PopTorque_Title);
                return;
            }

            var spec = gridViewBomSpec.GetFocusedRow() as PopBomSpecDto;
            if (spec == null)
                return;

            if (spec.TorqueLower == null || spec.TorqueUpper == null)
            {
                AppMessageBox.Info(this, Language.Pop_TorqueRangeMissing, Language.PopTorque_Title);
                return;
            }

            spec.TorqueResult = CeilTorque((spec.TorqueLower.Value + spec.TorqueUpper.Value) / 2m);
            spec.AngleResult = spec.AngleTarget;
            spec.IsManualInput = true;
            spec.EquipmentCode = ManualEquipmentCode;

            RefreshBomSpecGrid();
            AppLog.Info($"[POP] 수동 렌치 중간값 입력 - {spec.BomSpecName}/{spec.InputPosition} = {spec.TorqueResult} " +
                        $"(하한 {spec.TorqueLower} ~ 상한 {spec.TorqueUpper})");
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  [검사 스팩] 그리드 이벤트
        // ─────────────────────────────────────────────────────────────────────────

        private void GridViewInspection_ShowingEditor(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 공정이 진행 중일 때만 결과를 받는다 — 대기·중지·완료 상태의 입력은 실적에 붙을 곳이 없다.
            if (!IsProcessRunning)
            {
                e.Cancel = true;
                return;
            }

            var view = (GridView)sender;
            var fieldName = view.FocusedColumn?.FieldName;
            if (fieldName == null || !_inspectionInputFields.Contains(fieldName))
            {
                e.Cancel = true;
                return;
            }

            // 정성 검사는 결과값이 없다 — 결과상태(합격/불합격)만 받는다.
            if (fieldName == "ResultValue")
            {
                var spec = view.GetFocusedRow() as PopInspectionSpecDto;
                if (spec != null && !spec.IsQuantitative)
                    e.Cancel = true;
            }
        }

        private async void gridViewInspection_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            var fieldName = e.Column?.FieldName;
            if (fieldName == null || !_inspectionInputFields.Contains(fieldName))
                return;

            var spec = ((GridView)sender).GetRow(e.RowHandle) as PopInspectionSpecDto;
            if (spec == null)
                return;

            try
            {
                await Screen.SetInspectionResult(spec, fieldName);
            }
            catch (Exception ex)
            {
                HandleError("검사 결과 전송", ex);
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  [연결 장비] 그리드 이벤트
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>연결 실패/사용불가 상태는 붉은 글씨로 눈에 띄게 한다(상태 표시일 뿐 판정은 아니다).</summary>
        private void GridViewEquipment_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            if (e.Column.FieldName != "CommunicationState" && e.Column.FieldName != "EquipmentState")
                return;

            var equipment = gridViewEquipment.GetRow(e.RowHandle) as PopEquipmentDto;
            if (equipment == null)
                return;

            var connected = equipment.CommunicationState == Language.Pop_CommConnected;
            e.Appearance.ForeColor = connected ? AppTheme.Emerald : AppTheme.Rose;
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
                    AppMessageBox.Warn(this, Language.Pop_ReconnectFailed, Language.Pop_Title);
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
