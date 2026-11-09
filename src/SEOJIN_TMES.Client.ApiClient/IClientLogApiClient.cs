using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Common.Models;

namespace SEOJIN_TMES.Client.ApiClient
{
    /// <summary>
    /// 클라이언트 이벤트 로그(시작 · 로그인 · 메뉴 · 설비 접속 · 레지스터 쓰기 · 치명적 오류)를 서버로 보낸다.
    /// <see cref="ServerLog"/>가 이 통로로 한 건씩 보낸다 — 화면이 직접 부르지 않는다.
    /// </summary>
    public interface IClientLogApiClient : IClientLogSender
    {
    }

    public class ClientLogApiClient : ApiClientBase, IClientLogApiClient
    {
        public ClientLogApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        /// <summary>POST api/v1.0/pop/event-logs — 바디는 로그 1건(배열이 아니다).</summary>
        public Task SetEventLog(ClientLogDto log, CancellationToken cancellationToken = default)
        {
            if (log == null)
                return Task.CompletedTask;

            return PostAsync(ApiPrefix + "pop/event-logs", log, cancellationToken);
        }
    }
}
