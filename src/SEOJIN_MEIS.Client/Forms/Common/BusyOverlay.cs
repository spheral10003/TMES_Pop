using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using SEOJIN_MEIS.Client.Theme;

namespace SEOJIN_MEIS.Client.Forms.Common
{
    /// <summary>
    /// 조회·처리가 끝날 때까지 화면을 덮는 대기 표시.
    ///
    /// <code>
    /// using (var busy = BusyOverlay.Show(this))                  // 되돌릴 수 없는 처리
    ///     await SaveAsync();
    ///
    /// using (var busy = BusyOverlay.Show(this, cancelable: true))// 조회 — [취소] 제공
    ///     var rows = await api.SearchAsync(cond, busy.Token);
    /// </code>
    ///
    /// <b>탭 화면(UserControl)과 팝업(Form) 양쪽에 그대로 쓴다</b> — 대상 컨트롤 위에 덮개를 얹는 방식이라
    /// 어느 쪽이든 자기 영역만 가린다.
    ///
    /// 덮개가 입력을 가로채므로 조회 중에 버튼을 두 번 누르는 것도 함께 막힌다
    /// (각 화면이 btnSearch.Enabled를 끄던 처리는 그대로 두어도 무방하다).
    /// </summary>
    internal sealed class BusyOverlay : IDisposable
    {
        private readonly Control _owner;
        private readonly Panel _veil;
        private readonly BusyIndicator _indicator;
        private readonly CancellationTokenSource _cancellation;
        private bool _disposed;

        /// <summary>
        /// 취소 토큰. <c>cancelable: false</c>면 <see cref="CancellationToken.None"/>이라
        /// 호출부는 조건 없이 그대로 API에 넘기면 된다.
        /// </summary>
        public CancellationToken Token => _cancellation?.Token ?? CancellationToken.None;

        /// <summary>사용자가 [취소]를 눌렀는지. 취소 후 결과 처리를 건너뛸 때 본다.</summary>
        public bool IsCancelled => _cancellation != null && _cancellation.IsCancellationRequested;

        private BusyOverlay(Control owner, bool cancelable)
        {
            _owner = owner;

            if (cancelable)
                _cancellation = new CancellationTokenSource();

            // 덮개: 화면 전체를 채워 입력을 막고, 아래 내용이 비치도록 반투명으로 둔다.
            _veil = new Panel();
            _veil.Dock = DockStyle.Fill;
            _veil.BackColor = Color.FromArgb(90, AppTheme.Surface);

            _indicator = new BusyIndicator();
            _indicator.Cancelable = cancelable;
            _indicator.CancelRequested += (s, e) => Cancel();

            _veil.Controls.Add(_indicator);
            _veil.Resize += (s, e) => CenterIndicator();

            owner.Controls.Add(_veil);
            _veil.BringToFront();
            CenterIndicator();

            _indicator.StartAnimation();
        }

        /// <summary>
        /// 대기 표시를 띄운다. 반환값을 <c>using</c>으로 받아 두면 블록을 벗어날 때 사라진다.
        /// </summary>
        /// <param name="owner">덮을 화면(탭 화면 또는 팝업).</param>
        /// <param name="cancelable">
        /// [취소]를 제공할지. <b>조회처럼 되돌릴 필요가 없는 작업만 true</b>로 둔다 —
        /// 저장·전송처럼 서버 상태를 바꾸는 처리는 중간에 끊으면 무엇이 반영됐는지 알 수 없다.
        /// </param>
        public static BusyOverlay Show(Control owner, bool cancelable = false)
        {
            if (owner == null || owner.IsDisposed)
                return null;

            return new BusyOverlay(owner, cancelable);
        }

        private void CenterIndicator()
        {
            _indicator.Location = new Point(
                Math.Max(0, (_veil.Width - _indicator.Width) / 2),
                Math.Max(0, (_veil.Height - _indicator.Height) / 2));
        }

        private void Cancel()
        {
            if (_cancellation == null || _cancellation.IsCancellationRequested)
                return;

            _cancellation.Cancel();

            // 취소를 누른 뒤에도 응답이 올 때까지 시간이 걸릴 수 있다.
            // 덮개는 그대로 두되(그 사이 조작을 막는다) 취소 링크만 없애 두 번 눌리지 않게 한다.
            _indicator.Cancelable = false;
            _indicator.Invalidate();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _indicator.StopAnimation();

            // 화면이 이미 닫혔을 수 있다(조회 중 탭/팝업을 닫는 경우) — 그때는 정리할 것도 없다.
            if (!_owner.IsDisposed)
                _owner.Controls.Remove(_veil);

            _veil.Dispose();       // 자식(_indicator)도 함께 정리된다
            _cancellation?.Dispose();
        }
    }
}
