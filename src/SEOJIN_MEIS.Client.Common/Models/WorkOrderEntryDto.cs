using System;

namespace SEOJIN_MEIS.Client.Common.Models
{
    /// <summary>
    /// POP 진입 화면(작업지시 조회)의 목록 한 줄.
    /// 서버 조회 결과를 그대로 표시하며, 클라이언트는 판정/계산을 하지 않는다.
    /// TODO: 실제 서버 응답 필드명이 확정되면 속성명을 맞추거나 JsonProperty로 매핑할 것.
    /// </summary>
    public class WorkOrderEntryDto
    {
        /// <summary>작업지시번호 (예: WO_260701-01).</summary>
        public string WorkOrderNo { get; set; }

        /// <summary>품목번호.</summary>
        public string ItemCode { get; set; }

        /// <summary>생산품목 시리얼번호.</summary>
        public string SerialNo { get; set; }

        /// <summary>작업지시 수량.</summary>
        public decimal OrderQty { get; set; }

        /// <summary>수주번호.</summary>
        public string SalesOrderNo { get; set; }

        /// <summary>고객사 발주번호.</summary>
        public string CustomerPoNo { get; set; }

        /// <summary>FCID.</summary>
        public string Fcid { get; set; }

        /// <summary>시작 예정일자.</summary>
        public DateTime? PlanStartDate { get; set; }

        /// <summary>종료 예정일자.</summary>
        public DateTime? PlanEndDate { get; set; }

        /// <summary>작업 시작일자(실적).</summary>
        public DateTime? ActualStartDate { get; set; }

        /// <summary>작업 종료일자(실적).</summary>
        public DateTime? ActualEndDate { get; set; }

        /// <summary>작업지시 상태.</summary>
        public string StatusName { get; set; }

        /// <summary>비고.</summary>
        public string Remark { get; set; }
    }

    /// <summary>
    /// 작업 진입 확인 팝업의 공정 목록 한 줄(선택한 작업지시의 라우팅 정보).
    /// 사용자는 이 목록에서 이동할 "페이지 공정"을 고른다.
    /// </summary>
    public class RoutingProcessDto
    {
        /// <summary>라우팅 그룹 (예: MAIN-01).</summary>
        public string RoutingGroup { get; set; }

        /// <summary>라우팅 공정 (예: OP10).</summary>
        public string RoutingOperation { get; set; }

        /// <summary>페이지 공정 (예: 0010).</summary>
        public string PageOperation { get; set; }

        /// <summary>진행 상태 (진행중/대기/완료 등 서버가 준 문자열을 그대로 표시).</summary>
        public string ProgressState { get; set; }
    }
}
