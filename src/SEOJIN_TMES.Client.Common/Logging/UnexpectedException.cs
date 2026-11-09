using System;
using SEOJIN_TMES.Client.Common.Errors;

namespace SEOJIN_TMES.Client.Common.Logging
{
    /// <summary>
    /// 예기치 못한 예외를 하나로 묶는 래퍼.
    ///
    /// 화면·통신 코드는 예상한 실패(ApiException, ModbusTimeoutException, DocumentShareException …)를 각자 처리한다.
    /// 그 밖의 예외는 원인이 무엇이든 이 타입으로 감싸서 <see cref="ServerLog.Unexpected"/>로 남긴다 —
    /// 로그에서 <c>UnexpectedException</c> 한 단어로 "처리하지 못한 오류"만 골라 볼 수 있게 하기 위해서다.
    /// 원래 예외는 <see cref="Exception.InnerException"/>에 그대로 있다.
    /// </summary>
    public class UnexpectedException : Exception
    {
        /// <summary>예외가 잡힌 자리(예: <c>로그인</c>, <c>UI 스레드</c>).</summary>
        public string Origin { get; }

        /// <summary>
        /// 오류 코드 — 화면이 잡은 예외는 <see cref="ErrorCodes.Unexpected"/>(SYS_001),
        /// 전역 처리기까지 올라온 예외는 <see cref="ErrorCodes.Unhandled"/>(SYS_002).
        /// </summary>
        public string Code { get; }

        /// <remarks>Message는 로그(로컬 파일 · 서버) 전용이라 한국어로 고정한다 — 화면에는 ErrorText.ForDisplay로 띄운다.</remarks>
        public UnexpectedException(string origin, Exception innerException, string code = ErrorCodes.Unexpected)
            : base($"[{origin}] 예기치 못한 오류 - {innerException?.Message}", innerException)
        {
            Origin = origin;
            Code = code;
        }

        /// <summary>이미 감싼 예외면 그대로 돌려준다(두 겹으로 싸지 않는다 — 처음 감쌀 때의 코드를 유지한다).</summary>
        public static UnexpectedException Wrap(string origin, Exception exception, string code = ErrorCodes.Unexpected) =>
            exception as UnexpectedException ?? new UnexpectedException(origin, exception, code);
    }
}
