using SEOJIN_TMES.Client.ApiClient;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Forms.Common;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SEOJIN_TMES.Client.Core
{
    /// <summary>
    /// 화면별 버튼 권한(<c>GET api/v1.0/gm/auth/permissions?path=/POP01</c>)을 받아 화면 열기 여부와 버튼 표시를 정한다.
    /// 권한 판정은 서버가 하고, 클라이언트는 받은 Y/N으로 화면을 열지 · 버튼을 보일지만 정한다.
    ///
    /// - 메뉴를 누르면 MainForm이 화면을 만들기 전에 <see cref="CanOpen"/>으로 조회 권한을 확인한다 —
    ///   없으면 "권한이 없습니다" 안내만 띄우고 탭을 열지 않는다.
    /// - 열린 화면은 Init에서 권한 대상 버튼을 먼저 숨긴 뒤(<see cref="SetVisible"/>에 false) <see cref="Get"/> 응답으로 다시 보인다 —
    ///   응답 전에 버튼이 잠깐 보였다 사라지거나, 그 사이에 눌리는 일을 막는다.
    /// </summary>
    public static class MenuPermission
    {
        /// <summary>
        /// 메뉴의 버튼 권한을 받는다. <paramref name="menuCode"/>는 서버 메뉴 코드(<see cref="Menu.MenuCodes"/>, 예: POP01)다 —
        /// 처음에는 메뉴 이름(/AttachmentSearch)을 보냈으나 서버 권한이 메뉴 코드 기준이라 코드로 바꿨다.
        /// 권한 행이 없거나(data=null) 조회에 실패하면 <b>모두 없음</b>으로 돌려준다 —
        /// 확인하지 못한 권한으로 저장 · 삭제 버튼을 열어 두지 않기 위해서다. 실패 사유는 로그에만 남기고 창은 띄우지 않는다
        /// (화면 열기 전 <see cref="CanOpen"/>에서 이미 안내했고, 여기까지 와서 창을 또 띄우면 겹친다).
        /// </summary>
        public static async Task<ProgramPermissionDto> Get(string menuCode)
        {
            try
            {
                return await GetFromServer(menuCode);
            }
            catch (ApiException ex)
            {
                AppLog.Error($"버튼 권한 조회 실패 - {menuCode} : [{ex.Code}] {ex.Message}", ex);
                return ProgramPermissionDto.None();
            }
            catch (Exception ex)
            {
                // 화면들이 async void(SetPermission)에서 부르므로 예상 밖 예외도 여기서 막는다 — 새면 앱이 종료된다.
                AppLog.Error($"버튼 권한 조회 중 예외 - {menuCode}", ex);
                return ProgramPermissionDto.None();
            }
        }

        /// <summary>
        /// 메뉴 화면을 열어도 되는지 — 조회 권한(srchAuthYn)이 Y일 때만 true.
        /// 권한이 없으면(권한 행 없음 포함) "권한이 없습니다"(Common_NoPermission, 다국어)를,
        /// 조회에 실패하면 그 오류 안내를 <paramref name="owner"/> 위에 띄우고 false — 권한을 확인하지 못한 화면은 열지 않는다.
        /// </summary>
        public static async Task<bool> CanOpen(string menuCode, IWin32Window owner)
        {
            ProgramPermissionDto permission;
            try
            {
                permission = await GetFromServer(menuCode);
            }
            catch (ApiException ex)
            {
                AppLog.Error($"메뉴 열기 전 권한 조회 실패 - {menuCode} : [{ex.Code}] {ex.Message}", ex);
                AppMessageBox.Warn(owner, ex.Message, Language.Common_Notice);
                return false;
            }
            catch (Exception ex)
            {
                // MainForm 메뉴 클릭(async void)에서 부르므로 예상 밖 예외도 여기서 막는다.
                AppLog.Error($"메뉴 열기 전 권한 조회 중 예외 - {menuCode}", ex);
                AppMessageBox.Warn(owner, Language.Common_UnexpectedError, Language.Common_Notice);
                return false;
            }

            if (permission.CanSearch)
                return true;

            AppLog.Warn($"조회 권한 없음 - 화면 열지 않음 - {menuCode}");
            AppMessageBox.Warn(owner, Language.Common_NoPermission, Language.Common_Notice);
            return false;
        }

        /// <summary>서버에서 권한을 받는다. 권한 행이 없으면(data=null) 모두 없음. 실패는 ApiException으로 그대로 올린다.</summary>
        private static async Task<ProgramPermissionDto> GetFromServer(string menuCode)
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

        /// <summary>권한 여부로 버튼들의 Visible을 한 번에 정한다.</summary>
        public static void SetVisible(bool allowed, params Control[] buttons)
        {
            foreach (var button in buttons)
                button.Visible = allowed;
        }
    }
}
