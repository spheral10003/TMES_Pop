using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SEOJIN_MEIS.Client.Common.Logging;

namespace SEOJIN_MEIS.Client.ApiClient
{
    /// <summary>
    /// 모든 REST 호출의 전체 URL·메서드·응답코드·소요시간을 AppLog로 남긴다.
    /// 요청/응답 바디는 로그인 비밀번호 등 민감정보가 섞일 수 있어 기록하지 않는다.
    ///
    /// 통신 자체가 실패한 예외(서버 미기동, 네트워크 단절, 타임아웃 등)는 그대로 올리지 않고
    /// <see cref="ApiException"/>으로 바꿔서 올린다 — 화면마다 이미 catch(ApiException)로 안내 창을 띄우고 있어서,
    /// 이렇게 해야 앱이 죽지 않고 안내만 뜬 뒤 계속 작업할 수 있다(Debug 빌드에서 그대로 중단되던 문제).
    /// </summary>
    public class LoggingHandler : DelegatingHandler
    {
        public LoggingHandler(HttpMessageHandler innerHandler) : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            AppLog.Info($"API 요청 {request.Method} {request.RequestUri}");

            try
            {
                var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                stopwatch.Stop();
                AppLog.Info($"API 응답 {(int)response.StatusCode} {response.ReasonPhrase} ({stopwatch.ElapsedMilliseconds}ms) {request.Method} {request.RequestUri}");
                return response;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // 취소(호출부가 취소) 또는 타임아웃(HttpClient가 내부 토큰으로 취소) — 이 자리에서는 둘을 구분할 수 없다.
                // 여기 토큰은 두 경우 모두 취소 상태인 링크 토큰이라서, 구분은 원본 토큰을 쥐고 있는 ApiClientBase가 한다.
                stopwatch.Stop();
                AppLog.Warn($"API 호출 중단 - 취소 또는 응답 시간 초과 ({stopwatch.ElapsedMilliseconds}ms) {request.Method} {request.RequestUri}");
                throw;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                AppLog.Error($"API 호출 예외 ({stopwatch.ElapsedMilliseconds}ms) {request.Method} {request.RequestUri}", ex);
                throw ApiException.FromTransportFailure(request.Method?.Method, request.RequestUri?.ToString(), ex);
            }
        }
    }
}
