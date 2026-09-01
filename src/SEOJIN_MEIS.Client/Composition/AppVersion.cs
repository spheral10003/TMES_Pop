using System;
using System.Deployment.Application;
using System.Reflection;
using SEOJIN_MEIS.Client.Common.Logging;

namespace SEOJIN_MEIS.Client.Composition
{
    /// <summary>
    /// 지금 돌고 있는 클라이언트의 버전.
    ///
    /// 현장 PC는 ClickOnce로 설치되므로 <b>실제 배포 버전은 매니페스트에 있다</b>
    /// (csproj의 <c>ApplicationVersion</c>이 <c>1.0.0.%2a</c>라 게시할 때마다 마지막 자리가 자동으로 올라간다).
    /// 그래서 어셈블리 버전이 아니라 <see cref="ApplicationDeployment.CurrentVersion"/>을 우선 쓴다 —
    /// "이 PC에 몇 번째 배포본이 깔려 있는가"가 현장에서 확인해야 하는 값이기 때문이다.
    ///
    /// 개발 PC에서 exe를 직접 실행하면 ClickOnce가 아니므로 어셈블리 버전으로 대체한다.
    /// </summary>
    public static class AppVersion
    {
        /// <summary>버전 문자열. ClickOnce로 설치됐으면 배포 버전(예: 1.0.0.7), 아니면 어셈블리 버전(예: 1.0.0).</summary>
        public static string Current { get; } = Resolve();

        private static string Resolve()
        {
            try
            {
                if (ApplicationDeployment.IsNetworkDeployed)
                    return ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            }
            catch (InvalidDeploymentException ex)
            {
                // 매니페스트가 깨졌거나 읽을 수 없는 경우 — 버전 표시 하나 때문에 앱이 죽으면 안 되므로
                // 아래 어셈블리 버전으로 넘어간다. 원인은 로그로 남겨 둔다.
                AppLog.Warn($"ClickOnce 배포 버전을 읽지 못했다 - {ex.Message}");
            }

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return version == null ? "-" : version.ToString(3);
        }
    }
}
