using System.Drawing;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using SEOJIN_TMES.Client.Theme;

namespace SEOJIN_TMES.Client.Forms.Common
{
    /// <summary>
    /// MES 전용 커스텀 그리드 뷰 — 앱의 모든 목록이 같은 형태를 갖도록 <see cref="GridView"/>를 상속했다.
    ///
    /// 색·간격·구분선 같은 <b>공통 형태</b>는 <see cref="AppTheme.Grid"/>가 단일 기준으로 갖고 있고,
    /// 이 클래스는 표준 그리드로는 표현할 수 없는 두 가지를 직접 그려 더한다:
    ///
    ///  - <b>헤더 아래 액센트 라인</b> : 헤더 배경을 통째로 칠하는 대신 아래쪽에 얇은 색 줄만 그어
    ///    화면 구분은 유지하면서 색 면적을 줄인다(헤더보다 데이터가 먼저 눈에 들어온다).
    ///  - <b>포커스 행의 왼쪽 색 띠</b> : 배경만 옅게 깔면 어느 줄을 보고 있는지 흐릿하므로
    ///    첫 컬럼 왼쪽에 굵은 띠를 그려 현재 줄을 분명히 한다.
    ///
    /// 화면 코드는 <see cref="Accent"/>만 정하면 되고(또는 <c>AppTheme.Grid</c>를 그대로 호출해도 된다),
    /// 라이트/다크 전환은 <see cref="ApplyLook"/>를 다시 부르면 따라온다.
    /// </summary>
    public class MeisGridView : GridView
    {
        /// <summary>포커스 행 왼쪽에 그리는 색 띠의 폭(px).</summary>
        private const int FocusBarWidth = 3;

        /// <summary>헤더 아래 액센트 라인의 두께(px).</summary>
        private const int HeaderAccentHeight = 2;

        private Color _accent = AppTheme.Indigo;

        public MeisGridView()
        {
            WireCustomDraw();
        }

        public MeisGridView(GridControl grid) : base(grid)
        {
            WireCustomDraw();
        }

        /// <summary>
        /// 헤더 액센트 라인과 포커스 행 색 띠를 직접 그리기 위해 CustomDraw 이벤트에 붙는다.
        /// (RaiseCustomDraw* 재정의는 DevExpress 버전에 따라 인자 형식이 달라 이벤트 쪽이 안전하다.)
        /// </summary>
        private void WireCustomDraw()
        {
            CustomDrawColumnHeader += MeisGridView_CustomDrawColumnHeader;
            CustomDrawCell += MeisGridView_CustomDrawCell;
        }

        /// <summary>
        /// 이 목록의 포인트 컬러 — 헤더 아래 액센트 라인과 포커스 행 표시에 쓴다.
        /// 값을 넣으면 형태가 바로 다시 적용된다.
        /// </summary>
        public Color Accent
        {
            get { return _accent; }
            set
            {
                _accent = value;
                ApplyLook();
            }
        }

        // ViewName은 재정의하지 않는다 — DevExpress는 이 이름으로 뷰의 페인터/등록 정보를 찾기 때문에
        // 등록되지 않은 이름을 돌려주면 그리드가 아무것도 그리지 못한다(실측: 헤더·행 전체가 빈 회색).
        // 표준 GridView의 페인터를 그대로 쓰고, 추가 표현만 CustomDraw로 얹는다.

        /// <summary>공통 형태를 입힌다(색 값은 항상 현재 테마에서 읽으므로 다크 전환도 그대로 따라간다).</summary>
        public void ApplyLook()
        {
            if (GridControl == null)
                return;

            AppTheme.Grid(GridControl, this, _accent);
        }

        /// <summary>
        /// 헤더 아래쪽에 포인트 컬러 액센트 라인을 그린다.
        ///
        /// 처음에는 컬럼마다 왼쪽에 색 점을 찍었는데, 이 앱의 헤더 캡션은 대부분 <b>가운데 정렬</b>이라
        /// 「작업지시 수량」처럼 캡션이 긴 컬럼에서 점이 글자에 달라붙어 보였다(실측).
        /// 컬럼마다 그리지만 폭을 이어 붙이면 헤더 전체를 지나가는 한 줄이 되므로,
        /// 화면별 색 구분은 그대로 유지하면서 글자와 절대 겹치지 않는다.
        /// </summary>
        private void MeisGridView_CustomDrawColumnHeader(object sender, ColumnHeaderCustomDrawEventArgs e)
        {
            if (e.Column == null)
                return;

            e.DefaultDraw();
            e.Handled = true;

            var line = new Rectangle(
                e.Bounds.Left,
                e.Bounds.Bottom - HeaderAccentHeight,
                e.Bounds.Width,
                HeaderAccentHeight);

            using (var brush = new SolidBrush(_accent))
                e.Cache.FillRectangle(brush, line);
        }

        /// <summary>포커스 행의 첫 컬럼 왼쪽에 굵은 색 띠를 그린다.</summary>
        private void MeisGridView_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            if (e.RowHandle != FocusedRowHandle || e.Column == null || e.Column.VisibleIndex != 0)
                return;

            e.DefaultDraw();
            e.Handled = true;

            var bar = new Rectangle(e.Bounds.Left, e.Bounds.Top, FocusBarWidth, e.Bounds.Height);
            using (var brush = new SolidBrush(_accent))
                e.Cache.FillRectangle(brush, bar);
        }
    }
}
