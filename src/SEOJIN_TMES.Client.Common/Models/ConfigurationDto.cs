using Newtonsoft.Json;

namespace SEOJIN_TMES.Client.Common.Models
{
    /// <summary>
    /// 시스템 설정 — <c>GET api/v1.0/sm/configurations</c> 응답의 <c>data</c>(객체 하나).
    /// 클라이언트는 로그인 직후 초기 비밀번호 안내에 <see cref="InitPassword"/>만 쓴다.
    ///
    /// 필드명은 2026-11-11 화성 Test 서버 응답 로그로 확인했다. 그 밖에
    /// <c>pswdOptionUseYn · pswdMinLen · pswdMaxLen · pswdChgCycle · ltrEsntlYn · numEsntlYn · spclLtrEsntlYn ·
    /// pswdFailLmtNmtm · bfrPswdRuseYn · userLogKpngPrd · menuWholIndctYn · useYn</c>이 오지만 쓰지 않아 받지 않는다.
    /// </summary>
    public class ConfigurationDto
    {
        /// <summary>초기 비밀번호 — 서버 <c>initPswd</c>. <b>평문</b>으로 온다(해시 아님).</summary>
        [JsonProperty("initPswd")]
        public string InitPassword { get; set; }
    }
}
