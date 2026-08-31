using System.Net.Http;

namespace SEOJIN_MEIS.Client.ApiClient
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

        public ApiClientFactory(ApiClientOptions options)
        {
            TokenProvider = new TokenProvider();

            var anonymousClient = new HttpClient(new LoggingHandler(new HttpClientHandler())) { BaseAddress = options.BaseUri, Timeout = options.Timeout };
            AuthApiClient = new AuthApiClient(anonymousClient);

            var authenticatedHandler = new AuthTokenHandler(TokenProvider, AuthApiClient, new LoggingHandler(new HttpClientHandler()));
            var authenticatedClient = new HttpClient(authenticatedHandler) { BaseAddress = options.BaseUri, Timeout = options.Timeout };

            WorkOrderApiClient = new WorkOrderApiClient(authenticatedClient);
            ProductionApiClient = new ProductionApiClient(authenticatedClient);
            EquipmentStatusApiClient = new EquipmentStatusApiClient(authenticatedClient);
            UserProfileApiClient = new UserProfileApiClient(authenticatedClient);
            NotificationApiClient = new NotificationApiClient(authenticatedClient);
            PopApiClient = new PopApiClient(authenticatedClient);
            WorkStatusApiClient = new WorkStatusApiClient(authenticatedClient);
        }
    }
}
