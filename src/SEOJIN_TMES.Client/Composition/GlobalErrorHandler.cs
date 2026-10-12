using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using SEOJIN_TMES.Client.ApiClient;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Forms.Common;


namespace SEOJIN_TMES.Client.Composition
{
    /// <summary>
    /// 어디서도 처리되지 않은 예외의 마지막 방어선.
    /// 기본 동작(.NET 기본 오류 창 / 디버거 중단 / 프로세스 종료) 대신 공통 안내 창만 띄우고,
    /// UI 스레드에서 난 예외라면 그대로 작업을 이어갈 수 있게 한다.
    ///
    /// 정상 경로에서는 각 화면이 catch(ApiException)으로 직접 안내하므로 여기까지 오지 않는다 —
    /// 여기 걸리는 건 화면이 미처 감싸지 못한 예외이며, 로그(AppLog.Error)에 전체 스택이 남는다.
    /// </summary>
    internal static class GlobalErrorHandler
    {
        /// <summary>Application.Run 전에 한 번 호출한다.</summary>
        public static void Install()
        {
            // Automatic(기본값)이면 디버거가 붙은 Debug 실행에서 ThreadException을 타지 않고 그대로 중단된다.
            // CatchException으로 고정해서 Debug/Release 모두 아래 핸들러를 타게 한다.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => Report(e.Exception, "UI 스레드");

            // UI 스레드 밖(백그라운드 스레드)에서 난 예외. 프로세스는 어차피 종료되지만, 원인은 남기고 안내는 한다.
            AppDomain.CurrentDomain.UnhandledException += (s, e) => Report(e.ExceptionObject as Exception, "백그라운드 스레드");

            // await 하지 않고 던져 둔 작업(fire-and-forget)의 예외. 창까지 띄우면 시끄러우므로 로그만 남긴다.
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                e.SetObserved();
                AppLog.Error("[전역] 처리되지 않은 백그라운드 작업 예외", e.Exception);
            };
        }

        /// <summary>안내 창을 띄운다. 창 띄우기 자체가 또 실패해도 앱을 죽이지 않는다.</summary>
        private static void Report(Exception exception, string origin)
        {
            AppLog.Error($"[전역] 처리되지 않은 예외 ({origin})", exception);

            if (exception == null)
                return;

            try
            {
                AppMessageBox.Error(Form.ActiveForm, BuildMessage(exception), Language.Message_TitleError);
            }
            catch (Exception reportFailure)
            {
                AppLog.Error("[전역] 오류 안내 창 표시 실패", reportFailure);
            }
        }

        /// <summary>
        /// API 예외는 이미 사용자에게 보여줄 문구를 담고 있으므로 그대로 쓰고,
        /// 그 외에는 공통 안내 문구 뒤에 원인 한 줄을 붙인다.
        /// </summary>
        private static string BuildMessage(Exception exception) =>
            exception is ApiException
                ? exception.Message
                : $"{Language.Common_UnexpectedError}{Environment.NewLine}{Environment.NewLine}{exception.Message}";
    }
}
