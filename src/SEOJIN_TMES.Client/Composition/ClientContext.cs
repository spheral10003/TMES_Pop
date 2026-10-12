using SEOJIN_TMES.Client.ApiClient;
using SEOJIN_TMES.Client.Common.Configuration;
using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Common.Storage;
using SEOJIN_TMES.Client.Equipment;
using SEOJIN_TMES.Client.Equipment.Management;

namespace SEOJIN_TMES.Client.Composition
{
    /// <summary>
    /// 프로세스 전역에서 공유하는 조립 결과(설정, API 클라이언트, 설비 Modbus 매니저, 문서 공유 폴더, 로그인 세션).
    /// 별도 DI 컨테이너 없이 Program.cs(컴포지션 루트)에서 한 번 생성해 폼들에 전달한다.
    /// </summary>
    public class ClientContext
    {
        public AppSettings Settings { get; }
        public ApiClientFactory ApiClients { get; }
        public readonly INutrunnerManager EquipmentManager = new NutrunnerManager();
         
        /// <summary>작업 표준/도면 PDF를 읽어 오는 SMB 공유 폴더 클라이언트(POP 화면 좌측 도면 영역).</summary>
        public DocumentShareClient DocumentShare { get; }

        public UserSession CurrentUser { get; set; }

        public ClientContext(
            AppSettings settings,
            ApiClientFactory apiClients,
            INutrunnerManager equipmentManager,
            DocumentShareClient documentShare)
        {
            Settings = settings;
            ApiClients = apiClients;
            EquipmentManager = equipmentManager;
            DocumentShare = documentShare;
        }
    }
}
