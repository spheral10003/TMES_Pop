using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using SEOJIN_TMES.Client.ApiClient;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Forms.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SEOJIN_TMES.Client.Forms.Pop
{
    public partial class PopScreen
    {

        /// <summary>[BOM 스팩] 탭의 "결과값 입력" 컬럼(데이터에 없는 화면 전용 컬럼)의 필드명.</summary>
        private const string ResultInputField = "__ResultInput";

        /// <summary>[BOM 스팩] 탭의 "토크 결과값" 컬럼 필드명(<see cref="PopBomSpecDto.TorqueResult"/>).</summary>
        private const string TorqueResultField = "TorqueResult";
        private const string AngleResultField = "AngleResult";

        /// <summary>토크 결과값을 다루는 소수 자릿수 — 이 자리에서 올림해 표시하고 저장한다.</summary>
        private const int TorqueDecimals = 2;

        private List<PopBomSpecDto> _bomSpecs = new List<PopBomSpecDto>();

        private int _bomSpecSelectedRow = 0;

        /// <summary>
        /// [BOM 스팩] 그리드를 다시 그리는 중(DataSource 바인딩 · RefreshData)인 구간의 깊이.
        /// 0보다 크면 <c>GridViewBomSpec_FocusedRowChanged</c>를 무시한다 —
        /// 다시 그릴 때 DevExpress가 포커스 행을 재설정하며 이벤트를 올리는데,
        /// 그때마다 너트러너에 목표 토크를 다시 쓰거나 "사용할 수 있는 너트러너가 없습니다" 창이 뜨면 안 된다.
        /// 바깥 구간 안에서 <see cref="RefreshBomSpecGrid"/>를 또 불러도 풀리지 않도록 bool이 아닌 카운터로 둔다.
        /// </summary>
        private int _bomSpecRedrawDepth;

        private bool IsBomSpecRedrawing => _bomSpecRedrawDepth > 0;

        private void BeginBomSpecRedraw() => _bomSpecRedrawDepth++;

        private void EndBomSpecRedraw()
        {
            if (_bomSpecRedrawDepth > 0)
                _bomSpecRedrawDepth--;
        }

        /// <summary>값만 바뀐 행을 다시 그린다 — 포커스 이벤트는 올리지 않는다.</summary>
        private void RefreshBomSpecGrid()
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

        /// <summary>
        /// 연결 장비에서 올라온 토크 값을 아직 비어 있는 첫 줄에 채운다.
        /// 수동 렌치를 켠 동안에는 작업자 입력이 우선이므로 자동 반영하지 않는다.
        /// 공정을 시작하지 않은 상태(대기 · 중지 · 완료)에서 올라온 체결 데이터는 실적이 아니므로
        /// 화면에도 반영하지 않고 서버로도 보내지 않는다(로그만 남기고 버린다).
        /// </summary>
        private async void ApplyTorqueFromEquipment(string equipmentId, decimal TorqueValue, decimal AngleValue)
        {
            if (!IsProcessRunning)
            {
                AppLog.Info($"[POP] 공정 미시작 - 설비 데이터 버림 (설비 {equipmentId}, 상태 {_progressState}, " +
                $"토크 {TorqueValue}, 각도 {AngleValue})");
                                return;
            }

            if (chkManualWrench.Checked)
                return;

            var target = gridViewBomSpec.GetRow(_bomSpecSelectedRow) as PopBomSpecDto;

            if (target == null)
                return;

            if (target.TorqueResult == null && target.AngleResult == null)
                return;

            bool torqueOk = target.TorqueResult >= target.TorqueLower && target.TorqueResult <= target.TorqueUpper;
            bool angleOk = target.AngleResult >= target.AngleLower && target.AngleResult <= target.AngleUpper;

            if (torqueOk && angleOk)
            {
                _bomSpecSelectedRow++;
            }

            AppLog.Info($"[POP] {Language.Pop_TorqueNoPendingRow} (설비 {equipmentId}, 값 {TorqueValue}, 값 {AngleValue}");

            target.TorqueResult = TorqueValue;
            target.AngleResult = AngleValue;
            target.IsManualInput = false;
            target.EquipmentCode = equipmentId;
            RefreshBomSpecGrid();

            var result = new PopBomSpecResultDto
            {
                eqpCd = equipmentId,
                torqueRsltVl = TorqueValue,
                angleRsltVl = AngleValue
            };

            BeginResultSave();
            try
            {
                await _apiClient.PopApiClient.SaveBomSpecResultsAsync(_key, result, target);
            }
            catch (ApiException ex)
            {

                AppLog.Warn($"[POP] 설비 토크 결과 저장 실패 - {equipmentId} : {ex.Message}");
                if (IsDisposed || Disposing) return;
                AppMessageBox.Warn(this, ex.Message, Language.Pop_Title);
                return;
            }
            finally
            {
                EndResultSave();
            }

            if (IsDisposed || Disposing) return;

            if (_bomSpecs.Count >= _bomSpecSelectedRow)
            {
                gridViewBomSpec.FocusedRowHandle = _bomSpecSelectedRow;
            }
            else
            {
                _bomSpecSelectedRow = _bomSpecs.Count;
            }

            AppLog.Info($"[POP] 설비 토크 결과값 수신 - {equipmentId} → {target.BomSpecName}/{target.InputPosition} = {TorqueValue}");

            await TryAutoProgressAsync();
        }



        /// <summary>
        /// Helper method for BOM tab: Highlight torque results outside specification range.
        /// Extracted from original LoadBomAsync() method in Bom.cs.
        /// </summary>
        private void ApplyBomSpecHighlighting(List<PopBomSpecDto> specs)
        {
            if (specs == null || specs.Count == 0)
                return;

            for (int i = 0; i < gridViewBomSpec.RowCount; i++)
            {
                var target = gridViewBomSpec.GetRow(i) as PopBomSpecDto;
                
                if (target == null)
                    continue;

                if (target.TorqueResult == null && target.AngleResult == null)
                    continue;

                gridViewBomSpec.FormatConditions.Clear();

                bool torqueOk = target.TorqueResult >= target.TorqueLower && target.TorqueResult <= target.TorqueUpper;
                bool angleOk = target.AngleResult >= target.AngleLower && target.AngleResult <= target.AngleUpper;


                if (!(torqueOk && angleOk))
                {
                    //gridViewBomSpec.Appearance.Row.BackColor = AppTheme.Rose;
                    //e.Appearance.ForeColor = Color.White;   // 글자가 안 보이면 지정
                    //gridViewBomSpec.Appearance.Row.Hight = AppTheme.Rose; e.HighPriority = true;                  // 선택/포커스 행 색상보다 우선 적용
                }

                gridViewBomSpec.FocusedRowHandle = i;
                
            }
        }
    }
}
