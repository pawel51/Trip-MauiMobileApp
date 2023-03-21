using GoogleApi.Entities.Places.Details.Response;
using GoogleApi.Entities.Places.Photos.Response;

namespace Shared.Responses
{
    public class PlaceDetailsDto : BaseResponse
    {
        public PlaceDetailsDto() : base(0)
        {
            PlacesDetailsResponse = new();
            Images = new();
        }

        public DetailsResult PlacesDetailsResponse { get; set; }

        public List<PlacesPhotosResponse> Images { get; set; }
    }
}
