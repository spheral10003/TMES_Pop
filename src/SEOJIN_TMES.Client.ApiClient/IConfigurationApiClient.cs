using SEOJIN_TMES.Client.Common.Models;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SEOJIN_TMES.Client.ApiClient
{
    /// <summary>
    /// 시스템 설정 API — <c>GET api/v1.0/sm/configurations</c>. 응답은 <c>{"data": {...}}</c> 봉투로 온다.
    /// 로그인 직후 "초기 비밀번호로 로그인했는지" 안내하려고 부른다.
    /// 응답에 초기 비밀번호가 평문으로 들어 있으므로 본문 로그 마스킹(<see cref="LoggingHandler"/>의 pswd)을 빼지 말 것.
    /// </summary>
    public interface IConfigurationApiClient
    {
        Task<ConfigurationDto> GetConfiguration(CancellationToken cancellationToken = default);
    }

    public class ConfigurationApiClient : ApiClientBase, IConfigurationApiClient
    {
        public ConfigurationApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<ConfigurationDto> GetConfiguration(CancellationToken cancellationToken = default)
        {
            var configuration = await GetDataAsync<ConfigurationDto>(ApiPrefix + "sm/configurations", cancellationToken).ConfigureAwait(false);
            return configuration ?? new ConfigurationDto();
        }
    }
}
