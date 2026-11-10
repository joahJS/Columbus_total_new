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
    /// 계근건 1건을 계근표(계량증명서) 형태로 인쇄 미리보기에 띄운다. 항목은 실제 TS2020
    /// 출력물에 맞춰 요청받은 것만 정확히 이 순서로 보여준다: 일자/총중량, 차량번호/공차중량,
    /// 거래처/실중량, 품명/감량, 계량자/구분, 비고(한 줄 전체) - 표 아래에 1차#1/1차#2/2차#1/2차#2
    /// 사진을 가로로 한 줄 나열하고, 마지막에 지점 정보 푸터(BuildInfoRows 참고). 현장에서는
    /// A4 세로 한 장에 같은 내용을
    /// 위아래 두 칸에 반복해서 찍어, 잘라서 업체용/보관용으로 나눠 가진다고 확인되어 그
    /// 방식을 그대로 따랐다. GridPrinter와 마찬가지로 DevExpress 인쇄 어셈블리 의존 없이
    /// 표준 PrintDocument로 직접 그린다.
    ///
    /// "총중량"/"공차중량"은 TS2020처럼 1차/2차 순서가 아니라 더 무거운 쪽/가벼운 쪽
    /// 기준이다 - 입고/출고 어느 쪽이든 총중량이 항상 더 큰 값이 되도록.
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

                var tableHeight = DrawInfoTable(g, area.Left, y, area.Width, labelFont, valueFont);
                y += tableHeight + 6f;

                // 사진 줄에 쓸 수 있는 최대 공간(표/푸터를 뺀 나머지)을 구한 다음, 실제
                // 사진 비율에 맞는 높이만큼만 쓴다(ComputePhotoRowHeight 참고) - 무조건 꽉
                // 채우면 가로로 긴 사진이라 위아래에 빈 여백만 커진다.
                var footerHeight = (footerFont.GetHeight(g) + 2f) * 2;
                var maxPhotoRowHeight = Math.Max(110f, area.Bottom - y - footerHeight - 6f);
                var photoLabelFont = new Font("맑은 고딕", 7f);
                float photoRowHeight;
                try
                {
                    photoRowHeight = ComputePhotoRowHeight(g, area.Width / 4f, maxPhotoRowHeight, photoLabelFont);
                }
                finally
                {
                    photoLabelFont.Dispose();
                }

                DrawPhotoRow(g, area.Left, y, area.Width, photoRowHeight);
                y += photoRowHeight + 4f;

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

            /// <summary>총중량/공차중량처럼 Value2 칸 왼쪽에 시간을 같이 보여줘야 하는 행만
            /// 채운다(전달받은 계근표 이미지처럼 "시간 ... 중량"을 한 칸 안에 나눠 찍는다).</summary>
            public string Value2Time;

            /// <summary>true면 Value2를 칸 오른쪽에 붙여 찍는다(중량 값들은 기본 우측정렬).</summary>
            public bool RightAlignValue2;

            /// <summary>true면 이 행은 Label1/Value1만 쓰고, Value1 칸이 나머지 폭 전체를
            /// 차지한다(비고처럼 한 줄 전체를 쓰는 항목용) - Label2/Value2는 무시된다.</summary>
            public bool FullWidth;
        }

        /// <summary>요청받은 항목만 정확히 이 순서로: 일자/총중량, 차량번호/공차중량,
        /// 거래처/실중량, 품명/감량, 계량자/구분, 비고(한 줄 전체). "총중량"/"공차중량"은
        /// 1차/2차 계량 순서가 아니라 더 무거운 쪽/가벼운 쪽 기준이다 - 입고든 출고든 총중량이
        /// 항상 더 큰 값이 되도록(SyncOrchestrator.Transform()의 입출구분 재계산과 같은 기준).</summary>
        private static List<InfoRow> BuildInfoRows()
        {
            var r = _record;

            var firstIsGross = !r.SecondWeight.HasValue || r.FirstWeight >= r.SecondWeight.Value;
            var grossTime = firstIsGross ? r.FirstDateTime : r.SecondDateTime;
            var grossWeight = firstIsGross ? (decimal?)r.FirstWeight : r.SecondWeight;
            var tareTime = firstIsGross ? r.SecondDateTime : (DateTime?)r.FirstDateTime;
            var tareWeight = firstIsGross ? r.SecondWeight : (decimal?)r.FirstWeight;

            return new List<InfoRow>
            {
                new InfoRow { Label1 = "일자", Value1 = r.FirstDateTime.ToString("yyyy-MM-dd"), Label2 = "총중량", Value2Time = grossTime.HasValue ? grossTime.Value.ToString("HH:mm") : "-", Value2 = grossWeight.HasValue ? FormatWeight(grossWeight.Value) : "-", RightAlignValue2 = true },
                new InfoRow { Label1 = "차량번호", Value1 = r.VehicleNo, Label2 = "공차중량", Value2Time = tareTime.HasValue ? tareTime.Value.ToString("HH:mm") : "-", Value2 = tareWeight.HasValue ? FormatWeight(tareWeight.Value) : "-", RightAlignValue2 = true },
                new InfoRow { Label1 = "거래처", Value1 = r.CustomerName, Label2 = "실중량", Value2 = r.NetWeight.HasValue ? FormatWeight(r.NetWeight.Value) : "-", RightAlignValue2 = true },
                new InfoRow { Label1 = "품명", Value1 = r.ProductName, Label2 = "감량", Value2 = r.LossWeight.HasValue ? FormatWeight(r.LossWeight.Value) : "-", RightAlignValue2 = true },
                new InfoRow { Label1 = "계량자", Value1 = r.WeigherName, Label2 = "구분", Value2 = r.InOutType.HasValue ? r.InOutType.Value.ToDisplayString() : "-" },
                new InfoRow { Label1 = "비고", Value1 = r.Remark, FullWidth = true },
            };
        }

        private static string FormatWeight(decimal value)
        {
            return value.ToString("N0") + " kg";
        }

        private static float DrawInfoTable(Graphics g, float x, float y, float width, Font labelFont, Font valueFont)
        {
            var startY = y;
            var labelWidth = width * 0.22f;
            var valueWidth = width * 0.28f;
            var rowHeight = Math.Max(labelFont.GetHeight(g), valueFont.GetHeight(g)) + 8f;

            using (var borderPen = new Pen(Color.Black))
            using (var gridPen = new Pen(Color.Gray))
            {
                foreach (var row in BuildInfoRows())
                {
                    var cellX = x;
                    DrawCell(g, cellX, y, labelWidth, rowHeight, row.Label1, labelFont, gridPen, StringAlignment.Center, isLabel: true);
                    cellX += labelWidth;

                    if (row.FullWidth)
                    {
                        DrawCell(g, cellX, y, width - labelWidth, rowHeight, row.Value1, valueFont, gridPen, StringAlignment.Near);
                    }
                    else
                    {
                        DrawCell(g, cellX, y, valueWidth, rowHeight, row.Value1, valueFont, gridPen, StringAlignment.Near);
                        cellX += valueWidth;
                        DrawCell(g, cellX, y, labelWidth, rowHeight, row.Label2, labelFont, gridPen, StringAlignment.Center, isLabel: true);
                        cellX += labelWidth;
                        var value2Width = width - labelWidth - valueWidth - labelWidth;

                        if (row.Value2Time != null)
                        {
                            DrawSplitCell(g, cellX, y, value2Width, rowHeight, row.Value2Time, row.Value2, valueFont, gridPen);
                        }
                        else
                        {
                            DrawCell(g, cellX, y, value2Width, rowHeight, row.Value2, valueFont, gridPen, row.RightAlignValue2 ? StringAlignment.Far : StringAlignment.Near);
                        }
                    }

                    y += rowHeight;
                }

                g.DrawRectangle(borderPen, x, startY, width, y - startY);
            }

            return y - startY;
        }

        private static void DrawCell(Graphics g, float x, float y, float width, float height, string text, Font font, Pen gridPen, StringAlignment alignment, bool isLabel = false)
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
                Alignment = alignment,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap,
            };
            var padded = new RectangleF(x + 4f, y, width - 8f, height);
            g.DrawString(text ?? string.Empty, font, Brushes.Black, padded, format);
        }

        /// <summary>한 칸 안에 왼쪽은 시간, 오른쪽은 중량을 나눠 찍는다(총중량/공차중량 행용 -
        /// 전달받은 계근표 이미지처럼 "시간 ... 중량"을 한 칸에 같이 보여준다).</summary>
        private static void DrawSplitCell(Graphics g, float x, float y, float width, float height, string leftText, string rightText, Font font, Pen gridPen)
        {
            g.DrawRectangle(gridPen, x, y, width, height);

            var leftFormat = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap,
            };
            var rightFormat = new StringFormat
            {
                Alignment = StringAlignment.Far,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap,
            };

            var padded = new RectangleF(x + 4f, y, width - 8f, height);
            g.DrawString(leftText ?? string.Empty, font, Brushes.Black, padded, leftFormat);
            g.DrawString(rightText ?? string.Empty, font, Brushes.Black, padded, rightFormat);
        }

        private class PhotoSlot
        {
            public string Stage;
            public int Seq;
            public string Label;
        }

        // 이 프로젝트는 구버전 csproj라 System.ValueTuple 패키지가 안 잡혀있어 튜플 리터럴을
        // 쓸 수 없다(CS8137/CS8179) - 그래서 클래스로 대신한다.
        private static readonly PhotoSlot[] PhotoSlots =
        {
            new PhotoSlot { Stage = "1", Seq = 1, Label = "1차 #1" },
            new PhotoSlot { Stage = "1", Seq = 2, Label = "1차 #2" },
            new PhotoSlot { Stage = "2", Seq = 1, Label = "2차 #1" },
            new PhotoSlot { Stage = "2", Seq = 2, Label = "2차 #2" },
        };

        /// <summary>1차#1/1차#2/2차#1/2차#2 네 장을 표 아래에 가로로 한 줄 나열한다(이전엔
        /// 표 오른쪽에 2x2로 붙여놨었는데, 한 줄로 바꿔달라는 요청에 맞췄다).</summary>
        private static void DrawPhotoRow(Graphics g, float x, float y, float width, float height)
        {
            var cellWidth = width / 4f;
            var labelFont = new Font("맑은 고딕", 7f);

            try
            {
                for (var i = 0; i < PhotoSlots.Length; i++)
                {
                    var slot = PhotoSlots[i];
                    DrawPhotoCell(g, x + cellWidth * i, y, cellWidth, height, slot.Label, FindPhoto(slot.Stage, slot.Seq), labelFont);
                }
            }
            finally
            {
                labelFont.Dispose();
            }
        }

        /// <summary>사진 줄에 쓸 높이를 실제 사진들의 가로세로 비율에 맞춰 계산한다. 가능한
        /// 세로 공간을 무조건 다 채우면(이전 방식) 사진은 보통 가로로 긴데 칸만 세로로
        /// 길어져서 위아래에 빈 여백만 커지는 문제가 있었다 - 실제 사진 비율대로 칸 너비에서
        /// 필요한 높이를 계산해서, 그 중 가장 큰 값(+라벨 높이)만큼만 쓰고 나머지는 포기한다.
        /// 사진이 하나도 없으면 기본값(110)을 쓴다.</summary>
        private static float ComputePhotoRowHeight(Graphics g, float cellWidth, float maxHeight, Font labelFont)
        {
            var labelHeight = labelFont.GetHeight(g) + 2f;
            var maxImageHeight = 0f;

            foreach (var slot in PhotoSlots)
            {
                var photo = FindPhoto(slot.Stage, slot.Seq);
                if (photo == null || photo.Data == null || photo.Data.Length == 0)
                {
                    continue;
                }

                try
                {
                    using (var stream = new MemoryStream(photo.Data))
                    using (var image = Image.FromStream(stream))
                    {
                        ApplyExifOrientation(image);
                        var neededImageHeight = cellWidth * image.Height / (float)image.Width;
                        if (neededImageHeight > maxImageHeight)
                        {
                            maxImageHeight = neededImageHeight;
                        }
                    }
                }
                catch (Exception)
                {
                    // 디코딩 실패한 사진은 크기 계산에서 건너뛴다 - 실제로 찍을 때는
                    // DrawPhotoCell이 "사진을 불러올 수 없음"으로 따로 처리한다.
                }
            }

            var needed = maxImageHeight > 0f ? maxImageHeight + labelHeight + 4f : 110f;
            return Math.Min(maxHeight, Math.Max(80f, needed));
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
                    ApplyExifOrientation(image);
                    var fitted = FitRect(image.Size, imageRect);
                    g.DrawImage(image, fitted);
                }
            }
            catch (Exception)
            {
                g.DrawString("사진을 불러올 수 없음", labelFont, Brushes.Gray, imageRect, centerFormat);
            }
        }

        private const int ExifOrientationTagId = 0x0112;

        /// <summary>카메라가 세로로 찍은 사진을 가로 픽셀로 저장하고 "회전해서 보여줘라"는
        /// EXIF Orientation 태그만 남겨두는 경우가 있다(실제로 토탈솔루션에서 사진이 위아래로
        /// 눌린 가로 사진처럼 나오는 원인이었다 - 실제 사진 속 픽셀 자체는 가로였던 것).
        /// GDI+(System.Drawing)는 이 태그를 자동으로 반영하지 않아서, 여기서 직접 읽어
        /// RotateFlip으로 돌려놓는다 - 그래야 이후 Width/Height와 그려지는 모양이 실제
        /// 의도한 방향과 맞는다.</summary>
        private static void ApplyExifOrientation(Image image)
        {
            if (Array.IndexOf(image.PropertyIdList, ExifOrientationTagId) < 0)
            {
                return;
            }

            var orientation = image.GetPropertyItem(ExifOrientationTagId).Value[0];
            switch (orientation)
            {
                case 2: image.RotateFlip(RotateFlipType.RotateNoneFlipX); break;
                case 3: image.RotateFlip(RotateFlipType.Rotate180FlipNone); break;
                case 4: image.RotateFlip(RotateFlipType.Rotate180FlipX); break;
                case 5: image.RotateFlip(RotateFlipType.Rotate90FlipX); break;
                case 6: image.RotateFlip(RotateFlipType.Rotate90FlipNone); break;
                case 7: image.RotateFlip(RotateFlipType.Rotate270FlipX); break;
                case 8: image.RotateFlip(RotateFlipType.Rotate270FlipNone); break;
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
