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
    /// 고르는 것도, 조회 결과를 보여 주는 것도 전부 <b>카드 타일(<see cref="CardTile"/>)</b>로 한다.
    ///  1) [알람 구분] 타일(설비/품질/자재/생산/안전·환경/시스템)을 누르면
    ///  2) 그 구분의 [알람 등급] 타일(P1 긴급 ~ P4 정보)이 생기고, 등급을 누르면
    ///  3) 그 구분 · 등급의 [알람 코드 / 조치 내용] 타일이 깔린다. 하나를 고르고 비고를 적어 [등록]한다.
    ///
    /// 알람 유형 기준정보(<c>md/abnormal-alarm-types</c>)는 창이 뜰 때 한 번(그리고 [조회] 때) 통째로 읽고,
    /// 구분 · 등급 타일의 건수는 그 목록을 묶어 보여 주기만 한다(서버 값을 가공하는 판정·집계가 아니다).
    /// 등록(<c>pop/{sn}/{subRpRef}/abnormal-alarms</c>)까지 이 창이 맡는다 —
    /// 실패하면 창을 닫지 않고 안내만 해서, 고른 내용 그대로 다시 시도할 수 있게 한다.
    ///
    /// 일반 .NET Form이며 Windows 제목 줄 대신 pnlTitle(패널) + lblTitle(라벨)이 제목 줄 역할을 한다.
    /// VS 디자이너 호환을 위해 매개변수 없는 생성자만 두고, 실제 내용은 Initialize()로 넣는다.
    /// </summary>
    public partial class AdminAlarmForm : Form
    {
        // 알람 구분 · 등급 코드는 서버 스펙으로 고정된 값이다. 표시 문구만 다국어로 바꾼다.
        private readonly string[] _alarmTypes = { "EQP", "QLT", "MAT", "PRD", "EHS", "SYS" };
        private readonly string[] _alarmGrades = { "P1", "P2", "P3", "P4" };

        private readonly Size _typeTileSize = new Size(182, 104);
        private readonly Size _gradeTileSize = new Size(240, 104);
        private readonly Size _codeTileSize = new Size(272, 104);
        private readonly Padding _tileMargin = new Padding(0, 0, 10, 10);

        private ApiClientFactory _apiClient;
        private PopProcessKey _key;
        private List<AbnormalAlarmTypeDto> _alarms = new List<AbnormalAlarmTypeDto>();

        private string _selectedType;
        private string _selectedGrade;
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
            lblGradeTitle.Text = Language.AdminAlarm_Grade;
            lblCodeTitle.Text = Language.AdminAlarm_ListTitle;
            lblRemark.Text = Language.Col_Remark;
            btnSearch.Text = Language.Common_Search;
            btnSend.Text = Language.AdminAlarm_Send;
            btnClose.Text = Language.Common_Close;

            ApplyTheme();
            DialogChrome.Apply(this, pnlTitle);
            ShowTypeTiles();
        }

        private void ApplyTheme()
        {
            AppTheme.TitleBar(pnlTitle, lblTitle, AppTheme.Amber);
            pnlBody.BackColor = AppTheme.Surface;
            pnlRemark.BackColor = AppTheme.Surface;
            pnlFooter.BackColor = AppTheme.Surface;

            foreach (var label in new[] { lblTypeTitle, lblGradeTitle, lblCodeTitle, lblRemark })
            {
                label.ForeColor = AppTheme.TextPrimary;
                label.Font = AppTheme.SectionFont;
            }

            AppTheme.Editor(txtRemark);
            btnSearch.Icon = AppIcons.Search;
        }

        /// <summary>알람 구분 · 등급 코드의 표시 문구. 모르는 코드는 서버가 준 코드를 그대로 보여 준다.</summary>
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
        //  조회
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>창이 뜨자마자 한 번 조회해 타일의 건수를 채운다.</summary>
        private async void AdminAlarmForm_Shown(object sender, EventArgs e) => await GetAlarmTypes();

        private async void btnSearch_Click(object sender, EventArgs e) => await GetAlarmTypes();

        /// <summary>
        /// 지금 공정의 회사 코드로 사용 중인(useYn=Y) 알람 유형을 전부 읽는다.
        /// 다시 조회해도 고르던 구분 · 등급은 유지하고 타일만 새 목록으로 다시 깐다.
        /// </summary>
        private async Task GetAlarmTypes()
        {
            var query = new AbnormalAlarmTypeQuery();
            query.CorpCode = _key?.CorpCode;
            query.UseYn = "Y";

            List<AbnormalAlarmTypeDto> alarms;
            btnSearch.Enabled = false;
            try
            {
                alarms = await _apiClient.PopApiClient.GetAbnormalAlarmTypes(query);
            }
            catch (ApiException ex)
            {
                AppLog.Warn($"[POP] 알람 유형 조회 실패 - {ex.Message}");
                if (IsDisposed || Disposing) return;

                AppMessageBox.Warn(this, ex.Message, Language.AdminAlarm_Title);
                return;
            }
            finally
            {
                if (!IsDisposed && !Disposing)
                    btnSearch.Enabled = true;
            }

            if (IsDisposed || Disposing) return;

            _alarms = alarms ?? new List<AbnormalAlarmTypeDto>();
            ShowTypeTiles();

            if (_alarms.Count == 0)
                AppMessageBox.Warn(this, Language.AdminAlarm_NoTypes, Language.AdminAlarm_Title);
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  타일 — 구분 → 등급 → 알람 코드
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// [알람 구분] 타일 6장 — 구분별 알람 코드 건수와, 그 안의 등급별 건수를 보조 문구로 보여 준다.
        /// 고른 구분이 있으면 그 아래 단계(등급 · 코드)도 이어서 다시 그린다.
        /// </summary>
        private void ShowTypeTiles()
        {
            ClearTiles(flowTypes);
            foreach (var type in _alarmTypes)
            {
                var inType = _alarms.Where(a => a.AlarmType == type).ToList();

                var tile = CreateTile(_typeTileSize, AppTheme.AlarmTypeColor(type));
                tile.Tag = type;
                tile.Title = CodeText(type);
                tile.Value = inType.Count.ToString();
                tile.Unit = Language.AdminAlarm_Unit;
                tile.SubText = string.Join(" · ", _alarmGrades.Select(g => g + " " + inType.Count(a => a.AlarmGrade == g)));
                tile.Selected = type == _selectedType;
                tile.Click += TypeTile_Click;
                flowTypes.Controls.Add(tile);
            }

            ShowGradeTiles();
        }

        private void TypeTile_Click(object sender, EventArgs e)
        {
            _selectedType = (string)((CardTile)sender).Tag;
            _selectedGrade = null;
            MarkSelected(flowTypes, sender);
            ShowGradeTiles();
        }

        /// <summary>구분을 고르면 생기는 [알람 등급] 타일 4장(P1 긴급 ~ P4 정보) — 그 구분 안의 등급별 건수.</summary>
        private void ShowGradeTiles()
        {
            ClearTiles(flowGrades);
            var type = _selectedType;
            lblGradeTitle.Visible = type != null;
            flowGrades.Visible = type != null;

            if (type != null)
            {
                foreach (var grade in _alarmGrades)
                {
                    var tile = CreateTile(_gradeTileSize, AppTheme.AlarmGradeColor(grade));
                    tile.Tag = grade;
                    tile.Title = grade + " " + CodeText(grade);
                    tile.Value = _alarms.Count(a => a.AlarmType == type && a.AlarmGrade == grade).ToString();
                    tile.Unit = Language.AdminAlarm_Unit;
                    tile.SubText = CodeText(type) + " (" + type + ")";
                    tile.Selected = grade == _selectedGrade;
                    tile.Click += GradeTile_Click;
                    flowGrades.Controls.Add(tile);
                }
            }

            ShowCodeTiles();
        }

        private void GradeTile_Click(object sender, EventArgs e)
        {
            _selectedGrade = (string)((CardTile)sender).Tag;
            MarkSelected(flowGrades, sender);
            ShowCodeTiles();
        }

        /// <summary>
        /// 등급까지 고르면 깔리는 [알람 코드 / 조치 내용] 타일 — 제목은 알람 코드, 큰 글자는 알람 명(조치 내용).
        /// 목록이 바뀌면 고른 알람도 비운다(새 목록에 없을 수 있다).
        /// </summary>
        private void ShowCodeTiles()
        {
            ClearTiles(flowCodes);
            _selectedAlarm = null;

            var type = _selectedType;
            var grade = _selectedGrade;
            var visible = type != null && grade != null;
            lblCodeTitle.Visible = visible;
            flowCodes.Visible = visible;
            if (type == null || grade == null)
                return;

            var gradeColor = AppTheme.AlarmGradeColor(grade);
            foreach (var alarm in _alarms.Where(a => a.AlarmType == type && a.AlarmGrade == grade))
            {
                var tile = CreateTile(_codeTileSize, gradeColor);
                tile.Tag = alarm;
                tile.Title = alarm.AlarmTypeCode;
                tile.Value = alarm.AlarmTypeName;
                tile.CompactValue = true;
                tile.SubText = CodeText(alarm.AlarmType) + " · " + alarm.AlarmGrade + " " + CodeText(alarm.AlarmGrade);
                tile.Click += CodeTile_Click;
                flowCodes.Controls.Add(tile);
            }
        }

        private void CodeTile_Click(object sender, EventArgs e)
        {
            _selectedAlarm = ((CardTile)sender).Tag as AbnormalAlarmTypeDto;
            MarkSelected(flowCodes, sender);
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

        /// <summary>
        /// 고른 알람을 지금 공정에 등록한다. 성공하면 OK로 닫고(완료 안내는 호출한 화면이 띄운다),
        /// 실패하면 안내만 하고 창은 유지한다.
        /// </summary>
        private async void btnSend_Click(object sender, EventArgs e)
        {
            if (_selectedAlarm == null)
            {
                AppMessageBox.Warn(this, Language.AdminAlarm_SelectAlarm, Language.AdminAlarm_Title);
                return;
            }

            var request = new AbnormalAlarmRequest();
            request.AlarmType = _selectedAlarm.AlarmType;
            request.AlarmTypeCode = _selectedAlarm.AlarmTypeCode;
            var remark = txtRemark.Text?.Trim();
            request.Remark = string.IsNullOrEmpty(remark) ? null : remark;

            btnSend.Enabled = false;
            try
            {
                await _apiClient.PopApiClient.SetAbnormalAlarm(_key, request);
            }
            catch (ApiException ex)
            {
                AppLog.Warn($"[POP] 알람 등록 실패 - {request.AlarmType}/{request.AlarmTypeCode} : {ex.Message}");
                if (IsDisposed || Disposing) return;

                AppMessageBox.Warn(this, ex.Message, Language.AdminAlarm_Title);
                return;
            }
            finally
            {
                if (!IsDisposed && !Disposing)
                    btnSend.Enabled = true;
            }

            AppLog.Info($"[POP] 알람 등록 - {_key.WorkOrderNo}/{_key.SerialNo} {request.AlarmType}/{request.AlarmTypeCode} : {request.Remark}");
            if (IsDisposed || Disposing) return;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
