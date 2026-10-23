using SEOJIN_TMES.Client.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SEOJIN_TMES.Client.ApiClient
{
    // <summary>
    /// 메뉴 목록 API — <c>GET api/v1.0/sm/menus?menuGroupCd=POP[&amp;menuNm=][&amp;useYn=]</c>.
    /// MainForm 좌측 메뉴에서 어떤 항목을 보여 줄지 서버 설정(useYn)으로 정한다.
    /// </summary>
    public interface IMenuApiClient
    {
        /// <param name="menuName">메뉴 이름(menuNm). 비우면 전체.</param>
        /// <param name="useYn">사용 여부(Y/N). 비우면 전체.</param>
        Task<List<MenuDto>> GetMenusAsync(string menuName = null, string useYn = null, CancellationToken cancellationToken = default);
    }

    public class MenuApiClient : ApiClientBase, IMenuApiClient
    {
        /// <summary>이 클라이언트는 POP 메뉴 그룹만 쓴다 — 다른 그룹(웹 등)의 메뉴가 섞여 오지 않도록 고정한다.</summary>
        private const string PopMenuGroupCode = "POP";

        public MenuApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<List<MenuDto>> GetMenusAsync(string menuName = null, string useYn = null, CancellationToken cancellationToken = default)
        {
            var query = BuildQuery(("menuGroupCd", PopMenuGroupCode), ("menuNm", menuName), ("useYn", useYn));
            var menus = await GetAsync <ApiDataResponse<List<MenuDto>>>($"{ApiPrefix}sm/menus{query}", cancellationToken).ConfigureAwait(false);
            return menus?.Data ?? new List<MenuDto>();
        }
    }
}
