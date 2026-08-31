using System;

namespace SEOJIN_MEIS.Client.Common.Models
{
    /// <summary>서버에 보고/조회하는 설비 상태 스냅샷 (소켓으로 받은 원시 데이터를 가공한 결과).</summary>
    public class EquipmentStatusDto
    {
        public string EquipmentId { get; set; }
        public string EquipmentName { get; set; }
        public string RunState { get; set; }
        public string AlarmCode { get; set; }
        public string AlarmMessage { get; set; }
        public int ProductionCount { get; set; }
        public DateTime ReportedAtUtc { get; set; }
    }
}
