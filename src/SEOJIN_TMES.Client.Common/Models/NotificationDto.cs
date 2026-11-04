using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SEOJIN_TMES.Client.Common.Models
{
    /// <summary>
    /// 알림 목록/상세 한 건.
    /// 서버 조회 결과를 그대로 표시하며, 클라이언트는 판정/계산을 하지 않는다.
    ///
    /// 속성명은 서버 필드명을 그대로 PascalCase로 옮기고 <see cref="JsonPropertyAttribute"/>로 고정한다 —
    /// 그리드 컬럼(FieldName)이 이 속성명을 쓰므로, 이름을 "정리"하면 컬럼이 조용히 사라진다.
    ///
    /// 식별자·일시는 문자열로 받는다. 서버 형식(숫자/문자, 일시 서식)이 확정되지 않았는데
    /// long·DateTime으로 받으면 한 건만 형식이 달라도 목록 전체의 역직렬화가 실패한다.
    /// 화면은 서버가 준 값을 그대로 보여 준다.
    /// </summary>
    public class NotificationDto
    {
        /// <summary>사업부 — 서버 <c>corpCd</c>.</summary>
        [JsonProperty("corpCd")]
        public string CorpCd { get; set; }

        /// <summary>알람 순번 — 서버 <c>alarmSeqId</c>.</summary>
        [JsonProperty("alarmSeqId")]
        public string AlarmSeqId { get; set; }

        /// <summary>
        /// 발송 참조키(alarmSendRef) — 조회(<c>GetNotificationsItem</c>) 응답의 <c>sysObjId</c> 값을 담는다.
        /// 읽음 처리(PATCH)가 순번이 아니라 이 값의 배열을 받기 때문에 따로 받아 둔다.
        /// </summary>
        [JsonProperty("sysObjId")]
        public string AlarmSendRef { get; set; }

        /// <summary>알람 그룹 — 서버 <c>alarmGroupNo</c>.</summary>
        [JsonProperty("alarmGroupNo")]
        public string AlarmGroupNo { get; set; }

        /// <summary>알람 코드 — 서버 <c>alarmFormCd</c>.</summary>
        [JsonProperty("alarmFormCd")]
        public string AlarmFormCd { get; set; }

        /// <summary>제목 — 서버 <c>subject</c>.</summary>
        [JsonProperty("subject")]
        public string Subject { get; set; }

        /// <summary>본문 — 서버 <c>body</c>. 이메일용 완전한 HTML 문서로 온다(우측 상세에 렌더링).</summary>
        [JsonProperty("body")]
        public string Body { get; set; }

        /// <summary>작성자(발신자) 아이디 — 서버 <c>createdBy</c>.</summary>
        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }

        /// <summary>작성일자 — 서버 <c>createdAt</c>.</summary>
        [JsonProperty("createdAt")]
        public string CreatedAt { get; set; }

        /// <summary>읽음 여부 — 서버 <c>idntyYn</c>(Y/N).</summary>
        [JsonProperty("idntyYn")]
        public string IdntyYn { get; set; }

        /// <summary>확인일자(읽은 시각) — 서버 <c>idntyTime</c>.</summary>
        [JsonProperty("idntyTime")]
        public string IdntyTime { get; set; }

        /// <summary>
        /// 읽음 여부를 bool로 본 값. 서버의 Y/N 표기를 화면에서 매번 문자열 비교하지 않으려는 표시용 파생 값이다.
        /// </summary>
        [JsonIgnore]
        public bool IsRead => string.Equals(IdntyYn, "Y", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>알림 조회의 읽음여부 조건.</summary>
    public enum NotificationReadState
    {
        /// <summary>전체(조건을 서버로 보내지 않는다).</summary>
        All,

        /// <summary>읽은 알림만.</summary>
        Read,

        /// <summary>안 읽은 알림만.</summary>
        Unread
    }

    /// <summary>
    /// <c>GET api/v1.0/gm/alarm-consumer/my-alarms</c> 응답
    /// (Swagger 정의상 <c>WithUnreadCountAndAlarmSendRefListResponse</c>).
    ///
    /// 상단 종 배지를 채우는 폴링이 이 값을 쓴다. 개수와 함께 <b>안 읽은 알림의 참조키 목록</b>이 오므로,
    /// 읽음 처리(<c>PATCH</c> 같은 경로)에 그대로 실어 보낼 수 있다.
    /// </summary>
    public class MyAlarmSummaryDto
    {
        /// <summary>안 읽은 알림 개수. 서버가 <c>int64</c>로 준다.</summary>
        [JsonProperty("unreadCount")]
        public long UnreadCount { get; set; }

        /// <summary>안 읽은 알림의 발송 참조키 목록. 읽음 처리 요청 바디가 이 목록이다.</summary>
        [JsonProperty("alarmSendRefList")]
        public List<string> AlarmSendRefList { get; set; } = new List<string>();
    }
}
