using System.Globalization;
using System.Text;

namespace Tripaui.Converters
{
    public class StringShortcutConverter : IValueConverter
    {
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var newValue = value.ToString();
            var maxLength = (int)parameter;
            if (maxLength >= newValue.Length) return value;
            StringBuilder stringBuilder = new();
            stringBuilder
                .Append(newValue.Substring(0, (int)parameter - 2))
                .Append("..");
            return stringBuilder.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
