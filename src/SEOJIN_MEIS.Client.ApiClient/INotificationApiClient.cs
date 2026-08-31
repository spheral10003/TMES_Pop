using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SEOJIN_MEIS.Client.Common.Models;

namespace SEOJIN_MEIS.Client.ApiClient
{
    /// <summary>
    /// 알림(상단 종 아이콘) 관련 API.
    /// 엔드포인트 경로/응답 형태는 <b>가정값</b>이며 실제 서버 스펙 확정 후 조정 필요(TODO).
    /// </summary>
    public interface INotificationApiClient
    {
        /// <summary>
        /// 안 읽은 알림 개수. 상단 배지 숫자를 채우기 위해 1분 간격으로 폴링한다.
        /// 화면을 막지 않아야 하므로 실패는 호출부(MainForm)가 로그만 남기고 넘어간다.
        /// </summary>
        Task<int> GetUnreadCountAsync(CancellationToken cancellationToken = default);

        /// <summary>알림 목록. 값이 있는 조건만 쿼리에 붙는다(읽음여부 전체면 아예 보내지 않는다).</summary>
        Task<List<NotificationDto>> GetNotificationsAsync(string userName, string subject, NotificationReadState readState, CancellationToken cancellationToken = default);

        /// <summary>선택한 알림들을 읽음 처리한다.</summary>
        Task MarkAsReadAsync(IEnumerable<long> notificationIds, CancellationToken cancellationToken = default);
    }

    public class NotificationApiClient : ApiClientBase, INotificationApiClient
    {
        public NotificationApiClient(System.Net.Http.HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<int> GetUnreadCountAsync(CancellationToken cancellationToken = default)
        {
            var response = await GetAsync<ApiDataResponse<int>>(ApiPrefix + "notifications/unread-count", cancellationToken)
                               .ConfigureAwait(false);
            return response?.Data ?? 0;
        }

        public async Task<List<NotificationDto>> GetNotificationsAsync(string userName, string subject, NotificationReadState readState, CancellationToken cancellationToken = default)
        {
            var query = BuildQuery(
                ("userName", userName),
                ("subject", subject),
                ("isRead", ReadStateValue(readState)));

            var response = await GetAsync<ApiDataResponse<List<NotificationDto>>>(ApiPrefix + "notifications" + query, cancellationToken)
                               .ConfigureAwait(false);
            return response?.Data ?? new List<NotificationDto>();
        }

        public Task MarkAsReadAsync(IEnumerable<long> notificationIds, CancellationToken cancellationToken = default)
        {
            var request = new NotificationReadRequest { NotificationIds = new List<long>(notificationIds).ToArray() };
            return PostAsync(ApiPrefix + "notifications/read", request, cancellationToken);
        }

        /// <summary>읽음여부 조건 → 쿼리 값. 전체면 null이라 쿼리에서 빠진다.</summary>
        private static string ReadStateValue(NotificationReadState readState)
        {
            if (readState == NotificationReadState.Read) return "true";
            if (readState == NotificationReadState.Unread) return "false";
            return null;
        }
    }
}
