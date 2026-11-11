using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SEOJIN_TMES.Client.Common.Models;

namespace SEOJIN_TMES.Client.ApiClient
{
    /// <summary>
    /// 공지사항 API — <c>GET api/v1.0/gm/notices[?ntcType=]</c>.
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
        /// <summary>
        /// 응답이 페이지 객체로 올 때 목록이 들어 있을 수 있는 키(가정값 — 서버 스펙 확인 후 정리할 것).
        /// 목록이 배열로 바로 오면 쓰이지 않는다.
        /// </summary>
        private static readonly string[] PagedListKeys = { "content", "list", "items", "records", "rows" };

        public NoticeApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<List<NoticeDto>> GetNotices(string ntcType = null, CancellationToken cancellationToken = default)
        {
            var query = BuildQuery(("ntcType", ntcType));

            // 응답 형태(배열 / 페이지 객체)가 확정되지 않아 JToken으로 받아 목록 위치를 찾는다.
            // 형식이 맞지 않으면 GetDataAsync 안에서 ApiException(FromInvalidResponse)으로 올라간다.
            var token = await GetDataAsync<JToken>(ApiPrefix + "gm/notices" + query, cancellationToken).ConfigureAwait(false);

            return ListOf(token).OfType<JObject>().Select(NoticeDto.From).ToList();
        }

        private static IEnumerable<JToken> ListOf(JToken token)
        {
            if (token is JArray array)
                return array;

            if (token is JObject obj)
            {
                foreach (var key in PagedListKeys)
                {
                    if (obj.TryGetValue(key, System.StringComparison.OrdinalIgnoreCase, out var inner) && inner is JArray list)
                        return list;
                }
            }

            return Enumerable.Empty<JToken>();
        }
    }
}
