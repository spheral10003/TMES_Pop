using SEOJIN_TMES.Client.ApiClient;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Forms.Common;
using System.Linq;
using System.Threading.Tasks;

namespace SEOJIN_TMES.Client.Forms.Pop
{
    /// <summary>
    /// 공정 자동 진행 — 탭 3개(투입자재 / BOM 스팩 / 검사 스팩)의 입력이 모두 끝나면
    /// 서버에 [공정 종료]를 보내고, 다음 공정으로 옮겨 [공정 시작]까지 자동으로 이어 간다.
    ///
    ///  - 세 탭 모두 데이터가 없는 공정도 "끝난 공정"으로 보고 그대로 종료 → 다음 공정으로 넘어간다.
    ///  - 작업자가 [이전 공정] · [다음 공정] · [공정 목록]으로 직접 옮긴 경우에는 자동 시작하지 않는다
    ///    (그 경로는 <see cref="MoveToProcessAsync"/>만 부르고 이 파일의 흐름을 타지 않는다).
    ///    작업자가 [공정 시작]을 누르면 그때부터 같은 완료 검사가 돈다.
    /// </summary>
    public partial class PopScreen
    {
        /// <summary>자동 완료 · 이동 · 시작이 진행 중인지. 저장 응답이 겹쳐 들어와도 한 번만 돌게 막는다.</summary>
        private bool _autoProgressing;

        /// <summary>
        /// 서버에 보내는 중인 실적 저장(시리얼 · 토크 · 검사) 개수.
        /// 화면 값은 저장 응답 전에 이미 채워져 있으므로, 응답을 기다리지 않고 완료를 보내면
        /// 서버에는 마지막 실적이 없는 채로 공정이 끝날 수 있다 — 0일 때만 완료를 보낸다.
        /// </summary>
        private int _pendingResultSaves;

        private void BeginResultSave() => _pendingResultSaves++;

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
        private async Task TryAutoProgressAsync()
        {
            if (_autoProgressing || _pendingResultSaves > 0)
                return;

            _autoProgressing = true;
            var moved = false;
            try
            {
                while (!IsDisposed && !Disposing && IsProcessRunning && IsAllTabsCompleted())
                {
                    if (!await CompleteCurrentProcessAsync())
                        return;

                    if (!await MoveNextAndStartAsync())
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
                MaterialsChk();
        }

        /// <summary>
        /// 탭 3개의 입력이 모두 끝났는지. 세 탭 모두 비어 있으면 할 일이 없는 공정이므로 끝난 것으로 본다.
        /// </summary>
        private bool IsAllTabsCompleted()
        {
            var materialsDone = _materials == null || _materials.All(m => m.IsSerialInputCompleted);
            var bomSpecsDone = _bomSpecs == null || _bomSpecs.All(IsBomSpecPassed);
            var inspectionsDone = _inspections == null || _inspections.All(IsInspectionEntered);

            return materialsDone && bomSpecsDone && inspectionsDone;
        }

        /// <summary>
        /// BOM 스팩 한 줄이 측정되고 NG가 아닌지 — 토크 · 각도가 모두 들어와 있고 하한~상한 안이어야 한다.
        /// 그리드의 NG 표시(<see cref="GridViewBomSpec_RowStyle"/>)와 같은 기준을 써서,
        /// 화면에 빨간 줄이 하나라도 남아 있으면 넘어가지 않는다.
        /// </summary>
        private static bool IsBomSpecPassed(PopBomSpecDto spec)
        {
            if (spec.TorqueResult == null || spec.AngleResult == null)
                return false;

            var torqueOk = spec.TorqueResult >= spec.TorqueLower && spec.TorqueResult <= spec.TorqueUpper;
            var angleOk = spec.AngleResult >= spec.AngleLower && spec.AngleResult <= spec.AngleUpper;
            return torqueOk && angleOk;
        }

        /// <summary>검사 스팩 한 줄이 입력됐는지 — 결과상태는 필수이고, 정량 검사는 결과값까지 있어야 한다.</summary>
        private static bool IsInspectionEntered(PopInspectionSpecDto spec)
        {
            if (string.IsNullOrWhiteSpace(spec.ResultState))
                return false;

            return !spec.IsQuantitative || !string.IsNullOrWhiteSpace(spec.ResultValue);
        }

        /// <summary>서버에 [공정 종료]를 보내고 화면을 다시 읽는다. 실패하면 사유를 알리고 false.</summary>
        private async Task<bool> CompleteCurrentProcessAsync()
        {
            try
            {
                AppLog.Step($"[POP] 공정 자동 완료 - {_key.RoutingOperation}/{_key.PageOperation}");
                await _appSet.ApiClients.PopApiClient.ChangeProcessStateAsync(_key, PopProcessAction.end);
                if (IsDisposed || Disposing) return false;

                await LoadAllAsync();
                return !IsDisposed && !Disposing;
            }
            catch (ApiException ex)
            {
                AppLog.Warn($"[POP] 공정 자동 완료 실패 - {ex.Message}");
                if (IsDisposed || Disposing) return false;

                AppMessageBox.Warn(this, ex.Message, Language.Pop_Title);
                return false;
            }
        }

        /// <summary>
        /// 공정 목록의 다음 공정으로 옮기고 [공정 시작]을 자동으로 보낸다.
        /// 마지막 공정이었으면 알리고 false. 옮긴 공정이 대기(Ready)가 아니면(이미 진행 · 완료 등)
        /// 시작 요청은 보내지 않고 서버 상태를 그대로 따른다.
        /// </summary>
        private async Task<bool> MoveNextAndStartAsync()
        {
            if (!await EnsureProcessesAsync())
                return false;

            var current = CurrentProcessIndex();
            if (current < 0 || current + 1 >= _processes.Count)
            {
                AppMessageBox.Info(this, Language.Pop_LastProcess, Language.Pop_Title);
                return false;
            }

            await MoveToProcessAsync(_processes[current + 1]);
            if (IsDisposed || Disposing) return false;

            if (!Contains(_progressState, "Ready"))
            {
                AppLog.Info($"[POP] 다음 공정 자동 시작 생략 - 상태 {_progressState}");
                return IsProcessRunning;
            }

            try
            {
                await _appSet.ApiClients.PopApiClient.ChangeProcessStateAsync(_key, PopProcessAction.start);
                if (IsDisposed || Disposing) return false;

                await LoadAllAsync();
                if (IsDisposed || Disposing) return false;

                AppLog.Step($"[POP] 공정 자동 시작 - {_key.RoutingOperation}/{_key.PageOperation} → {_progressState}");
                return IsProcessRunning;
            }
            catch (ApiException ex)
            {
                AppLog.Warn($"[POP] 공정 자동 시작 실패 - {ex.Message}");
                if (IsDisposed || Disposing) return false;

                AppMessageBox.Warn(this, ex.Message, Language.Pop_Title);
                return false;
            }
        }
    }
}
