namespace SEOJIN_TMES.Client.Common.Logging
{
    /// <summary>
    /// 서버로 보내는 로그의 종류. <b>여기 없는 것은 서버로 보내지 않고 로컬 파일(AppLog)에만 남긴다.</b>
    ///
    /// 일부러 뺀 것: 설비 Alive 신호, 동작 결과(Event 채널로 올라오는 체결 결과와 그 수신 확인),
    /// 이벤트 핸드셰이크, 레지스터 읽기 — 양이 많고 업무 데이터는 별도 API로 간다.
    /// 예상된 실패(API 오류 응답, Modbus 시간 초과, 파일 잠금 등)도 로컬에만 남긴다.
    /// </summary>
    public enum ClientLogType
    {
        /// <summary>프로그램 시작.</summary>
        AppStart,

        /// <summary>로그인 성공(아이디).</summary>
        Login,

        /// <summary>메뉴 클릭(메뉴명 + 실행).</summary>
        MenuClick,

        /// <summary>설비 접속 성공/실패(IP · 포트).</summary>
        EquipmentConnect,

        /// <summary>설정 레지스터(토크 · 드라이버 잠금) 쓰기 이력 — 송신 1건 + 응답 1건.</summary>
        ModbusWrite,

        /// <summary>시스템 차원의 치명적 오류 · 예기치 못한 예외.</summary>
        CriticalError
    }
}
