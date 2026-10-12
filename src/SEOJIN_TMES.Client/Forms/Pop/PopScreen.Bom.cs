using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using SEOJIN_TMES.Client.ApiClient;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Forms.Common;
using SEOJIN_TMES.Client.Mock;
using SEOJIN_TMES.Client.Theme;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace SEOJIN_TMES.Client.Forms.Pop
{
    public partial class PopScreen
    {

        /// <summary>[BOM 스팩] 탭의 "결과값 입력" 컬럼(데이터에 없는 화면 전용 컬럼)의 필드명.</summary>
        private const string ResultInputField = "__ResultInput";

        /// <summary>[BOM 스팩] 탭의 "토크 결과값" 컬럼 필드명(<see cref="PopBomSpecDto.TorqueResult"/>).</summary>
        private const string TorqueResultField = "TorqueResult";

        /// <summary>토크 결과값을 다루는 소수 자릿수 — 이 자리에서 올림해 표시하고 저장한다.</summary>
        private const int TorqueDecimals = 2;

        private List<PopBomSpecDto> _bomSpecs = new List<PopBomSpecDto>();

        private int _bomSpecSelectedRow = 0;

        private async Task<List<PopBomSpecDto>> FetchBomSpecsAsync()
        {
            //if (UseMock)
            //{
            //    await Task.Yield();
            //    return MockPopData.GetBomSpecs(_key);
            //}

            return await _appSet.ApiClients.PopApiClient.GetBomSpecsAsync(_key)
                   ?? new List<PopBomSpecDto>();
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

        ///// <summary>
        ///// [입력] 버튼 → 그 줄의 토크 하한값과 상한값의 중간값을 토크 결과값에 바로 채운다.
        /////
        ///// 예전에는 &lt;토크 결과값 입력&gt; 팝업을 띄웠는데, 수동 렌치로 조립할 때는
        ///// 어차피 규격 한가운데를 목표로 조이므로 창을 띄우고 숫자를 다시 치는 과정이 군더더기였다.
        ///// 여기서 내는 값은 <b>입력 편의를 위한 기본값</b>일 뿐이고(합·불 판정은 서버 몫),
        ///// 다르게 넣어야 하면 "토크 결과값" 칸을 눌러 직접 고쳐 쓰면 된다.
        ///// </summary>
        //private void ResultInput_ButtonClick(object sender, ButtonPressedEventArgs e)
        //{
        //    if (!chkManualWrench.Checked)
        //    {
        //        AppMessageBox.Info(this, Language.Pop_ManualWrenchOnly, Language.PopTorque_Title);
        //        return;
        //    }

        //    var spec = gridViewBomSpec.GetFocusedRow() as PopBomSpecDto;
        //    if (spec == null)
        //        return;

        //    if (spec.TorqueLower == null || spec.TorqueUpper == null)
        //    {
        //        AppMessageBox.Info(this, Language.Pop_TorqueRangeMissing, Language.PopTorque_Title);
        //        return;
        //    }

        //    spec.TorqueResult = CeilTorque((spec.TorqueLower.Value + spec.TorqueUpper.Value) / 2m);
        //    spec.IsManualInput = true;

        //    gridViewBomSpec.RefreshData();
        //    AppLog.Info($"[POP] 수동 렌치 중간값 입력 - {spec.BomSpecName}/{spec.InputPosition} = {spec.TorqueResult} " +
        //                $"(하한 {spec.TorqueLower} ~ 상한 {spec.TorqueUpper})");
        //}

        ///// <summary>
        ///// "토크 결과값" 칸에 들어온 값이 실수인지 확인하고, 통과하면 소수 둘째 자리에서 올려 저장한다.
        ///// 빈칸(또는 "-")은 "아직 측정하지 않음"(null)으로 되돌린다 — 잘못 넣은 값을 지울 수 있어야 한다.
        ///// </summary>
        //private void GridViewBomSpec_ValidatingEditor(object sender, BaseContainerValidateEditorEventArgs e)
        //{
        //    if (gridViewBomSpec.FocusedColumn?.FieldName != TorqueResultField)
        //        return;

        //    var text = Convert.ToString(e.Value, CultureInfo.CurrentCulture)?.Trim();

        //    if (string.IsNullOrEmpty(text) || text == "-")
        //    {
        //        e.Value = null;
        //        return;
        //    }

        //    // double 로 받아 보고 나서 decimal 로 옮긴다 — 화면 입력은 실수 표기(1e3 · 소수점)를 모두 허용하되,
        //    // 저장 값은 자릿수 오차가 없는 decimal 로 유지하기 위함이다(DTO 타입도 decimal?).
        //    if (!double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out var parsed)
        //        || double.IsNaN(parsed) || double.IsInfinity(parsed))
        //    {
        //        e.Valid = false;
        //        e.ErrorText = Language.PopTorque_Invalid;
        //        return;
        //    }

        //    try
        //    {
        //        e.Value = CeilTorque((decimal)parsed);
        //    }
        //    catch (OverflowException)
        //    {
        //        // decimal 범위를 넘는 값(1e30 등) — 토크 값으로 의미가 없으므로 숫자 오류와 같이 다룬다.
        //        e.Valid = false;
        //        e.ErrorText = Language.PopTorque_Invalid;
        //    }
        //}

        //private void GridViewBomSpec_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        //{
        //    if (e.FocusedRowHandle < 0)
        //        return;

        //    var value = gridViewBomSpec.GetRowCellValue(e.FocusedRowHandle, TorqueResultField);

        //    // 선택된 Row 처리
        //}

        //private void GridViewBomSpec_RowClick(object sender, RowClickEventArgs e)
        //{
        //    if (e.RowHandle < 0)
        //        return;

        //    // 클릭한 Row 처리
        //    //var row = gridViewBomSpec.GetRow(e.RowHandle);
        //    _bomSpecSelectedRow = e.RowHandle;
        //}


        ///// <summary>
        ///// 값 변환에 실패했을 때 DevExpress 기본 오류 창 대신 셀 안의 오류 표시로 알린다
        ///// (창을 띄우면 작업자가 확인을 누를 때까지 다음 줄로 넘어가지 못한다).
        ///// </summary>
        //private void GridViewBomSpec_InvalidValueException(object sender, InvalidValueExceptionEventArgs e)
        //{
        //    if (gridViewBomSpec.FocusedColumn?.FieldName != TorqueResultField)
        //        return;

        //    e.ExceptionMode = ExceptionMode.DisplayError;
        //    e.ErrorText = Language.PopTorque_Invalid;
        //}

        ///// <summary>
        ///// 작업자가 직접 고친 줄은 수동 입력으로 표시한다 — 서버가 입력 경로(설비/사람)를 구분해 저장한다.
        ///// </summary>
        //private void GridViewBomSpec_CellValueChanged(object sender, CellValueChangedEventArgs e)
        //{
        //    if (e.Column.FieldName != TorqueResultField)
        //        return;

        //    var spec = gridViewBomSpec.GetRow(e.RowHandle) as PopBomSpecDto;
        //    if (spec == null)
        //        return;

        //    spec.IsManualInput = true;
        //    AppLog.Info($"[POP] 토크 결과값 직접 입력 - {spec.BomSpecName}/{spec.InputPosition} = {spec.TorqueResult}");
        //}

        ///// <summary>
        ///// TorqueResult 필드의 특정 셀 색상을 변경합니다.
        ///// 토크 값이 범위를 벗어나면 셀의 글자 색상을 Rose(빨강)로 표시합니다.
        ///// </summary>
        //private void GridViewBomSpec_RowCellStyle(object sender, RowCellStyleEventArgs e)
        //{
        //    if (e.Column.FieldName != TorqueResultField)
        //        return;

        //    bool isTorqueRec = false;
        //    bool isAngleRec = false;
        //    var target = gridViewBomSpec.GetRow(_bomSpecSelectedRow) as PopBomSpecDto;

        //    if (target == null) return;

        //    if (target.TorqueLower > target.TorqueResult || target.TorqueResult > target.TorqueUpper)
        //    {
        //        //gridViewBomSpec.Columns[TorqueResultField].AppearanceCell.ForeColor = AppTheme.Rose;
        //        //AppLog.Info($"[POP] {Language.Pop_TorqueNoPendingRow} (설비 {equipmentId}, 값 {TorqueValue}, 결과 NG)");
        //        isTorqueRec = false;
        //    }
        //    else
        //    {
        //        isTorqueRec = true;
        //    }

        //    if (target.AngleLower > target.AngleResult || target.AngleResult > target.AngleUpper)
        //    {
        //        //gridViewBomSpec.Columns["AngleResult"].AppearanceCell.ForeColor = AppTheme.Rose;
        //        //AppLog.Info($"[POP] {Language.Pop_TorqueNoPendingRow} (설비 {equipmentId}, 값 {AngleValue}, 결과 NG)");
        //        isAngleRec = false;
        //    }
        //    else
        //    {
        //        isAngleRec = true;
        //    }

        //    if (isTorqueRec && isAngleRec)
        //    {
        //        //AppLog.Info($"[POP] {Language.Pop_TorqueNoPendingRow} (설비 {equipmentId}, 값 {TorqueValue}, 값 {AngleValue}, 결과 OK)");
        //        //_bomSpecSelectedRow++;

        //        e.Appearance.ForeColor = AppTheme.Rose;
        //    }
        //    else
        //    {
        //        //AppLog.Info($"[POP] {Language.Pop_TorqueNoPendingRow} (설비 {equipmentId}, 값 {TorqueValue}, 값 {AngleValue}, 결과 NG)");

        //        e.Appearance.ForeColor = AppTheme.Emerald;
        //    }



        //    var row = gridViewBomSpec.GetRow(e.RowHandle) as PopBomSpecDto;
        //    if (row == null)
        //        return;

        //    if (decimal.TryParse(row.TorqueResult?.ToString(), out decimal value))
        //    {
        //        decimal.TryParse(row.TorqueLower?.ToString(), out decimal min);
        //        decimal.TryParse(row.TorqueUpper?.ToString(), out decimal max);

        //        // 토크 값이 범위를 벗어나면 글자 색상을 Rose(빨강)로 변경
        //        if (value < min || value > max)
        //        {
        //            e.Appearance.ForeColor = AppTheme.Rose;
        //        }
        //        else
        //        {
        //            e.Appearance.ForeColor = AppTheme.Emerald;
        //        }
        //    }
        //}


        /// <summary>
        /// 연결 장비에서 올라온 토크 값을 아직 비어 있는 첫 줄에 채운다.
        /// 수동 렌치를 켠 동안에는 작업자 입력이 우선이므로 자동 반영하지 않는다.
        /// </summary>
        private void ApplyTorqueFromEquipment(string equipmentId, decimal TorqueValue, decimal AngleValue)
        {
            if (chkManualWrench.Checked)
                return;
            //var row = gridViewBomSpec.GetRow(_bomSpecSelectedRow);
            //var target = _bomSpecs[_bomSpecSelectedRow];
            bool isTorqueRec = false;
            bool isAngleRec = false;
            var target =  gridViewBomSpec.GetRow(_bomSpecSelectedRow) as PopBomSpecDto;
            
            if (target == null) return;

            if (target.TorqueLower > TorqueValue || TorqueValue > target.TorqueUpper)
            {
                //gridViewBomSpec.Columns[TorqueResultField].AppearanceCell.ForeColor = AppTheme.Rose;
                AppLog.Info($"[POP] {Language.Pop_TorqueNoPendingRow} (설비 {equipmentId}, 값 {TorqueValue}, 결과 NG)");
                isTorqueRec = false;
            }
            else
            {
                isTorqueRec = true;
            }

            if (target.AngleLower > AngleValue || AngleValue > target.AngleUpper)
            {
                //gridViewBomSpec.Columns["AngleResult"].AppearanceCell.ForeColor = AppTheme.Rose;
                AppLog.Info($"[POP] {Language.Pop_TorqueNoPendingRow} (설비 {equipmentId}, 값 {AngleValue}, 결과 NG)");
                isAngleRec = false;
            }
            else
            {
                isAngleRec = true;
            }

            if(isTorqueRec && isAngleRec)
            {
                AppLog.Info($"[POP] {Language.Pop_TorqueNoPendingRow} (설비 {equipmentId}, 값 {TorqueValue}, 값 {AngleValue}, 결과 OK)");
                _bomSpecSelectedRow++;
            }
            else
            {
                AppLog.Info($"[POP] {Language.Pop_TorqueNoPendingRow} (설비 {equipmentId}, 값 {TorqueValue}, 값 {AngleValue}, 결과 NG)");
            }

            target.TorqueResult = TorqueValue;
            target.AngleResult = AngleValue;
            target.IsManualInput = false;
            gridViewBomSpec.RefreshData();
            
            if(_bomSpecs.Count >= _bomSpecSelectedRow)
            {
                gridViewBomSpec.FocusedRowHandle = _bomSpecSelectedRow;
            }
            else
            {
                _bomSpecSelectedRow = _bomSpecs.Count;
            }
            
            AppLog.Info($"[POP] 설비 토크 결과값 수신 - {equipmentId} → {target.BomSpecName}/{target.InputPosition} = {TorqueValue}");
        }


    }
}
