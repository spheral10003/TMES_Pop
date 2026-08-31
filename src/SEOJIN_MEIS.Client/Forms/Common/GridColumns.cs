using System;
using System.Collections.Generic;
using DevExpress.Utils;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;

namespace SEOJIN_MEIS.Client.Forms.Common
{
    /// <summary>
    /// 그리드 컬럼 구성(표시 순서·헤더 문구·폭·정렬·날짜 서식)을 한 곳에서 지정하는 도우미.
    /// DTO 속성명이 그대로 헤더로 나오는 것을 막고, 헤더 문구를 다국어 리소스에서 가져오기 위해 쓴다.
    ///
    /// 사용 예:
    ///   GridColumns.Apply(gridView, Strings.Col_No,
    ///       GridColumns.Text("WorkOrderNo", Strings.Col_WorkOrderNo, 130),
    ///       GridColumns.Date("PlanStartDate", Strings.Col_PlanStartDate));
    /// </summary>
    public static class GridColumns
    {
        /// <summary>일반 문자열 컬럼.</summary>
        public static GridColumnSpec Text(string fieldName, string caption, int width = 120) =>
            new GridColumnSpec(fieldName, caption, width, HorzAlignment.Center, null);

        /// <summary>좌측 정렬 컬럼(비고 등 긴 문자열).</summary>
        public static GridColumnSpec Left(string fieldName, string caption, int width = 160) =>
            new GridColumnSpec(fieldName, caption, width, HorzAlignment.Near, null);

        /// <summary>수량 컬럼(우측 정렬 + 천단위 구분).</summary>
        public static GridColumnSpec Number(string fieldName, string caption, int width = 100) =>
            new GridColumnSpec(fieldName, caption, width, HorzAlignment.Far, "n0");

        /// <summary>일자 컬럼(yyyy-MM-dd).</summary>
        public static GridColumnSpec Date(string fieldName, string caption, int width = 120) =>
            new GridColumnSpec(fieldName, caption, width, HorzAlignment.Center, "yyyy-MM-dd");

        /// <summary>일시 컬럼(yyyy-MM-dd HH:mm) — 알림 발신일시처럼 분 단위가 필요한 경우.</summary>
        public static GridColumnSpec DateTime(string fieldName, string caption, int width = 140) =>
            new GridColumnSpec(fieldName, caption, width, HorzAlignment.Center, "yyyy-MM-dd HH:mm");

        /// <summary>
        /// 컬럼 구성을 적용한다. 목록에 없는 컬럼은 숨기고, 순서는 인자로 준 순서를 따른다.
        /// <paramref name="rowNumberCaption"/>을 주면 맨 앞에 순번(NO) 컬럼을 붙인다(데이터가 아니라 화면 표시용).
        /// </summary>
        public static void Apply(GridView view, string rowNumberCaption, params GridColumnSpec[] specs)
        {
            if (view == null) throw new ArgumentNullException(nameof(view));

            view.BeginUpdate();
            try
            {
                view.OptionsView.ColumnAutoWidth = false;

                var known = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var visibleIndex = 0;

                if (!string.IsNullOrEmpty(rowNumberCaption))
                {
                    var rowNo = EnsureRowNumberColumn(view, rowNumberCaption);
                    rowNo.VisibleIndex = visibleIndex++;
                    known.Add(rowNo.FieldName);
                }

                foreach (var spec in specs)
                {
                    var column = view.Columns.ColumnByFieldName(spec.FieldName);
                    if (column == null)
                        continue; // 서버 응답에 없는 필드는 조용히 건너뛴다.

                    known.Add(spec.FieldName);
                    column.Caption = spec.Caption;
                    column.VisibleIndex = visibleIndex++;
                    column.Width = spec.Width;
                    column.AppearanceCell.TextOptions.HAlignment = spec.Alignment;
                    column.AppearanceCell.Options.UseTextOptions = true;

                    if (!string.IsNullOrEmpty(spec.Format))
                    {
                        column.DisplayFormat.FormatType = spec.Format == "n0" ? FormatType.Numeric : FormatType.DateTime;
                        column.DisplayFormat.FormatString = spec.Format;
                    }

                }

                // 구성에 없는 컬럼은 숨긴다(서버가 필드를 더 내려줘도 화면이 흐트러지지 않도록).
                foreach (GridColumn column in view.Columns)
                {
                    if (!known.Contains(column.FieldName))
                        column.Visible = false;
                }
            }
            finally
            {
                view.EndUpdate();
            }
        }

        /// <summary>순번 컬럼(바인딩되지 않은 컬럼)을 만들거나 이미 있으면 재사용한다.</summary>
        private static GridColumn EnsureRowNumberColumn(GridView view, string caption)
        {
            var column = view.Columns.ColumnByFieldName(RowNumberField);
            if (column == null)
            {
                column = view.Columns.AddField(RowNumberField);
                column.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
                column.OptionsColumn.AllowEdit = false;
                column.OptionsColumn.AllowSort = DefaultBoolean.False;
                view.CustomUnboundColumnData += RowNumberHandler;
            }

            column.Caption = caption;
            column.Width = 50;
            column.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Center;
            column.AppearanceCell.Options.UseTextOptions = true;
            return column;
        }

        private const string RowNumberField = "__RowNo";

        private static void RowNumberHandler(object sender, CustomColumnDataEventArgs e)
        {
            if (e.Column.FieldName == RowNumberField && e.IsGetData)
                e.Value = e.ListSourceRowIndex + 1;
        }
    }

    /// <summary>컬럼 하나의 표시 규칙.</summary>
    public struct GridColumnSpec
    {
        public string FieldName { get; }
        public string Caption { get; }
        public int Width { get; }
        public HorzAlignment Alignment { get; }
        public string Format { get; }

        public GridColumnSpec(string fieldName, string caption, int width, HorzAlignment alignment, string format)
        {
            FieldName = fieldName;
            Caption = caption;
            Width = width;
            Alignment = alignment;
            Format = format;
        }
    }
}
