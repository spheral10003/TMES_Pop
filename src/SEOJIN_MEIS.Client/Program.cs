using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using SEOJIN_MEIS.Client.ApiClient;
using SEOJIN_MEIS.Client.Common.Logging;
using SEOJIN_MEIS.Client.Common.Storage;
using SEOJIN_MEIS.Client.Composition;
using SEOJIN_MEIS.Client.Equipment;
using SEOJIN_MEIS.Client.Forms;
using SEOJIN_MEIS.Client.Theme;

namespace SEOJIN_MEIS.Client
{
    internal static class Program
    {
        /// <summary>
        /// 컴포지션 루트: 설정 로드 → API 클라이언트/설비 Modbus 매니저 조립 → 로그인 → 메인 화면.
        /// 별도 DI 컨테이너 없이 이 지점에서만 객체 그래프를 구성한다.
        /// 각 단계는 AppLog.Step으로 남기며, Debug 빌드는 콘솔 창(OutputType=Exe)에서 그대로 보인다.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 처리되지 않은 예외를 .NET 기본 오류 창/디버거 중단 대신 공통 안내 창으로 바꾼다.
            // (서버 미기동 상태에서 API 호출이 터져도 앱이 죽지 않고 계속 쓸 수 있게 하는 마지막 방어선)
            GlobalErrorHandler.Install();

            // 전역 스킨 + SVG 팔레트를 적용한다(자세한 색 체계는 Theme/AppTheme.cs).
            // XtraForm/XtraUserControl은 별도 설정 없이 UserLookAndFeel.Default를 그대로 따라가므로
            // 이 한 줄로 전체 화면 톤(에디터/그리드/스크롤바 등)이 남보라 계열로 맞춰진다.
            // 상단바의 달/해 버튼을 누르면 AppTheme.ApplySkin(true/false)로 다크 모드로 전환된다.
            DevExpress.UserSkins.BonusSkins.Register();
            DevExpress.Skins.SkinManager.EnableFormSkins();
            AppTheme.ApplySkin(false);

            AppLog.Step("애플리케이션 시작");
            AppLog.Step($"로그 파일 - {AppLog.CurrentFilePath ?? "(파일 기록 불가 - 콘솔/출력 창만 사용)"}");

            AppLog.Step("설정 로드 시작 (App.config)");
            var settings = AppSettingsLoader.Load();
            AppLog.Step($"설정 로드 완료 - ApiBaseUrl={settings.ApiBaseUrl}, ApiTimeoutSeconds={settings.ApiTimeoutSeconds}, 설비 {settings.Equipments.Count}대");
            ApplyCulture(settings.DefaultCulture);
            AppLog.Step($"문화권 적용 완료 - {Thread.CurrentThread.CurrentUICulture}");

            AppLog.Step("API 클라이언트 구성");
            var apiOptions = new ApiClientOptions(settings.ApiBaseUrl, settings.ApiTimeoutSeconds,
                settings.LogApiResponseBody, settings.LogApiResponseBodyMaxLength);
            var apiClients = new ApiClientFactory(apiOptions);

            AppLog.Step("설비 Modbus 매니저 구성 및 등록 시작");
            var equipmentManager = new EquipmentModbusManager();
            equipmentManager.RegisterAll(settings.Equipments);
            AppLog.Step("설비 Modbus 매니저 등록 완료 (접속 시도는 백그라운드에서 계속 진행)");

            AppLog.Step($"문서 공유 폴더(SMB) 구성 - RootPath={settings.DocumentShare.RootPath ?? "(미설정)"}, 계정={(settings.DocumentShare.UsesExplicitCredentials ? settings.DocumentShare.ResolveUserName() : "현재 윈도우 계정")}");
            var documentShare = new DocumentShareClient(settings.DocumentShare);

            var context = new ClientContext(settings, apiClients, equipmentManager, documentShare);

            try
            {
                AppLog.Step("로그인 화면 표시");
                using (var loginForm = new LoginForm())
                {
                    loginForm.Initialize(context);
                    if (loginForm.ShowDialog() != DialogResult.OK)
                    {
                        AppLog.Step("로그인 취소/종료 - 애플리케이션 종료");
                        return;
                    }
                }
                AppLog.Step($"로그인 성공 - UserId={context.CurrentUser?.UserId}");

                AppLog.Step("메인 화면 시작");
                var mainForm = new MainForm();
                mainForm.Initialize(context);
                Application.Run(mainForm);
                AppLog.Step("메인 화면 종료");
            }
            finally
            {
                AppLog.Step("설비 Modbus 매니저 · 문서 공유 폴더 정리 및 애플리케이션 종료");
                equipmentManager.Dispose();
                documentShare.Dispose();

                // 로그 파일은 마지막에 닫는다 — 위 정리 과정에서 나는 로그까지 파일에 남겨야 한다.
                AppLog.Close();
            }

            // 프로세스를 확실히 끝낸다.
            //
            // Debug 구성은 콘솔 서브시스템(OutputType=Exe)이라 앱이 자기 콘솔 창을 하나 갖는데,
            // 화면을 닫아도 프로세스가 남아 있으면 그 콘솔 창만 덩그러니 남는다.
            // 설비 수신 스레드는 전부 IsBackground=true라 붙잡지 않지만,
            // 통신·타이머 쪽에서 뒤늦게 도는 작업이 남을 수 있어 여기서 매듭짓는다.
            //
            // 정리(Dispose·로그 닫기)는 위 finally에서 이미 끝났으므로 남는 작업은 없다.
            Environment.Exit(0);
        }

        private static void ApplyCulture(string cultureName)
        {
            if (string.IsNullOrWhiteSpace(cultureName))
                return;

            try
            {
                var culture = CultureInfo.GetCultureInfo(cultureName);
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
            }
            catch (CultureNotFoundException)
            {
                // 알 수 없는 culture 설정 값이면 OS 기본 culture를 그대로 사용한다.
            }
        }
    }
}
