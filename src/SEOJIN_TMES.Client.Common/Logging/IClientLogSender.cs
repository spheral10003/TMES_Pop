using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SEOJIN_TMES.Client.Common.Models;

namespace SEOJIN_TMES.Client.Common.Logging
{
    /// <summary>
    /// 로그를 서버로 보내는 통로. 구현은 ApiClient 프로젝트(<c>ClientLogApiClient</c>)에 있다 —
    /// Common은 ApiClient를 모르므로(프로젝트 경계) 계약만 여기 둔다.
    /// </summary>
    public interface IClientLogSender
    {
        /// <summary>
        /// 로그 묶음을 서버로 보낸다. 실패는 예외로 올린다(<see cref="ServerLog"/>가 받아서 로컬에만 남긴다).
        /// </summary>
        Task SetClientLogs(IReadOnlyList<ClientLogDto> logs, CancellationToken cancellationToken = default);
    }
}
