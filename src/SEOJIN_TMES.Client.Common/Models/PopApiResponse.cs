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
        [JsonProperty("sysObjId")] public string WorkOrderRef { get; set; }
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
        [JsonProperty("subRoutingProcImagePath")] public string ProcImagePath { get; set; }

        [JsonProperty("equipmentList")] public List<EquipmentApi> EquipmentList { get; set; } = new List<EquipmentApi>();
        [JsonProperty("inputMaterialList")] public List<InputMaterialApi> InputMaterialList { get; set; } = new List<InputMaterialApi>();
        //[JsonProperty("bomInspSpecList")] public List<BomInspSpecApi> BomInspSpecList { get; set; } = new List<BomInspSpecApi>();
        [JsonProperty("bomInspSpec")] public BomInspSpecGroupApi BomInspSpec { get; set; } = new BomInspSpecGroupApi();
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
        [JsonProperty("mdlNm")] public string EqpModel { get; set; }
        [JsonProperty("remark")] public string Remark { get; set; }
        
    }

    public class InputMaterialApi
    {
        [JsonProperty("bomRef")] public string BomRef { get; set; }
        [JsonProperty("inputItemCd")] public string InputItemCd { get; set; }
        [JsonProperty("inputItemNm")] public string InputItemNm { get; set; }
        [JsonProperty("usageQty")] public decimal UsageQty { get; set; }
        [JsonProperty("unit")] public string Unit { get; set; }
        [JsonProperty("indctLoc")] public string IndctLoc { get; set; }        // 투입위치
        [JsonProperty("inputItemGrp")] public string InputItemGrp { get; set; } // 자재그룹
        [JsonProperty("serialVrfcYn")] public string SerialVrfcYn { get; set; }
        [JsonProperty("inputtedSerials")] public string InputtedSerials { get; set; } // 구분자 형식은 서버 확인 필요

        /// <summary>품목번호 확인 대상 여부(Y/N). 서버는 이 확인도 공정의 할 일로 센다 — 빠뜨리면 완료가 WORK_005로 거절된다.</summary>
        [JsonProperty("itemCdVrfcYn")] public string ItemCdVrfcYn { get; set; }

        /// <summary>품목번호 입력이 끝났는지(서버 판정). 확인 대상이 아니거나 아직 입력 전이면 null로 온다.</summary>
        [JsonProperty("itemCdInputCompleted")] public bool? ItemCdInputCompleted { get; set; }
    }

    public class BomInspSpecGroupApi
    {
        [JsonProperty("bomInspSpecList")] public List<BomInspSpecApi> BomInspSpecList { get; set; } = new List<BomInspSpecApi>();

        /// <summary>
        /// 수동 렌치(작업자 직접 입력) 허용 여부. 서버 기준정보 값이다.
        /// null로 올 수도 있어 bool?로 받고, 값이 없으면 허용하지 않는 쪽(false)으로 본다.
        /// </summary>
        [JsonProperty("manualEnabled")] public bool? ManualEnabled { get; set; }
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
        [JsonProperty("indctLoc")] public string IndctLoc { get; set; }        // 투입위치
        [JsonProperty("inptMth")] public string InptMth { get; set; }          // EQP / 그 외=수동
        [JsonProperty("torqueRsltVl")] public decimal? TorqueRsltVl { get; set; }
        [JsonProperty("angleRsltVl")] public decimal? AngleRsltVl { get; set; }
        [JsonProperty("eqpCd")] public string EquipmentCode { get; set; }

    }

    public class InspChkItemApi
    {
        [JsonProperty("inspItemCd")] public string InspItemCode { get; set; }
        [JsonProperty("inspItemNm")] public string InspItemNm { get; set; }
        [JsonProperty("indctLoc")] public string IndctLoc { get; set; }        // 투입위치
        [JsonProperty("inspDataType")] public string InspDataType { get; set; } // QLT 정성 / QNT 정량
        [JsonProperty("lsl")] public decimal? Lsl { get; set; }                  // 최소값 — 정량 검사만
        [JsonProperty("target")] public decimal? Target { get; set; }            // 타겟값
        [JsonProperty("usl")] public decimal? Usl { get; set; }                  // 최대값
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
        public bool BomManualEnabled { get; set; }
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

        public static PopScreenData Map(PopApiResponse data)
        {
            var eq = data.EquipmentList.FirstOrDefault();
            var bom = data.BomInspSpec ?? new BomInspSpecGroupApi();
            var bomList = bom.BomInspSpecList ?? new List<BomInspSpecApi>();

            return new PopScreenData
            {
                Process = new PopProcessInfoDto
                {
                    WorkOrderRef = data.WorkOrderRef,
                    WorkOrderNo = data.WoNo,
                    ItemCode = data.ItemCd,
                    SerialNo = data.Sn,
                    RoutingGroup = data.RoutingGroupCd,
                    RoutingOperation = data.RoutingCd,
                    PageOperation = data.ProcCd,
                    PageStartTime = data.TrackInDtm,
                    PageEndTime = data.TrackOutDtm,
                    RunTime = TimeSpan.FromSeconds(data.ActlWorkDurSec),
                    IdleTime = TimeSpan.FromSeconds(data.DttmDurSec),
                    WorkTime = TimeSpan.FromSeconds(data.TotalWorkDurSec),
                    WorkstationCode = eq?.WkstnCd,
                    WorkstationName = eq?.WkstnNm,
                    ProgressState = data.WorkStatus,   // 필요 시 Ready→"대기" 등 변환
                    DrawingFilePath = data.ProcImagePath,          // 응답에 없음 → 별도 API/필드 필요
                },

                Materials = data.InputMaterialList.Select(m => new PopInputMaterialDto
                {
                    MaterialCode = m.InputItemCd,
                    ItemCode = m.InputItemCd,
                    InputQty = (int)m.UsageQty,
                    InputUnit = m.Unit,
                    InputPosition = m.IndctLoc,
                    MaterialGroup = m.InputItemGrp,
                    SerialCheckRequired = m.SerialVrfcYn == "Y",
                    ItemCodeCheckRequired = m.ItemCdVrfcYn == "Y",
                    ItemCodeInputCompleted = m.ItemCdInputCompleted == true,
                    SerialNumbers = string.IsNullOrWhiteSpace(m.InputtedSerials)
                        ? new List<string>()
                        : m.InputtedSerials.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                           .Select(s => s.Trim()).ToList()
                }).ToList(),

                BomSpecs = bomList.Select(b => new PopBomSpecDto
                {
                    MaterialCode = b.InputItemCd,
                    Seq = b.Seq,
                    InputPosition = b.IndctLoc,
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
                    EquipmentCode = b.EquipmentCode,
                    IsManualInput = !string.Equals(b.InptMth, "EQP", StringComparison.OrdinalIgnoreCase)
                }).ToList(),

                BomManualEnabled = bom.ManualEnabled == true,

                InspectionSpecs = data.InspChkItemList.Select(i => new PopInspectionSpecDto
                {
                    InspectionItemCode = i.InspItemCode,
                    InspectionSpecName = i.InspItemNm,
                    InputPosition = i.IndctLoc,
                    InspectionRemark = i.InspRemark,
                    // 서버 코드(QNT / QLT)를 그대로 둔다 — 정량/정성 문구는 그리드가 화면 언어로 바꿔 보여 준다.
                    InspectionMethod = i.InspDataType,
                    InspectionLower = i.Lsl,
                    InspectionTarget = i.Target,
                    InspectionUpper = i.Usl,
                    // 판정은 서버 코드(OK / NG)를 그대로 쓴다 — 화면 표시도 저장 Body도 같은 값이다.
                    ResultState = i.JgmtRslt == PopInspectionSpecDto.ResultOk || i.JgmtRslt == PopInspectionSpecDto.ResultNg
                        ? i.JgmtRslt
                        : "",
                    ResultValue = i.InspDataType == PopInspectionSpecDto.MethodQuantitative ? i.InspRsltVl : null
                }).ToList(),

                Equipments = data.EquipmentList.Select(e => new PopEquipmentDto
                {
                    //Id = int.Parse(e.EqpCd),
                    EquipmentCode = e.EqpCd,
                    EquipmentName = e.EqpNm,
                    EquipmentIp = e.EqpIp,
                    EquipmentPort = e.EqpPort?.ToString(),
                    EquipmentType = e.EqpType,
                    EquipmentRemark = e.Remark,
                    EquipmentState = e.EqpStatus = "사용불가",
                    EquipmentModel = e.EqpModel,
                    RangeLower = e.TorqueMinVl,
                    RangeUpper = e.TorqueMaxVl,
                    CommunicationState = "Disconnected"
                    // CommunicationState: Modbus 연결 상태로 클라이언트에서 채움
                }).ToList()
            };
        }


    }
}
