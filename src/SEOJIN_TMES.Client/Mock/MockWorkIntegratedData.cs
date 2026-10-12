using System;
using System.Collections.Generic;
using SEOJIN_TMES.Client.Common.Models;

namespace SEOJIN_TMES.Client.Mock
{
    /// <summary>
    /// &lt;작업통합 현황 조회&gt;의 [작업 통합 개요] 트리 목업 데이터.
    /// 값은 화면 설계서(목업 이미지)의 계층과 샘플 행을 그대로 옮긴 것이며, 실제 업무 데이터가 아니다.
    ///
    ///   A001 (품목)
    ///    └ ROUTING_GROUP_01
    ///       └ OP10
    ///          ├ 0010 ~ 0080 (페이지 공정)
    ///          └ 0080 > M001 (투입자재) > TORQUE_01 (스팩, 값 있음)
    ///
    /// App.config의 <c>UseMockData</c>가 true일 때만 사용된다.
    /// </summary>
    public static class MockWorkIntegratedData
    {
        /// <summary>
        /// 시리얼번호 하나에 대한 작업 통합 개요.
        /// 샘플에 없는 시리얼번호면 빈 목록을 돌려준다 — 화면의 "조회 결과 없음" 경로를 그대로 탄다.
        /// </summary>
        public static List<WorkIntegratedNodeDto> GetOverview(string serialNo)
        {
            if (!string.Equals(serialNo?.Trim(), "SER-01", StringComparison.OrdinalIgnoreCase))
                return new List<WorkIntegratedNodeDto>();

            var nodes = new List<WorkIntegratedNodeDto>();
            var nextId = 1;

            var item = Node(ref nextId, 0, "A001", "생산품목 A001", WorkIntegratedNodeKind.Item);
            nodes.Add(item);

            var routingGroup = Node(ref nextId, item.NodeId, "ROUTING_GROUP_01", "메인 라우팅 그룹", WorkIntegratedNodeKind.RoutingGroup);
            nodes.Add(routingGroup);

            var routingOperation = Node(ref nextId, routingGroup.NodeId, "OP10", "조립 공정", WorkIntegratedNodeKind.RoutingOperation);
            nodes.Add(routingOperation);

            // 페이지 공정 0010 ~ 0080. 마지막 공정에만 투입자재/스팩을 달아 값 컬럼이 채워지는 줄을 보여 준다.
            for (var i = 1; i <= 8; i++)
            {
                var pageCode = (i * 10).ToString("D4");
                var pageOperation = Node(ref nextId, routingOperation.NodeId, pageCode, "페이지 공정 " + pageCode,
                    WorkIntegratedNodeKind.PageOperation);
                nodes.Add(pageOperation);

                if (i != 8)
                    continue;

                var material = Node(ref nextId, pageOperation.NodeId, "M001", "투입자재 M001", WorkIntegratedNodeKind.Material);
                nodes.Add(material);

                var spec = Node(ref nextId, material.NodeId, "TORQUE_01", "토크 스팩 01", WorkIntegratedNodeKind.Spec);
                spec.TorqueResult = 6.180m;
                spec.TorqueTarget = 6.000m;
                spec.TorqueLower = 5.400m;
                spec.TorqueMax = 6.600m;
                spec.AngleTarget = 90.000m;
                spec.AngleLower = 85.000m;
                spec.AngleUpper = 95.000m;
                spec.AngleResult = 91.200m;
                nodes.Add(spec);
            }

            return nodes;
        }

        private static WorkIntegratedNodeDto Node(ref int nextId, int parentNodeId, string code, string description, string kind)
        {
            var node = new WorkIntegratedNodeDto
            {
                NodeId = nextId++,
                ParentNodeId = parentNodeId,
                NodeCode = code,
                NodeDescription = description,
                NodeKind = kind
            };

            return node;
        }
    }
}
