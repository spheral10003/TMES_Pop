using System;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace SEOJIN_TMES.Client.Common.Models
{
    /// <summary>
    /// 공지사항 한 건 — <c>GET api/v1.0/gm/notices</c> 응답 목록의 원소.
    /// 홈 화면이 공지 유형(<see cref="NtcType"/>)별 상자로 나눠 보여 준다. 정렬 · 새 글 여부는 서버가 준 그대로 쓴다.
    ///
    /// <b>필드명은 가정값이다(TODO: 서버 스펙 확인).</b> 작업 시점에 Swagger가 인증 뒤에 있어 응답을 직접 보지 못했다.
    /// 그래서 <see cref="From"/>에서 후보 키를 차례로 찾아 읽는다 — 한 이름으로 고정해 두면 이름이 하나만 달라도
    /// 제목이 전부 빈칸이 되는데, 예외가 나지 않아 발견이 늦다.
    /// 실제 필드명은 App.config의 <c>LogApiResponseBody=true</c>로 응답 로그를 보고 확인해 후보를 정리할 것.
    /// </summary>
    public class NoticeDto
    {
        /// <summary>공지 유형 코드 — 서버 <c>ntcType</c>. 값은 <see cref="NoticeType"/>의 상수.</summary>
        public string NtcType { get; set; }

        /// <summary>공지 식별자(가정: <c>ntcSeq</c> · <c>ntcNo</c> · <c>ntcId</c>).</summary>
        public string NtcId { get; set; }

        /// <summary>제목(가정: <c>ntcTtl</c> · <c>ntcTitle</c> · <c>title</c> · <c>subject</c>).</summary>
        public string Title { get; set; }

        /// <summary>본문(가정: <c>ntcCn</c> · <c>ntcCont</c> · <c>content</c>).</summary>
        public string Content { get; set; }

        /// <summary>등록일시 — 서버가 준 문자열 그대로(가정: <c>regDt</c> · <c>createdAt</c> · <c>ntcDt</c>).</summary>
        public string RegisteredAt { get; set; }

        /// <summary>새 글 여부 Y/N — 서버 판정값(가정: <c>newYn</c>). 없으면 N 배지를 띄우지 않는다.</summary>
        public string NewYn { get; set; }

        /// <summary>N 배지 표시 여부. Y/N 문자열을 화면에서 매번 비교하지 않으려는 표시용 파생 값이다.</summary>
        public bool IsNew => string.Equals(NewYn, "Y", StringComparison.OrdinalIgnoreCase)
                             || string.Equals(NewYn, "true", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// 화면 표시용 등록일시. ISO 형식(<c>2026-11-05T19:10:04.123</c>)으로 와도 <c>yyyy-MM-dd HH:mm:ss</c>로 맞춘다
        /// — 상자마다 날짜 폭이 들쭉날쭉하지 않게. 해석할 수 없는 형식이면 받은 문자열을 그대로 보여 준다.
        /// </summary>
        public string RegisteredAtText
        {
            get
            {
                if (string.IsNullOrWhiteSpace(RegisteredAt))
                    return string.Empty;

                return DateTime.TryParse(RegisteredAt, CultureInfo.InvariantCulture, DateTimeStyles.None, out var value)
                    ? value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                    : RegisteredAt;
            }
        }

        /// <summary>응답 객체 하나를 읽는다. 각 항목은 후보 키 중 <b>값이 있는 첫 번째</b>를 쓴다.</summary>
        public static NoticeDto From(JObject item)
        {
            return new NoticeDto
            {
                NtcType = Text(item, "ntcType"),
                NtcId = Text(item, "ntcSeq", "ntcNo", "ntcId", "sysObjId"),
                Title = Text(item, "ntcTtl", "ntcTitle", "ntcSj", "ntcNm", "title", "subject"),
                Content = Text(item, "ntcCn", "ntcCont", "ntcContent", "content", "body"),
                RegisteredAt = Text(item, "regDt", "createdAt", "ntcDt", "regDate", "createdDate", "postStartDt"),
                NewYn = Text(item, "newYn", "isNew")
            };
        }

        private static string Text(JObject item, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (item.TryGetValue(key, StringComparison.OrdinalIgnoreCase, out var token)
                    && token.Type != JTokenType.Null
                    && !string.IsNullOrWhiteSpace(token.ToString()))
                    return token.ToString();
            }
            return null;
        }
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
