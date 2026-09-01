using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.Utils.Svg;
using SEOJIN_MEIS.Client.Composition;
using SEOJIN_MEIS.Client.Localization;
using SEOJIN_MEIS.Client.Theme;

namespace SEOJIN_MEIS.Client.Forms.Home
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
        private ClientContext _context;

        private SvgImageBox _icon;
        private LabelControl _title;
        private LabelControl _hint;
        private readonly List<InfoTile> _tiles = new List<InfoTile>();

        public HomeScreen()
        {
            InitializeComponent();
        }

        public void Initialize(ClientContext context)
        {
            _context = context;

            BuildHeader();
            BuildTiles();

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
            _title.Text = "MEIS POP";

            _hint = new LabelControl();
            _hint.Location = new Point(98, 58);
            _hint.Text = Strings.MainForm_WelcomeHint;

            card.Controls.Add(_icon);
            card.Controls.Add(_title);
            card.Controls.Add(_hint);
        }

        /// <summary>정보 타일 세 개. 값은 <see cref="RefreshInfo"/>에서 채운다.</summary>
        private void BuildTiles()
        {
            _tiles.Add(new InfoTile(Strings.MainForm_TileServer, AppTheme.Indigo));
            _tiles.Add(new InfoTile(Strings.MainForm_TileEquipment, AppTheme.Cyan));
            _tiles.Add(new InfoTile(Strings.MainForm_TileUser, AppTheme.Emerald));

            for (var i = 0; i < _tiles.Count; i++)
            {
                _tiles[i].Panel.Location = new Point(28 + i * 260, 110);
                card.Controls.Add(_tiles[i].Panel);
            }
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
            if (_context == null || _tiles.Count < 3)
                return;

            _tiles[0].Value = _context.Settings.ApiBaseUrl;
            _tiles[1].Value = string.Format(Strings.MainForm_EquipmentCount, _context.Settings.Equipments.Count);
            _tiles[2].Value = LoginUserText();
        }

        /// <summary>로그인 사용자 — 이름을 알고 있으면 "이름 (아이디)", 아니면 아이디만.</summary>
        private string LoginUserText()
        {
            var user = _context.CurrentUser;
            if (user == null)
                return string.Empty;

            return string.IsNullOrEmpty(user.UserName) ? user.UserId : $"{user.UserName} ({user.UserId})";
        }

        /// <summary>언어를 바꿨을 때 문구를 다시 읽는다(탭 제목은 MainForm이 갱신한다).</summary>
        public void ApplyLanguage()
        {
            if (_hint == null)
                return;

            _hint.Text = Strings.MainForm_WelcomeHint;
            _tiles[0].Caption = Strings.MainForm_TileServer;
            _tiles[1].Caption = Strings.MainForm_TileEquipment;
            _tiles[2].Caption = Strings.MainForm_TileUser;
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
