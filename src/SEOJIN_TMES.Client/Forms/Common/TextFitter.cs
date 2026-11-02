using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using SEOJIN_TMES.Client.Theme;

namespace SEOJIN_TMES.Client.Forms.Common
{
    /// <summary>다국어로 글자가 길어졌을 때 칸에 맞추는 방식.</summary>
    public enum TextFitMode
    {
        /// <summary>
        /// 한 줄 유지 + 글자 자동 축소. 높이가 한 줄뿐인 버튼 · 상태 표시 · 항목 이름 라벨용 —
        /// 줄바꿈하면 두 번째 줄이 칸 밖으로 잘려 오히려 뜻이 사라진다.
        /// </summary>
        Shrink,

        /// <summary>
        /// 줄바꿈 우선, 그래도 칸 높이를 넘으면 글자 축소. 여러 줄을 받을 수 있는 안내 문구 상자용 —
        /// 문장은 줄을 바꿔 읽는 편이 글자를 줄이는 것보다 현장 거리에서 잘 읽힌다.
        /// </summary>
        Wrap
    }

    /// <summary>
    /// 다국어(ko/en/vi)로 길이가 달라지는 문구를 칸 크기에 맞춘다 — 넘치면 글자를 줄이고, 들어가면 원래 크기로 되돌린다.
    ///
    /// 출발점은 <see cref="ScreenScaler"/>가 배율만 적용한 글꼴이다(배율로 한 번, 문구 길이로 한 번 줄어든다).
    /// 문구가 바뀌거나(TextChanged) 칸 크기가 바뀌거나(SizeChanged) 배율이 바뀌면(<see cref="ScreenScaler.Rescaled"/>) 다시 맞춘다.
    /// 아무리 줄여도 <see cref="MinShrinkRatio"/> 아래로는 줄이지 않는다 — 그보다 좁으면 잘리는 편이 읽기에 낫다.
    ///
    /// 컨트롤 종류별 권장(POP 화면 기준):
    ///  - 버튼 · 상태 표시(공정 상태 · 작업자) · 항목 이름 라벨 · 체크박스 → <see cref="TextFitMode.Shrink"/>
    ///  - 안내 문구 상자(lblMessage)                                    → <see cref="TextFitMode.Wrap"/>
    ///  - 그리드 머리글 → 줄바꿈, 셀 → 말줄임 + 힌트(<see cref="AppTheme.GridTextFit"/>)
    ///  - 탭 머리글 → 탭 폭이 글자에 맞춰 늘어나므로 손대지 않는다
    /// </summary>
    public sealed class TextFitter : IDisposable
    {
        /// <summary>배율 글꼴 대비 최대 축소 비율. 0.7 = 30%까지만 줄인다.</summary>
        private const float MinShrinkRatio = 0.7F;

        /// <summary>한 번에 줄이는 글자 크기(pt). 계산값으로 바로 가지 않고 실제로 재 보면서 내려간다.</summary>
        private const float ShrinkStep = 0.25F;

        /// <summary>CheckEdit의 체크 상자와 글자 사이 몫(px, 배율 1.0 기준).</summary>
        private const int CheckGlyphWidth = 22;

        /// <summary>WinForms Button(Flat)의 테두리 · 글자 안쪽 여백 몫(px, 배율 1.0 기준).</summary>
        private const int ButtonChrome = 4;

        /// <summary>LabelControl 아이콘과 글자 사이 간격(px, 배율 1.0 기준).</summary>
        private const int ImageTextGap = 4;

        private const TextFormatFlags SingleLineFlags = TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix;
        private const TextFormatFlags WrapFlags = TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix;

        private readonly ScreenScaler _scaler;
        private readonly Dictionary<Control, TextFitMode> _targets = new Dictionary<Control, TextFitMode>();

        public TextFitter(ScreenScaler scaler)
        {
            _scaler = scaler ?? throw new ArgumentNullException(nameof(scaler));
            _scaler.Rescaled += Scaler_Rescaled;
        }

        /// <summary>
        /// 문구를 칸에 맞출 컨트롤을 등록하고 바로 한 번 맞춘다.
        /// 크기가 고정된 컨트롤만 받는다 — 자동 크기 LabelControl은 글자를 줄이면 칸도 같이 줄어
        /// 다시 넘친 것으로 재어져 최소 크기까지 끝없이 줄어든다(실측). 그런 라벨은 AutoSizeMode = None으로 둘 것.
        /// </summary>
        public void Register(TextFitMode mode, params Control[] controls)
        {
            foreach (var control in controls)
            {
                if (control is LabelControl label && label.AutoSizeMode != LabelAutoSizeMode.None)
                    throw new ArgumentException($"자동 크기 라벨은 글자 맞춤 대상이 될 수 없다: {control.Name}", nameof(controls));

                if (_targets.ContainsKey(control))
                    continue;

                _targets[control] = mode;
                control.TextChanged += Target_Changed;
                control.SizeChanged += Target_Changed;
                Fit(control);
            }
        }

        /// <summary>등록한 컨트롤을 모두 다시 맞춘다(언어를 바꿔 문구를 한꺼번에 갈아 끼운 뒤 등).</summary>
        public void FitAll()
        {
            foreach (var control in _targets.Keys)
                Fit(control);
        }

        public void Dispose()
        {
            _scaler.Rescaled -= Scaler_Rescaled;

            foreach (var control in _targets.Keys)
            {
                control.TextChanged -= Target_Changed;
                control.SizeChanged -= Target_Changed;
            }

            _targets.Clear();
        }

        private void Scaler_Rescaled(object sender, EventArgs e)
        {
            FitAll();
        }

        private void Target_Changed(object sender, EventArgs e)
        {
            // 배율을 맞추는 도중의 크기 변화는 중간 상태다 — 끝나면 Rescaled에서 한꺼번에 맞춘다.
            if (_scaler.IsApplying)
                return;

            if (sender is Control control)
                Fit(control);
        }

        private void Fit(Control control)
        {
            // 스케일러가 기준 글꼴을 잡기 전에 줄이면, 줄인 글꼴이 기준으로 저장되어 배율마다 두 번 줄어든다.
            if (!_scaler.IsAttached || control.IsDisposed || !_targets.TryGetValue(control, out var mode))
                return;

            var full = _scaler.ScaledFontOf(control);
            var available = AvailableSize(control);
            var text = control.Text;

            if (string.IsNullOrEmpty(text) || available.Width <= 0 || available.Height <= 0 || Fits(text, full, available, mode))
            {
                SetFont(control, full);
                return;
            }

            // 넘친 비율만큼 한 번에 내려 본 뒤, 실제로 들어갈 때까지 조금씩 더 줄인다(측정과 실제 그리기의 차이를 메운다).
            var minSize = Math.Max(AppTheme.MinScaledFontSize, full.Size * MinShrinkRatio);
            var size = Math.Max(minSize, full.Size * EstimateRatio(text, full, available, mode));

            var font = AppTheme.ResizedFont(full, size);
            while (size > minSize && !Fits(text, font, available, mode))
            {
                size = Math.Max(minSize, size - ShrinkStep);
                font = AppTheme.ResizedFont(full, size);
            }

            // 줄인 글꼴은 Dispose하지 않는다 — 이유는 ScreenScaler.ApplyFactor 주석 참고(GC가 거둔다).
            SetFont(control, font);
        }

        private void SetFont(Control control, Font font)
        {
            if (!ReferenceEquals(CurrentFont(control), font))
                _scaler.SetControlFont(control, font);
        }

        private static Font CurrentFont(Control control)
        {
            if (control is BaseEdit edit)
                return edit.Properties.Appearance.Font;
            if (control is BaseStyleControl styled)
                return styled.Appearance.Font;
            return control.Font;
        }

        private static bool Fits(string text, Font font, Size available, TextFitMode mode)
        {
            if (mode == TextFitMode.Wrap)
                return TextRenderer.MeasureText(text, font, new Size(available.Width, int.MaxValue), WrapFlags).Height <= available.Height;

            return TextRenderer.MeasureText(text, font, Size.Empty, SingleLineFlags).Width <= available.Width;
        }

        /// <summary>한 줄이면 폭 비율, 여러 줄이면 면적 비율의 제곱근(글자 크기가 줄면 줄 수와 줄 길이가 함께 준다).</summary>
        private static float EstimateRatio(string text, Font font, Size available, TextFitMode mode)
        {
            if (mode == TextFitMode.Wrap)
            {
                var needed = TextRenderer.MeasureText(text, font, new Size(available.Width, int.MaxValue), WrapFlags);
                return (float)Math.Sqrt((double)available.Height / Math.Max(1, needed.Height));
            }

            var width = TextRenderer.MeasureText(text, font, Size.Empty, SingleLineFlags).Width;
            return (float)available.Width / Math.Max(1, width);
        }

        /// <summary>글자가 실제로 쓸 수 있는 칸 — 테두리 · 안쪽 여백 · 아이콘 · 체크 상자 몫을 뺀다.</summary>
        private Size AvailableSize(Control control)
        {
            var size = control.ClientSize - control.Padding.Size;

            if (control is ButtonBase button)
            {
                size.Width -= _scaler.Scale(ButtonChrome);

                // 아이콘은 안쪽 여백 바로 뒤(왼쪽)에, 글자는 남은 칸의 가운데에 그려진다 —
                // 가운데 글자가 아이콘과 겹치지 않으려면 양쪽에서 아이콘 폭만큼 뺀다.
                if (button.Image != null)
                    size.Width -= 2 * button.Image.Width;
            }
            else if (control is CheckEdit)
            {
                size.Width -= _scaler.Scale(CheckGlyphWidth);
            }
            else if (control is LabelControl label && label.ImageOptions.Image != null)
            {
                // 아이콘은 안쪽 여백 바로 뒤, 글자는 그 오른쪽에 붙는다.
                size.Width -= label.ImageOptions.Image.Width + _scaler.Scale(ImageTextGap);
            }

            return size;
        }
    }
}
