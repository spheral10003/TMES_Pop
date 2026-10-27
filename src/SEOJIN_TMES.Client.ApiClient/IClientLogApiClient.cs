using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Common.Models;

namespace SEOJIN_TMES.Client.ApiClient
{
    /// <summary>
    /// 클라이언트 이벤트 로그(시작 · 로그인 · 메뉴 · 설비 접속 · 레지스터 쓰기 · 치명적 오류)를 서버로 보낸다.
    /// <see cref="ServerLog"/>가 이 통로로 묶음 전송한다 — 화면이 직접 부르지 않는다.
    /// </summary>
    public interface IClientLogApiClient : IClientLogSender
    {
    }

    public class ClientLogApiClient : ApiClientBase, IClientLogApiClient
    {
        /// <summary>
        /// 서버 로그 수신 경로(<see cref="ApiClientBase.ApiPrefix"/> 뒤에 붙는 상대 경로).
        ///
        /// TODO(서버 API 미구현): 서버에 로그 API가 생기면 여기에 경로만 적으면 전송이 시작된다
        ///      (예: <c>"gm/client-logs"</c>). 비어 있는 동안에는 보내지 않고 로컬 파일에만 남는다.
        ///      요청 바디 형식(<see cref="ClientLogDto"/> 배열)과 필드명도 그때 서버 스펙에 맞출 것.
        /// </summary>
        private const string ClientLogPath = "";

        public ClientLogApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public Task SetClientLogs(IReadOnlyList<ClientLogDto> logs, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(ClientLogPath) || logs == null || logs.Count == 0)
                return Task.CompletedTask;

            return PostAsync(ApiPrefix + ClientLogPath, logs, cancellationToken);
        }
    }
}
