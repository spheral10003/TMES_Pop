using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Common.Security;

namespace SEOJIN_TMES.Client.ApiClient
{
    /// <summary>로그인/토큰 재발급. 인증 자체 판정은 서버 책임이며 클라이언트는 결과만 받아 저장한다.</summary>
    public interface IAuthApiClient
    {
        /// <summary>
        /// 로그인. 서버가 nonce 방식이므로 내부에서 두 번 호출한다 —
        /// ① my-salt로 salt 수신 → ② SHA256(SHA256(비밀번호) + salt)를 실어 login.
        /// 화면은 평문 비밀번호만 넘기면 되고, 해시/salt 처리는 여기서 끝난다.
        /// </summary>
        /// <param name="corpCd">사업장 코드 — 로그인 화면의 접속서버 콤보에서 고른 값.</param>
        Task<LoginResponse> LoginAsync(string corpCd, string orgNo, string password, CancellationToken cancellationToken = default);

        //Task<List<string>> GetLoginStationAsync(CancellationToken cancellationToken = default);

        /// <summary>로그인 직전에 받아오는 salt(nonce). 로그인 바디에 그대로 다시 실어 보낸다.</summary>
        Task<string> GetSaltAsync(CancellationToken cancellationToken = default);

        Task<LoginResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    }

    public class AuthApiClient : ApiClientBase, IAuthApiClient
    {
        public AuthApiClient(System.Net.Http.HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<LoginResponse> LoginAsync(string corpCd, string orgNo, string password, CancellationToken cancellationToken = default)
        {
            var salt = await GetSaltAsync(cancellationToken).ConfigureAwait(false);

            var request = new LoginRequest
            {
                CorpCd = corpCd,
                OrgNo = orgNo,
                Password = PasswordHash.ForLogin(password, salt),
                Salt = salt
            };

            // 응답은 { "data": { accessToken, refreshToken, user } } 형태다(my-salt와 같은 봉투).
            // 봉투를 벗기지 않고 LoginResponse로 바로 받으면 토큰이 null이 되고,
            // 그러면 이후 모든 API에 Authorization 헤더가 붙지 않아 전부 401이 된다.
            var envelope = await PostAsync<LoginRequest, ApiDataResponse<LoginResponse>>(
                ApiPrefix + "auth/login", request, cancellationToken).ConfigureAwait(false);

            return RequireTokens(envelope?.Data, "로그인");
        }

        /// <summary>
        /// 토큰이 실제로 담겨 왔는지 확인한다.
        /// 200인데 토큰이 비어 있으면 <b>실패로 다루는 편이 안전하다</b> —
        /// 그냥 넘기면 "로그인 성공"이라고 알린 뒤 모든 화면이 401로 실패해 원인을 찾기 어려워진다.
        /// </summary>
        private static LoginResponse RequireTokens(LoginResponse response, string what)
        {
            if (!string.IsNullOrEmpty(response?.AccessToken))
                return response;

            AppLog.Warn($"{what} 응답에 accessToken이 없다 - 서버 응답 형식을 확인할 것(봉투 구조 변경 여부)");
            throw new ApiException(HttpStatusCode.Unauthorized, null, $"{what} 응답에 토큰이 없습니다.");
        }

        public async Task<string> GetSaltAsync(CancellationToken cancellationToken = default)
        {
            var response = await GetAsync<ApiDataResponse<string>>(ApiPrefix + "common/my-salt", cancellationToken).ConfigureAwait(false);
            var salt = response?.Data;

            // salt가 비어 있으면 어차피 서버가 로그인을 거절한다. 원인을 찾기 쉽도록 로그를 남겨 둔다.
            if (string.IsNullOrEmpty(salt))
                AppLog.Warn("my-salt 응답에 salt(data)가 없다 - 서버 응답 형식을 확인할 것");

            return salt;
        }

        //public async Task<List<string>> GetLoginStationAsync(CancellationToken cancellationToken = default)
        //{
        //    var response = await GetAsync<ApiDataResponse<List<string>>>(ApiPrefix + "gm/users/self/workstations", cancellationToken).ConfigureAwait(false);
        //    return response?.Data ?? new List<string>();
        //}


        public async Task<LoginResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            var request = new RefreshTokenRequest { RefreshToken = refreshToken };

            // 경로는 auth/refresh 가 아니라 auth/login/refresh 다(Swagger: POST /api/v1.0/auth/login/refresh).
            // 응답 봉투도 로그인과 같다.
            var envelope = await PostAsync<RefreshTokenRequest, ApiDataResponse<LoginResponse>>(
                ApiPrefix + "auth/login/refresh", request, cancellationToken).ConfigureAwait(false);

            return RequireTokens(envelope?.Data, "토큰 갱신");
        }
    }
}
