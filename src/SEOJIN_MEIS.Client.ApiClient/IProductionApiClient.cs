using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SEOJIN_MEIS.Client.Common.Models;

namespace SEOJIN_MEIS.Client.ApiClient
{
    /// <summary>생산 실적 등록/조회 API. 수량 계산·집계 로직은 전부 서버에서 처리하고 클라이언트는 입력값만 전달한다.</summary>
    public interface IProductionApiClient
    {
        Task RegisterResultAsync(ProductionResultRequest request, CancellationToken cancellationToken = default);
        Task<List<ProductionStatusDto>> GetProductionStatusAsync(System.DateTime standardDate, CancellationToken cancellationToken = default);
        Task<List<WorkIntegratedStatusDto>> GetWorkIntegratedStatusAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// &lt;작업통합 현황 조회&gt; 화면의 [작업 통합 개요] 트리.
        /// 생산품목 시리얼번호 하나가 거쳐 온 공정/자재/스팩을 부모-자식 관계를 가진 평평한 목록으로 받는다.
        /// </summary>
        Task<List<WorkIntegratedNodeDto>> GetWorkIntegratedOverviewAsync(string serialNo, CancellationToken cancellationToken = default);
    }

    public class ProductionApiClient : ApiClientBase, IProductionApiClient
    {
        public ProductionApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public Task RegisterResultAsync(ProductionResultRequest request, CancellationToken cancellationToken = default)
        {
            return PostAsync(ApiPrefix + "production/results", request, cancellationToken);
        }

        public Task<List<ProductionStatusDto>> GetProductionStatusAsync(System.DateTime standardDate, CancellationToken cancellationToken = default)
        {
            return GetAsync<List<ProductionStatusDto>>($"{ApiPrefix}production/status?date={standardDate:yyyy-MM-dd}", cancellationToken);
        }

        public Task<List<WorkIntegratedStatusDto>> GetWorkIntegratedStatusAsync(CancellationToken cancellationToken = default)
        {
            return GetAsync<List<WorkIntegratedStatusDto>>(ApiPrefix + "production/work-integrated-status", cancellationToken);
        }

        public Task<List<WorkIntegratedNodeDto>> GetWorkIntegratedOverviewAsync(string serialNo, CancellationToken cancellationToken = default)
        {
            var query = BuildQuery(("serialNo", serialNo));
            return GetAsync<List<WorkIntegratedNodeDto>>(ApiPrefix + "production/work-integrated-overview" + query, cancellationToken);
        }
    }
}
