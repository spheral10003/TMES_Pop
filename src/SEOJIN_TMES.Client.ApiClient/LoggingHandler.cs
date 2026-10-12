using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using SEOJIN_TMES.Client.Common.Logging;

namespace SEOJIN_TMES.Client.ApiClient
{
    /// <summary>
    /// 모든 REST 호출의 전체 URL·메서드·요청/응답 헤더·응답코드·소요시간을 AppLog로 남긴다.
    /// <b>응답 본문</b>은 App.config의 <c>LogApiResponseBody</c>가 켜져 있을 때만 남긴다(서버 연동을 맞출 때 쓴다).
    ///
    /// 비밀 값은 기록 전에 가린다 — 헤더는 <c>Authorization=Bearer ***(61자)</c>,
    /// 본문은 <c>"accessToken":"***"</c> 꼴이다. 로그 파일은 현장 PC에 그대로 쌓이므로
    /// 토큰·비밀번호가 적히면 그 자체가 유출 경로가 된다.
    /// <b>요청 본문은 남기지 않는다</b> — 로그인 바디처럼 통째로 민감한 경우가 있어서다.
    ///
    /// 통신 자체가 실패한 예외(서버 미기동, 네트워크 단절, 타임아웃 등)는 그대로 올리지 않고
    /// <see cref="ApiException"/>으로 바꿔서 올린다 — 화면마다 이미 catch(ApiException)로 안내 창을 띄우고 있어서,
    /// 이렇게 해야 앱이 죽지 않고 안내만 뜬 뒤 계속 작업할 수 있다(Debug 빌드에서 그대로 중단되던 문제).
    /// </summary>
    public class LoggingHandler : DelegatingHandler
    {
        private readonly bool _logResponseBody;
        private readonly int _bodyMaxLength;

        public LoggingHandler(HttpMessageHandler innerHandler, bool logResponseBody = false, int bodyMaxLength = 4000)
            : base(innerHandler)
        {
            _logResponseBody = logResponseBody;
            _bodyMaxLength = bodyMaxLength <= 0 ? 4000 : bodyMaxLength;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            AppLog.Info($"API 요청 {request.Method} {request.RequestUri}");
            AppLog.Info($"  요청 헤더 {Describe(request.Headers)}");

            try
            {
                var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                stopwatch.Stop();
                AppLog.Info($"API 응답 {(int)response.StatusCode} {response.ReasonPhrase} ({stopwatch.ElapsedMilliseconds}ms) {request.Method} {request.RequestUri}");
                AppLog.Info($"  응답 헤더 {Describe(response.Headers)}");

                if (_logResponseBody)
                    AppLog.Info($"  응답 본문 {await ReadBodyForLogAsync(response).ConfigureAwait(false)}");

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

        /// <summary>
        /// 로그에 남길 응답 본문을 만든다.
        ///
        /// <b>본문을 먼저 버퍼에 올린 뒤</b> 읽는다 — 스트림을 여기서 소비해 버리면
        /// 뒤에서 역직렬화할 내용이 사라진다(버퍼에 올려 두면 몇 번이든 다시 읽힌다).
        /// 어떤 실패도 밖으로 던지지 않는다. 로그 때문에 정상 응답 처리가 깨지면 안 된다.
        /// </summary>
        private async Task<string> ReadBodyForLogAsync(HttpResponseMessage response)
        {
            if (response.Content == null)
                return "(본문 없음)";

            try
            {
                await response.Content.LoadIntoBufferAsync().ConfigureAwait(false);
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (string.IsNullOrEmpty(body))
                    return "(빈 본문)";

                var masked = MaskSecrets(body);
                return masked.Length > _bodyMaxLength
                    ? masked.Substring(0, _bodyMaxLength) + $"... (전체 {masked.Length}자)"
                    : masked;
            }
            catch (Exception ex)
            {
                return $"(본문을 읽지 못함: {ex.Message})";
            }
        }

        /// <summary>
        /// JSON 본문에서 비밀 값을 <c>***</c>로 바꾼다 — <c>"accessToken":"eyJ..."</c> → <c>"accessToken":"***"</c>.
        ///
        /// 로그 파일은 현장 PC에 그대로 쌓이므로 토큰·비밀번호가 적히면 그대로 유출 경로가 된다.
        /// 문자열 값만 바꾸므로 본문의 나머지(업무 데이터)는 그대로 보여 연동 확인에는 지장이 없다.
        /// </summary>
        private static string MaskSecrets(string body) => SecretValuePattern.Replace(body, "$1\"***\"");

        /// <summary>
        /// <c>"키" : "값"</c> 에서 키가 비밀 항목인 경우를 잡는다(대소문자 무시).
        /// 키 이름은 서버 스펙(accessToken/refreshToken/password/salt)과 흔한 변형을 함께 넣었다.
        /// </summary>
        private static readonly Regex SecretValuePattern = new Regex(
            "(\"(?:access_?token|refresh_?token|password|passwd|pwd|salt|secret|api_?key|authorization)\"\\s*:\\s*)\"[^\"]*\"",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// 헤더를 한 줄로 정리한다. 값이 <b>비밀</b>인 헤더는 이름만 남기고 가린다 —
        /// 로그 파일은 현장 PC에 그대로 쌓이므로 토큰이 적히면 그대로 유출 경로가 된다(루트 CLAUDE.md 규약).
        /// 그래도 "헤더가 붙었는지 / 길이가 그럴듯한지"는 보이므로 인증 문제를 추적하는 데는 충분하다.
        /// </summary>
        private static string Describe(HttpHeaders headers)
        {
            if (headers == null)
                return "(없음)";

            var parts = headers
                .Select(h => h.Key + "=" + Describe(h.Key, string.Join(", ", h.Value)))
                .ToList();

            return parts.Count == 0 ? "(없음)" : string.Join(" | ", parts);
        }

        /// <summary>
        /// 인증 헤더 — <c>"Bearer eyJ..."</c>처럼 [방식 + 자격증명] 꼴이라 <b>방식만</b> 남기고 가린다.
        /// 방식이 보여야 "헤더가 아예 안 붙음"과 "붙였는데 서버가 거절함"을 구분할 수 있다.
        /// </summary>
        private static readonly string[] AuthHeaders = { "Authorization", "Proxy-Authorization" };

        /// <summary>값 전체가 비밀인 헤더 — 일부만 남겨도 세션을 재현할 수 있어 <b>통째로</b> 가린다.</summary>
        private static readonly string[] FullySecretHeaders = { "Cookie", "Set-Cookie", "X-Api-Key" };

        private static string Describe(string name, string value)
        {
            var isAuth = AuthHeaders.Contains(name, StringComparer.OrdinalIgnoreCase);
            var isSecret = isAuth || FullySecretHeaders.Contains(name, StringComparer.OrdinalIgnoreCase);

            if (!isSecret)
                return value;

            if (string.IsNullOrEmpty(value))
                return "(빈 값)";

            return isAuth
                ? $"{SchemeOf(value)}***({value.Length}자)"
                : $"***({value.Length}자)";
        }

        /// <summary>
        /// 인증 헤더의 방식(Bearer/Basic 등)만 뽑는다.
        /// 첫 낱말이 <b>영문자로만</b> 되어 있을 때에만 방식으로 인정한다 —
        /// 그냥 "첫 공백 앞"으로 자르면 <c>JSESSIONID=비밀값;</c> 같은 값이 방식으로 오인돼 그대로 새어 나간다.
        /// </summary>
        private static string SchemeOf(string value)
        {
            var space = value.IndexOf(' ');
            if (space <= 0)
                return string.Empty;

            var candidate = value.Substring(0, space);
            return candidate.All(char.IsLetter) ? candidate + " " : string.Empty;
        }
    }
}
