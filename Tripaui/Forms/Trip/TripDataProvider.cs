using DevExpress.Maui.DataForm;
using GoogleApi.Entities.Maps.Common.Enums;
using System.Collections;

namespace Tripaui.Forms
{
    public class TripDataProvider : IPickerSourceProvider
    {
        public IEnumerable GetSource(string propertyName)
        {
            if (propertyName == "TravelMode")
            {
                return Enum.GetValues(typeof(TravelMode)).Cast<TravelMode>();
            }
            else
            {
                return Enum.GetValues(typeof(TransitMode)).Cast<TransitMode>();
            }
        }
    }
}
