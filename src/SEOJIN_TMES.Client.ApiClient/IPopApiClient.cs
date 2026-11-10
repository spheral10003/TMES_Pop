using SEOJIN_TMES.Client.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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

        /// <summary>
        /// 지금 페이지 공정이 이 시리얼의 마지막 공정인지 — <c>GET pop/{sn}/{subRpRef}/is-last?corpCd=&amp;woNo=</c>
        /// (응답 <c>{"data":{"lastProc":bool}}</c>).
        /// 무엇을 "마지막"으로 볼지는 서버가 판단한다(클라이언트는 공정 목록 순서로 따지지 않는다).
        /// </summary>
        Task<bool> GetIsLastProcessAsync(PopProcessKey key, CancellationToken cancellationToken = default);

        /// <summary>&lt;투입자재 정보 입력&gt; 팝업 [저장] — 입력한 시리얼번호를 서버로 보낸다.</summary>
        //Task SaveMaterialSerialsAsync(PopMaterialSerialSaveRequest request, CancellationToken cancellationToken = default);

        Task SaveMaterialSerialsAsync(PopMaterialSerialSaveRequest request, PopProcessKey key, CancellationToken cancellationToken = default);
        Task SaveMaterialSerialsAsync(PopMaterialSerialSaveRequest request, PopProcessKey key, string code, CancellationToken cancellationToken = default);

        /// <summary>[BOM 스팩] 탭 토크 결과값 저장(설비 수신값 / 수동 렌치 입력값 공통).</summary>
        Task SaveBomSpecResultsAsync(PopProcessKey key, PopBomSpecResultDto result, PopBomSpecDto bom, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// [검사 스팩] 탭 한 행의 검사 결과 저장(셀 값이 바뀔 때마다 호출).
        /// <paramref name="inspectionItemCode"/>는 URL 경로에만 쓰고 Body(<paramref name="result"/>)에는 담지 않는다.
        /// </summary>
        Task SaveInspectionResultAsync(PopProcessKey key, string inspectionItemCode, PopInspectionResult result, CancellationToken cancellationToken = default);

        /// <summary>
        /// 공정 상태 전이 요청 — 시작/중지/재시작/리웍/종료.
        /// 전이 가능 여부와 실적 기록은 전부 서버가 판단한다(클라이언트는 요청만 보낸다).
        /// 반환값은 바뀐 진행 상태 · 시각 · 시간 집계뿐이다(탭 목록 · 도면 경로는 오지 않는다).
        /// </summary>
        Task<PopProcessInfoDto> ChangeProcessStateAsync(PopProcessKey key, PopProcessAction action, PopProcessActionRequest request = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 이상 알람 유형 기준정보 조회 — &lt;알람 조회 / 등록&gt; 팝업의 타일 목록.
        /// 회사 코드만 필수이고, 비운 조건은 쿼리에서 빠져 전체로 조회된다.
        /// </summary>
        Task<List<AbnormalAlarmTypeDto>> GetAbnormalAlarmTypes(AbnormalAlarmTypeQuery query, CancellationToken cancellationToken = default);

        /// <summary>
        /// 이상 알람 등록 — 지금 공정(시리얼 + 서브 라우팅)에 고른 알람을 올린다.
        /// 누구에게 어떻게 통지할지는 서버가 정한다.
        /// </summary>
        Task SetAbnormalAlarm(PopProcessKey key, AbnormalAlarmRequest request, CancellationToken cancellationToken = default);
    }

    /// <summary>POP 상단 버튼이 서버에 요청하는 공정 상태 전이 종류.</summary>
    public enum PopProcessAction
    {
        start,
        stop,
        resume,
        /// <summary>리웍 — 완료된 공정을 진행 중으로 되돌려 값을 다시 고칠 수 있게 한다(resume과 같은 경로 · 조건 쿼리).</summary>
        rework,
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
            var response = await GetAsync<ApiDataResponse<PopApiResponse>>($"{PopPrefix}{key.SerialNo}/{key.SubRoutingRef}{query}", cancellationToken);
            PopMapper.Map(response?.Data);
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

        public async Task<bool> GetIsLastProcessAsync(PopProcessKey key, CancellationToken cancellationToken = default)
        {
            // 응답은 {"data":{"lastProc":true|false}, "status":200, ...} 봉투로 온다(테스트 서버 로그로 확인).
            // 본문 전체를 bool로 읽으면 해석 실패 → 화면이 늘 "마지막 아님"으로 처리했다.
            // 본문 · data가 비어 오면 마지막이 아닌 것(false)으로 본다.
            var url = $"{PopPrefix}{key.SerialNo}/{key.SubRoutingRef}/is-last{KeyQuery(key)}";
            var response = await GetAsync<ApiDataResponse<PopIsLastProcessDto>>(url, cancellationToken);
            return response?.Data?.IsLastProcess ?? false;
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

        public Task SaveInspectionResultAsync(PopProcessKey key, string inspectionItemCode, PopInspectionResult result, CancellationToken cancellationToken = default)
        {

            var query = PostKeyQuery(key);
            string url = $"{PopPrefix}{key.SerialNo}/{key.SubRoutingRef}/insp-chk-results/{inspectionItemCode}{query}";
            return PostAsync(url, result, cancellationToken);
        }

        public async Task<PopProcessInfoDto> ChangeProcessStateAsync(PopProcessKey key, PopProcessAction action, PopProcessActionRequest request = null, CancellationToken cancellationToken = default)
        {
            var query = PostKeyQuery(key);
            string url = $"{PopPrefix}{key.SerialNo}/{key.SubRoutingRef}/{action.ToString()}{query}";

            // 응답은 {"data":{workStatus, trackInDtm, actlWorkDurSec, ...}} 봉투로 온다 — 벗겨 읽어야 바뀐 상태를 화면이 쓸 수 있다.
            // PopProcessInfoDto로 바로 읽으면 안 된다: 시간 필드가 초 단위 숫자(예: 706)라 TimeSpan으로 변환하지 못해
            // 응답 해석 실패로 공정 이동이 막혔다. 공정 정보 조회(GetProcessInfoAsync)와 같이 원본 형식으로 읽고 PopMapper로 바꾼다.
            var data = await PostDataAsync<PopProcessActionRequest, PopApiResponse>(url, request, cancellationToken);
            return data == null ? null : PopMapper.Map(data).Process;
        }

        public async Task<List<AbnormalAlarmTypeDto>> GetAbnormalAlarmTypes(AbnormalAlarmTypeQuery query, CancellationToken cancellationToken = default)
        {
            var condition = BuildQuery(
                ("corpCd", query?.CorpCode),
                ("alrmType", query?.AlarmType),
                ("alrmTypeCd", query?.AlarmTypeCode),
                ("alrmTypeNm", query?.AlarmTypeName),
                ("alrmGrd", query?.AlarmGrade),
                ("useYn", query?.UseYn));
            string url = $"{ApiPrefix}md/abnormal-alarm-types{condition}";
            var response = await GetAsync<ApiDataResponse<List<AbnormalAlarmTypeDto>>>(url, cancellationToken);
            return response?.Data ?? new List<AbnormalAlarmTypeDto>();
        }

        public Task SetAbnormalAlarm(PopProcessKey key, AbnormalAlarmRequest request, CancellationToken cancellationToken = default)
        {
            var query = BuildQuery(("corpCd", key.CorpCode), ("woNo", key.WorkOrderNo), ("wkstnRef", key.WorkstationRef));
            string url = $"{PopPrefix}{key.SerialNo}/{key.SubRoutingRef}/abnormal-alarms{query}";
            return PostAsync(url, request, cancellationToken);
        }

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
