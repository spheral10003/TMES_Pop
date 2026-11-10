using System;
using System.Globalization;
using System.IO;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Common.Logging;

namespace SEOJIN_TMES.Client.Common.Errors
{
    /// <summary>
    /// 오류 코드 → 문구. 같은 코드를 두 가지로 꺼낸다:
    ///  - <see cref="Display"/> : 현재 화면 언어(ko/en/vi) — 사용자에게 보여 줄 때
    ///  - <see cref="Korean"/>  : 항상 한국어 — 서버로 보내는 로그(<c>errMsg</c>)에 쓸 때
    ///
    /// 화면에는 문구 아래에 "오류 코드: XXX_000 - 설명" 줄을 붙인다(<see cref="WithCode"/>) —
    /// 현장에서 언어가 달라도 코드 하나로 같은 오류인지 알 수 있게 하고, 코드만 보고는 뜻을 모르는 일이 없게 하기 위해서다.
    /// 로그(로컬 파일 · 서버)는 "[XXX_000] 설명"(한국어, <see cref="Summary"/>)으로 적는다.
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

        /// <summary>
        /// 코드의 설명(지정한 언어). 문구가 없는 코드(서버 업무 코드 등)는 null — 설명 없이 코드만 찍힌다.
        /// </summary>
        public static string Description(string code, CultureInfo culture, params object[] args)
        {
            if (string.IsNullOrEmpty(code))
                return null;

            var text = Format(code, culture, args);
            return text == code ? null : text;
        }

        /// <summary>
        /// 화면용 — 안내 문구 아래에 빈 줄을 두고 "오류 코드: XXX - 설명" 줄을 붙인다.
        /// <paramref name="description"/>을 주지 않으면 코드 문구(화면 언어)를 설명으로 쓴다.
        /// </summary>
        public static string WithCode(string message, string code, string description = null)
        {
            if (string.IsNullOrEmpty(code))
                return message;

            return $"{message}{Environment.NewLine}{Environment.NewLine}{CodeLine(code, description, message)}";
        }

        /// <summary>
        /// "오류 코드: XXX - 설명" 한 줄(현재 화면 언어). 한 줄짜리 라벨처럼 <see cref="WithCode"/>를 쓸 수 없는 곳에서 쓴다.
        /// 설명이 이미 <paramref name="shownMessage"/>(코드 줄 위에 보여 준 문구)에 들어 있으면 두 번 적지 않는다 —
        /// 안내 문구가 곧 코드 문구인 경우(통신 실패 등) 같은 문장이 연달아 찍히면 읽기만 번거롭다.
        /// </summary>
        public static string CodeLine(string code, string description = null, string shownMessage = null)
        {
            var line = string.Format(Language.Error_CodeFormat, code);
            var text = OneLine(description ?? Description(code, CultureInfo.CurrentUICulture));

            if (string.IsNullOrEmpty(text) || (!string.IsNullOrEmpty(shownMessage) && shownMessage.Contains(text)))
                return line;

            return $"{line} - {text}";
        }

        /// <summary>
        /// 로그용 "[XXX] 설명"(한국어). 로컬 파일 · 서버 로그(<c>msg</c> · <c>errMsg</c>) 모두 이 형식으로 코드와 설명을 함께 적는다.
        /// 설명을 주지 않으면 코드 문구(한국어), 문구가 없는 코드면 "[XXX]"만.
        /// </summary>
        public static string Summary(string code, string description = null)
        {
            if (string.IsNullOrEmpty(code))
                return OneLine(description) ?? string.Empty;

            var text = OneLine(description ?? Description(code, ServerCulture));
            return string.IsNullOrEmpty(text) ? $"[{code}]" : $"[{code}] {text}";
        }

        /// <summary>
        /// 예외 코드의 설명(지정한 언어). 코드를 들고 있는 예외는 그 예외가 정한 설명(서버 업무 코드면 서버 사유)을,
        /// 아니면 코드 문구를 쓴다.
        /// </summary>
        public static string DescriptionOf(Exception exception, CultureInfo culture, string fallbackCode = ErrorCodes.Unexpected)
        {
            // UnexpectedException은 로그용 래퍼다 — 감싼 예외가 코드를 들고 있으면(CodeOf와 같은 규칙) 그 설명을 쓴다.
            var coded = exception as ICodedException
                ?? (exception is UnexpectedException ? exception.InnerException as ICodedException : null);
            if (coded != null)
                return coded.DescriptionFor(culture);

            return Description(CodeOf(exception, fallbackCode), culture);
        }

        /// <summary>여러 줄 문구를 로그 · 코드 줄에 넣을 수 있게 한 줄로 편다.</summary>
        private static string OneLine(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            return text.Replace("\r\n", " ").Replace('\n', ' ').Replace('\r', ' ').Trim();
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
