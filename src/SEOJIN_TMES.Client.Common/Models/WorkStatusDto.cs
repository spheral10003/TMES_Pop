using System;

namespace SEOJIN_TMES.Client.Common.Models
{
    /// <summary>
    /// &lt;작업 조회 현황&gt; 화면의 조회 조건.
    /// 값이 없는 조건은 서버로 보내지 않으므로, 비워 두면 그 조건은 걸리지 않는다.
    /// </summary>
    public class WorkStatusQuery
    {
        /// <summary>지시일자 시작(필수). 화면에서 기본값은 한 달 전.</summary>
        public DateTime? OrderDateFrom { get; set; }

        /// <summary>지시일자 종료(필수). 화면에서 기본값은 오늘.</summary>
        public DateTime? OrderDateTo { get; set; }

        /// <summary>작업지시번호(부분 일치).</summary>
        public string WorkOrderNo { get; set; }

        /// <summary>생산품목 코드.</summary>
        public string ItemCode { get; set; }

        /// <summary>생산품목 시리얼번호(부분 일치).</summary>
        public string SerialNo { get; set; }

        /// <summary>작업지시 상태. 비어 있거나 "전체"면 상태로 거르지 않는다.</summary>
        public string StatusName { get; set; }
    }

    /// <summary>
    /// &lt;작업 조회 현황&gt; 화면 아래쪽 [작업 진행 현황] 목록 한 줄.
    /// 선택한 작업지시가 라우팅/페이지 공정별로 어떤 스팩을 어디까지 진행했는지 보여 준다.
    /// 판정(스팩 결과 상태)은 서버가 낸 값을 그대로 표시한다.
    /// </summary>
    public class WorkProgressDto
    {
        /// <summary>라우팅 그룹.</summary>
        public string RoutingGroup { get; set; }

        /// <summary>라우팅 공정.</summary>
        public string RoutingOperation { get; set; }

        /// <summary>페이지 공정명.</summary>
        public string PageOperationName { get; set; }

        /// <summary>스팩명 (BOM 스팩 / 검사 스팩의 이름).</summary>
        public string SpecName { get; set; }

        /// <summary>스팩 유형 (토크작업 / 정량 / 정성 등).</summary>
        public string SpecType { get; set; }

        /// <summary>진행 상태 (대기 / 진행중 / 완료).</summary>
        public string ProgressState { get; set; }

        /// <summary>스팩 결과 상태 (합격 / 불합격). 아직 결과가 없으면 비어 있다.</summary>
        public string SpecResultState { get; set; }
    }

    /// <summary>
    /// 품목 기준정보 한 건 — 조회 조건의 생산품목 돋보기(&lt;생산품목 선택&gt; 팝업)에서 고른다.
    /// </summary>
    public class ItemDto
    {
        /// <summary>품목코드.</summary>
        public string ItemCode { get; set; }

        /// <summary>품목명.</summary>
        public string ItemName { get; set; }

        /// <summary>규격.</summary>
        public string Specification { get; set; }

        /// <summary>단위.</summary>
        public string Unit { get; set; }
    }
}
