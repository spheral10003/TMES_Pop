namespace SEOJIN_TMES.Client.Common.Models
{
    public class ItemQuery
    {
        public string CorpCode { get; set; }
        /// <summary>품목코드.</summary>
        public string ItemCode { get; set; }
        /// <summary>품목명.</summary>
        public string ItemName { get; set; }
        /// <summary> 품목 계정 /// </summary>
        public string ItemAcnt { get; set; }
        /// <summary> 품목 유형 /// </summary>
        public string itemType { get; set; }
        /// <summary> 사용 가능 여부 /// </summary>
        public string Usable { get; set; }
        
        
    }
}
