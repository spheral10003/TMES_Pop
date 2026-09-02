using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using SEOJIN_MEIS.Client.Common.Models;
using SEOJIN_MEIS.Client.Forms.Common;
using SEOJIN_MEIS.Client.Localization;
using SEOJIN_MEIS.Client.Theme;

namespace SEOJIN_MEIS.Client.Forms.Pop
{
    /// <summary>
    /// &lt;투입자재 정보 입력&gt; 팝업 — [투입자재] 탭에서 "시리얼번호 입력상태" 셀을 클릭하면 열린다.
    ///
    /// 자재 정보(코드/수량/품목코드)는 읽기 전용으로 보여주고, 투입수량만큼 준비된 줄에
    /// 시리얼번호를 하나씩 입력받는다(바코드 스캐너를 고려해 Enter를 누르면 다음 줄로 내려간다).
    /// [저장]을 누르면 결과가 <see cref="SerialNumbers"/>에 담기고 DialogResult.OK로 닫힌다 —
    /// 서버 전송은 호출한 화면(<see cref="PopScreen"/>)이 맡는다.
    ///
    /// VS 디자이너 호환을 위해 매개변수 없는 생성자만 두고, 실제 내용은 Initialize()로 넣는다.
    /// </summary>
    public partial class MaterialSerialForm : XtraForm
    {
        private PopInputMaterialDto _material;
        private List<MaterialSerialRow> _rows;

        /// <summary>[저장]으로 확정한 시리얼번호 목록(투입수량 순서). 닫기/취소면 null.</summary>
        public List<string> SerialNumbers { get; private set; }

        public MaterialSerialForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 자재 한 건을 받아 화면을 채운다.
        /// 이미 입력된 시리얼번호가 있으면 그대로 되살려 이어서 수정할 수 있게 한다.
        /// </summary>
        public void Initialize(PopInputMaterialDto material)
        {
            _material = material ?? new PopInputMaterialDto();

            Text = Strings.PopSerial_Title;
            lblTitle.Text = Strings.PopSerial_Title;
            lblMaterialCode.Text = "· " + Strings.PopSerial_MaterialCode;
            lblQty.Text = "· " + Strings.PopSerial_Qty;
            lblItemCode.Text = "· " + Strings.PopSerial_ItemCode;
            lblListTitle.Text = Strings.PopSerial_ListTitle;
            btnSave.Text = Strings.PopSerial_Save;
            btnClose.Text = Strings.Common_Close;

            txtMaterialCode.Text = _material.MaterialCode;
            txtQty.Text = _material.InputQty.ToString();
            txtItemCode.Text = _material.ItemCode;

            ApplyTheme();
            ConfigureGrid();

            grid.DataSource = _rows;

            if (gridView.RowCount > 0)
                FocusSerialCell(0);
        }

        private void ApplyTheme()
        {
            AppTheme.DialogHeader(headerPanel, AppTheme.Indigo);
            AppTheme.DialogHeaderTitle(lblTitle);
            AppTheme.ContentSurface(bodyPanel);
            AppTheme.ContentSurface(footerPanel);
            AppTheme.CardPanel(fieldCard);
            AppTheme.CardPanel(listCard);
            spacer.BackColor = AppTheme.Surface;

            AppTheme.SectionTitle(lblListTitle, AppTheme.Indigo);
            AppTheme.BodyText(lblMaterialCode);
            AppTheme.BodyText(lblQty);
            AppTheme.BodyText(lblItemCode);
            AppTheme.Editor(txtMaterialCode);
            AppTheme.Editor(txtQty);
            AppTheme.Editor(txtItemCode);
            AppTheme.Grid(grid, gridView, AppTheme.Indigo);

            headerIcon.SvgImage = AppIcons.Get(AppIcons.Clipboard, Color.White);
            headerIcon.SizeMode = SvgImageSizeMode.Stretch;
            headerIcon.BackColor = Color.Transparent;

        }

        /// <summary>
        /// 투입수량만큼 빈 줄을 만들고(이미 입력된 값은 되살린다), 시리얼번호 컬럼만 입력 가능하게 둔다.
        /// </summary>
        private void ConfigureGrid()
        {
            _rows = new List<MaterialSerialRow>();
            var saved = _material.SerialNumbers ?? new List<string>();

            for (var i = 0; i < Math.Max(_material.InputQty, 1); i++)
            {
                var row = new MaterialSerialRow();
                row.SerialNo = i < saved.Count ? saved[i] : null;
                _rows.Add(row);
            }

            // 컬럼이 만들어지도록 빈 목록을 먼저 물린 뒤 구성한다(다른 화면과 같은 방식).
            grid.DataSource = new List<MaterialSerialRow>();
            GridColumns.Apply(gridView, Strings.Col_No,
                GridColumns.Left("SerialNo", Strings.Col_SerialNumber, 420));

            gridView.OptionsBehavior.Editable = true;
            gridView.OptionsView.ColumnAutoWidth = true;

            var serialColumn = gridView.Columns.ColumnByFieldName("SerialNo");
            if (serialColumn != null)
            {
                serialColumn.OptionsColumn.AllowEdit = true;
                serialColumn.ColumnEdit = CreateSerialEditor();
            }
        }

        /// <summary>
        /// 시리얼번호 입력용 에디터. 바코드 스캐너 입력은 Enter로 끝나므로,
        /// Enter를 받으면 값을 확정하고 다음 줄로 내려가 바로 이어서 스캔할 수 있게 한다.
        /// </summary>
        private RepositoryItemTextEdit CreateSerialEditor()
        {
            var editor = new RepositoryItemTextEdit();
            editor.Appearance.Font = AppTheme.BodyFont;
            editor.Appearance.Options.UseFont = true;
            editor.KeyDown += SerialEditor_KeyDown;

            grid.RepositoryItems.Add(editor);
            return editor;
        }

        private void SerialEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            e.Handled = true;
            gridView.CloseEditor();

            var next = gridView.FocusedRowHandle + 1;
            if (next < gridView.RowCount)
                FocusSerialCell(next);
        }

        /// <summary>지정한 줄의 시리얼번호 칸으로 포커스를 옮기고 바로 입력 상태로 만든다.</summary>
        private void FocusSerialCell(int rowHandle)
        {
            var serialColumn = gridView.Columns.ColumnByFieldName("SerialNo");
            if (serialColumn == null)
                return;

            gridView.FocusedRowHandle = rowHandle;
            gridView.FocusedColumn = serialColumn;
            gridView.ShowEditor();
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
        /// 입력값을 확정한다. 빈칸이 남아 있거나 같은 시리얼번호가 두 번 들어오면 안내만 하고 창은 유지한다
        /// (최종 유효성 판정은 서버가 하지만, 명백한 입력 실수는 여기서 먼저 걸러 준다).
        /// </summary>
        private void btnSave_Click(object sender, EventArgs e)
        {
            gridView.CloseEditor();
            gridView.UpdateCurrentRow();

            var serials = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in _rows)
            {
                var serial = row.SerialNo?.Trim();

                if (string.IsNullOrEmpty(serial))
                {
                    AppMessageBox.Warn(this, Strings.PopSerial_Incomplete, Strings.PopSerial_Title);
                    return;
                }

                if (!seen.Add(serial))
                {
                    AppMessageBox.Warn(this, $"{Strings.PopSerial_Duplicated} ({serial})", Strings.PopSerial_Title);
                    return;
                }

                serials.Add(serial);
            }

            SerialNumbers = serials;
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    /// <summary>&lt;투입자재 정보 입력&gt; 그리드의 한 줄(순번 컬럼은 GridColumns가 만들어 준다).</summary>
    public class MaterialSerialRow
    {
        public string SerialNo { get; set; }
    }
}
