using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.Utils.Animation;
using DevExpress.Utils.Svg;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraWaitForm;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Theme;

namespace SEOJIN_TMES.Client.Forms.Common
{
    /// <summary>
    /// 조회·처리 중에 띄우는 대기 표시(점 스피너 + "잠시만 기다려 주세요 / 처리 중..." + [취소]).
    ///
    /// 상자는 DevExpress <see cref="PanelControl"/>, 스피너와 두 줄 문구는
    /// <see cref="ProgressPanel"/>(DevExpress의 WaitForm이 쓰는 바로 그 컨트롤),
    /// [취소]는 <see cref="HyperlinkLabelControl"/>이다.
    ///
    /// <see cref="ProgressPanel"/>은 <b>보이는 동안에만</b> 프레임을 돌리므로 타이머를 따로 두지 않는다.
    /// 예전에는 점 8개를 직접 그리며 125ms마다 <c>Invalidate()</c>를 불렀는데,
    /// 그 반복 다시 그리기가 화면 깜빡임의 원인이었다(<see cref="BusyOverlay"/>의 덮개 주석 참고).
    ///
    /// 이 컨트롤은 <b>상자 하나</b>일 뿐이고, 화면 위에 얹고 입력을 막는 일은
    /// <see cref="BusyOverlay"/>가 한다.
    /// </summary>
    internal sealed class BusyIndicator : PanelControl
    {
        /// <summary>상자 크기. 첨부 디자인 비율(가로로 긴 직사각형)을 유지하면서 한 단계 키운 값이다.</summary>
        public const int BoxWidth = 320;
        public const int BoxHeight = 108;

        private const int DotCount = 8;
        private const int RingDiameter = 46;

        /// <summary>점 하나의 크기. ProgressPanel은 이 이미지 크기를 그대로 점 크기로 쓴다.</summary>
        private const int DotSize = 10;

        /// <summary>스피너와 문구 사이 간격.</summary>
        private const int RingToTextGap = 18;

        private const int CancelRightMargin = 14;
        private const int CancelBottomMargin = 8;

        /// <summary>
        /// 아래쪽에 [취소]만 두는 띠의 높이.
        /// 스피너 패널을 상자 전체에 깔면 그 위에 얹은 링크가 가려질 수 있어 영역을 아예 나눠 둔다.
        /// </summary>
        private const int CancelStripHeight = 24;

        /// <summary>진행률 막대 — [취소] 띠에 얹는다. 업로드처럼 진행 정도를 알 수 있는 처리에서만 보인다.</summary>
        private const int BarSideMargin = 16;
        private const int BarHeight = 8;
        private const int BarToCancelGap = 10;

        private readonly ProgressPanel _progress;
        private readonly HyperlinkLabelControl _cancel;
        private readonly ProgressBarControl _bar;

        /// <summary>[취소]를 눌렀을 때.</summary>
        public event EventHandler CancelRequested;

        /// <summary>취소를 제공할지. 되돌릴 수 없는 처리(저장 등)에서는 false로 둔다.</summary>
        public bool Cancelable
        {
            get => _cancel.Visible;
            set => _cancel.Visible = value;
        }

        public BusyIndicator()
        {
            Size = new Size(BoxWidth, BoxHeight);
            AppTheme.CardPanel(this);

            _progress = CreateProgressPanel();
            Controls.Add(_progress);
            PlaceProgressPanel();

            _cancel = CreateCancelLink();
            Controls.Add(_cancel);
            PlaceCancelLink();

            _bar = CreateProgressBar();
            Controls.Add(_bar);
        }

        /// <summary>
        /// 진행률을 보여 준다. 처음 부를 때 막대가 나타나고, 문구 둘째 줄(처리 중...)이 <paramref name="description"/>으로 바뀐다.
        /// 스피너는 그대로 돈다 — 막대가 한동안 멈춰 있어도(서버가 저장 중) 화면이 굳은 게 아님을 보여 준다.
        /// </summary>
        public void SetProgress(int percent, string description)
        {
            if (!_bar.Visible)
            {
                PlaceProgressBar();
                _bar.Visible = true;
            }

            _bar.Position = Math.Max(0, Math.Min(100, percent));

            if (description != null)
                _progress.Description = description;
        }

        /// <summary>
        /// 시안 포인트 색의 얇은 막대. 스킨을 끄는 이유는 ProgressPanel과 같다(카드 흰색과 어긋난 회색 바탕이 생긴다).
        /// </summary>
        private static ProgressBarControl CreateProgressBar()
        {
            var bar = new ProgressBarControl();
            bar.Visible = false;
            bar.Properties.Minimum = 0;
            bar.Properties.Maximum = 100;
            bar.Properties.ShowTitle = false;
            bar.Properties.ProgressViewStyle = ProgressViewStyle.Solid;
            bar.Properties.BorderStyle = BorderStyles.NoBorder;
            bar.Properties.LookAndFeel.UseDefaultLookAndFeel = false;
            bar.Properties.LookAndFeel.Style = LookAndFeelStyle.Flat;
            bar.Properties.StartColor = AppTheme.Indigo;
            bar.Properties.EndColor = AppTheme.Indigo;
            bar.Properties.Appearance.BackColor = AppTheme.Blend(AppTheme.Indigo, AppTheme.Card, 0.15f);
            bar.Properties.Appearance.Options.UseBackColor = true;
            return bar;
        }

        /// <summary>[취소] 띠 세로 가운데. [취소]가 보이면 그 왼쪽까지만 쓴다.</summary>
        private void PlaceProgressBar()
        {
            int right = _cancel.Visible ? _cancel.Left - BarToCancelGap : ClientSize.Width - BarSideMargin;
            int top = ClientSize.Height - CancelStripHeight + (CancelStripHeight - BarHeight) / 2;

            _bar.Bounds = new Rectangle(BarSideMargin, top, Math.Max(0, right - BarSideMargin), BarHeight);
            _bar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        }

        /// <summary>
        /// 스피너 + 두 줄 문구. 카드 안에 들어가므로 스킨이 제 배경을 칠하지 못하게 막는다
        /// (그대로 두면 카드의 흰색과 어긋난 회색 사각형이 상자 안에 생긴다).
        /// </summary>
        private static ProgressPanel CreateProgressPanel()
        {
            var panel = new ProgressPanel();
            panel.BorderStyle = BorderStyles.NoBorder;

            panel.LookAndFeel.UseDefaultLookAndFeel = false;
            panel.LookAndFeel.Style = LookAndFeelStyle.Flat;
            panel.Appearance.BackColor = AppTheme.Card;
            panel.Appearance.Options.UseBackColor = true;

            // 예전 손그림과 같은 모양 — 점 8개가 원형으로 돈다.
            panel.WaitAnimationType = WaitingAnimatorType.Ring;
            panel.RingAnimationDiameter = RingDiameter;
            panel.AnimationElementCount = DotCount;

            // 기본 점은 검정이다. 시안의 포인트 색을 쓰려면 점 그림을 직접 넘기는 수밖에 없다.
            panel.AnimationElementImage = AppIcons.Get(AppIcons.SpinnerDot, AppTheme.Indigo)
                .Render(new Size(DotSize, DotSize), null);
            panel.AnimationToTextDistance = RingToTextGap;
            panel.ContentAlignment = ContentAlignment.MiddleCenter;

            panel.Caption = Language.Common_PleaseWait;
            panel.Description = Language.Common_Loading;

            panel.AppearanceCaption.Font = AppTheme.BodyBoldFont;
            panel.AppearanceCaption.ForeColor = AppTheme.TextPrimary;
            panel.AppearanceCaption.Options.UseFont = true;
            panel.AppearanceCaption.Options.UseForeColor = true;

            panel.AppearanceDescription.Font = AppTheme.SmallFont;
            panel.AppearanceDescription.ForeColor = AppTheme.TextMuted;
            panel.AppearanceDescription.Options.UseFont = true;
            panel.AppearanceDescription.Options.UseForeColor = true;

            return panel;
        }

        private HyperlinkLabelControl CreateCancelLink()
        {
            var link = new HyperlinkLabelControl();
            link.Text = Language.Common_Cancel;
            link.Visible = false;   // Cancelable 기본값이 false다

            link.Appearance.Font = AppTheme.SmallFont;
            link.Appearance.ForeColor = AppTheme.Indigo;
            link.Appearance.Options.UseFont = true;
            link.Appearance.Options.UseForeColor = true;

            link.AppearanceHovered.ForeColor = AppTheme.Rose;
            link.AppearanceHovered.Options.UseForeColor = true;

            link.Click += OnCancelClicked;
            return link;
        }

        /// <summary>[취소] 띠를 뺀 나머지를 스피너가 쓴다(테두리 두께를 감안해 ClientSize 기준).</summary>
        private void PlaceProgressPanel()
        {
            _progress.Bounds = new Rectangle(0, 0, ClientSize.Width, ClientSize.Height - CancelStripHeight);
            _progress.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        }

        /// <summary>
        /// 우측 아래에 놓는다. 자동 크기 라벨이라 <b>Text를 넣은 뒤</b>에 폭이 정해지므로
        /// 생성자 끝에서 한 번 잡고, 이후에는 Anchor가 따라가게 둔다.
        /// </summary>
        private void PlaceCancelLink()
        {
            _cancel.Location = new Point(
                ClientSize.Width - _cancel.Width - CancelRightMargin,
                ClientSize.Height - _cancel.Height - CancelBottomMargin);

            _cancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        }

        private void OnCancelClicked(object sender, EventArgs e) =>
            CancelRequested?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// 애니메이션을 돌린다/멈춘다.
        /// <see cref="ProgressPanel"/>은 화면에 보이는 동안에만 프레임을 돌리므로 표시 여부로 제어한다 —
        /// 감춰 둔 채로 돌면 CPU만 쓴다.
        /// </summary>
        public void StartAnimation() => _progress.Visible = true;

        public void StopAnimation() => _progress.Visible = false;
    }
}
