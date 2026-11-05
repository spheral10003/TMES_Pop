using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Forms.Common;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Theme;

namespace SEOJIN_TMES.Client.Forms.Pop
{
    /// <summary>
    /// &lt;투입자재 정보 입력&gt; 팝업 — [투입자재] 탭에서 "시리얼번호 입력상태" 셀을 클릭하면 열린다.
    ///
    /// 자재 정보(코드/수량)는 읽기 전용으로 보여주고, 서버 값(serialVrfcYn · itemCdVrfcYn)에 따라 입력을 받는다:
    /// - 품목코드 : 품목코드 확인(itemCdVrfcYn=Y) 자재만 입력란이 열린다. 빈칸에서 시작하며,
    ///              작업자가 스캔한 값이 투입자재 코드와 같은지 확인한다 → <see cref="InputtedItemCode"/>
    /// - 시리얼   : 시리얼 확인 자재만 투입수량만큼 준비된 줄에 시리얼번호를 받는다 → <see cref="SerialNumbers"/>
    /// 바코드 스캐너를 고려해 Enter를 누르면 다음 입력으로 넘어간다.
    /// [저장]을 누르면 DialogResult.OK로 닫힌다 — 서버 전송은 호출한 화면(<see cref="PopScreen"/>)이 맡는다.
    ///
    /// VS 디자이너 호환을 위해 매개변수 없는 생성자만 두고, 실제 내용은 Initialize()로 넣는다.
    /// </summary>
    public partial class MaterialSerialForm : XtraForm
    {
        /// <summary>창을 줄일 수 있는 한계 — 크기를 통일하기 전 원래 크기(테두리 포함). 이보다 작으면 자재 정보 칸이 잘린다.</summary>
        private readonly Size _minimumFormSize = new Size(622, 522);

        /// <summary>입력 컬럼의 필드명(<see cref="MaterialSerialRow.InputValue"/>).</summary>
        private const string InputField = "InputValue";

        private PopInputMaterialDto _material;
        private List<MaterialSerialRow> _rows;

        /// <summary>[저장]으로 확정한 시리얼번호 목록(투입수량 순서). 시리얼 확인 대상이 아니거나 닫기/취소면 null.</summary>
        public List<string> SerialNumbers { get; private set; }

        /// <summary>[저장]으로 확정한 품목코드. 품목코드 확인 대상이 아니거나 닫기/취소면 null.</summary>
        public string InputtedItemCode { get; private set; }

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

            Text = Language.PopSerial_Title;
            lblTitle.Text = Language.PopSerial_Title;
            lblMaterialCode.Text = "· " + Language.PopSerial_MaterialCode;
            lblQty.Text = "· " + Language.PopSerial_Qty;
            lblItemCode.Text = "· " + Language.PopSerial_ItemCode;
            lblListTitle.Text = Language.PopSerial_ListTitle;
            btnSave.Text = Language.PopSerial_Save;
            btnClose.Text = Language.Common_Close;

            txtMaterialCode.Text = _material.MaterialCode;
            txtQty.Text = _material.InputQty.ToString();

            ApplyTheme();
            ConfigureItemCode();
            ConfigureGrid();

            grid.DataSource = _rows;

            // 품목코드를 확인하는 자재면 품목코드부터 스캔하게 하고, 아니면 바로 시리얼번호로 간다.
            if (_material.ItemCodeCheckRequired)
                ActiveControl = txtItemCode;
            else if (gridView.RowCount > 0)
                FocusSerialCell(0);
        }

        private void ApplyTheme()
        {
            AppTheme.DialogHeader(headerPanel, AppTheme.Indigo);
            DialogChrome.Apply(this, headerPanel);
            DialogChrome.FitPopupSize(this);
            DialogChrome.EnableResizeGrip(pnlGrip);
            MinimumSize = _minimumFormSize;
            AppTheme.DialogHeaderTitle(lblTitle);
            AppTheme.ContentSurface(bodyPanel);
            AppTheme.ContentSurface(footerPanel);
            pnlGrip.BackColor = AppTheme.Surface;
            AppTheme.StandardButton(btnSave);
            AppTheme.StandardButton(btnClose);
            AppTheme.CardPanel(fieldCard);
            AppTheme.CardPanel(listCard);
            spacer.BackColor = AppTheme.Surface;

            AppTheme.SectionTitle(lblListTitle, AppTheme.Indigo);
            AppTheme.BodyText(lblMaterialCode);
            AppTheme.BodyText(lblQty);
            AppTheme.BodyText(lblItemCode);
            AppTheme.Editor(txtMaterialCode);
            AppTheme.Editor(txtQty);
            AppTheme.CodeEditor(txtItemCode);
            AppTheme.Grid(grid, gridView, AppTheme.Indigo);

            headerIcon.SvgImage = AppIcons.Get(AppIcons.Clipboard, Color.White);
            headerIcon.SizeMode = SvgImageSizeMode.Stretch;
            headerIcon.BackColor = Color.Transparent;

        }

        /// <summary>
        /// 품목코드 입력란을 서버의 품목코드 확인 여부(itemCdVrfcYn, Y일 때만)에 맞춰 열거나 닫는다.
        /// 서버가 내려준 품목코드를 미리 채우지 않는다 — 작업자가 직접 스캔한 값으로 확인해야 의미가 있다.
        /// </summary>
        private void ConfigureItemCode()
        {
            var required = _material.ItemCodeCheckRequired;

            txtItemCode.Text = string.Empty;
            txtItemCode.Properties.ReadOnly = !required;
            txtItemCode.Enabled = required;
            txtItemCode.TabStop = required;

            txtItemCode.KeyDown -= TxtItemCode_KeyDown;
            if (required)
                txtItemCode.KeyDown += TxtItemCode_KeyDown;
        }

        /// <summary>바코드 스캐너 입력은 Enter로 끝난다 — 그때 투입자재 코드와 대조하고, 맞으면 시리얼번호 첫 줄로 넘어간다.</summary>
        private void TxtItemCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            e.Handled = true;
            e.SuppressKeyPress = true;

            if (!CheckItemCode())
                return;

            if (gridView.RowCount > 0)
            {
                grid.Focus();
                FocusSerialCell(0);
            }
        }

        /// <summary>
        /// 입력한 품목코드가 투입자재 코드와 같은지 확인한다. 다르면 안내하고 입력란을 비운 뒤 다시 입력받는다
        /// (다른 자재를 투입하려는 실수를 저장 전에 막기 위함 — 최종 판정은 서버가 한다).
        /// </summary>
        private bool CheckItemCode()
        {
            var itemCode = txtItemCode.Text?.Trim();

            if (string.IsNullOrEmpty(itemCode))
            {
                AppMessageBox.Warn(this, Language.PopSerial_ItemCodeIncomplete, Language.PopSerial_Title);
                txtItemCode.Focus();
                return false;
            }

            if (!string.Equals(itemCode, _material.MaterialCode?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                AppMessageBox.Warn(this,
                    $"{Language.PopSerial_ItemCodeMismatch}\n({Language.PopSerial_MaterialCode}: {_material.MaterialCode} / {Language.PopSerial_ItemCode}: {itemCode})",
                    Language.PopSerial_Title);
                txtItemCode.Text = string.Empty;
                txtItemCode.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// 시리얼 확인 자재면 투입수량만큼 줄을 만들고(이미 입력된 값은 되살린다) 입력 컬럼만 입력 가능하게 둔다.
        /// 그리드는 시리얼만 받는다 — 품목코드만 확인하는 자재는 줄이 없다.
        /// </summary>
        private void ConfigureGrid()
        {
            _rows = new List<MaterialSerialRow>();
            var saved = _material.SerialNumbers ?? new List<string>();
            var rowCount = _material.SerialCheckRequired ? Math.Max(_material.InputQty, 1) : 0;

            for (var i = 0; i < rowCount; i++)
            {
                var row = new MaterialSerialRow();
                row.InputValue = i < saved.Count ? saved[i] : null;
                _rows.Add(row);
            }

            // 컬럼이 만들어지도록 빈 목록을 먼저 물린 뒤 구성한다(다른 화면과 같은 방식).
            grid.DataSource = new List<MaterialSerialRow>();
            GridColumns.Apply(gridView, Language.Col_No,
                GridColumns.Left(InputField, Language.Col_SerialNumber, 420));

            gridView.OptionsBehavior.Editable = true;
            GridColumns.FitToWidth(gridView);

            var serialColumn = gridView.Columns.ColumnByFieldName(InputField);
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
            var serialColumn = gridView.Columns.ColumnByFieldName(InputField);
            if (serialColumn == null)
                return;

            gridView.FocusedRowHandle = rowHandle;
            gridView.FocusedColumn = serialColumn;
            gridView.ShowEditor();
        }

        /// <summary>부모 창(없으면 화면) 가운데에 띄운다.</summary>
        public void CenterOn(IWin32Window owner)
        {
            DialogChrome.CenterOn(this, owner);   // 부모가 최소화돼 있어도 화면 안에 놓는다
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// 입력값을 확정한다. 품목코드가 투입자재 코드와 다르거나, 시리얼 빈칸이 남아 있거나,
        /// 같은 시리얼번호가 두 번 들어오면 안내만 하고 창은 유지한다
        /// (최종 유효성 판정은 서버가 하지만, 명백한 입력 실수는 여기서 먼저 걸러 준다).
        /// </summary>
        private void btnSave_Click(object sender, EventArgs e)
        {
            gridView.CloseEditor();
            gridView.UpdateCurrentRow();

            // Enter 없이 손으로 치고 바로 [저장]을 누른 경우도 있으므로 여기서 한 번 더 대조한다.
            if (_material.ItemCodeCheckRequired && !CheckItemCode())
                return;

            List<string> serials = null;
            if (_material.SerialCheckRequired)
            {
                serials = new List<string>();
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var row in _rows)
                {
                    var serial = row.InputValue?.Trim();

                    if (string.IsNullOrEmpty(serial))
                    {
                        AppMessageBox.Warn(this, Language.PopSerial_Incomplete, Language.PopSerial_Title);
                        return;
                    }

                    if (!seen.Add(serial))
                    {
                        AppMessageBox.Warn(this, $"{Language.PopSerial_Duplicated} ({serial})", Language.PopSerial_Title);
                        return;
                    }

                    serials.Add(serial);
                }
            }

            SerialNumbers = serials;
            InputtedItemCode = _material.ItemCodeCheckRequired ? txtItemCode.Text.Trim() : null;
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    /// <summary>&lt;투입자재 정보 입력&gt; 그리드의 한 줄(순번 컬럼은 GridColumns가 만들어 준다).</summary>
    public class MaterialSerialRow
    {
        /// <summary>입력값 — 시리얼번호.</summary>
        public string InputValue { get; set; }
    }
}
