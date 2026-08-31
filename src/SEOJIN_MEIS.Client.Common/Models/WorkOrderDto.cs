using System;

namespace SEOJIN_MEIS.Client.Common.Models
{
    /// <summary>작업지시 조회 결과. 서버가 실제 판정/계산 로직을 갖고, 클라이언트는 표시만 담당.</summary>
    public class WorkOrderDto
    {
        public string WorkOrderNo { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string ProcessCode { get; set; }
        public string ProcessName { get; set; }
        public string EquipmentId { get; set; }
        public decimal PlanQty { get; set; }
        public decimal GoodQty { get; set; }
        public decimal DefectQty { get; set; }
        public string Status { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
    }

    /// <summary>생산 실적 등록 요청. 실제 계산(양품/불량 집계 등)은 서버가 처리.</summary>
    public class ProductionResultRequest
    {
        public string WorkOrderNo { get; set; }
        public string EquipmentId { get; set; }
        public decimal GoodQty { get; set; }
        public decimal DefectQty { get; set; }
        public string DefectCode { get; set; }
        public string LotNo { get; set; }
        public string UserId { get; set; }
    }

    public class ProductionStatusDto
    {
        public string WorkOrderNo { get; set; }
        public string ItemName { get; set; }
        public string EquipmentId { get; set; }
        public decimal PlanQty { get; set; }
        public decimal GoodQty { get; set; }
        public decimal DefectQty { get; set; }
        public decimal AchievementRate { get; set; }
        public string ShiftCode { get; set; }
        public DateTime StandardDate { get; set; }
    }

    public class WorkIntegratedStatusDto
    {
        public string DeptCode { get; set; }
        public string DeptName { get; set; }
        public string LineCode { get; set; }
        public string LineName { get; set; }
        public int TotalWorkOrderCount { get; set; }
        public int InProgressCount { get; set; }
        public int CompletedCount { get; set; }
        public decimal TotalPlanQty { get; set; }
        public decimal TotalGoodQty { get; set; }
        public decimal TotalDefectQty { get; set; }
    }
}
