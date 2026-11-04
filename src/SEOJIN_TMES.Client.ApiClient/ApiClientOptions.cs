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

        /// <summary>업로드 제한 시간의 기본분(App.config의 <c>ApiUploadTimeoutSeconds</c>). 서버가 받은 파일을 저장하고 응답할 여유다.</summary>
        public TimeSpan UploadBaseTimeout { get; }

        /// <summary>업로드 제한 시간을 잡을 때 가정하는 최저 전송 속도(byte/초, App.config의 <c>ApiUploadMinKBytesPerSecond</c>).</summary>
        public long UploadMinBytesPerSecond { get; }

        public ApiClientOptions(string baseUrl, int timeoutSeconds, bool logResponseBody = false, int responseBodyMaxLength = 4000,
            int uploadTimeoutSeconds = 60, int uploadMinKBytesPerSecond = 512)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentException("baseUrl이 비어 있습니다. App.config의 ApiBaseUrl 설정을 확인하세요.", nameof(baseUrl));

            BaseUri = new Uri(baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/");
            Timeout = TimeSpan.FromSeconds(timeoutSeconds <= 0 ? 30 : timeoutSeconds);
            LogResponseBody = logResponseBody;
            ResponseBodyMaxLength = responseBodyMaxLength;
            UploadBaseTimeout = TimeSpan.FromSeconds(uploadTimeoutSeconds <= 0 ? 60 : uploadTimeoutSeconds);
            UploadMinBytesPerSecond = (uploadMinKBytesPerSecond <= 0 ? 512 : uploadMinKBytesPerSecond) * 1024L;
        }

        /// <summary>
        /// 본문 크기에 맞춘 업로드 제한 시간 = 기본분 + (크기 ÷ 최저 전송 속도).
        /// 일반 API의 <see cref="Timeout"/>(기본 30초)은 요청 <b>전체</b>에 걸리므로 200MB짜리 문서는
        /// 보내는 도중에 끊겼다. 그렇다고 무제한으로 두면 서버가 멈췄을 때 화면이 끝없이 기다린다.
        /// </summary>
        public TimeSpan UploadTimeoutFor(long contentBytes) =>
            UploadBaseTimeout + TimeSpan.FromSeconds(Math.Max(0, contentBytes) / (double)UploadMinBytesPerSecond);
    }
}
