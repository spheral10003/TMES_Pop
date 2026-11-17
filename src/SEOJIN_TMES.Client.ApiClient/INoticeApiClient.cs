using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SEOJIN_TMES.Client.Common.Models;

namespace SEOJIN_TMES.Client.ApiClient
{
    /// <summary>
    /// 공지사항 API. 응답은 <c>{"data":...}</c> 봉투로 온다.
    /// <list type="bullet">
    /// <item>목록 — <c>GET api/v1.0/gm/notices?exceptBodyYn=Y[&amp;ntcType=]</c>. 본문(<c>body</c>) 없이 받는다.</item>
    /// <item>한 건 — <c>GET api/v1.0/gm/notices/{sysSeqId}?userId=</c>. 공지를 열 때 본문을 이것으로 받는다.</item>
    /// </list>
    /// 홈 화면은 유형 조건 없이 <b>한 번에 전체</b>를 받아 유형별로 나눠 보여 준다
    /// (유형마다 따로 부르면 홈을 열 때마다 호출이 네 번씩 나간다).
    /// </summary>
    public interface INoticeApiClient
    {
        /// <summary>공지 목록(본문 제외). 홈 상자와 공지 폴링이 쓴다.</summary>
        /// <param name="ntcType">공지 유형(NTC01~NTC07). 비우면 전체.</param>
        Task<List<NoticeDto>> GetNotices(string ntcType = null, CancellationToken cancellationToken = default);

        /// <summary>공지 한 건(본문 포함). 받은 것이 없으면 null.</summary>
        /// <param name="sysSeqId">공지 순번 — 목록의 <see cref="NoticeDto.SysSeqId"/>.</param>
        /// <param name="userId">조회한 사용자 아이디 — 서버 필수 조건(조회수 집계용으로 보인다).</param>
        Task<NoticeDto> GetNotice(string sysSeqId, string userId, CancellationToken cancellationToken = default);
    }

    public class NoticeApiClient : ApiClientBase, INoticeApiClient
    {
        /// <summary>
        /// 목록 조회 때 본문을 빼 달라는 조건 값.
        /// 공지 본문은 HTML이라 건마다 크고, 목록(홈 상자 · 1분 주기 폴링)은 제목 · 일시만 쓰므로 본문을 받을 이유가 없다.
        /// </summary>
        private const string ExceptBody = "Y";

        public NoticeApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<List<NoticeDto>> GetNotices(string ntcType = null, CancellationToken cancellationToken = default)
        {
            var query = BuildQuery(("exceptBodyYn", ExceptBody), ("ntcType", ntcType));
            var notices = await GetDataAsync<List<NoticeDto>>(ApiPrefix + "gm/notices" + query, cancellationToken).ConfigureAwait(false);
            return notices ?? new List<NoticeDto>();
        }

        public Task<NoticeDto> GetNotice(string sysSeqId, string userId, CancellationToken cancellationToken = default)
        {
            var path = ApiPrefix + "gm/notices/" + Uri.EscapeDataString(sysSeqId ?? string.Empty)
                     + BuildQuery(("userId", userId));
            return GetDataAsync<NoticeDto>(path, cancellationToken);
        }
    }
}
