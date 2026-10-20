using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SEOJIN_TMES.Client.Common
{
    /// <summary>
    /// 스크롤 영역(<see cref="XtraScrollableControl"/>) 안의 <see cref="PictureBox"/>에 그림을 띄우고
    /// 확대/축소 · 화면 맞춤 · 원본 크기 · 드래그 이동을 처리한다.
    ///
    /// 원래 PopScreen 안에 필드 · 이벤트 · 계산이 흩어져 있던 것을 모았다 — 도면을 보여 주는 다른 화면에서도
    /// 컨트롤 두 개만 넘기면 같은 조작감을 그대로 쓸 수 있다.
    ///
    /// 동작 규칙:
    ///  - Ctrl + 휠 : 확대/축소(1회 <see cref="ZoomStep"/>배). Ctrl 없이 굴리면 평소처럼 스크롤된다.
    ///  - 왼쪽 버튼 드래그 : 그림 이동(스크롤 위치 이동).
    ///  - 맞춤 모드(<see cref="IsFitMode"/>)에서는 창 크기가 바뀔 때마다 다시 맞춘다.
    ///    직접 배율을 바꾸면(확대/축소/100%) 맞춤 모드가 풀린다.
    ///  - 배율을 바꿔도 화면 가운데에 보이던 지점이 그대로 가운데에 남는다.
    ///
    /// <see cref="SetImage"/>로 받은 Image는 이 클래스가 소유한다(교체 · <see cref="Clear"/> · Dispose 때 해제).
    /// </summary>
    public sealed class ImageView : IDisposable
    {
        public const double ZoomMin = 10;
        public const double ZoomMax = 800;

        /// <summary>버튼/휠 1회당 배율.</summary>
        public const double ZoomStep = 1.25;

        /// <summary>맞춤 배율 계산 시 스크롤 영역 테두리에 닿지 않게 남기는 여백(px).</summary>
        private const int FitMargin = 4;

        private readonly XtraScrollableControl _host;
        private readonly PictureBox _picture;

        private Image _image;
        private double _zoom = 100;
        private bool _fitMode = true;

        // 드래그 이동(팬)용
        private bool _dragging;
        private Point _dragStart;
        private Point _dragScroll;

        private bool _disposed;

        /// <summary>배율이 바뀔 때마다 알린다(배율 표시 라벨 등을 갱신할 때 쓴다).</summary>
        public event EventHandler ZoomChanged;

        public ImageZoomViewer(XtraScrollableControl host, PictureBox picture)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _picture = picture ?? throw new ArgumentNullException(nameof(picture));

            // 크기는 이 클래스가 직접 정한다 — Zoom 모드면 PictureBox가 다시 비율을 맞춰 이중으로 축소된다.
            _picture.SizeMode = PictureBoxSizeMode.StretchImage;

            _host.MouseWheel += View_MouseWheel;
            _host.Resize += Host_Resize;
            _picture.MouseWheel += View_MouseWheel;
            _picture.MouseEnter += Picture_MouseEnter;
            _picture.MouseDown += Picture_MouseDown;
            _picture.MouseMove += Picture_MouseMove;
            _picture.MouseUp += Picture_MouseUp;
        }

        // ────────────────────────────── 상태 ──────────────────────────────

        public bool HasImage => _image != null;

        /// <summary>현재 배율(%).</summary>
        public double Zoom => _zoom;

        /// <summary>창 크기가 바뀌면 자동으로 다시 맞추는 상태인지.</summary>
        public bool IsFitMode => _fitMode;

        // ────────────────────────────── 그림 교체 ──────────────────────────────

        /// <summary>
        /// 그림을 바꾸고 화면에 맞춘다. 이전 그림은 여기서 해제한다.
        /// 넘긴 <paramref name="image"/>는 이 클래스가 소유하므로 호출 측에서 Dispose하지 말 것.
        /// </summary>
        public void SetImage(Image image)
        {
            if (image == null)
            {
                Clear();
                return;
            }

            var old = _image;
            _image = image;
            _picture.Image = image;
            old?.Dispose();

            ZoomToFit();
        }

        /// <summary>그림을 내리고 배율 상태를 처음(맞춤 모드, 100%)으로 되돌린다.</summary>
        public void Clear()
        {
            var old = _image;
            _picture.Image = null;
            _picture.Size = new Size(1, 1);
            _host.AutoScrollMinSize = Size.Empty;
            _image = null;
            old?.Dispose();

            _dragging = false;
            _fitMode = true;
            _zoom = 100;
            OnZoomChanged();
        }

        // ────────────────────────────── 배율 ──────────────────────────────

        public void ZoomIn() => ZoomBy(ZoomStep);

        public void ZoomOut() => ZoomBy(1 / ZoomStep);

        /// <summary>원본 크기(100%).</summary>
        public void ActualSize() => SetZoom(100);

        /// <summary>현재 배율에 <paramref name="factor"/>를 곱한다. 직접 조작이므로 맞춤 모드는 풀린다.</summary>
        public void ZoomBy(double factor) => SetZoom(_zoom * factor);

        /// <summary>지정 배율(%)로 바꾼다. 직접 조작이므로 맞춤 모드는 풀린다.</summary>
        public void SetZoom(double percent)
        {
            if (_image == null) return;

            _fitMode = false;
            ApplyZoom(percent);
        }

        /// <summary>스크롤 영역 안에 그림 전체가 보이도록 배율을 계산한다. 이후 창 크기가 바뀌어도 계속 맞춘다.</summary>
        public void ZoomToFit()
        {
            if (_image == null) return;

            _fitMode = true;

            var r = _host.ClientRectangle;
            double w = Math.Max(1, r.Width - FitMargin);
            double h = Math.Max(1, r.Height - FitMargin);
            ApplyZoom(Math.Min(w / _image.Width, h / _image.Height) * 100.0);
        }

        /// <summary>
        /// 배율을 적용한다. 바꾸기 전 화면 가운데가 그림의 어느 비율 위치였는지 기억해 두었다가,
        /// 바꾼 뒤에도 같은 지점이 가운데에 오도록 스크롤을 옮긴다.
        /// </summary>
        private void ApplyZoom(double percent)
        {
            if (_image == null) return;

            percent = Math.Max(ZoomMin, Math.Min(ZoomMax, percent));

            double fx = 0.5, fy = 0.5;
            if (_picture.Width > 0 && _picture.Height > 0)
            {
                fx = (_host.ClientSize.Width / 2.0 - _picture.Left) / _picture.Width;
                fy = (_host.ClientSize.Height / 2.0 - _picture.Top) / _picture.Height;
                fx = Math.Max(0, Math.Min(1, fx));
                fy = Math.Max(0, Math.Min(1, fy));
            }

            _zoom = percent;
            LayoutImage();

            var sx = (int)(fx * _picture.Width - _host.ClientSize.Width / 2.0);
            var sy = (int)(fy * _picture.Height - _host.ClientSize.Height / 2.0);
            if (_picture.Width > _host.ClientSize.Width || _picture.Height > _host.ClientSize.Height)
                _host.AutoScrollPosition = new Point(Math.Max(0, sx), Math.Max(0, sy));

            OnZoomChanged();
        }

        /// <summary>현재 배율로 PictureBox 크기를 정하고, 화면보다 작은 방향은 가운데에 둔다.</summary>
        private void LayoutImage()
        {
            if (_image == null) return;

            var w = Math.Max(1, (int)Math.Round(_image.Width * _zoom / 100.0));
            var h = Math.Max(1, (int)Math.Round(_image.Height * _zoom / 100.0));

            _host.SuspendLayout();
            try
            {
                _host.AutoScrollMinSize = new Size(w, h);
                _picture.Size = new Size(w, h);

                var cw = _host.ClientSize.Width;
                var ch = _host.ClientSize.Height;
                if (w <= cw || h <= ch)
                {
                    var x = w < cw ? (cw - w) / 2 : _picture.Left;
                    var y = h < ch ? (ch - h) / 2 : _picture.Top;
                    _picture.Location = new Point(x, y);
                }
            }
            finally
            {
                _host.ResumeLayout();
            }
        }

        private void OnZoomChanged() => ZoomChanged?.Invoke(this, EventArgs.Empty);

        // ────────────────────────────── 마우스 · 크기 ──────────────────────────────

        private void View_MouseWheel(object sender, MouseEventArgs e)
        {
            if (_image == null || (Control.ModifierKeys & Keys.Control) != Keys.Control) return;

            ZoomBy(e.Delta > 0 ? ZoomStep : 1 / ZoomStep);

            // 확대/축소에 쓴 휠이 스크롤까지 움직이지 않게 막는다.
            if (e is HandledMouseEventArgs handled)
                handled.Handled = true;
        }

        /// <summary>휠 이벤트는 포커스를 가진 컨트롤로 가므로, 그림 위에 올라오면 스크롤 영역에 포커스를 준다.</summary>
        private void Picture_MouseEnter(object sender, EventArgs e) => _host.Focus();

        private void Picture_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || _image == null) return;

            _dragging = true;
            _dragStart = Control.MousePosition;
            _dragScroll = new Point(-_host.AutoScrollPosition.X, -_host.AutoScrollPosition.Y);
            _picture.Cursor = Cursors.SizeAll;
        }

        private void Picture_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_dragging) return;

            var p = Control.MousePosition;
            _host.AutoScrollPosition = new Point(
                _dragScroll.X - (p.X - _dragStart.X),
                _dragScroll.Y - (p.Y - _dragStart.Y));
        }

        private void Picture_MouseUp(object sender, MouseEventArgs e)
        {
            _dragging = false;
            _picture.Cursor = Cursors.Default;
        }

        /// <summary>맞춤 모드면 다시 맞추고, 아니면 가운데 정렬만 갱신한다.</summary>
        private void Host_Resize(object sender, EventArgs e)
        {
            if (_fitMode) ZoomToFit();
            else LayoutImage();
        }

        // ────────────────────────────── 정리 ──────────────────────────────

        /// <summary>이벤트 연결을 끊고 들고 있던 그림을 해제한다. 화면이 닫힐 때 호출한다.</summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _host.MouseWheel -= View_MouseWheel;
            _host.Resize -= Host_Resize;
            _picture.MouseWheel -= View_MouseWheel;
            _picture.MouseEnter -= Picture_MouseEnter;
            _picture.MouseDown -= Picture_MouseDown;
            _picture.MouseMove -= Picture_MouseMove;
            _picture.MouseUp -= Picture_MouseUp;

            _picture.Image = null;
            _image?.Dispose();
            _image = null;
        }
    }
}
