using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace KinectKeyboardSimulator
{
    public class BooleanToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visible = Visibility.Collapsed;

            if (value != null)
            {
                bool b = (bool)value;
                visible = b == false ? Visibility.Collapsed : Visibility.Visible;
            }
            return visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
