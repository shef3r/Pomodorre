using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Pomodorre.Converters
{
    public class ObjectToJsonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return JsonSerializer.Serialize(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
