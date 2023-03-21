using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GoogleApi.GoogleMaps;
using static GoogleApi.GooglePlaces.Search;
using static GoogleApi.GooglePlaces;
using Shared.Entities;
using System.Diagnostics;
using Shared.Common;
using Shared.Responses;
using Shared.GoogleApiModels;
using GoogleApi.Entities.Maps.Directions.Response;

namespace Tripaui.Test.ServicesTest
{
    public class DirectionsTest : FirebaseServiceTest
    {
        string key = "";
        private DirectionsApi _directionsApi;
        private DistanceMatrixApi _distanceMatrixApi;
        private DirectionsService _directionsService;
        private readonly TripsService _tripsService;
        private FindSearchApi _findSearchApi;
        private PhotosApi _photosApi;
        private DetailsApi _placeDetails;
        private readonly TextSearchApi _textSearchApi;
        private readonly NearBySearchApi _nearBySearchApi;
        private PlacesService _placeService;
        public DirectionsTest() 
        {
            _directionsApi = new DirectionsApi();
            _findSearchApi = new FindSearchApi();
            _photosApi = new PhotosApi();
            _placeDetails = new DetailsApi();
            _textSearchApi = new TextSearchApi();
            _nearBySearchApi = new NearBySearchApi();
            _distanceMatrixApi = new DistanceMatrixApi();
            _placeService = new(_findSearchApi, _nearBySearchApi, _textSearchApi, _photosApi, _placeDetails, _distanceMatrixApi, null);
            _directionsService = new DirectionsService(
                _directionsApi,
                new Repositories.PlanRepository(ct),
                null);
            _tripsService = new TripsService(
                new Repositories.TripsRepository(ct),
                _placeService);
        }


        [Fact]
        public async Task ShouldAddNewPlan()
        {
            TripPlan plan = new TripPlan();
            var response = await _directionsService.SaveItemForTripAsync("tripId1", plan) as OkResponse;
            Assert.NotNull(response);
        }

        [Fact]
        public async Task ShouldDeletePlan()
        {
            TripPlan plan = new TripPlan();
            var addResponse = await _directionsService.SaveItemForTripAsync("tripId1", plan) as OkResponse;
            Assert.NotNull(addResponse);

            var getResponse = await _directionsService.GetItemOfTrip("tripId1") as OkResponse;
            Assert.NotNull(getResponse);
            if (getResponse == null)
            {
                throw new Exception("Cant test deletion because addition is not working");
            }
            var addedItem = getResponse.Payload as TripPlan;
            Assert.NotNull(addedItem);

            var deleteResponse = await _directionsService.DeleteItemOfTrip("tripId1") as OkResponse;
            Assert.NotNull(deleteResponse);
        }


        [Fact]
        public async Task ShouldReturnCollectionOfCoordinates()
        {
            if (!ct.IsAuthenticated())
                Debug.WriteLine("unathenticated!!");
            await _tripsService.DeleteAllTrips();
            TripModel model = new()
            {
                CreatedAt = DateTime.Now,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(2),
                Name = "My awesome trip in mushroomland"
            };
            var newTripId = model.Id.ToString();
            await _tripsService.AddTrip(model);

            await AddPlacesToTrip(newTripId);

            var directionsResponse = await GetDirectionResponseFromTrip(model);

            var tripDetails = await _tripsService.GetTripDetails(newTripId) as TripDetailsDto;

            var route = directionsResponse.Routes.FirstOrDefault();
            var waypoints = directionsResponse.WayPoints.ToList();

            var dictionaryOfPolylineCoordinates = _directionsService.GetDictionaryOfPolylineCoordinates(route, waypoints);
            var length = dictionaryOfPolylineCoordinates.Count;
            Assert.NotNull(dictionaryOfPolylineCoordinates);
            Assert.Equal(3, length);
            var lastList = dictionaryOfPolylineCoordinates.ToList()[length - 1];
            var lastButOneList = dictionaryOfPolylineCoordinates.ToList()[length - 2];
            Assert.Equal(lastButOneList.Value.Last().Latitude, lastList.Value.First().Latitude);
            Assert.Equal(lastButOneList.Value.Last().Longitude, lastList.Value.First().Longitude);
        }

        [Fact]
        public async Task ShouldGenerateBasicPlan()
        {
            if (!ct.IsAuthenticated())
                Debug.WriteLine("unathenticated!!");
            await _tripsService.DeleteAllTrips();
            TripModel model = new()
            {
                CreatedAt = DateTime.Now,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(2),
                Name = "My awesome trip in mushroomland"
            };
            var newTripId = model.Id.ToString();
            await _tripsService.AddTrip(model);

            await AddPlacesToTrip(newTripId);

            var directionsResponse = await GetDirectionResponseFromTrip(model);

            var tripDetails = await _tripsService.GetTripDetails(newTripId) as TripDetailsDto;

            var route = directionsResponse.Routes.FirstOrDefault();
            var waypoints = directionsResponse.WayPoints.ToList();

            var response = _directionsService.GenerateBasicTripPlan(route, waypoints, tripDetails);

            Assert.NotNull(response);
            Assert.True(response.SinglePlacePlans.Count == 2);
            Assert.NotNull(response.Destination);
        }

        private async Task<DirectionsResponse> GetDirectionResponseFromTrip(TripModel model)
        {
            var placesIdsResponse = await _tripsService.GetPlacesIdsFromTrip(model.Id.ToString());
            var placesIds = ((OkResponse)placesIdsResponse).Payload as List<string>;
            var response = await _directionsService.GetTripDirectionReponse(model, key);
            return (DirectionsResponse)((OkResponse)response).Payload;
        }

        private async Task AddPlacesToTrip(string newTripId)
        {
            var skyclub = await _placeService.SearchText("Sky club bialystok", key) as PlaceSearchDto<BasicSearchItem>;
            var maskiclub = await _placeService.SearchText("maski club bialystok", key) as PlaceSearchDto<BasicSearchItem>;
            var hellclub = await _placeService.SearchText("hell club bialystok", key) as PlaceSearchDto<BasicSearchItem>;
            await _tripsService.AddPlaceToTripAsync(newTripId, maskiclub.PlacesList.First().Candidate.PlaceId);
            await _tripsService.AddPlaceToTripAsync(newTripId, skyclub.PlacesList.First().Candidate.PlaceId);
            await _tripsService.AddPlaceToTripAsync(newTripId, hellclub.PlacesList.First().Candidate.PlaceId);
        }
    }
}
