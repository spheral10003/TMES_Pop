namespace SEOJIN_TMES.Client.Common.Models
{
    /// <summary>
    /// &lt;작업통합 현황 조회&gt; 화면의 [작업 통합 개요] 트리 한 줄.
    ///
    /// 생산품목 시리얼번호 하나가 거쳐 온 전 과정을
    /// <b>품목 → 라우팅 그룹 → 라우팅 공정 → 페이지 공정 → 투입자재 → 스팩</b> 계층으로 펼쳐 보여 준다.
    /// 계층은 <see cref="NodeId"/>/<see cref="ParentNodeId"/> 부모-자식 관계로 표현하며(트리 구조를
    /// 중첩 객체가 아니라 평평한 목록으로 내려받는다), 값 컬럼은 스팩 줄에서만 채워진다.
    ///
    /// TODO: 실제 서버 응답 필드명이 확정되면 속성명을 맞추거나 JsonProperty로 매핑할 것.
    /// </summary>
    public class WorkIntegratedNodeDto
    {
        /// <summary>줄 식별자(트리 구성용). 같은 응답 안에서만 유일하면 된다.</summary>
        public int NodeId { get; set; }

        /// <summary>부모 줄의 <see cref="NodeId"/>. 최상위(품목) 줄은 0.</summary>
        public int ParentNodeId { get; set; }

        /// <summary>항목 코드 — 계층에 따라 품목코드 / 라우팅 그룹 / 공정 / 자재코드 / 스팩명이 들어간다.</summary>
        public string NodeCode { get; set; }

        /// <summary>항목 설명.</summary>
        public string NodeDescription { get; set; }

        /// <summary>
        /// 줄의 종류(품목/라우팅그룹/라우팅공정/페이지공정/투입자재/스팩).
        /// 화면에서 값 컬럼을 보여줄 줄인지 판단하는 데만 쓰고, 업무 판정에는 쓰지 않는다.
        /// </summary>
        public string NodeKind { get; set; }

        /// <summary>토크 결과값(실적). 스팩 줄에서만 값이 있다.</summary>
        public decimal? TorqueResult { get; set; }

        /// <summary>토크 목푯값(기준정보).</summary>
        public decimal? TorqueTarget { get; set; }

        /// <summary>토크 하한값(기준정보).</summary>
        public decimal? TorqueLower { get; set; }

        /// <summary>토크 최댓값(기준정보).</summary>
        public decimal? TorqueMax { get; set; }

        /// <summary>앵글 목푯값(기준정보).</summary>
        public decimal? AngleTarget { get; set; }

        /// <summary>앵글 하한값(기준정보).</summary>
        public decimal? AngleLower { get; set; }

        /// <summary>앵글 상한값(기준정보).</summary>
        public decimal? AngleUpper { get; set; }

        /// <summary>앵글 결과값(실적).</summary>
        public decimal? AngleResult { get; set; }
    }

    /// <summary>&lt;작업 통합 개요&gt; 트리에서 줄의 종류를 나타내는 값(서버와 맞춰 쓰는 문자열 상수).</summary>
    public static class WorkIntegratedNodeKind
    {
        public const string Item = "ITEM";
        public const string RoutingGroup = "ROUTING_GROUP";
        public const string RoutingOperation = "ROUTING_OPERATION";
        public const string PageOperation = "PAGE_OPERATION";
        public const string Material = "MATERIAL";
        public const string Spec = "SPEC";
    }
}
