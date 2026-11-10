using System;
using System.Drawing;
using DevExpress.Data;
using DevExpress.Utils;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using SEOJIN_TMES.Client.Theme;

namespace SEOJIN_TMES.Client.Forms.Common
{
    /// <summary>
    /// 상태 컬럼 바로 앞에 상태 구슬 그림 컬럼을 붙인다 — 대기(파랑) · 작업 중(노랑) · 완료(초록) · 중지(빨강).
    /// 그림 컬럼과 상태 컬럼의 헤더는 하나로 합쳐 상태 컬럼 문구만 보이게 그린다(두 칸이 한 항목이라는 것이 보이도록).
    ///
    /// 그림은 상태 컬럼 값(서버 코드)으로만 고른다 — 판정은 하지 않고 서버가 준 상태를 색으로 바꿔 보여 줄 뿐이다.
    /// <see cref="GridColumns.Apply"/>는 목록에 없는 컬럼을 숨기므로 반드시 그 <b>뒤에</b> <see cref="Attach"/>한다.
    /// </summary>
    public class StateImageColumn
    {
        private const string ImageFieldName = "__StateImage";
        private const int ImageSize = 16;
        private const int ColumnWidth = 30;

        private readonly GridView _view;
        private readonly string _stateFieldName;
        private GridColumn _imageColumn;

        /// <summary>셀 크기로 줄여 둔 구슬 그림 — 대기(파랑) · 작업 중(노랑) · 완료(초록) · 중지(빨강).</summary>
        private Image _ready;
        private Image _progress;
        private Image _finished;
        private Image _stopped;

        /// <param name="view">대상 그리드 뷰.</param>
        /// <param name="stateFieldName">상태 코드가 들어 있는 컬럼(예: ProgressState, StatusName).</param>
        public StateImageColumn(GridView view, string stateFieldName)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _stateFieldName = stateFieldName;
        }

        /// <summary>그림 컬럼을 상태 컬럼 앞에 넣고 그리기 이벤트를 건다. 상태 컬럼이 없으면 아무것도 하지 않는다.</summary>
        public void Attach()
        {
            var stateColumn = _view.Columns.ColumnByFieldName(_stateFieldName);
            if (stateColumn == null || !stateColumn.Visible)
                return;

            _imageColumn = _view.Columns.ColumnByFieldName(ImageFieldName);
            if (_imageColumn == null)
            {
                _imageColumn = _view.Columns.AddField(ImageFieldName);
                _imageColumn.UnboundType = UnboundColumnType.Object;   // 값은 없고 그림만 그린다
                _imageColumn.OptionsColumn.AllowEdit = false;
                _imageColumn.OptionsColumn.AllowSort = DefaultBoolean.False;
                _imageColumn.OptionsColumn.AllowSize = false;
                _imageColumn.OptionsColumn.FixedWidth = true;
                _imageColumn.OptionsFilter.AllowFilter = false;

                LoadImages();

                _view.CustomDrawCell += view_CustomDrawCell;
                _view.CustomDrawColumnHeader += view_CustomDrawColumnHeader;
            }

            _imageColumn.Caption = string.Empty;
            _imageColumn.Width = ColumnWidth;
            // 상태 컬럼의 자리에 넣으면 상태 컬럼이 한 칸 뒤로 밀려 "그림 → 상태 글자" 순서가 된다.
            _imageColumn.VisibleIndex = stateColumn.VisibleIndex;
        }

        /// <summary>
        /// 구슬 그림 4개를 셀 크기로 한 번만 줄여 둔다. Properties.Resources의 속성은 읽을 때마다 원본 크기
        /// 새 비트맵을 만들어 주므로, 셀을 그릴 때마다 꺼내면 매번 큰 그림을 줄이고 버리게 된다.
        /// 그리드가 없어질 때 함께 Dispose한다.
        /// </summary>
        private void LoadImages()
        {
            if (_ready != null)
                return;

            _ready = LoadSphere(Properties.Resources.sphere_blue);
            _progress = LoadSphere(Properties.Resources.sphere_yellow);
            _finished = LoadSphere(Properties.Resources.sphere_green);
            _stopped = LoadSphere(Properties.Resources.sphere_red);

            if (_view.GridControl != null)
                _view.GridControl.Disposed += GridControl_Disposed;
        }

        private Image LoadSphere(Bitmap source)
        {
            using (source)
                return AppImages.Resized(source, new Size(ImageSize, ImageSize));
        }

        private void GridControl_Disposed(object sender, EventArgs e)
        {
            _ready?.Dispose();
            _progress?.Dispose();
            _finished?.Dispose();
            _stopped?.Dispose();
        }

        /// <summary>상태 코드 → 구슬 그림. 모르는 코드면 null(그림 없음).</summary>
        private Image ResolveImage(object state)
        {
            switch ((state as string)?.Trim().ToUpperInvariant())
            {
                case "READY": return _ready;
                case "PROGRESS": return _progress;
                // 공정은 Finished, 작업지시는 Completed로 내려온다 — 둘 다 완료.
                case "FINISHED":
                case "COMPLETED": return _finished;
                case "STOPPED": return _stopped;
                default: return null;
            }
        }

        private void view_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            if (e.Column != _imageColumn)
                return;

            e.DefaultDraw();   // 선택 · 줄무늬 배경은 기본대로

            var image = ResolveImage(_view.GetRowCellValue(e.RowHandle, _stateFieldName));
            if (image != null)
            {
                var x = e.Bounds.X + (e.Bounds.Width - ImageSize) / 2;
                var y = e.Bounds.Y + (e.Bounds.Height - ImageSize) / 2;
                e.Cache.DrawImage(image, new Rectangle(x, y, ImageSize, ImageSize));
            }

            e.Handled = true;
        }

        /// <summary>
        /// 그림 컬럼과 상태 컬럼 헤더를 한 칸으로 그린다. 두 컬럼 어느 쪽이 다시 그려져도 같은 모양이 되도록
        /// 양쪽 모두 합친 영역 전체를 그린다(한쪽만 그리면 마우스를 올려 그 칸만 다시 그릴 때 빈칸이 된다).
        /// </summary>
        private void view_CustomDrawColumnHeader(object sender, ColumnHeaderCustomDrawEventArgs e)
        {
            if (e.Column == null || (e.Column != _imageColumn && e.Column.FieldName != _stateFieldName))
                return;

            var stateColumn = _view.Columns.ColumnByFieldName(_stateFieldName);
            var viewInfo = _view.GetViewInfo() as GridViewInfo;
            if (stateColumn == null || viewInfo == null)
                return;

            var imageInfo = viewInfo.ColumnsInfo[_imageColumn];
            var stateInfo = viewInfo.ColumnsInfo[stateColumn];
            if (imageInfo == null || stateInfo == null)
                return;

            var merged = Rectangle.Union(imageInfo.Bounds, stateInfo.Bounds);

            // 배경 · 테두리만 기본 그리기로 합친 칸 전체에 그리고, 글자는 직접 가운데에 쓴다.
            // 기본 그리기에 글자까지 맡기면 헤더 기본 정렬(왼쪽)과 원래 글자 위치(CaptionRect)를 따라가
            // 그림 칸 왼쪽 끝에 붙어 앞 컬럼 헤더 글자와 겹쳐 보였다.
            var info = e.Info;
            var oldBounds = info.Bounds;
            var oldCaption = info.Caption;
            try
            {
                info.Bounds = merged;
                info.Caption = string.Empty;
                e.Painter.DrawObject(info);
            }
            finally
            {
                info.Bounds = oldBounds;
                info.Caption = oldCaption;
            }

            using (var appearance = (AppearanceObject)e.Appearance.Clone())
            {
                appearance.TextOptions.HAlignment = HorzAlignment.Center;
                appearance.TextOptions.VAlignment = VertAlignment.Center;
                appearance.TextOptions.WordWrap = WordWrap.NoWrap;
                appearance.TextOptions.Trimming = Trimming.EllipsisCharacter;

                var textBounds = Rectangle.Inflate(merged, -4, 0);
                appearance.DrawString(e.Cache, stateColumn.GetCaption(), textBounds);
            }

            e.Handled = true;
        }
    }
}
