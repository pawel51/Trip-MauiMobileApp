using GoogleApi.Entities.Places.Common.Enums;
using Microsoft.Maui.Controls;
using System.Globalization;

namespace Tripaui.Converters
{
    public class PriceLevelToPercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case PriceLevel.VeryExpensive:
                    return "1";
                case PriceLevel.Expensive:
                    return "0.75";
                case PriceLevel.Moderate:
                    return "0.5";
                case PriceLevel.Inexpensive:
                    return "0.25";
                default:
                    return "0";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
