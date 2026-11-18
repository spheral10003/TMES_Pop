using SEOJIN_TMES.Client.Common.Models;
using System.Collections.Generic;
using System.Linq;

namespace SEOJIN_TMES.Client.Forms.WorkIntegratedStatus
{
    /// <summary>TreeList 바인딩용 노드 (RoutingItem을 감싸고 Children을 가진다)</summary>
    public class WorkIntegratedNode
    {
        public WorkIntegratedNode(WorkIntegratedDto item) { Item = item; }

        public WorkIntegratedDto Item { get; }
        public List<WorkIntegratedNode> Children { get; } = new List<WorkIntegratedNode>();

        // TreeList 컬럼은 "프로퍼티 이름"으로 바인딩되므로 필요한 값만 노출
        public string Name => Item.NodeDescription;
        public string Code => Item.NodeCode;
        /// <summary>투입자재 품목코드/시리얼번호(서버 materialInputCode). 화면 컬럼 FieldName "Serial"과 이름이 같아야 값이 나온다.</summary>
        public string Serial => Item.WholeInputMaterialCode;
        /// <summary>구매 계획 방식(서버 prchsPlanMth). 화면 컬럼 FieldName "PrchsPlanMth"와 이름이 같아야 값이 나온다.</summary>
        public string PrchsPlanMth => Item.PurchasePlanMethod;
        public string Type => Item.NodeType;
        public int Level => Item.NodeLevel;
        public int SortSeq => Item.SortSeq;

        public string Unit => Item.Unit;
        public decimal? UsageQty => Item.UsageQty;

        public string TorqueUnit => Item.TorqueUnit;
        public decimal? TorqueLower => Item.TorqueLower;
        public decimal? TorqueTarget => Item.TorqueTarget;
        public decimal? TorqueUpper => Item.TorqueUpper;
        public decimal? TorqueResult => Item.TorqueResult;

        public string AngleUnit => Item.AngleUnit;
        public decimal? AngleLower => Item.AngleLower;
        public decimal? AngleTarget => Item.AngleTarget;
        public decimal? AngleUpper => Item.AngleUpper;
        public decimal? AngleResult => Item.AngleResult;

        /// <summary>평면 리스트 → 루트 노드 목록(트리)</summary>
        public static List<WorkIntegratedNode> BuildTree(IEnumerable<WorkIntegratedDto> items)
        {
            // 같은 부모 안에서 sortSeq 순서가 되도록 미리 정렬 (BOM/검사항목 동순위는 Type으로 2차 정렬)
            var ordered = items
                .OrderBy(x => x.NodeLevel)
                .ThenBy(x => x.SortSeq)
                .ThenBy(x => x.NodeType)
                .ToList();

            // code는 중복되므로 SysObjId를 키로 사용
            var map = ordered.ToDictionary(x => x.NodeId, x => new WorkIntegratedNode(x));
            var roots = new List<WorkIntegratedNode>();

            foreach (var item in ordered)
            {
                var node = map[item.NodeId];

                if (!string.IsNullOrEmpty(item.ParentNodeId) &&
                    map.TryGetValue(item.ParentNodeId, out var parent))
                    parent.Children.Add(node);   // 이미 정렬된 순서대로 추가됨
                else
                    roots.Add(node);             // parentRef가 없거나 부모를 못 찾으면 루트
            }
            return roots;
        }
    }
}
