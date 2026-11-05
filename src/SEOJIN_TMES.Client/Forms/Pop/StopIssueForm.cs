using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SEOJIN_TMES.Client.ApiClient;
using SEOJIN_TMES.Client.Common.Extensions;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Forms.Common;
using SEOJIN_TMES.Client.Theme;

namespace SEOJIN_TMES.Client.Forms.Pop
{
    /// <summary>
    /// &lt;공정 정지 사유&gt; 팝업 — POP 본 화면의 [중지]에서 연다(<c>PopScreen.GetStopReason</c>).
    ///
    /// 중지 사유(<see cref="PopProcessStopType"/>)를 <b>카드 타일(<see cref="CardTile"/>)</b>로 깔아 하나를 고르게 한다 —
    /// 위에 작게 코드(PD01 등), 가운데 크게 사유 명(Description). 고르면 [확인]이 켜진다.
    /// 배치는 &lt;알람 조회 / 등록&gt;(<see cref="AdminAlarmForm"/>)과 같고, 색만 이 창 고유의 강조 색(<see cref="AppTheme.StopIssue"/>)을 쓴다.
    ///
    /// 일반 .NET Form이며 Windows 제목 줄 대신 pnlTitle(패널) + lblTitle(라벨)이 제목 줄 역할을 한다.
    /// VS 디자이너 호환을 위해 매개변수 없는 생성자만 두고, 실제 내용은 Initialize()로 넣는다.
    /// </summary>
    public partial class StopIssueForm : Form
    {
        // 카드 순서 = 이 배열 순서. SelectedIndex로 enum 값(PD01~)을 되찾을 때 같은 배열을 쓴다.
        private readonly PopProcessStopType[] _stopTypes = (PopProcessStopType[])Enum.GetValues(typeof(PopProcessStopType));

        private readonly Padding _tileMargin = new Padding(0, 0, 10, 10);

        /// <summary>
        /// 카드 한 장의 최소 폭. 한 줄에 들어갈 카드 수는 목록 폭을 이 값으로 나눠 정한다 —
        /// 창을 넓히면 한 줄 장수가 늘고, 좁히면 줄어들며 다음 줄로 넘어간다.
        /// </summary>
        private const int MinTileWidth = 170;

        /// <summary>
        /// 창을 줄일 수 있는 한계. 이보다 작으면 카드가 한 줄에 2장 아래로 떨어지고
        /// 비고 입력란 위로 카드가 한 줄도 다 안 보인다.
        /// </summary>
        private readonly Size _minimumFormSize = new Size(520, 480);

        private int _selectedIndex = -1;

        /// <summary>[확인] 시 고른 카드의 인덱스(<see cref="PopProcessStopType"/> 선언 순서). 취소면 -1.</summary>
        public int SelectedIndex { get; private set; } = -1;

        /// <summary>[확인] 시 고른 중지 사유 — ToString()이 서버로 보낼 코드(PD01~PD09)다.</summary>
        public PopProcessStopType StopType { get; private set; }

        /// <summary>[확인] 시 txtRemark 내용.</summary>
        public string Remark { get; private set; }

        public StopIssueForm()
        {
            InitializeComponent();
        }

        /// <summary>문구 · 색 · 창틀을 입히고 중지 사유 카드를 깐다.</summary>
        public void Initialize()
        {
            Text = Language.StopIssue_Title;
            lblTitle.Text = Language.StopIssue_Title;
            lblCodeTitle.Text = Language.StopIssue_Code;
            lblRemarkTitle.Text = Language.StopIssue_Remark;
            btnOK.Text = Language.Common_Confirm;
            btnCancel.Text = Language.Common_Cancel;

            ApplyTheme();
            DialogChrome.Apply(this, pnlTitle, lblClose);
            DialogChrome.FitPopupSize(this);
            DialogChrome.EnableResizeGrip(pnlGrip);
            MinimumSize = _minimumFormSize;
            ShowCodeTiles();
        }

        private void ApplyTheme()
        {
            AppTheme.TitleBar(pnlTitle, lblTitle, AppTheme.StopIssue);
            AppTheme.TitleBarClose(lblClose, AppTheme.StopIssue);
            AppTheme.StandardButton(btnCancel, keepFont: true);
            AppTheme.StandardButton(btnOK, keepFont: true);
            pnlBody.BackColor = AppTheme.Surface;
            pnlFooter.BackColor = AppTheme.Surface;
            pnlGrip.BackColor = AppTheme.Surface;

            foreach (var label in new[] { lblCodeTitle, lblRemarkTitle })
            {
                label.ForeColor = AppTheme.TextPrimary;
                label.Font = AppTheme.SectionFont;
            }

            txtRemark.BackColor = AppTheme.Card;
            txtRemark.ForeColor = AppTheme.TextPrimary;
            txtRemark.Font = AppTheme.BodyFont;
        }

        /// <summary>부모 창(없으면 화면) 가운데에 띄운다.</summary>
        public void CenterOn(IWin32Window owner)
        {
            DialogChrome.CenterOn(this, owner);   // 부모가 최소화돼 있어도 화면 안에 놓는다
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  카드 — 중지 사유
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>중지 사유 카드 — 위에 작게 코드(PD01), 가운데 크게 사유 명(자재부족). 사유 명이 길면 줄을 바꾼다.</summary>
        private void ShowCodeTiles()
        {
            flowCodes.SuspendLayout();
            while (flowCodes.Controls.Count > 0)
                flowCodes.Controls[0].Dispose();   // Controls.Clear()는 Dispose하지 않아 핸들·글꼴이 남는다

            for (var i = 0; i < _stopTypes.Length; i++)
            {
                var tile = new CardTile();
                tile.Size = new Size(MinTileWidth, CardTile.MinHeight);
                tile.Margin = _tileMargin;
                tile.Accent = AppTheme.StopIssue;
                tile.Tag = i;
                tile.Caption = _stopTypes[i].ToString();
                tile.Title = _stopTypes[i].GetDescription();
                tile.WrapTitle = true;
                tile.Selected = i == _selectedIndex;
                tile.Click += CodeTile_Click;
                flowCodes.Controls.Add(tile);
            }
            LayoutCodeTiles();
            flowCodes.ResumeLayout();

            UpdateOkButton();
        }

        /// <summary>창 크기가 바뀌면 카드의 폭 · 높이를 다시 맞춘다.</summary>
        private void flowCodes_SizeChanged(object sender, EventArgs e) => LayoutCodeTiles();

        /// <summary>
        /// 카드 크기 — 한 줄 장수는 목록 폭을 <see cref="MinTileWidth"/>로 나눠 정하고, 그 폭으로 목록을 고르게 나눈다.
        /// 높이는 사유 명이 줄바꿈된 만큼 늘리되, 같은 줄 카드는 그중 가장 높은 카드에 맞춰 줄이 들쭉날쭉하지 않게 한다.
        ///
        /// 폭은 세로 스크롤바 자리를 항상 비워 두고 계산한다 — 스크롤바가 생겼다 없어질 때마다 폭이 바뀌면
        /// 높이가 다시 바뀌고, 그 때문에 스크롤바가 다시 바뀌는 식으로 배치가 흔들린다(AdminAlarmForm과 같은 이유).
        /// </summary>
        private void LayoutCodeTiles()
        {
            var tiles = flowCodes.Controls.OfType<CardTile>().ToList();
            if (tiles.Count == 0)
                return;

            var available = flowCodes.Width - SystemInformation.VerticalScrollBarWidth - 1;
            var perRow = Math.Max(1, Math.Min(tiles.Count, available / (MinTileWidth + _tileMargin.Horizontal)));
            var width = Math.Max(1, available / perRow - _tileMargin.Horizontal);

            flowCodes.SuspendLayout();
            for (var start = 0; start < tiles.Count; start += perRow)
            {
                var row = tiles.Skip(start).Take(perRow).ToList();
                var height = row.Max(t => t.GetPreferredHeight(width));
                foreach (var tile in row)
                    tile.Size = new Size(width, height);
            }
            flowCodes.ResumeLayout();
        }

        private void CodeTile_Click(object sender, EventArgs e)
        {
            _selectedIndex = (int)((CardTile)sender).Tag;
            foreach (Control control in flowCodes.Controls)
            {
                if (control is CardTile tile)
                    tile.Selected = ReferenceEquals(tile, sender);
            }
            UpdateOkButton();
        }

        /// <summary>사유를 골라야 확인할 수 있다 — 사유 없이 중지되면 비가동 집계가 안 된다.</summary>
        private void UpdateOkButton()
        {
            btnOK.Enabled = _selectedIndex >= 0;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  확인 / 취소
        // ─────────────────────────────────────────────────────────────────────────

        private void btnOK_Click(object sender, EventArgs e)
        {
            var index = _selectedIndex;
            if (index < 0 || index >= _stopTypes.Length)
            {
                // 사유 없이 중지되면 비가동 집계가 안 되므로 창을 유지한다.
                AppMessageBox.Warn(this, Language.StopIssue_SelectCode, Language.StopIssue_Title);
                return;
            }

            SelectedIndex = index;
            StopType = _stopTypes[index];
            Remark = txtRemark.Text?.Trim();

            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>하단 [취소]와 제목 띠의 X 공용.</summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void lblClose_MouseEnter(object sender, EventArgs e) =>
            lblClose.BackColor = AppTheme.TitleBarCloseHover(AppTheme.StopIssue);

        private void lblClose_MouseLeave(object sender, EventArgs e) =>
            lblClose.BackColor = AppTheme.StopIssue;
    }
}
