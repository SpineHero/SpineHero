using System;
using System.Globalization;
using System.Windows.Data;

namespace SpineHero.Views.ValueConverters
{
    public class PercentToDegreesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value * 3.6;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value / 3.6;
        }
    }
}