using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Core;
using SEOJIN_TMES.Client.Forms;
using System;
using System.Threading;
using System.Windows.Forms;

namespace SEOJIN_TMES.Client
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
            Mutex mutex = new Mutex(true, "SEOJIN_TMES_POP", out bool bNew);
            
            if (bNew)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                try
                {
                    AppLog.Step("애플리케이션 시작");
                    AppLog.Step($"로그 파일 - {AppLog.CurrentFilePath ?? "(파일 기록 불가 - 콘솔/출력 창만 사용)"}");

                    Global.Initialize();

                    AppLog.Step("로그인 화면 표시");
                    using (var loginForm = new LoginForm())
                    {
                        loginForm.Init();
                        if (loginForm.ShowDialog() != DialogResult.OK)
                        {
                            AppLog.Step("로그인 취소/종료 - 애플리케이션 종료");
                            return;
                        }
                    }

                    Application.Run(new MainForm());

                }
                catch (Exception ex)
                {
                    AppLog.Error("애플리케이션 실행 중 처리되지 않은 예외", ex);
                }
                finally
                {
                    AppLog.Step("설비 Modbus 매니저 · 문서 공유 폴더 정리 및 애플리케이션 종료");
                    // 초기화가 실패했어도 안전하게 돈다
                    Global.Shutdown();
                    // 로그 파일은 마지막에 닫는다 — 위 정리 과정에서 나는 로그까지 파일에 남겨야 한다.
                    AppLog.Close();
                    
                    mutex.ReleaseMutex();
                }
            }
            else
            {
                AppLog.Step("애플리케이션 실행중.....");
                Application.Exit();
            }
            
            Environment.Exit(0);
        }

        //private static void ApplyCulture(string cultureName)
        //{
        //    if (string.IsNullOrWhiteSpace(cultureName))
        //        return;

        //    try
        //    {
        //        var culture = CultureInfo.GetCultureInfo(cultureName);
        //        Thread.CurrentThread.CurrentCulture = culture;
        //        Thread.CurrentThread.CurrentUICulture = culture;
        //    }
        //    catch (CultureNotFoundException)
        //    {
        //        // 알 수 없는 culture 설정 값이면 OS 기본 culture를 그대로 사용한다.
        //    }
        //}
    }
}
