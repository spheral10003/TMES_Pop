using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SEOJIN_MEIS.Client.Common.Models;

namespace SEOJIN_MEIS.Client.ApiClient
{
    /// <summary>
    /// 로그인 사용자 프로필(사번/이름/연락처/부서) 조회 API.
    /// 로그인 응답에는 토큰만 있어서 프로필은 이 API로 따로 조회한다(사용자 정보 팝업에서 사용).
    /// TODO: 엔드포인트 경로는 가정값(`api/v1.0/users/me`) — 실제 서버 스펙 확정 후 조정 필요.
    /// </summary>
    public interface IUserProfileApiClient
    {
        Task<UserProfileDto> GetMyProfileAsync(CancellationToken cancellationToken = default);
    }

    public class UserProfileApiClient : ApiClientBase, IUserProfileApiClient
    {
        public UserProfileApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public Task<UserProfileDto> GetMyProfileAsync(CancellationToken cancellationToken = default) =>
            GetAsync<UserProfileDto>(ApiPrefix + "users/me", cancellationToken);
    }
}
