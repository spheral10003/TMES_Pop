using SEOJIN_TMES.Client.ApiClient;
using SEOJIN_TMES.Client.Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SEOJIN_TMES.Client.Forms.Pop
{
    /// <summary>
    /// POP 본 화면이 하는 일의 계약.
    /// <see cref="PopScreen"/>의 이벤트 핸들러(PopScreen.cs)는 이 인터페이스로만 처리를 부르고,
    /// 구현은 그리드는 PopScreen.Grid.cs, 공정 · 설비 · 도면은 PopScreen.Method.cs에 둔다 —
    /// 이벤트 쪽에서는 "무엇을 부르는지"만 보이게 하려는 것.
    /// </summary>
    internal interface IPopScreen
    {
        /// <summary>지금 보고 있는 공정의 조회 키. 진입 대상 없이 열렸으면 null.</summary>
        PopProcessKey Key { get; }

        /// <summary>서버 상태가 진행 중(Progress)인지. 대기 · 중지 · 완료면 false.</summary>
        bool IsProcessRunning { get; }

        /// <summary>
        /// 그리드 · 투입자재 팝업에서 값을 입력받을 수 있는지 — 완료(Finished) · 중지(Stopped) 상태는 막는다(사용자 요구).
        /// 완료된 공정은 [리웍]으로 진행 중이 되면 다시 입력할 수 있다. 대기 상태의 입력을 받을지는 서버가 판정한다.
        /// </summary>
        bool IsInputAllowed { get; }

        // 그리드
        void InitGrids();
        void SetGridData(PopScreenData screen);
        void RefreshBomSpecGrid();
        void RefreshEquipmentStates();
        void SelectBomSpecRow();

        // 공정
        Task GetProcessInfo();
        Task GetIsLastProcess();
        Task<bool> SetProcessAction(PopProcessAction action);
        Task StartRework();
        Task MoveProcess(int offset);
        Task ShowProcessList();
        Task StartEnteredProcess();
        Task TryAutoProgress();
        Task ShowAdminAlarm();

        // 실적
        Task InputMaterialSerial(int rowHandle);
        Task CheckMaterialSerials();
        bool SelectInputTab();
        void ShowInspectionTabIfBomDone();
        Task<bool> SetBomSpecResult(PopBomSpecDto spec, decimal? torque, decimal? angle, string equipmentCode);
        Task<bool> SetInspectionResult(PopInspectionSpecDto spec, string fieldName);

        // 설비
        Task SetTorqueFromEquipment(string equipmentId, decimal torqueValue, decimal angleValue);
        IReadOnlyList<string> WriteTorqueToEquipment(decimal torqueTarget);
        void WriteBomSpecTorque();

        // 도면(공유 폴더 파일)
        Task LoadDrawing();
    }
}
