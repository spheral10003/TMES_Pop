using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEOJIN_TMES.Client.Common.Models
{
    // <summary>
    /// 서버 메뉴 항목 — <c>GET api/v1.0/sm/menus</c> 응답의 알맹이 한 건.
    /// 클라이언트는 메뉴 구조(아이콘 · 화면 연결)를 <c>MenuCatalog</c>에 갖고 있고,
    /// 서버에서는 <b>어떤 메뉴를 보여 줄지(useYn)</b>만 받는다.
    ///
    /// TODO: 필드명은 쿼리 파라미터명(menuNm, useYn, menuGroupCd)과 같다고 <b>가정</b>했다.
    /// 서버 응답 스펙이 확정되면 <see cref="JsonPropertyAttribute"/>만 맞출 것.
    /// </summary>
    public class MenuDto
    {
        /// <summary>메뉴 이름 — 서버 <c>menuNm</c>. <c>MenuCatalog</c>의 노드 Key(POP 등)와 같은 값이다.</summary>
        [JsonProperty("menuNm")]
        public string MenuName { get; set; }

        /// <summary>사용 여부 — 서버 <c>useYn</c>("Y"/"N"). "Y"만 화면에 표시한다.</summary>
        [JsonProperty("useYn")]
        public string UseYn { get; set; }

        /// <summary>메뉴 그룹 — 서버 <c>menuGroupCd</c>. POP 클라이언트는 항상 "POP"으로 조회한다.</summary>
        [JsonProperty("menuGroupCd")]
        public string MenuGroupCode { get; set; }

        [JsonIgnore]
        public bool IsUsed => string.Equals(UseYn, "Y", System.StringComparison.OrdinalIgnoreCase);
    }
}
