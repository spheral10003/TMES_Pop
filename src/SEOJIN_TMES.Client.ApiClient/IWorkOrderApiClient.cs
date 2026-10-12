using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SEOJIN_TMES.Client.Common.Models;

namespace SEOJIN_TMES.Client.ApiClient
{
    /// <summary>작업지시 조회 관련 API. 엔드포인트 경로는 실제 서버 스펙 확정 후 조정 필요(TODO).</summary>
    public interface IWorkOrderApiClient
    {
        Task<List<WorkOrderDto>> GetWorkOrdersAsync(string equipmentId, CancellationToken cancellationToken = default);

        /// <summary>
        /// POP 진입 화면의 작업지시 조회. 작업지시번호 / 생산품목 시리얼번호 중 입력된 조건만 서버로 전달한다.
        /// </summary>
        Task<List<WorkOrderEntryDto>> SearchWorkOrdersAsync(string corpCode, string workOrderNo, string serialNo, CancellationToken cancellationToken = default);

        Task<List<WorkOrderEntryDto>> SearchWorkOrdersAsync(string corpCode, CancellationToken cancellationToken = default);

        /// <summary>선택한 작업지시의 라우팅(공정) 목록. 작업 진입 확인 팝업에서 사용한다.</summary>
        Task<List<RoutingProcessDto>> GetRoutingProcessesAsync(string corpCode, string workStationRef, string serialNo , CancellationToken cancellationToken = default);
    }

    public class WorkOrderApiClient : ApiClientBase, IWorkOrderApiClient
    {
        private const string WorkOrderPrefix = ApiPrefix + "pop/";

        public WorkOrderApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public Task<List<WorkOrderDto>> GetWorkOrdersAsync(string equipmentId, CancellationToken cancellationToken = default)
        {
            var url = string.IsNullOrEmpty(equipmentId)
                ? WorkOrderPrefix + "workorders"
                : $"{WorkOrderPrefix}workorders?equipmentId={equipmentId}";
            return GetAsync<List<WorkOrderDto>>(url, cancellationToken);
        }

        public async Task<List<WorkOrderEntryDto>> SearchWorkOrdersAsync(string corpCode, string workOrderNo, string serialNo, CancellationToken cancellationToken = default)
        {
            var query = BuildQuery(("corpCd", corpCode), ("woNo", workOrderNo), ("sn", serialNo));
            var response = await GetAsync<ApiDataResponse<List<WorkOrderEntryDto>>>(ApiPrefix + "md/work-orders/overview" + query, cancellationToken);
            return response?.Data ?? new List<WorkOrderEntryDto>();
            //return GetAsync<List<WorkOrderEntryDto>>(WorkOrderPrefix + "work-orders/search" + query, cancellationToken);
        }

        public async Task<List<WorkOrderEntryDto>> SearchWorkOrdersAsync(string corpCode, CancellationToken cancellationToken = default)
        {
            var query = BuildQuery(("corpCd", corpCode));
            var response = await GetAsync<ApiDataResponse<List<WorkOrderEntryDto>>>($"{ApiPrefix}md/work-orders/overview{query}", cancellationToken);
            return response?.Data ?? new List<WorkOrderEntryDto>();

        }


        public async Task<List<RoutingProcessDto>> GetRoutingProcessesAsync(string corpCode, string workStationRef, string serialNo, CancellationToken cancellationToken = default)
        {
            var query = BuildQuery(("corpCd", corpCode), ("wkstnRef", workStationRef));
            var response = await GetAsync<ApiDataResponse<List<RoutingProcessDto>>>($"{WorkOrderPrefix}{serialNo}/processes{query}", cancellationToken);
            return response?.Data ?? new List<RoutingProcessDto>();
        }
    }
}
