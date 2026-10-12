using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SEOJIN_TMES.Client.Common.Models;

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
        Task<List<WorkOrderEntryDto>> SearchWorkOrdersAsync(WorkStatusQuery query, CancellationToken cancellationToken = default);

        /// <summary>선택한 작업지시의 공정/스팩별 진행 현황.</summary>
        Task<List<WorkProgressDto>> GetWorkProgressAsync(string workOrderNo, string serialNo, CancellationToken cancellationToken = default);

        /// <summary>&lt;생산품목 선택&gt; 팝업의 품목 검색(품목코드/품목명 부분 일치).</summary>
        Task<List<ItemDto>> SearchItemsAsync(string keyword, CancellationToken cancellationToken = default);
    }

    public class WorkStatusApiClient : ApiClientBase, IWorkStatusApiClient
    {
        public WorkStatusApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public Task<List<WorkOrderEntryDto>> SearchWorkOrdersAsync(WorkStatusQuery query, CancellationToken cancellationToken = default)
        {
            var condition = query ?? new WorkStatusQuery();
            var url = ApiPrefix + "work-status/workorders" + BuildQuery(
                ("orderDateFrom", condition.OrderDateFrom?.ToString("yyyy-MM-dd")),
                ("orderDateTo", condition.OrderDateTo?.ToString("yyyy-MM-dd")),
                ("workOrderNo", condition.WorkOrderNo),
                ("itemCode", condition.ItemCode),
                ("serialNo", condition.SerialNo),
                ("statusName", condition.StatusName));

            return GetAsync<List<WorkOrderEntryDto>>(url, cancellationToken);
        }

        public Task<List<WorkProgressDto>> GetWorkProgressAsync(string workOrderNo, string serialNo, CancellationToken cancellationToken = default)
        {
            var query = BuildQuery(("serialNo", serialNo));
            return GetAsync<List<WorkProgressDto>>(
                $"{ApiPrefix}work-status/workorders/{WebUtility.UrlEncode(workOrderNo)}/progress{query}", cancellationToken);
        }

        public Task<List<ItemDto>> SearchItemsAsync(string keyword, CancellationToken cancellationToken = default) =>
            GetAsync<List<ItemDto>>(ApiPrefix + "items/search" + BuildQuery(("keyword", keyword)), cancellationToken);
    }
}
