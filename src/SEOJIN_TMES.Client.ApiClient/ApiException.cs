using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using SEOJIN_TMES.Client.Common.Errors;
using SEOJIN_TMES.Client.Common.Languages;

namespace SEOJIN_TMES.Client.ApiClient
{
    /// <summary>
    /// 서버 REST API 호출 실패 시 던지는 예외. 서버가 반환한 상태코드/본문을 그대로 담아 상위(Form)에서 메시지 처리.
    ///
    /// 응답을 아예 못 받은 실패(서버 미기동/네트워크 단절/타임아웃 등)도 화면이 같은 방식으로 다룰 수 있도록
    /// 이 예외로 감싸서 올린다(<see cref="FromTransportFailure"/>) — 그래야 각 화면의 catch(ApiException)에서
    /// 안내 창만 띄우고 작업을 계속 이어갈 수 있다. 이 경우 <see cref="StatusCode"/>는 0이다.
    ///
    /// 안내 문구는 오류 코드(<see cref="ErrorCodes"/>)로 정하고, <see cref="Message"/>를 <b>읽을 때</b> 그 시점의 화면 언어로 만든다
    /// (예외는 백그라운드 스레드에서 생기므로 만들 때 문구를 굳히면 언어가 어긋날 수 있다).
    /// Message 끝에는 "오류 코드: XXX - 설명" 줄이 붙어 있어 화면은 <c>ex.Message</c>를 그대로 띄우면 된다.
    /// 서버가 오류 본문에 사유를 담아 주면 그 사유가 안내의 첫 줄이다 — 작업자에게는 "왜 거절됐는지"가 가장 쓸모 있다.
    /// 서버 로그 · 로컬 로그용 한국어 문구는 <see cref="MessageFor"/> · <see cref="ToLogText"/>로 꺼낸다.
    /// </summary>
    public class ApiException : Exception, ICodedException
    {
        public HttpStatusCode StatusCode { get; }
        public string ResponseBody { get; }

        /// <summary>
        /// 서버가 오류 본문에 담아 준 업무 오류 코드(예: WORK_005). 본문이 없거나 JSON이 아니면 null.
        /// 화면이 "어떤 거절인지"에 따라 다르게 처리해야 할 때 이 값으로 구분한다.
        /// </summary>
        public string ErrorCode { get; }

        /// <summary>
        /// 화면 · 로그에 쓰는 오류 코드 — 서버 업무 코드가 있으면 그것, 없으면 클라이언트 코드(API_xxx).
        /// </summary>
        public string Code => string.IsNullOrEmpty(ErrorCode) ? _clientCode : ErrorCode;

        /// <summary>안내 문구를 정하는 클라이언트 코드(API_xxx).</summary>
        private readonly string _clientCode;

        /// <summary>코드 문구에 넣을 인자(API_005의 HTTP 상태코드).</summary>
        private readonly object[] _codeArgs;

        /// <summary>
        /// 서버가 오류 본문에 담아 준 사유(<c>message</c> 등). 서버가 준 문장이라 번역하지 않고 그대로 보여 준다.
        /// 본문에 사유가 없으면 null(상태 문구 "Bad Request"는 여기 넣지 않는다 — 안내로는 쓸모가 없어 로그에만 남긴다).
        /// </summary>
        public string ServerMessage { get; }

        /// <summary>HTTP 상태 문구(Bad Request 등). 로그용.</summary>
        private readonly string _reasonPhrase;

        /// <summary>호출 대상("POST https://..."). 개발자용 단서.</summary>
        private readonly string _request;

        /// <summary>실제 원인(가장 안쪽 예외의 메시지). 개발자용 단서.</summary>
        private readonly string _cause;

        /// <summary>코드 대신 호출자가 직접 준 문구(공개 생성자로 만든 경우).</summary>
        private readonly string _rawMessage;

        public ApiException(HttpStatusCode statusCode, string responseBody, string message, Exception innerException = null)
            : base(message, innerException)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
            _clientCode = ErrorCodes.ApiErrorResponse;
            _codeArgs = new object[] { (int)statusCode };
            _rawMessage = message;
        }

        private ApiException(HttpStatusCode statusCode, string responseBody, string clientCode, object[] codeArgs,
            string errorCode, string serverMessage, string reasonPhrase, string request, string cause, Exception innerException)
            : base(ErrorText.Korean(clientCode, codeArgs), innerException)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
            ErrorCode = errorCode;
            ServerMessage = serverMessage;
            _clientCode = clientCode;
            _codeArgs = codeArgs;
            _reasonPhrase = reasonPhrase;
            _request = request;
            _cause = cause;
        }

        /// <summary>서버 응답 자체를 받지 못한 실패(연결 실패/타임아웃 등)인지 여부.</summary>
        public bool IsTransportFailure => (int)StatusCode == 0;

        /// <summary>현재 화면 언어의 안내 문구 + "오류 코드: XXX - 설명" 줄.</summary>
        public override string Message => WithLead(null);

        /// <summary>
        /// 화면이 자기 안내 문구(예: "업로드 실패")를 맨 앞에 두고 싶을 때 — 그 아래에 서버 사유 · 상세 · 코드 줄을 잇는다.
        /// 화면 문구만 띄우면 서버가 내려준 사유가 사라지므로, 자체 문구를 쓰는 화면도 이 메서드를 거친다.
        /// </summary>
        public string WithLead(string lead)
        {
            var culture = CultureInfo.CurrentUICulture;
            var body = MessageFor(culture);
            if (!string.IsNullOrWhiteSpace(lead))
                body = lead + Environment.NewLine + body;

            return ErrorText.WithCode(body, Code, DescriptionFor(culture));
        }

        /// <summary>
        /// 코드 설명 — 서버 업무 코드(WORK_005 등)는 클라이언트에 문구가 없으므로 서버 사유가 곧 설명이다.
        /// 클라이언트 코드(API_xxx)는 코드 문구(HTTP 상태코드 포함).
        /// </summary>
        public string DescriptionFor(CultureInfo culture)
        {
            if (!string.IsNullOrEmpty(ErrorCode) && !string.IsNullOrWhiteSpace(ServerMessage))
                return ServerMessage;

            return _rawMessage ?? ErrorText.Format(_clientCode, culture, _codeArgs);
        }

        /// <summary>
        /// 지정한 언어의 안내 문구(코드 줄 제외) — "무엇이 잘못됐는지 + 호출 대상 + 실제 원인(개발자용)" 순서.
        /// 서버가 사유를 주었으면 그 사유가 첫 줄이고, 우리 코드 문구("서버가 요청을 처리하지 못했습니다. (HTTP 400)")는
        /// 코드 줄의 설명으로 내려간다 — 같은 내용을 두 번 적지 않으려고.
        /// </summary>
        public string MessageFor(CultureInfo culture)
        {
            var message = string.IsNullOrWhiteSpace(ServerMessage)
                ? _rawMessage ?? ErrorText.Format(_clientCode, culture, _codeArgs)
                : ServerMessage;

            if (!string.IsNullOrEmpty(_request))
                message += $"{Environment.NewLine}({_request})";

            if (!string.IsNullOrWhiteSpace(_cause))
                message += Environment.NewLine + string.Format(Language.GetString(nameof(Language.Error_CauseFormat), culture), _cause);

            return message;
        }

        /// <summary>
        /// 로그(로컬 파일)용 한 줄 — 항상 한국어. "[코드] 설명 | HTTP 상태 | 서버 사유 | 호출 대상 | 원인".
        /// 화면 언어와 상관없이 같은 형식이라 로그를 코드로 찾아볼 수 있고, 서버가 준 사유도 그대로 남는다.
        /// </summary>
        public string ToLogText()
        {
            var culture = ErrorText.ServerCulture;
            var text = ErrorText.Summary(Code, DescriptionFor(culture));

            if (!IsTransportFailure)
                text += $" | HTTP {(int)StatusCode} {_reasonPhrase}".TrimEnd();
            if (!string.IsNullOrEmpty(ErrorCode))
                text += " | 클라이언트 분류 " + ErrorText.Summary(_clientCode, ErrorText.Format(_clientCode, culture, _codeArgs));
            if (!string.IsNullOrWhiteSpace(ServerMessage))
                text += " | 서버 사유: " + OneLine(ServerMessage);
            if (!string.IsNullOrEmpty(_request))
                text += " | " + _request;
            if (!string.IsNullOrWhiteSpace(_cause))
                text += " | 원인: " + OneLine(_cause);

            return text;
        }

        /// <summary>
        /// 실패 상태코드 응답을 예외로 바꾼다. 서버 오류 본문(<c>{"message":..., "code":...}</c>)의 사유를 안내 첫 줄로 쓴다 —
        /// 상태코드만 보여 주면("API 호출 실패: 400") 작업자도 개발자도 무엇이 거절됐는지 알 수 없었다.
        /// 본문이 JSON이 아니거나 사유가 없으면(HTML 오류 페이지 등) 코드 문구("서버가 요청을 처리하지 못했습니다. (HTTP 400)")로 안내하고,
        /// 상태 문구(Bad Request 등)는 로그(<see cref="ToLogText"/>)에만 남긴다.
        /// </summary>
        public static ApiException FromErrorResponse(HttpStatusCode statusCode, string reasonPhrase, string responseBody)
        {
            string errorCode = null;
            string serverMessage = null;

            try
            {
                if (!string.IsNullOrWhiteSpace(responseBody) && JToken.Parse(responseBody) is JObject body)
                {
                    errorCode = FirstText(body, "code");
                    // 서버 공통 봉투는 message. 다른 형태(msg · detail)로 오는 응답도 사유를 놓치지 않게 차례로 본다.
                    serverMessage = FirstText(body, "message", "msg", "detail");
                }
            }
            catch (JsonException)
            {
                // 오류 본문이 JSON이 아니어도 실패 사실은 그대로 알려야 한다.
            }

            return new ApiException(statusCode, responseBody, ErrorCodes.ApiErrorResponse, new object[] { (int)statusCode },
                errorCode, serverMessage, reasonPhrase, null, null, null);
        }

        /// <summary>
        /// 통신 단계 예외를 화면에 그대로 띄울 수 있는 예외로 바꿔 담는다.
        /// 이미 <see cref="ApiException"/>이면 그대로 돌려준다(중복 포장 방지).
        /// </summary>
        public static ApiException FromTransportFailure(string method, string requestUri, Exception exception)
        {
            var apiException = exception as ApiException;
            if (apiException != null)
                return apiException;

            // HttpClient의 타임아웃도 TaskCanceledException(=OperationCanceledException)으로 올라온다.
            // 타임아웃의 실제 예외 문구는 "작업이 취소되었습니다"뿐이라 원인으로 덧붙여 봐야 혼란만 준다.
            string code;
            string cause = null;
            if (exception is OperationCanceledException)
            {
                code = ErrorCodes.ApiTimeout;
            }
            else
            {
                code = exception is HttpRequestException ? ErrorCodes.ApiConnectFailed : ErrorCodes.ApiCommunication;
                cause = RootCause(exception).Message;
            }

            return new ApiException((HttpStatusCode)0, null, code, null, null, null, null, Describe(method, requestUri), cause, exception);
        }

        /// <summary>
        /// 성공 응답을 받았지만 본문을 해석하지 못한 경우(HTML 오류 페이지, 필드 형식 불일치 등).
        /// 응답은 받았으므로 <see cref="IsTransportFailure"/>가 false가 되도록 받은 상태코드를 그대로 담는다.
        /// </summary>
        public static ApiException FromInvalidResponse(HttpStatusCode statusCode, string responseBody, string method, string requestUri, Exception exception)
        {
            return new ApiException(statusCode, responseBody, ErrorCodes.ApiInvalidResponse, null, null, null, null,
                Describe(method, requestUri), RootCause(exception).Message, exception);
        }

        /// <summary>
        /// 로그인 · 토큰 갱신이 200인데 토큰이 비어 있는 경우. 401로 다뤄 로그인 화면이 "로그인 실패"로 안내하게 한다.
        /// </summary>
        public static ApiException FromMissingToken()
        {
            return new ApiException(HttpStatusCode.Unauthorized, null, ErrorCodes.ApiNoToken, null, null, null, null, null, null, null);
        }

        /// <summary>본문에서 처음으로 값이 있는 문자열 필드. 필드가 객체 · 배열이면(형식이 다른 서버) 건너뛴다.</summary>
        private static string FirstText(JObject body, params string[] names)
        {
            foreach (var name in names)
            {
                if (body[name] is JValue value && value.Type == JTokenType.String && !string.IsNullOrWhiteSpace((string)value))
                    return ((string)value).Trim();
            }

            return null;
        }

        private static string OneLine(string text) =>
            text?.Replace("\r\n", " ").Replace('\n', ' ').Replace('\r', ' ').Trim();

        private static string Describe(string method, string requestUri) =>
            string.IsNullOrEmpty(requestUri) ? null : $"{method} {requestUri}";

        /// <summary>가장 안쪽 원인(SocketException 등)까지 내려가서 실제 사유를 꺼낸다.</summary>
        private static Exception RootCause(Exception exception)
        {
            while (exception.InnerException != null)
                exception = exception.InnerException;

            return exception;
        }
    }
}
