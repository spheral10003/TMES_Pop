using SEOJIN_TMES.Client.ApiClient;
using SEOJIN_TMES.Client.Common.Configuration;
using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Common.Storage;
using SEOJIN_TMES.Client.Composition;
using System.Globalization;
using System.Threading;

namespace SEOJIN_TMES.Client.Core
{
    public class Setting
    {

        /// <summary>
        /// 언어 콤보에 표시할 목록(표시 문구 ↔ 문화권 코드 ↔ 국기 아이콘).
        /// 국기는 Images\flag\{두 글자 코드}.png 이며 문화권 코드 앞 두 글자로 찾는다.
        ///
        /// 언어를 추가할 때는 <b>세 곳을 함께</b> 갖춰야 한다 —
        /// 여기 한 줄 + Common.Languages\Language.{두 글자 코드}.resx + Images\flag\{두 글자 코드}.png.
        /// 번역 리소스 없이 항목만 넣으면 골라도 중립 리소스(한국어)로 되돌아가 고장처럼 보인다.
        /// </summary>
        //public readonly LanguageOption[] Languages =
        //{
        //    new LanguageOption("한국어", "ko-KR"),
        //    new LanguageOption("English", "en-US"),
        //    new LanguageOption("Việt Nam", "vi-VN")
        //};

        private DocumentShareClient _docShare;
        private AppSettings _appSet;
        private ApiClientFactory _apiClients;
        private ApiClientOptions _apiOptions;

        private UserSession _currentUser;

        public AppSettings AppSet { get => _appSet; set => _appSet = value; }
        public ApiClientFactory ApiClients { get => _apiClients; set => _apiClients = value; }
        public ApiClientOptions ApiOptions { get => _apiOptions; set => _apiOptions = value; }
        public DocumentShareClient DocShare { get => _docShare;}
        public UserSession CurrentUser { get => _currentUser; set => _currentUser = value; }

        public Setting()
        {
            Init();
        }
        public void Init()
        {
            _appSet = AppSettingsLoader.Load();

            _apiOptions = new ApiClientOptions(_appSet.ApiBaseUrl, _appSet.ApiTimeoutSeconds,
                _appSet.LogApiResponseBody, _appSet.LogApiResponseBodyMaxLength);

            _apiClients = new ApiClientFactory(_apiOptions);

            _docShare = new DocumentShareClient(_appSet.DocumentShare);

            _currentUser = new UserSession();

            ApplyCulture();
        }

        private void ApplyCulture()
        {
            if (string.IsNullOrWhiteSpace(_appSet.DefaultCulture))
                return;

            try
            {
                var culture = CultureInfo.GetCultureInfo(_appSet.DefaultCulture);
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
            }
            catch (CultureNotFoundException)
            {
                // 알 수 없는 culture 설정 값이면 OS 기본 culture를 그대로 사용한다.
            }
        }

    }
}
