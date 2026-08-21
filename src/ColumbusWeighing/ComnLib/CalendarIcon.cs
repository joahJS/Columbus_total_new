using System.Drawing;
using System.Drawing.Drawing2D;

namespace ColumbusWeighing.ComnLib
{
    /// <summary>
    /// DateEdit류 컨트롤의 드롭다운 버튼에 쓸 달력 모양 아이콘을 코드로 직접 그린다.
    /// 현재 스킨("Basic")에서는 ButtonPredefines.Glyph 버튼에 기본 제공되는 달력 이미지가 없어
    /// 화살표조차 표시되지 않으므로, 외부 이미지 리소스 없이 항상 같은 모양이 보이도록 그려서 쓴다.
    /// </summary>
    public static class CalendarIcon
    {
        public static Image Create()
        {
            var bitmap = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                var body = new Rectangle(1, 3, 13, 11);
                using (var bodyBrush = new SolidBrush(Color.White))
                using (var headerBrush = new SolidBrush(Color.FromArgb(41, 128, 225)))
                using (var borderPen = new Pen(Color.FromArgb(90, 90, 90)))
                using (var gridPen = new Pen(Color.FromArgb(200, 200, 200)))
                {
                    g.FillRectangle(bodyBrush, body);
                    g.FillRectangle(headerBrush, body.X, body.Y, body.Width, 4);
                    g.DrawRectangle(borderPen, body);
                    g.DrawLine(borderPen, body.X, body.Y + 4, body.Right, body.Y + 4);

                    g.DrawLine(gridPen, body.X + 4, body.Y + 6, body.X + 4, body.Bottom - 2);
                    g.DrawLine(gridPen, body.X + 8, body.Y + 6, body.X + 8, body.Bottom - 2);

                    g.DrawLine(borderPen, 4, 1, 4, 4);
                    g.DrawLine(borderPen, 11, 1, 11, 4);
                }
            }

            return bitmap;
        }
    }
}
