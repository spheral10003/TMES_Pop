using System.Net.Http;

namespace SEOJIN_TMES.Client.ApiClient
{
    /// <summary>
    /// 인증 없는 HttpClient(로그인/토큰갱신 전용)와 인증 포함 HttpClient(그 외 전체 API)를 구성하는 조립 지점.
    /// Program.cs(컴포지션 루트)에서 한 번 생성해 각 API 클라이언트에 주입한다.
    /// </summary>
    public class ApiClientFactory
    {
        public ITokenProvider TokenProvider { get; }
        public IAuthApiClient AuthApiClient { get; }
        public IWorkOrderApiClient WorkOrderApiClient { get; }
        public IProductionApiClient ProductionApiClient { get; }
        public IEquipmentStatusApiClient EquipmentStatusApiClient { get; }
        public IUserProfileApiClient UserProfileApiClient { get; }
        public INotificationApiClient NotificationApiClient { get; }
        public IPopApiClient PopApiClient { get; }
        public IWorkStatusApiClient WorkStatusApiClient { get; }
        public IFileApiClient FileApiClient { get; }
        public IMenuApiClient MenuApiClient { get; }
        public IClientLogApiClient ClientLogApiClient { get; }
        public ApiClientFactory(ApiClientOptions options)
        {
            TokenProvider = new TokenProvider();

            var anonymousClient = new HttpClient(NewLoggingHandler(options)) { BaseAddress = options.BaseUri, Timeout = options.Timeout };
            AuthApiClient = new AuthApiClient(anonymousClient);

            var authenticatedHandler = new AuthTokenHandler(TokenProvider, AuthApiClient, NewLoggingHandler(options));
            var authenticatedClient = new HttpClient(authenticatedHandler) { BaseAddress = options.BaseUri, Timeout = options.Timeout };

            // 업로드 전용. HttpClient.Timeout은 첫 요청 뒤에 바꿀 수 없고 요청 전체(전송 포함)에 걸리므로,
            // 무제한으로 두고 제한 시간은 FileApiClient가 본문 크기에 맞춰 호출마다 건다.
            // 핸들러(토큰 · 로그)는 같은 것을 쓴다 — 이쪽이 정리되며 공유 핸들러를 닫지 않게 disposeHandler: false.
            var uploadClient = new HttpClient(authenticatedHandler, disposeHandler: false)
            {
                BaseAddress = options.BaseUri,
                Timeout = System.Threading.Timeout.InfiniteTimeSpan
            };

            WorkOrderApiClient = new WorkOrderApiClient(authenticatedClient);
            ProductionApiClient = new ProductionApiClient(authenticatedClient);
            EquipmentStatusApiClient = new EquipmentStatusApiClient(authenticatedClient);
            UserProfileApiClient = new UserProfileApiClient(authenticatedClient);
            NotificationApiClient = new NotificationApiClient(authenticatedClient);
            PopApiClient = new PopApiClient(authenticatedClient);
            WorkStatusApiClient = new WorkStatusApiClient(authenticatedClient);
            FileApiClient = new FileApiClient(authenticatedClient, uploadClient, options);
            MenuApiClient = new MenuApiClient(authenticatedClient);
            ClientLogApiClient = new ClientLogApiClient(authenticatedClient);
        }

        /// <summary>
        /// 인증 없는 쪽과 인증 포함 쪽이 <b>같은 로그 설정</b>을 쓰도록 한 곳에서 만든다.
        /// 두 곳에 따로 적어 두면 한쪽만 고쳐져 로그인 응답만 안 찍히는 식으로 어긋난다.
        /// </summary>
        private static LoggingHandler NewLoggingHandler(ApiClientOptions options) =>
            new LoggingHandler(new HttpClientHandler(), options.LogResponseBody, options.ResponseBodyMaxLength);
    }
}
