using System.Windows.Forms;
using System.Collections.Generic;
using SEOJIN_TMES.Client.Common.Logging;

namespace SEOJIN_TMES.Client.Forms.Common
{
    /// <summary>
    /// 공통 메시지 창 진입점 — 화면 코드는 <c>MessageBox</c>/<c>XtraMessageBox</c> 대신 이 클래스만 호출한다.
    /// 실제 UI는 공통 유저컨트롤(<see cref="AppMessagePanel"/>)이고, 이 클래스는 그것을
    /// <see cref="AppMessageForm"/>으로 감싸 모달로 띄우는 얇은 껍데기다.
    ///
    /// 사용 예:
    ///   AppMessageBox.Warn(this, ex.Message);                       // 확인 1개
    ///   if (!AppMessageBox.Confirm(this, "이동하시겠습니까?")) return; // 확인/취소
    ///
    /// 모든 메시지는 콘솔 로그(AppLog)에도 남기므로, 사용자가 무엇을 봤는지 로그로 추적할 수 있다.
    /// </summary>
    public static class AppMessageBox
    {
        /// <summary>일반 안내(확인 1개).</summary>
        public static void Info(IWin32Window owner, string message, string title = null) =>
            Show(owner, message, title, AppMessageKind.Info, AppMessageButtons.Ok);

        /// <summary>성공 안내(확인 1개).</summary>
        public static void Success(IWin32Window owner, string message, string title = null) =>
            Show(owner, message, title, AppMessageKind.Success, AppMessageButtons.Ok);

        /// <summary>주의 안내(확인 1개) — 조회 실패처럼 업무 흐름은 유지되는 경우.</summary>
        public static void Warn(IWin32Window owner, string message, string title = null) =>
            Show(owner, message, title, AppMessageKind.Warning, AppMessageButtons.Ok);

        /// <summary>오류 안내(확인 1개).</summary>
        public static void Error(IWin32Window owner, string message, string title = null) =>
            Show(owner, message, title, AppMessageKind.Error, AppMessageButtons.Ok);

        /// <summary>확인/취소 질문. [확인]을 누르면 true.</summary>
        public static bool Confirm(IWin32Window owner, string message, string title = null) =>
            Show(owner, message, title, AppMessageKind.Question, AppMessageButtons.OkCancel) == DialogResult.OK;

        /// <summary>메시지 창을 띄우고 선택 결과를 돌려준다(닫기 = Cancel).</summary>
        public static DialogResult Show(IWin32Window owner, string message, string title, AppMessageKind kind, AppMessageButtons buttons)
        {
            AppLog.Info($"[MSG/{kind}] {Flatten(title)}{Flatten(message)}");

            var safeOwner = LiveOwner(owner);
            RestoreIfMinimized(safeOwner);

            // 조회·처리 중이면 화면에 대기 표시가 떠 있다. 그대로 두면
            // "조회 결과가 없습니다" 같은 안내 뒤에서 계속 돌아가 아직 처리 중인 것처럼 보인다.
            // 안내를 띄우는 동안만 감췄다가 되돌린다(작업이 끝났으면 곧 사라진다).
            var hiddenVeils = HideBusyVeils(safeOwner);
            try
            {
                using (var form = new AppMessageForm())
                {
                    form.Initialize(message, title, kind, buttons);
                    form.CenterOn(safeOwner);
                    return form.ShowDialog(safeOwner);
                }
            }
            finally
            {
                foreach (var veil in hiddenVeils)
                {
                    if (!veil.IsDisposed)
                        veil.Visible = true;
                }
            }
        }

        /// <summary>
        /// 소유자가 속한 창 전체에서 대기 표시 덮개를 찾아 감춘다(되돌릴 수 있게 목록을 돌려준다).
        ///
        /// 소유자 자신이 아니라 <b>창 전체</b>를 뒤지는 이유: 대기 표시는 조회를 시작한 화면(탭)에 붙는데,
        /// 안내 창의 소유자는 그 화면일 수도, 그 안의 팝업일 수도 있어 어느 쪽이든 걸리게 하려는 것이다.
        /// </summary>
        private static List<Control> HideBusyVeils(IWin32Window owner)
        {
            var hidden = new List<Control>();

            var root = (owner as Control)?.FindForm();
            if (root == null)
                return hidden;

            CollectVisibleVeils(root, hidden);

            foreach (var veil in hidden)
                veil.Visible = false;

            return hidden;
        }

        private static void CollectVisibleVeils(Control parent, List<Control> found)
        {
            foreach (Control child in parent.Controls)
            {
                if (child is BusyOverlay.BusyVeil && child.Visible)
                    found.Add(child);
                else
                    CollectVisibleVeils(child, found);
            }
        }

        /// <summary>
        /// 쓸 수 있는 소유자만 골라 준다.
        ///
        /// 이미 Dispose된 창/화면을 소유자로 넘기면 <c>ShowDialog(owner)</c>가 owner의 Handle을 만들려다
        /// ObjectDisposedException을 던진다. 비동기 조회가 끝나기 전에 사용자가 팝업이나 탭을 닫으면
        /// 늦게 도착한 실패 응답이 딱 이 상황을 만든다.
        ///
        /// 그런 소유자는 버리고 앱의 다른 살아 있는 창을 소유자로 삼는다 — <b>메시지 자체를 삼키지는 않는다.</b>
        /// 각 화면은 이 방어선에 기대지 말고 await 뒤에 IsDisposed를 확인해 안내 여부를 스스로 정할 것
        /// (없어진 화면의 오류를 뒤늦게 띄우는 게 늘 옳은 것은 아니다 — NotificationForm 참고).
        ///
        /// 소유자를 null로 두지 않는 이유: 소유자 없는 모달은 메인 창과 Z 순서가 묶이지 않고
        /// 작업 표시줄에도 없다(ShowInTaskbar=false). 메인 창을 최소화했다 복원하면 메인 창이 그 위를 덮어
        /// 안내 창이 보이지 않는데, 메인 창은 모달 때문에 입력이 막혀 있어 화면 전체가 멈춘 것처럼 된다.
        /// 화면에서 떼어진(부모 없는) 컨트롤도 같은 이유로 소유자로 쓰지 않는다.
        /// </summary>
        private static IWin32Window LiveOwner(IWin32Window owner)
        {
            var control = owner as Control;
            if (owner != null && control == null)
                return owner;

            if (control != null && (control.IsDisposed || control.Disposing))
            {
                AppLog.Warn($"[MSG] 이미 닫힌 소유자({control.GetType().Name}) 대신 앱의 다른 창을 소유자로 안내 창을 띄운다");
                control = null;
            }
            else if (control != null && control.FindForm() == null)
            {
                AppLog.Warn($"[MSG] 화면에서 떼어진 소유자({control.GetType().Name}) 대신 앱의 다른 창을 소유자로 안내 창을 띄운다");
                control = null;
            }

            return control ?? FallbackOwner();
        }

        /// <summary>
        /// 소유자로 쓸 수 있는 앱의 창 — 지금 활성 창, 없으면 가장 나중에 열린 보이는 창(대개 떠 있는 팝업, 그다음 MainForm).
        /// 하나도 없으면(로그인 전 등) null — 그때는 화면 가운데에 단독으로 띄운다.
        /// </summary>
        private static IWin32Window FallbackOwner()
        {
            var active = Form.ActiveForm;
            if (active != null && !active.IsDisposed && active.Visible)
                return active;

            for (int i = Application.OpenForms.Count - 1; i >= 0; i--)
            {
                var form = Application.OpenForms[i];
                if (!form.IsDisposed && form.Visible && !(form is AppMessageForm))
                    return form;
            }

            return null;
        }

        /// <summary>
        /// 소유자 창이 최소화돼 있으면 되돌린다.
        ///
        /// 설비 연결 실패 알림이나 늦게 도착한 조회 실패처럼, 작업자가 창을 내려 둔 사이에도 안내가 뜰 수 있다.
        /// 최소화된 창에 딸린 모달은 작업자 눈에 띄지 않는데 그동안 창은 입력이 막혀 있어,
        /// 다시 올렸을 때 "아무것도 안 눌리는" 상태로 보였다. 모달은 답을 받아야 끝나므로 창을 올려 안내를 보여 준다.
        /// SW_RESTORE는 최소화 전 상태(최대화/보통)로 되돌린다.
        /// </summary>
        private static void RestoreIfMinimized(IWin32Window owner)
        {
            var form = (owner as Control)?.FindForm();
            if (form == null || form.IsDisposed || form.WindowState != FormWindowState.Minimized)
                return;

            AppLog.Info($"[MSG] 최소화된 소유자({form.GetType().Name})를 복원하고 안내 창을 띄운다");
            ShowWindow(form.Handle, SW_RESTORE);
        }

        private const int SW_RESTORE = 9;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ShowWindow(System.IntPtr hWnd, int nCmdShow);

        private static string Flatten(string text) =>
            string.IsNullOrEmpty(text) ? string.Empty : text.Replace("\r\n", " ").Replace("\n", " ") + " ";
    }
}
