using Newtonsoft.Json;
using System;

namespace SEOJIN_TMES.Client.Common.Models
{
    /// <summary>
    /// POP 진입 화면(작업지시 조회)의 목록 한 줄.
    /// 서버 조회 결과를 그대로 표시하며, 클라이언트는 판정/계산을 하지 않는다.
    /// TODO: 실제 서버 응답 필드명이 확정되면 속성명을 맞추거나 JsonProperty로 매핑할 것.
    /// </summary>
    public class WorkOrderEntryDto
    {
        /// <summary>생성자.</summary>
        [JsonProperty("createdBy")]
        public string Creator { get; set; }

        /// <summary>생성 일자.</summary>
        [JsonProperty("createdAt")]
        public string CreatedDate { get; set; }

        /// <summary>회사 법인.</summary>
        [JsonProperty("corpCd")]
        public string CorpCode { get; set; }

        /// <summary>작업지시번호 (예: WO_260701-01).</summary>
        [JsonProperty("woNo")]
        public string WorkOrderNo { get; set; }

        /// <summary>품목번호.</summary>
        [JsonProperty("itemCd")]
        public string ItemCode { get; set; }

        /// <summary>생산품목 시리얼번호.</summary>
        [JsonProperty("sn")]
        public string SerialNo { get; set; }

        /// <summary>작업계획 수량.</summary>
        [JsonProperty("ppQty")]
        public decimal? OrderQty { get; set; }

        /// <summary>작업지시 수량.</summary>
        public decimal? WorkQty { get; set; }

        /// <summary>수주번호.</summary>
        [JsonProperty("soNo")]
        public string SalesOrderNo { get; set; }
        /// <summary>고객사 발주번호.</summary>
        [JsonProperty("custPoNo")]
        public string CustomerPoNo { get; set; }
        /// <summary>작업 계획 번호.</summary>
        [JsonProperty("workPlanNo")]
        public string WorkPlanNo { get; set; }

        /// <summary>FCID.</summary>
        [JsonProperty("fcid")]
        public string Fcid { get; set; }

        /// <summary>시작 예정일자.</summary>
        [JsonProperty("ppStartDt")]
        public DateTime? PlanStartDate { get; set; }

        /// <summary>종료 예정일자.</summary>
        [JsonProperty("ppEndDt")]
        public DateTime? PlanEndDate { get; set; }

        /// <summary>작업 시작일자(실적).</summary>
        [JsonProperty("workStartDt")]
        public DateTime? WorkStartDate { get; set; }

        /// <summary>작업 종료일자(실적).</summary>
        [JsonProperty("workEndDt")]
        public DateTime? WorkEndDate { get; set; }

        /// <summary>작업지시 상태.</summary>
        [JsonProperty("status")]
        public string StatusName { get; set; }

        /// <summary>비고.</summary>
        [JsonProperty("remark")]
        public string Remark { get; set; }
    }

    //public class WorkOrderEntryDto
    //{
    //    /// <summary>작업지시번호 (예: WO_260701-01).</summary>
    //    public string createdBy { get; set; }

    //    /// <summary>품목번호.</summary>
    //    public string createdAt { get; set; }

    //    /// <summary>생산품목 시리얼번호.</summary>
    //    public string corpCd { get; set; }

    //    /// <summary>품목.</summary>
    //    public string itemCd { get; set; }

    //    /// <summary>수주번호.</summary>
    //    public string ppRef { get; set; }

    //    /// <summary>고객사 발주번호.</summary>
    //    public string workPlanNo { get; set; }

    //    /// <summary>작업지시번호.</summary>
    //    public string woNo { get; set; }
    //    public string sn { get; set; }
    //    public decimal qty { get; set; }
    //    /// <summary>작업지시 상태.</summary>
    //    public string status { get; set; }

    //    /// <summary>시작 예정일자.</summary>
    //    public DateTime? ppStartDt { get; set; }

    //    /// <summary>종료 예정일자.</summary>
    //    public DateTime? ppEndDt { get; set; }

    //    /// <summary>작업 시작일자(실적).</summary>
    //    public DateTime? workStartDt { get; set; }

    //    /// <summary>작업 종료일자(실적).</summary>
    //    public DateTime? workEndDt { get; set; }


    //    public string fcid { get; set; }
    //    /// <summary>비고.</summary>
    //    public string remark { get; set; }
    //}




    /// <summary>
    /// 작업 진입 확인 팝업의 공정 목록 한 줄(선택한 작업지시의 라우팅 정보).
    /// 사용자는 이 목록에서 이동할 "페이지 공정"을 고른다.
    /// </summary>
    public class RoutingProcessDto
    {
        [JsonProperty("corpCd")]
        public string CorpCode { get; set; }

        [JsonProperty("sn")]
        public string SerialNo { get; set; }

        [JsonProperty("routingGroupRef")]
        public string RoutingGroupRef { get; set; }

        [JsonProperty("routingGroupCd")]
        public string RoutingGroupcCode { get; set; }

        /// <summary>라우팅 그룹 (예: MAIN-01).</summary>
        [JsonProperty("routingGroupNm")]
        public string RoutingGroup { get; set; }

        /// <summary>라우팅 공정 (예: OP10).</summary>
        [JsonProperty("routingCd")]
        public string RoutingCode { get; set; }
        
        /// <summary>라우팅 공정 (예: OP10).</summary>
        [JsonProperty("routingNm")]
        public string RoutingName { get; set; }

        /// <summary>페이지 공정 서브 라우팅 키.</summary>
        [JsonProperty("subRpRef")]
        public string SubRoutingReference { get; set; }

        /// <summary>페이지 공정 코드 (예: 0010).</summary>
        [JsonProperty("procCd")]
        public string PageCode { get; set; }
        [JsonProperty("procNm")]
        public string PageName { get; set; }

        /// <summary>진행 상태 (진행중/대기/완료 등 서버가 준 문자열을 그대로 표시).</summary>
        [JsonProperty("workStatus")]
        public string ProgressState { get; set; }

        
    }
}
