using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SEOJIN_TMES.Client.ApiClient;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Forms.Common;
using SEOJIN_TMES.Client.Theme;

namespace SEOJIN_TMES.Client.Forms.Pop
{
    /// <summary>
    /// &lt;알람 조회 / 등록&gt; 팝업 — POP 본 화면의 [알림] 버튼(btnAdminAlarm)으로 연다.
    ///
    /// 여기서 "조회"는 발생한 알람 이력이 아니라 <b>등록해 둔 알람 코드 정의</b>(<c>md/abnormal-alarm-types</c>)다.
    /// 창이 뜨면 회사 코드 + 사용 여부(Y)로 자동 조회하고, 고르기는 전부 <b>카드 타일(<see cref="CardTile"/>)</b>로 한다.
    ///  1) [알람 유형] 타일 6장(대분류 — EQP/QLT/MAT/PRD/EHS/SYS) 중 하나를 누르면
    ///  2) 그 유형에 연결된 [알람 코드] 타일이 깔린다(위에 등급, 가운데 알람 코드, 아래 알람 명).
    /// 유형과 알람 코드를 모두 골라야 [등록]이 켜지고, 누르면 alrmType · alrmTypeCd를 서버로 보낸다.
    /// 실패하면 창을 닫지 않고 안내만 해서, 고른 그대로 다시 시도할 수 있게 한다.
    ///
    /// 일반 .NET Form이며 Windows 제목 줄 대신 pnlTitle(패널) + lblTitle(라벨)이 제목 줄 역할을 한다.
    /// VS 디자이너 호환을 위해 매개변수 없는 생성자만 두고, 실제 내용은 Initialize()로 넣는다.
    /// </summary>
    public partial class AdminAlarmForm : Form
    {
        // 알람 유형(대분류) 코드는 서버 스펙으로 고정된 6가지다. 코드는 그대로 쓰고 표시 문구만 다국어로 바꾼다.
        private readonly string[] _alarmTypes = { "EQP", "QLT", "MAT", "PRD", "EHS", "SYS" };

        private readonly Size _typeTileSize = new Size(182, CardTile.MinHeight);
        private readonly Padding _tileMargin = new Padding(0, 0, 10, 10);

        /// <summary>알람 코드 카드는 창 폭과 상관없이 한 줄에 4장 — 폭은 목록 폭에서 나눠 구한다(<see cref="LayoutCodeTiles"/>).</summary>
        private const int CodeTilesPerRow = 4;

        /// <summary>
        /// 창을 줄일 수 있는 한계. 이보다 작으면 알람 유형 카드가 세 줄로 넘어가 알람 코드 목록이 한 줄도 안 남는다.
        /// (폭 820이면 유형 카드가 한 줄에 4장 → 두 줄, 높이 620이면 그 아래 알람 코드 카드가 한 줄 이상 보인다.)
        /// </summary>
        private readonly Size _minimumFormSize = new Size(820, 620);

        private ApiClientFactory _apiClient;
        private PopProcessKey _key;
        private List<AbnormalAlarmTypeDto> _alarms = new List<AbnormalAlarmTypeDto>();

        private string _selectedType;
        private AbnormalAlarmTypeDto _selectedAlarm;

        public AdminAlarmForm()
        {
            InitializeComponent();
        }

        /// <summary>조회·등록에 쓸 API와, 알람을 올릴 지금 공정의 키를 받아 화면을 준비한다.</summary>
        public void Initialize(ApiClientFactory apiClient, PopProcessKey key)
        {
            _apiClient = apiClient;
            _key = key;

            Text = Language.AdminAlarm_Title;
            lblTitle.Text = Language.AdminAlarm_Title;
            lblTypeTitle.Text = Language.AdminAlarm_Type;
            lblCodeTitle.Text = Language.AdminAlarm_ListTitle;
            btnSend.Text = Language.AdminAlarm_Send;
            btnClose.Text = Language.Common_Close;

            ApplyTheme();
            DialogChrome.Apply(this, pnlTitle, lblClose);
            DialogChrome.EnableResizeGrip(pnlGrip);
            MinimumSize = _minimumFormSize;
            ShowTypeTiles();
        }

        private void ApplyTheme()
        {
            AppTheme.TitleBar(pnlTitle, lblTitle, AppTheme.Amber);
            AppTheme.TitleBarClose(lblClose, AppTheme.Amber);
            AppTheme.DialogFlatButton(btnClose, AppTheme.Amber);
            AppTheme.DialogFlatButton(btnSend, AppTheme.Amber);
            pnlBody.BackColor = AppTheme.Surface;
            pnlFooter.BackColor = AppTheme.Surface;
            pnlGrip.BackColor = AppTheme.Surface;

            foreach (var label in new[] { lblTypeTitle, lblCodeTitle })
            {
                label.ForeColor = AppTheme.TextPrimary;
                label.Font = AppTheme.SectionFont;
            }
        }

        /// <summary>알람 유형 · 등급 코드의 표시 문구. 모르는 코드는 서버가 준 코드를 그대로 보여 준다.</summary>
        private string CodeText(string code)
        {
            switch (code)
            {
                case "EQP": return Language.AlarmType_EQP;
                case "QLT": return Language.AlarmType_QLT;
                case "MAT": return Language.AlarmType_MAT;
                case "PRD": return Language.AlarmType_PRD;
                case "EHS": return Language.AlarmType_EHS;
                case "SYS": return Language.AlarmType_SYS;
                case "P1": return Language.AlarmGrade_P1;
                case "P2": return Language.AlarmGrade_P2;
                case "P3": return Language.AlarmGrade_P3;
                case "P4": return Language.AlarmGrade_P4;
                default: return code ?? string.Empty;
            }
        }

        /// <summary>부모 창(없으면 화면) 가운데에 띄운다.</summary>
        public void CenterOn(IWin32Window owner)
        {
            var ownerForm = (owner as Control)?.FindForm();
            var bounds = ownerForm != null ? ownerForm.Bounds : Screen.PrimaryScreen.WorkingArea;

            Location = new Point(
                bounds.Left + ((bounds.Width - Width) / 2),
                bounds.Top + ((bounds.Height - Height) / 2));
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  조회 — 창이 뜰 때 자동으로 한 번
        // ─────────────────────────────────────────────────────────────────────────

        private async void AdminAlarmForm_Shown(object sender, EventArgs e)
        {
            try
            {
                await GetAlarmTypes();
            }
            catch (Exception ex)
            {
                ServerLog.Unexpected("POP 알람 코드 조회", ex);
            }
        }

        /// <summary>지금 공정의 회사 코드로, 사용 중인(useYn=Y) 알람 코드 정의를 전부 읽는다.</summary>
        private async Task GetAlarmTypes()
        {
            var query = new AbnormalAlarmTypeQuery();
            query.CorpCode = _key?.CorpCode;
            query.UseYn = "Y";

            List<AbnormalAlarmTypeDto> alarms;
            try
            {
                alarms = await _apiClient.PopApiClient.GetAbnormalAlarmTypes(query);
            }
            catch (ApiException ex)
            {
                AppLog.Warn($"[POP] 알람 코드 조회 실패 - {ex.Message}");
                if (IsDisposed || Disposing) return;

                AppMessageBox.Warn(this, ex.Message, Language.AdminAlarm_Title);
                return;
            }

            if (IsDisposed || Disposing) return;

            _alarms = alarms ?? new List<AbnormalAlarmTypeDto>();
            ShowTypeTiles();

            if (_alarms.Count == 0)
                AppMessageBox.Warn(this, Language.AdminAlarm_NoTypes, Language.AdminAlarm_Title);
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  타일 — 알람 유형(대분류) → 알람 코드
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>[알람 유형] 타일 6장 — 위에 코드(EQP 등), 가운데 크게 유형 명(설비 등).</summary>
        private void ShowTypeTiles()
        {
            ClearTiles(flowTypes);
            foreach (var type in _alarmTypes)
            {
                var tile = CreateTile(_typeTileSize, AppTheme.AlarmTypeColor(type));
                tile.Tag = type;
                tile.Caption = type;
                tile.Title = CodeText(type);
                tile.Selected = type == _selectedType;
                tile.Click += TypeTile_Click;
                flowTypes.Controls.Add(tile);
            }

            ShowCodeTiles();
        }

        private void TypeTile_Click(object sender, EventArgs e)
        {
            _selectedType = (string)((CardTile)sender).Tag;
            MarkSelected(flowTypes, sender);
            ShowCodeTiles();
        }

        /// <summary>
        /// 고른 유형에 연결된 [알람 코드] 타일 — 유형 타일과 같은 배치로, 위에 작게 "알람 코드 / 등급 명", 가운데 크게 알람 명.
        /// 상단 띠는 등급 색이라 급한 알람이 먼저 눈에 들어온다. 유형을 바꾸면 고른 알람 코드는 비운다.
        ///
        /// 제목(lblCodeTitle)과 목록(flowCodes)은 유형을 고르기 전에도 숨기지 않는다(목록만 빈다).
        /// 창이 뜰 때 숨겨 둔 도킹 컨트롤을 나중에 Visible=true로 바꾸면 WinForms가 그 컨트롤을 z순서 맨 앞으로 올려,
        /// Fill인 목록이 제목 자리까지 차지해 카드 상단 32px가 제목에 가려졌다(실측: 둘 다 Y=156).
        /// </summary>
        private void ShowCodeTiles()
        {
            ClearTiles(flowCodes);
            _selectedAlarm = null;
            lblCodeTitle.Text = Language.AdminAlarm_ListTitle;

            var type = _selectedType;
            if (type != null)
            {
                var codes = _alarms.Where(a => a.AlarmType == type).ToList();
                if (codes.Count == 0)
                    lblCodeTitle.Text = Language.AdminAlarm_ListTitle + "  ·  " + Language.AdminAlarm_NoTypes;

                flowCodes.SuspendLayout();
                foreach (var alarm in codes)
                {
                    var tile = CreateTile(new Size(1, CardTile.MinHeight), AppTheme.AlarmGradeColor(alarm.AlarmGrade));
                    tile.Tag = alarm;
                    // 위쪽 작은 글자: "알람 코드 / 등급 명"(예: ALM001 / 긴급). 등급은 코드(P1~P4)가 아니라 명칭으로 보인다.
                    tile.Caption = alarm.AlarmTypeCode + " / " + CodeText(alarm.AlarmGrade);
                    tile.Title = alarm.AlarmTypeName;
                    tile.WrapTitle = true;
                    tile.Click += CodeTile_Click;
                    flowCodes.Controls.Add(tile);
                }
                LayoutCodeTiles();
                flowCodes.ResumeLayout();
            }

            UpdateSendButton();
        }

        /// <summary>창 크기가 바뀌면 알람 코드 카드의 폭 · 높이를 다시 맞춘다.</summary>
        private void flowCodes_SizeChanged(object sender, EventArgs e) => LayoutCodeTiles();

        /// <summary>
        /// 알람 코드 카드 크기 — 폭은 목록 폭을 4등분하고, 높이는 알람 명이 줄바꿈된 만큼 늘린다.
        /// 같은 줄 4장은 그중 가장 높은 카드에 맞춰 높이를 통일해 줄이 들쭉날쭉하지 않게 한다.
        ///
        /// 폭은 세로 스크롤바 자리를 항상 비워 두고 계산한다 — 스크롤바가 생겼다 없어질 때마다 폭이 바뀌면
        /// 높이가 다시 바뀌고, 그 때문에 스크롤바가 다시 바뀌는 식으로 배치가 흔들린다.
        /// </summary>
        private void LayoutCodeTiles()
        {
            var tiles = flowCodes.Controls.OfType<CardTile>().ToList();
            if (tiles.Count == 0)
                return;

            var available = flowCodes.Width - SystemInformation.VerticalScrollBarWidth - 1;
            var width = Math.Max(1, available / CodeTilesPerRow - _tileMargin.Horizontal);

            flowCodes.SuspendLayout();
            for (var start = 0; start < tiles.Count; start += CodeTilesPerRow)
            {
                var row = tiles.Skip(start).Take(CodeTilesPerRow).ToList();
                var height = row.Max(t => t.GetPreferredHeight(width));
                foreach (var tile in row)
                    tile.Size = new Size(width, height);
            }
            flowCodes.ResumeLayout();
        }

        private void CodeTile_Click(object sender, EventArgs e)
        {
            _selectedAlarm = ((CardTile)sender).Tag as AbnormalAlarmTypeDto;
            MarkSelected(flowCodes, sender);
            UpdateSendButton();
        }

        /// <summary>유형과 알람 코드를 모두 골라야 등록할 수 있다.</summary>
        private void UpdateSendButton()
        {
            btnSend.Enabled = _selectedType != null && _selectedAlarm != null;
        }

        private CardTile CreateTile(Size size, Color accent)
        {
            var tile = new CardTile();
            tile.Size = size;
            tile.Margin = _tileMargin;
            tile.Accent = accent;
            return tile;
        }

        /// <summary>같은 줄에서 누른 타일 하나만 고른 상태로 둔다.</summary>
        private void MarkSelected(FlowLayoutPanel flow, object clicked)
        {
            foreach (Control control in flow.Controls)
            {
                if (control is CardTile tile)
                    tile.Selected = ReferenceEquals(tile, clicked);
            }
        }

        /// <summary>타일을 비운다. Controls.Clear()는 컨트롤을 Dispose하지 않아 핸들·글꼴이 남으므로 하나씩 Dispose한다.</summary>
        private void ClearTiles(FlowLayoutPanel flow)
        {
            flow.SuspendLayout();
            while (flow.Controls.Count > 0)
                flow.Controls[0].Dispose();
            flow.ResumeLayout();
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  등록
        // ─────────────────────────────────────────────────────────────────────────

        private async void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                await SetAlarm();
            }
            catch (Exception ex)
            {
                ServerLog.Unexpected("POP 알람 등록", ex);
            }
        }

        /// <summary>
        /// 고른 알람(alrmType · alrmTypeCd)을 지금 공정에 등록한다. 성공하면 OK로 닫고(완료 안내는 호출한 화면이 띄운다),
        /// 실패하면 안내만 하고 창은 유지한다.
        /// </summary>
        private async Task SetAlarm()
        {
            var alarm = _selectedAlarm;
            if (_selectedType == null || alarm == null)
            {
                AppMessageBox.Warn(this, Language.AdminAlarm_SelectAlarm, Language.AdminAlarm_Title);
                return;
            }

            var request = new AbnormalAlarmRequest();
            request.alrmType = alarm.AlarmType;
            request.alrmTypeCd = alarm.AlarmTypeCode;

            btnSend.Enabled = false;
            try
            {
                await _apiClient.PopApiClient.SetAbnormalAlarm(_key, request);
            }
            catch (ApiException ex)
            {
                AppLog.Warn($"[POP] 알람 등록 실패 - {request.alrmType}/{request.alrmTypeCd} : {ex.Message}");
                if (IsDisposed || Disposing) return;

                AppMessageBox.Warn(this, ex.Message, Language.AdminAlarm_Title);
                return;
            }
            finally
            {
                if (!IsDisposed && !Disposing)
                    UpdateSendButton();
            }

            AppLog.Info($"[POP] 알람 등록 - {_key.WorkOrderNo}/{_key.SerialNo} {request.alrmType}/{request.alrmTypeCd}");
            if (IsDisposed || Disposing) return;

            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>하단 [닫기]와 제목 띠의 X 공용.</summary>
        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void lblClose_MouseEnter(object sender, EventArgs e) =>
            lblClose.BackColor = AppTheme.TitleBarCloseHover(AppTheme.Amber);

        private void lblClose_MouseLeave(object sender, EventArgs e) =>
            lblClose.BackColor = AppTheme.Amber;
    }
}
