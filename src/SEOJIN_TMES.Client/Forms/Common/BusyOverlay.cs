using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using SEOJIN_TMES.Client.Theme;

namespace SEOJIN_TMES.Client.Forms.Common
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
        /// <summary>
        /// 덮개 패널. <b>타입을 따로 두는 이유</b>는 화면 트리에서 이것만 골라내기 위해서다 —
        /// <see cref="AppMessageBox"/>가 안내 창을 띄우기 전에 이걸 찾아 잠시 감춘다.
        /// </summary>
        internal sealed class BusyVeil : Panel
        {
            public BusyVeil()
            {
                // 덮개 위에 얹힌 대기 상자가 다시 그려질 때 바탕이 한 번 지워졌다
                // 다시 칠해지는 게 그대로 보이면 깜빡임으로 읽힌다.
                DoubleBuffered = true;
            }
        }

        private readonly Control _owner;
        private readonly BusyVeil _veil;
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

            // 덮개: 화면 전체를 채워 입력을 막는다.
            //
            // 예전에는 알파 90짜리 반투명 색을 썼다. WinForms의 투명 배경은 형제 컨트롤이 아니라
            // <b>부모 배경만</b> 비치므로(PaintTransparentBackground) 보이던 결과는 어차피
            // "부모 배경색에 Surface를 35% 섞은 불투명 색"이었고, 대신 덮개를 다시 그릴 때마다
            // 부모 배경을 덮개 영역에 재합성하는 비용을 치렀다 — 조회 중 화면이 깜빡이던 주된 원인이다.
            // 같은 색을 미리 섞어 불투명으로 깔면 모양은 그대로면서 그 재합성이 사라진다.
            _veil = new BusyVeil();
            _veil.Dock = DockStyle.Fill;
            _veil.BackColor = AppTheme.Blend(AppTheme.Surface, OpaqueBackColorOf(owner), 90f / 255f);

            _indicator = new BusyIndicator();
            _indicator.Cancelable = cancelable;
            _indicator.CancelRequested += (s, e) => Cancel();

            // Anchor를 None으로 두면 부모가 커지거나 줄어도 WinForms가 가운데 비율을 유지해 준다.
            // 그래도 처음 한 번은 직접 맞춰야 하고, 도킹이 늦게 반영되는 경우가 있어 Resize에서도 다시 맞춘다.
            _indicator.Anchor = AnchorStyles.None;

            _veil.Controls.Add(_indicator);
            _veil.Resize += (s, e) => CenterIndicator();

            // 덮개를 넣고 빼는 동안 부모가 도킹 레이아웃을 여러 번 돌리지 않게 묶는다
            // (형제 컨트롤까지 다시 배치되면서 화면 전체가 한 번 더 그려진다).
            owner.SuspendLayout();
            try
            {
                owner.Controls.Add(_veil);
                _veil.BringToFront();
            }
            finally
            {
                owner.ResumeLayout(true);
            }

            // Dock=Fill이 실제 크기로 반영된 뒤에 가운데를 계산해야 한다.
            // Add 직후에는 아직 기본 크기(200x100)인 경우가 있어, 레이아웃을 강제로 끝내고 잰다.
            _veil.PerformLayout();
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

        /// <summary>
        /// 덮개의 <b>보이는 영역</b> 한가운데에 상자를 놓는다.
        /// 화면이 스크롤되는 경우 <c>Width/Height</c>는 내용 전체 크기라 그것으로 계산하면
        /// 상자가 보이는 범위 아래로 내려간다 — 그래서 실제로 보이는 <c>ClientSize</c>를 기준으로 삼는다.
        /// </summary>
        private void CenterIndicator()
        {
            var area = _veil.ClientSize;

            _indicator.Location = new Point(
                Math.Max(0, (area.Width - _indicator.Width) / 2),
                Math.Max(0, (area.Height - _indicator.Height) / 2));
        }

        /// <summary>
        /// 덮개 색을 미리 섞기 위한 <b>불투명한</b> 바탕색.
        /// 화면이 자기 배경을 투명으로 두는 경우가 있어, 불투명한 조상이 나올 때까지 거슬러 올라간다.
        /// </summary>
        private static Color OpaqueBackColorOf(Control control)
        {
            for (var c = control; c != null; c = c.Parent)
            {
                if (c.BackColor.A == 255)
                    return c.BackColor;
            }

            return AppTheme.Surface;
        }

        /// <summary>
        /// 안내 창을 띄우는 동안 잠시 감춘다.
        /// 대기 표시가 남아 있는 채로 "조회 결과가 없습니다" 같은 창이 뜨면 아직 처리 중인 것처럼 보인다.
        /// </summary>
        internal void SetVisible(bool visible)
        {
            if (_disposed || _veil.IsDisposed)
                return;

            _veil.Visible = visible;

            // 다시 보일 때는 애니메이션도 함께 살린다(감춰 둔 동안 타이머를 돌릴 이유가 없다).
            if (visible)
            {
                _veil.BringToFront();
                _indicator.StartAnimation();
            }
            else
            {
                _indicator.StopAnimation();
            }
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
            {
                _owner.SuspendLayout();
                try
                {
                    _owner.Controls.Remove(_veil);
                }
                finally
                {
                    _owner.ResumeLayout(true);
                }
            }

            _veil.Dispose();       // 자식(_indicator)도 함께 정리된다
            _cancellation?.Dispose();
        }
    }
}
