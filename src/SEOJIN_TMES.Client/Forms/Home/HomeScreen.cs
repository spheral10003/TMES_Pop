using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.Utils.Svg;
using SEOJIN_TMES.Client.ApiClient;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Composition;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Theme;
using SEOJIN_TMES.Client.Core;

namespace SEOJIN_TMES.Client.Forms.Home
{
    /// <summary>
    /// 홈 화면 — 접속 서버 · 등록 설비 수 · 로그인 사용자처럼 <b>지금 실제로 알고 있는 정보</b>만 보여준다.
    ///
    /// 예전에는 <c>MainForm</c>이 직접 만든 패널을 "열린 탭이 없을 때만" 띄웠기 때문에,
    /// 화면을 하나라도 열면 홈 정보를 다시 볼 방법이 없었다.
    /// 지금은 다른 화면과 똑같이 <b>탭으로 열리는 UserControl</b>이라, 여러 화면을 띄워 둔 상태에서도
    /// 홈 탭을 눌러 언제든 돌아올 수 있다(홈 탭은 닫히지 않는다).
    ///
    /// 표시값은 로그인 사용자와 설정에서 오므로 <see cref="RefreshInfo"/>로 다시 채울 수 있게 두었다
    /// (프로필 조회가 늦게 도착하면 사용자 타일이 아이디에서 이름으로 바뀐다).
    /// </summary>
    public partial class HomeScreen : XtraUserControl, IThemedScreen
    {
        private Setting _appSet;
        private Devices _equip;
        private SvgImageBox _icon;
        private LabelControl _title;
        private LabelControl _hint;
        private readonly List<InfoTile> _tiles = new List<InfoTile>();
        private readonly List<NoticeBox> _noticeBoxes = new List<NoticeBox>();
        private TableLayoutPanel _noticeGrid;
        private NoticeViewer _noticeViewer;

        /// <summary>
        /// 공지 상자 하나에 보여 줄 최대 건수 — 최근(작성일시 내림차순) 것부터.
        /// 1920×1080에서 상자 하나에 스크롤 없이 7줄, 1280×1024에서 6줄이 들어가서 작은 쪽에 맞췄다.
        /// </summary>
        private const int MaxNoticeRows = 6;

        /// <summary>공지 조회가 진행 중인지. 탭을 빠르게 오가며 조회가 겹쳐 나가지 않게 막는다.</summary>
        private bool _noticeLoading;

        public HomeScreen()
        {
            InitializeComponent();
        }

        public void Init()
        {
            //_context = context;
            _appSet = Global.Instance.Settings;
            _equip = Global.Instance.Devices;
            BuildHeader();
            BuildTiles();
            BuildNotices();
            VisibleChanged += HomeScreen_VisibleChanged;

            ApplyTheme();
            RefreshInfo();
        }

        /// <summary>브랜드 아이콘 + 제목 + 안내 문구.</summary>
        private void BuildHeader()
        {
            _icon = new SvgImageBox();
            _icon.SizeMode = SvgImageSizeMode.Stretch;
            _icon.Size = new Size(54, 54);
            _icon.Location = new Point(28, 26);
            _icon.BackColor = Color.Transparent;

            _title = new LabelControl();
            _title.Location = new Point(96, 32);
            _title.Text = "TMES POP";

            _hint = new LabelControl();
            _hint.Location = new Point(98, 58);
            _hint.Text = Language.MainForm_WelcomeHint;

            card.Controls.Add(_icon);
            card.Controls.Add(_title);
            card.Controls.Add(_hint);
        }

        /// <summary>
        /// 정보 타일 정의 — <b>표시 순서대로</b> 둔다.
        /// 제목(다국어)·값·강조색을 한 자리에 모아 두는 이유는, 예전처럼 생성·값 채우기·언어 갱신을
        /// 세 군데에서 인덱스로 다루면 타일을 하나 끼워 넣을 때 조용히 어긋나기 때문이다.
        /// </summary>
        private static readonly TileSpec[] TileSpecs =
        {
            new TileSpec(() => Language.MainForm_TileServer,      s => s.ServerText(),          AppTheme.Indigo),
            new TileSpec(() => Language.MainForm_TileWorkstation, s => s.WorkstationText(),     AppTheme.Violet),
            new TileSpec(() => Language.MainForm_TileEquipment,   s => s.EquipmentText(),       AppTheme.Cyan),
            new TileSpec(() => Language.MainForm_TileUser,        s => s.LoginUserText(),       AppTheme.Emerald)
        };

        /// <summary>타일 폭(240) + 사이 간격(20).</summary>
        private const int TileStride = 260;

        private void BuildTiles()
        {
            for (var i = 0; i < TileSpecs.Length; i++)
            {
                var tile = new InfoTile(TileSpecs[i].Caption(), TileSpecs[i].Accent);
                tile.Panel.Location = new Point(28 + i * TileStride, 110);
                card.Controls.Add(tile.Panel);
                _tiles.Add(tile);
            }
        }

        /// <summary>
        /// 홈에 보여 줄 공지 유형 — <b>2×2 칸에 놓이는 순서대로</b>(왼쪽 위 → 오른쪽 위 → 왼쪽 아래 → 오른쪽 아래).
        /// 서버는 NTC05~07(생산 · 품질 · 안전)도 주지만 홈에는 이 네 가지만 둔다.
        /// 배치는 웹 MES 홈과 같게 일반 · 시스템 / 중요 · 긴급으로 맞췄다.
        /// </summary>
        private static readonly NoticeSpec[] NoticeSpecs =
        {
            new NoticeSpec(NoticeType.General,   () => Language.Home_NoticeGeneral),
            new NoticeSpec(NoticeType.System,    () => Language.Home_NoticeSystem),
            new NoticeSpec(NoticeType.Important, () => Language.Home_NoticeImportant),
            new NoticeSpec(NoticeType.Urgent,    () => Language.Home_NoticeUrgent)
        };

        /// <summary>공지 상자 네 개를 2×2 표에 채운다. 칸 크기는 비율(50%)이라 창 크기에 따라 함께 늘고 준다.</summary>
        private void BuildNotices()
        {
            _noticeGrid = new TableLayoutPanel();
            _noticeGrid.Dock = DockStyle.Fill;
            _noticeGrid.ColumnCount = 2;
            _noticeGrid.RowCount = 2;
            _noticeGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            _noticeGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            _noticeGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            _noticeGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            for (var i = 0; i < NoticeSpecs.Length; i++)
            {
                var box = new NoticeBox(NoticeSpecs[i].NtcType);
                box.Caption = NoticeSpecs[i].Caption();
                box.NoticeOpened += NoticeBox_NoticeOpened;
                _noticeGrid.Controls.Add(box.Panel, i % 2, i / 2);
                _noticeBoxes.Add(box);
            }

            // 내용 보기는 공지 상자 표와 같은 자리(Fill)에 겹쳐 두고, 열 때 앞으로 올린다.
            _noticeViewer = new NoticeViewer();
            noticeArea.Controls.Add(_noticeGrid);
            noticeArea.Controls.Add(_noticeViewer.Panel);
        }

        /// <summary>
        /// 공지 줄을 더블클릭하면 공지 상자 자리를 덮어 본문을 보여 준다.
        /// 목록에는 본문이 없으므로(<c>exceptBodyYn=Y</c>) 제목부터 띄우고 본문은 <c>GET gm/notices/{sysSeqId}</c>로 받아 넣는다.
        /// </summary>
        private async void NoticeBox_NoticeOpened(object sender, NoticeDto notice)
        {
            var index = sender is NoticeBox box ? _noticeBoxes.IndexOf(box) : -1;
            var typeCaption = index < 0 ? string.Empty : NoticeSpecs[index].Caption();
            _noticeViewer.Open(notice, typeCaption);

            var api = _appSet?.ApiClients?.NoticeApiClient;
            if (api == null)
                return;

            var sysSeqId = notice.SysSeqId;
            try
            {
                var detail = await api.GetNotice(sysSeqId, _appSet.CurrentUser?.UserId);

                // 받는 사이 닫았거나 다른 공지를 열었으면 버린다 — 늦게 온 본문이 지금 보는 공지를 덮지 않게.
                if (!IsCurrentNotice(sysSeqId))
                    return;

                _noticeViewer.ShowBody(detail?.Body);
            }
            catch (ApiException ex)
            {
                AppLog.Warn($"홈 공지 본문 조회 실패(sysSeqId={sysSeqId}) - {ex.ToLogText()}");
                if (IsCurrentNotice(sysSeqId))
                    _noticeViewer.ShowBodyLoadFailed();
            }
        }

        private bool IsCurrentNotice(string sysSeqId) =>
            !IsDisposed && !Disposing && _noticeViewer.IsOpen && _noticeViewer.OpenedSysSeqId == sysSeqId;

        /// <summary>내용 보기가 떠 있으면 Esc로도 닫는다(X와 같은 동작).</summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape && _noticeViewer != null && _noticeViewer.IsOpen)
            {
                _noticeViewer.Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// 한 유형의 최근 공지 <see cref="MaxNoticeRows"/>건(작성일시 내림차순).
        /// 새 글 여부(newYn)와 상관없이 최근 것부터 채운다 — 상자에 스크롤 없이 들어가는 만큼만 보여 준다.
        /// </summary>
        private static List<NoticeDto> RecentNotices(List<NoticeDto> notices, string ntcType)
        {
            return notices.Where(n => n.NtcType == ntcType)
                          .OrderByDescending(n => n.CreatedTime ?? DateTime.MinValue)
                          .Take(MaxNoticeRows)
                          .ToList();
        }

        /// <summary>
        /// 홈 탭이 보일 때마다 공지를 다시 받는다 — 다른 화면에서 일하다 홈으로 돌아오면 새 공지가 보이게.
        /// 처음 탭이 열릴 때도 여기서 첫 조회가 나간다(MainForm이 Init 뒤에 탭을 띄운다).
        /// </summary>
        private void HomeScreen_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
                LoadNotices();
        }

        /// <summary>
        /// 공지 전체를 한 번에 받아 유형별 상자에 최근 <see cref="MaxNoticeRows"/>건씩 나눠 넣는다(유형 조건 없이 1회 호출).
        /// 실패해도 창을 띄우지 않는다 — 홈은 첫 화면이라 서버가 잠깐 안 될 때마다 안내 창이 뜨면 일을 시작할 수 없다.
        /// 대신 상자마다 "불러오지 못했습니다"를 보여 주고 로그를 남긴다.
        /// </summary>
        private async void LoadNotices()
        {
            var api = _appSet?.ApiClients?.NoticeApiClient;
            if (api == null || _noticeLoading)
                return;

            _noticeLoading = true;
            try
            {
                var notices = await api.GetNotices();
                if (IsDisposed || Disposing)
                    return;

                ShowNotices(notices);
            }
            catch (ApiException ex)
            {
                AppLog.Warn($"홈 공지 조회 실패 - {ex.ToLogText()}");
                if (IsDisposed || Disposing)
                    return;

                foreach (var box in _noticeBoxes)
                    box.SetLoadFailed();
            }
            finally
            {
                _noticeLoading = false;
            }
        }

        /// <summary>
        /// 받은 공지 목록을 유형별 상자에 최근 <see cref="MaxNoticeRows"/>건씩 나눠 넣는다.
        /// 홈 탭이 스스로 조회했을 때와, MainForm의 공지 폴링이 새 공지를 찾았을 때 모두 여기로 온다
        /// (홈 탭이 가려져 있어도 채워 두면 돌아왔을 때 바로 보인다 — 돌아올 때 다시 조회도 한다).
        /// </summary>
        public void ShowNotices(List<NoticeDto> notices)
        {
            if (IsDisposed || Disposing || notices == null)
                return;

            foreach (var box in _noticeBoxes)
                box.SetItems(RecentNotices(notices, box.NtcType));
        }

        /// <summary>
        /// 타일 값을 지금 상태로 다시 채운다.
        /// 로그인 직후에는 프로필이 없어 아이디만 나오고, 프로필을 받은 뒤 다시 부르면 이름이 붙는다.
        ///
        /// <c>Control.Refresh()</c>를 재정의하지 않고 별도 이름을 쓰는 이유:
        /// 그쪽은 WinForms가 다시 그리려고 스스로 부르는 메서드라, 값 갱신을 얹으면
        /// 의도하지 않은 시점에 설정·세션을 다시 읽게 된다.
        /// </summary>
        public void RefreshInfo()
        {
            if (_appSet == null)
                return;

            for (var i = 0; i < _tiles.Count; i++)
                _tiles[i].Value = TileSpecs[i].Value(this);
        }

        /// <summary>로그인 사용자 — 이름을 알고 있으면 "이름 (아이디)", 아니면 아이디만.</summary>
        private string LoginUserText()
        {
            var user = _appSet.CurrentUser;
            if (user == null)
                return string.Empty;

            return string.IsNullOrEmpty(user.UserName) ? user.UserId : $"{user.UserName} ({user.UserId})";
        }

        /// <summary>
        /// 메인 화면이 뜰 때 고른 워크스테이션.
        /// 서버가 내려주는 값이 아니라 <b>접속할 때 사용자가 선택한 값</b>이라 세션에서 그대로 읽는다.
        /// 고르지 않고 접속했으면 빈칸 대신 "-"로 두어 타일이 비어 보이지 않게 한다.
        /// </summary>
        private string WorkstationText()
        {
            var workstation = _appSet.CurrentUser?.Workstation;
            return string.IsNullOrWhiteSpace(workstation) ? "-" : workstation;
        }

        private string ServerText() => _appSet.AppSet.ApiBaseUrl;

        private string EquipmentText() =>
            string.Format(Language.MainForm_EquipmentCount, _equip.Manager.Count);

        /// <summary>언어를 바꿨을 때 문구를 다시 읽는다(탭 제목은 MainForm이 갱신한다).</summary>
        public void ApplyLanguage()
        {
            if (_hint == null)
                return;

            _hint.Text = Language.MainForm_WelcomeHint;

            for (var i = 0; i < _tiles.Count; i++)
                _tiles[i].Caption = TileSpecs[i].Caption();

            for (var i = 0; i < _noticeBoxes.Count; i++)
            {
                _noticeBoxes[i].Caption = NoticeSpecs[i].Caption();
                _noticeBoxes[i].ApplyLanguage();
            }

            _noticeViewer?.ApplyLanguage();

            RefreshInfo();
        }

        public void ApplyTheme()
        {
            BackColor = AppTheme.Surface;
            AppTheme.CardPanel(card);

            if (_icon != null)
                _icon.SvgImage = AppIcons.Get(AppIcons.Brand, AppTheme.Violet);

            if (_title != null)
            {
                AppTheme.SectionTitle(_title, AppTheme.Violet);
                _title.Appearance.Font = AppTheme.TitleFont;
            }

            if (_hint != null)
                AppTheme.Caption(_hint);

            foreach (var tile in _tiles)
                tile.ApplyTheme();

            AppTheme.ContentSurface(noticeArea);
            if (_noticeGrid != null)
                _noticeGrid.BackColor = AppTheme.Surface;

            foreach (var box in _noticeBoxes)
                box.ApplyTheme();

            _noticeViewer?.ApplyTheme();
        }

        /// <summary>공지 상자 하나의 정의 — 공지 유형 코드와 제목(언어가 바뀌면 다시 읽으므로 함수로).</summary>
        private sealed class NoticeSpec
        {
            public string NtcType { get; }
            public Func<string> Caption { get; }

            public NoticeSpec(string ntcType, Func<string> caption)
            {
                NtcType = ntcType;
                Caption = caption;
            }
        }

        /// <summary>
        /// 타일 하나의 정의. 제목은 언어가 바뀌면 다시 읽어야 하므로 <see cref="Func{TResult}"/>로,
        /// 값은 세션·설정이 바뀌면 다시 읽어야 하므로 화면을 받는 함수로 둔다.
        /// </summary>
        private sealed class TileSpec
        {
            public Func<string> Caption { get; }
            public Func<HomeScreen, string> Value { get; }
            public Color Accent { get; }

            public TileSpec(Func<string> caption, Func<HomeScreen, string> value, Color accent)
            {
                Caption = caption;
                Value = value;
                Accent = accent;
            }
        }

        /// <summary>
        /// 정보 타일 하나(왼쪽 색 막대 + 제목 + 값).
        /// 라벨을 필드로 들고 있어야 값·문구를 나중에 바꿔 넣을 수 있다 —
        /// 예전처럼 <c>Controls.OfType&lt;LabelControl&gt;()</c>으로 순서에 기대어 찾으면
        /// 컨트롤을 하나 추가하는 순간 조용히 어긋난다.
        /// </summary>
        private sealed class InfoTile
        {
            private readonly LabelControl _caption;
            private readonly LabelControl _value;

            public PanelControl Panel { get; }

            public InfoTile(string caption, Color accent)
            {
                Panel = new PanelControl();
                Panel.Size = new Size(240, 84);

                var bar = new PanelControl();
                bar.Dock = DockStyle.Left;
                bar.Width = 5;
                bar.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
                bar.Appearance.BackColor = accent;
                bar.Appearance.Options.UseBackColor = true;

                _caption = new LabelControl();
                _caption.Location = new Point(20, 18);
                _caption.Text = caption;

                _value = new LabelControl();
                _value.Location = new Point(20, 42);
                // 값이 잠깐 빈 문자열이 되어도 폭이 0으로 줄지 않도록 크기를 고정한다(CLAUDE.md 5번).
                _value.AutoSizeMode = LabelAutoSizeMode.None;
                _value.Size = new Size(205, 20);
                _value.AutoEllipsis = true;

                Panel.Controls.Add(_caption);
                Panel.Controls.Add(_value);
                Panel.Controls.Add(bar);
            }

            public string Caption
            {
                set => _caption.Text = value;
            }

            public string Value
            {
                set => _value.Text = value ?? string.Empty;
            }

            public void ApplyTheme()
            {
                AppTheme.CardPanel(Panel);
                AppTheme.Caption(_caption);
                AppTheme.BodyText(_value, true);
            }
        }
    }
}
