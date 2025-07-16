using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SizeChartGenerator.Models;

namespace SizeChartGenerator
{
    public class SizeChartPreview : Control
    {
        public static readonly DependencyProperty ChartProperty =
            DependencyProperty.Register("Chart", typeof(SizeChart), typeof(SizeChartPreview),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public SizeChart Chart
        {
            get { return (SizeChart)GetValue(ChartProperty); }
            set { SetValue(ChartProperty, value); }
        }

        static SizeChartPreview()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SizeChartPreview),
                new FrameworkPropertyMetadata(typeof(SizeChartPreview)));
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (Chart == null || Chart.Categories.Count == 0 || Chart.Sizes.Count == 0)
                return;

            const double cellWidth = 100;
            const double cellHeight = 40;
            const double headerHeight = 50;

            var blackBrush = new SolidColorBrush(Colors.Black);
            var whiteBrush = new SolidColorBrush(Colors.White);
            var grayBrush = new SolidColorBrush(Color.FromRgb(245, 245, 245));
            var borderPen = new Pen(new SolidColorBrush(Color.FromRgb(224, 224, 224)), 1);
            var typeface = new Typeface("Microsoft YaHei");

            // 绘制表头
            var headerRect = new Rect(0, 0, (Chart.Categories.Count + 1) * cellWidth, headerHeight);
            drawingContext.DrawRectangle(blackBrush, null, headerRect);

            // 绘制表头文字
            DrawText(drawingContext, "尺码", 0, 0, cellWidth, headerHeight, whiteBrush, typeface, 14, FontWeights.Bold);

            for (int i = 0; i < Chart.Categories.Count; i++)
            {
                DrawText(drawingContext, Chart.Categories[i].Name, 
                    (i + 1) * cellWidth, 0, cellWidth, headerHeight, 
                    whiteBrush, typeface, 14, FontWeights.Bold);
            }

            // 绘制数据行
            for (int row = 0; row < Chart.Sizes.Count; row++)
            {
                double y = headerHeight + row * cellHeight;

                // 交替行背景
                if (row % 2 == 1)
                {
                    var rowRect = new Rect(0, y, (Chart.Categories.Count + 1) * cellWidth, cellHeight);
                    drawingContext.DrawRectangle(grayBrush, null, rowRect);
                }

                // 尺码
                DrawCell(drawingContext, Chart.Sizes[row], 0, y, cellWidth, cellHeight, 
                    blackBrush, borderPen, typeface);

                // 数据
                for (int col = 0; col < Chart.Categories.Count; col++)
                {
                    var category = Chart.Categories[col];
                    var value = "";
                    
                    if (Chart.Values.ContainsKey(category.Name) && 
                        Chart.Values[category.Name].ContainsKey(Chart.Sizes[row]))
                    {
                        value = Chart.Values[category.Name][Chart.Sizes[row]].ToString();
                    }

                    DrawCell(drawingContext, value, (col + 1) * cellWidth, y, 
                        cellWidth, cellHeight, blackBrush, borderPen, typeface);
                }
            }

            // 绘制外边框
            var tableRect = new Rect(0, 0, 
                (Chart.Categories.Count + 1) * cellWidth,
                headerHeight + Chart.Sizes.Count * cellHeight);
            drawingContext.DrawRectangle(null, new Pen(blackBrush, 2), tableRect);
        }

        private void DrawText(DrawingContext context, string text, double x, double y, 
            double width, double height, Brush brush, Typeface typeface, double fontSize, FontWeight fontWeight)
        {
            var formattedText = new FormattedText(
                text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                fontSize,
                brush,
                VisualTreeHelper.GetDpi(this).PixelsPerDip)
            {
                TextAlignment = TextAlignment.Center,
                FontWeight = fontWeight
            };

            var center = new Point(x + width / 2, y + height / 2 - formattedText.Height / 2);
            context.DrawText(formattedText, center);
        }

        private void DrawCell(DrawingContext context, string text, double x, double y,
            double width, double height, Brush textBrush, Pen borderPen, Typeface typeface)
        {
            var rect = new Rect(x, y, width, height);
            context.DrawRectangle(null, borderPen, rect);
            DrawText(context, text, x, y, width, height, textBrush, typeface, 14, FontWeights.Normal);
        }
    }
}