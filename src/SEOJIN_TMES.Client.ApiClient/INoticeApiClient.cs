using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SEOJIN_TMES.Client.Common.Models;

namespace SEOJIN_TMES.Client.ApiClient
{
    /// <summary>
    /// 공지사항 API — <c>GET api/v1.0/gm/notices[?ntcType=]</c>. 응답은 <c>{"data":[...]}</c> 봉투로 온다.
    /// 홈 화면은 유형 조건 없이 <b>한 번에 전체</b>를 받아 유형별로 나눠 보여 준다
    /// (유형마다 따로 부르면 홈을 열 때마다 호출이 네 번씩 나간다).
    /// </summary>
    public interface INoticeApiClient
    {
        /// <param name="ntcType">공지 유형(NTC01~NTC07). 비우면 전체.</param>
        Task<List<NoticeDto>> GetNotices(string ntcType = null, CancellationToken cancellationToken = default);
    }

    public class NoticeApiClient : ApiClientBase, INoticeApiClient
    {
        public NoticeApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<List<NoticeDto>> GetNotices(string ntcType = null, CancellationToken cancellationToken = default)
        {
            var query = BuildQuery(("ntcType", ntcType));
            var notices = await GetDataAsync<List<NoticeDto>>(ApiPrefix + "gm/notices" + query, cancellationToken).ConfigureAwait(false);
            return notices ?? new List<NoticeDto>();
        }
    }
}
