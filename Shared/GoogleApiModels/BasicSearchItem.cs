using CommunityToolkit.Mvvm.ComponentModel;
using GoogleApi.Entities.Places.Photos.Response;
using GoogleApi.Entities.Places.Search.Common;

namespace Shared.GoogleApiModels
{
    public partial class BasicSearchItem : ObservableObject
    {
        public BaseResult Candidate { get; set; }
        public PlacesPhotosResponse? Image { get; set; }
        public string FormatedAddress { get; set; } = "";

        [ObservableProperty]
        public string textDistanceInKm = "";

        [ObservableProperty]
        public int distanceInMeters = 0;
    }
}
