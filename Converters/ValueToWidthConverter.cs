using System;
using System.Globalization;
using System.Windows.Data;

namespace SizeChartGenerator.Converters
{
    public class ValueToWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 3 && values[0] is double value && 
                values[1] is double maximum && values[2] is double actualWidth)
            {
                return (value / maximum) * actualWidth;
            }
            return 0.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}