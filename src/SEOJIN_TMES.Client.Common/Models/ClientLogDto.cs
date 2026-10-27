using System;

namespace SEOJIN_TMES.Client.Common.Models
{
    /// <summary>
    /// 서버로 보내는 클라이언트 이벤트 로그 1건.
    ///
    /// 종류(<see cref="LogType"/>)마다 채워지는 필드가 다르므로 한 형태로 평평하게 둔다 —
    /// 해당 없는 필드는 null로 나간다. 종류별로 어떤 필드가 채워지는지는 루트 EXCEPTION_LOGGING.md 참고.
    ///
    /// <b>서버 로그 API가 아직 없다.</b> 필드명·형식은 가정값이며 서버 스펙 확정 후 맞출 것(TODO).
    /// 이름을 맞출 때는 이 클래스에 <c>[JsonProperty]</c>로 고정한다(공통 CLAUDE.md의 "DTO는 서버 계약이다").
    /// </summary>
    public class ClientLogDto
    {
        /// <summary>로그 종류(<c>ClientLogType</c>의 이름 — AppStart / Login / MenuClick / EquipmentConnect / ModbusWrite / CriticalError).</summary>
        public string LogType { get; set; }

        /// <summary>클라이언트에서 일이 일어난 시각(전송 시각이 아니다 — 시작·로그인 로그는 메인 화면이 뜰 때 묶어서 나간다).</summary>
        public DateTime OccurredAt { get; set; }

        /// <summary>로그인 아이디. 로그인 전에 생긴 로그(시작·설비 접속)는 비어 있다.</summary>
        public string UserId { get; set; }

        /// <summary>로그인 때 고른 사업장 코드(corpCd).</summary>
        public string CorpCd { get; set; }

        /// <summary>로그인 때 고른 워크스테이션.</summary>
        public string Workstation { get; set; }

        /// <summary>현장 PC 이름 — 같은 사용자가 여러 PC에서 쓰는 경우를 구분한다.</summary>
        public string MachineName { get; set; }

        /// <summary>클라이언트 버전(ClickOnce 배포 버전).</summary>
        public string AppVersion { get; set; }

        /// <summary>사람이 읽는 한 줄 요약.</summary>
        public string Message { get; set; }

        // ── 메뉴 (MenuClick) ──

        public string MenuKey { get; set; }
        public string MenuName { get; set; }

        /// <summary>메뉴를 눌러 일어난 실행 — <c>Open</c>(새 탭) / <c>Focus</c>(이미 열린 탭으로 전환).</summary>
        public string MenuAction { get; set; }

        // ── 설비 (EquipmentConnect / ModbusWrite) ──

        public string EquipmentId { get; set; }
        public string Host { get; set; }
        public int? Port { get; set; }

        /// <summary>Request(레지스터 Read/Write) / Event(체결 결과 Push) 중 어느 채널인지.</summary>
        public string Channel { get; set; }

        /// <summary>접속 성공 여부 / 레지스터 쓰기 성공 여부(응답이 예외 응답이 아닌지).</summary>
        public bool? Success { get; set; }

        /// <summary>Modbus 요청 설명(예: <c>WriteSingleRegister(Torque=2)</c>).</summary>
        public string ModbusRequest { get; set; }

        /// <summary><c>Send</c>(장비로 보낸 값) / <c>Receive</c>(장비가 돌려준 응답).</summary>
        public string Direction { get; set; }

        /// <summary>보낸 값 또는 받은 값(여러 개면 쉼표로 잇는다).</summary>
        public string RegisterValues { get; set; }

        // ── 예외 (CriticalError) ──

        /// <summary>예외가 난 자리(예: <c>UI 스레드</c>, <c>로그인</c>).</summary>
        public string Origin { get; set; }

        public string ExceptionType { get; set; }

        /// <summary>예외 전체 문자열(스택 포함).</summary>
        public string ExceptionDetail { get; set; }
    }
}
