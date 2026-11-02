using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraTab;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Theme;

namespace SEOJIN_TMES.Client.Forms.Common
{
    /// <summary>
    /// 화면 하나를 기준 해상도(1920×1080) 대비 비율로 통째로 확대 · 축소한다.
    ///
    /// <b>배율</b> = min(호스트 폭 / 기준 폭, 호스트 높이 / 기준 높이). 호스트는 화면이 Dock=Fill로 붙는 MainForm의 contentHost다.
    ///  - 작은 쪽 배율을 쓰는 이유: 큰 쪽을 쓰면 다른 축이 넘쳐 아래 · 오른쪽이 잘린다.
    ///  - 다만 레터박스(남는 쪽을 빈 띠로 두기)는 하지 않는다. 고정 크기 부분(상단 버튼 줄 · 우측 정보 · 하단 탭)만
    ///    배율대로 줄이고, 남는 공간은 원래처럼 Dock=Fill인 도면 영역이 가져간다 — 1280×1024처럼 세로가 남는 화면에서
    ///    빈 띠 대신 도면이 커진다. 고정 부분은 각 축 배율보다 작거나 같게 줄었으므로 어느 쪽도 넘치지 않는다.
    ///
    /// <b>무엇을</b> 줄이나: 위치 · 크기 · 안쪽 여백(Padding), 글꼴(WinForms Font · DevExpress Appearance),
    /// 버튼 · 라벨 아이콘, 그리드 행 · 머리글 높이와 그리드/탭 글꼴.
    ///
    /// <b>오차가 쌓이지 않게</b>: WinForms <c>Control.Scale(SizeF)</c>는 현재 값에 배율을 곱하고 정수로 반올림하므로,
    /// 창 크기를 바꿀 때마다 부르면 1px씩 어긋남이 쌓인다(그리고 글꼴은 아예 바꾸지 않는다).
    /// 그래서 처음 상태(디자이너 값)를 기준값으로 저장해 두고, 매번 <b>기준값 × 배율</b>을 새로 계산해 넣는다.
    /// 같은 배율이면 몇 번을 다시 맞춰도 결과가 같다.
    ///
    /// 앵커(Right · Bottom)가 걸린 컨트롤은 값을 넣는 순간의 부모 크기로 거리를 기억한다. 그래서 루트부터 차례로
    /// 부모를 먼저 "기준 크기 × 배율"로 맞춘 뒤 자식을 넣고, 마지막에 레이아웃을 풀어 실제 호스트 크기로 늘린다
    /// (디자이너 상태를 배율만큼 줄인 화면을 만든 다음 Dock/Anchor가 평소처럼 늘리는 것과 같다).
    ///
    /// 사용법: 화면 생성자의 <c>InitializeComponent()</c> 직후 만들고(디자이너 값이 그대로일 때 기준을 잡는다),
    /// 테마 · 그리드 구성이 끝난 뒤 <see cref="Attach"/>로 호스트에 붙인다.
    /// 상태에 따라 코드로 바꾸는 크기(접기/펴기 폭 등)는 <see cref="Scale(int)"/>로 환산하고 <see cref="Rescaled"/>에서 다시 맞춘다.
    /// </summary>
    public sealed class ScreenScaler : IDisposable
    {
        /// <summary>
        /// 기준 해상도 1920×1080에서 POP를 띄웠을 때 contentHost의 표시 영역(= 배율 1.0인 크기).
        /// MainForm 기준: 최대화 창은 작업 영역(작업 표시줄 48px 제외 → 1920×1032)에 맞춰지고,
        /// 거기서 사이드바(펼침 240) · 상단 바(34) · 탭 줄(38) · 패널 테두리 · contentHost 여백(12,4,12,12)을 뺀 값이다.
        /// 작업 표시줄을 뺀 높이를 기준으로 잡아, 작업 표시줄이 있든 없든 1920×1080에서는 배율이 1.0이 된다
        /// (폭 쪽 배율이 1.0이고 작은 쪽을 쓰므로). 실제 값은 배율을 바꿀 때 로그(AppLog.Info)로 남긴다.
        /// </summary>
        public static readonly Size ReferenceHostSize = new Size(1648, 936);

        /// <summary>배율의 하한 · 상한. 호스트가 비정상적으로 작거나(최소화 중) 큰 경우 화면이 뭉개지지 않게 막는다.</summary>
        private const float MinFactor = 0.5F;
        private const float MaxFactor = 2.0F;

        /// <summary>이보다 작은 배율 변화는 무시한다 — 창을 끌어 크기를 바꾸는 동안 1px마다 전체를 다시 맞추지 않게.</summary>
        private const float FactorTolerance = 0.002F;

        private readonly Control _root;
        private readonly Size _referenceHostSize;

        /// <summary>루트부터 깊이 우선(부모 먼저) 순서. 이 순서대로 넣어야 앵커 거리가 기준 상태로 계산된다.</summary>
        private readonly List<LayoutBaseline> _layouts = new List<LayoutBaseline>();

        private readonly List<FontBaseline> _fonts = new List<FontBaseline>();
        private readonly List<ImageBaseline> _images = new List<ImageBaseline>();
        private readonly List<GridBaseline> _grids = new List<GridBaseline>();

        /// <summary>컨트롤별 "배율만 적용한" 글꼴. <see cref="TextFitter"/>가 다국어 축소의 출발점으로 쓴다.</summary>
        private readonly Dictionary<Control, Font> _scaledControlFonts = new Dictionary<Control, Font>();

        /// <summary>지금 배율로 만들어 둔 글꼴(기준 글꼴 → 축소 글꼴). 같은 기준 글꼴을 쓰는 컨트롤끼리 공유한다.</summary>
        private Dictionary<Font, Font> _fontCache = new Dictionary<Font, Font>();

        private Control _host;
        private bool _captured;
        private bool _applyPending;

        public ScreenScaler(Control root)
            : this(root, ReferenceHostSize)
        {
        }

        public ScreenScaler(Control root, Size referenceHostSize)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
            _referenceHostSize = referenceHostSize;
            CaptureLayout(_root);
        }

        /// <summary>지금 적용된 배율(기준 해상도 = 1.0).</summary>
        public float Factor { get; private set; } = 1F;

        /// <summary>호스트에 붙어 글꼴 기준값을 잡았는지(<see cref="Attach"/> 이후).</summary>
        public bool IsAttached => _host != null;

        /// <summary>배율을 다시 맞추는 중인지. 이 동안의 크기 · 글자 변경 이벤트는 중간 상태이므로 무시해도 된다.</summary>
        public bool IsApplying { get; private set; }

        /// <summary>
        /// 배율을 다시 맞춘 직후. 위치 · 크기는 디자이너 기준으로 되돌려졌으므로,
        /// 화면이 상태에 따라 바꿔 둔 크기(접힌 패널 등)는 여기서 다시 맞춘다.
        /// </summary>
        public event EventHandler Rescaled;

        /// <summary>디자이너 기준 픽셀 값을 지금 배율로 환산한다.</summary>
        public int Scale(int designValue) => (int)Math.Round(designValue * Factor);

        /// <summary>
        /// 화면을 호스트(contentHost)에 붙이고 지금 크기에 맞춘다. 이후 호스트 크기가 바뀔 때마다 다시 맞춘다.
        /// 글꼴 · 아이콘 · 그리드 기준값은 여기서 잡는다 — 테마와 그리드 구성이 끝난 값이 기준이어야 하기 때문이다.
        /// </summary>
        public void Attach(Control host)
        {
            if (host == null || _host == host)
                return;

            Detach();
            _host = host;
            _host.Resize += Host_Resize;

            if (!_captured)
            {
                foreach (var layout in _layouts)
                    CaptureAppearance(layout.Control);
                _captured = true;
            }

            ApplyFactor(CalculateFactor(), force: true);
        }

        /// <summary>배율만 적용한 글꼴(다국어 축소 전). 기준을 잡지 않은 컨트롤이면 지금 글꼴을 돌려준다.</summary>
        public Font ScaledFontOf(Control control)
        {
            return _scaledControlFonts.TryGetValue(control, out var font) ? font : GetControlFont(control);
        }

        /// <summary>
        /// 컨트롤 종류에 맞는 자리에 글꼴을 넣는다 — DevExpress 컨트롤은 Control.Font가 아니라 Appearance가 실제 글꼴이다.
        /// </summary>
        public void SetControlFont(Control control, Font font)
        {
            if (control is BaseEdit edit)
            {
                edit.Properties.Appearance.Font = font;
                edit.Properties.Appearance.Options.UseFont = true;
            }
            else if (control is BaseStyleControl styled)
            {
                styled.Appearance.Font = font;
                styled.Appearance.Options.UseFont = true;
            }
            else
            {
                control.Font = font;
            }
        }

        public void Dispose()
        {
            Detach();

            // 아이콘 사본 · 글꼴은 해제하지 않는다 — 화면이 닫히는 중에도 컨트롤이 아직 들고 있을 수 있다(GC가 거둔다).
            _fontCache.Clear();
            _scaledControlFonts.Clear();
        }

        private void Detach()
        {
            if (_host == null)
                return;

            _host.Resize -= Host_Resize;
            _host = null;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  배율 계산 · 적용
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// 크기가 바뀔 때마다 바로 맞추지 않고 메시지 루프로 한 번 미룬다 —
        /// 사이드바 접기 · 창 끌기는 Resize를 연달아 보내는데, 그 사이의 중간 크기마다 전체를 다시 맞출 필요는 없다.
        /// </summary>
        private void Host_Resize(object sender, EventArgs e)
        {
            if (_applyPending || _host == null)
                return;

            if (!_host.IsHandleCreated)
            {
                ApplyFactor(CalculateFactor(), force: false);
                return;
            }

            _applyPending = true;
            _host.BeginInvoke(new Action(ApplyPendingFactor));
        }

        private void ApplyPendingFactor()
        {
            _applyPending = false;
            if (_host == null || _root.IsDisposed)
                return;

            ApplyFactor(CalculateFactor(), force: false);
        }

        private float CalculateFactor()
        {
            var size = _host.DisplayRectangle.Size;
            if (size.Width <= 0 || size.Height <= 0)
                return Factor;   // 최소화 등 — 지금 배율을 유지한다

            var factor = Math.Min(
                (float)size.Width / _referenceHostSize.Width,
                (float)size.Height / _referenceHostSize.Height);

            return Math.Max(MinFactor, Math.Min(MaxFactor, factor));
        }

        private void ApplyFactor(float factor, bool force)
        {
            if (_root.IsDisposed || IsApplying)
                return;

            if (!force && Math.Abs(factor - Factor) < FactorTolerance)
                return;

            AppLog.Info($"[화면 배율] {_root.Name} : 호스트 {_host?.DisplayRectangle.Size} / 기준 {_referenceHostSize} → 배율 {factor:0.###}");

            IsApplying = true;
            Factor = factor;

            // 호스트까지 멈춰야 루트 크기를 "기준 × 배율"로 넣는 동안 Dock=Fill이 곧바로 되돌리지 않는다.
            _host?.SuspendLayout();
            foreach (var layout in _layouts)
                layout.Control.SuspendLayout();

            // 이전 배율의 글꼴은 Dispose하지 않고 GC에 맡긴다. DevExpress는 그리기 정보(ViewInfo)에 글꼴을 잠시 들고 있어,
            // 바꾼 직후 해제하면 다음 그리기에서 "매개 변수가 잘못되었습니다"로 죽을 수 있다(AppTheme의 글꼴도 같은 방식이다).
            _fontCache = new Dictionary<Font, Font>();

            try
            {
                foreach (var layout in _layouts)
                    layout.Apply(factor);

                foreach (var font in _fonts)
                    ApplyFont(font);

                foreach (var image in _images)
                    image.Apply(factor);

                foreach (var grid in _grids)
                    ApplyGrid(grid);
            }
            finally
            {
                for (var i = _layouts.Count - 1; i >= 0; i--)
                    _layouts[i].Control.ResumeLayout(false);

                // 자식들은 "기준 × 배율" 상태 그대로 맞물려 있다 — 루트부터 다시 배치하면 Dock/Anchor가 실제 크기로 늘린다.
                _root.PerformLayout();
                _host?.ResumeLayout(true);

                IsApplying = false;
            }

            _root.Invalidate(true);
            Rescaled?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>배율 1.0이면 기준 글꼴 그대로, 아니면 이번 배율로 만든 글꼴(같은 기준 글꼴끼리 공유).</summary>
        private Font ScaledFont(Font baseline)
        {
            if (Math.Abs(Factor - 1F) < FactorTolerance)
                return baseline;

            if (!_fontCache.TryGetValue(baseline, out var scaled))
            {
                scaled = AppTheme.ResizedFont(baseline, baseline.Size * Factor);
                _fontCache[baseline] = scaled;
            }

            return scaled;
        }

        private void ApplyFont(FontBaseline baseline)
        {
            var font = ScaledFont(baseline.Font);

            if (baseline.Appearance != null)
            {
                baseline.Appearance.Font = font;
                baseline.Appearance.Options.UseFont = true;
                return;
            }

            _scaledControlFonts[baseline.Control] = font;
            SetControlFont(baseline.Control, font);
        }

        private void ApplyGrid(GridBaseline grid)
        {
            grid.View.BeginUpdate();
            try
            {
                if (grid.RowHeight > 0)
                    grid.View.RowHeight = Scale(grid.RowHeight);
                if (grid.ColumnPanelRowHeight > 0)
                    grid.View.ColumnPanelRowHeight = Scale(grid.ColumnPanelRowHeight);
            }
            finally
            {
                grid.View.EndUpdate();
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  기준값 저장
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// 위치 · 크기 · 여백의 기준값을 부모 먼저 순서로 저장한다(생성자에서 — 디자이너 값 그대로일 때).
        /// 안쪽 구조를 스스로 관리하는 컨트롤(에디터 · 그리드 · 이미지 뷰어 등)의 자식은 건드리지 않는다.
        /// </summary>
        private void CaptureLayout(Control control)
        {
            // 탭 페이지 크기는 XtraTabControl이 정한다 — 넣어도 덮어쓰이고, 바뀌는 순간 탭이 다시 배치된다.
            if (!(control is XtraTabPage))
                _layouts.Add(new LayoutBaseline(control, control == _root));

            if (!IsLayoutContainer(control))
                return;

            foreach (Control child in control.Controls)
                CaptureLayout(child);
        }

        private bool IsLayoutContainer(Control control)
        {
            return control == _root
                || control is Panel
                || control is PanelControl
                || control is XtraTabControl
                || control is XtraTabPage;
        }

        /// <summary>글꼴 · 아이콘 · 그리드 기준값(<see cref="Attach"/> 때 한 번).</summary>
        private void CaptureAppearance(Control control)
        {
            if (control is GridControl grid)
            {
                CaptureGrid(grid);
                return;
            }

            if (control is XtraTabControl tab)
            {
                CaptureAppearanceFont(tab.AppearancePage.Header, force: true);
                CaptureAppearanceFont(tab.AppearancePage.HeaderActive, force: false);
                CaptureAppearanceFont(tab.AppearancePage.HeaderHotTracked, force: false);
                CaptureAppearanceFont(tab.AppearancePage.HeaderDisabled, force: false);
                return;
            }

            // 컨테이너의 글꼴은 자식에게 물려주는 용도라 건드리지 않는다 — 자식마다 직접 맞춘다.
            if (IsLayoutContainer(control))
                return;

            _fonts.Add(new FontBaseline(control, GetControlFont(control)));

            if (control is ButtonBase button && button.Image != null)
                _images.Add(new ImageBaseline(button));
            else if (control is LabelControl label && label.ImageOptions.Image != null)
                _images.Add(new ImageBaseline(label));
        }

        private void CaptureGrid(GridControl grid)
        {
            foreach (var view in grid.ViewCollection)
            {
                if (!(view is GridView gridView))
                    continue;

                // 행 · 머리글 글꼴이 지정돼 있지 않으면(스킨 기본 글꼴) 그 실제 글꼴을 기준으로 직접 지정한다 —
                // 지정하지 않은 글꼴은 배율을 따라가지 않는다. 나머지 상태(선택 행 등)는 지정된 것만 맞춘다(없으면 행 글꼴을 물려받는다).
                CaptureAppearanceFont(gridView.Appearance.HeaderPanel, force: true);
                CaptureAppearanceFont(gridView.Appearance.Row, force: true);
                foreach (AppearanceObject appearance in gridView.Appearance)
                {
                    if (appearance != gridView.Appearance.HeaderPanel && appearance != gridView.Appearance.Row)
                        CaptureAppearanceFont(appearance, force: false);
                }

                _grids.Add(new GridBaseline(gridView));
            }

            foreach (RepositoryItem item in grid.RepositoryItems)
                CaptureAppearanceFont(item.Appearance, force: false);
        }

        private void CaptureAppearanceFont(AppearanceObject appearance, bool force)
        {
            if (!force && !appearance.Options.UseFont)
                return;

            _fonts.Add(new FontBaseline(appearance, appearance.GetFont()));
        }

        private static Font GetControlFont(Control control)
        {
            if (control is BaseEdit edit)
                return edit.Properties.Appearance.GetFont();
            if (control is BaseStyleControl styled)
                return styled.Appearance.GetFont();
            return control.Font;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  기준값
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>컨트롤 하나의 디자이너 위치 · 크기 · 여백.</summary>
        private sealed class LayoutBaseline
        {
            private readonly Rectangle _bounds;
            private readonly Padding _padding;
            private readonly bool _isRoot;

            public LayoutBaseline(Control control, bool isRoot)
            {
                Control = control;
                _bounds = control.Bounds;
                _padding = control.Padding;
                _isRoot = isRoot;
            }

            public Control Control { get; }

            public void Apply(float factor)
            {
                Control.Padding = new Padding(
                    Round(_padding.Left, factor), Round(_padding.Top, factor),
                    Round(_padding.Right, factor), Round(_padding.Bottom, factor));

                // 루트는 Dock=Fill이라 위치는 의미가 없고, 크기만 "기준 × 배율"로 잠깐 맞춘다(자식 앵커 계산용).
                if (_isRoot)
                {
                    Control.Size = new Size(Round(_bounds.Width, factor), Round(_bounds.Height, factor));
                    return;
                }

                // 왼쪽 · 오른쪽 끝을 각각 반올림한다 — 위치와 폭을 따로 반올림하면 맞붙은 컨트롤 사이에 1px 틈이 생긴다.
                var left = Round(_bounds.Left, factor);
                var top = Round(_bounds.Top, factor);
                var right = Round(_bounds.Right, factor);
                var bottom = Round(_bounds.Bottom, factor);
                Control.Bounds = new Rectangle(left, top, right - left, bottom - top);
            }

            private static int Round(int value, float factor) => (int)Math.Round(value * factor);
        }

        /// <summary>글꼴 기준값 — 컨트롤 자체의 글꼴이거나, DevExpress Appearance 하나의 글꼴이다.</summary>
        private sealed class FontBaseline
        {
            public FontBaseline(Control control, Font font)
            {
                Control = control;
                Font = font;
            }

            public FontBaseline(AppearanceObject appearance, Font font)
            {
                Appearance = appearance;
                Font = font;
            }

            public Control Control { get; }
            public AppearanceObject Appearance { get; }
            public Font Font { get; }
        }

        /// <summary>
        /// 버튼 · 라벨 아이콘 기준값. 원본(디자이너 resx 그림)은 그대로 두고, 배율이 1이 아니면 줄인 사본을 끼운다.
        /// SimpleButton/Button의 Image는 원본 크기로 그려지므로(CLAUDE.md 5번) 크기를 맞춘 그림을 넣어야 한다.
        /// </summary>
        private sealed class ImageBaseline
        {
            private readonly Control _control;
            private readonly Image _original;

            public ImageBaseline(ButtonBase button)
            {
                _control = button;
                _original = button.Image;
            }

            public ImageBaseline(LabelControl label)
            {
                _control = label;
                _original = label.ImageOptions.Image;
            }

            public void Apply(float factor)
            {
                var size = new Size(
                    Math.Max(1, (int)Math.Round(_original.Width * factor)),
                    Math.Max(1, (int)Math.Round(_original.Height * factor)));

                // 이전 사본은 글꼴과 같은 이유로 GC에 맡긴다(라벨 아이콘은 DevExpress가 그리기 정보에 들고 있다).
                SetImage(size == _original.Size ? _original : AppImages.Resized(_original, size));
            }

            private void SetImage(Image image)
            {
                if (_control is ButtonBase button)
                    button.Image = image;
                else if (_control is LabelControl label)
                    label.ImageOptions.Image = image;
            }
        }

        /// <summary>그리드 뷰의 행 · 머리글 높이 기준값(-1 = 글꼴에 따라 자동이라 맞출 필요가 없다).</summary>
        private sealed class GridBaseline
        {
            public GridBaseline(GridView view)
            {
                View = view;
                RowHeight = view.RowHeight;
                ColumnPanelRowHeight = view.ColumnPanelRowHeight;
            }

            public GridView View { get; }
            public int RowHeight { get; }
            public int ColumnPanelRowHeight { get; }
        }
    }
}
