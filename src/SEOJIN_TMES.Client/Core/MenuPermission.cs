using SEOJIN_TMES.Client.ApiClient;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Common.Models;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SEOJIN_TMES.Client.Core
{
    /// <summary>
    /// 화면별 버튼 권한(<c>GET api/v1.0/gm/auth/permissions?path=/POP01</c>)을 받아 버튼을 보이거나 숨긴다.
    /// 권한 판정은 서버가 하고, 화면은 받은 Y/N으로 Visible만 정한다.
    ///
    /// 각 화면은 Init에서 권한 대상 버튼을 먼저 숨긴 뒤(<see cref="SetVisible"/>에 false) 응답을 받아 다시 보인다 —
    /// 응답 전에 버튼이 잠깐 보였다 사라지거나, 그 사이에 눌리는 일을 막는다.
    /// </summary>
    public static class MenuPermission
    {
        /// <summary>
        /// 메뉴의 버튼 권한을 받는다. <paramref name="menuCode"/>는 서버 메뉴 코드(<see cref="Menu.MenuCodes"/>, 예: POP01)다 —
        /// 처음에는 메뉴 이름(/AttachmentSearch)을 보냈으나 서버 권한이 메뉴 코드 기준이라 코드로 바꿨다.
        /// 권한 행이 없거나(data=null) 조회에 실패하면 <b>모두 없음</b>으로 돌려준다 —
        /// 확인하지 못한 권한으로 저장 · 삭제 버튼을 열어 두지 않기 위해서다. 실패 사유는 로그에만 남기고 창은 띄우지 않는다
        /// (화면을 열 때마다 부르므로, 서버가 잠깐 불안정할 때 안내 창이 겹겹이 뜨지 않게).
        /// </summary>
        public static async Task<ProgramPermissionDto> Get(string menuCode)
        {
            try
            {
                var permission = await Global.Instance.Settings.ApiClients.AuthorityApiClient.GetProgramPermission(menuCode);
                if (permission == null)
                {
                    AppLog.Warn($"버튼 권한 없음(서버 권한 행 없음) - {menuCode}");
                    return ProgramPermissionDto.None();
                }

                AppLog.Info($"버튼 권한 {menuCode} : {permission}");
                return permission;
            }
            catch (ApiException ex)
            {
                AppLog.Error($"버튼 권한 조회 실패 - {menuCode} : [{ex.Code}] {ex.Message}", ex);
                return ProgramPermissionDto.None();
            }
        }

        /// <summary>권한 여부로 버튼들의 Visible을 한 번에 정한다.</summary>
        public static void SetVisible(bool allowed, params Control[] buttons)
        {
            foreach (var button in buttons)
                button.Visible = allowed;
        }
    }
}
