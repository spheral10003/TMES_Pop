using System;
using System.ComponentModel;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Core;
using SEOJIN_TMES.Client.Theme;

namespace SEOJIN_TMES.Client.Forms.Home
{
    /// <summary>
    /// 홈 공지 내용 보기 — 공지 상자 2×2 자리를 <b>통째로 덮어</b> 공지 하나의 본문(HTML)을 보여 준다.
    /// 오른쪽 위 X · Esc(HomeScreen이 넘겨 준다)로 빠져나오면 다시 공지 상자들이 보인다.
    ///
    /// 별도 창(팝업)이 아니라 덮개로 둔 이유: 홈은 첫 화면이라 공지를 잠깐 읽고 돌아가는 동선이 대부분인데,
    /// 창을 띄우면 창 위치 · 크기를 매번 다시 맞춰 봐야 하고 현장 터치 화면에서 닫기가 번거롭다.
    ///
    /// 본문 표시는 알림 화면(NotificationForm)의 webContent와 같은 방식이다 —
    /// <see cref="HtmlBody.ForWebBrowser"/>로 다듬어 WebBrowser에 넣고, 링크 이동 · 새 창은 막는다.
    /// NoticeBox처럼 코드로 조립한다(HomeScreen.Designer에는 담을 자리(noticeArea)만 있다).
    /// </summary>
    internal sealed class NoticeViewer
    {
        private readonly PanelControl _card;
        private readonly PanelControl _accentBar;
        private readonly PanelControl _header;
        private readonly PanelControl _headerText;
        private readonly LabelControl _title;
        private readonly LabelControl _meta;
        private readonly SimpleButton _btnCloseX;
        private readonly WebBrowser _web;

        /// <summary>로딩 중에 다음 본문이 들어오면 여기 두었다가 로딩이 끝난 뒤 넣는다(<see cref="SetBodyHtml"/> 참고).</summary>
        private string _pendingBodyHtml;

        /// <summary>noticeArea에 넣는 바깥 패널. 평소에는 숨겨 둔다.</summary>
        public PanelControl Panel { get; }

        /// <summary>내용 보기가 떠 있는지 — HomeScreen이 Esc를 넘길지 판단할 때 본다.</summary>
        public bool IsOpen => Panel.Visible;

        public NoticeViewer()
        {
            Panel = new PanelControl();
            Panel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            Panel.Dock = DockStyle.Fill;
            Panel.Padding = new Padding(8, 4, 8, 8);
            Panel.Visible = false;

            _card = new PanelControl();
            _card.Dock = DockStyle.Fill;

            _accentBar = new PanelControl();
            _accentBar.Dock = DockStyle.Top;
            _accentBar.Height = 3;

            // 머리: 왼쪽에 제목 · (유형 · 작성일시), 오른쪽에 X.
            _header = new PanelControl();
            _header.Dock = DockStyle.Top;
            _header.Height = 64;
            // 위아래 여백 10 → X 버튼(Dock Right) 높이가 64 - 20 = 44가 되어 폭 44와 같은 정사각형이 된다.
            _header.Padding = new Padding(16, 10, 10, 10);

            _headerText = new PanelControl();
            _headerText.Dock = DockStyle.Fill;

            _title = new LabelControl();
            _title.AutoSizeMode = LabelAutoSizeMode.None;
            _title.AutoEllipsis = true;
            _title.Dock = DockStyle.Top;
            _title.Height = 28;

            _meta = new LabelControl();
            _meta.AutoSizeMode = LabelAutoSizeMode.None;
            _meta.AutoEllipsis = true;
            _meta.Dock = DockStyle.Top;
            _meta.Height = 20;

            _btnCloseX = new SimpleButton();
            _btnCloseX.Dock = DockStyle.Right;
            _btnCloseX.Width = 44;
            _btnCloseX.Click += BtnClose_Click;

            _web = new WebBrowser();
            _web.Dock = DockStyle.Fill;
            _web.AllowWebBrowserDrop = false;
            _web.IsWebBrowserContextMenuEnabled = false;
            _web.ScriptErrorsSuppressed = true;
            _web.WebBrowserShortcutsEnabled = false;
            _web.Navigating += Web_Navigating;
            _web.NewWindow += Web_NewWindow;
            _web.DocumentCompleted += Web_DocumentCompleted;

            // 나중에 Add한 컨트롤이 먼저 도킹된다.
            // 제목 줄 안: 일시(Top)를 먼저, 제목(Top)을 나중에 넣어야 제목이 위로 간다.
            _headerText.Controls.Add(_meta);
            _headerText.Controls.Add(_title);
            // 머리: 글자 칸(Fill) → X(Right).
            _header.Controls.Add(_headerText);
            _header.Controls.Add(_btnCloseX);
            // 카드: 본문(Fill) → 머리(Top) → 주황 윗선(Top, 맨 위).
            _card.Controls.Add(_web);
            _card.Controls.Add(_header);
            _card.Controls.Add(_accentBar);

            Panel.Controls.Add(_card);
            Panel.Disposed += Panel_Disposed;

            ApplyLanguage();
        }

        /// <summary>공지 하나를 연다. <paramref name="typeCaption"/>은 그 공지가 있던 상자 이름(시스템공지 등).</summary>
        public void Open(NoticeDto notice, string typeCaption)
        {
            _title.Text = notice.Subject ?? string.Empty;
            _meta.Text = string.IsNullOrEmpty(notice.CreatedAtText) ? typeCaption : $"{typeCaption}  ·  {notice.CreatedAtText}";

            Panel.Visible = true;
            Panel.BringToFront();
            SetBodyHtml(HtmlBody.ForWebBrowser(notice.Body));
        }

        /// <summary>내용 보기를 닫고 공지 상자들로 돌아간다. 다음에 열 때 앞 본문이 잠깐 비치지 않게 비워 둔다.</summary>
        public void Close()
        {
            Panel.Visible = false;
            SetBodyHtml(string.Empty);
        }

        private void BtnClose_Click(object sender, EventArgs e) => Close();

        public void ApplyLanguage()
        {
            _btnCloseX.ToolTip = Language.Common_Close;
        }

        public void ApplyTheme()
        {
            AppTheme.ContentSurface(Panel);
            AppTheme.CardPanel(_card);
            AppTheme.NoticeAccentBar(_accentBar);
            AppTheme.PlainPanel(_header);
            AppTheme.PlainPanel(_headerText);
            AppTheme.BodyText(_title, true);
            _title.Appearance.Font = AppTheme.TitleFont;
            AppTheme.Caption(_meta);
            _meta.Appearance.Font = AppTheme.BodyFont;
            AppTheme.IconButton(_btnCloseX, AppIcons.Close, AppTheme.TextMuted, 22);
            // 글자가 없는 버튼이라 그림을 버튼 한가운데에 둔다(기본 위치는 왼쪽에 붙는다).
            _btnCloseX.ImageOptions.Location = ImageLocation.MiddleCenter;
            // 본문(WebBrowser)은 테마를 입히지 않는다 — 서버가 만든 공지의 색 · 배경을 원본 그대로 보여야 한다.
        }

        /// <summary>
        /// 본문을 WebBrowser에 넣는다. 앞 본문이 아직 로딩 중이면 바로 넣지 않고 로딩이 끝난 뒤 넣는다 —
        /// 로딩 중에 DocumentText를 다시 넣으면 두 로딩이 엉켜 본문이 빈 화면으로 남는다(알림 화면에서 실험으로 확인).
        /// </summary>
        private void SetBodyHtml(string html)
        {
            var state = _web.ReadyState;
            if (state != WebBrowserReadyState.Uninitialized && state != WebBrowserReadyState.Complete)
            {
                _pendingBodyHtml = html;
                return;
            }

            _pendingBodyHtml = null;
            _web.DocumentText = html;
        }

        private void Web_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (_pendingBodyHtml == null || Panel.IsDisposed)
                return;

            var html = _pendingBodyHtml;
            _pendingBodyHtml = null;
            _web.DocumentText = html;
        }

        /// <summary>
        /// 본문 안의 링크 클릭 · 폼 전송 · 스크립트 이동을 모두 막는다 — 공지 본문은 읽기 전용이고,
        /// 생산 현장 PC에서 외부 사이트로 나가는 길을 만들지 않기 위해서다.
        /// DocumentText를 넣을 때 컨트롤이 스스로 about:blank로 이동하므로 그것만 허용한다.
        /// </summary>
        private void Web_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (e.Url != null && string.Equals(e.Url.Scheme, "about", StringComparison.OrdinalIgnoreCase))
                return;

            e.Cancel = true;
            AppLog.Info($"[공지] 본문에서 외부 이동 차단: {e.Url}");
        }

        /// <summary>본문에서 새 창(IE)이 뜨는 것을 막는다(target="_blank" 링크, Shift+클릭 등).</summary>
        private void Web_NewWindow(object sender, CancelEventArgs e) => e.Cancel = true;

        private void Panel_Disposed(object sender, EventArgs e)
        {
            _btnCloseX.Click -= BtnClose_Click;
            _web.Navigating -= Web_Navigating;
            _web.NewWindow -= Web_NewWindow;
            _web.DocumentCompleted -= Web_DocumentCompleted;
        }
    }
}
