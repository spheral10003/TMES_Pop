using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEOJIN_TMES.Client.Common.Imaging
{
    {
        /// <summary>최소 배율(1.0 = 100%). 기존 PopScreen의 ZoomMin(10%)과 같다.</summary>
        public double MinZoom { get; set; } = 0.10;

    /// <summary>최대 배율. 기존 PopScreen의 ZoomMax(800%)와 같다.</summary>
    public double MaxZoom { get; set; } = 8.0;

    /// <summary>휠 / 버튼 1회당 배율.</summary>
    public double ZoomStep { get; set; } = 1.25;

    /// <summary>화면 맞춤 시 사방에 남길 여백(px). 테두리에 딱 붙으면 잘린 것처럼 보여서 둔다.</summary>
    public int FitMargin { get; set; } = 2;

    public int ImageWidth { get; private set; }
    public int ImageHeight { get; private set; }
    public int ViewportWidth { get; private set; }
    public int ViewportHeight { get; private set; }

    /// <summary>현재 배율(1.0 = 100%).</summary>
    public double Zoom { get; private set; } = 1.0;

    /// <summary>현재 배율(%) — 상태 표시용.</summary>
    public int ZoomPercent => (int)Math.Round(Zoom * 100.0);

    /// <summary>
    /// true면 뷰포트 크기가 바뀔 때마다 자동으로 다시 맞춘다.
    /// 사용자가 직접 확대/축소하면 해제된다(기존 PopScreen의 _fitMode와 같은 의미).
    /// </summary>
    public bool FitMode { get; private set; } = true;

    public double OffsetX { get; private set; }
    public double OffsetY { get; private set; }

    public bool HasImage => ImageWidth > 0 && ImageHeight > 0;

    public double DisplayWidth => ImageWidth * Zoom;
    public double DisplayHeight => ImageHeight * Zoom;

    /// <summary>이미지가 뷰포트보다 커서 드래그로 옮길 여지가 있는지.</summary>
    public bool CanPan => HasImage && (DisplayWidth > ViewportWidth || DisplayHeight > ViewportHeight);

    /// <summary>배율 · 위치가 바뀌었다 — 그리는 쪽은 다시 그리면 된다.</summary>
    public event EventHandler Changed;

    /// <summary>새 이미지를 받으면 항상 화면 맞춤부터 시작한다(기존 ShowImage → ZoomToFit 흐름).</summary>
    public void SetImageSize(int width, int height)
    {
        ImageWidth = Math.Max(0, width);
        ImageHeight = Math.Max(0, height);

        if (!HasImage)
        {
            Reset();
            return;
        }

        FitMode = true;
        ApplyFit();
        OnChanged();
    }

    /// <summary>
    /// 뷰포트 크기 변경. 맞춤 모드면 다시 맞추고, 아니면 화면 중심에 있던 이미지 지점이
    /// 새 중심에 오도록 유지한다(창을 키웠다 줄여도 보던 곳을 잃지 않게).
    /// </summary>
    public void SetViewportSize(int width, int height)
    {
        width = Math.Max(0, width);
        height = Math.Max(0, height);
        if (width == ViewportWidth && height == ViewportHeight) return;

        double centerImageX = (ViewportWidth / 2.0 - OffsetX) / Zoom;
        double centerImageY = (ViewportHeight / 2.0 - OffsetY) / Zoom;

        ViewportWidth = width;
        ViewportHeight = height;
        if (!HasImage) return;

        if (FitMode)
        {
            ApplyFit();
        }
        else
        {
            OffsetX = ViewportWidth / 2.0 - centerImageX * Zoom;
            OffsetY = ViewportHeight / 2.0 - centerImageY * Zoom;
            ClampOffset();
        }
        OnChanged();
    }

    /// <summary>이미지 전체가 보이도록 맞춘다. 이후 창 크기가 바뀌어도 계속 맞춘다.</summary>
    public void ZoomToFit()
    {
        if (!HasImage) return;
        FitMode = true;
        ApplyFit();
        OnChanged();
    }

    /// <summary>원본 크기(100%) — 화면 중심 기준.</summary>
    public void ZoomActual()
    {
        SetZoom(1.0);
    }

    public void ZoomIn()
    {
        ZoomBy(ZoomStep, ViewportWidth / 2.0, ViewportHeight / 2.0);
    }

    public void ZoomOut()
    {
        ZoomBy(1.0 / ZoomStep, ViewportWidth / 2.0, ViewportHeight / 2.0);
    }

    /// <summary>화면 중심을 기준으로 지정 배율로 바꾼다.</summary>
    public void SetZoom(double zoom)
    {
        SetZoomAt(zoom, ViewportWidth / 2.0, ViewportHeight / 2.0);
    }

    /// <summary>
    /// 현재 배율에 <paramref name="factor"/>를 곱한다. 기준점(보통 마우스 위치) 아래의 이미지 픽셀이
    /// 확대 후에도 같은 자리에 남는다 — 기존 구현은 화면 중심 기준이라 휠로 원하는 곳을 키우기 어려웠다.
    /// </summary>
    public void ZoomBy(double factor, double anchorX, double anchorY)
    {
        SetZoomAt(Zoom * factor, anchorX, anchorY);
    }

    /// <summary>지정 배율 + 기준점. 사용자가 직접 조작한 것이므로 맞춤 모드는 해제한다.</summary>
    public void SetZoomAt(double zoom, double anchorX, double anchorY)
    {
        if (!HasImage) return;

        FitMode = false;
        zoom = ClampZoom(zoom);
        if (Math.Abs(zoom - Zoom) < 1e-9) return;

        // 기준점 아래에 있던 이미지 좌표
        double imageX = (anchorX - OffsetX) / Zoom;
        double imageY = (anchorY - OffsetY) / Zoom;

        Zoom = zoom;
        OffsetX = anchorX - imageX * Zoom;
        OffsetY = anchorY - imageY * Zoom;
        ClampOffset();
        OnChanged();
    }

    /// <summary>드래그 이동량(px)만큼 이미지를 옮긴다. 이미지가 화면 밖으로 빠져나가지는 않는다.</summary>
    public void Pan(double dx, double dy)
    {
        if (!CanPan) return;

        double oldX = OffsetX, oldY = OffsetY;
        OffsetX += dx;
        OffsetY += dy;
        ClampOffset();

        if (Math.Abs(oldX - OffsetX) > 1e-9 || Math.Abs(oldY - OffsetY) > 1e-9) OnChanged();
    }

    /// <summary>이미지를 비웠을 때의 초기 상태(기존 ClearImage의 _fitMode = true, _zoom = 100).</summary>
    public void Reset()
    {
        ImageWidth = 0;
        ImageHeight = 0;
        Zoom = 1.0;
        FitMode = true;
        OffsetX = 0;
        OffsetY = 0;
        OnChanged();
    }

    /// <summary>이미지를 그릴 위치 · 크기(뷰포트 좌표).</summary>
    public ImageViewRect GetDisplayRect()
    {
        return new ImageViewRect(OffsetX, OffsetY, DisplayWidth, DisplayHeight);
    }

    /// <summary>뷰포트 좌표 → 이미지 픽셀 좌표(클릭 위치 표시 등에 쓴다).</summary>
    public void ViewToImage(double viewX, double viewY, out double imageX, out double imageY)
    {
        imageX = (viewX - OffsetX) / Zoom;
        imageY = (viewY - OffsetY) / Zoom;
    }

    private void ApplyFit()
    {
        double w = Math.Max(1, ViewportWidth - FitMargin * 2);
        double h = Math.Max(1, ViewportHeight - FitMargin * 2);
        Zoom = ClampZoom(Math.Min(w / ImageWidth, h / ImageHeight));
        ClampOffset();
    }

    private double ClampZoom(double zoom)
    {
        return Math.Max(MinZoom, Math.Min(MaxZoom, zoom));
    }

    /// <summary>
    /// 축마다: 이미지가 뷰포트보다 작으면 가운데 정렬, 크면 가장자리에 빈 공간이 생기지 않게 제한한다.
    /// (기존 LayoutImage의 가운데 배치 + AutoScroll 범위 제한을 한 번에 처리)
    /// </summary>
    private void ClampOffset()
    {
        OffsetX = ClampAxis(OffsetX, DisplayWidth, ViewportWidth);
        OffsetY = ClampAxis(OffsetY, DisplayHeight, ViewportHeight);
    }

    private static double ClampAxis(double offset, double display, double viewport)
    {
        if (display <= viewport) return (viewport - display) / 2.0;
        return Math.Min(0, Math.Max(viewport - display, offset));
    }

    private void OnChanged()
    {
        Changed?.Invoke(this, EventArgs.Empty);
    }
}

/// <summary>UI 비의존 사각형(System.Drawing.RectangleF 대용 — Common은 System.Drawing을 참조하지 않는다).</summary>
public struct ImageViewRect
{
    public ImageViewRect(double x, double y, double width, double height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public double X { get; }
    public double Y { get; }
    public double Width { get; }
    public double Height { get; }
}
}
