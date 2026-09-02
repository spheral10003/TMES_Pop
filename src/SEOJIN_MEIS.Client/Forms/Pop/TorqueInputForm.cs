using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using SEOJIN_MEIS.Client.Common.Models;
using SEOJIN_MEIS.Client.Forms.Common;
using SEOJIN_MEIS.Client.Localization;
using SEOJIN_MEIS.Client.Theme;

namespace SEOJIN_MEIS.Client.Forms.Pop
{
    /// <summary>
    /// &lt;토크 결과값 입력&gt; 팝업 — [BOM 스팩] 탭에서 <b>수동 렌치</b>를 켠 뒤 [입력] 버튼을 누르면 열린다.
    ///
    /// 토크 장비가 연결되지 않았거나 수동 렌치로 작업한 경우, 작업자가 계측값을 직접 적어 넣는 통로다.
    /// 허용 범위는 참고용으로만 보여 주고 합/불 판정은 하지 않는다(판정은 서버 몫).
    /// [저장]을 누르면 값이 <see cref="Value"/>에 담기고 DialogResult.OK로 닫힌다.
    ///
    /// VS 디자이너 호환을 위해 매개변수 없는 생성자만 두고, 실제 내용은 Initialize()로 넣는다.
    /// </summary>
    public partial class TorqueInputForm : XtraForm
    {
        /// <summary>[저장]으로 확정한 토크 결과값. 닫기/취소면 null.</summary>
        public decimal? Value { get; private set; }

        public TorqueInputForm()
        {
            InitializeComponent();
        }

        public void Initialize(PopBomSpecDto spec)
        {
            var target = spec ?? new PopBomSpecDto();

            Text = Strings.PopTorque_Title;
            lblTitle.Text = Strings.PopTorque_Title;
            lblValueCaption.Text = "· " + Strings.PopTorque_Value;
            btnSave.Text = Strings.PopSerial_Save;
            btnClose.Text = Strings.Common_Close;

            lblSpecName.Text = $"{Strings.Col_BomSpecName} : {target.BomSpecName} ({Strings.Col_InputPosition} {target.InputPosition})";
            lblRange.Text = $"{Strings.PopTorque_Range} : {Format(target.TorqueLower)} ~ {Format(target.TorqueUpper)} {target.TorqueUnit}";

            txtValue.Text = target.TorqueResult?.ToString(CultureInfo.CurrentCulture);

            ApplyTheme();

            // 스캐너/키패드 입력이 Enter로 끝나는 경우를 고려해 Enter를 [저장]과 같게 동작시킨다.
            txtValue.KeyDown += Value_KeyDown;
        }

        private void ApplyTheme()
        {
            AppTheme.DialogHeader(headerPanel, AppTheme.Violet);
            AppTheme.DialogHeaderTitle(lblTitle);
            AppTheme.ContentSurface(bodyPanel);
            AppTheme.ContentSurface(footerPanel);
            AppTheme.CardPanel(card);

            AppTheme.BodyText(lblSpecName, true);
            AppTheme.Caption(lblRange);
            AppTheme.BodyText(lblValueCaption);
            AppTheme.Editor(txtValue);

            headerIcon.SvgImage = AppIcons.Get(AppIcons.Reset, Color.White);
            headerIcon.SizeMode = SvgImageSizeMode.Stretch;
            headerIcon.BackColor = Color.Transparent;

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

        private void Value_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            e.Handled = true;
            Confirm();
        }

        private void btnSave_Click(object sender, EventArgs e) => Confirm();

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>숫자로 읽히는 값일 때만 확정한다(범위 판정은 하지 않는다).</summary>
        private void Confirm()
        {
            var text = txtValue.Text?.Trim();
            if (!decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out var value)
                && !decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value))
            {
                AppMessageBox.Warn(this, Strings.PopTorque_Invalid, Strings.PopTorque_Title);
                txtValue.Focus();
                return;
            }

            Value = value;
            DialogResult = DialogResult.OK;
            Close();
        }

        private static string Format(decimal? value) =>
            value?.ToString("0.###", CultureInfo.CurrentCulture) ?? "-";
    }
}
