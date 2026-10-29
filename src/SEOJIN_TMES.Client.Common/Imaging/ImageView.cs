using ImageBoxExtensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SEOJIN_TMES.Client.Common.Imaging
{
    /// <summary>
    /// 도구상자의 <see cref="ImageBoxEx"/>(ImageGlass.ImageBox)에 기존 PopScreen과 같은 조작 규칙을 입힌다.
    ///
    /// 배율 계산 · 스크롤 · 드래그 이동 · 마우스 위치 기준 확대는 ImageBox가 직접 하므로,
    /// 예전처럼 PictureBox 크기를 키우고 AutoScroll을 맞추는 계산은 필요 없다. 여기서는 "규칙"만 정한다.
    ///  - Ctrl + 휠 : 확대/축소(1.25배 단계, 10~800%). Ctrl 없이 굴리면 스크롤(Shift면 가로).
    ///  - 왼쪽 버튼 드래그 : 그림 이동(ImageBox의 AutoPan).
    ///  - 맞춤 모드(<see cref="IsFitMode"/>)에서는 창 크기가 바뀔 때마다 다시 맞춘다.
    ///    직접 배율을 바꾸면 맞춤 모드가 풀린다.
    ///
    /// ImageBox는 Image를 바꿔도 이전 그림을 해제하지 않는다 → <see cref="SetImage"/>로 받은 Image는 이 클래스가 소유한다.
    /// </summary>
    public sealed class ImageView : IDisposable
    {
        public const double ZoomMin = 10;
        public const double ZoomMax = 800;

        /// <summary>버튼/휠 1회당 배율.</summary>
        public const double ZoomStep = 1.25;

        private readonly ImageBoxEx _box;
        private Image _image;
        private bool _fitMode = true;
        private bool _disposed;

        /// <summary>배율이 바뀔 때마다 알린다(배율 표시 라벨 등).</summary>
        public event EventHandler ZoomChanged;

        public ImageView(ImageBoxEx box)
        {
            _box = box ?? throw new ArgumentNullException(nameof(box));

            // Fit 모드는 확대/축소 자체를 막으므로 Normal로 두고, 맞춤은 ZoomToFit()으로 직접 한다.
            _box.SizeMode = ImageBoxSizeMode.Normal;
            _box.AutoPan = true;
            _box.AutoCenter = true;
            // 클릭만으로 확대되면 드래그하려다 도면이 커진다.
            _box.AllowClickZoom = false;
            // ZoomIn/ZoomOut은 이 목록의 다음/이전 값으로 이동한다 → 기존 1.25배 단계를 그대로 재현한다.
            _box.ZoomLevels = BuildZoomLevels();
            // 기본값(Client)은 빈 자리에 회색 체크무늬를 깐다 — 도면 바깥은 바탕색(흰색) 그대로 보이게 끈다.
            _box.GridDisplayMode = ImageBoxGridDisplayMode.None;

            _box.MouseWheel += Box_MouseWheel;
            _box.MouseEnter += Box_MouseEnter;
            _box.Resize += Box_Resize;
            _box.Zoomed += Box_Zoomed;
            _box.ZoomChanged += Box_ZoomChanged;
        }

        // ────────────────────────────── 상태 ──────────────────────────────

        public bool HasImage => _image != null;

        /// <summary>현재 배율(%).</summary>
        public double Zoom => _box.Zoom;

        /// <summary>창 크기가 바뀌면 자동으로 다시 맞추는 상태인지.</summary>
        public bool IsFitMode => _fitMode;

        // ────────────────────────────── 그림 교체 ──────────────────────────────

        /// <summary>그림을 바꾸고 화면에 맞춘다. 이전 그림은 여기서 해제한다(호출 측에서 Dispose하지 말 것).</summary>
        public void SetImage(Image image)
        {
            if (image == null)
            {
                Clear();
                return;
            }

            var old = _image;
            _image = image;
            _box.Image = image;
            old?.Dispose();

            ZoomToFit();
        }

        /// <summary>그림을 내리고 처음 상태(맞춤 모드)로 되돌린다.</summary>
        public void Clear()
        {
            var old = _image;
            _box.Image = null;
            _image = null;
            old?.Dispose();

            _fitMode = true;
            OnZoomChanged();
        }

        // ────────────────────────────── 배율 ──────────────────────────────

        public void ZoomIn()
        {
            if (_image == null) return;
            _fitMode = false;
            _box.ZoomIn(true);   // true: 화면 가운데 지점 유지
        }

        public void ZoomOut()
        {
            if (_image == null) return;
            _fitMode = false;
            _box.ZoomOut(true);
        }

        /// <summary>원본 크기(100%).</summary>
        public void ActualSize()
        {
            SetZoom(100);
        }

        /// <summary>지정 배율(%)로 바꾼다. 화면 가운데에 보이던 지점이 그대로 가운데에 남는다.</summary>
        public void SetZoom(double percent)
        {
            if (_image == null) return;
            _fitMode = false;

            // Zoom 속성 setter는 위치를 보존하지 않으므로 ImageBox 내부 PerformZoom과 같은 방식으로 직접 보정한다.
            var center = _box.CenterPoint;
            var pixel = _box.PointToImage(center);
            _box.Zoom = Clamp(percent);
            _box.ScrollTo(pixel, center);
        }

        /// <summary>그림 전체가 보이도록 맞춘다. 이후 창 크기가 바뀌어도 계속 맞춘다.</summary>
        public void ZoomToFit()
        {
            if (_image == null) return;
            _fitMode = true;

            // 확대된 상태에서는 스크롤바가 떠 있어, ImageBox.ZoomToFit이 스크롤바만큼 좁아진 영역으로 배율을 계산한다
            // (그 뒤 스크롤바가 사라지면 그림이 한쪽으로 치우치고 작게 남는다). 가장 작게 줄여 스크롤바를 먼저 걷어 낸 뒤 맞춘다.
            //
            // 스크롤 위치도 직접 0으로 되돌려야 한다. ImageBox는 스크롤바가 사라져도 확대 때 옮겨 둔 AutoScrollPosition을
            // 그대로 들고 있고, 그림을 그 값만큼 밀어 그린다 — 확대 후 Fit하면 그림이 사라지거나 잘린 크기로 보이던 원인.
            // (축소 후 Fit은 스크롤 위치가 원래 0이라 문제가 드러나지 않았다.)
            _box.Zoom = ZoomMin;
            _box.AutoScrollPosition = Point.Empty;
            _box.ZoomToFit();
            _box.AutoScrollPosition = Point.Empty;
            // ImageBox의 하한(1%)은 기존 규칙(10%)보다 낮다 — 아주 큰 도면도 10% 아래로는 내리지 않는다.
            if (_box.Zoom < ZoomMin) _box.Zoom = ZoomMin;
        }

        private static double Clamp(double percent)
        {
            return Math.Max(ZoomMin, Math.Min(ZoomMax, percent));
        }

        /// <summary>100%를 기준으로 1.25배씩 늘리고 줄인 단계(10~800%). ImageBox는 정수 % 목록만 받는다.</summary>
        private static ImageBoxZoomLevelCollection BuildZoomLevels()
        {
            var levels = new SortedSet<int>();
            for (double z = 100; z <= ZoomMax; z *= ZoomStep) levels.Add((int)Math.Round(z));
            for (double z = 100; z >= ZoomMin; z /= ZoomStep) levels.Add((int)Math.Round(z));
            levels.Add((int)ZoomMin);
            levels.Add((int)ZoomMax);
            return new ImageBoxZoomLevelCollection(levels);
        }

        private void OnZoomChanged()
        {
            ZoomChanged?.Invoke(this, EventArgs.Empty);
        }

        // ────────────────────────────── 마우스 · 크기 ──────────────────────────────

        /// <summary>ImageGlass 판 ImageBox는 휠을 직접 처리하지 않는다 — 여기서 확대/스크롤을 나눠 준다.</summary>
        private void Box_MouseWheel(object sender, MouseEventArgs e)
        {
            if (_image == null) return;

            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                _fitMode = false;
                _box.ZoomWithMouseWheel(e.Delta, e.Location);   // 마우스 아래 지점을 기준으로 확대
            }
            else
            {
                _box.ScrollWithMouseWheel(e.Delta, (Control.ModifierKeys & Keys.Shift) == Keys.Shift);
            }

            if (e is HandledMouseEventArgs handled) handled.Handled = true;
        }

        /// <summary>휠 이벤트는 포커스를 가진 컨트롤로 가므로, 그림 위에 올라오면 포커스를 준다.</summary>
        private void Box_MouseEnter(object sender, EventArgs e)
        {
            if (!_box.Focused) _box.Focus();
        }

        private void Box_Resize(object sender, EventArgs e)
        {
            if (_fitMode) ZoomToFit();
        }

        /// <summary>키보드 단축키(+/-) 등 ImageBox가 스스로 바꾼 배율도 "직접 조작"으로 보고 맞춤 모드를 푼다.</summary>
        private void Box_Zoomed(object sender, ImageBoxZoomEventArgs e)
        {
            if (e.Source == ImageBoxActionSources.User) _fitMode = false;
        }

        private void Box_ZoomChanged(object sender, EventArgs e)
        {
            // 축소 화면에서 NearestNeighbor(기본값)는 도면 선이 끊겨 보인다. 확대 시에는 픽셀을 그대로 키우는 편이 선명하다.
            _box.InterpolationMode = _box.Zoom < 100
                ? InterpolationMode.HighQualityBicubic
                : InterpolationMode.NearestNeighbor;

            OnZoomChanged();
        }

        // ────────────────────────────── 정리 ──────────────────────────────

        /// <summary>이벤트 연결을 끊고 들고 있던 그림을 해제한다. 화면이 닫힐 때 호출한다.</summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _box.MouseWheel -= Box_MouseWheel;
            _box.MouseEnter -= Box_MouseEnter;
            _box.Resize -= Box_Resize;
            _box.Zoomed -= Box_Zoomed;
            _box.ZoomChanged -= Box_ZoomChanged;

            _box.Image = null;
            _image?.Dispose();
            _image = null;
        }
    }
}
