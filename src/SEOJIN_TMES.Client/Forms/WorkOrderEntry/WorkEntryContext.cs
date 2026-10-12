using SEOJIN_TMES.Client.Common.Models;

namespace SEOJIN_TMES.Client.Forms.WorkOrderEntry
{
    /// <summary>
    /// POP 진입 화면에서 고른 "작업 대상" — 작업지시 한 건 + 이동할 페이지 공정 한 건.
    /// POP 화면이 이 값을 받아 어떤 작업지시/공정으로 들어왔는지 표시한다.
    /// </summary>
    public class WorkEntryContext
    {
        public WorkOrderEntryDto WorkOrder { get; }
        public RoutingProcessDto Process { get; }

        public WorkEntryContext(WorkOrderEntryDto workOrder, RoutingProcessDto process)
        {
            WorkOrder = workOrder;
            Process = process;
        }

        /// <summary>화면 상단에 한 줄로 보여줄 요약: 작업지시번호 / 시리얼번호 / 라우팅그룹-라우팅공정-페이지공정.</summary>
        public string Summary =>
            $"{WorkOrder?.WorkOrderNo} / {WorkOrder?.SerialNo} / {Process?.RoutingGroup}-{Process?.RoutingCode}-{Process?.PageName}";
    }
}
