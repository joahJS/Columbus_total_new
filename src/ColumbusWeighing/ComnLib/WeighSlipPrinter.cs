using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;
using ColumbusWeighing.Models;

namespace ColumbusWeighing.ComnLib
{
    /// <summary>
    /// 계근건 1건을 계근표(계량증명서) 형태로 인쇄 미리보기에 띄운다. 각 지점이 지금 쓰는
    /// TS2020/MES 프로그램이 출력하는 계근표(번호/일자/지점/거래처/차량/품명 + 중량 정보 +
    /// 1차·2차 사진 2x2 + 지점 정보 푸터)를 참고해서 만들었다. 현장에서는 A4 세로 한 장에
    /// 같은 내용을 위아래 두 칸에 반복해서 찍어, 잘라서 업체용/보관용으로 나눠 가진다고
    /// 확인되어 그 방식을 그대로 따랐다. GridPrinter와 마찬가지로 DevExpress 인쇄 어셈블리
    /// 의존 없이 표준 PrintDocument로 직접 그린다.
    ///
    /// 실제 TS2020 출력물 사진 한 장만 보고 만든 거라 정확한 칸 배치/문구가 조금 다를 수
    /// 있다 - 미리보기를 보고 조정이 필요하면 바로 고칠 것.
    /// </summary>
    public static class WeighSlipPrinter
    {
        // 이 클래스는 GridPrinter와 동일하게, 한 번에 하나의 인쇄 작업만 처리한다고 가정한다.
        private static WeighingRecord _record;
        private static List<WeighPhoto> _photos;

        public static void ShowPrintPreview(IWin32Window owner, WeighingRecord record, List<WeighPhoto> photos)
        {
            _record = record;
            _photos = photos ?? new List<WeighPhoto>();

            try
            {
                using (var document = new PrintDocument())
                {
                    document.DefaultPageSettings.Landscape = false;
                    document.DefaultPageSettings.Margins = new Margins(40, 40, 30, 30);
                    document.PrintPage += PrintPage;

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
                        document.PrintPage -= PrintPage;
                    }
                }
            }
            catch (Exception ex)
            {
                ComnFunc.gp_PrintMessage("인쇄 중 오류가 발생했습니다.\r\n" + ex.Message, "인쇄 오류", MessageType.오류);
            }
            finally
            {
                _record = null;
                _photos = null;
            }
        }

        private static void PrintPage(object sender, PrintPageEventArgs e)
        {
            var bounds = e.MarginBounds;
            var halfHeight = bounds.Height / 2f;

            var topRect = new RectangleF(bounds.Left, bounds.Top, bounds.Width, halfHeight - 6f);
            var bottomRect = new RectangleF(bounds.Left, bounds.Top + halfHeight + 6f, bounds.Width, halfHeight - 6f);

            DrawSlip(e.Graphics, topRect, "업체용");
            DrawCutLine(e.Graphics, bounds, bounds.Top + halfHeight);
            DrawSlip(e.Graphics, bottomRect, "보관용");

            e.HasMorePages = false;
        }

        private static void DrawCutLine(Graphics g, Rectangle bounds, float y)
        {
            using (var pen = new Pen(Color.Gray) { DashStyle = DashStyle.Dash })
            {
                g.DrawLine(pen, bounds.Left, y, bounds.Right, y);
            }
        }

        private static void DrawSlip(Graphics g, RectangleF area, string copyLabel)
        {
            var titleFont = new Font("맑은 고딕", 15f, FontStyle.Bold);
            var copyFont = new Font("맑은 고딕", 9f);
            var labelFont = new Font("맑은 고딕", 8.5f, FontStyle.Bold);
            var valueFont = new Font("맑은 고딕", 8.5f);
            var footerFont = new Font("맑은 고딕", 7.5f);

            try
            {
                var y = area.Top + 2f;

                var titleSize = g.MeasureString("계량증명서", titleFont);
                g.DrawString("계량증명서", titleFont, Brushes.Black, area.Left + (area.Width - titleSize.Width) / 2f, y);
                g.DrawString(copyLabel, copyFont, Brushes.Black, area.Right - 50f, y + titleSize.Height / 2f - 7f);
                y += titleSize.Height + 4f;

                var tableWidth = area.Width * 0.62f;
                var tableHeight = DrawInfoTable(g, area.Left, y, tableWidth, labelFont, valueFont);

                var photoX = area.Left + area.Width * 0.64f;
                var photoWidth = area.Right - photoX;
                DrawPhotoGrid(g, photoX, y, photoWidth, tableHeight);

                y += tableHeight + 4f;
                DrawFooter(g, area, y, footerFont);
            }
            finally
            {
                titleFont.Dispose();
                copyFont.Dispose();
                labelFont.Dispose();
                valueFont.Dispose();
                footerFont.Dispose();
            }
        }

        private class InfoRow
        {
            public string Label1;
            public string Value1;
            public string Label2;
            public string Value2;
        }

        private static List<InfoRow> BuildInfoRows()
        {
            var r = _record;
            return new List<InfoRow>
            {
                new InfoRow { Label1 = "번호", Value1 = r.WeighSeq.ToString(), Label2 = "일자", Value2 = r.FirstDateTime.ToString("yyyy-MM-dd") },
                new InfoRow { Label1 = "지점", Value1 = r.BranchCode.ToDisplayString() + "지점", Label2 = "거래처명", Value2 = r.CustomerName },
                new InfoRow { Label1 = "차량번호", Value1 = r.VehicleNo, Label2 = "품명", Value2 = r.ProductName },
                new InfoRow { Label1 = "운전자", Value1 = r.DriverName, Label2 = "계량자", Value2 = r.WeigherName },
                new InfoRow { Label1 = "1차시간", Value1 = r.FirstDateTime.ToString("HH:mm"), Label2 = "2차시간", Value2 = r.SecondDateTime.HasValue ? r.SecondDateTime.Value.ToString("HH:mm") : "-" },
                new InfoRow { Label1 = "1차중량", Value1 = FormatWeight(r.FirstWeight), Label2 = "2차중량", Value2 = r.SecondWeight.HasValue ? FormatWeight(r.SecondWeight.Value) : "-" },
                new InfoRow { Label1 = "실중량", Value1 = r.NetWeight.HasValue ? FormatWeight(r.NetWeight.Value) : "-", Label2 = "업체중량", Value2 = r.VendorWeight.HasValue ? FormatWeight(r.VendorWeight.Value) : "-" },
                new InfoRow { Label1 = "입출구분", Value1 = r.InOutType.HasValue ? r.InOutType.Value.ToDisplayString() : "-", Label2 = "비고", Value2 = r.Remark },
            };
        }

        private static string FormatWeight(decimal value)
        {
            return value.ToString("N0") + " kg";
        }

        private static float DrawInfoTable(Graphics g, float x, float y, float width, Font labelFont, Font valueFont)
        {
            var startY = y;
            var labelWidth = width * 0.18f;
            var valueWidth = width * 0.32f;
            var rowHeight = Math.Max(labelFont.GetHeight(g), valueFont.GetHeight(g)) + 8f;

            using (var borderPen = new Pen(Color.Black))
            using (var gridPen = new Pen(Color.Gray))
            {
                foreach (var row in BuildInfoRows())
                {
                    var cellX = x;
                    DrawCell(g, cellX, y, labelWidth, rowHeight, row.Label1, labelFont, gridPen, true);
                    cellX += labelWidth;
                    DrawCell(g, cellX, y, valueWidth, rowHeight, row.Value1, valueFont, gridPen, false);
                    cellX += valueWidth;
                    DrawCell(g, cellX, y, labelWidth, rowHeight, row.Label2, labelFont, gridPen, true);
                    cellX += labelWidth;
                    DrawCell(g, cellX, y, valueWidth, rowHeight, row.Value2, valueFont, gridPen, false);
                    y += rowHeight;
                }

                g.DrawRectangle(borderPen, x, startY, width, y - startY);
            }

            return y - startY;
        }

        private static void DrawCell(Graphics g, float x, float y, float width, float height, string text, Font font, Pen gridPen, bool isLabel)
        {
            if (isLabel)
            {
                using (var backBrush = new SolidBrush(Color.WhiteSmoke))
                {
                    g.FillRectangle(backBrush, x, y, width, height);
                }
            }

            g.DrawRectangle(gridPen, x, y, width, height);

            var format = new StringFormat
            {
                Alignment = isLabel ? StringAlignment.Center : StringAlignment.Near,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap,
            };
            var padded = new RectangleF(x + 4f, y, width - 8f, height);
            g.DrawString(text ?? string.Empty, font, Brushes.Black, padded, format);
        }

        private static void DrawPhotoGrid(Graphics g, float x, float y, float width, float height)
        {
            var cellWidth = width / 2f;
            var cellHeight = height / 2f;
            var labelFont = new Font("맑은 고딕", 7f);

            try
            {
                DrawPhotoCell(g, x, y, cellWidth, cellHeight, "1차 #1", FindPhoto("1", 1), labelFont);
                DrawPhotoCell(g, x + cellWidth, y, cellWidth, cellHeight, "1차 #2", FindPhoto("1", 2), labelFont);
                DrawPhotoCell(g, x, y + cellHeight, cellWidth, cellHeight, "2차 #1", FindPhoto("2", 1), labelFont);
                DrawPhotoCell(g, x + cellWidth, y + cellHeight, cellWidth, cellHeight, "2차 #2", FindPhoto("2", 2), labelFont);
            }
            finally
            {
                labelFont.Dispose();
            }
        }

        private static WeighPhoto FindPhoto(string stage, int seq)
        {
            foreach (var photo in _photos)
            {
                if (photo.Stage == stage && photo.Seq == seq)
                {
                    return photo;
                }
            }

            return null;
        }

        private static void DrawPhotoCell(Graphics g, float x, float y, float width, float height, string label, WeighPhoto photo, Font labelFont)
        {
            using (var pen = new Pen(Color.Black))
            {
                g.DrawRectangle(pen, x, y, width, height);
            }

            var labelHeight = labelFont.GetHeight(g) + 2f;
            g.DrawString(label, labelFont, Brushes.Black, x + 2f, y + 1f);

            var imageRect = new RectangleF(x + 2f, y + labelHeight, width - 4f, height - labelHeight - 2f);
            var centerFormat = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

            if (photo == null || photo.Data == null || photo.Data.Length == 0)
            {
                g.DrawString("사진 없음", labelFont, Brushes.Gray, imageRect, centerFormat);
                return;
            }

            try
            {
                using (var stream = new MemoryStream(photo.Data))
                using (var image = Image.FromStream(stream))
                {
                    var fitted = FitRect(image.Size, imageRect);
                    g.DrawImage(image, fitted);
                }
            }
            catch (Exception)
            {
                g.DrawString("사진을 불러올 수 없음", labelFont, Brushes.Gray, imageRect, centerFormat);
            }
        }

        private static RectangleF FitRect(Size imageSize, RectangleF bounds)
        {
            var imageRatio = (float)imageSize.Width / imageSize.Height;
            var boundsRatio = bounds.Width / bounds.Height;

            float width, height;
            if (imageRatio > boundsRatio)
            {
                width = bounds.Width;
                height = width / imageRatio;
            }
            else
            {
                height = bounds.Height;
                width = height * imageRatio;
            }

            var x = bounds.X + (bounds.Width - width) / 2f;
            var y = bounds.Y + (bounds.Height - height) / 2f;
            return new RectangleF(x, y, width, height);
        }

        private class BranchFooterInfo
        {
            public string CompanyName;
            public string Address;
            public string Phone;
        }

        // TODO: 영천(A)/녹산(C) 지점 주소·전화번호를 확인되는 대로 채워 넣을 것. 지금은
        // 확인된 생곡(B)지점만 정확하고, 나머지는 비워둬서 그 줄만 빠지고 출력된다.
        private static readonly Dictionary<string, BranchFooterInfo> BranchFooters = new Dictionary<string, BranchFooterInfo>
        {
            { "A", new BranchFooterInfo { CompanyName = "콜럼버스 주식회사 영천지점", Address = "", Phone = "" } },
            { "B", new BranchFooterInfo { CompanyName = "콜럼버스 주식회사 생곡지점", Address = "부산광역시 강서구 생곡산단2로 11번길 30", Phone = "051-972-0314" } },
            { "C", new BranchFooterInfo { CompanyName = "콜럼버스 주식회사 녹산지점", Address = "", Phone = "" } },
        };

        private static void DrawFooter(Graphics g, RectangleF area, float y, Font font)
        {
            BranchFooterInfo info;
            if (_record.BranchCode == null || !BranchFooters.TryGetValue(_record.BranchCode, out info))
            {
                info = new BranchFooterInfo { CompanyName = (_record.BranchCode ?? string.Empty).ToDisplayString() + "지점", Address = "", Phone = "" };
            }

            var centerFormat = new StringFormat { Alignment = StringAlignment.Center };
            var lineHeight = font.GetHeight(g) + 2f;

            g.DrawString("상기와 같이 계량하였음을 증명함.", font, Brushes.Black, new RectangleF(area.Left, y, area.Width, lineHeight), centerFormat);
            y += lineHeight;

            var footerText = info.CompanyName;
            if (!string.IsNullOrEmpty(info.Address))
            {
                footerText += "   " + info.Address;
            }

            if (!string.IsNullOrEmpty(info.Phone))
            {
                footerText += "   " + info.Phone;
            }

            g.DrawString(footerText, font, Brushes.Black, new RectangleF(area.Left, y, area.Width, lineHeight), centerFormat);
        }
    }
}
