using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Common.Security;
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

        /// <summary>
        /// 비밀번호 변경 — <c>PATCH api/v1.0/gm/users/{orgNo}/pswd-chg</c>, 바디 <c>{ "pswd": SHA256(새 비밀번호) }</c>.
        /// 평문만 넘기면 해시는 여기서 <see cref="PasswordHash.ForChange"/>로 만든다(평문은 서버로 나가지 않는다).
        /// 비밀번호 규칙(길이 · 문자 조합 · 이전 비밀번호 재사용) 판정은 서버가 하며, 어긋나면 <see cref="ApiException"/>으로 올라온다.
        /// </summary>
        /// <param name="orgNo">사번(로그인 아이디와 같은 값).</param>
        /// <returns>
        /// 서버 응답 봉투(<c>{status, message, code, data}</c>) — 성공 여부는 <see cref="ApiResponse{T}.IsSuccess"/>로 본다.
        /// 서버가 2xx에 본문 없이 응답하면 null이다(이때는 상태코드로 이미 성공이 확인된 것이다).
        /// </returns>
        Task<ApiResponse<object>> SetPassword(string orgNo, string plainPassword, CancellationToken cancellationToken = default);
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

        public Task<ApiResponse<object>> SetPassword(string orgNo, string plainPassword, CancellationToken cancellationToken = default)
        {
            // 사번이 경로에 들어가므로 이스케이프한다(GetProfileAsync와 같은 이유).
            var path = ApiPrefix + "gm/users/" + Uri.EscapeDataString(orgNo ?? string.Empty) + "/pswd-chg";
            var body = new PasswordChangeRequest { PasswordHash = PasswordHash.ForChange(plainPassword) };
            return PatchAsync<PasswordChangeRequest, ApiResponse<object>>(path, body, cancellationToken);
        }
    }
}
