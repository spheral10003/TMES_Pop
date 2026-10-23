using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEOJIN_TMES.Client.Common.Models
{
    /// <summary>
    /// 서버 메뉴 항목 — <c>GET api/v1.0/sm/menus</c> 응답의 알맹이 한 건.
    /// 클라이언트는 메뉴 구조(아이콘 · 화면 연결)를 <c>MenuCatalog</c>에 갖고 있고,
    /// 서버에서는 <b>어떤 화면을 열 수 있는지</b>만 받는다 — 둘은 <see cref="MenuCode"/>(POP01, POP02 …)로 맞춘다.
    /// 필드명은 실제 응답(2026-10-23 로그)에서 확인했다. 응답에는 이 외에도 menuNmKo/En/Vi, sortSeq, fldrYn 등이 오지만
    /// 메뉴 문구 · 순서 · 분류 구조는 클라이언트 <c>MenuCatalog</c>가 정하므로 받지 않는다.
    /// (fldrYn은 운영상 항상 "N"이라 화면 구분에 쓸 수 없다 — 화면 여부는 클라이언트 MenuNode가 판단한다.)
    /// </summary>
    public class MenuDto
    {
        /// <summary>메뉴 코드 — 서버 <c>menuCd</c>(POP01 …). <c>MenuCatalog</c> 화면 노드의 MenuCode와 매칭하는 키다.</summary>
        [JsonProperty("menuCd")]
        public string MenuCode { get; set; }

        /// <summary>메뉴 이름 — 서버 <c>menuNm</c>(로그인 언어 기준). 로그 표시용으로만 쓴다.</summary>
        [JsonProperty("menuNm")]
        public string MenuName { get; set; }

        /// <summary>사용 여부 — 서버 <c>useYn</c>("Y"/"N"). "Y"만 화면에 표시한다.</summary>
        [JsonProperty("useYn")]
        public string UseYn { get; set; }

        /// <summary>메뉴 그룹 — 서버 <c>menuGroupCd</c>. POP 클라이언트는 항상 "POP"으로 조회한다.</summary>
        [JsonProperty("menuGroupCd")]
        public string MenuGroupCode { get; set; }

        [JsonIgnore]
        public bool IsUsed => string.Equals(UseYn, "Y", StringComparison.OrdinalIgnoreCase);
    }
}
