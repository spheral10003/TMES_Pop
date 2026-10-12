using System;
using System.Net;
using System.Net.Http;

namespace SEOJIN_TMES.Client.ApiClient
{
    /// <summary>
    /// 서버 REST API 호출 실패 시 던지는 예외. 서버가 반환한 상태코드/본문을 그대로 담아 상위(Form)에서 메시지 처리.
    ///
    /// 응답을 아예 못 받은 실패(서버 미기동/네트워크 단절/타임아웃 등)도 화면이 같은 방식으로 다룰 수 있도록
    /// 이 예외로 감싸서 올린다(<see cref="FromTransportFailure"/>) — 그래야 각 화면의 catch(ApiException)에서
    /// 안내 창만 띄우고 작업을 계속 이어갈 수 있다. 이 경우 <see cref="StatusCode"/>는 0이다.
    /// </summary>
    public class ApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string ResponseBody { get; }

        public ApiException(HttpStatusCode statusCode, string responseBody, string message, Exception innerException = null)
            : base(message, innerException)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }

        /// <summary>서버 응답 자체를 받지 못한 실패(연결 실패/타임아웃 등)인지 여부.</summary>
        public bool IsTransportFailure => (int)StatusCode == 0;

        /// <summary>
        /// 통신 단계 예외를 화면에 그대로 띄울 수 있는 안내 문구로 바꿔 담는다.
        /// 이미 <see cref="ApiException"/>이면 그대로 돌려준다(중복 포장 방지).
        /// </summary>
        public static ApiException FromTransportFailure(string method, string requestUri, Exception exception)
        {
            var apiException = exception as ApiException;
            if (apiException != null)
                return apiException;

            return new ApiException((HttpStatusCode)0, null, BuildMessage(method, requestUri, exception), exception);
        }

        /// <summary>"무엇이 잘못됐는지(사용자용) + 호출 대상 + 실제 원인(개발자용)" 순서로 만든다.</summary>
        private static string BuildMessage(string method, string requestUri, Exception exception)
        {
            var message = Describe(exception);

            if (!string.IsNullOrEmpty(requestUri))
                message += $"{Environment.NewLine}({method} {requestUri})";

            // 타임아웃의 실제 예외 문구는 "작업이 취소되었습니다"뿐이라 덧붙여 봐야 혼란만 준다.
            if (!(exception is OperationCanceledException))
                message += $"{Environment.NewLine}원인: {RootCause(exception).Message}";

            return message;
        }

        private static string Describe(Exception exception)
        {
            // HttpClient의 타임아웃도 TaskCanceledException(=OperationCanceledException)으로 올라온다.
            if (exception is OperationCanceledException)
                return "서버 응답 시간이 초과되었습니다. 잠시 후 다시 시도해 주세요.";

            if (exception is HttpRequestException)
                return "서버에 연결할 수 없습니다. 서버 주소와 네트워크 상태를 확인해 주세요.";

            return "서버 통신 중 오류가 발생했습니다.";
        }

        /// <summary>가장 안쪽 원인(SocketException 등)까지 내려가서 실제 사유를 꺼낸다.</summary>
        private static Exception RootCause(Exception exception)
        {
            while (exception.InnerException != null)
                exception = exception.InnerException;

            return exception;
        }
    }
}
