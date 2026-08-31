using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SEOJIN_MEIS.Client.ApiClient
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

        protected async Task<TResponse> GetAsync<TResponse>(string relativeUrl, CancellationToken cancellationToken = default)
        {
            using (var response = await SendAsync("GET", relativeUrl, () => HttpClient.GetAsync(relativeUrl, cancellationToken), cancellationToken).ConfigureAwait(false))
            {
                return await ReadAsync<TResponse>(response).ConfigureAwait(false);
            }
        }

        protected async Task<TResponse> PostAsync<TRequest, TResponse>(string relativeUrl, TRequest body, CancellationToken cancellationToken = default)
        {
            using (var content = ToJsonContent(body))
            using (var response = await SendAsync("POST", relativeUrl, () => HttpClient.PostAsync(relativeUrl, content, cancellationToken), cancellationToken).ConfigureAwait(false))
            {
                return await ReadAsync<TResponse>(response).ConfigureAwait(false);
            }
        }

        protected async Task PostAsync<TRequest>(string relativeUrl, TRequest body, CancellationToken cancellationToken = default)
        {
            using (var content = ToJsonContent(body))
            using (var response = await SendAsync("POST", relativeUrl, () => HttpClient.PostAsync(relativeUrl, content, cancellationToken), cancellationToken).ConfigureAwait(false))
            {
                await EnsureSuccessAsync(response).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// HttpClient 호출을 감싸 <b>응답 시간 초과</b>를 <see cref="ApiException"/>으로 바꿔 올린다.
        ///
        /// HttpClient는 타임아웃도 취소와 똑같이 TaskCanceledException으로 던진다.
        /// 핸들러(LoggingHandler) 안에서 보는 토큰은 타임아웃 토큰과 호출자 토큰이 합쳐진 링크 토큰이라
        /// 둘을 구분할 수 없고, <b>호출자가 준 원본 토큰</b>을 볼 수 있는 이 자리에서만 구분할 수 있다.
        /// 원본 토큰이 취소되지 않았는데 취소 예외가 났다면 = 타임아웃이므로 화면이 안내할 수 있게 ApiException으로 바꾼다.
        /// (호출자가 정말 취소한 경우는 오류가 아니므로 그대로 통과시킨다.)
        /// </summary>
        private async Task<HttpResponseMessage> SendAsync(string method, string relativeUrl, Func<Task<HttpResponseMessage>> send, CancellationToken cancellationToken)
        {
            try
            {
                return await send().ConfigureAwait(false);
            }
            catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                throw ApiException.FromTransportFailure(method, AbsoluteUrl(relativeUrl), ex);
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
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                throw new ApiException(response.StatusCode, body, $"API 호출 실패: {(int)response.StatusCode} {response.ReasonPhrase}");

            return string.IsNullOrWhiteSpace(body) ? default : JsonConvert.DeserializeObject<TResponse>(body);
        }

        private static async Task EnsureSuccessAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return;

            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            throw new ApiException(response.StatusCode, body, $"API 호출 실패: {(int)response.StatusCode} {response.ReasonPhrase}");
        }
    }
}
