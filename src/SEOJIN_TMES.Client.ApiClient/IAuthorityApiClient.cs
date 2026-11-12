using SEOJIN_TMES.Client.Common.Models;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SEOJIN_TMES.Client.ApiClient
{
    /// <summary>
    /// 화면별 버튼 권한 API — <c>GET api/v1.0/gm/auth/permissions?path=/POP01</c>.
    /// 로그인 토큰의 사용자 기준으로 서버가 판정해 내려준다(요청에 사용자 아이디를 싣지 않는다).
    /// </summary>
    public interface IAuthorityApiClient
    {
        /// <param name="menuCode">서버 메뉴 코드(menuCd). "POP01" 형식 — 앞의 '/'는 여기서 붙인다.</param>
        /// <returns>권한 행이 없으면 null.</returns>
        Task<ProgramPermissionDto> GetProgramPermission(string menuCode, CancellationToken cancellationToken = default);
    }

    public class AuthorityApiClient : ApiClientBase, IAuthorityApiClient
    {
        public AuthorityApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public Task<ProgramPermissionDto> GetProgramPermission(string menuCode, CancellationToken cancellationToken = default)
        {
            // 서버는 path를 "/메뉴코드" 형식으로 받는다. 이미 '/'가 붙어 와도 두 번 붙이지 않는다.
            // path는 쿼리 문자열이다(Swagger: in=query) — '/'는 BuildQuery가 이스케이프한다.
            // 응답은 APIResponse«AuthConfigurationFrameDto» 봉투라 GetDataAsync로 벗긴다.
            var path = "/" + (menuCode ?? string.Empty).Trim().TrimStart('/');
            var query = BuildQuery(("path", path));
            return GetDataAsync<ProgramPermissionDto>(ApiPrefix + "gm/auth/permissions" + query, cancellationToken);
        }
    }
}
