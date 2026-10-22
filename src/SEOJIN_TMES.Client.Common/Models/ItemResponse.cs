using Newtonsoft.Json;

namespace SEOJIN_TMES.Client.Common.Models
{
    public class ItemResponse
    {

        /// <summary>품목코드.</summary>
        [JsonProperty("itemCd")] public string ItemCode { get; set; }
        /// <summary>품목명.</summary>
        [JsonProperty("itemNm")] public string ItemName { get; set; }
        /// <summary> 품목 계정 /// </summary>
        [JsonProperty("itemAcnt")] public string ItemAcnt { get; set; }
        /// <summary> 품목 유형 /// </summary>
        [JsonProperty("itemType")] public string itemType { get; set; }
        /// <summary> 사용 가능 여부 /// </summary>
        [JsonProperty("useYn")] public string Usable { get; set; }
        /// <summary>모델.</summary>
        [JsonProperty("mdl")] public string Model { get; set; }
        /// <summary>규격.</summary>
        [JsonProperty("spec")] public string Specification { get; set; }
        /// <summary>단위.</summary>
        [JsonProperty("unit")] public string Unit { get; set; }
        /// <summary>도면번호.</summary>
        [JsonProperty("flrplnNo")] public string FloorPlanNo { get; set; }
        /// <summary>고객 품목코드.</summary>
        [JsonProperty("custItemCd")] public string CustomerItemCode { get; set; }
        /// <summary>비고.</summary>
        [JsonProperty("remark")] public string Remark { get; set; }
    }
}
