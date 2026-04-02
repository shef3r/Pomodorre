using Microsoft.UI.Xaml.Data;
using System;

namespace Pomodorre.WinUI.Pages
{
    public class StringEqualsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string stringValue && parameter is string paramValue)
            {
                return stringValue.Equals(paramValue, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolValue && boolValue && parameter is string paramValue)
            {
                return paramValue;
            }
            return null!;
        }
    }
}
