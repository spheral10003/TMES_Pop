using SEOJIN_TMES.Client.Common.Logging;
using System;
using System.Runtime.InteropServices;

namespace SEOJIN_TMES.Client.Forms.Common
{
    /// <summary>
    /// 프로그램이 떠 있는 동안 화면 보호기 · 모니터 꺼짐 · 절전 진입을 막는다.
    ///
    /// POP는 작업자가 한참 손대지 않아도 공정 상태 · 설비 체결값을 계속 보여 줘야 한다.
    /// 화면 보호기가 뜨면 현장에서 화면을 다시 깨워야 하고, 절전에 들어가면 설비 Modbus 연결까지 끊긴다.
    ///
    /// 시스템 설정(SystemParametersInfo)을 바꾸는 방식은 쓰지 않는다 — 비정상 종료 시 바뀐 설정이 PC에 그대로 남는다.
    /// 이 방식은 프로세스가 끝나면 Windows가 알아서 원래대로 돌려놓는다.
    /// 걸렸는지는 관리자 명령 프롬프트의 <c>powercfg /requests</c> → DISPLAY 항목으로 확인한다.
    /// </summary>
    public static class DisplayKeeper
    {
        [Flags]
        private enum ExecutionState : uint
        {
            ES_SYSTEM_REQUIRED = 0x00000001,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_CONTINUOUS = 0x80000000,
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern ExecutionState SetThreadExecutionState(ExecutionState esFlags);

        /// <summary>
        /// 화면을 계속 켜 두도록 요청한다.
        /// 요청은 호출한 스레드에 묶이므로 프로그램 끝까지 살아 있는 UI 스레드(Main)에서 부를 것.
        /// </summary>
        public static void Keep()
        {
            var previous = SetThreadExecutionState(
                ExecutionState.ES_CONTINUOUS |
                ExecutionState.ES_DISPLAY_REQUIRED |
                ExecutionState.ES_SYSTEM_REQUIRED);

            if (previous == 0)
                AppLog.Warn($"화면 보호기 차단 실패 - SetThreadExecutionState 오류 {Marshal.GetLastWin32Error()}");
            else
                AppLog.Step("화면 보호기 · 절전 차단 시작");
        }

        /// <summary>요청을 거둔다. 이후로는 PC의 화면 보호기 · 전원 설정을 따른다.</summary>
        public static void Release()
        {
            SetThreadExecutionState(ExecutionState.ES_CONTINUOUS);
        }
    }
}
