using Newtonsoft.Json;

namespace SEOJIN_TMES.Client.Common.Models
{
    /// <summary>
    /// 이 서버(B/E)는 일부 응답을 <c>{ "data": ... }</c> 로 한 겹 싸서 내려준다
    /// (확인된 예: my-salt → <c>{"data":"31d5dd…"}</c>).
    /// 그런 엔드포인트는 이 타입으로 받아 <see cref="Data"/>만 꺼내 쓴다.
    /// </summary>
    public class ApiDataResponse<T>
    {
        [JsonProperty("data")]
        public T Data { get; set; }
    }
}
