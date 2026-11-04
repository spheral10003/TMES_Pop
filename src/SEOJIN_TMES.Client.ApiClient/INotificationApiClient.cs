using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SEOJIN_TMES.Client.Common.Models;

namespace SEOJIN_TMES.Client.ApiClient
{
    /// <summary>
    /// 알림(상단 종 아이콘) 관련 API.
    /// 엔드포인트 경로/응답 형태는 <b>가정값</b>이며 실제 서버 스펙 확정 후 조정 필요(TODO).
    /// </summary>
    public interface INotificationApiClient
    {
        /// <summary>
        /// 내 알림 요약(안 읽은 개수 + 안 읽은 알림의 참조키 목록).
        /// 상단 배지 숫자를 채우기 위해 1분 간격으로 폴링한다.
        /// 화면을 막지 않아야 하므로 실패는 호출부(MainForm)가 로그만 남기고 넘어간다.
        /// </summary>
        Task<MyAlarmSummaryDto> GetMyAlarms(CancellationToken cancellationToken = default);

        /// <summary>알림 목록. 값이 있는 조건만 쿼리에 붙는다(읽음여부 전체면 아예 보내지 않는다).</summary>
        Task<List<NotificationDto>> GetNotificationsItem(string userName, string subject, NotificationReadState readState, CancellationToken cancellationToken = default);

        /// <summary>
        /// 선택한 알림들을 읽음 처리한다(PATCH). 조회 응답에서 받은 <c>alarmSendRef</c>(<c>sysObjId</c>)를 넘기며,
        /// 바디는 래퍼 객체 없이 문자열 배열 <c>["ref1","ref2",...]</c> 그대로다.
        /// </summary>
        Task MarkAsRead(IEnumerable<string> alarmSendRefs, CancellationToken cancellationToken = default);
    }

    public class NotificationApiClient : ApiClientBase, INotificationApiClient
    {
        public NotificationApiClient(System.Net.Http.HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<MyAlarmSummaryDto> GetMyAlarms(CancellationToken cancellationToken = default)
        {
            // 이 엔드포인트는 Swagger에 알맹이 타입으로 선언돼 있으나 서버가 봉투로 내려줄 수 있다.
            // GetDataAsync가 두 형태를 모두 받아 주므로 어느 쪽이든 값이 채워진다.
            var summary = await GetDataAsync<MyAlarmSummaryDto>(ApiPrefix + "gm/alarm-consumer/my-alarms", cancellationToken)
                              .ConfigureAwait(false);

            return summary ?? new MyAlarmSummaryDto();
        }

        public async Task<List<NotificationDto>> GetNotificationsItem(string userName, string subject, NotificationReadState readState, CancellationToken cancellationToken = default)
        {
            var query = BuildQuery(
                ("orgNo", userName),
                ("subject", subject),
                ("isConfirmed", ReadStateValue(readState)));

            var list = await GetDataAsync<List<NotificationDto>>(ApiPrefix + "gm/alarms" + query, cancellationToken)
                           .ConfigureAwait(false);
            return list ?? new List<NotificationDto>();
        }

        public async Task<List<NotificationDto>> GetNotifications(IEnumerable<string> alarmSeqIds, CancellationToken cancellationToken = default)
        {
            var list = await GetDataAsync<List<NotificationDto>>($"{ApiPrefix}gm/alarms/{alarmSeqIds}", cancellationToken).ConfigureAwait(false);
            return list ?? new List<NotificationDto>();
        }

        public Task MarkAsRead(IEnumerable<string> alarmSendRefs, CancellationToken cancellationToken = default)
        {
            // 서버가 객체가 아닌 배열을 바로 받으므로 래퍼 없이 문자열 배열을 그대로 직렬화한다.
            var body = new List<string>(alarmSendRefs).ToArray();

            // 요약 조회(GET my-alarms)와 같은 경로를 PATCH로 부르면 읽음 처리가 된다.
            return PatchAsync(ApiPrefix + "gm/alarm-consumer/my-alarms", body, cancellationToken);
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
