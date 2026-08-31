using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using SEOJIN_MEIS.Client.Common.Models;
using SEOJIN_MEIS.Client.Forms.Common;
using SEOJIN_MEIS.Client.Localization;
using SEOJIN_MEIS.Client.Theme;

namespace SEOJIN_MEIS.Client.Forms.WorkOrderEntry
{
    /// <summary>
    /// &lt;작업 진입 확인&gt; 팝업 — 선택한 작업지시의 라우팅(공정) 목록을 보여주고,
    /// 사용자가 이동할 **페이지 공정**을 고르면 [이동]으로 확정한다.
    ///
    /// 결과는 <see cref="SelectedProcess"/>로 받는다(DialogResult.OK일 때만 값이 있다).
    /// 데이터 조회는 호출하는 화면(<see cref="WorkOrderEntryScreen"/>)이 미리 해서 넘겨준다 —
    /// 공정이 하나도 없을 때 빈 팝업이 뜨지 않도록 하기 위함.
    ///
    /// VS 디자이너 호환을 위해 매개변수 없는 생성자만 두고, 실제 내용은 Initialize()로 넣는다.
    /// </summary>
    public partial class WorkEntryConfirmForm : XtraForm
    {
        private WorkOrderEntryDto _workOrder;

        /// <summary>[이동]으로 확정한 페이지 공정. 취소/닫기면 null.</summary>
        public RoutingProcessDto SelectedProcess { get; private set; }

        public WorkEntryConfirmForm()
        {
            InitializeComponent();
        }

        public void Initialize(WorkOrderEntryDto workOrder, List<RoutingProcessDto> processes)
        {
            _workOrder = workOrder;

            Text = Strings.WorkEntryConfirm_Title;
            lblTitle.Text = _workOrder != null
                ? $"{Strings.WorkEntryConfirm_Title} - {_workOrder.WorkOrderNo}"
                : Strings.WorkEntryConfirm_Title;
            lblListTitle.Text = Strings.WorkEntryConfirm_ListTitle;
            btnMove.Text = Strings.WorkEntryConfirm_Move;
            btnClose.Text = Strings.Common_Close;

            ApplyTheme();

            grid.DataSource = processes ?? new List<RoutingProcessDto>();
            GridColumns.Apply(gridView, Strings.Col_No,
                GridColumns.Text("RoutingGroup", Strings.Col_RoutingGroup, 150),
                GridColumns.Text("RoutingOperation", Strings.Col_RoutingOperation, 140),
                GridColumns.Text("PageOperation", Strings.Col_PageOperation, 140),
                GridColumns.Text("ProgressState", Strings.Col_ProgressState, 120));

            if (gridView.RowCount > 0)
                gridView.FocusedRowHandle = 0;

            // 행을 더블클릭하면 [이동]과 같게 동작한다.
            gridView.DoubleClick += (s, e) => MoveToSelectedProcess();
        }

        private void ApplyTheme()
        {
            AppTheme.DialogHeader(headerPanel, AppTheme.Indigo);
            AppTheme.DialogHeaderTitle(lblTitle);
            AppTheme.ContentSurface(bodyPanel);
            AppTheme.ContentSurface(footerPanel);
            AppTheme.CardPanel(listCard);
            AppTheme.SectionTitle(lblListTitle, AppTheme.Indigo);
            AppTheme.Grid(grid, gridView, AppTheme.Indigo);

            headerIcon.SvgImage = AppIcons.Get(AppIcons.Layers, Color.White);
            headerIcon.SizeMode = SvgImageSizeMode.Stretch;
            headerIcon.BackColor = Color.Transparent;

            AppTheme.AccentButton(btnMove, AppTheme.Indigo);
            AppTheme.QuietButton(btnClose, AppTheme.Slate);
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

        private void btnMove_Click(object sender, EventArgs e) => MoveToSelectedProcess();

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>선택한 페이지 공정을 확정한다. 선택이 없으면 안내만 하고 창은 유지한다.</summary>
        private void MoveToSelectedProcess()
        {
            var process = gridView.GetFocusedRow() as RoutingProcessDto;
            if (process == null)
            {
                AppMessageBox.Warn(this, Strings.WorkEntryConfirm_SelectProcess, Strings.WorkEntryConfirm_Title);
                return;
            }

            SelectedProcess = process;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
