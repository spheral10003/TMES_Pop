using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEOJIN_TMES.Client.Common.Models
{
    public class ApiResponse<T>
    {
        [JsonProperty("data")] public T Data { get; set; }
        [JsonProperty("status")] public int Status { get; set; }
        [JsonProperty("message")] public string Message { get; set; }
        [JsonProperty("code")] public string Code { get; set; }
        [JsonProperty("totalCount")] public int TotalCount { get; set; }
    }

    public class PopApiResponse
    {
        [JsonProperty("woNo")] public string WoNo { get; set; }
        [JsonProperty("itemCd")] public string ItemCd { get; set; }
        [JsonProperty("sn")] public string Sn { get; set; }
        [JsonProperty("routingGroupCd")] public string RoutingGroupCd { get; set; }
        [JsonProperty("routingCd")] public string RoutingCd { get; set; }
        [JsonProperty("procCd")] public string ProcCd { get; set; }
        [JsonProperty("workStatus")] public string WorkStatus { get; set; }
        [JsonProperty("trackInDtm")] public DateTime? TrackInDtm { get; set; }
        [JsonProperty("trackOutDtm")] public DateTime? TrackOutDtm { get; set; }
        [JsonProperty("actlWorkDurSec")] public double ActlWorkDurSec { get; set; }
        [JsonProperty("dttmDurSec")] public double DttmDurSec { get; set; }
        [JsonProperty("totalWorkDurSec")] public double TotalWorkDurSec { get; set; }

        [JsonProperty("equipmentList")] public List<EquipmentApi> EquipmentList { get; set; } = new List<EquipmentApi>();
        [JsonProperty("inputMaterialList")] public List<InputMaterialApi> InputMaterialList { get; set; } = new List<InputMaterialApi>();
        [JsonProperty("bomInspSpecList")] public List<BomInspSpecApi> BomInspSpecList { get; set; } = new List<BomInspSpecApi>();
        [JsonProperty("inspChkItemList")] public List<InspChkItemApi> InspChkItemList { get; set; } = new List<InspChkItemApi>();
    }

    public class EquipmentApi
    {
        [JsonProperty("eqpCd")] public string EqpCd { get; set; }
        [JsonProperty("eqpNm")] public string EqpNm { get; set; }
        [JsonProperty("eqpStatus")] public string EqpStatus { get; set; }
        [JsonProperty("eqpType")] public string EqpType { get; set; }
        [JsonProperty("eqpIp")] public string EqpIp { get; set; }
        [JsonProperty("eqpPort")] public int? EqpPort { get; set; }
        [JsonProperty("wkstnCd")] public string WkstnCd { get; set; }
        [JsonProperty("wkstnNm")] public string WkstnNm { get; set; }
        [JsonProperty("torqueMinVl")] public decimal? TorqueMinVl { get; set; }
        [JsonProperty("torqueMaxVl")] public decimal? TorqueMaxVl { get; set; }
        [JsonProperty("remark")] public string Remark { get; set; }
    }

    public class InputMaterialApi
    {
        [JsonProperty("bomRef")] public string BomRef { get; set; }
        [JsonProperty("inputItemCd")] public string InputItemCd { get; set; }
        [JsonProperty("inputItemNm")] public string InputItemNm { get; set; }
        [JsonProperty("usageQty")] public decimal UsageQty { get; set; }
        [JsonProperty("unit")] public string Unit { get; set; }
        [JsonProperty("serialVrfcYn")] public string SerialVrfcYn { get; set; }
        [JsonProperty("inputtedSerials")] public string InputtedSerials { get; set; } // 구분자 형식은 서버 확인 필요
    }

    public class BomInspSpecApi
    {
        [JsonProperty("bomRef")] public string BomRef { get; set; }
        [JsonProperty("inputItemCd")] public string InputItemCd { get; set; }
        [JsonProperty("torqueLsl")] public decimal? TorqueLsl { get; set; }   // "8.0" 문자열도 자동 변환
        [JsonProperty("torqueTarget")] public decimal? TorqueTarget { get; set; }
        [JsonProperty("torqueUsl")] public decimal? TorqueUsl { get; set; }
        [JsonProperty("torqueUnit")] public string TorqueUnit { get; set; }
        [JsonProperty("angleLsl")] public decimal? AngleLsl { get; set; }
        [JsonProperty("angleTarget")] public decimal? AngleTarget { get; set; }
        [JsonProperty("angleUsl")] public decimal? AngleUsl { get; set; }
        [JsonProperty("angleUnit")] public string AngleUnit { get; set; }
        [JsonProperty("seq")] public int Seq { get; set; }
        [JsonProperty("inptMth")] public string InptMth { get; set; }          // EQP / 그 외=수동
        [JsonProperty("torqueRsltVl")] public decimal? TorqueRsltVl { get; set; }
        [JsonProperty("angleRsltVl")] public decimal? AngleRsltVl { get; set; }
    }

    public class InspChkItemApi
    {
        [JsonProperty("inspItemNm")] public string InspItemNm { get; set; }
        [JsonProperty("inspDataType")] public string InspDataType { get; set; } // QLT 정성 / QNT 정량
        [JsonProperty("jgmtRslt")] public string JgmtRslt { get; set; }         // OK / NG
        [JsonProperty("inspRsltVl")] public string InspRsltVl { get; set; }
        [JsonProperty("inspRemark")] public string InspRemark { get; set; }
    }

    // ===== 2. 역직렬화 + DTO 매핑 =====
    public class PopScreenData
    {
        public PopProcessInfoDto Process { get; set; }
        public List<PopInputMaterialDto> Materials { get; set; }
        public List<PopBomSpecDto> BomSpecs { get; set; }
        public List<PopInspectionSpecDto> InspectionSpecs { get; set; }
        public List<PopEquipmentDto> Equipments { get; set; }
    }


    public static class PopMapper
    {
        public static PopScreenData Parse(string json)
        {
            var res = JsonConvert.DeserializeObject<ApiResponse<PopApiResponse>>(json);
            if (res == null || res.Status != 200 || res.Data == null)
                throw new InvalidOperationException($"조회 실패: {res?.Message}");
            return Map(res.Data);
        }

        public static PopScreenData Map(PopApiResponse d)
        {
            var eq = d.EquipmentList.FirstOrDefault();

            return new PopScreenData
            {
                Process = new PopProcessInfoDto
                {
                    WorkOrderNo = d.WoNo,
                    ItemCode = d.ItemCd,
                    SerialNo = d.Sn,
                    RoutingGroup = d.RoutingGroupCd,
                    RoutingOperation = d.RoutingCd,
                    PageOperation = d.ProcCd,
                    PageStartTime = d.TrackInDtm,
                    PageEndTime = d.TrackOutDtm,
                    RunTime = TimeSpan.FromSeconds(d.ActlWorkDurSec),
                    IdleTime = TimeSpan.FromSeconds(d.DttmDurSec),
                    WorkTime = TimeSpan.FromSeconds(d.TotalWorkDurSec),
                    WorkstationCode = eq?.WkstnCd,
                    WorkstationName = eq?.WkstnNm,
                    ProgressState = d.WorkStatus,   // 필요 시 Ready→"대기" 등 변환
                    DrawingFilePath = null          // 응답에 없음 → 별도 API/필드 필요
                },

                Materials = d.InputMaterialList.Select(m => new PopInputMaterialDto
                {
                    MaterialCode = m.InputItemCd,
                    ItemCode = m.InputItemCd,
                    InputQty = (int)m.UsageQty,
                    InputUnit = m.Unit,
                    SerialCheckRequired = m.SerialVrfcYn == "Y",
                    SerialNumbers = string.IsNullOrWhiteSpace(m.InputtedSerials)
                        ? new List<string>()
                        : m.InputtedSerials.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                           .Select(s => s.Trim()).ToList()
                }).ToList(),

                BomSpecs = d.BomInspSpecList.Select(b => new PopBomSpecDto
                {
                    MaterialCode = b.InputItemCd,
                    InputPosition = b.Seq.ToString(),
                    BomSpecName = $"토크작업{b.Seq:00}",
                    SpecType = "토크 작업",
                    TorqueLower = b.TorqueLsl,
                    TorqueTarget = b.TorqueTarget,
                    TorqueUpper = b.TorqueUsl,
                    TorqueUnit = b.TorqueUnit,
                    TorqueResult = b.TorqueRsltVl,
                    AngleLower = b.AngleLsl,
                    AngleTarget = b.AngleTarget,
                    AngleUpper = b.AngleUsl,
                    AngleUnit = b.AngleUnit,
                    AngleResult = b.AngleRsltVl,
                    IsManualInput = !string.Equals(b.InptMth, "EQP", StringComparison.OrdinalIgnoreCase)
                }).ToList(),

                InspectionSpecs = d.InspChkItemList.Select(i => new PopInspectionSpecDto
                {
                    InspectionSpecName = i.InspItemNm,
                    InspectionRemark = i.InspRemark,
                    InspectionMethod = i.InspDataType == "QNT" ? "정량" : "정성",
                    ResultState = i.JgmtRslt == "OK" ? "합격" : i.JgmtRslt == "NG" ? "불합격" : "",
                    ResultValue = i.InspDataType == "QNT" ? i.InspRsltVl : null
                }).ToList(),

                Equipments = d.EquipmentList.Select(e => new PopEquipmentDto
                {
                    EquipmentCode = e.EqpCd,
                    EquipmentName = e.EqpNm,
                    EquipmentIp = e.EqpIp,
                    EquipmentPort = e.EqpPort?.ToString(),
                    EquipmentType = e.EqpType,
                    EquipmentRemark = e.Remark,
                    EquipmentState = e.EqpStatus == "Active" ? "가동가능" : "사용불가",
                    RangeLower = e.TorqueMinVl,
                    RangeUpper = e.TorqueMaxVl
                    // CommunicationState: Modbus 연결 상태로 클라이언트에서 채움
                }).ToList()
            };
        }


    }
}
