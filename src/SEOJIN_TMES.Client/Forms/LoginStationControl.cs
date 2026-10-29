using DevExpress.XtraEditors;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Forms.Common;
using SEOJIN_TMES.Client.Theme;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SEOJIN_TMES.Client.Forms
{
    /// <summary>
    /// 로그인 직후 접속할 워크스테이션을 고르는 화면. 메인 화면이 뜨면서 흐린 배경 위 가운데에 1200×700으로 띄운다(<see cref="MainForm"/>).
    ///
    /// 고르기는 알람 등록 팝업과 같은 <b>카드 타일(<see cref="CardTile"/>)</b>로 한다.
    /// 워크스테이션에는 등급이 없어 모든 카드를 같은 색으로 두고, 코드 · 이름 · 라인/부서만 보여 준다.
    /// 카드를 누르면 바로 골라지고 화면이 걷힌다(작업장 터치에서 [확인]까지 두 번 누르게 하지 않으려고).
    /// [취소]하면 null을 돌려줘 워크스테이션 없이 메인 화면을 쓴다 — 이때는 공정을 시작할 수 없다.
    ///
    /// 문구는 메인 화면이 적용해 둔 스레드 문화권을 그대로 따른다.
    /// </summary>
    public partial class LoginStationControl : XtraUserControl, IThemedScreen
    {
        /// <summary>
        /// 카드 한 장 크기. 개수에 맞춰 늘이지 않고 고정해 두고, 목록 폭에 들어가는 만큼 한 줄에 놓고 넘치면 다음 줄로 넘긴다.
        /// 1200×700 선택 화면에서 한 줄 3장 · 세 줄(9장)이 스크롤 없이 보이고, 그보다 많으면 세로 스크롤로 본다.
        /// 예전에는 2열 × 2행에 맞춰 카드를 늘였는데, 화면이 커지자 카드가 지나치게 커지고 5장째부터는 스크롤해야 했다.
        /// </summary>
        private static readonly Size TileSize = new Size(360, 150);

        private readonly Padding _tileMargin = new Padding(0, 0, 10, 10);

        /// <summary>카드를 누르면 그 워크스테이션, [취소]나 창 닫힘이면 null로 끝난다.</summary>
        private TaskCompletionSource<WorkstationInfo> _selection;

        public LoginStationControl()
        {
            InitializeComponent();
        }

        /// <summary>디자이너가 만든 UI에 다국어 문구와 테마를 입힌다.</summary>
        public void Init()
        {
            SetTexts();
            ApplyTheme();

            // 고르는 중에 로그인 창을 닫으면 기다리던 쪽이 영영 끝나지 않으므로 취소로 마무리한다.
            Disposed += LoginStationControl_Disposed;
        }

        private void SetTexts()
        {
            lblTitle.Text = Language.Login_StationTitle;
            lblHint.Text = Language.Login_StationHint;
            btnCancel.Text = Language.Common_Cancel;
        }

        public void ApplyTheme()
        {
            BackColor = AppTheme.Surface;
            flowStations.BackColor = AppTheme.Surface;
            pnlFooter.BackColor = AppTheme.Surface;

            lblTitle.ForeColor = AppTheme.TextPrimary;
            lblTitle.Font = AppTheme.TitleFont;
            lblHint.ForeColor = AppTheme.TextMuted;
            lblHint.Font = AppTheme.BodyFont;

            AppTheme.StandardButton(btnCancel, keepFont: true);
        }

        /// <summary>
        /// 워크스테이션 카드를 채우고 사용자가 카드 또는 [취소]를 누를 때까지 기다린다.
        /// </summary>
        /// <returns>고른 워크스테이션. 취소하면 null.</returns>
        public Task<WorkstationInfo> ShowStations(IList<WorkstationInfo> stations)
        {
            _selection = new TaskCompletionSource<WorkstationInfo>();

            ClearTiles();
            flowStations.SuspendLayout();
            foreach (var station in stations)
            {
                var tile = new CardTile();
                tile.Margin = _tileMargin;
                tile.Size = TileSize;
                tile.AutoFitText = true;
                tile.Accent = AppTheme.Indigo;
                tile.Tag = station;
                tile.Caption = station.WorkstationCode;
                tile.Title = station.WorkstationName;
                tile.SubText = SubText(station);
                tile.Click += StationTile_Click;
                flowStations.Controls.Add(tile);
            }
            flowStations.ResumeLayout();

            return _selection.Task;
        }

        /// <summary>카드 아래 보조 문구 — 라인 · 부서 중 서버가 내려준 값만 잇는다.</summary>
        private static string SubText(WorkstationInfo station)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(station.LineName)) parts.Add(station.LineName);
            if (!string.IsNullOrWhiteSpace(station.DepartmentName)) parts.Add(station.DepartmentName);
            return string.Join("  ·  ", parts);
        }

        /// <summary>카드를 누르면 그 워크스테이션으로 바로 끝낸다(기다리던 쪽이 이 화면을 걷어 낸다).</summary>
        private void StationTile_Click(object sender, EventArgs e)
        {
            var tile = (CardTile)sender;
            tile.Selected = true;
            _selection?.TrySetResult(tile.Tag as WorkstationInfo);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _selection?.TrySetResult(null);
        }

        private void LoginStationControl_Disposed(object sender, EventArgs e)
        {
            _selection?.TrySetResult(null);
        }

        /// <summary>카드를 비운다. Controls.Clear()는 컨트롤을 Dispose하지 않아 핸들·글꼴이 남으므로 하나씩 Dispose한다.</summary>
        private void ClearTiles()
        {
            flowStations.SuspendLayout();
            while (flowStations.Controls.Count > 0)
                flowStations.Controls[0].Dispose();
            flowStations.ResumeLayout();
        }
    }
}
