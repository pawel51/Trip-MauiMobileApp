using GoogleApi.Entities.Maps.Common;
using GoogleApi.Entities.Places.Common;
using GoogleApi.Entities.Places.Details.Response;
using Services;
using Shared.GoogleApiModels;
using Shared.Responses;
using static GoogleApi.GooglePlaces;
using static GoogleApi.GooglePlaces.Search;
using static GoogleApi.GoogleMaps;
using GoogleApi.Entities.Common;
using GoogleApi.Entities.Places.Search.NearBy.Response;
using GoogleApi.Entities.Places.Search.NearBy.Request.Enums;

namespace Tripaui.Test.ServicesTest
{
    public class PlacesServiceTest
    {
        string key = "";
        private readonly FindSearchApi _findSearchApi;
        private readonly PhotosApi _photosApi;
        private DetailsApi _placeDetails;
        private TextSearchApi _textSearchApi;
        private readonly PlacesService _placeService;
        private readonly NearBySearchApi _nearBySearchApi;
        private readonly DistanceMatrixApi _distanceMatrixApi;

        public PlacesServiceTest()
        {
            _findSearchApi = new FindSearchApi();
            _photosApi = new PhotosApi();
            _placeDetails = new DetailsApi();
            _textSearchApi = new TextSearchApi();
            _nearBySearchApi = new NearBySearchApi();
            _nearBySearchApi = new NearBySearchApi();
            _distanceMatrixApi = new DistanceMatrixApi();
            _placeService = new(_findSearchApi, _nearBySearchApi, _textSearchApi, _photosApi, _placeDetails, _distanceMatrixApi, null);
        }


        [Fact]
        public async Task TryGetPlaceDetails()
        {
            var placesCandidates = await _placeService.SearchText("colosseum rome", key);
            
            string id = ((PlaceSearchDto<BasicSearchItem>)placesCandidates).PlacesList[0].Candidate.PlaceId;
            var placedetails = await _placeService.GetPlaceDetails(id) as PlaceDetailsDto;
            placedetails.PlacesDetailsResponse.Review ??= new List<Review>();
            placedetails.PlacesDetailsResponse.Photos ??= new List<Photo>();

            Assert.Equal(0, placedetails.Status);
            Assert.NotNull(placedetails);
        }

        [Fact]
        public async Task SearchText_Test()
        {
            var searchResult = (PlaceSearchDto<BasicSearchItem>) await _placeService.SearchText("Restaurant Paris", key);
            foreach (var res in searchResult.PlacesList)
            {
                if(res.Candidate == null)
                {
                    Console.WriteLine("1");
                }
                if(res.Image == null || res.Image.Buffer.Length == 0)
                {
                    Console.WriteLine("1");
                }
            }
            
            Assert.NotNull(searchResult);
            Assert.Equal(0, searchResult.Status);
        }

        [Fact]
        public async Task SearchNearbyTest()
        {
            Coordinate defaultlocation = new Coordinate(52.230370, 21.010816);
            var searchResponse = await _placeService.SearchNearBy("warszawa landmarks", defaultlocation, key) as PlaceSearchDto<BasicSearchItem>;
            Assert.NotNull(searchResponse);
            var placesNearby = searchResponse?.PlacesList;
            Assert.NotNull(placesNearby);
        }
    }
}
