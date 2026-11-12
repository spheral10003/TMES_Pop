using SEOJIN_TMES.Client.Common.Logging;
using System;
using System.Diagnostics;
using System.IO;

namespace SEOJIN_TMES.Client.Forms.Common
{
    /// <summary>
    /// Windows 화상 키보드(osk.exe)를 띄운다 — 물리 키보드가 없는 터치 POP에서 입력 칸을 눌렀을 때 쓴다.
    ///
    /// OSK는 포커스를 가져가지 않는 창이라, 지금 포커스를 가진 편집기에 그대로 글자가 들어간다.
    /// 그래서 값을 받아 셀에 옮겨 주는 처리가 따로 필요 없다(직접 타이핑한 것과 같은 경로).
    /// </summary>
    public static class OnScreenKeyboard
    {
        /// <summary>
        /// OSK가 떠 있지 않으면 띄운다. 이미 떠 있으면 아무것도 하지 않는다(창이 하나 더 생기지 않게).
        /// 실패해도 예외를 올리지 않는다 — 키보드가 안 떠도 화면 작업은 계속돼야 한다.
        /// </summary>
        public static void Show()
        {
            try
            {
                if (Process.GetProcessesByName("osk").Length > 0)
                    return;

                var path = GetOskPath();
                if (!File.Exists(path))
                {
                    AppLog.Warn($"화상 키보드 실행 파일 없음 - {path}");
                    return;
                }

                // osk.exe는 uiAccess 매니페스트를 가진 프로그램이라 ShellExecute로 띄워야 한다(CreateProcess로는 거절된다).
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true })?.Dispose();
            }
            catch (Exception ex)
            {
                AppLog.Warn($"화상 키보드 실행 실패 - {ex.Message}");
            }
        }

        /// <summary>
        /// 64비트 Windows에서 32비트 프로세스가 System32를 부르면 SysWOW64로 돌려지는데, 거기에는 osk.exe가 없거나
        /// "화상 키보드를 시작할 수 없습니다"로 실패한다. 그 경우 Sysnative로 진짜 System32를 가리킨다.
        /// </summary>
        private static string GetOskPath()
        {
            var windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            var system = Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess ? "Sysnative" : "System32";
            return Path.Combine(windows, system, "osk.exe");
        }
    }
}
