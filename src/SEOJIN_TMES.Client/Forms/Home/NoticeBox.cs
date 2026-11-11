using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Theme;

namespace SEOJIN_TMES.Client.Forms.Home
{
    /// <summary>
    /// 홈 화면의 공지 상자 하나(공지 유형 하나 = 상자 하나).
    /// 제목 줄(폴더 아이콘 + 유형 이름) 아래 주황 윗선이 있는 카드에 공지를 한 줄씩 보여 준다 —
    /// 줄마다 주황 점 · 제목 · 새 글이면 N 배지 · 등록일시.
    ///
    /// 정보 타일(<c>HomeScreen.InfoTile</c>)처럼 코드로 조립한다 — 같은 상자를 유형 수만큼 찍어 내므로
    /// 디자이너 화면으로 따로 두면 네 벌을 똑같이 유지해야 한다.
    /// 공지가 없거나 불러오지 못하면 목록 대신 가운데 안내(폴더 그림 + 문구)를 띄운다.
    /// </summary>
    internal sealed class NoticeBox
    {
        /// <summary>N 배지 지름(px).</summary>
        private const int BadgeDiameter = 16;

        /// <summary>제목 앞 주황 점 지름(px)과 왼쪽 여백 — 점 뒤로 제목이 이어진다.</summary>
        private const int DotDiameter = 6;
        private const int DotLeft = 14;
        private const int TitleLeft = 30;

        private readonly LabelControl _header;
        private readonly PanelControl _body;
        private readonly PanelControl _accentBar;
        private readonly GridControl _grid;
        private readonly GridView _view;
        private readonly GridColumn _colTitle;
        private readonly GridColumn _colNew;
        private readonly GridColumn _colDate;
        private readonly LabelControl _empty;
        private Font _badgeFont;
        private bool _loadFailed;

        /// <summary>이 상자가 보여 주는 공지 유형(<see cref="NoticeType"/>).</summary>
        public string NtcType { get; }

        /// <summary>표 칸(TableLayoutPanel)에 그대로 넣는 바깥 패널.</summary>
        public PanelControl Panel { get; }

        public NoticeBox(string ntcType)
        {
            NtcType = ntcType;

            Panel = new PanelControl();
            Panel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            Panel.Dock = DockStyle.Fill;
            Panel.Padding = new Padding(8, 4, 8, 8);
            Panel.Margin = new Padding(0);
            // 배지 글꼴은 상자가 들고 있는 유일한 GDI 자원이라, 바깥 패널이 정리될 때 함께 정리한다.
            Panel.Disposed += Panel_Disposed;

            _header = new LabelControl();
            _header.AutoSizeMode = LabelAutoSizeMode.None;
            _header.Dock = DockStyle.Top;
            _header.Height = 30;

            _body = new PanelControl();
            _body.Dock = DockStyle.Fill;

            _accentBar = new PanelControl();
            _accentBar.Dock = DockStyle.Top;
            _accentBar.Height = 3;

            _grid = new GridControl();
            _view = new GridView(_grid);
            _grid.MainView = _view;
            _grid.Dock = DockStyle.Fill;

            _colTitle = NewColumn(nameof(NoticeDto.Title), 0);
            _colNew = NewColumn(nameof(NoticeDto.IsNew), 1);
            _colNew.Width = 34;
            _colNew.OptionsColumn.FixedWidth = true;
            _colDate = NewColumn(nameof(NoticeDto.RegisteredAtText), 2);
            _colDate.Width = 150;
            _colDate.OptionsColumn.FixedWidth = true;
            _view.Columns.AddRange(new[] { _colTitle, _colNew, _colDate });
            _view.CustomDrawCell += View_CustomDrawCell;

            _empty = new LabelControl();
            _empty.AutoSizeMode = LabelAutoSizeMode.None;
            _empty.Dock = DockStyle.Fill;

            // 나중에 Add한 컨트롤이 먼저 도킹된다 — 윗선(Top)을 먼저 잡고, 목록과 안내(둘 다 Fill)가 남은 자리를 나눠 쓴다.
            // 목록과 안내는 같은 자리를 겹쳐 쓰고 Visible로 하나만 보인다.
            _body.Controls.Add(_grid);
            _body.Controls.Add(_empty);
            _body.Controls.Add(_accentBar);

            // 본문(Fill)을 먼저, 제목(Top)을 나중에 Add해야 제목이 위를 차지한다.
            Panel.Controls.Add(_body);
            Panel.Controls.Add(_header);

            SetItems(new List<NoticeDto>());
        }

        private static GridColumn NewColumn(string fieldName, int visibleIndex)
        {
            var column = new GridColumn();
            column.FieldName = fieldName;
            column.Visible = true;
            column.VisibleIndex = visibleIndex;
            return column;
        }

        /// <summary>상자 제목(공지 유형 이름). 언어가 바뀌면 다시 넣는다.</summary>
        public string Caption
        {
            set => _header.Text = value;
        }

        /// <summary>공지 목록을 채운다. 비어 있으면 "조회할 데이터가 없습니다." 안내를 띄운다.</summary>
        public void SetItems(List<NoticeDto> items)
        {
            _loadFailed = false;
            _grid.DataSource = items;
            ShowEmpty(items.Count == 0);
        }

        /// <summary>조회 실패 — 목록을 비우고 "불러오지 못했습니다" 안내를 띄운다(창은 띄우지 않는다).</summary>
        public void SetLoadFailed()
        {
            _loadFailed = true;
            _grid.DataSource = new List<NoticeDto>();
            ShowEmpty(true);
        }

        private void ShowEmpty(bool empty)
        {
            _empty.Text = _loadFailed ? Language.Home_NoticeLoadFailed : Language.Home_NoticeEmpty;
            _empty.Visible = empty;
            _grid.Visible = !empty;
        }

        /// <summary>언어를 바꿨을 때 안내 문구를 다시 읽는다(제목은 <see cref="Caption"/>으로 넣는다).</summary>
        public void ApplyLanguage()
        {
            ShowEmpty(_empty.Visible);
        }

        public void ApplyTheme()
        {
            AppTheme.ContentSurface(Panel);
            AppTheme.NoticeHeader(_header);
            AppTheme.CardPanel(_body);
            AppTheme.NoticeAccentBar(_accentBar);
            AppTheme.NoticeList(_grid, _view);
            AppTheme.NoticeDateColumn(_colDate);
            AppTheme.NoticeEmpty(_empty);

            _badgeFont?.Dispose();
            _badgeFont = AppTheme.NoticeBadgeFont;
        }

        /// <summary>
        /// 제목 칸은 앞에 주황 점을, 새 글 칸은 N 배지를 그린다. 나머지 칸은 기본 그리기에 맡긴다.
        /// 바탕은 <c>e.Appearance</c>로 칠한다 — 마우스를 올린 줄의 주황 바탕 · 글자색이 그대로 따라온다.
        /// </summary>
        private void View_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            if (e.Column == _colTitle)
            {
                e.Appearance.FillRectangle(e.Cache, e.Bounds);
                DrawCircle(e.Graphics, new Rectangle(e.Bounds.X + DotLeft, e.Bounds.Y + (e.Bounds.Height - DotDiameter) / 2, DotDiameter, DotDiameter));

                var textBounds = new Rectangle(e.Bounds.X + TitleLeft, e.Bounds.Y, Math.Max(0, e.Bounds.Width - TitleLeft), e.Bounds.Height);
                e.Appearance.DrawString(e.Cache, e.DisplayText, textBounds);
                e.Handled = true;
            }
            else if (e.Column == _colNew)
            {
                e.Appearance.FillRectangle(e.Cache, e.Bounds);
                if (e.CellValue is bool isNew && isNew)
                {
                    var badge = new Rectangle(e.Bounds.X + (e.Bounds.Width - BadgeDiameter) / 2,
                                              e.Bounds.Y + (e.Bounds.Height - BadgeDiameter) / 2,
                                              BadgeDiameter, BadgeDiameter);
                    DrawCircle(e.Graphics, badge);
                    TextRenderer.DrawText(e.Graphics, "N", _badgeFont, badge, Color.White,
                                          TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                }
                e.Handled = true;
            }
        }

        private static void DrawCircle(Graphics graphics, Rectangle bounds)
        {
            var previous = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var brush = new SolidBrush(AppTheme.NoticeAccent))
                graphics.FillEllipse(brush, bounds);
            graphics.SmoothingMode = previous;
        }

        private void Panel_Disposed(object sender, EventArgs e)
        {
            _view.CustomDrawCell -= View_CustomDrawCell;
            _badgeFont?.Dispose();
            _badgeFont = null;
        }
    }
}
