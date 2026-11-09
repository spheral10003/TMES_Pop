using Newtonsoft.Json;

namespace SEOJIN_TMES.Client.Common.Models
{
    /// <summary>
    /// 서버로 보내는 클라이언트 이벤트 로그 1건 — <c>POST api/v1.0/pop/event-logs</c>의 요청 바디 그대로다.
    ///
    /// 종류(<see cref="EventName"/>)마다 채워지는 필드가 다르다 — 해당 없는 필드는 null로 나간다.
    /// 종류별로 어떤 필드가 채워지는지는 루트 EXCEPTION_LOGGING.md 참고.
    /// 메뉴 · 레지스터 값처럼 서버 필드가 따로 없는 정보는 <see cref="Message"/> 한 줄에 함께 적는다.
    ///
    /// 필드명은 서버 계약이다 — <c>[JsonProperty]</c> 이름을 바꾸면 서버가 값을 못 읽는다.
    /// 포트 · 시각도 서버 스펙이 문자열이라 문자열로 둔다(값을 만드는 쪽은 <c>ServerLog</c>).
    /// </summary>
    public class ClientLogDto
    {
        /// <summary>법인 코드. 로그인 전에 생긴 로그(시작 · 시작 직후 설비 접속)는 비어 있다.</summary>
        [JsonProperty("corpCd")]
        public string CorpCd { get; set; }

        /// <summary>이벤트명(<c>ClientLogType</c>의 이름 — AppStart / Login / MenuClick / EquipmentConnect / ModbusWrite / CriticalError).</summary>
        [JsonProperty("evntNm")]
        public string EventName { get; set; }

        /// <summary>장비 코드(설비 로그만).</summary>
        [JsonProperty("eqpCd")]
        public string EquipmentCode { get; set; }

        /// <summary>장비 IP(설비 로그만).</summary>
        [JsonProperty("eqpIp")]
        public string EquipmentIp { get; set; }

        /// <summary>장비 포트(설비 로그만). 서버 스펙이 문자열이다.</summary>
        [JsonProperty("eqpPort")]
        public string EquipmentPort { get; set; }

        /// <summary>사람이 읽는 한 줄 요약.</summary>
        [JsonProperty("msg")]
        public string Message { get; set; }

        /// <summary>오류 내용 — 예외면 예외 전체 문자열(스택 포함), 설비 접속 실패면 실패 사유.</summary>
        [JsonProperty("errMsg")]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 클라이언트에서 일이 일어난 시각(<c>yyyy-MM-dd HH:mm:ss</c>).
        /// 전송 시각이 아니다 — 시작 · 로그인 로그는 메인 화면이 뜰 때 묶어서 나간다.
        /// </summary>
        [JsonProperty("evntOcrnDtm")]
        public string OccurredAt { get; set; }

        /// <summary>워크스테이션 코드. 메인 화면에서 고르기 전에 생긴 로그는 비어 있다.</summary>
        [JsonProperty("wkstnCd")]
        public string WorkstationCode { get; set; }

        /// <summary>공장(부서) 코드 — 고른 워크스테이션의 deptCd.</summary>
        [JsonProperty("deptCd")]
        public string DeptCode { get; set; }

        /// <summary>라인 코드 — 고른 워크스테이션의 lineCd.</summary>
        [JsonProperty("lineCd")]
        public string LineCode { get; set; }

        /// <summary>작업자(로그인) 아이디.</summary>
        [JsonProperty("userId")]
        public string UserId { get; set; }

        /// <summary>오류 여부 <c>Y</c> / <c>N</c>.</summary>
        [JsonProperty("errYn")]
        public string ErrorYn { get; set; }

        /// <summary>오류 코드. 지금은 예외의 타입 이름(예: <c>NullReferenceException</c>)만 채운다 — 클라이언트 오류 코드 체계는 아직 없다.</summary>
        [JsonProperty("errCd")]
        public string ErrorCode { get; set; }
    }
}
