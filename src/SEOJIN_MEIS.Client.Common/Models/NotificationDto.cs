using System;
using Newtonsoft.Json;

namespace SEOJIN_MEIS.Client.Common.Models
{
    /// <summary>
    /// 알림 목록/상세 한 건.
    /// 서버 조회 결과를 그대로 표시하며, 클라이언트는 판정/계산을 하지 않는다.
    /// TODO: 실제 서버 응답 필드명이 확정되면 속성명을 맞추거나 JsonProperty로 매핑할 것.
    /// </summary>
    public class NotificationDto
    {
        /// <summary>알림 식별자(읽음 처리에 쓴다).</summary>
        public long NotificationId { get; set; }

        /// <summary>법인코드.</summary>
        public string CorpCode { get; set; }

        /// <summary>알림양식코드.</summary>
        public string FormCode { get; set; }

        /// <summary>제목.</summary>
        public string Subject { get; set; }

        /// <summary>내용(우측 상세 영역에 표시).</summary>
        public string Content { get; set; }

        /// <summary>발신자아이디.</summary>
        public string SenderId { get; set; }

        /// <summary>수신 사용자명(조회 조건과 같은 값).</summary>
        public string UserName { get; set; }

        /// <summary>발신일시.</summary>
        public DateTime? SentAt { get; set; }

        /// <summary>읽음 여부.</summary>
        public bool IsRead { get; set; }

        /// <summary>그리드에 보여줄 읽음 상태 문구. 서버 값이 아니라 표시용 파생 값이다.</summary>
        [JsonIgnore]
        public string ReadStateText => IsRead ? "읽음" : "안읽음";
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

    /// <summary>알림 읽음 처리 요청 바디.</summary>
    public class NotificationReadRequest
    {
        [JsonProperty("notificationIds")]
        public long[] NotificationIds { get; set; }
    }
}
