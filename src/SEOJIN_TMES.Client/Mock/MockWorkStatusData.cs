using System;
using System.Collections.Generic;
using System.Linq;
using SEOJIN_TMES.Client.Common.Models;

namespace SEOJIN_TMES.Client.Mock
{
    /// <summary>
    /// &lt;작업 조회 현황&gt; 화면의 목업 데이터.
    /// 값은 화면 설계서(목업 이미지)의 샘플 행을 그대로 옮긴 것이며, 실제 업무 데이터가 아니다.
    ///
    /// 작업 지시 정보는 <see cref="MockWorkOrderData"/>의 샘플을 그대로 쓰지 않고
    /// 이 화면 설계서에 맞는 별도 샘플(WO_260701-01 / WO_260722-01)을 쓴다.
    ///
    /// App.config의 <c>UseMockData</c>가 true일 때만 사용된다.
    /// </summary>
    public static class MockWorkStatusData
    {
        /// <summary>
        /// [조회] 결과 목업. 조건은 서버와 같은 감각으로 동작하도록
        /// 문자열은 부분 일치(대소문자 무시), 지시일자는 시작 예정일자 기준 범위로 거른다.
        /// </summary>
        public static List<WorkOrderEntryDto> SearchWorkOrders(WorkStatusQuery query)
        {
            var condition = query ?? new WorkStatusQuery();

            return CreateWorkOrders()
                .Where(row => InDateRange(row.PlanStartDate, condition.OrderDateFrom, condition.OrderDateTo))
                .Where(row => Matches(row.WorkOrderNo, condition.WorkOrderNo))
                .Where(row => Matches(row.ItemCode, condition.ItemCode))
                .Where(row => Matches(row.SerialNo, condition.SerialNo))
                .Where(row => MatchesExact(row.StatusName, condition.StatusName))
                .ToList();
        }

        /// <summary>
        /// [작업 진행 현황] 목업 — 선택한 작업지시의 공정/스팩별 진행 상태.
        /// 화면 설계서 샘플대로 페이지공정01(BOM 스팩 3건) + 페이지공정02(검사 스팩 2건 + BOM 스팩 1건)를 만든다.
        /// 진행중인 작업지시는 뒤쪽 스팩이 아직 대기 상태로 남는다.
        /// </summary>
        public static List<WorkProgressDto> GetWorkProgress(string workOrderNo, string serialNo)
        {
            var workOrder = CreateWorkOrders().FirstOrDefault(row =>
                string.Equals(row.WorkOrderNo, workOrderNo, StringComparison.OrdinalIgnoreCase));

            if (workOrder == null)
                return new List<WorkProgressDto>();

            var completed = workOrder.StatusName == "완료";

            return new List<WorkProgressDto>
            {
                Progress("페이지공정01", "BOM스펙01", "토크작업", "완료", "합격"),
                Progress("페이지공정01", "BOM스펙02", "토크작업", "완료", "합격"),
                Progress("페이지공정01", "BOM스펙03", "토크작업", "완료", "불합격"),
                Progress("페이지공정02", "검사스펙01", "정성", "완료", "합격"),
                Progress("페이지공정02", "검사스펙02", "정량", completed ? "완료" : "진행중", completed ? "합격" : null),
                Progress("페이지공정02", "BOM스펙01", "정성", completed ? "완료" : "대기", completed ? "합격" : null)
            };
        }

        /// <summary>&lt;생산품목 선택&gt; 팝업의 품목 검색 목업(품목코드/품목명 부분 일치).</summary>
        public static List<ItemDto> SearchItems(string keyword)
        {
            return CreateItems()
                .Where(item => Matches(item.ItemCode, keyword) || Matches(item.ItemName, keyword))
                .ToList();
        }

        private static WorkProgressDto Progress(
            string pageOperationName, string specName, string specType, string progressState, string resultState)
        {
            return new WorkProgressDto
            {
                RoutingGroup = "라우팅그룹01",
                RoutingOperation = "라우팅공정01",
                PageOperationName = pageOperationName,
                SpecName = specName,
                SpecType = specType,
                ProgressState = progressState,
                SpecResultState = resultState
            };
        }

        /// <summary>
        /// 화면 설계서 샘플 행(작업 지시 정보 목록).
        /// 일자는 <b>오늘 기준 상대값</b>으로 만든다 — 고정 날짜로 두면 언제 실행하느냐에 따라
        /// 화면 기본 조회 범위(최근 한 달)를 벗어나 목업인데도 결과가 0건이 되기 때문이다.
        /// </summary>
        private static List<WorkOrderEntryDto> CreateWorkOrders()
        {
            var today = DateTime.Today;

            return new List<WorkOrderEntryDto>
            {
                new WorkOrderEntryDto
                {
                    WorkOrderNo = "WO_" + today.AddDays(-26).ToString("yyMMdd") + "-01",
                    ItemCode = "A001",
                    SerialNo = "SN-0001",
                    OrderQty = 1m,
                    SalesOrderNo = "SO-0001",
                    CustomerPoNo = "PO-0001",
                    Fcid = "FCID-0001",
                    PlanStartDate = today.AddDays(-26),
                    PlanEndDate = today.AddDays(-24),
                    WorkStartDate = today.AddDays(-26),
                    WorkEndDate = null,
                    StatusName = "진행중",
                    Remark = "1차 양산"
                },
                new WorkOrderEntryDto
                {
                    WorkOrderNo = "WO_" + today.AddDays(-18).ToString("yyMMdd") + "-01",
                    ItemCode = "A001",
                    SerialNo = "SN-0002",
                    OrderQty = 1m,
                    SalesOrderNo = "SO-0002",
                    CustomerPoNo = "PO-0002",
                    Fcid = "FCID-0002",
                    PlanStartDate = today.AddDays(-18),
                    PlanEndDate = today.AddDays(-10),
                    WorkStartDate = today.AddDays(-18),
                    WorkEndDate = today.AddDays(-11),
                    StatusName = "완료",
                    Remark = null
                },
                new WorkOrderEntryDto
                {
                    WorkOrderNo = "WO_" + today.AddDays(-4).ToString("yyMMdd") + "-01",
                    ItemCode = "B002",
                    SerialNo = "SN-0003",
                    OrderQty = 5m,
                    SalesOrderNo = "SO-0003",
                    CustomerPoNo = "PO-0003",
                    Fcid = "FCID-0003",
                    PlanStartDate = today.AddDays(-4),
                    PlanEndDate = today.AddDays(4),
                    WorkStartDate = null,
                    WorkEndDate = null,
                    StatusName = "대기",
                    Remark = "자재 입고 대기"
                }
            };
        }

        private static List<ItemDto> CreateItems()
        {
            return new List<ItemDto>
            {
                new ItemDto { ItemCode = "A001", ItemName = "abcd", Specification = "100 x 50", Unit = "EA" },
                new ItemDto { ItemCode = "A002", ItemName = "abce", Specification = "120 x 50", Unit = "EA" },
                new ItemDto { ItemCode = "B002", ItemName = "bcde", Specification = "80 x 40", Unit = "EA" },
                new ItemDto { ItemCode = "C003", ItemName = "cdef", Specification = "60 x 30", Unit = "SET" }
            };
        }

        /// <summary>시작 예정일자가 지시일자 범위 안인지(범위가 비어 있으면 통과).</summary>
        private static bool InDateRange(DateTime? value, DateTime? from, DateTime? to)
        {
            if (value == null)
                return from == null && to == null;

            if (from != null && value.Value.Date < from.Value.Date)
                return false;

            if (to != null && value.Value.Date > to.Value.Date)
                return false;

            return true;
        }

        /// <summary>조건이 비어 있으면 통과, 값이 있으면 부분 일치 여부로 판정한다.</summary>
        private static bool Matches(string value, string condition)
        {
            if (string.IsNullOrWhiteSpace(condition))
                return true;

            return value != null
                   && value.IndexOf(condition.Trim(), StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>상태처럼 목록에서 고르는 조건은 완전 일치로 본다(비어 있으면 통과).</summary>
        private static bool MatchesExact(string value, string condition)
        {
            if (string.IsNullOrWhiteSpace(condition))
                return true;

            return string.Equals(value, condition.Trim(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
