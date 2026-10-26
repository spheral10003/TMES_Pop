using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SEOJIN_TMES.Client.Common.Models;

namespace SEOJIN_TMES.Client.ApiClient
{
    /// <summary>
    /// POP 본 화면(진행 중 공정 정보 + 하단 4개 탭) 전용 API.
    /// 조회는 전부 <see cref="PopProcessKey"/>(작업지시 + 시리얼 + 라우팅 + 페이지 공정)를 조건으로 받는다.
    ///
    /// TODO: 엔드포인트 경로는 모두 <b>가정값</b>이다. 실제 서버 스펙이 확정되면 경로/파라미터명을 맞출 것.
    /// </summary>
    public interface IPopApiClient
    {
        /// <summary>&lt;진행 중 공정 정보&gt; 한 건(작업시간·가동시간 등 계산값 포함).</summary>
        Task<PopScreenData> GetProcessInfoAsync(PopProcessKey key, CancellationToken cancellationToken = default);

        /// <summary>[투입자재] 탭 — 이 공정에서 투입할 자재 목록.</summary>
        Task<List<PopInputMaterialDto>> GetInputMaterialsAsync(PopProcessKey key, CancellationToken cancellationToken = default);

        /// <summary>[BOM 스팩] 탭 — 기준정보에 등록된 토크 작업 스펙 목록.</summary>
        Task<List<PopBomSpecDto>> GetBomSpecsAsync(PopProcessKey key, CancellationToken cancellationToken = default);

        /// <summary>[검사 스팩] 탭 — 기준정보에 등록된 검사 항목 목록.</summary>
        Task<List<PopInspectionSpecDto>> GetInspectionSpecsAsync(PopProcessKey key, CancellationToken cancellationToken = default);

        /// <summary>[연결 장비] 탭 — 워크스테이션에 연결된 장비 기준정보 목록(통신상태는 클라이언트가 덧입힌다).</summary>
        Task<List<PopEquipmentDto>> GetWorkstationEquipmentsAsync(PopProcessKey key, CancellationToken cancellationToken = default);

        Task<string> GetOmsPathAsync(PopProcessKey key, CancellationToken cancellationToken = default);
        /// <summary>&lt;투입자재 정보 입력&gt; 팝업 [저장] — 입력한 시리얼번호를 서버로 보낸다.</summary>
        //Task SaveMaterialSerialsAsync(PopMaterialSerialSaveRequest request, CancellationToken cancellationToken = default);

        Task SaveMaterialSerialsAsync(PopMaterialSerialSaveRequest request, PopProcessKey key, CancellationToken cancellationToken = default);
        Task SaveMaterialSerialsAsync(PopMaterialSerialSaveRequest request, PopProcessKey key, string code, CancellationToken cancellationToken = default);

        /// <summary>[BOM 스팩] 탭 토크 결과값 저장(설비 수신값 / 수동 렌치 입력값 공통).</summary>
        Task SaveBomSpecResultsAsync(PopProcessKey key, PopBomSpecResultDto result, PopBomSpecDto bom, CancellationToken cancellationToken = default);
        
        /// <summary>[검사 스팩] 탭 한 행의 검사 결과 저장(셀 값이 바뀔 때마다 호출).</summary>
        Task SaveInspectionResultAsync(PopProcessKey key, PopInspectionResult result, CancellationToken cancellationToken = default);

        /// <summary>
        /// 공정 상태 전이 요청 — 시작/중지/생략/이전/다음.
        /// 전이 가능 여부와 실적 기록은 전부 서버가 판단한다(클라이언트는 요청만 보낸다).
        /// </summary>
        Task<PopProcessInfoDto> ChangeProcessStateAsync(PopProcessKey key, PopProcessAction action, PopProcessActionRequest request = null, CancellationToken cancellationToken = default);

        /// <summary>&lt;관리자 알람 유형 선택&gt; 팝업의 알람 유형 목록(기준정보).</summary>
        Task<List<AdminAlarmTypeDto>> GetAdminAlarmTypesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 관리자 알람 전송 — 알람 유형·대상·비고에 현재 공정/작업자 정보를 붙여 서버에 알린다.
        /// 누구에게 어떻게 통지할지(알림/문자/방송 등)는 서버가 정한다.
        /// </summary>
        Task NotifyAdministratorAsync(PopAdminAlarmRequest request, CancellationToken cancellationToken = default);
    }

    /// <summary>POP 상단 버튼이 서버에 요청하는 공정 상태 전이 종류.</summary>
    public enum PopProcessAction
    {
        start,
        stop,
        resume,
        end,
        Previous,
        Next
    }

    public class PopApiClient : ApiClientBase, IPopApiClient
    {
        private const string PopPrefix = ApiPrefix + "pop/";

        public PopApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<PopScreenData> GetProcessInfoAsync(PopProcessKey key, CancellationToken cancellationToken = default)
        {
            var query = BuildQuery(("corpCd", key.CorpCode), ("woNo", key.WorkOrderNo), ("routingGroupRef", key.RoutingGroupRef), ("wkstnRef", key.WorkstationRef));
            //PopScreenData screen = PopMapper.Parse(json);
            var response = await GetAsync<ApiDataResponse<PopApiResponse>>($"{PopPrefix}{key.SerialNo}/{key.SubRoutingRef}{query}", cancellationToken);
            PopMapper.Map(response?.Data);
            //return response?.Data ?? new PopProcessInfoDto();
            return PopMapper.Map(response?.Data);
        }
            

        public async Task<List<PopInputMaterialDto>> GetInputMaterialsAsync(PopProcessKey key, CancellationToken cancellationToken = default)
        {
            var response = await GetAsync<ApiDataResponse<List<PopInputMaterialDto>>>($"{PopPrefix}{key.SerialNo}/{key.SubRoutingRef}/materials{KeyQuery(key)}", cancellationToken);
            return response?.Data ?? new List<PopInputMaterialDto>();
        }
            

        public async Task<List<PopBomSpecDto>> GetBomSpecsAsync(PopProcessKey key, CancellationToken cancellationToken = default)
        {
            var response = await GetAsync<ApiDataResponse<List<PopBomSpecDto>>>($"{PopPrefix}{key.SerialNo}/{key.SubRoutingRef}/bom-specs{KeyQuery(key)}", cancellationToken);
            return response?.Data ?? new List<PopBomSpecDto>();
        }
            
        public async Task<List<PopInspectionSpecDto>> GetInspectionSpecsAsync(PopProcessKey key, CancellationToken cancellationToken = default)
        {
            var response = await GetAsync<ApiDataResponse<List<PopInspectionSpecDto>>>($"{PopPrefix}{key.SerialNo}/{key.SubRoutingRef}/inspection-items{KeyQuery(key)}", cancellationToken);
            return response?.Data ?? new List<PopInspectionSpecDto>();
        }
            
        public async Task<List<PopEquipmentDto>> GetWorkstationEquipmentsAsync(PopProcessKey key, CancellationToken cancellationToken = default)
        {
            var query = BuildQuery(("corpCd", key.CorpCode), ("wkstnRef", key.WorkstationRef));
            var response = await GetAsync<ApiDataResponse<List<PopEquipmentDto>>>($"{ApiPrefix}md/equipments{query}", cancellationToken);
            return response?.Data ?? new List<PopEquipmentDto>();
        }

        public async Task<string> GetOmsPathAsync(PopProcessKey key, CancellationToken cancellationToken = default)
        {
            var response = await GetPathAsync<string>($"{PopPrefix}{key.SubRoutingRef}/oms-image", cancellationToken);
            return response;
        }

        public Task SaveMaterialSerialsAsync(PopMaterialSerialSaveRequest request, PopProcessKey key, CancellationToken cancellationToken = default)
        {
            
            var query = PostKeyQuery(key); //BuildQuery(("corpCd", key.CorpCode), ("woNo", key.WorkOrderNo), ("routingGroupRef", key.RoutingGroupRef)); //KeyQuery(request.Key);
            
            return PostAsync($"{PopPrefix}{key.SerialNo}/{key.SubRoutingRef}/bom-input-results/{query}", request, cancellationToken);
            
        }
        public Task SaveMaterialSerialsAsync(PopMaterialSerialSaveRequest request, PopProcessKey key, string code, CancellationToken cancellationToken = default)
        {
            
            var query = PostKeyQuery(key);
            string url = $"{PopPrefix}{key.SerialNo}/{key.SubRoutingRef}/bom-input-results/{code}{query}";
            return PostAsync(url, request, cancellationToken);
        
        }

        public Task SaveBomSpecResultsAsync(PopProcessKey key, PopBomSpecResultDto result, PopBomSpecDto bom, CancellationToken cancellationToken = default)
        {
            var query = PostKeyQuery(key);
            string url = $"{PopPrefix}{key.SerialNo}/{key.SubRoutingRef}/bom-spec-results/{bom.MaterialCode}/{bom.Seq}{query}";
            return PostAsync(url, result, cancellationToken);
        }

        public Task SaveInspectionResultAsync(PopProcessKey key, PopInspectionResult result, CancellationToken cancellationToken = default)
        {
            
            var query = PostKeyQuery(key);
            string url = $"{PopPrefix}{key.SerialNo}/{key.SubRoutingRef}/insp-chk-results/{result.InspectionItemCode}{query}";
            // TODO: 경로는 가정값 — 서버 스펙 확정 시 맞출 것.
            return PostAsync(url, result, cancellationToken);
        }

        public Task<PopProcessInfoDto> ChangeProcessStateAsync(PopProcessKey key, PopProcessAction action, PopProcessActionRequest request = null, CancellationToken cancellationToken = default)
        {
            var query = PostKeyQuery(key);
            string url = $"{PopPrefix}{key.SerialNo}/{key.SubRoutingRef}/{action.ToString()}{query}";
            
            return PostAsync<PopProcessActionRequest, PopProcessInfoDto>(url, request, cancellationToken);
        }

        public Task<List<AdminAlarmTypeDto>> GetAdminAlarmTypesAsync(CancellationToken cancellationToken = default) =>
            GetAsync<List<AdminAlarmTypeDto>>(PopPrefix + "md/abnormal-alarm-types", cancellationToken);

        public Task NotifyAdministratorAsync(PopAdminAlarmRequest request, CancellationToken cancellationToken = default) =>
            PostAsync(PopPrefix + "admin-alarm", request, cancellationToken);

        /// <summary>탭 조회 API가 공통으로 쓰는 조건 쿼리(작업지시 + 시리얼 + 라우팅 + 페이지 공정).</summary>
        private static string KeyQuery(PopProcessKey key)
        {
            if (key == null)
                return string.Empty;

            return BuildQuery(("corpCd", key.CorpCode), ("woNo", key.WorkOrderNo));
        }

        private static string PostKeyQuery(PopProcessKey key)
        {
            if (key == null)
                return string.Empty;

            return BuildQuery(("corpCd", key.CorpCode), ("woNo", key.WorkOrderNo), ("routingGroupRef", key.RoutingGroupRef));
        }
    }

    /// <summary>결과 저장 요청 본문(조건 키 + 저장할 행 목록).</summary>
    public class PopResultSaveRequest<T>
    {
        public PopProcessKey Key { get; set; }
        public List<T> Items { get; set; }
    }

    /// <summary>공정 상태 전이 요청 본문.</summary>
    public class PopProcessActionRequest
    {
        public string dttmCd { get; set; }
        public string remark { get; set; }
        //public PopProcessKey Key { get; set; }
        //public string Action { get; set; }
    }

    /// <summary>관리자 알람 요청 본문 — &lt;관리자 알람 유형 선택&gt; 팝업에서 입력한 내용 + 현재 공정/작업자.</summary>
    public class PopAdminAlarmRequest
    {
        /// <summary>알람이 발생한 공정(작업지시 + 라우팅 + 페이지 공정).</summary>
        public PopProcessKey Key { get; set; }

        /// <summary>알람을 보낸 작업자 아이디.</summary>
        public string UserId { get; set; }

        /// <summary>알람 유형 코드(기준정보의 <see cref="AdminAlarmTypeDto.Code"/>).</summary>
        public string AlarmTypeCode { get; set; }

        /// <summary>알람 유형 명 — 서버 로그/알림 문구에 그대로 쓸 수 있게 함께 보낸다.</summary>
        public string AlarmTypeName { get; set; }

        /// <summary>대상 코드. 자재 결품이면 자재 코드, 설비 이상이면 장비 코드. 대상이 없는 유형이면 null.</summary>
        public string TargetCode { get; set; }

        /// <summary>작업자가 적은 비고.</summary>
        public string Remark { get; set; }
    }

    public enum PopProcessStopType
    {
        [Description("자재부족")]
        PD01 = 0,
        [Description("자재불량")]
        PD02 = 1,
        [Description("작업불량")]
        PD03 = 2,
        [Description("장비점검")]  
        PD04 = 3,
        [Description("자원부족")]
        PD05 = 4,
        [Description("품질검사")]
        PD06 = 5,
        [Description("식사시간")]
        PD07 = 6,
        [Description("휴식시간")]
        PD08 = 7,
        [Description("업무종료")]
        PD09 = 8,
    }
}
