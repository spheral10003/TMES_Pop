using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using SEOJIN_TMES.Client.ApiClient;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Composition;
using SEOJIN_TMES.Client.Forms.Common;
using SEOJIN_TMES.Client.Theme;
using SEOJIN_TMES.Client.Core;

namespace SEOJIN_TMES.Client.Forms.Notification
{
    /// <summary>
    /// 알림 팝업 — 상단 바의 종 아이콘을 누르면 열린다.
    ///
    /// 구성:
    ///  - 조회 조건 : 사용자명 / 제목 / 읽음여부(전체·읽음·안읽음) → [조회]
    ///  - 좌측      : 알림 목록 그리드(체크박스로 여러 건 선택 가능) + [읽음]
    ///  - 우측      : 목록에서 선택한 한 건의 상세(발신자아이디 · 이름 / 제목 / 본문 HTML) — 읽기 전용
    ///
    /// 본문은 WebBrowser(IE 엔진)로 그린다. 서버가 보내는 본문은 이메일용 완전한 HTML(인라인 CSS + table 레이아웃)이라
    /// 원본 색 · 모양 그대로 보여야 하는데, DevExpress RichEditControl / HtmlContentControl은 인라인 CSS 표를
    /// 정확히 그리지 못할 수 있고, WebView2는 프로젝트에 없어 새 의존성(런타임 설치 포함)이 필요하다.
    ///
    /// 목록에서 행을 옮기면 우측 상세가 바로 따라 바뀐다.
    /// 조회/읽음 처리 실패는 안내 창만 띄우고 창은 그대로 둔다(작업을 이어갈 수 있도록).
    ///
    /// VS 디자이너 호환을 위해 매개변수 없는 생성자만 두고, 실제 의존성은 Initialize()로 주입한다.
    /// </summary>
    public partial class NotificationForm : XtraForm
    {
        //private ClientContext _context;
        private ApiClientFactory _apiClients;
        /// <summary>읽음여부 콤보의 표시 문구 ↔ 조회 조건 값.</summary>
        private readonly List<ReadStateOption> _readStates = new List<ReadStateOption>();
        /// <summary>발신자 아이디 → 이름. 창이 떠 있는 동안 같은 발신자를 다시 조회하지 않기 위한 캐시.</summary>
        private readonly Dictionary<string, string> _senderNames = new Dictionary<string, string>(StringComparer.Ordinal);

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

        public void Init()
        {
            //_context = context;
            _apiClients = Global.Instance.Settings.ApiClients;

            Text = Language.Notification_Title;
            lblTitle.Text = Language.Notification_Title;
            btnSearch.Text = Language.Common_Search;
            btnClose.Text = Language.Common_Close;

            lblUserName.Text = "· " + Language.Notification_UserName;
            lblSubject.Text = "· " + Language.Notification_Subject;
            lblReadState.Text = "· " + Language.Notification_ReadState;

            lblListTitle.Text = Language.Notification_ListTitle;
            btnMarkRead.Text = Language.Notification_MarkRead;

            lblSenderId.Text = "• " + Language.Notification_SenderId;
            lblDetailSubject.Text = "• " + Language.Notification_Subject;

            ApplyTheme();
            BuildReadStateList();
            ConfigureGrid();

            // 조건 입력란에서 Enter를 누르면 바로 조회한다.
            txtUserName.KeyDown += Condition_KeyDown;
            txtSubject.KeyDown += Condition_KeyDown;

            // 목록에서 행이 바뀌면(클릭 · 키보드 이동 모두 포커스 변경으로 들어온다) 우측 상세를 갱신한다.
            gridView.FocusedRowChanged += gridView_FocusedRowChanged;
            gridView.CustomColumnDisplayText += gridView_CustomColumnDisplayText;

            // 창을 열 때는 서버를 부르지 않는다 — 조회는 [조회] 버튼(또는 조건란에서 Enter)으로만 시작한다.
            // 종 아이콘을 누르는 것만으로 조회가 나가면, 알림을 확인할 생각이 없는 클릭에도 서버 부하가 걸린다.
        }

        /// <summary>현재 테마 색을 팝업에 입힌다(포인트 컬러: Indigo).</summary>
        private void ApplyTheme()
        {
            AppTheme.DialogHeader(headerPanel, AppTheme.Indigo);
            DialogChrome.Apply(this, headerPanel);
            DialogChrome.FitPopupSize(this);
            AppTheme.DialogHeaderTitle(lblTitle);

            headerIcon.SvgImage = AppIcons.Get(AppIcons.Bell, Color.White);
            headerIcon.SizeMode = SvgImageSizeMode.Stretch;
            headerIcon.BackColor = Color.Transparent;

            AppTheme.StandardButton(btnSearch);
            AppTheme.StandardButton(btnClose);

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
            AppTheme.StandardButton(btnMarkRead);
            AppTheme.Grid(grid, gridView, AppTheme.Indigo);

            AppTheme.BodyText(lblSenderId);
            AppTheme.BodyText(lblDetailSubject);
            AppTheme.Editor(txtSenderId);
            AppTheme.Editor(txtSenderName);
            AppTheme.Editor(txtDetailSubject);
            // webContent(본문 HTML)는 테마를 입히지 않는다 — 메일 본문이 가진 색 · 배경을 원본 그대로 보여야 한다.
        }

        /// <summary>읽음여부 콤보를 채운다(기본값: 전체).</summary>
        private void BuildReadStateList()
        {
            _readStates.Clear();
            _readStates.Add(new ReadStateOption(NotificationReadState.All, Language.Notification_ReadStateAll));
            _readStates.Add(new ReadStateOption(NotificationReadState.Unread, Language.Notification_ReadStateUnread));
            _readStates.Add(new ReadStateOption(NotificationReadState.Read, Language.Notification_ReadStateRead));

            cboReadState.Properties.Items.Clear();
            foreach (var option in _readStates)
                cboReadState.Properties.Items.Add(option.Text);

            cboReadState.SelectedIndex = 0;
        }

        /// <summary>
        /// 목록 컬럼 구성 + 체크박스 다중 선택([읽음]에서 여러 건을 한 번에 처리하기 위함).
        ///
        /// 처음 보이는 폭에는 No. · 사업부 · 제목만 들어가게 폭을 잡았다(목록 카드 폭 460 기준) —
        /// 읽음여부 · 확인시간 · 작성자 · 작성일자는 가로 스크롤로 본다. 컬럼을 그리드 폭에 맞춰 줄이면
        /// (FitToWidth) 제목이 잘려 읽기 어려워지므로 쓰지 않는다.
        /// </summary>
        private void ConfigureGrid()
        {
            grid.DataSource = new List<NotificationDto>();

            gridView.OptionsSelection.MultiSelect = true;
            gridView.OptionsSelection.MultiSelectMode = GridMultiSelectMode.CheckBoxRowSelect;
            gridView.OptionsSelection.ShowCheckBoxSelectorInColumnHeader = DevExpress.Utils.DefaultBoolean.True;
            gridView.OptionsView.ShowIndicator = false;

            GridColumns.Apply(gridView, Language.Col_No,
                GridColumns.Text(nameof(NotificationDto.CorpCd), Language.Notification_CorpCd, 80),
                GridColumns.Left(nameof(NotificationDto.Subject), Language.Notification_Subject, 270),
                GridColumns.Text(nameof(NotificationDto.IdntyYn), Language.Notification_ReadState, 70),
                GridColumns.Text(nameof(NotificationDto.IdntyTime), Language.Notification_IdntyTime, 140),
                GridColumns.Text(nameof(NotificationDto.CreatedBy), Language.Col_CreatedBy, 90),
                GridColumns.Text(nameof(NotificationDto.CreatedAt), Language.Notification_CreatedAt, 140));
        }

        private void gridView_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e) => ShowDetail(FocusedNotification());

        /// <summary>읽음여부(서버 값 Y/N)를 화면 언어의 읽음/안읽음 문구로 보여 준다. 값 자체는 바꾸지 않는다.</summary>
        private void gridView_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column == null || e.Column.FieldName != nameof(NotificationDto.IdntyYn))
                return;

            var value = e.Value as string;
            if (string.IsNullOrEmpty(value))
                return;

            e.DisplayText = string.Equals(value, "Y", StringComparison.OrdinalIgnoreCase)
                ? Language.Notification_ReadStateRead
                : Language.Notification_ReadStateUnread;
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

                    var result = await _apiClients.NotificationApiClient.GetNotificationsAsync(
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

                    AppMessageBox.Warn(this, ex.Message, Language.Notification_Title);
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
                AppMessageBox.Warn(this, Language.Notification_SelectRow, Language.Notification_Title);
                return;
            }

            var targetIds = selected.Where(n => !n.IsRead && !string.IsNullOrEmpty(n.AlarmSeqId)).Select(n => n.AlarmSeqId).ToList();
            if (targetIds.Count == 0)
            {
                AppMessageBox.Info(this, Language.Notification_AlreadyRead, Language.Notification_Title);
                return;
            }

            // 읽음 처리는 서버 상태를 바꾸므로 [취소]를 주지 않는다 —
            // 중간에 끊으면 어느 건까지 반영됐는지 알 수 없다.
            using (BusyOverlay.Show(this))
            {
                try
                {
                    btnMarkRead.Enabled = false;
                    await _apiClients.NotificationApiClient.MarkAsReadAsync(targetIds);
                }
                catch (ApiException ex)
                {
                    if (IsClosed)
                    {
                        AppLog.Warn($"[알림] 창을 닫은 뒤 읽음 처리 실패 응답이 도착(안내 생략): {ex.Message.Replace(Environment.NewLine, " ")}");
                        return;
                    }

                    AppMessageBox.Warn(this, ex.Message, Language.Notification_Title);
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

        /// <summary>우측 상세 영역(발신자 아이디 · 이름 / 제목 / 본문 HTML). 선택이 없으면 비운다.</summary>
        private void ShowDetail(NotificationDto notification)
        {
            var senderId = notification?.CreatedBy ?? string.Empty;
            txtSenderId.Text = senderId;
            txtDetailSubject.Text = notification?.Subject ?? string.Empty;
            webContent.DocumentText = GetBodyHtml(notification?.Body);

            // 알림 응답에는 발신자 이름이 없다 — 사용자 프로필 API로 따로 받아 채운다.
            if (_senderNames.TryGetValue(senderId, out var cachedName))
            {
                txtSenderName.Text = cachedName;
                return;
            }

            txtSenderName.Text = string.Empty;
            if (!string.IsNullOrEmpty(senderId))
                _ = SetSenderName(senderId);
        }

        /// <summary>
        /// 발신자 이름을 <c>GET gm/users/{userId}</c>(사용자 프로필)로 받아 이름 칸에 채운다.
        ///
        /// 행을 빠르게 옮기면 응답이 뒤섞여 도착할 수 있으므로, 응답이 왔을 때 지금 보고 있는 알림의
        /// 발신자가 같을 때만 화면에 쓴다. 실패해도 안내 창은 띄우지 않는다 — 이름은 보조 정보라
        /// 행을 옮길 때마다 창이 뜨면 목록을 볼 수 없다. 이름 칸을 비워 두고 로그만 남긴다.
        /// </summary>
        private async Task SetSenderName(string senderId)
        {
            string name;
            try
            {
                var profile = await _apiClients.UserProfileApiClient.GetProfileAsync(senderId);
                name = profile?.UserName ?? string.Empty;
            }
            catch (ApiException ex)
            {
                AppLog.Warn($"[알림] 발신자 이름 조회 실패({senderId}) - StatusCode={(int)ex.StatusCode}");
                name = string.Empty;
            }
            catch (Exception ex)
            {
                AppLog.Error($"[알림] 발신자 이름 조회 중 예기치 못한 오류({senderId})", ex);
                name = string.Empty;
            }

            if (IsClosed)
                return;

            // 실패도 빈 이름으로 기억한다 — 같은 발신자 행을 오갈 때마다 실패할 조회를 다시 보내지 않도록.
            _senderNames[senderId] = name;

            if (string.Equals(FocusedNotification()?.CreatedBy, senderId, StringComparison.Ordinal))
                txtSenderName.Text = name;
        }

        /// <summary>
        /// 본문 HTML을 WebBrowser(IE 엔진)에 넣을 수 있게 다듬는다.
        ///
        /// - <b>X-UA-Compatible(IE=edge)를 head 맨 앞에 넣는다.</b> WebBrowser 컨트롤은 레지스트리 설정이 없으면
        ///   IE7 호환 모드로 그려서 인라인 CSS(여백 · 배경 · 테두리)와 표 레이아웃이 어긋난다.
        ///   이 meta는 head 안에서 title/meta 말고 다른 요소보다 앞에 있어야 효과가 있다.
        /// - <b>script 블록은 지운다.</b> 알림 본문은 보여 주기만 하는 메일이라 스크립트가 필요 없고,
        ///   스크립트로 외부 주소를 부르는 것을 막기 위해서다(링크 · 새 창은 Navigating / NewWindow에서 막는다).
        ///
        /// 본문의 색 · 레이아웃 자체는 건드리지 않는다 — 서버가 만든 메일 모양을 그대로 보여 준다.
        /// </summary>
        private string GetBodyHtml(string? body)
        {
            if (string.IsNullOrWhiteSpace(body))
                return string.Empty;

            const string compatibleMeta = "<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">";
            const RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Singleline;

            var html = Regex.Replace(body, @"<script\b.*?</script\s*>", string.Empty, options);

            var head = Regex.Match(html, @"<head\b[^>]*>", options);
            if (head.Success)
                return html.Insert(head.Index + head.Length, compatibleMeta);

            // head가 없는 조각 HTML이면 html 태그 바로 뒤에, 그것도 없으면 맨 앞에 head를 만들어 붙인다.
            var root = Regex.Match(html, @"<html\b[^>]*>", options);
            if (root.Success)
                return html.Insert(root.Index + root.Length, "<head>" + compatibleMeta + "</head>");

            return "<!DOCTYPE html><html><head>" + compatibleMeta + "</head><body>" + html + "</body></html>";
        }

        /// <summary>
        /// 본문 안의 링크 클릭 · 폼 전송 · 스크립트 이동을 모두 막는다 — 알림 본문은 읽기 전용이고,
        /// 생산 현장 PC에서 외부 사이트로 나가는 길을 만들지 않기 위해서다.
        /// DocumentText를 넣을 때 컨트롤이 스스로 about:blank로 이동하므로 그것만 허용한다.
        /// </summary>
        private void webContent_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (e.Url != null && string.Equals(e.Url.Scheme, "about", StringComparison.OrdinalIgnoreCase))
                return;

            e.Cancel = true;
            AppLog.Info($"[알림] 본문에서 외부 이동 차단: {e.Url}");
        }

        /// <summary>본문에서 새 창(IE)이 뜨는 것을 막는다(target="_blank" 링크, Shift+클릭 등).</summary>
        private void webContent_NewWindow(object sender, System.ComponentModel.CancelEventArgs e) => e.Cancel = true;

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
