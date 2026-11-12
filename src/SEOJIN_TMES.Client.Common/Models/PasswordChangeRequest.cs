using Newtonsoft.Json;

namespace SEOJIN_TMES.Client.Common.Models
{
    /// <summary>
    /// 비밀번호 변경 요청 바디 — <c>PATCH api/v1.0/gm/users/{orgNo}/pswd-chg</c>.
    /// 평문은 넣지 않는다. 값은 <see cref="Security.PasswordHash.ForChange"/>로 만든 SHA256(비밀번호)다.
    /// </summary>
    public class PasswordChangeRequest
    {
        /// <summary>새 비밀번호의 SHA256(소문자 16진수 64자) — 서버 <c>pswd</c>.</summary>
        [JsonProperty("pswd")]
        public string PasswordHash { get; set; }
    }
}
