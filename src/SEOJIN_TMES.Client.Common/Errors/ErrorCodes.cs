namespace SEOJIN_TMES.Client.Common.Errors
{
    /// <summary>
    /// 클라이언트 오류 코드. 형식은 서버 업무 오류 코드(<c>WORK_005</c>)와 같은 <c>분류_번호</c>다.
    ///
    /// 코드마다 문구가 Languages의 <c>Error_{코드}</c> 키로 있다(ko/en/vi). 화면은 현재 언어로,
    /// 서버 로그(<c>errCd</c> · <c>errMsg</c>)는 한국어로 같은 키를 꺼낸다 — <see cref="ErrorText"/> 참고.
    ///
    /// 코드를 추가할 때: 여기 상수 한 줄 + 세 resx에 <c>Error_{코드}</c> 키 + Language.cs 속성 + 루트 EXCEPTION_LOGGING.md 표.
    /// 이미 나간 코드의 의미는 바꾸지 말 것 — 서버에 쌓인 로그와 현장 문의가 코드로 연결된다.
    /// </summary>
    public static class ErrorCodes
    {
        // ── API: 서버 통신 ──

        /// <summary>서버에 연결할 수 없음(서버 미기동 · 네트워크 단절 · DNS · SSL).</summary>
        public const string ApiConnectFailed = "API_001";

        /// <summary>서버 응답 시간 초과.</summary>
        public const string ApiTimeout = "API_002";

        /// <summary>그 밖의 통신 단계 오류(원인을 분류하지 못한 경우).</summary>
        public const string ApiCommunication = "API_003";

        /// <summary>성공 응답을 받았지만 본문을 해석하지 못함(HTML 오류 페이지 · 필드 형식 불일치).</summary>
        public const string ApiInvalidResponse = "API_004";

        /// <summary>서버가 실패 상태코드로 응답했는데 업무 오류 코드를 주지 않음. 문구 인자: HTTP 상태코드.</summary>
        public const string ApiErrorResponse = "API_005";

        /// <summary>로그인 · 토큰 갱신 응답에 토큰이 없음.</summary>
        public const string ApiNoToken = "API_006";

        // ── EQP: 설비(너트러너) ──

        /// <summary>설비 접속 실패.</summary>
        public const string EquipmentConnectFailed = "EQP_001";

        /// <summary>설정 레지스터 쓰기에 설비가 오류(예외) 응답.</summary>
        public const string EquipmentWriteRejected = "EQP_002";

        /// <summary>설비 재연결을 시작하지 못함.</summary>
        public const string EquipmentReconnectFailed = "EQP_003";

        // ── FILE: 파일 ──

        /// <summary>도면 공유 폴더(SMB) 파일 읽기 실패(입출력 오류 — 아래로 분류되지 않은 경우).</summary>
        public const string FileShareFailed = "FILE_001";

        /// <summary>로컬 파일 읽기 · 쓰기 실패(첨부 업로드 · 엑셀 저장 등).</summary>
        public const string FileAccessFailed = "FILE_002";

        /// <summary>표시할 도면 파일 경로가 비어 있음.</summary>
        public const string DrawingPathEmpty = "FILE_003";

        /// <summary>도면 공유 폴더(App.config DocumentShare.RootPath) 미설정.</summary>
        public const string DrawingShareNotConfigured = "FILE_004";

        /// <summary>도면 파일 없음.</summary>
        public const string DrawingNotFound = "FILE_005";

        /// <summary>도면 파일 크기 상한 초과. 문구 인자: 파일 크기(MB), 상한(MB).</summary>
        public const string DrawingTooLarge = "FILE_006";

        /// <summary>도면 파일 접근 권한 없음.</summary>
        public const string DrawingAccessDenied = "FILE_007";

        /// <summary>공유 폴더 연결 — 접근 거부(Windows 오류 5).</summary>
        public const string ShareAccessDenied = "FILE_008";

        /// <summary>공유 폴더 연결 — 공유를 찾을 수 없음(Windows 오류 53).</summary>
        public const string ShareNotFound = "FILE_009";

        /// <summary>공유 폴더 연결 — 공유 이름 오류(Windows 오류 67).</summary>
        public const string ShareBadName = "FILE_010";

        /// <summary>공유 폴더 연결 — 계정 또는 비밀번호 오류(Windows 오류 86 · 1326).</summary>
        public const string ShareBadCredential = "FILE_011";

        /// <summary>공유 폴더 연결 — 같은 서버에 다른 계정으로 이미 연결됨(Windows 오류 1219).</summary>
        public const string ShareCredentialConflict = "FILE_012";

        /// <summary>공유 폴더 연결 — 그 밖의 실패. 문구 인자: Windows 오류 번호.</summary>
        public const string ShareConnectFailed = "FILE_013";

        // ── SYS: 예기치 못한 오류 ──

        /// <summary>화면이 잡았지만 원인을 예상하지 못한 예외(<c>ServerLog.Unexpected</c>).</summary>
        public const string Unexpected = "SYS_001";

        /// <summary>어디서도 잡지 못한 예외(전역 처리기 · <c>ServerLog.Critical</c>).</summary>
        public const string Unhandled = "SYS_002";
    }
}
