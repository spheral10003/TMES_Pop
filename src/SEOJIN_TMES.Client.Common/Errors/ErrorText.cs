using System;
using System.Globalization;
using System.IO;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Common.Storage;

namespace SEOJIN_TMES.Client.Common.Errors
{
    /// <summary>
    /// 오류 코드 → 문구. 같은 코드를 두 가지로 꺼낸다:
    ///  - <see cref="Display"/> : 현재 화면 언어(ko/en/vi) — 사용자에게 보여 줄 때
    ///  - <see cref="Korean"/>  : 항상 한국어 — 서버로 보내는 로그(<c>errMsg</c>)에 쓸 때
    ///
    /// 화면에는 문구 아래에 "오류 코드: XXX_000" 줄을 붙인다(<see cref="WithCode"/>) —
    /// 현장에서 언어가 달라도 코드 하나로 같은 오류인지 알 수 있게 하기 위해서다.
    /// </summary>
    public static class ErrorText
    {
        /// <summary>서버로 보내는 문구의 언어. 서버 로그는 다국어를 쓰지 않는다.</summary>
        public static readonly CultureInfo ServerCulture = new CultureInfo("ko-KR");

        /// <summary>코드 문구(현재 화면 언어). 문구가 없는 코드(서버 업무 코드 등)는 코드 그대로 돌려준다.</summary>
        public static string Display(string code, params object[] args) => Format(code, CultureInfo.CurrentUICulture, args);

        /// <summary>코드 문구(한국어) — 서버 전송용.</summary>
        public static string Korean(string code, params object[] args) => Format(code, ServerCulture, args);

        /// <summary>지정한 언어의 코드 문구.</summary>
        public static string Format(string code, CultureInfo culture, params object[] args)
        {
            if (string.IsNullOrEmpty(code))
                return string.Empty;

            var template = Language.GetString("Error_" + code, culture);
            if (string.IsNullOrEmpty(template))
                return code;

            return args == null || args.Length == 0 ? template : string.Format(culture, template, args);
        }

        /// <summary>화면용 — 안내 문구 아래에 빈 줄을 두고 "오류 코드: XXX" 줄을 붙인다.</summary>
        public static string WithCode(string message, string code)
        {
            if (string.IsNullOrEmpty(code))
                return message;

            return $"{message}{Environment.NewLine}{Environment.NewLine}{string.Format(Language.Error_CodeFormat, code)}";
        }

        /// <summary>
        /// 예외의 오류 코드. 코드를 들고 있는 예외는 그 값을, 아니면 예외 종류로 정한다.
        /// 분류할 수 없으면 <paramref name="fallbackCode"/>(기본 <see cref="ErrorCodes.Unexpected"/>).
        /// </summary>
        public static string CodeOf(Exception exception, string fallbackCode = ErrorCodes.Unexpected)
        {
            switch (exception)
            {
                case ICodedException coded:
                    return coded.Code;
                case UnexpectedException unexpected:
                    // 감싼 원래 예외가 코드를 들고 있으면(ApiException) 그 코드가 더 구체적이다.
                    return unexpected.InnerException is ICodedException inner ? inner.Code : unexpected.Code;
                case DocumentShareException _:
                    return ErrorCodes.FileShareFailed;
                case IOException _:
                case UnauthorizedAccessException _:
                    return ErrorCodes.FileAccessFailed;
                default:
                    return fallbackCode;
            }
        }

        /// <summary>
        /// 예외를 화면에 띄울 문구(현재 화면 언어 + 오류 코드 줄).
        ///  - 코드를 들고 있는 예외(ApiException)는 Message가 이미 그 형태라 그대로 쓴다.
        ///  - 그 밖에는 코드 문구 + 원인(예외 메시지) + 코드 줄.
        /// </summary>
        public static string ForDisplay(Exception exception, string fallbackCode = ErrorCodes.Unexpected)
        {
            if (exception == null)
                return WithCode(Display(fallbackCode), fallbackCode);

            if (exception is ICodedException)
                return exception.Message;

            var code = CodeOf(exception, fallbackCode);

            // UnexpectedException은 로그용 래퍼다 — 원인으로는 감싼 원래 예외의 메시지를 보여 준다.
            var cause = exception is UnexpectedException && exception.InnerException != null
                ? exception.InnerException
                : exception;

            if (cause is ICodedException)
                return cause.Message;

            var message = Display(code);
            if (!string.IsNullOrWhiteSpace(cause.Message))
                message += Environment.NewLine + Environment.NewLine + string.Format(Language.Error_CauseFormat, cause.Message);

            return WithCode(message, code);
        }
    }
}
