using System;
using System.Globalization;
using Newtonsoft.Json;

namespace SEOJIN_TMES.Client.Common.Models
{
    /// <summary>
    /// 공지사항 한 건 — <c>GET api/v1.0/gm/notices</c> 응답 목록(<c>data</c>)의 원소.
    /// 홈 화면이 공지 유형(<see cref="NtcType"/>)별 상자로 나눠 보여 준다.
    ///
    /// 필드명은 2026-11-11 화성 Test 서버 응답 로그로 확인했다
    /// (<c>sysSeqId · ntcType · subject · body · regDt · createdAt · readCount · useYn</c>).
    /// 일시는 문자열로 받는다 — 형식이 하나만 달라도 목록 전체의 역직렬화가 실패하지 않게.
    /// </summary>
    public class NoticeDto
    {
        /// <summary>공지 순번 — 서버 <c>sysSeqId</c>.</summary>
        [JsonProperty("sysSeqId")]
        public string SysSeqId { get; set; }

        /// <summary>공지 유형 코드 — 서버 <c>ntcType</c>. 값은 <see cref="NoticeType"/>의 상수.</summary>
        [JsonProperty("ntcType")]
        public string NtcType { get; set; }

        /// <summary>제목 — 서버 <c>subject</c>.</summary>
        [JsonProperty("subject")]
        public string Subject { get; set; }

        /// <summary>
        /// 본문 — 서버 <c>body</c>. HTML 조각(<c>&lt;div&gt;…</c>)으로 온다.
        /// 목록 조회는 <c>exceptBodyYn=Y</c>로 본문 없이 받으므로 비어 있고, 한 건 조회(<c>gm/notices/{sysSeqId}</c>)에서만 채워진다.
        /// </summary>
        [JsonProperty("body")]
        public string Body { get; set; }

        /// <summary>등록일자 — 서버 <c>regDt</c>(<c>yyyy-MM-dd</c>, 시각 없음).</summary>
        [JsonProperty("regDt")]
        public string RegDt { get; set; }

        /// <summary>작성일시 — 서버 <c>createdAt</c>(<c>yyyy-MM-dd HH:mm:ss</c>). 목록의 날짜 칸이 이 값이다.</summary>
        [JsonProperty("createdAt")]
        public string CreatedAt { get; set; }

        /// <summary>작성자 아이디 — 서버 <c>createdBy</c>.</summary>
        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }

        /// <summary>
        /// 새 글 여부 Y/N — 서버 <c>newYn</c>.
        /// <b>2026-11-11 기준 서버 응답에 이 필드가 아직 없다</b>(서버에 추가 요청 필요). 없으면 N 배지를 띄우지 않는다.
        /// </summary>
        [JsonProperty("newYn")]
        public string NewYn { get; set; }

        /// <summary>N 배지 표시 여부. Y/N 문자열을 화면에서 매번 비교하지 않으려는 표시용 파생 값이다.</summary>
        [JsonIgnore]
        public bool IsNew => string.Equals(NewYn, "Y", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// 작성일시를 일시 값으로 본 것 — "최근 공지 몇 건"을 고를 때 정렬 기준으로만 쓴다.
        /// 해석할 수 없으면 null(가장 오래된 것으로 밀린다).
        /// </summary>
        [JsonIgnore]
        public DateTime? CreatedTime =>
            DateTime.TryParse(CreatedAt ?? RegDt, CultureInfo.InvariantCulture, DateTimeStyles.None, out var value) ? value : (DateTime?)null;

        /// <summary>
        /// 화면 표시용 일시 — <c>yyyy-MM-dd HH:mm:ss</c>로 맞춘다(상자마다 날짜 폭이 들쭉날쭉하지 않게).
        /// 해석할 수 없는 형식이면 받은 문자열을 그대로 보여 준다.
        /// </summary>
        [JsonIgnore]
        public string CreatedAtText =>
            CreatedTime?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) ?? CreatedAt ?? RegDt ?? string.Empty;
    }

    /// <summary>
    /// 공지 유형 코드(<c>ntcType</c>).
    /// 서버는 NTC05~07(생산 · 품질 · 안전)도 내려주지만 홈 화면은 NTC01~04 네 가지만 보여 준다.
    /// </summary>
    public static class NoticeType
    {
        public const string System = "NTC01";
        public const string General = "NTC02";
        public const string Important = "NTC03";
        public const string Urgent = "NTC04";
        public const string Production = "NTC05";
        public const string Quality = "NTC06";
        public const string Safety = "NTC07";
    }
}
