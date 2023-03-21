using Shared.Entities;
using System.Collections.ObjectModel;

namespace Shared.Responses
{
    public class TripDetailsDto : BaseResponse
    {
        public TripDetailsDto() : base(0)
        {
        }

        public TripModel TripModel { get; set; } = new();

        public ObservableCollection<PlaceDetailsDto> PlaceDetailsList { get; set; } = new();
    }
}
