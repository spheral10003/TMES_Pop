using SEOJIN_TMES.Client.Common.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SEOJIN_TMES.Client.ApiClient
{
    /// <summary>
    /// 사용자 프로필(사번/이름/연락처/부서) 조회 API — <c>GET api/v1.0/gm/users/{userId}</c>.
    /// 로그인 응답에는 토큰만 있어서 프로필은 이 API로 따로 조회한다(사용자 정보 팝업에서 사용).
    /// </summary>
    public interface IUserProfileApiClient
    {
        /// <param name="userId">조회할 사용자 아이디. 로그인한 본인 조회에는 <c>UserSession.UserId</c>를 넘긴다.</param>
        Task<UserProfileDto> GetProfileAsync(string userId, CancellationToken cancellationToken = default);

        Task<WorkstationDto> GetLoginStationAsync(CancellationToken cancellationToken = default);
    }

    public class UserProfileApiClient : ApiClientBase, IUserProfileApiClient
    {
        public UserProfileApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<UserProfileDto> GetProfileAsync(string userId, CancellationToken cancellationToken = default)
        {
            // 아이디가 경로에 들어가므로 이스케이프한다 — '/'나 공백이 섞이면 엉뚱한 경로로 나간다.
            var path = ApiPrefix + "gm/users/" + Uri.EscapeDataString(userId ?? string.Empty);

            // 응답은 { "data": { ... } } 봉투다(Swagger: APIResponse«WithCorpDetailResponse»).
            // 벗기지 않고 UserProfileDto로 바로 받으면 모든 값이 null이 되어 팝업이 빈칸으로 뜬다.
            var envelope = await GetAsync<ApiDataResponse<UserProfileDto>>(path, cancellationToken).ConfigureAwait(false);
            return envelope?.Data;
        }

        public async Task<WorkstationDto> GetLoginStationAsync(CancellationToken cancellationToken = default)
        {
            var response = await GetAsync<ApiDataResponse<WorkstationDto>>(ApiPrefix + "gm/users/self/workstations", cancellationToken).ConfigureAwait(false);
            return response?.Data ?? new WorkstationDto();
        }
    }
}
