using Newtonsoft.Json;

namespace SEOJIN_TMES.Client.Common.Models
{
    /// <summary>
    /// 로그인 사용자 프로필(사용자 정보 팝업에 표시).
    /// 서버 <c>GET api/v1.0/gm/users/{userId}</c> 응답의 알맹이(Swagger 정의상 <c>WithCorpDetailResponse</c>)이며,
    /// 클라이언트는 값 판단 없이 화면에 표시만 한다.
    ///
    /// ⚠️ 서버는 이것을 <c>{ "data": { ... } }</c>로 한 겹 싸서 내려준다(<c>APIResponse«WithCorpDetailResponse»</c>).
    /// 받을 때 <see cref="ApiDataResponse{T}"/>로 감싸지 않으면 모든 값이 null이 된다.
    ///
    /// 속성명은 화면 쪽에서 읽기 쉬운 이름을 쓰고, 서버 필드명은 <see cref="JsonPropertyAttribute"/>로 맞춘다
    /// (서버가 필드명을 바꾸면 이 파일만 고치면 된다).
    /// </summary>
    public class UserProfileDto
    {
        /// <summary>사번 — 서버 <c>orgNo</c>(로그인 아이디와 같은 값이다).</summary>
        [JsonProperty("orgNo")]
        public string EmployeeNo { get; set; }

        /// <summary>이름 — 서버 <c>userNm</c>.</summary>
        [JsonProperty("userNm")]
        public string UserName { get; set; }

        /// <summary>연락처 — 서버 <c>cellphone</c>(휴대전화).</summary>
        [JsonProperty("cellphone")]
        public string PhoneNumber { get; set; }

        /// <summary>부서명 — 서버 <c>deptNm</c>.</summary>
        [JsonProperty("deptNm")]
        public string DepartmentName { get; set; }

        /// <summary>
        /// 부서 코드 — 서버 <c>deptCd</c>.
        /// ⚠️ 현재 <c>WithCorpDetailResponse</c>에는 이 필드가 <b>없어서 항상 비어 있다</b>(부서명만 내려온다).
        /// 서버가 내려주기 시작하면 그대로 채워진다.
        /// </summary>
        [JsonProperty("deptCd")]
        public string DeptCode { get; set; }
    }
}
