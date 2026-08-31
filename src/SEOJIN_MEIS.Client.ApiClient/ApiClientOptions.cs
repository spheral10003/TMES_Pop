using System;

namespace SEOJIN_MEIS.Client.ApiClient
{
    public class ApiClientOptions
    {
        public Uri BaseUri { get; }
        public TimeSpan Timeout { get; }

        public ApiClientOptions(string baseUrl, int timeoutSeconds)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentException("baseUrl이 비어 있습니다. App.config의 ApiBaseUrl 설정을 확인하세요.", nameof(baseUrl));

            BaseUri = new Uri(baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/");
            Timeout = TimeSpan.FromSeconds(timeoutSeconds <= 0 ? 30 : timeoutSeconds);
        }
    }
}
