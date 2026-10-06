using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;

namespace ColumbusWeighing.ComnLib
{
    /// <summary>
    /// 그리드 형태의 목록(헤더 + 행)을 표 형태로 인쇄 미리보기에 띄우는 공용 헬퍼.
    /// 계량 데이터 관리/거래처 관리/제품 관리가 모두 같은 방식(가로 방향, 자동 쪽넘김,
    /// 컬럼 사이 구분선, 바깥 테두리, 가운데 정렬 헤더)으로 인쇄하도록 공통화했다.
    /// DevExpress 인쇄 관련 어셈블리 의존 없이 표준 PrintDocument로 직접 그린다.
    /// </summary>
    public static class GridPrinter
    {
        /// <summary>인쇄 중에만 쓰는 상태(페이지 넘길 때 어디까지 그렸는지). 이 클래스는 한 번에
        /// 하나의 인쇄 작업만 처리한다고 가정한다(여러 화면에서 동시에 인쇄 미리보기를 여는
        /// 경우는 없음).</summary>
        private static string[][] _rows;
        private static int _rowIndex;
        private static Font _headerFont;
        private static Font _bodyFont;

        public static void ShowPrintPreview(IWin32Window owner, string[] headers, int[] columnWidths, string[][] rows)
        {
            if (rows.Length == 0)
            {
                ComnFunc.gp_PrintMessage("인쇄할 데이터가 없습니다. 먼저 조회하세요.", "안내", MessageType.알림);
                return;
            }

            _rows = rows;
            _rowIndex = 0;
            _headerFont = new Font("맑은 고딕", 8f, FontStyle.Bold);
            _bodyFont = new Font("맑은 고딕", 7.5f);

            try
            {
                using (var document = new PrintDocument())
                {
                    document.DefaultPageSettings.Landscape = true;
                    // 기본 여백(1인치=100)의 절반.
                    document.DefaultPageSettings.Margins = new Margins(50, 50, 50, 50);

                    PrintPageEventHandler onPrintPage = (s, e) => PrintPage(e, headers, columnWidths);
                    // 미리보기 자체도 한 번의 "인쇄 작업"이라 PrintPage가 호출된다 - 거기서
                    // 이미 끝까지 다 그려놓은 _rowIndex를 리셋하지 않으면, 실제로 인쇄 버튼을
                    // 눌렀을 때(두 번째 인쇄 작업) 커서가 이미 끝에 가 있어 헤더만 찍히고
                    // 데이터 행이 하나도 안 나온다. BeginPrint에서 매 인쇄 작업 시작마다 되돌린다.
                    document.BeginPrint += (s, e) => _rowIndex = 0;
                    document.PrintPage += onPrintPage;

                    try
                    {
                        using (var preview = new PrintPreviewDialog())
                        {
                            preview.Document = document;
                            preview.WindowState = FormWindowState.Maximized;
                            preview.ShowDialog(owner);
                        }
                    }
                    finally
                    {
                        document.PrintPage -= onPrintPage;
                    }
                }
            }
            catch (Exception ex)
            {
                ComnFunc.gp_PrintMessage("인쇄 중 오류가 발생했습니다.\r\n" + ex.Message, "인쇄 오류", MessageType.오류);
            }
            finally
            {
                _headerFont.Dispose();
                _bodyFont.Dispose();
                _headerFont = null;
                _bodyFont = null;
                _rows = null;
            }
        }

        private static void PrintPage(PrintPageEventArgs e, string[] headers, int[] columnWidths)
        {
            var bounds = e.MarginBounds;
            var totalWeight = (float)columnWidths.Sum();
            var colWidths = columnWidths.Select(w => bounds.Width * w / totalWeight).ToArray();

            var top = (float)bounds.Top;
            var y = top;
            DrawRow(e.Graphics, headers, colWidths, bounds.Left, ref y, _headerFont, StringAlignment.Center);
            e.Graphics.DrawLine(Pens.Black, bounds.Left, y, bounds.Right, y);
            y += 2f;

            var rowHeight = _bodyFont.GetHeight(e.Graphics) + 4f;
            var hasMore = false;
            while (_rowIndex < _rows.Length)
            {
                if (y + rowHeight > bounds.Bottom)
                {
                    hasMore = true;
                    break;
                }

                DrawRow(e.Graphics, _rows[_rowIndex], colWidths, bounds.Left, ref y, _bodyFont, StringAlignment.Near);
                e.Graphics.DrawLine(Pens.LightGray, bounds.Left, y, bounds.Right, y);
                _rowIndex++;
            }

            DrawColumnSeparators(e.Graphics, colWidths, bounds.Left, top, y);

            using (var borderPen = new Pen(Color.Black, 2f))
            {
                e.Graphics.DrawRectangle(borderPen, bounds.Left, top, bounds.Right - bounds.Left, y - top);
            }

            e.HasMorePages = hasMore;
        }

        private static void DrawRow(Graphics g, string[] values, float[] colWidths, float startX, ref float y, Font font, StringAlignment alignment)
        {
            var format = new StringFormat
            {
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap,
                Alignment = alignment,
            };
            var height = font.GetHeight(g) + 4f;
            var x = startX;
            for (var i = 0; i < values.Length; i++)
            {
                var rect = new RectangleF(x, y, colWidths[i], height);
                g.DrawString(values[i] ?? string.Empty, font, Brushes.Black, rect, format);
                x += colWidths[i];
            }

            y += height;
        }

        /// <summary>컬럼 사이에 세로 구분선을 긋는다(컬럼 개수-1개 - 바깥 테두리는 DrawRectangle이 따로 그린다).</summary>
        private static void DrawColumnSeparators(Graphics g, float[] colWidths, float startX, float top, float bottom)
        {
            var x = startX;
            for (var i = 0; i < colWidths.Length - 1; i++)
            {
                x += colWidths[i];
                g.DrawLine(Pens.LightGray, x, top, x, bottom);
            }
        }
    }
}
