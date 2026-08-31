using System.Threading;
using System.Threading.Tasks;
using SEOJIN_MEIS.Client.Common.Logging;
using SEOJIN_MEIS.Client.Common.Models;
using SEOJIN_MEIS.Client.Common.Security;

namespace SEOJIN_MEIS.Client.ApiClient
{
    /// <summary>로그인/토큰 재발급. 인증 자체 판정은 서버 책임이며 클라이언트는 결과만 받아 저장한다.</summary>
    public interface IAuthApiClient
    {
        /// <summary>
        /// 로그인. 서버가 nonce 방식이므로 내부에서 두 번 호출한다 —
        /// ① my-salt로 salt 수신 → ② 비밀번호 해시 + salt를 실어 login.
        /// 화면은 평문 비밀번호만 넘기면 되고, 해시/salt 처리는 여기서 끝난다.
        /// </summary>
        Task<LoginResponse> LoginAsync(string orgNo, string password, CancellationToken cancellationToken = default);

        /// <summary>로그인 직전에 받아오는 salt(nonce). 로그인 바디에 그대로 다시 실어 보낸다.</summary>
        Task<string> GetSaltAsync(CancellationToken cancellationToken = default);

        Task<LoginResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    }

    public class AuthApiClient : ApiClientBase, IAuthApiClient
    {
        public AuthApiClient(System.Net.Http.HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<LoginResponse> LoginAsync(string orgNo, string password, CancellationToken cancellationToken = default)
        {
            var salt = await GetSaltAsync(cancellationToken).ConfigureAwait(false);

            var request = new LoginRequest
            {
                OrgNo = orgNo,
                Password = PasswordHash.ForLogin(password, salt),
                Salt = salt
            };

            return await PostAsync<LoginRequest, LoginResponse>(ApiPrefix + "auth/login", request, cancellationToken)
                       .ConfigureAwait(false);
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

        public Task<LoginResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            var request = new RefreshTokenRequest { RefreshToken = refreshToken };
            return PostAsync<RefreshTokenRequest, LoginResponse>(ApiPrefix + "auth/refresh", request, cancellationToken);
        }
    }
}
