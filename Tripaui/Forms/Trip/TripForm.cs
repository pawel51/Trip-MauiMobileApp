using CommunityToolkit.Mvvm.ComponentModel;
using GoogleApi.Entities.Maps.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace Tripaui.Forms
{
    public sealed partial class TripForm : BaseForm
    {
        [ObservableProperty]
        [Required(ErrorMessage = "Required")]
        private string name = "";

        [ObservableProperty]
        [Required(ErrorMessage = "Required")]
        private DateTime startDate = DateTime.Now;

        [ObservableProperty]
        [Required(ErrorMessage = "Required")]
        private DateTime endDate = DateTime.Now;

        [ObservableProperty]
        [Required(ErrorMessage = "Required")]
        private TimeSpan startTime = new TimeSpan(12,0,0);

        [ObservableProperty]
        [Required(ErrorMessage = "Required")]
        private TravelMode travelMode = TravelMode.Driving;

        [ObservableProperty]
        [Required(ErrorMessage = "Required")]
        private TransitMode transitMode = TransitMode.Bus;

    }
}
