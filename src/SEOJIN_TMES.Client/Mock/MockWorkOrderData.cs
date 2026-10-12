using System;
using System.Collections.Generic;
using System.Linq;
using SEOJIN_TMES.Client.Common.Models;

namespace SEOJIN_TMES.Client.Mock
{
    /// <summary>
    /// 서버 API가 아직 없을 때 화면을 그대로 확인하기 위한 목업 데이터.
    /// 값은 화면 설계서의 샘플 행을 그대로 옮긴 것이며, 실제 업무 데이터가 아니다.
    ///
    /// App.config의 <c>UseMockData</c>가 true일 때만 사용된다(기본 false 이므로 운영 빌드에는 영향이 없다).
    /// 서버 스펙이 확정되면 이 파일과 <c>UseMockData</c> 설정을 함께 걷어내면 된다.
    /// </summary>
    public static class MockWorkOrderData
    {
        /// <summary>
        /// POP 진입 화면 [조회] 결과 목업.
        /// 조건은 서버와 같은 감각으로 동작하도록 <b>부분 일치(대소문자 무시)</b>로 걸러 준다.
        /// 두 조건이 모두 들어오면 둘 다 만족하는 행만 남는다(AND).
        /// </summary>
        public static List<WorkOrderEntryDto> SearchWorkOrders(string workOrderNo, string serialNo)
        {
            return CreateWorkOrders()
                .Where(row => Matches(row.WorkOrderNo, workOrderNo) && Matches(row.SerialNo, serialNo))
                .ToList();
        }

        /// <summary>
        /// &lt;작업 진입 확인&gt; 팝업의 공정(라우팅) 목록 목업.
        /// 조회 목업과 같은 샘플에서 해당 작업지시를 찾아, 그 작업지시 상태에 맞는 공정 진행 상태를 만들어 준다
        /// (완료 지시는 전 공정 완료, 진행중 지시는 앞 공정만 완료 + 한 공정 진행중 …).
        /// 샘플에 없는 작업지시면 빈 목록을 돌려준다 — 화면의 "공정 정보 없음" 안내 경로를 그대로 탄다.
        /// </summary>
        public static List<RoutingProcessDto> GetRoutingProcesses(string workOrderNo, string serialNo)
        {
            var workOrder = CreateWorkOrders().FirstOrDefault(row =>
                string.Equals(row.WorkOrderNo, workOrderNo, StringComparison.OrdinalIgnoreCase)
                && (string.IsNullOrWhiteSpace(serialNo)
                    || string.Equals(row.SerialNo, serialNo, StringComparison.OrdinalIgnoreCase)));

            if (workOrder == null)
                return new List<RoutingProcessDto>();

            var routingGroup = ResolveRoutingGroup(workOrder.ItemCode);
            var operationCount = routingGroup.StartsWith("MAIN") ? 5 : 4;

            // 진행중 지시는 세 번째 공정이 작업 중이라고 본다(앞은 완료, 뒤는 대기).
            var runningIndex = workOrder.StatusName == "진행중" ? 2 : -1;

            var processes = new List<RoutingProcessDto>(operationCount);
            for (var i = 0; i < operationCount; i++)
            {
                processes.Add(new RoutingProcessDto
                {
                    RoutingGroup = routingGroup,
                    RoutingName = "OP" + ((i + 1) * 10).ToString("D2"),
                    PageName = ((i + 1) * 10).ToString("D4"),
                    ProgressState = ResolveProgressState(workOrder.StatusName, i, runningIndex)
                });
            }

            return processes;
        }

        /// <summary>품목번호 계열(ITM-A/B → 본공정, ITM-C/D → 서브공정)로 라우팅 그룹을 정한다.</summary>
        private static string ResolveRoutingGroup(string itemCode)
        {
            if (string.IsNullOrEmpty(itemCode) || itemCode.Length < 5)
                return "MAIN-01";

            switch (itemCode[4])
            {
                case 'A': return "MAIN-01";
                case 'B': return "MAIN-02";
                case 'C': return "SUB-01";
                default: return "SUB-02";
            }
        }

        /// <summary>작업지시 상태에 맞춘 공정별 진행 상태.</summary>
        private static string ResolveProgressState(string workOrderStatus, int index, int runningIndex)
        {
            if (workOrderStatus == "완료")
                return "완료";

            if (runningIndex < 0)
                return "대기";

            if (index < runningIndex) return "완료";
            if (index == runningIndex) return "진행중";
            return "대기";
        }

        /// <summary>조건이 비어 있으면 통과, 값이 있으면 부분 일치 여부로 판정한다.</summary>
        private static bool Matches(string value, string condition)
        {
            if (string.IsNullOrWhiteSpace(condition))
                return true;

            return value != null
                   && value.IndexOf(condition.Trim(), StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>화면 설계서 샘플 행(작업 지시 정보 목록). 상태/일자 조합이 골고루 나오도록 구성했다.</summary>
        private static List<WorkOrderEntryDto> CreateWorkOrders()
        {
            return new List<WorkOrderEntryDto>
            {
                new WorkOrderEntryDto
                {
                    WorkOrderNo = "WO_260701-01",
                    ItemCode = "ITM-A1001",
                    SerialNo = "SN-260701-0001",
                    OrderQty = 100m,
                    SalesOrderNo = "SO_260625-01",
                    CustomerPoNo = "PO-2606-0012",
                    Fcid = "FC-1001",
                    PlanStartDate = new DateTime(2026, 7, 1),
                    PlanEndDate = new DateTime(2026, 7, 3),
                    WorkStartDate = new DateTime(2026, 7, 1),
                    WorkEndDate = null,
                    StatusName = "진행중",
                    Remark = "1차 양산"
                },
                new WorkOrderEntryDto
                {
                    WorkOrderNo = "WO_260701-02",
                    ItemCode = "ITM-A1002",
                    SerialNo = "SN-260701-0002",
                    OrderQty = 50m,
                    SalesOrderNo = "SO_260625-01",
                    CustomerPoNo = "PO-2606-0012",
                    Fcid = "FC-1002",
                    PlanStartDate = new DateTime(2026, 7, 1),
                    PlanEndDate = new DateTime(2026, 7, 4),
                    WorkStartDate = null,
                    WorkEndDate = null,
                    StatusName = "대기",
                    Remark = null
                },
                new WorkOrderEntryDto
                {
                    WorkOrderNo = "WO_260702-01",
                    ItemCode = "ITM-B2001",
                    SerialNo = "SN-260702-0001",
                    OrderQty = 200m,
                    SalesOrderNo = "SO_260626-03",
                    CustomerPoNo = "PO-2606-0031",
                    Fcid = "FC-2001",
                    PlanStartDate = new DateTime(2026, 7, 2),
                    PlanEndDate = new DateTime(2026, 7, 6),
                    WorkStartDate = new DateTime(2026, 7, 2),
                    WorkEndDate = new DateTime(2026, 7, 5),
                    StatusName = "완료",
                    Remark = "고객 요청 선출하"
                },
                new WorkOrderEntryDto
                {
                    WorkOrderNo = "WO_260702-02",
                    ItemCode = "ITM-B2002",
                    SerialNo = "SN-260702-0002",
                    OrderQty = 80m,
                    SalesOrderNo = "SO_260626-03",
                    CustomerPoNo = "PO-2606-0031",
                    Fcid = "FC-2002",
                    PlanStartDate = new DateTime(2026, 7, 2),
                    PlanEndDate = new DateTime(2026, 7, 7),
                    WorkStartDate = new DateTime(2026, 7, 3),
                    WorkEndDate = null,
                    StatusName = "진행중",
                    Remark = null
                },
                new WorkOrderEntryDto
                {
                    WorkOrderNo = "WO_260703-01",
                    ItemCode = "ITM-C3001",
                    SerialNo = "SN-260703-0001",
                    OrderQty = 120m,
                    SalesOrderNo = "SO_260701-02",
                    CustomerPoNo = "PO-2607-0002",
                    Fcid = "FC-3001",
                    PlanStartDate = new DateTime(2026, 7, 3),
                    PlanEndDate = new DateTime(2026, 7, 8),
                    WorkStartDate = null,
                    WorkEndDate = null,
                    StatusName = "대기",
                    Remark = "자재 입고 대기"
                },
                new WorkOrderEntryDto
                {
                    WorkOrderNo = "WO_260703-02",
                    ItemCode = "ITM-C3002",
                    SerialNo = "SN-260703-0002",
                    OrderQty = 300m,
                    SalesOrderNo = "SO_260701-02",
                    CustomerPoNo = "PO-2607-0002",
                    Fcid = "FC-3002",
                    PlanStartDate = new DateTime(2026, 7, 3),
                    PlanEndDate = new DateTime(2026, 7, 10),
                    WorkStartDate = new DateTime(2026, 7, 4),
                    WorkEndDate = null,
                    StatusName = "진행중",
                    Remark = null
                },
                new WorkOrderEntryDto
                {
                    WorkOrderNo = "WO_260704-01",
                    ItemCode = "ITM-D4001",
                    SerialNo = "SN-260704-0001",
                    OrderQty = 60m,
                    SalesOrderNo = "SO_260702-05",
                    CustomerPoNo = "PO-2607-0018",
                    Fcid = "FC-4001",
                    PlanStartDate = new DateTime(2026, 7, 4),
                    PlanEndDate = new DateTime(2026, 7, 9),
                    WorkStartDate = new DateTime(2026, 7, 4),
                    WorkEndDate = new DateTime(2026, 7, 8),
                    StatusName = "완료",
                    Remark = null
                },
                new WorkOrderEntryDto
                {
                    WorkOrderNo = "WO_260705-01",
                    ItemCode = "ITM-D4002",
                    SerialNo = "SN-260705-0001",
                    OrderQty = 40m,
                    SalesOrderNo = "SO_260702-05",
                    CustomerPoNo = "PO-2607-0018",
                    Fcid = "FC-4002",
                    PlanStartDate = new DateTime(2026, 7, 5),
                    PlanEndDate = new DateTime(2026, 7, 11),
                    WorkStartDate = null,
                    WorkEndDate = null,
                    StatusName = "보류",
                    Remark = "설계 변경 검토중"
                }
            };
        }
    }
}
