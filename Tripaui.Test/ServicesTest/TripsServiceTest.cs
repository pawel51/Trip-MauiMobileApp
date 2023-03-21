using Microsoft.Extensions.Configuration;
using Repositories;
using Services;
using Shared.Common;
using Shared.Entities;
using Shared.GoogleApiModels;
using Shared.Responses;
using System.Diagnostics;
using static GoogleApi.GoogleMaps;
using static GoogleApi.GooglePlaces;
using static GoogleApi.GooglePlaces.Search;

namespace Tripaui.Test.ServicesTest
{
    public sealed class TripsServiceTest : FirebaseServiceTest
    {
        string key = "";
        private readonly TripsService _tripsService;
        private FindSearchApi _findSearchApi;
        private PhotosApi _photosApi;
        private DetailsApi _placeDetails;
        private readonly TextSearchApi _textSearchApi;
        private PlacesService _placeService;
        private NearBySearchApi _nearBySearchApi;
        private DistanceMatrixApi _distanceMatrixApi;

        public TripsServiceTest() : base()
        {
            _findSearchApi = new FindSearchApi();
            _photosApi = new PhotosApi();
            _placeDetails = new DetailsApi();
            _textSearchApi = new TextSearchApi();
            _nearBySearchApi = new NearBySearchApi();
            _distanceMatrixApi = new();
            _placeService = new(_findSearchApi, _nearBySearchApi, _textSearchApi, _photosApi, _placeDetails, _distanceMatrixApi, null);
            _tripsService = new TripsService(new TripsRepository(ct), _placeService);
        }

        [Fact]
        public async Task AddTrip_ShouldAddTrip()
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
            var newTripId = model.Id;
            await _tripsService.AddTrip(model);
            var trips = await _tripsService.GetAllTripsOfLoggedInUser();
            Assert.NotNull(trips);
            if (trips is CollectionResponse<TripSmallDetaisDto> userTripsResponse)
            {
                var addedTrip = userTripsResponse.Collection.FirstOrDefault(t => t.TripModel.Id == newTripId);
                Assert.NotNull(addedTrip);
            }
        }

        [Fact]
        public async Task AddPlaceToTrip_ShouldAddPlaceToTrip()
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
            var newPlace = await _placeService.SearchText("Colosseum Rome", key) as PlaceSearchDto<BasicSearchItem>;
            if (newPlace == null || newPlace.PlacesList.Count() == 0) throw new Exception("Place not found");
            string newPlaceId = newPlace.PlacesList[0].Candidate.PlaceId;
            await _tripsService.AddTrip(model);
            await _tripsService.AddPlaceToTripAsync(newTripId, newPlaceId);
            var tripDetails = await _tripsService.GetTripDetails(newTripId) as TripDetailsDto;
            if (tripDetails == null) throw new Exception($"Trip doesnt exist in database");
            Assert.NotNull(tripDetails.PlaceDetailsList.FirstOrDefault(place => place.PlacesDetailsResponse.PlaceId == newPlaceId));
        }

        [Fact]
        public async Task AddPlaceToTripWhichAlreadyExistsOnThatTrip_ShouldReturnErrorResponse()
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
            var newPlace = await _placeService.SearchText("Colosseum Rome", key) as PlaceSearchDto<BasicSearchItem>;
            if (newPlace == null || newPlace.PlacesList.Count() == 0) throw new Exception("Place not found");
            string newPlaceId = newPlace.PlacesList[0].Candidate.PlaceId;
            await _tripsService.AddTrip(model);
            await _tripsService.AddPlaceToTripAsync(newTripId, newPlaceId);
            var response = await _tripsService.AddPlaceToTripAsync(newTripId, newPlaceId) as DefaultErrorResponse;
            Assert.NotNull(response);
            var tripDetails = await _tripsService.GetTripDetails(newTripId) as TripDetailsDto;
            if (tripDetails == null) throw new Exception("Test Initialization Error. Trip doesn't exists");
            var placesWithIdAddedCount = tripDetails.PlaceDetailsList.Count(e => e.PlacesDetailsResponse.PlaceId.Equals(newPlaceId));
            Assert.True(1 == placesWithIdAddedCount);
        }

        [Fact]
        public async Task DeletePlaceAsync_shouldDeletePlaceById()
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
            var newPlace = await _placeService.SearchText("Colosseum Rome", key) as PlaceSearchDto<BasicSearchItem>;
            if (newPlace == null || newPlace.PlacesList.Count() == 0) throw new Exception("Place not found");
            string newPlaceId = newPlace.PlacesList[0].Candidate.PlaceId;
            await _tripsService.AddTrip(model);
            await _tripsService.AddPlaceToTripAsync(newTripId, newPlaceId);
            await _tripsService.DeletePlaceFromTrip(newTripId, newPlaceId);
            var tripDetails = await _tripsService.GetTripDetails(newTripId) as TripDetailsDto;
            if (tripDetails == null) throw new Exception("Trip wasn't found");
            var removedPlaceId = tripDetails.PlaceDetailsList.FirstOrDefault(p => p.PlacesDetailsResponse.PlaceId == newPlaceId);
            Assert.Null(removedPlaceId);
        }

        [Fact]
        public async Task GetAllTrips_ShouldReturnCorrectAmountOfTrips()
        {
            if (!ct.IsAuthenticated())
                Debug.WriteLine("unathenticated!!");
            await _tripsService.DeleteAllTrips();

            for (int i = 0; i < 3; i++)
            {
                TripModel model = new()
                {
                    CreatedAt = DateTime.Now,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(2),
                    Name = $"Another boiring trip {i}"
                };
                await _tripsService.AddTrip(model);
            }

            var trips = await _tripsService.GetAllTripsOfLoggedInUser() as CollectionResponse<TripSmallDetaisDto>;
            if (trips == null) throw new Exception("Firebase error");
            Assert.Equal(3, trips.Collection.Count());

            //Cleanup
            await _tripsService.DeleteAllTrips();
        }

        [Fact]
        public async Task DeleteTrip_ShouldDeleteAddedTrip()
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
            var newTripId = model.Id;
            await _tripsService.AddTrip(model);
            await _tripsService.DeleteTripById(model.Id.ToString());

            var trips = await _tripsService.GetAllTripsOfLoggedInUser();
            if (trips is CollectionResponse<TripSmallDetaisDto> userTripsResponse)
            {
                var addedTrip = userTripsResponse.Collection.FirstOrDefault(t => t.TripModel.Id == newTripId);
                Assert.Null(addedTrip);
            }
            else
            {
                throw new ArgumentException("Trip wasn't added correctly");
            }

        }
    }
}
