using SEOJIN_TMES.Client.Common.Models;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SEOJIN_TMES.Client.ApiClient
{
    /// <summary>
    /// 화면별 버튼 권한 API — <c>GET api/v1.0/gm/auth/permissions?path=/AttachmentSearch</c>.
    /// 로그인 토큰의 사용자 기준으로 서버가 판정해 내려준다(요청에 사용자 아이디를 싣지 않는다).
    /// </summary>
    public interface IAuthorityApiClient
    {
        /// <param name="menuPath">메뉴 경로. "/AttachmentSearch", "/WorkStatusSearch", "/WorkIntegratedStatus", "/POP" 형식.</param>
        /// <returns>권한 행이 없으면 null.</returns>
        Task<ProgramPermissionDto> GetProgramPermission(string menuPath, CancellationToken cancellationToken = default);
    }

    public class AuthorityApiClient : ApiClientBase, IAuthorityApiClient
    {
        public AuthorityApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public Task<ProgramPermissionDto> GetProgramPermission(string menuPath, CancellationToken cancellationToken = default)
        {
            // path는 쿼리 문자열이다(Swagger: in=query). '/'가 들어가므로 BuildQuery가 이스케이프한다.
            // 응답은 APIResponse«AuthConfigurationFrameDto» 봉투라 GetDataAsync로 벗긴다.
            var query = BuildQuery(("path", menuPath));
            return GetDataAsync<ProgramPermissionDto>(ApiPrefix + "gm/auth/permissions" + query, cancellationToken);
        }
    }
}
