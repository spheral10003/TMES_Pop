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
    /// Message 끝에는 "오류 코드" 줄이 붙어 있어 화면은 <c>ex.Message</c>를 그대로 띄우면 된다.
    /// 서버 로그용 한국어 문구는 <see cref="MessageFor"/>로 꺼낸다.
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

        /// <summary>서버가 오류 본문에 담아 준 사유. 서버가 준 문장이라 번역하지 않고 그대로 보여 준다.</summary>
        private readonly string _serverMessage;

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
            string errorCode, string serverMessage, string request, string cause, Exception innerException)
            : base(ErrorText.Korean(clientCode, codeArgs), innerException)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
            ErrorCode = errorCode;
            _clientCode = clientCode;
            _codeArgs = codeArgs;
            _serverMessage = serverMessage;
            _request = request;
            _cause = cause;
        }

        /// <summary>서버 응답 자체를 받지 못한 실패(연결 실패/타임아웃 등)인지 여부.</summary>
        public bool IsTransportFailure => (int)StatusCode == 0;

        /// <summary>현재 화면 언어의 안내 문구 + 오류 코드 줄.</summary>
        public override string Message => ErrorText.WithCode(MessageFor(CultureInfo.CurrentUICulture), Code);

        /// <summary>
        /// 지정한 언어의 안내 문구(코드 줄 제외) — "무엇이 잘못됐는지(사용자용) + 서버 사유 + 호출 대상 + 실제 원인(개발자용)" 순서.
        /// </summary>
        public string MessageFor(CultureInfo culture)
        {
            var message = _rawMessage ?? ErrorText.Format(_clientCode, culture, _codeArgs);

            if (!string.IsNullOrWhiteSpace(_serverMessage))
                message += Environment.NewLine + _serverMessage;

            if (!string.IsNullOrEmpty(_request))
                message += $"{Environment.NewLine}({_request})";

            if (!string.IsNullOrWhiteSpace(_cause))
                message += Environment.NewLine + string.Format(Language.GetString(nameof(Language.Error_CauseFormat), culture), _cause);

            return message;
        }

        /// <summary>
        /// 실패 상태코드 응답을 예외로 바꾼다. 서버 오류 본문(<c>{"message":..., "code":...}</c>)의 사유를 안내 문구에 붙인다 —
        /// 상태코드만 보여 주면("API 호출 실패: 400") 작업자도 개발자도 무엇이 거절됐는지 알 수 없었다.
        /// 본문이 JSON이 아니면(HTML 오류 페이지 등) 상태코드만으로 안내한다.
        /// </summary>
        public static ApiException FromErrorResponse(HttpStatusCode statusCode, string reasonPhrase, string responseBody)
        {
            string errorCode = null;
            string serverMessage = null;

            try
            {
                if (!string.IsNullOrWhiteSpace(responseBody) && JToken.Parse(responseBody) is JObject body)
                {
                    errorCode = (string)body["code"];
                    serverMessage = (string)body["message"];
                }
            }
            catch (JsonException)
            {
                // 오류 본문이 JSON이 아니어도 실패 사실은 그대로 알려야 한다.
            }

            if (string.IsNullOrWhiteSpace(errorCode))
                errorCode = null;

            // 서버가 사유를 주지 않으면 상태 문구(Bad Request 등)라도 남긴다.
            if (string.IsNullOrWhiteSpace(serverMessage))
                serverMessage = reasonPhrase;

            return new ApiException(statusCode, responseBody, ErrorCodes.ApiErrorResponse, new object[] { (int)statusCode },
                errorCode, serverMessage, null, null, null);
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

            return new ApiException((HttpStatusCode)0, null, code, null, null, null, Describe(method, requestUri), cause, exception);
        }

        /// <summary>
        /// 성공 응답을 받았지만 본문을 해석하지 못한 경우(HTML 오류 페이지, 필드 형식 불일치 등).
        /// 응답은 받았으므로 <see cref="IsTransportFailure"/>가 false가 되도록 받은 상태코드를 그대로 담는다.
        /// </summary>
        public static ApiException FromInvalidResponse(HttpStatusCode statusCode, string responseBody, string method, string requestUri, Exception exception)
        {
            return new ApiException(statusCode, responseBody, ErrorCodes.ApiInvalidResponse, null, null, null,
                Describe(method, requestUri), RootCause(exception).Message, exception);
        }

        /// <summary>
        /// 로그인 · 토큰 갱신이 200인데 토큰이 비어 있는 경우. 401로 다뤄 로그인 화면이 "로그인 실패"로 안내하게 한다.
        /// </summary>
        public static ApiException FromMissingToken()
        {
            return new ApiException(HttpStatusCode.Unauthorized, null, ErrorCodes.ApiNoToken, null, null, null, null, null, null);
        }

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
