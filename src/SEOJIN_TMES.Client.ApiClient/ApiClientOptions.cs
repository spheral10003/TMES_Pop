using System;

namespace SEOJIN_TMES.Client.ApiClient
{
    public class ApiClientOptions
    {
        public Uri BaseUri { get; }
        public TimeSpan Timeout { get; }

        /// <summary>응답 본문을 로그에 남길지(App.config의 <c>LogApiResponseBody</c>).</summary>
        public bool LogResponseBody { get; }

        /// <summary>로그에 남길 응답 본문의 최대 길이(자).</summary>
        public int ResponseBodyMaxLength { get; }

        public ApiClientOptions(string baseUrl, int timeoutSeconds, bool logResponseBody = false, int responseBodyMaxLength = 4000)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentException("baseUrl이 비어 있습니다. App.config의 ApiBaseUrl 설정을 확인하세요.", nameof(baseUrl));

            BaseUri = new Uri(baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/");
            Timeout = TimeSpan.FromSeconds(timeoutSeconds <= 0 ? 30 : timeoutSeconds);
            LogResponseBody = logResponseBody;
            ResponseBodyMaxLength = responseBodyMaxLength;
        }
    }
}
