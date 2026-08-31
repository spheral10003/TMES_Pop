using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SEOJIN_MEIS.Client.Common.Models;

namespace SEOJIN_MEIS.Client.ApiClient
{
    /// <summary>작업지시 조회 관련 API. 엔드포인트 경로는 실제 서버 스펙 확정 후 조정 필요(TODO).</summary>
    public interface IWorkOrderApiClient
    {
        Task<List<WorkOrderDto>> GetWorkOrdersAsync(string equipmentId, CancellationToken cancellationToken = default);

        /// <summary>
        /// POP 진입 화면의 작업지시 조회. 작업지시번호 / 생산품목 시리얼번호 중 입력된 조건만 서버로 전달한다.
        /// </summary>
        Task<List<WorkOrderEntryDto>> SearchWorkOrdersAsync(string workOrderNo, string serialNo, CancellationToken cancellationToken = default);

        /// <summary>선택한 작업지시의 라우팅(공정) 목록. 작업 진입 확인 팝업에서 사용한다.</summary>
        Task<List<RoutingProcessDto>> GetRoutingProcessesAsync(string workOrderNo, string serialNo, CancellationToken cancellationToken = default);
    }

    public class WorkOrderApiClient : ApiClientBase, IWorkOrderApiClient
    {
        public WorkOrderApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public Task<List<WorkOrderDto>> GetWorkOrdersAsync(string equipmentId, CancellationToken cancellationToken = default)
        {
            var url = string.IsNullOrEmpty(equipmentId)
                ? ApiPrefix + "workorders"
                : $"{ApiPrefix}workorders?equipmentId={equipmentId}";
            return GetAsync<List<WorkOrderDto>>(url, cancellationToken);
        }

        public Task<List<WorkOrderEntryDto>> SearchWorkOrdersAsync(string workOrderNo, string serialNo, CancellationToken cancellationToken = default)
        {
            var query = BuildQuery(("workOrderNo", workOrderNo), ("serialNo", serialNo));
            return GetAsync<List<WorkOrderEntryDto>>(ApiPrefix + "workorders/search" + query, cancellationToken);
        }

        public Task<List<RoutingProcessDto>> GetRoutingProcessesAsync(string workOrderNo, string serialNo, CancellationToken cancellationToken = default)
        {
            var query = BuildQuery(("serialNo", serialNo));
            return GetAsync<List<RoutingProcessDto>>($"{ApiPrefix}workorders/{WebUtility.UrlEncode(workOrderNo)}/routing{query}", cancellationToken);
        }
    }
}
