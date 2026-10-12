using Newtonsoft.Json;

namespace SEOJIN_TMES.Client.Common.Models
{
    /// <summary>
    /// 서버 /api/v1.0/auth/login 요청 바디.
    /// 서버가 nonce 방식이라 <b>my-salt로 먼저 받은 salt</b>와, 평문이 아닌 <b>SHA256 해시 비밀번호</b>를 함께 올린다.
    /// 필드명은 서버 스펙 그대로(corpCd/orgNo/password/salt) 나가야 하므로 JsonProperty로 고정한다.
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// 사업장 코드 — 로그인 화면의 <b>접속서버</b> 콤보에서 고른 값이 그대로 올라간다(예: "GUM").
        /// 같은 아이디라도 사업장에 따라 계정·권한이 다르므로 서버가 이 값으로 대상을 좁힌다.
        /// </summary>
        [JsonProperty("corpCd")]
        public string CorpCd { get; set; }

        /// <summary>로그인 아이디(서버 스펙상 orgNo).</summary>
        [JsonProperty("orgNo")]
        public string OrgNo { get; set; }

        /// <summary>SHA256으로 해시한 비밀번호(소문자 16진수).</summary>
        [JsonProperty("password")]
        public string Password { get; set; }

        /// <summary>my-salt API로 받은 salt를 그대로 되돌려 보낸다.</summary>
        [JsonProperty("salt")]
        public string Salt { get; set; }
    }

    /// <summary>
    /// 서버 /api/v1.0/auth/login 응답의 <b>알맹이</b>(Swagger 정의상 <c>TokenResponse</c>).
    ///
    /// ⚠️ 서버는 이것을 <c>{ "data": { ... } }</c> 로 <b>한 겹 싸서</b> 내려준다
    /// (Swagger 정의 <c>APIResponse«TokenResponse»</c>). 그래서 받을 때 반드시
    /// <see cref="ApiDataResponse{T}"/>로 감싸서 역직렬화해야 한다 —
    /// 이 타입으로 바로 받으면 <b>모든 필드가 조용히 null</b>이 되어,
    /// 로그인은 200으로 성공한 것처럼 보이는데 이후 모든 API가 401로 실패한다(실제로 겪은 증상).
    ///
    /// 성공/실패 판정은 HTTP 상태코드로 한다(2xx=성공, 401 등=실패).
    /// </summary>
    public class LoginResponse
    {
        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }

        [JsonProperty("refreshToken")]
        public string RefreshToken { get; set; }

        /// <summary>로그인한 사용자 요약(사업장/아이디/이름). 서버가 토큰과 함께 내려준다.</summary>
        [JsonProperty("user")]
        public LoginUser User { get; set; }
    }

    /// <summary>로그인 응답에 함께 오는 사용자 정보(Swagger 정의상 <c>UserResponse</c>).</summary>
    public class LoginUser
    {
        [JsonProperty("corpCd")]
        public string CorpCd { get; set; }

        [JsonProperty("orgNo")]
        public string OrgNo { get; set; }

        [JsonProperty("userNm")]
        public string UserName { get; set; }
    }

    public class RefreshTokenRequest
    {
        [JsonProperty("refreshToken")]
        public string RefreshToken { get; set; }
    }
}
