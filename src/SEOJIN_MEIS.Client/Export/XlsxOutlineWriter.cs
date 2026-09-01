using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace SEOJIN_MEIS.Client.Export
{
    /// <summary>
    /// 계층 자료를 <b>Excel 아웃라인(접기/펼치기)</b>이 들어간 xlsx로 쓴다.
    ///
    /// 왜 직접 쓰나 — DevExpress의 두 경로가 모두 막혀 있다.
    ///   · <c>TreeList.ExportToXlsx</c>는 인쇄 링크 기반이라 트리 들여쓰기를 <b>그림처럼 재현만</b> 하고
    ///     아웃라인 레벨을 만들지 못한다(<c>XlsxExportOptions</c>·<c>TreeListOptionsPrint</c> 어디에도 그룹 옵션이 없다).
    ///   · Office File API(<c>DevExpress.Docs</c>)의 <c>Workbook</c>은 아웃라인을 만들 수 있지만
    ///     이 설치본에서는 라이선스가 없어 저장하는 파일마다 "Evaluation Warning" 시트가 붙는다.
    ///
    /// xlsx는 XML 몇 개를 담은 zip이라, 필요한 부분(아웃라인·서식·틀고정)만 직접 쓰면 외부 의존성이 필요 없다.
    /// 계층은 각 행의 <c>outlineLevel</c> 속성으로 표현하고, <c>summaryBelow="0"</c>으로
    /// <b>부모 행이 자식 위에</b> 오게 한다(트리 표시 순서와 같아야 접었을 때 대표 행이 맞는다).
    /// </summary>
    public sealed class XlsxOutlineWriter
    {
        /// <summary>엑셀 아웃라인은 최대 7단계다. 그보다 깊은 계층은 7로 눌러 담는다.</summary>
        private const int MaxOutlineLevel = 7;

        private readonly string _sheetName;
        private readonly IList<XlsxColumn> _columns;
        private readonly List<Row> _rows = new List<Row>();

        public XlsxOutlineWriter(string sheetName, IList<XlsxColumn> columns)
        {
            if (columns == null || columns.Count == 0)
                throw new ArgumentException("컬럼이 최소 하나는 있어야 한다.", nameof(columns));

            _sheetName = ToSheetName(sheetName);
            _columns = columns;
        }

        /// <summary>
        /// 자료 행 하나. <paramref name="outlineLevel"/>이 0이면 최상위이고, 깊어질수록 1씩 커진다.
        /// <paramref name="values"/>는 컬럼 순서와 같아야 하며, <see cref="decimal"/>은 숫자로,
        /// 나머지는 문자열로 들어간다(숫자로 넣어야 엑셀에서 계산·비교가 된다).
        /// </summary>
        public void AddRow(int outlineLevel, params object[] values)
        {
            _rows.Add(new Row
            {
                OutlineLevel = Math.Max(0, Math.Min(outlineLevel, MaxOutlineLevel)),
                Values = values ?? new object[0]
            });
        }

        public void Save(string filePath)
        {
            // 통째로 만든 뒤 한 번에 내보낸다 — 중간에 실패했을 때 반쯤 쓰인 파일을 남기지 않는다.
            using (var buffer = new MemoryStream())
            {
                using (var zip = new ZipArchive(buffer, ZipArchiveMode.Create, true))
                {
                    WriteEntry(zip, "[Content_Types].xml", ContentTypesXml());
                    WriteEntry(zip, "_rels/.rels", RootRelsXml());
                    WriteEntry(zip, "xl/workbook.xml", WorkbookXml());
                    WriteEntry(zip, "xl/_rels/workbook.xml.rels", WorkbookRelsXml());
                    WriteEntry(zip, "xl/styles.xml", StylesXml());
                    WriteEntry(zip, "xl/worksheets/sheet1.xml", SheetXml());
                }

                File.WriteAllBytes(filePath, buffer.ToArray());
            }
        }

        private static void WriteEntry(ZipArchive zip, string entryName, string xml)
        {
            var entry = zip.CreateEntry(entryName, CompressionLevel.Optimal);
            using (var stream = entry.Open())
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
            {
                writer.Write(xml);
            }
        }

        // ────────────────────────────── 시트 본문 ──────────────────────────────

        private string SheetXml()
        {
            var sb = new StringBuilder();
            sb.Append(XmlHeader);
            sb.Append("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" ");
            sb.Append("xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\">");

            // 요소 순서가 스키마로 정해져 있다 — sheetPr → sheetViews → sheetFormatPr → cols → sheetData.
            // 순서를 바꾸면 엑셀이 "읽을 수 없는 내용" 복구 대화상자를 띄운다.
            sb.Append("<sheetPr><outlinePr summaryBelow=\"0\" /></sheetPr>");

            // 제목행 고정 — 아래로 스크롤해도 어느 열인지 보인다.
            sb.Append("<sheetViews><sheetView workbookViewId=\"0\">");
            sb.Append("<pane ySplit=\"1\" topLeftCell=\"A2\" activePane=\"bottomLeft\" state=\"frozen\" />");
            sb.Append("</sheetView></sheetViews>");

            var maxLevel = _rows.Count == 0 ? 0 : _rows.Max(r => r.OutlineLevel);
            sb.Append($"<sheetFormatPr defaultRowHeight=\"15\" outlineLevelRow=\"{maxLevel}\" />");

            sb.Append(ColsXml());

            sb.Append("<sheetData>");
            AppendHeaderRow(sb);
            for (var i = 0; i < _rows.Count; i++)
                AppendDataRow(sb, _rows[i], rowNumber: i + 2);   // 1행은 제목행
            sb.Append("</sheetData>");

            sb.Append("</worksheet>");
            return sb.ToString();
        }

        private string ColsXml()
        {
            var sb = new StringBuilder("<cols>");
            for (var i = 0; i < _columns.Count; i++)
            {
                // 엑셀의 컬럼 폭 단위는 픽셀이 아니라 "기본 글꼴 문자 수"다. 통용되는 근사식으로 옮긴다.
                var width = Math.Max(4, (_columns[i].WidthInPixels - 5) / 7.0);
                sb.Append($"<col min=\"{i + 1}\" max=\"{i + 1}\" width=\"{width.ToString("0.##", CultureInfo.InvariantCulture)}\" customWidth=\"1\" />");
            }
            return sb.Append("</cols>").ToString();
        }

        private void AppendHeaderRow(StringBuilder sb)
        {
            sb.Append("<row r=\"1\">");
            for (var i = 0; i < _columns.Count; i++)
                AppendInlineString(sb, CellRef(i, 1), StyleHeader, _columns[i].Caption);
            sb.Append("</row>");
        }

        private void AppendDataRow(StringBuilder sb, Row row, int rowNumber)
        {
            sb.Append(row.OutlineLevel > 0
                ? $"<row r=\"{rowNumber}\" outlineLevel=\"{row.OutlineLevel}\">"
                : $"<row r=\"{rowNumber}\">");

            for (var i = 0; i < _columns.Count; i++)
            {
                var value = i < row.Values.Length ? row.Values[i] : null;
                var reference = CellRef(i, rowNumber);

                if (value is decimal number)
                {
                    // 숫자는 서식만 소수 세 자리로 주고 값 자체는 원본을 넣는다(반올림해 저장하지 않는다).
                    sb.Append($"<c r=\"{reference}\" s=\"{StyleNumber}\">");
                    sb.Append($"<v>{number.ToString(CultureInfo.InvariantCulture)}</v></c>");
                    continue;
                }

                var text = value?.ToString() ?? string.Empty;
                if (text.Length == 0)
                    continue;   // 빈 칸은 아예 쓰지 않는다 — 파일이 작아지고 엑셀 동작도 같다

                // 계층 컬럼(첫 컬럼)만 아웃라인 깊이만큼 들여쓴다.
                var style = i == 0
                    ? StyleIndentBase + row.OutlineLevel
                    : (_columns[i].Align == XlsxAlign.Center ? StyleTextCenter : StyleTextLeft);

                AppendInlineString(sb, reference, style, text);
            }

            sb.Append("</row>");
        }

        /// <summary>
        /// 문자열은 sharedStrings.xml 없이 셀에 바로 넣는다(<c>t="inlineStr"</c>).
        /// 공유 문자열표는 같은 값이 많이 반복될 때 이득인데, 이 화면은 코드·설명이 대부분 제각각이라
        /// 표를 따로 두는 이득이 없고 파트가 하나 늘어날 뿐이다.
        /// </summary>
        private static void AppendInlineString(StringBuilder sb, string reference, int style, string text)
        {
            sb.Append($"<c r=\"{reference}\" s=\"{style}\" t=\"inlineStr\"><is><t xml:space=\"preserve\">");
            sb.Append(Escape(text));
            sb.Append("</t></is></c>");
        }

        // ────────────────────────────── 서식 ──────────────────────────────

        private const int StyleDefault = 0;
        private const int StyleHeader = 1;
        private const int StyleTextLeft = 2;
        private const int StyleTextCenter = 3;
        private const int StyleNumber = 4;

        /// <summary>들여쓰기 0~7단계 서식이 이 번호부터 연속으로 놓인다.</summary>
        private const int StyleIndentBase = 5;

        private string StylesXml()
        {
            var sb = new StringBuilder();
            sb.Append(XmlHeader);
            sb.Append("<styleSheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">");

            // 화면의 값 컬럼과 같은 소수 세 자리 서식(6.180 / 5.400).
            sb.Append("<numFmts count=\"1\"><numFmt numFmtId=\"164\" formatCode=\"0.000\" /></numFmts>");

            sb.Append("<fonts count=\"2\">");
            sb.Append("<font><sz val=\"11\" /><name val=\"맑은 고딕\" /></font>");
            sb.Append("<font><b /><sz val=\"11\" /><name val=\"맑은 고딕\" /></font>");
            sb.Append("</fonts>");

            // fill 0(none)·1(gray125)은 엑셀이 요구하는 고정 자리다. 실제 쓰는 색은 2번부터.
            sb.Append("<fills count=\"3\">");
            sb.Append("<fill><patternFill patternType=\"none\" /></fill>");
            sb.Append("<fill><patternFill patternType=\"gray125\" /></fill>");
            sb.Append($"<fill><patternFill patternType=\"solid\"><fgColor rgb=\"FF{HeaderFillRgb}\" /><bgColor indexed=\"64\" /></patternFill></fill>");
            sb.Append("</fills>");

            sb.Append("<borders count=\"1\"><border><left /><right /><top /><bottom /><diagonal /></border></borders>");
            sb.Append("<cellStyleXfs count=\"1\"><xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\" /></cellStyleXfs>");

            var xfs = new List<string>
            {
                "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\" />",                                   // 0 기본
                "<xf numFmtId=\"0\" fontId=\"1\" fillId=\"2\" borderId=\"0\" xfId=\"0\" applyFont=\"1\" applyFill=\"1\" applyAlignment=\"1\"><alignment horizontal=\"center\" vertical=\"center\" /></xf>", // 1 제목행
                "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyAlignment=\"1\"><alignment horizontal=\"left\" /></xf>",      // 2 문자 왼쪽
                "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyAlignment=\"1\"><alignment horizontal=\"center\" /></xf>",    // 3 문자 가운데
                "<xf numFmtId=\"164\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyNumberFormat=\"1\" applyAlignment=\"1\"><alignment horizontal=\"center\" /></xf>" // 4 숫자
            };

            // 들여쓰기는 서식에만 있는 속성이라 단계마다 서식을 따로 만들어 둔다.
            for (var indent = 0; indent <= MaxOutlineLevel; indent++)
            {
                xfs.Add($"<xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyAlignment=\"1\">" +
                        $"<alignment horizontal=\"left\" indent=\"{indent}\" /></xf>");
            }

            sb.Append($"<cellXfs count=\"{xfs.Count}\">");
            foreach (var xf in xfs)
                sb.Append(xf);
            sb.Append("</cellXfs>");

            sb.Append("<cellStyles count=\"1\"><cellStyle name=\"Normal\" xfId=\"0\" builtinId=\"0\" /></cellStyles>");
            sb.Append("</styleSheet>");
            return sb.ToString();
        }

        /// <summary>제목행 배경(RRGGBB). 화면 테마를 따르지 않는 고정 색 — <c>AppTheme.ExportHeaderBack</c>과 같은 값이다.</summary>
        private const string HeaderFillRgb = "EEECF8";

        // ────────────────────────────── 나머지 파트(고정) ──────────────────────────────

        private const string XmlHeader = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>";

        private static string ContentTypesXml() =>
            XmlHeader +
            "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">" +
            "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\" />" +
            "<Default Extension=\"xml\" ContentType=\"application/xml\" />" +
            "<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\" />" +
            "<Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\" />" +
            "<Override PartName=\"/xl/styles.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml\" />" +
            "</Types>";

        private static string RootRelsXml() =>
            XmlHeader +
            "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
            "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\" />" +
            "</Relationships>";

        private string WorkbookXml() =>
            XmlHeader +
            "<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" " +
            "xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\">" +
            $"<sheets><sheet name=\"{Escape(_sheetName)}\" sheetId=\"1\" r:id=\"rId1\" /></sheets>" +
            "</workbook>";

        private static string WorkbookRelsXml() =>
            XmlHeader +
            "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
            "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\" />" +
            "<Relationship Id=\"rId2\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles\" Target=\"styles.xml\" />" +
            "</Relationships>";

        // ────────────────────────────── 잔심부름 ──────────────────────────────

        /// <summary>0-based 컬럼 번호를 엑셀 열 이름(A, B, … Z, AA)으로.</summary>
        private static string ColumnName(int index)
        {
            var name = string.Empty;
            for (var i = index; i >= 0; i = i / 26 - 1)
                name = (char)('A' + i % 26) + name;
            return name;
        }

        private static string CellRef(int columnIndex, int rowNumber) => ColumnName(columnIndex) + rowNumber;

        private static string Escape(string text) =>
            text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");

        /// <summary>시트 이름 제약(31자 이하, <c>: \ / ? * [ ]</c> 불가)에 맞춘다.</summary>
        private static string ToSheetName(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return "Sheet1";

            var cleaned = new string(title.Where(c => ":\\/?*[]".IndexOf(c) < 0).ToArray()).Trim();
            if (cleaned.Length == 0)
                return "Sheet1";

            return cleaned.Length > 31 ? cleaned.Substring(0, 31) : cleaned;
        }

        private struct Row
        {
            public int OutlineLevel;
            public object[] Values;
        }
    }

    public enum XlsxAlign
    {
        Left,
        Center
    }

    /// <summary>내보낼 컬럼 하나 — 제목·폭·정렬. 폭은 화면 컬럼 폭(px)을 그대로 넘기면 된다.</summary>
    public struct XlsxColumn
    {
        public string Caption;
        public int WidthInPixels;
        public XlsxAlign Align;

        public XlsxColumn(string caption, int widthInPixels, XlsxAlign align)
        {
            Caption = caption;
            WidthInPixels = widthInPixels;
            Align = align;
        }
    }
}
