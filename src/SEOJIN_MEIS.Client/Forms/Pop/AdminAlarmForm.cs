using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using SEOJIN_MEIS.Client.ApiClient;
using SEOJIN_MEIS.Client.Common.Models;
using SEOJIN_MEIS.Client.Forms.Common;
using SEOJIN_MEIS.Client.Localization;
using SEOJIN_MEIS.Client.Theme;

namespace SEOJIN_MEIS.Client.Forms.Pop
{
    /// <summary>
    /// &lt;관리자 알람 유형 선택&gt; 팝업 — POP 본 화면의 [관리자 알람] 버튼으로 연다.
    ///
    /// 작업자가 알람 유형을 고르면 그 유형이 요구하는 대상(<see cref="AdminAlarmTargetKind"/>)에 맞춰
    /// 상세 입력 영역이 바뀐다 — 자재 결품이면 <b>현재 공정의 투입자재</b> 중에서,
    /// 설비 이상이면 <b>워크스테이션에 연결된 장비</b> 중에서 고르게 해 코드 오타를 없앤다.
    /// 대상이 없는 유형(품질 이상/기타)은 비고만 받는다.
    ///
    /// [전송]을 누르면 입력 내용이 <see cref="Request"/>에 담기고 DialogResult.OK로 닫힌다 —
    /// 서버 전송은 호출한 화면(<see cref="PopScreen"/>)이 맡는다.
    ///
    /// VS 디자이너 호환을 위해 매개변수 없는 생성자만 두고, 실제 내용은 Initialize()로 넣는다.
    /// </summary>
    public partial class AdminAlarmForm : XtraForm
    {
        private List<AdminAlarmTypeDto> _alarmTypes = new List<AdminAlarmTypeDto>();
        private List<PopInputMaterialDto> _materials = new List<PopInputMaterialDto>();
        private List<PopEquipmentDto> _equipments = new List<PopEquipmentDto>();

        /// <summary>[전송]으로 확정한 알람 내용(공정 키/작업자는 호출한 화면이 채운다). 닫기/취소면 null.</summary>
        public PopAdminAlarmRequest Request { get; private set; }

        public AdminAlarmForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 알람 유형 목록과, 대상 선택에 쓸 현재 공정의 투입자재/연결 장비를 받아 화면을 채운다.
        /// </summary>
        public void Initialize(
            List<AdminAlarmTypeDto> alarmTypes,
            List<PopInputMaterialDto> materials,
            List<PopEquipmentDto> equipments)
        {
            _alarmTypes = alarmTypes ?? new List<AdminAlarmTypeDto>();
            _materials = materials ?? new List<PopInputMaterialDto>();
            _equipments = equipments ?? new List<PopEquipmentDto>();

            Text = Strings.AdminAlarm_Title;
            lblTitle.Text = Strings.AdminAlarm_Title;
            lblAlarmType.Text = "· " + Strings.AdminAlarm_Type;
            lblDetailTitle.Text = Strings.AdminAlarm_DetailTitle;
            lblRemark.Text = "· " + Strings.Col_Remark;
            btnSend.Text = Strings.AdminAlarm_Send;
            btnClose.Text = Strings.Common_Close;

            ApplyTheme();

            cboAlarmType.Properties.Items.Clear();
            foreach (var type in _alarmTypes)
                cboAlarmType.Properties.Items.Add(type);

            if (cboAlarmType.Properties.Items.Count > 0)
                cboAlarmType.SelectedIndex = 0;

            ApplySelectedType();
        }

        private void ApplyTheme()
        {
            AppTheme.DialogHeader(headerPanel, AppTheme.Amber);
            AppTheme.DialogHeaderTitle(lblTitle);
            AppTheme.ContentSurface(bodyPanel);
            AppTheme.ContentSurface(footerPanel);
            AppTheme.CardPanel(typeCard);
            AppTheme.CardPanel(detailCard);
            spacer.BackColor = AppTheme.Surface;

            AppTheme.SectionTitle(lblDetailTitle, AppTheme.Amber);
            AppTheme.BodyText(lblAlarmType);
            AppTheme.BodyText(lblTarget);
            AppTheme.BodyText(lblRemark);
            AppTheme.Editor(cboAlarmType);
            AppTheme.Editor(cboTarget);
            AppTheme.Editor(txtRemark);

            headerIcon.SvgImage = AppIcons.Get(AppIcons.Bell, Color.White);
            headerIcon.SizeMode = SvgImageSizeMode.Stretch;
            headerIcon.BackColor = Color.Transparent;

        }

        private void cboAlarmType_SelectedIndexChanged(object sender, EventArgs e) => ApplySelectedType();

        /// <summary>
        /// 고른 알람 유형에 맞춰 대상 입력줄을 바꾼다 —
        /// 라벨 문구를 유형이 정한 것으로 바꾸고, 고를 수 있는 목록을 채우고, 필요 없으면 줄 자체를 감춘다.
        /// 비고 입력란은 대상줄이 사라진 만큼 위로 올려 빈 줄이 남지 않게 한다.
        /// </summary>
        private void ApplySelectedType()
        {
            var selected = cboAlarmType.SelectedItem as AdminAlarmTypeDto;
            var kind = selected?.TargetKind ?? AdminAlarmTargetKind.None;

            cboTarget.Properties.Items.Clear();
            cboTarget.EditValue = null;

            if (kind == AdminAlarmTargetKind.Material)
            {
                foreach (var material in _materials)
                    cboTarget.Properties.Items.Add(material.MaterialCode);
            }
            else if (kind == AdminAlarmTargetKind.Equipment)
            {
                foreach (var equipment in _equipments)
                    cboTarget.Properties.Items.Add(equipment.EquipmentCode);
            }

            var needsTarget = kind != AdminAlarmTargetKind.None;
            lblTarget.Visible = needsTarget;
            cboTarget.Visible = needsTarget;

            if (needsTarget)
            {
                lblTarget.Text = "· " + (string.IsNullOrWhiteSpace(selected.TargetCaption)
                    ? Strings.AdminAlarm_Target
                    : selected.TargetCaption);

                if (cboTarget.Properties.Items.Count > 0)
                    cboTarget.SelectedIndex = 0;
            }

            // 대상줄(38px)이 없으면 비고를 그 자리부터 시작하고, 늘어난 만큼 높이도 키운다.
            var remarkTop = needsTarget ? 84 : 46;
            var delta = txtRemark.Top - remarkTop;
            lblRemark.Top = remarkTop + 4;
            txtRemark.Top = remarkTop;
            txtRemark.Height += delta;
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

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// 입력 내용을 확정한다. 유형을 안 골랐거나, 대상이 필요한 유형인데 대상이 비었으면
        /// 안내만 하고 창은 유지한다(관리자를 부르는 알람이라 최소한 무엇 때문인지는 담겨야 한다).
        /// </summary>
        private void btnSend_Click(object sender, EventArgs e)
        {
            var selected = cboAlarmType.SelectedItem as AdminAlarmTypeDto;
            if (selected == null)
            {
                AppMessageBox.Warn(this, Strings.AdminAlarm_SelectType, Strings.AdminAlarm_Title);
                return;
            }

            var targetCode = cboTarget.Visible ? cboTarget.Text?.Trim() : null;
            if (cboTarget.Visible && string.IsNullOrEmpty(targetCode))
            {
                AppMessageBox.Warn(this, Strings.AdminAlarm_SelectTarget, Strings.AdminAlarm_Title);
                return;
            }

            var request = new PopAdminAlarmRequest();
            request.AlarmTypeCode = selected.Code;
            request.AlarmTypeName = selected.Name;
            request.TargetCode = targetCode;
            request.Remark = txtRemark.Text?.Trim();

            Request = request;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
