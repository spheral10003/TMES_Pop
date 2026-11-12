using SEOJIN_TMES.Client.Common.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;

namespace SEOJIN_TMES.Client.ApiClient
{
    /// <summary>
    /// &lt;작업 조회 현황&gt; 화면 전용 API — 조건 조회(작업 지시 정보) + 선택 지시의 작업 진행 현황 + 품목 검색.
    ///
    /// TODO: 엔드포인트 경로는 모두 <b>가정값</b>이다. 실제 서버 스펙이 확정되면 경로/파라미터명을 맞출 것.
    /// </summary>
    public interface IWorkStatusApiClient
    {
        /// <summary>조회 조건에 맞는 작업 지시 정보 목록.</summary>
        Task<List<WorkOrderEntryDto>> SearchWorkOrdersAsync(WorkOrderQuery query, CancellationToken cancellationToken = default);

        /// <summary>선택한 작업지시의 공정/스팩별 진행 현황.</summary>
        Task<List<WorkProgressDto>> GetWorkProgressAsync(string corpCode, string workStationRef, string serialNo, CancellationToken cancellationToken = default);

        /// <summary>&lt;생산품목 선택&gt; 팝업의 품목 검색(품목코드/품목명 부분 일치).</summary>
        Task<List<ItemResponse>> SearchItemsAsync(ItemQuery keyword, CancellationToken cancellationToken = default);
    }

    public class WorkStatusApiClient : ApiClientBase, IWorkStatusApiClient
    {
        public WorkStatusApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<List<WorkOrderEntryDto>> SearchWorkOrdersAsync(WorkOrderQuery query, CancellationToken cancellationToken = default)
        {
            var condition = query ?? new WorkOrderQuery();
            var url = ApiPrefix + "md/work-orders/overview"; 
            var search = BuildQuery(
                ("corpCd", condition.CorpCode),
                ("startDt", condition.OrderDateFrom?.ToString("yyyy-MM-dd")),
                ("endDt", condition.OrderDateTo?.ToString("yyyy-MM-dd")),
                ("itemCd", condition.ItemCode),
                ("workPlanNo", condition.ItemCode),
                ("woNo", condition.WorkOrderNo),
                ("sn", condition.SerialNo),
                ("statusList", condition.StatusName),
                ("fcid", condition.FCID)
                );
            
            var response = await GetAsync<ApiDataResponse<List<WorkOrderEntryDto>>>($"{url}{search}", cancellationToken);
            return response?.Data ?? new List<WorkOrderEntryDto>();
        }

        public async Task<List<WorkProgressDto>> GetWorkProgressAsync(string corpCode, string workStationRef, string serialNo, CancellationToken cancellationToken = default)
        {
            var url = ApiPrefix + $"pop/{serialNo}/processes";
            var query = BuildQuery(("corpCd", corpCode), ("wkstnRef", workStationRef), ("sn", serialNo));
            var response = await GetAsync<ApiDataResponse<List<WorkProgressDto>>>($"{url}{query}", cancellationToken);
            return response?.Data ?? new List<WorkProgressDto>();
        }

        public async Task<List<ItemResponse>> SearchItemsAsync(ItemQuery keyword, CancellationToken cancellationToken = default)
        {
            var url = $"{ ApiPrefix }md/items";
            var query = BuildQuery(("corpCd", keyword.CorpCode), ("itemCd", keyword.ItemCode), ("itemNm", keyword.ItemName), ("itemAcnt", keyword.ItemAcnt));
            var response = await GetAsync<ApiDataResponse<List<ItemResponse>>>($"{url}{query}", cancellationToken);
            return response?.Data ?? new List<ItemResponse>();
        }
            
    }
}
