using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Age_Of_Nothing.UI.Converters
{
    [ValueConversion(typeof(bool), typeof(Brush))]
    public class CraftStuckPropertyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && (bool)value)
                return Brushes.Red;
            else
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF01D328"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
