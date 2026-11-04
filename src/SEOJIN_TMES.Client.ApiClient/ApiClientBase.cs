using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SEOJIN_TMES.Client.ApiClient;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Common.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SEOJIN_TMES.Client.ApiClient
{
    /// <summary>
    /// REST 호출 공통 배관(직렬화/역직렬화, 오류 처리)만 담당한다.
    /// 실제 업무 판단(수량 계산, 상태 전이 등)은 절대 여기 두지 않고 서버 응답을 그대로 전달한다.
    /// </summary>
    public abstract class ApiClientBase
    {
        /// <summary>
        /// 모든 REST 엔드포인트의 공통 접두사(API 버전 포함).
        /// API 개정(Revision) 시 이 상수 한 곳만 바꾸면 전체 엔드포인트에 반영된다 — 각 클라이언트에 버전을 흩어 두지 말 것.
        /// </summary>
        protected const string ApiPrefix = "api/v1.0/";

        protected readonly HttpClient HttpClient;

        protected ApiClientBase(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        protected Task<TResponse> GetAsync<TResponse>(string relativeUrl, CancellationToken cancellationToken = default) =>
            ExecuteAsync("GET", relativeUrl, async () =>
            {
                using (var response = await HttpClient.GetAsync(relativeUrl, cancellationToken).ConfigureAwait(false))
                {
                    return await ReadAsync<TResponse>(response).ConfigureAwait(false);
                }
            }, cancellationToken);

        protected Task<string> GetPathAsync<TResponse>(string relativeUrl, CancellationToken cancellationToken = default) =>
            ExecuteAsync("GET", relativeUrl, async () =>
            {
                using (var response = await HttpClient.GetAsync(relativeUrl, cancellationToken).ConfigureAwait(false))
                {
                    return await ReadPathAsync<string>(response).ConfigureAwait(false);
                }
            }, cancellationToken);

        /// <summary>
        /// multipart/form-data 본문을 POST하고 응답의 <b>알맹이</b>를 돌려준다(봉투가 있으면 벗긴다 — GetDataAsync와 같은 규칙).
        /// 본문은 호출자가 만들고, 이 메서드는 전송 · 타임아웃 변환 · 실패 시 ApiException만 담당한다.
        /// </summary>
        protected Task<TData> PostMultipartAsync<TData>(string relativeUrl, MultipartFormDataContent content, CancellationToken cancellationToken = default) =>
            ExecuteAsync("POST", relativeUrl, async () =>
            {
                using (var response = await HttpClient.PostAsync(relativeUrl, content, cancellationToken).ConfigureAwait(false))
                {
                    var body = await ReadBodyAsync(response).ConfigureAwait(false);
                    return Parse(response, body, Unwrap<TData>);
                }
            }, cancellationToken);

        /// <summary>
        /// 큰 파일용 multipart POST. <see cref="PostMultipartAsync{TData}(string, MultipartFormDataContent, CancellationToken)"/>와 같되
        /// 두 가지가 다르다.
        ///  - 제한 시간을 호출마다 받는다. <paramref name="client"/>는 Timeout이 무제한인 업로드 전용 HttpClient여야 하고,
        ///    시간 초과는 여기서 건 토큰이 낸다. 호출자 토큰은 ExecuteAsync에 그대로 넘기므로
        ///    시간 초과는 "취소"가 아니라 다른 호출과 똑같이 ApiException(서버 응답 시간 초과)으로 올라간다.
        ///  - 보낸 바이트 수를 <paramref name="progress"/>로 알린다(null이면 알리지 않는다).
        /// </summary>
        protected Task<TData> PostMultipartAsync<TData>(HttpClient client, string relativeUrl, MultipartFormDataContent content,
            TimeSpan timeout, IProgress<TransferProgress> progress, CancellationToken cancellationToken = default) =>
            ExecuteAsync("POST", relativeUrl, async () =>
            {
                using (var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    timeoutSource.CancelAfter(timeout);

                    HttpContent body = progress == null ? (HttpContent)content : new ProgressContent(content, progress);
                    using (var response = await client.PostAsync(relativeUrl, body, timeoutSource.Token).ConfigureAwait(false))
                    {
                        var responseBody = await ReadBodyAsync(response).ConfigureAwait(false);
                        return Parse(response, responseBody, Unwrap<TData>);
                    }
                }
            }, cancellationToken);

        /// <summary>
        /// GET 후 <b>알맹이</b>만 돌려준다. 이 서버는 응답을 <c>{ "data": ... }</c>로 싸는 곳과
        /// 그냥 내려주는 곳이 섞여 있고, Swagger 선언도 일관되지 않다
        /// (같은 엔드포인트가 알맹이 타입으로 선언돼 있는데 실제로는 봉투로 오는 경우가 있었다).
        ///
        /// 그래서 <b>봉투가 있으면 벗기고 없으면 그대로</b> 읽는다.
        /// 봉투 유무를 잘못 짚으면 값이 전부 null이 되는데, 예외가 나지 않아 발견이 늦다
        /// (로그인 토큰·사용자 프로필에서 실제로 겪었다).
        /// </summary>
        protected Task<TData> GetDataAsync<TData>(string relativeUrl, CancellationToken cancellationToken = default) =>
            ExecuteAsync("GET", relativeUrl, async () =>
            {
                using (var response = await HttpClient.GetAsync(relativeUrl, cancellationToken).ConfigureAwait(false))
                {
                    var body = await ReadBodyAsync(response).ConfigureAwait(false);
                    return Parse(response, body, Unwrap<TData>);
                }
            }, cancellationToken);

        /// <summary>봉투(<c>{"data":...}</c>)가 있으면 그 안을, 없으면 본문 전체를 대상 형식으로 읽는다.</summary>
        private static TData Unwrap<TData>(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
                return default;

            var token = JToken.Parse(body);

            // 최상위가 객체이고 "data" 키가 있으면 그것이 알맹이다.
            if (token is JObject obj && obj.TryGetValue("data", StringComparison.OrdinalIgnoreCase, out var data))
                token = data;

            return token.Type == JTokenType.Null ? default : token.ToObject<TData>();
        }

        /// <summary>
        /// JSON 본문을 POST하고 응답의 <b>알맹이</b>를 돌려준다(봉투가 있으면 벗긴다 — GetDataAsync와 같은 규칙).
        /// <see cref="PostAsync{TRequest, TResponse}"/>는 본문 전체를 그대로 읽어, 봉투로 오는 응답이면 값이 전부 null이 된다.
        /// </summary>
        protected Task<TData> PostDataAsync<TRequest, TData>(string relativeUrl, TRequest body, CancellationToken cancellationToken = default) =>
            ExecuteAsync("POST", relativeUrl, async () =>
            {
                using (var content = ToJsonContent(body))
                using (var response = await HttpClient.PostAsync(relativeUrl, content, cancellationToken).ConfigureAwait(false))
                {
                    var responseBody = await ReadBodyAsync(response).ConfigureAwait(false);
                    return Parse(response, responseBody, Unwrap<TData>);
                }
            }, cancellationToken);

        protected Task<TResponse> PostAsync<TRequest, TResponse>(string relativeUrl, TRequest body, CancellationToken cancellationToken = default) =>
            ExecuteAsync("POST", relativeUrl, async () =>
            {
                using (var content = ToJsonContent(body))
                using (var response = await HttpClient.PostAsync(relativeUrl, content, cancellationToken).ConfigureAwait(false))
                {
                    return await ReadAsync<TResponse>(response).ConfigureAwait(false);
                }
            }, cancellationToken);

        protected Task PostAsync<TRequest>(string relativeUrl, TRequest body, CancellationToken cancellationToken = default) =>
            ExecuteAsync("POST", relativeUrl, async () =>
            {
                using (var content = ToJsonContent(body))
                using (var response = await HttpClient.PostAsync(relativeUrl, content, cancellationToken).ConfigureAwait(false))
                {
                    await EnsureSuccessAsync(response).ConfigureAwait(false);
                    return true;
                }
            }, cancellationToken);

        /// <summary>
        /// GET 후 본문을 <b>바이트 그대로</b> 돌려준다(파일 다운로드용).
        /// JSON 봉투를 벗기는 GetDataAsync와 달리 본문을 해석하지 않는다 — 이미지/문서 바이너리를 깨뜨리지 않기 위해서다.
        /// </summary>
        protected Task<byte[]> GetBytesAsync(string relativeUrl, CancellationToken cancellationToken = default) =>
            ExecuteAsync("GET", relativeUrl, async () =>
            {
                using (var response = await HttpClient.GetAsync(relativeUrl, cancellationToken).ConfigureAwait(false))
                {
                    await EnsureSuccessAsync(response).ConfigureAwait(false);
                    return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                }
            }, cancellationToken);

        /// <summary>
        /// 바디(JSON)를 실어 DELETE한다. HttpClient.DeleteAsync는 바디를 받지 못해서 요청 메시지를 직접 만든다.
        /// 실패는 다른 호출과 똑같이 ApiException으로 올린다.
        /// </summary>
        protected Task DeleteAsync<TRequest>(string relativeUrl, TRequest body, CancellationToken cancellationToken = default) =>
            ExecuteAsync("DELETE", relativeUrl, async () =>
            {
                // request를 Dispose하면 Content도 함께 정리된다.
                using (var request = new HttpRequestMessage(HttpMethod.Delete, relativeUrl) { Content = ToJsonContent(body) })
                using (var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false))
                {
                    await EnsureSuccessAsync(response).ConfigureAwait(false);
                    return true;
                }
            }, cancellationToken);

        /// <summary>
        /// 바디(JSON)를 실어 PATCH한다. .NET Framework 4.7.2의 HttpClient에는 PatchAsync도, HttpMethod.Patch도 없어서
        /// 메서드 이름으로 요청 메시지를 직접 만든다. 실패는 다른 호출과 똑같이 ApiException으로 올린다.
        /// </summary>
        protected Task PatchAsync<TRequest>(string relativeUrl, TRequest body, CancellationToken cancellationToken = default) =>
            ExecuteAsync("PATCH", relativeUrl, async () =>
            {
                // request를 Dispose하면 Content도 함께 정리된다.
                using (var request = new HttpRequestMessage(new HttpMethod("PATCH"), relativeUrl) { Content = ToJsonContent(body) })
                using (var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false))
                {
                    await EnsureSuccessAsync(response).ConfigureAwait(false);
                    return true;
                }
            }, cancellationToken);

        /// <summary>
        /// 호출 한 건(전송 → 본문 읽기 → 역직렬화) 전체를 감싸 <b>어떤 실패든 <see cref="ApiException"/>으로만</b> 올린다.
        /// 화면은 catch(ApiException) 하나로 안내하므로, 다른 예외가 새면 async void 핸들러를 타고 앱이 중단된다.
        ///
        /// - 타임아웃: HttpClient는 타임아웃도 취소와 똑같이 TaskCanceledException으로 던진다.
        ///   핸들러(LoggingHandler) 안에서 보는 토큰은 타임아웃 토큰과 호출자 토큰이 합쳐진 링크 토큰이라
        ///   둘을 구분할 수 없고, <b>호출자가 준 원본 토큰</b>을 볼 수 있는 이 자리에서만 구분할 수 있다.
        ///   호출자가 정말 취소한 경우는 오류가 아니므로 그대로 통과시킨다.
        /// - LoggingHandler 밖에서 나는 통신 예외: HttpClient는 핸들러가 응답을 돌려준 <b>뒤에</b> 본문을 버퍼로 읽는데,
        ///   그 사이 연결이 끊기면 HttpRequestException/IOException이 핸들러를 거치지 않고 바로 올라온다.
        /// - 응답 형식 오류는 <see cref="Parse{T}"/>에서 따로 ApiException으로 바꾼다(상태코드를 알 수 있는 곳이 거기다).
        /// </summary>
        private async Task<T> ExecuteAsync<T>(string method, string relativeUrl, Func<Task<T>> call, CancellationToken cancellationToken)
        {
            try
            {
                return await call().ConfigureAwait(false);
            }
            catch (ApiException)
            {
                throw;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                // 타임아웃은 LoggingHandler가 이미 Warn으로 남겼다 — 그 밖의 예외만 여기서 스택까지 남긴다.
                if (!(ex is OperationCanceledException))
                    AppLog.Error($"API 호출 예외(핸들러 밖) {method} {AbsoluteUrl(relativeUrl)}", ex);

                throw ApiException.FromTransportFailure(method, AbsoluteUrl(relativeUrl), ex);
            }
        }

        /// <summary>
        /// 성공 응답의 본문을 해석한다. 해석이 안 되면(HTML 오류 페이지, 필드 형식 불일치 등)
        /// 통신 실패(StatusCode 0)가 아니라 <b>받은 상태코드 그대로</b> ApiException으로 올린다 —
        /// 0으로 올리면 로그인 화면이 "서버에 연결할 수 없습니다"로 잘못 안내한다.
        /// </summary>
        private static T Parse<T>(HttpResponseMessage response, string body, Func<string, T> parse)
        {
            try
            {
                return parse(body);
            }
            catch (Exception ex)
            {
                var request = response.RequestMessage;
                AppLog.Error($"API 응답 해석 실패 ({typeof(T).Name}) {request?.Method} {request?.RequestUri}", ex);
                throw ApiException.FromInvalidResponse(response.StatusCode, body, request?.Method?.Method, request?.RequestUri?.ToString(), ex);
            }
        }


        /// <summary>
        /// 값이 있는 조건만 골라 쿼리 문자열을 만든다(빈 조건은 서버로 보내지 않는다).
        /// 조건이 하나도 없으면 빈 문자열이라 그대로 이어 붙여도 URL이 깨지지 않는다.
        /// </summary>
        protected static string BuildQuery(params (string Name, string Value)[] conditions)
        {
            var parts = new List<string>();
            foreach (var condition in conditions)
            {
                if (!string.IsNullOrWhiteSpace(condition.Value))
                    parts.Add($"{condition.Name}={WebUtility.UrlEncode(condition.Value.Trim())}");
            }
            return parts.Count == 0 ? string.Empty : "?" + string.Join("&", parts);
        }


        /// <summary>안내 문구에 넣을 전체 URL(BaseAddress + 상대 경로).</summary>
        private string AbsoluteUrl(string relativeUrl) =>
            HttpClient.BaseAddress == null ? relativeUrl : new Uri(HttpClient.BaseAddress, relativeUrl).ToString();

        private static StringContent ToJsonContent(object body)
        {
            var json = JsonConvert.SerializeObject(body);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        private static async Task<TResponse> ReadAsync<TResponse>(HttpResponseMessage response)
        {
            var body = await ReadBodyAsync(response).ConfigureAwait(false);
            return string.IsNullOrWhiteSpace(body) ? default : Parse(response, body, JsonConvert.DeserializeObject<TResponse>);

            //if (string.IsNullOrWhiteSpace(body))
            //    return default;

            //var token = JsonConvert.DeserializeObject<ApiDataResponse<TResponse>>(body);
            //return token.Data;
        }

        private static async Task<string> ReadPathAsync<TResponse>(HttpResponseMessage response)
        {
            var body = await ReadBodyAsync(response).ConfigureAwait(false);
            return string.IsNullOrWhiteSpace(body) ? default : body;
        }

        /// <summary>본문을 읽고, 실패 상태코드면 <see cref="ApiException"/>으로 올린다.</summary>
        private static async Task<string> ReadBodyAsync(HttpResponseMessage response)
        {
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                throw ApiException.FromErrorResponse(response.StatusCode, response.ReasonPhrase, body);

            return body;
        }

        private static async Task EnsureSuccessAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return;

            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            throw ApiException.FromErrorResponse(response.StatusCode, response.ReasonPhrase, body);
        }
    }
}
