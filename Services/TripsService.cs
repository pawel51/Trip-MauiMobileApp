using Firebase.Database;
using GoogleApi.Entities.Places.Details.Request.Enums;
using Repositories;
using Shared.Common;
using Shared.Entities;
using Shared.Responses;
using System.Collections.ObjectModel;
using BaseResponse = Shared.Responses.BaseResponse;

namespace Services
{
    public sealed class TripsService
    {
        private readonly TripsRepository _tripsRepository;
        private readonly PlacesService _placesService;

        public TripsService(TripsRepository tripsRepository, PlacesService placesService)
        {
            _tripsRepository = tripsRepository;
            _placesService = placesService;
        }

        public async Task<BaseResponse> AddTrip(TripModel tripModel)
        {
            try
            {
                await _tripsRepository.PostAsync(tripModel);
            }
            catch (Exception ex)
            {
                return new DefaultErrorResponse() { Message = ex.Message };
            }
            
            return new OkResponse();
        }

        public async Task<BaseResponse> GetAllTripsOfLoggedInUser()
        {
            return await GetAllArchivizedTrips(false);
        }

        public async Task<BaseResponse> GetAllArchivizedTripsOfLoggedInUser()
        {
            return await GetAllArchivizedTrips(true);
        }

        private async Task<BaseResponse> GetAllArchivizedTrips(bool archivized)
        {
            ObservableCollection<TripSmallDetaisDto> usersTrips = new();
            try
            {
                var response = await _tripsRepository.GetAllAsync();
                IEnumerable<TripModel> trips = FilterTRips(archivized, response);
                foreach (var trip in trips)
                {
                    List<string> placesNames = new();
                    foreach (var place in trip.Places)
                    {
                        var placeDetailsResponse = await _placesService.GetPlaceDetails(place, FieldTypes.Name) as PlaceDetailsDto;
                        if (placeDetailsResponse == null) continue;
                        placesNames.Add(placeDetailsResponse.PlacesDetailsResponse.Name);
                    }
                    TripSmallDetaisDto tripDto = new()
                    {
                        TripModel = trip,
                        PlaceNames = placesNames
                    };

                    usersTrips.Add(tripDto);
                }


                return new CollectionResponse<TripSmallDetaisDto>() { Collection = usersTrips };
            }
            catch (Exception ex)
            {
                return new DefaultErrorResponse() { Message = ex.Message };
            }
        }

        private static IEnumerable<TripModel> FilterTRips(bool archivized, IReadOnlyCollection<FirebaseObject<TripModel>> response)
        {
            return response.Select(e => e.Object)
                .Where(e => e.IsArchivized == archivized);
        }

        public async Task DeleteAllTrips()
        {
            await _tripsRepository.DeleteAllAsync();
        }

        
        public async Task<BaseResponse> GetTripDetails(string tripId)
        {
            TripModel selectedTrip = null;
            try
            {
                selectedTrip = await _tripsRepository.GetByIdAsync(tripId);
            }
            catch (FirebaseException ex)
            {
                return new DefaultErrorResponse() { Message = ex.Message };
            }


            if (selectedTrip == null)
                return new DefaultErrorResponse() { Message = "Trip not found in database" };

            List<PlaceDetailsDto> placesList = new();
            foreach (var placeId in selectedTrip.Places)
            {
                try
                {
                    var placeDto = await _placesService.GetSmallPlaceDetails(
                        placeId,
                        3,
                        100,
                        FieldTypes.Name | FieldTypes.Opening_Hours | FieldTypes.Photo | FieldTypes.Place_Id | FieldTypes.Formatted_Address | FieldTypes.Rating | FieldTypes.User_Ratings_Total | FieldTypes.Price_Level | FieldTypes.Geometry | FieldTypes.Type)
                        as PlaceDetailsDto;
                    if (placeDto != null)
                        placesList.Add(placeDto);
                }
                catch (Exception) { continue; }
            }

            TripDetailsDto tripDto = new()
            {
                TripModel = selectedTrip,
                
            };
            foreach (var place in placesList)
            {
                tripDto.PlaceDetailsList.Add(place);
            }

            return tripDto;
        }

        public async Task<BaseResponse> AddPlaceToTripAsync(string tripId, string placeId)
        {
            try
            {
                List<string> placesIds = await _tripsRepository.GetPlacesIdsFromTrip(tripId);
                if (placesIds.Contains(placeId))
                {
                    return new DefaultErrorResponse()
                    {
                        Message = "Can't add that place to the chosen trip.\nPlace already exists in the trip."
                    };
                }
                placesIds.Add(placeId);
                await _tripsRepository.AddPlacesToTrip(tripId, placesIds);
                return new OkResponse();
            }
            catch(FirebaseException ex)
            {
                return new DefaultErrorResponse() { Message = ex.Message};
            }
        }

        public async Task<BaseResponse> DeletePlaceFromTrip(string tripId, string placeId) 
        {
            try
            {
                List<string> placesIds = await _tripsRepository.GetPlacesIdsFromTrip(tripId);
                placesIds.Remove(placeId);
                await _tripsRepository.AddPlacesToTrip(tripId, placesIds);
                return new OkResponse() { Message = $"Succsessfuly delete place: {placeId} from trip: {tripId}" };
            }
            catch(FirebaseException ex)
            {
                return new DefaultErrorResponse() { Message = ex.Message };
            }
        }


        public async Task<BaseResponse> GetByIdAsync(string tripId)
        {
            try
            {
                var tripModel = await _tripsRepository.GetByIdAsync(tripId);
                return new OkResponse() { Payload = tripModel };
            }
            catch (Exception ex)
            {
                return new DefaultErrorResponse() { Message = ex.Message };
            }
        }

        public async Task<BaseResponse> DeleteTripById(string id)
        {
            try
            {
                await _tripsRepository.DeleteAsync(id);
                return new OkResponse() { Message = $"Succsessfuly deleted trip: {id}" };
            }
            catch (FirebaseException ex)
            {
                return new DefaultErrorResponse() { Message = ex.Message };
            }
        }

        public async Task<BaseResponse> GetPlacesIdsFromTrip(string tripId)
        {
            try
            {
                var placesIds = await _tripsRepository.GetPlacesIdsFromTrip(tripId);
                return new OkResponse() { Payload = placesIds };
            }
            catch (Exception ex)
            {
                return new DefaultErrorResponse() { Message = ex.Message };
            }
        }

        public async Task<BaseResponse> PatchTripAsync(string tripId, string property, object value)
        {
            try
            {
                await _tripsRepository.PatchAsync(tripId,property, value);
                return new OkResponse();
            }
            catch (Exception ex)
            {
                return new DefaultErrorResponse() { Message = ex.Message };
            }
        }
    }
}
