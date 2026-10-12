using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SEOJIN_TMES.Client.Common.Models;

namespace SEOJIN_TMES.Client.ApiClient
{
    /// <summary>설비 Modbus-TCP 통신으로 수신한 상태를 서버로 전달(업로드)한다. 판정/이력 적재는 서버 책임.</summary>
    public interface IEquipmentStatusApiClient
    {
        Task ReportStatusAsync(EquipmentStatusDto status, CancellationToken cancellationToken = default);
    }

    public class EquipmentStatusApiClient : ApiClientBase, IEquipmentStatusApiClient
    {
        public EquipmentStatusApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public Task ReportStatusAsync(EquipmentStatusDto status, CancellationToken cancellationToken = default)
        {
            return PostAsync(ApiPrefix + "equipment/status", status, cancellationToken);
        }
    }
}
