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
    /// 로그인 직후 접속할 워크스테이션을 고르는 화면. 로그인 창 맨 위에 덮어 띄운다.
    ///
    /// 고르기는 알람 등록 팝업과 같은 <b>카드 타일(<see cref="CardTile"/>)</b>로 한다.
    /// 워크스테이션에는 등급이 없어 모든 카드를 같은 색으로 두고, 코드 · 이름 · 라인/부서만 보여 준다.
    /// [확인]은 카드를 골라야 켜지고, [취소]하면 null을 돌려줘 로그인 창이 정보 입력 화면으로 돌아간다.
    ///
    /// 문구는 로그인 창이 정해 둔 스레드 문화권(로그인 언어 콤보)을 그대로 따른다.
    /// </summary>
    public partial class LoginStationControl : XtraUserControl, IThemedScreen
    {
        private readonly Size _tileSize = new Size(212, 96);
        private readonly Padding _tileMargin = new Padding(0, 0, 10, 10);

        /// <summary>[확인]이면 고른 워크스테이션, [취소]나 창 닫힘이면 null로 끝난다.</summary>
        private TaskCompletionSource<WorkstationInfo> _selection;
        private WorkstationInfo _selected;

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
            btnConfirm.Text = Language.Common_Confirm;
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
        }

        /// <summary>
        /// 워크스테이션 카드를 채우고 사용자가 [확인] 또는 [취소]를 누를 때까지 기다린다.
        /// </summary>
        /// <returns>고른 워크스테이션. 취소하면 null.</returns>
        public Task<WorkstationInfo> ShowStations(IList<WorkstationInfo> stations)
        {
            _selection = new TaskCompletionSource<WorkstationInfo>();
            _selected = null;

            ClearTiles();
            flowStations.SuspendLayout();
            foreach (var station in stations)
            {
                var tile = new CardTile();
                tile.Size = _tileSize;
                tile.Margin = _tileMargin;
                tile.Accent = AppTheme.Indigo;
                tile.Tag = station;
                tile.Caption = station.WorkstationCode;
                tile.Title = station.WorkstationName;
                tile.SubText = SubText(station);
                tile.Click += StationTile_Click;
                tile.DoubleClick += StationTile_DoubleClick;
                flowStations.Controls.Add(tile);
            }
            flowStations.ResumeLayout();

            UpdateConfirmButton();
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

        private void StationTile_Click(object sender, EventArgs e)
        {
            _selected = ((CardTile)sender).Tag as WorkstationInfo;
            foreach (Control control in flowStations.Controls)
            {
                if (control is CardTile tile)
                    tile.Selected = ReferenceEquals(tile, sender);
            }
            UpdateConfirmButton();
        }

        /// <summary>카드를 두 번 누르면 고르고 바로 [확인]한 것으로 본다.</summary>
        private void StationTile_DoubleClick(object sender, EventArgs e)
        {
            StationTile_Click(sender, e);
            btnConfirm_Click(sender, e);
        }

        private void UpdateConfirmButton()
        {
            btnConfirm.Enabled = _selected != null;
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if (_selected == null)
                return;

            _selection?.TrySetResult(_selected);
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
