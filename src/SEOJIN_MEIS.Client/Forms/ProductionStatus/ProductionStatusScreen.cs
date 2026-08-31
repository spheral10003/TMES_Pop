using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using SEOJIN_MEIS.Client.ApiClient;
using SEOJIN_MEIS.Client.Forms.Common;
using SEOJIN_MEIS.Client.Composition;
using SEOJIN_MEIS.Client.Localization;
using SEOJIN_MEIS.Client.Theme;

namespace SEOJIN_MEIS.Client.Forms.ProductionStatus
{
    /// <summary>
    /// MEIS > 작업 현황 조회 > 생산 현황조회. 조회/집계 로직은 서버가 수행하고 화면은 결과만 표시한다.
    /// 화면은 조회 조건 카드 + 결과 그리드 카드로 구성되며, 색은 <see cref="AppTheme"/>가 담당한다(포인트 컬러: Cyan).
    /// VS 디자이너 호환을 위해 매개변수 없는 생성자만 두고, 실제 의존성은 Initialize()로 주입한다.
    /// </summary>
    public partial class ProductionStatusScreen : XtraUserControl, IThemedScreen
    {
        private ClientContext _context;

        public ProductionStatusScreen()
        {
            InitializeComponent();
        }

        public void Initialize(ClientContext context)
        {
            _context = context;

            lblFilterTitle.Text = Strings.ProductionStatus_FilterTitle;
            lblGridTitle.Text = Strings.ProductionStatus_Title;
            lblDate.Text = Strings.ProductionStatus_StandardDate;
            btnSearch.Text = Strings.Common_Refresh;
            dateStandard.EditValue = DateTime.Today;

            ApplyTheme();

            Load += async (s, e) => await SearchAsync();
        }

        /// <summary>현재 테마 색을 화면에 입힌다(테마 전환 시 MainForm이 다시 호출).</summary>
        public void ApplyTheme()
        {
            BackColor = AppTheme.Surface;
            spacer.BackColor = AppTheme.Surface;

            AppTheme.CardPanel(topCard);
            AppTheme.CardPanel(gridCard);
            AppTheme.SectionTitle(lblFilterTitle, AppTheme.Cyan);
            AppTheme.SectionTitle(lblGridTitle, AppTheme.Cyan);
            AppTheme.BodyText(lblDate);
            AppTheme.Editor(dateStandard);
            AppTheme.AccentButton(btnSearch, AppTheme.Cyan, AppIcons.Search);
            AppTheme.Grid(grid, gridView, AppTheme.Cyan);
        }

        private async void btnSearch_Click(object sender, EventArgs e) => await SearchAsync();

        private async System.Threading.Tasks.Task SearchAsync()
        {
            try
            {
                var standardDate = dateStandard.EditValue is DateTime dt ? dt : DateTime.Today;
                var result = await _context.ApiClients.ProductionApiClient.GetProductionStatusAsync(standardDate);
                grid.DataSource = result;
            }
            catch (ApiException ex)
            {
                AppMessageBox.Warn(this, ex.Message, Strings.ProductionStatus_Title);
            }
        }
    }
}
