using DevExpress.XtraEditors;
using SEOJIN_TMES.Client.ApiClient;
using SEOJIN_TMES.Client.Common.Configuration;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Composition;
using SEOJIN_TMES.Client.Core;
using SEOJIN_TMES.Client.Forms.Common;
using SEOJIN_TMES.Client.Mock;
using SEOJIN_TMES.Client.Theme;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SEOJIN_TMES.Client.Forms.WorkStatus
{
    /// <summary>
    /// &lt;생산품목 선택&gt; 팝업 — &lt;작업 조회 현황&gt; 화면의 생산품목 돋보기 버튼으로 연다.
    ///
    /// 품목코드/품목명을 부분 일치로 찾아 목록에서 한 건 고른다.
    /// 결과는 <see cref="SelectedItem"/>으로 받는다(DialogResult.OK일 때만 값이 있다).
    ///
    /// VS 디자이너 호환을 위해 매개변수 없는 생성자만 두고, 실제 내용은 Initialize()로 넣는다.
    /// </summary>
    public partial class ItemPickerForm : XtraForm
    {
        //private ClientContext _context;
        private AppSettings _appSet;
        private ApiClientFactory _apiClients;

        /// <summary>[선택]으로 확정한 품목. 취소/닫기면 null.</summary>
        public ItemDto SelectedItem { get; private set; }

        public ItemPickerForm()
        {
            InitializeComponent();
        }

        /// <summary>화면에 이미 입력돼 있던 품목코드가 있으면 검색어로 채워 바로 조회한다.</summary>
        public void Init(string keyword)
        {
            //_context = context;

            _appSet = Global.Instance.Settings.AppSet;
            _apiClients = Global.Instance.Settings.ApiClients;

            //Text = Language.ItemPicker_Title;
            //lblTitle.Text = Language.ItemPicker_Title;
            //lblKeyword.Text = "· " + Language.ItemPicker_Keyword;
            //lblListTitle.Text = Language.ItemPicker_ListTitle;
            //btnSearch.Text = Language.Common_Search;
            //btnSelect.Text = Language.ItemPicker_Select;
            //btnClose.Text = Language.Common_Close;

            ApplyLanguage();
            ApplyTheme();
            ConfigureGrid();

            txtKeyword.Text = keyword;

            // 바코드 스캐너처럼 Enter로 끝나는 입력을 그대로 조회로 연결한다.
            txtKeyword.KeyDown += Keyword_KeyDown;

            // 목록을 더블클릭하면 [선택]과 같게 동작한다.
            gridView.DoubleClick += (s, e) => ConfirmSelection();

            Load += async (s, e) => await SearchAsync();
        }

        private void ApplyLanguage()
        {
            Text = Language.ItemPicker_Title;
            lblTitle.Text = Language.ItemPicker_Title;
            lblKeyword.Text = "· " + Language.ItemPicker_Keyword;
            lblListTitle.Text = Language.ItemPicker_ListTitle;
            btnSearch.Text = Language.Common_Search;
            btnSelect.Text = Language.ItemPicker_Select;
            btnClose.Text = Language.Common_Close;
        }

        private void ApplyTheme()
        {
            AppTheme.DialogHeader(headerPanel, AppTheme.Cyan);
            DialogChrome.Apply(this, headerPanel);
            AppTheme.DialogHeaderTitle(lblTitle);
            AppTheme.ContentSurface(bodyPanel);
            AppTheme.ContentSurface(footerPanel);
            AppTheme.CardPanel(searchCard);
            AppTheme.CardPanel(listCard);
            spacer.BackColor = AppTheme.Surface;

            AppTheme.SectionTitle(lblListTitle, AppTheme.Cyan);
            AppTheme.BodyText(lblKeyword);
            AppTheme.CodeEditor(txtKeyword);
            btnSearch.Icon = AppIcons.Search;
            AppTheme.Grid(grid, gridView, AppTheme.Cyan);

            headerIcon.SvgImage = AppIcons.Get(AppIcons.Search, Color.White);
            headerIcon.SizeMode = SvgImageSizeMode.Stretch;
            headerIcon.BackColor = Color.Transparent;

        }

        private void ConfigureGrid()
        {
            grid.DataSource = new List<ItemDto>();
            GridColumns.Apply(gridView, Language.Col_No,
                GridColumns.Text("ItemCode", Language.Col_ItemCode, 130),
                GridColumns.Left("ItemName", Language.Col_ItemName, 180),
                GridColumns.Left("Specification", Language.Col_Specification, 150),
                GridColumns.Text("Unit", Language.Col_Unit, 90));
            gridView.OptionsView.ColumnAutoWidth = true;
        }

        private void Keyword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            e.Handled = true;
            _ = SearchAsync();
        }

        private async void btnSearch_Click(object sender, EventArgs e) => await SearchAsync();

        private async Task SearchAsync()
        {
            using (var busy = BusyOverlay.Show(this, cancelable: true))
            {
                try
                {
                    btnSearch.Enabled = false;
                    var result = await FetchItemsAsync(txtKeyword.Text?.Trim(), busy.Token);

                    if (IsDisposed || Disposing) return;   // 조회 중 팝업을 닫았다

                    grid.DataSource = result;
                    if (gridView.RowCount > 0)
                        gridView.FocusedRowHandle = 0;
                }
                catch (OperationCanceledException)
                {
                    // 사용자가 [취소]를 눌렀다 — 오류가 아니므로 조용히 끝낸다.
                }
                catch (ApiException ex)
                {
                    if (IsDisposed || Disposing) return;
                    AppMessageBox.Warn(this, ex.Message, Language.ItemPicker_Title);
                }
                finally
                {
                    if (!IsDisposed && !Disposing)
                        btnSearch.Enabled = true;
                }
            }
        }

        private async Task<List<ItemDto>> FetchItemsAsync(string keyword, CancellationToken cancellationToken)
        {
            if (_appSet.UseMockData)
            {
                await Task.Yield();
                return MockWorkStatusData.SearchItems(keyword);
            }

            return await _apiClients.WorkStatusApiClient.SearchItemsAsync(keyword, cancellationToken)
                   ?? new List<ItemDto>();
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

        private void btnSelect_Click(object sender, EventArgs e) => ConfirmSelection();

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>선택한 품목을 확정한다. 선택이 없으면 안내만 하고 창은 유지한다.</summary>
        private void ConfirmSelection()
        {
            var item = gridView.GetFocusedRow() as ItemDto;
            if (item == null)
            {
                AppMessageBox.Warn(this, Language.ItemPicker_SelectItem, Language.ItemPicker_Title);
                return;
            }

            SelectedItem = item;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
