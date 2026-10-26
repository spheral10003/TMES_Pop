using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
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
    /// 이상 알람 유형 기준정보(<c>md/abnormal-alarm-types</c>)를 조회해 <b>타일</b>로 깔고,
    /// 작업자가 타일을 눌러 알람 코드와 조치 내용을 고른 뒤 비고를 적어 [등록]한다.
    /// 등록(<c>pop/{sn}/{subRpRef}/abnormal-alarms</c>)까지 이 창이 맡는다 —
    /// 실패하면 창을 닫지 않고 안내만 해서, 고른 내용을 다시 고르지 않고 재시도할 수 있게 한다.
    ///
    /// 일반 .NET Form이며 Windows 제목 줄 대신 pnlTitle(패널) + lblTitle(라벨)이 제목 줄 역할을 한다.
    /// VS 디자이너 호환을 위해 매개변수 없는 생성자만 두고, 실제 내용은 Initialize()로 넣는다.
    /// </summary>
    public partial class AdminAlarmForm : Form
    {
        // 알람 구분 · 등급 코드는 서버 스펙으로 고정된 값이다. 표시 문구만 다국어로 바꾼다.
        private readonly string[] _alarmTypes = { "EQP", "QLT", "MAT", "PRD", "EHS", "SYS" };
        private readonly string[] _alarmGrades = { "P1", "P2", "P3", "P4" };

        private ApiClientFactory _apiClient;
        private PopProcessKey _key;
        private AbnormalAlarmTypeDto _selected;

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
            lblType.Text = Language.AdminAlarm_Type;
            lblGrade.Text = Language.AdminAlarm_Grade;
            lblName.Text = Language.AdminAlarm_Name;
            lblListTitle.Text = Language.AdminAlarm_ListTitle;
            lblRemark.Text = Language.Col_Remark;
            btnSearch.Text = Language.Common_Search;
            btnSend.Text = Language.AdminAlarm_Send;
            btnClose.Text = Language.Common_Close;

            FillCodeCombo(cboType, _alarmTypes);
            FillCodeCombo(cboGrade, _alarmGrades);

            ApplyTheme();
            DialogChrome.Apply(this, pnlTitle);
        }

        private void ApplyTheme()
        {
            AppTheme.TitleBar(pnlTitle, lblTitle, AppTheme.Amber);
            pnlSearch.BackColor = AppTheme.Card;
            pnlBody.BackColor = AppTheme.Surface;
            pnlRemark.BackColor = AppTheme.Surface;
            pnlFooter.BackColor = AppTheme.Surface;

            foreach (var label in new[] { lblType, lblGrade, lblName, lblRemark })
            {
                label.ForeColor = AppTheme.TextPrimary;
                label.Font = AppTheme.BodyBoldFont;
            }
            lblListTitle.ForeColor = AppTheme.Amber;
            lblListTitle.Font = AppTheme.SectionFont;

            AppTheme.Editor(cboType);
            AppTheme.Editor(cboGrade);
            AppTheme.Editor(txtName);
            AppTheme.Editor(txtRemark);
            AppTheme.PickerTiles(tileAlarms, 96);

            btnSearch.Icon = AppIcons.Search;
        }

        /// <summary>콤보 맨 앞에 [전체]를 두고 코드별 항목을 채운다. [전체]는 조건 없이 조회한다는 뜻이다.</summary>
        private void FillCodeCombo(ComboBoxEdit combo, string[] codes)
        {
            combo.Properties.Items.Clear();
            combo.Properties.Items.Add(new CodeItem(null, Language.Common_All));
            foreach (var code in codes)
                combo.Properties.Items.Add(new CodeItem(code, CodeText(code)));

            combo.SelectedIndex = 0;
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

        /// <summary>창이 뜨자마자 전체(사용 중인 코드)를 한 번 조회해 타일을 채운다.</summary>
        private async void AdminAlarmForm_Shown(object sender, EventArgs e) => await GetAlarmTypes();

        private async void btnSearch_Click(object sender, EventArgs e) => await GetAlarmTypes();

        private async void txtName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            e.SuppressKeyPress = true;
            await GetAlarmTypes();
        }

        /// <summary>
        /// 조회 조건으로 알람 유형을 읽어 타일을 다시 깐다. 회사 코드는 지금 공정의 것을 쓰고,
        /// 등록할 수 있는 코드만 보이도록 사용 여부는 항상 Y로 건다.
        /// </summary>
        private async Task GetAlarmTypes()
        {
            var query = new AbnormalAlarmTypeQuery();
            query.CorpCode = _key?.CorpCode;
            query.AlarmType = (cboType.SelectedItem as CodeItem)?.Code;
            query.AlarmGrade = (cboGrade.SelectedItem as CodeItem)?.Code;
            query.AlarmTypeName = txtName.Text?.Trim();
            query.UseYn = "Y";

            List<AbnormalAlarmTypeDto> alarmTypes;
            btnSearch.Enabled = false;
            try
            {
                alarmTypes = await _apiClient.PopApiClient.GetAbnormalAlarmTypes(query);
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

            FillTiles(alarmTypes);
            if (alarmTypes.Count == 0)
                AppMessageBox.Warn(this, Language.AdminAlarm_NoTypes, Language.AdminAlarm_Title);
        }

        /// <summary>알람 하나당 타일 하나. 다시 조회하면 고른 알람도 비운다(목록에 없을 수 있다).</summary>
        private void FillTiles(List<AbnormalAlarmTypeDto> alarmTypes)
        {
            _selected = null;
            tileAlarms.BeginUpdate();
            try
            {
                tileGroupAlarms.Items.Clear();
                foreach (var alarm in alarmTypes)
                    tileGroupAlarms.Items.Add(CreateTile(alarm));
            }
            finally
            {
                tileAlarms.EndUpdate();
            }
        }

        /// <summary>
        /// 타일 한 장 — 좌상단에 코드 · 구분, 가운데에 알람 명(조치 내용), 우하단에 등급.
        /// 바탕색은 등급 색이라 급한 알람이 먼저 눈에 들어온다.
        /// </summary>
        private TileItem CreateTile(AbnormalAlarmTypeDto alarm)
        {
            var item = new TileItem();
            item.Tag = alarm;
            AppTheme.AlarmTile(item, AppTheme.AlarmGradeColor(alarm.AlarmGrade));

            var code = new TileItemElement();
            code.Text = alarm.AlarmTypeCode + "  ·  " + CodeText(alarm.AlarmType);
            code.TextAlignment = TileItemContentAlignment.TopLeft;
            code.Appearance.Normal.Font = AppTheme.SmallFont;
            item.Elements.Add(code);

            var name = new TileItemElement();
            name.Text = alarm.AlarmTypeName;
            name.TextAlignment = TileItemContentAlignment.MiddleLeft;
            name.Appearance.Normal.Font = AppTheme.TitleFont;
            item.Elements.Add(name);

            var grade = new TileItemElement();
            grade.Text = alarm.AlarmGrade + " " + CodeText(alarm.AlarmGrade);
            grade.TextAlignment = TileItemContentAlignment.BottomRight;
            grade.Appearance.Normal.Font = AppTheme.BodyBoldFont;
            item.Elements.Add(grade);

            return item;
        }

        private void tileAlarms_ItemClick(object sender, TileItemEventArgs e)
        {
            _selected = e.Item?.Tag as AbnormalAlarmTypeDto;
            tileAlarms.SelectedItem = e.Item;
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
            if (_selected == null)
            {
                AppMessageBox.Warn(this, Language.AdminAlarm_SelectAlarm, Language.AdminAlarm_Title);
                return;
            }

            var request = new AbnormalAlarmRequest();
            request.AlarmType = _selected.AlarmType;
            request.AlarmTypeCode = _selected.AlarmTypeCode;
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

        /// <summary>조회 조건 콤보 항목 — 서버로 보낼 코드와 화면에 보일 문구 한 쌍. 코드가 null이면 [전체].</summary>
        private class CodeItem
        {
            public CodeItem(string code, string text)
            {
                Code = code;
                Text = text;
            }

            public string Code { get; }
            public string Text { get; }

            public override string ToString() => Text;
        }
    }
}
