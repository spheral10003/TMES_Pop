using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace SEOJIN_MEIS.Client.ApiClient
{
    /// <summary>
    /// 모든 인증 필요 API 호출에 Authorization: Bearer 헤더를 자동으로 붙이고,
    /// 401 응답을 받으면 RefreshToken으로 1회 재시도한다. 그 이상의 판단(재로그인 등)은 UI 계층 책임.
    /// </summary>
    public class AuthTokenHandler : DelegatingHandler
    {
        private readonly ITokenProvider _tokenProvider;
        private readonly IAuthApiClient _authApiClient;
        private readonly SemaphoreSlim _refreshLock = new SemaphoreSlim(1, 1);

        public AuthTokenHandler(ITokenProvider tokenProvider, IAuthApiClient authApiClient, HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            _tokenProvider = tokenProvider;
            _authApiClient = authApiClient;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            ApplyToken(request);
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.Unauthorized || string.IsNullOrEmpty(_tokenProvider.RefreshToken))
                return response;

            var refreshed = await TryRefreshAsync(cancellationToken).ConfigureAwait(false);
            if (!refreshed)
            {
                _tokenProvider.RaiseSessionExpired();
                return response;
            }

            response.Dispose();
            var retryRequest = await CloneRequestAsync(request).ConfigureAwait(false);
            ApplyToken(retryRequest);
            return await base.SendAsync(retryRequest, cancellationToken).ConfigureAwait(false);
        }

        private void ApplyToken(HttpRequestMessage request)
        {
            if (!string.IsNullOrEmpty(_tokenProvider.AccessToken))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenProvider.AccessToken);
        }

        private async Task<bool> TryRefreshAsync(CancellationToken cancellationToken)
        {
            await _refreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var result = await _authApiClient.RefreshTokenAsync(_tokenProvider.RefreshToken, cancellationToken).ConfigureAwait(false);
                if (result == null)
                    return false;

                _tokenProvider.SetTokens(result.AccessToken, result.RefreshToken);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri);
            if (request.Content != null)
            {
                var bytes = await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                clone.Content = new ByteArrayContent(bytes);
                foreach (var header in request.Content.Headers)
                    clone.Content.Headers.Add(header.Key, header.Value);
            }
            foreach (var header in request.Headers)
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            return clone;
        }
    }
}
