using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SEOJIN_MEIS.Client.Common.Models;

namespace SEOJIN_MEIS.Client.ApiClient
{
    /// <summary>
    /// 사용자 프로필(사번/이름/연락처/부서) 조회 API — <c>GET api/v1.0/gm/users/{userId}</c>.
    /// 로그인 응답에는 토큰만 있어서 프로필은 이 API로 따로 조회한다(사용자 정보 팝업에서 사용).
    /// </summary>
    public interface IUserProfileApiClient
    {
        /// <param name="userId">조회할 사용자 아이디. 로그인한 본인 조회에는 <c>UserSession.UserId</c>를 넘긴다.</param>
        Task<UserProfileDto> GetProfileAsync(string userId, CancellationToken cancellationToken = default);
    }

    public class UserProfileApiClient : ApiClientBase, IUserProfileApiClient
    {
        public UserProfileApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public Task<UserProfileDto> GetProfileAsync(string userId, CancellationToken cancellationToken = default)
        {
            // 아이디가 경로에 들어가므로 이스케이프한다 — '/'나 공백이 섞이면 엉뚱한 경로로 나간다.
            var path = ApiPrefix + "gm/users/" + Uri.EscapeDataString(userId ?? string.Empty);
            return GetAsync<UserProfileDto>(path, cancellationToken);
        }
    }
}
