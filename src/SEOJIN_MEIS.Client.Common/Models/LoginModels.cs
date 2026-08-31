using Newtonsoft.Json;

namespace SEOJIN_MEIS.Client.Common.Models
{
    /// <summary>
    /// 서버 /api/v1.0/auth/login 요청 바디.
    /// 서버가 nonce 방식이라 <b>my-salt로 먼저 받은 salt</b>와, 평문이 아닌 <b>SHA256 해시 비밀번호</b>를 함께 올린다.
    /// 필드명은 서버 스펙 그대로(orgNo/password/salt) 나가야 하므로 JsonProperty로 고정한다.
    /// </summary>
    public class LoginRequest
    {
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
    /// 서버 /api/auth/login 응답. 성공/실패 판정은 HTTP 상태코드로 하고(2xx=성공, 401 등=실패),
    /// 본문에는 토큰만 내려온다. 만료 시각·사용자 정보 등은 응답 바디에 포함하지 않는다.
    /// </summary>
    public class LoginResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; }
    }
}
