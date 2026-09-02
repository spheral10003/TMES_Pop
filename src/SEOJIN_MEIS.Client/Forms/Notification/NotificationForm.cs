using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using SEOJIN_MEIS.Client.ApiClient;
using SEOJIN_MEIS.Client.Common.Logging;
using SEOJIN_MEIS.Client.Common.Models;
using SEOJIN_MEIS.Client.Composition;
using SEOJIN_MEIS.Client.Forms.Common;
using SEOJIN_MEIS.Client.Localization;
using SEOJIN_MEIS.Client.Theme;

namespace SEOJIN_MEIS.Client.Forms.Notification
{
    /// <summary>
    /// 알림 팝업 — 상단 바의 종 아이콘을 누르면 열린다.
    ///
    /// 구성:
    ///  - 조회 조건 : 사용자명 / 제목 / 읽음여부(전체·읽음·안읽음) → [조회]
    ///  - 좌측      : 알림 목록 그리드(체크박스로 여러 건 선택 가능) + [읽음]
    ///  - 우측      : 목록에서 선택한 한 건의 상세(발신자아이디 / 제목 / 내용) — 읽기 전용
    ///
    /// 목록에서 행을 옮기면 우측 상세가 바로 따라 바뀐다.
    /// 조회/읽음 처리 실패는 안내 창만 띄우고 창은 그대로 둔다(작업을 이어갈 수 있도록).
    ///
    /// VS 디자이너 호환을 위해 매개변수 없는 생성자만 두고, 실제 의존성은 Initialize()로 주입한다.
    /// </summary>
    public partial class NotificationForm : XtraForm
    {
        private ClientContext _context;

        /// <summary>읽음여부 콤보의 표시 문구 ↔ 조회 조건 값.</summary>
        private readonly List<ReadStateOption> _readStates = new List<ReadStateOption>();

        /// <summary>
        /// 이 창이 이미 닫혀 사라졌는지. <b>await 뒤에는 반드시 확인해야 한다.</b>
        ///
        /// 이 창은 <c>using (var form = new NotificationForm())</c>으로 띄우므로 닫는 즉시 Dispose된다.
        /// 그런데 조회가 서버 응답을 기다리는 중에 사용자가 창을 닫을 수 있고,
        /// 응답(특히 실패)이 늦게 도착하면 이미 없어진 창에 결과를 채우거나
        /// 그 창을 소유자로 안내 창을 띄우려다 ObjectDisposedException이 난다
        /// (ShowDialog(owner)가 Dispose된 owner의 Handle을 건드린다).
        /// </summary>
        private bool IsClosed => IsDisposed || Disposing;

        public NotificationForm()
        {
            InitializeComponent();
        }

        public void Initialize(ClientContext context)
        {
            _context = context;

            Text = Strings.Notification_Title;
            lblTitle.Text = Strings.Notification_Title;
            btnSearch.Text = Strings.Common_Search;
            btnClose.Text = Strings.Common_Close;

            lblUserName.Text = "· " + Strings.Notification_UserName;
            lblSubject.Text = "· " + Strings.Notification_Subject;
            lblReadState.Text = "· " + Strings.Notification_ReadState;

            lblListTitle.Text = Strings.Notification_ListTitle;
            btnMarkRead.Text = Strings.Notification_MarkRead;

            lblSenderId.Text = "· " + Strings.Notification_SenderId;
            lblDetailSubject.Text = "· " + Strings.Notification_Subject;
            lblContent.Text = "· " + Strings.Notification_Content;

            ApplyTheme();
            BuildReadStateList();
            ConfigureGrid();

            // 조건 입력란에서 Enter를 누르면 바로 조회한다.
            txtUserName.KeyDown += Condition_KeyDown;
            txtSubject.KeyDown += Condition_KeyDown;

            // 목록에서 행이 바뀌면 우측 상세를 갱신한다.
            gridView.FocusedRowChanged += (s, e) => ShowDetail(FocusedNotification());

            // 창을 열 때는 서버를 부르지 않는다 — 조회는 [조회] 버튼(또는 조건란에서 Enter)으로만 시작한다.
            // 종 아이콘을 누르는 것만으로 조회가 나가면, 알림을 확인할 생각이 없는 클릭에도 서버 부하가 걸린다.
        }

        /// <summary>현재 테마 색을 팝업에 입힌다(포인트 컬러: Indigo).</summary>
        private void ApplyTheme()
        {
            AppTheme.DialogHeader(headerPanel, AppTheme.Indigo);
            AppTheme.DialogHeaderTitle(lblTitle);

            headerIcon.SvgImage = AppIcons.Get(AppIcons.Bell, Color.White);
            headerIcon.SizeMode = SvgImageSizeMode.Stretch;
            headerIcon.BackColor = Color.Transparent;

            // 헤더 색 띠 위에 얹히지만 버튼 자체가 흰 바탕이라 그대로 두어도 읽힌다
            // (MeisButton은 스스로 흰 바탕 + 회색 테두리로 그린다).
            btnSearch.Icon = AppIcons.Search;

            AppTheme.ContentSurface(bodyPanel);
            spacerFilter.BackColor = AppTheme.Surface;
            spacerMiddle.BackColor = AppTheme.Surface;

            AppTheme.CardPanel(filterCard);
            AppTheme.CardPanel(listCard);
            AppTheme.CardPanel(detailCard);
            AppTheme.PlainPanel(listHeader);

            AppTheme.BodyText(lblUserName);
            AppTheme.BodyText(lblSubject);
            AppTheme.BodyText(lblReadState);
            AppTheme.Editor(txtUserName);
            AppTheme.Editor(txtSubject);
            AppTheme.Editor(cboReadState);

            AppTheme.SectionTitle(lblListTitle, AppTheme.Indigo);
            btnMarkRead.Icon = AppIcons.Check;
            AppTheme.Grid(grid, gridView, AppTheme.Indigo);

            AppTheme.BodyText(lblSenderId);
            AppTheme.BodyText(lblDetailSubject);
            AppTheme.BodyText(lblContent);
            AppTheme.Editor(txtSenderId);
            AppTheme.Editor(txtDetailSubject);
            AppTheme.Editor(memoContent);
        }

        /// <summary>읽음여부 콤보를 채운다(기본값: 전체).</summary>
        private void BuildReadStateList()
        {
            _readStates.Clear();
            _readStates.Add(new ReadStateOption(NotificationReadState.All, Strings.Notification_ReadStateAll));
            _readStates.Add(new ReadStateOption(NotificationReadState.Unread, Strings.Notification_ReadStateUnread));
            _readStates.Add(new ReadStateOption(NotificationReadState.Read, Strings.Notification_ReadStateRead));

            cboReadState.Properties.Items.Clear();
            foreach (var option in _readStates)
                cboReadState.Properties.Items.Add(option.Text);

            cboReadState.SelectedIndex = 0;
        }

        /// <summary>목록 컬럼 구성 + 체크박스 다중 선택([읽음]에서 여러 건을 한 번에 처리하기 위함).</summary>
        private void ConfigureGrid()
        {
            grid.DataSource = new List<NotificationDto>();

            gridView.OptionsSelection.MultiSelect = true;
            gridView.OptionsSelection.MultiSelectMode = GridMultiSelectMode.CheckBoxRowSelect;
            gridView.OptionsSelection.ShowCheckBoxSelectorInColumnHeader = DevExpress.Utils.DefaultBoolean.True;
            gridView.OptionsView.ShowIndicator = false;

            GridColumns.Apply(gridView, Strings.Col_No,
                GridColumns.Text("CorpCode", Strings.Col_CorpCode, 66),
                GridColumns.Text("FormCode", Strings.Col_FormCode, 96),
                GridColumns.Left("Subject", Strings.Notification_Subject, 160),
                GridColumns.Text("SenderId", Strings.Notification_SenderId, 92),
                GridColumns.DateTime("SentAt", Strings.Notification_SentAt, 110),
                GridColumns.Text("ReadStateText", Strings.Notification_ReadState, 58));
        }

        private void Condition_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            e.Handled = true;
            _ = SearchAsync();
        }

        private async void btnSearch_Click(object sender, EventArgs e) => await SearchAsync();
        private async void btnMarkRead_Click(object sender, EventArgs e) => await MarkSelectedAsReadAsync();

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// 조건으로 알림을 조회해 좌측 목록에 채운다.
        /// 조건이 모두 비어 있어도 그대로 조회한다(알림은 전체 목록을 보는 게 기본 동작이다).
        /// </summary>
        private async Task SearchAsync()
        {
            using (var busy = BusyOverlay.Show(this, cancelable: true))
            {
                try
                {
                    btnSearch.Enabled = false;

                    var result = await _context.ApiClients.NotificationApiClient.GetNotificationsAsync(
                                     txtUserName.Text?.Trim(),
                                     txtSubject.Text?.Trim(),
                                     SelectedReadState(),
                                     busy.Token)
                                 ?? new List<NotificationDto>();

                    if (IsClosed)
                        return;

                    grid.DataSource = result;
                    gridView.ClearSelection();

                    if (result.Count > 0)
                        gridView.FocusedRowHandle = 0;

                    // 결과가 0건이면 안내 창 대신 그리드의 "데이터 없음" 표시로 충분하다.
                    ShowDetail(FocusedNotification());
                }
                catch (OperationCanceledException)
                {
                    // 사용자가 [취소]를 눌렀다 — 오류가 아니므로 조용히 끝낸다.
                }
                catch (ApiException ex)
                {
                    if (IsClosed)
                    {
                        // 사용자가 이미 창을 닫았다 — 없어진 창을 소유자로 안내 창을 띄울 수 없으므로 로그만 남긴다.
                        AppLog.Warn($"[알림] 창을 닫은 뒤 조회 실패 응답이 도착(안내 생략): {ex.Message.Replace(Environment.NewLine, " ")}");
                        return;
                    }

                    AppMessageBox.Warn(this, ex.Message, Strings.Notification_Title);
                }
                finally
                {
                    if (!IsClosed)
                        btnSearch.Enabled = true;
                }
            }
        }

        /// <summary>
        /// 체크한 알림들을 읽음 처리한다. 이미 읽은 건은 서버에 보내지 않는다.
        /// 처리 후에는 목록/상세가 서버 상태와 어긋나지 않도록 다시 조회한다.
        /// </summary>
        private async Task MarkSelectedAsReadAsync()
        {
            var selected = SelectedNotifications();
            if (selected.Count == 0)
            {
                AppMessageBox.Warn(this, Strings.Notification_SelectRow, Strings.Notification_Title);
                return;
            }

            var targetIds = selected.Where(n => !n.IsRead).Select(n => n.NotificationId).ToList();
            if (targetIds.Count == 0)
            {
                AppMessageBox.Info(this, Strings.Notification_AlreadyRead, Strings.Notification_Title);
                return;
            }

            // 읽음 처리는 서버 상태를 바꾸므로 [취소]를 주지 않는다 —
            // 중간에 끊으면 어느 건까지 반영됐는지 알 수 없다.
            using (BusyOverlay.Show(this))
            {
                try
                {
                    btnMarkRead.Enabled = false;
                    await _context.ApiClients.NotificationApiClient.MarkAsReadAsync(targetIds);
                }
                catch (ApiException ex)
                {
                    if (IsClosed)
                    {
                        AppLog.Warn($"[알림] 창을 닫은 뒤 읽음 처리 실패 응답이 도착(안내 생략): {ex.Message.Replace(Environment.NewLine, " ")}");
                        return;
                    }

                    AppMessageBox.Warn(this, ex.Message, Strings.Notification_Title);
                    return;
                }
                finally
                {
                    if (!IsClosed)
                        btnMarkRead.Enabled = true;
                }
            }

            if (IsClosed)
                return;

            // 대기 표시를 먼저 걷고 나서 다시 조회한다 — SearchAsync가 자기 것을 새로 띄운다.
            await SearchAsync();
        }

        /// <summary>우측 상세 영역. 선택이 없으면 비운다.</summary>
        private void ShowDetail(NotificationDto notification)
        {
            txtSenderId.Text = notification?.SenderId ?? string.Empty;
            txtDetailSubject.Text = notification?.Subject ?? string.Empty;
            memoContent.Text = NormalizeLineBreaks(notification?.Content);
        }

        /// <summary>
        /// 서버 내용에 섞여 오는 줄바꿈을 WinForms 여러 줄 편집기가 알아듣는 CRLF로 맞춘다.
        /// LF만 오면 편집기가 줄을 안 나누고 내용이 한 줄로 붙어 버린다.
        /// </summary>
        private static string NormalizeLineBreaks(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            const string cr = "\r";
            const string lf = "\n";

            return text.Replace(cr + lf, lf).Replace(cr, lf).Replace(lf, Environment.NewLine);
        }

        private NotificationDto FocusedNotification() => gridView.GetFocusedRow() as NotificationDto;

        /// <summary>체크한 행들. 체크가 하나도 없으면 현재 포커스 행 한 건을 대상으로 본다.</summary>
        private List<NotificationDto> SelectedNotifications()
        {
            var rows = gridView.GetSelectedRows()
                .Select(handle => gridView.GetRow(handle) as NotificationDto)
                .Where(row => row != null)
                .ToList();

            if (rows.Count > 0)
                return rows;

            var focused = FocusedNotification();
            return focused == null ? new List<NotificationDto>() : new List<NotificationDto> { focused };
        }

        private NotificationReadState SelectedReadState()
        {
            var index = cboReadState.SelectedIndex;
            return index >= 0 && index < _readStates.Count ? _readStates[index].State : NotificationReadState.All;
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

        /// <summary>읽음여부 콤보 항목 하나(표시 문구 ↔ 조회 조건 값).</summary>
        private class ReadStateOption
        {
            public NotificationReadState State { get; }
            public string Text { get; }

            public ReadStateOption(NotificationReadState state, string text)
            {
                State = state;
                Text = text;
            }
        }
    }
}
