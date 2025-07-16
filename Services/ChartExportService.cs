using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using SizeChartGenerator.Models;

namespace SizeChartGenerator.Services
{
    public static class ChartExportService
    {
        private const int CellWidth = 100;
        private const int CellHeight = 40;
        private const int HeaderHeight = 50;
        private const int Padding = 20;
        private const int FontSize = 14;
        private const string FontFamily = "Microsoft YaHei";

        public static void ExportToJpg(SizeChart chart, string filePath)
        {
            if (chart == null || chart.Categories.Count == 0 || chart.Sizes.Count == 0)
                return;

            // 计算图片尺寸
            int totalWidth = (chart.Categories.Count + 1) * CellWidth + Padding * 2;
            int totalHeight = (chart.Sizes.Count + 1) * CellHeight + HeaderHeight + Padding * 2;

            using (var bitmap = new Bitmap(totalWidth, totalHeight))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                // 设置高质量渲染
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                // 填充背景
                graphics.FillRectangle(Brushes.White, 0, 0, totalWidth, totalHeight);

                // 字体设置
                using (var headerFont = new Font(FontFamily, FontSize, FontStyle.Bold))
                using (var cellFont = new Font(FontFamily, FontSize))
                {
                    // 绘制表格
                    DrawTable(graphics, chart, headerFont, cellFont);
                }

                // 添加底部水印
                DrawWatermark(graphics, totalWidth, totalHeight);

                // 保存为JPEG
                var encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 95L);
                
                var jpegCodec = ImageCodecInfo.GetImageDecoders()
                    .FirstOrDefault(codec => codec.FormatID == ImageFormat.Jpeg.Guid);
                
                bitmap.Save(filePath, jpegCodec, encoderParameters);
            }
        }

        private static void DrawTable(Graphics graphics, SizeChart chart, Font headerFont, Font cellFont)
        {
            var blackBrush = Brushes.Black;
            var whiteBrush = Brushes.White;
            var grayBrush = new SolidBrush(Color.FromArgb(245, 245, 245));
            var borderPen = new Pen(Color.FromArgb(224, 224, 224), 1);

            int startX = Padding;
            int startY = Padding;

            // 绘制表头背景
            var headerRect = new Rectangle(startX, startY, (chart.Categories.Count + 1) * CellWidth, HeaderHeight);
            graphics.FillRectangle(blackBrush, headerRect);

            // 绘制表头文字
            var stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            // 尺码列标题
            var cellRect = new Rectangle(startX, startY, CellWidth, HeaderHeight);
            graphics.DrawString("尺码", headerFont, whiteBrush, cellRect, stringFormat);

            // 类别标题
            for (int i = 0; i < chart.Categories.Count; i++)
            {
                cellRect = new Rectangle(startX + (i + 1) * CellWidth, startY, CellWidth, HeaderHeight);
                graphics.DrawString(chart.Categories[i].Name, headerFont, whiteBrush, cellRect, stringFormat);
            }

            // 绘制数据行
            for (int row = 0; row < chart.Sizes.Count; row++)
            {
                int y = startY + HeaderHeight + row * CellHeight;
                
                // 交替行背景色
                if (row % 2 == 1)
                {
                    var rowRect = new Rectangle(startX, y, (chart.Categories.Count + 1) * CellWidth, CellHeight);
                    graphics.FillRectangle(grayBrush, rowRect);
                }

                // 尺码
                cellRect = new Rectangle(startX, y, CellWidth, CellHeight);
                graphics.DrawRectangle(borderPen, cellRect);
                graphics.DrawString(chart.Sizes[row], cellFont, blackBrush, cellRect, stringFormat);

                // 数据
                for (int col = 0; col < chart.Categories.Count; col++)
                {
                    cellRect = new Rectangle(startX + (col + 1) * CellWidth, y, CellWidth, CellHeight);
                    graphics.DrawRectangle(borderPen, cellRect);
                    
                    var category = chart.Categories[col];
                    if (chart.Values.ContainsKey(category.Name) && 
                        chart.Values[category.Name].ContainsKey(chart.Sizes[row]))
                    {
                        var value = chart.Values[category.Name][chart.Sizes[row]];
                        graphics.DrawString(value.ToString(), cellFont, blackBrush, cellRect, stringFormat);
                    }
                }
            }

            // 绘制外边框
            var tableRect = new Rectangle(startX, startY, 
                                         (chart.Categories.Count + 1) * CellWidth, 
                                         HeaderHeight + chart.Sizes.Count * CellHeight);
            graphics.DrawRectangle(new Pen(Color.Black, 2), tableRect);
        }

        private static void DrawWatermark(Graphics graphics, int width, int height)
        {
            using (var watermarkFont = new Font(FontFamily, 10))
            {
                var watermarkFormat = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                
                var watermarkBrush = new SolidBrush(Color.FromArgb(150, 150, 150));
                var watermarkText = $"温馨提示:由于手工测量会存在1-3cm误差，属于正常范围";
                
                var watermarkRect = new Rectangle(0, height - 30, width, 20);
                graphics.DrawString(watermarkText, watermarkFont, watermarkBrush, watermarkRect, watermarkFormat);
            }
        }
    }
}