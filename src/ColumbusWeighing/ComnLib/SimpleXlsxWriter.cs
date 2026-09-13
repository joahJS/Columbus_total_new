using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace ColumbusWeighing.ComnLib
{
    /// <summary>
    /// 외부 Excel 라이브러리(EPPlus/ClosedXML 등) 없이, 최소한의 .xlsx 파일을 직접 만든다.
    /// 모든 셀을 텍스트로 쓰기 때문에(숫자/날짜 타입이 아님) 열 너비가 좁아도 엑셀에서
    /// "####"로 깨지지 않는다 - 어차피 호출하는 쪽에서 이미 "2026-09-10", "43,520 kg"처럼
    /// 화면 표시용 문자열로 만들어서 넘겨주기 때문에 원본 값 타입을 보존할 필요가 없다.
    /// 서식/수식 등은 지원하지 않는, 표 내보내기 전용 도구다.
    /// </summary>
    public static class SimpleXlsxWriter
    {
        /// <summary>
        /// headers/rows를 한 시트에 쓴다. columnWidths는 각 열의 너비(대략 "몇 글자가 들어가면
        /// 좋겠다"는 값, 엑셀의 열 너비 단위와 근사치)이며, headers보다 개수가 적으면 나머지
        /// 열은 엑셀 기본 너비를 쓴다.
        /// </summary>
        public static void Write(string filePath, string sheetName, string[] headers, IEnumerable<string[]> rows, int[] columnWidths = null)
        {
            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
            {
                WriteEntry(archive, "[Content_Types].xml", ContentTypesXml());
                WriteEntry(archive, "_rels/.rels", RelsXml());
                WriteEntry(archive, "xl/workbook.xml", WorkbookXml(sheetName));
                WriteEntry(archive, "xl/_rels/workbook.xml.rels", WorkbookRelsXml());
                WriteEntry(archive, "xl/styles.xml", StylesXml());
                WriteEntry(archive, "xl/worksheets/sheet1.xml", SheetXml(headers, rows, columnWidths));
            }
        }

        private static void WriteEntry(ZipArchive archive, string entryName, string content)
        {
            var entry = archive.CreateEntry(entryName, CompressionLevel.Fastest);
            using (var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false)))
            {
                writer.Write(content);
            }
        }

        private static string ContentTypesXml()
        {
            return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">" +
                "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>" +
                "<Default Extension=\"xml\" ContentType=\"application/xml\"/>" +
                "<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>" +
                "<Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>" +
                "<Override PartName=\"/xl/styles.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml\"/>" +
                "</Types>";
        }

        private static string RelsXml()
        {
            return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/>" +
                "</Relationships>";
        }

        private static string WorkbookXml(string sheetName)
        {
            return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\">" +
                "<sheets><sheet name=\"" + XmlEscape(sheetName) + "\" sheetId=\"1\" r:id=\"rId1\"/></sheets>" +
                "</workbook>";
        }

        private static string WorkbookRelsXml()
        {
            return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/>" +
                "<Relationship Id=\"rId2\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles\" Target=\"styles.xml\"/>" +
                "</Relationships>";
        }

        /// <summary>스타일 0번(일반)/1번(헤더용 굵게)만 정의한다.</summary>
        private static string StylesXml()
        {
            return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<styleSheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">" +
                "<fonts count=\"2\">" +
                "<font><sz val=\"10\"/><name val=\"맑은 고딕\"/></font>" +
                "<font><b/><sz val=\"10\"/><name val=\"맑은 고딕\"/></font>" +
                "</fonts>" +
                "<fills count=\"1\"><fill><patternFill patternType=\"none\"/></fill></fills>" +
                "<borders count=\"1\"><border><left/><right/><top/><bottom/><diagonal/></border></borders>" +
                "<cellStyleXfs count=\"1\"><xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\"/></cellStyleXfs>" +
                "<cellXfs count=\"2\">" +
                "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\"/>" +
                "<xf numFmtId=\"0\" fontId=\"1\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyFont=\"1\"/>" +
                "</cellXfs>" +
                "<cellStyles count=\"1\"><cellStyle name=\"Normal\" xfId=\"0\" builtinId=\"0\"/></cellStyles>" +
                "</styleSheet>";
        }

        private static string SheetXml(string[] headers, IEnumerable<string[]> rows, int[] columnWidths)
        {
            var sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            sb.Append("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">");

            if (columnWidths != null && columnWidths.Length > 0)
            {
                sb.Append("<cols>");
                for (var i = 0; i < columnWidths.Length; i++)
                {
                    sb.Append(string.Format(
                        "<col min=\"{0}\" max=\"{0}\" width=\"{1}\" customWidth=\"1\"/>",
                        i + 1, columnWidths[i]));
                }

                sb.Append("</cols>");
            }

            sb.Append("<sheetData>");

            AppendRow(sb, 1, headers, 1);

            var rowIndex = 2;
            foreach (var row in rows)
            {
                AppendRow(sb, rowIndex, row, 0);
                rowIndex++;
            }

            sb.Append("</sheetData>");
            sb.Append("</worksheet>");
            return sb.ToString();
        }

        private static void AppendRow(StringBuilder sb, int rowIndex, string[] values, int styleIndex)
        {
            sb.Append("<row r=\"").Append(rowIndex).Append("\">");
            for (var col = 0; col < values.Length; col++)
            {
                var cellRef = ColumnLetter(col) + rowIndex;
                sb.Append("<c r=\"").Append(cellRef).Append("\" t=\"inlineStr\" s=\"").Append(styleIndex).Append("\">");
                sb.Append("<is><t xml:space=\"preserve\">").Append(XmlEscape(values[col] ?? string.Empty)).Append("</t></is>");
                sb.Append("</c>");
            }

            sb.Append("</row>");
        }

        private static string ColumnLetter(int zeroBasedIndex)
        {
            var dividend = zeroBasedIndex + 1;
            var columnName = string.Empty;
            while (dividend > 0)
            {
                var modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar('A' + modulo) + columnName;
                dividend = (dividend - modulo) / 26;
            }

            return columnName;
        }

        private static string XmlEscape(string value)
        {
            return value
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }
    }
}
