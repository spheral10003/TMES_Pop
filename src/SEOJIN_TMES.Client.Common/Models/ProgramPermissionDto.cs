using Newtonsoft.Json;
using System;

namespace SEOJIN_TMES.Client.Common.Models
{
    /// <summary>
    /// 로그인 사용자의 화면(프로그램)별 버튼 권한 — <c>GET api/v1.0/gm/auth/permissions?path=/POP01</c> 응답의 알맹이.
    /// 필드명은 구미 Test 서버 Swagger(2026-11-11, <c>AuthConfigurationFrameDto</c>)에서 확인했다.
    /// 값은 Y / N / DEFAULT 중 하나이며, 클라이언트는 <b>"Y"일 때만 허용</b>으로 본다 —
    /// 권한 판정은 서버가 하고 화면은 받은 값으로 버튼을 보이거나 숨기기만 한다.
    /// </summary>
    public class ProgramPermissionDto
    {
        /// <summary>조회 권한 — 서버 <c>srchAuthYn</c>.</summary>
        [JsonProperty("srchAuthYn")]
        public string SearchYn { get; set; }

        /// <summary>저장 권한 — 서버 <c>strgAuthYn</c>.</summary>
        [JsonProperty("strgAuthYn")]
        public string SaveYn { get; set; }

        /// <summary>삭제 권한 — 서버 <c>delAuthYn</c>.</summary>
        [JsonProperty("delAuthYn")]
        public string DeleteYn { get; set; }

        /// <summary>취소 권한 — 서버 <c>cancelAuthYn</c>.</summary>
        [JsonProperty("cancelAuthYn")]
        public string CancelYn { get; set; }

        /// <summary>출력 권한 — 서버 <c>prnotAuthYn</c>.</summary>
        [JsonProperty("prnotAuthYn")]
        public string PrintYn { get; set; }

        /// <summary>단가 권한 — 서버 <c>untprcAuthYn</c>. POP 화면에는 단가가 없어 지금은 쓰지 않는다.</summary>
        [JsonProperty("untprcAuthYn")]
        public string UnitPriceYn { get; set; }

        [JsonIgnore] public bool CanSearch => IsYes(SearchYn);
        [JsonIgnore] public bool CanSave => IsYes(SaveYn);
        [JsonIgnore] public bool CanDelete => IsYes(DeleteYn);
        [JsonIgnore] public bool CanCancel => IsYes(CancelYn);
        [JsonIgnore] public bool CanPrint => IsYes(PrintYn);

        /// <summary>
        /// 모든 권한이 없는 상태. 서버가 권한 행을 내려주지 않았거나(data=null) 조회에 실패했을 때 쓴다 —
        /// 권한을 확인하지 못한 채 버튼을 열어 두지 않기 위해서다.
        /// </summary>
        public static ProgramPermissionDto None() => new ProgramPermissionDto();

        /// <summary>로그용 한 줄 요약(조회/저장/삭제/취소/출력).</summary>
        public override string ToString() =>
            $"srch={SearchYn ?? "-"} strg={SaveYn ?? "-"} del={DeleteYn ?? "-"} cancel={CancelYn ?? "-"} prnot={PrintYn ?? "-"}";

        private static bool IsYes(string value) => string.Equals(value, "Y", StringComparison.OrdinalIgnoreCase);
    }
}
