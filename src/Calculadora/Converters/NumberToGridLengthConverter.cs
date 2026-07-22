using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Calculadora.Converters
{
    public class NumberToGridLengthConverter : IValueConverter
    {
        public static readonly NumberToGridLengthConverter Instance = new NumberToGridLengthConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                return new GridLength(d);
            }
            if (value is int i)
            {
                return new GridLength(i);
            }
            return new GridLength(220);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GridLength gl)
            {
                return gl.Value;
            }
            return 220;
        }
    }
}
