using System.Globalization;

namespace Tripaui.Converters
{
    public class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(string))
                throw new InvalidOperationException("The target must be a string");
            var timeSpan = (TimeSpan)value;
            string minutes = timeSpan.Minutes < 10 ? "0" + timeSpan.Minutes : "" + timeSpan.Minutes;
            return "" + (int)timeSpan.TotalHours + ":" + minutes;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
