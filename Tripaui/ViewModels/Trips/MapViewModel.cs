using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using CommunityToolkit.Mvvm.Input;
using Services;
using Tripaui.Extensions;
using Shared.Responses;
using GoogleApi.Entities.Maps.Directions.Response;
using GoogleApi.Entities.Places.Details.Response;
using Java.Time.Temporal;
using Shared.Entities;
using Tripaui.Services;

namespace Tripaui.ViewModels.Trips
{
    [QueryProperty(nameof(Trip), nameof(Trip))]
    public sealed partial class MapViewModel : BaseViewModel
    {
        private readonly DirectionsService _directionsService;
        private readonly TripsService _tripsService;
        private readonly PlacesService _placesService;
        private readonly GeolocationService _geolocationService;

        public Dictionary<string, Polyline> Polylines { get; set; } = new();
        public Dictionary<string, Pin> Pins { get; set; } = new();

        public Pin CurrentLocationPin = new();

        public Dictionary<string, DetailsResult> PlacesDetailsForPins { get; set; } = new();

        [ObservableProperty]
        private string tripId = "";

        [ObservableProperty]
        private TripModel trip;

        public MapViewModel(DirectionsService directionsService, TripsService tripsService, PlacesService placesService, GeolocationService geolocationService)
        {
            _directionsService = directionsService;
            _tripsService = tripsService;
            _placesService = placesService;
            _geolocationService = geolocationService;

            
        }

        public async Task PageAppearing()
        {
            if (Trip is null)
            { return; }
            var response = await _directionsService.GetTripDirectionReponse(Trip);
            if (await response.ValidateResponse() > 0)
            { return; }
            var directionsResponse = (DirectionsResponse)((OkResponse)response).Payload;
            var route = directionsResponse.Routes.FirstOrDefault();
            var waypoints = directionsResponse.WayPoints.ToList();
            if (route == null)
            { return; }
            var placesDetailsResponse = await _placesService.GetPlaceDataForMap(trip.Places);
            if (await placesDetailsResponse.ValidateResponse() > 0) return;
            PlacesDetailsForPins = ((OkResponse)placesDetailsResponse).Payload as Dictionary<string, DetailsResult>;

            var dictionaryOfPolylineCoordinates = _directionsService.GetDictionaryOfPolylineCoordinates(route, waypoints);
            TransformIntoPolylinesAndPinsDictionary(dictionaryOfPolylineCoordinates);

            Location location = await _geolocationService.GetCurrentLocation();
            CurrentLocationPin = new Pin()
            {
                Label = "YOU ARE HERE !",
                Location = location
            };
        }

        [RelayCommand]
        private async Task GoBack()
        {
            await Shell.Current.GoToAsync($"..");
        }

        private void TransformIntoPolylinesAndPinsDictionary(Dictionary<string, List<GoogleApi.Entities.Common.Coordinate>> polylinesDictionary)
        {
            var random = new Random();
            int i = 1;
            foreach (var coordinateList in polylinesDictionary)
            {
                TryAddNewPinToPinsDictionary(coordinateList);
                if (IsLastPolyline(polylinesDictionary, i))
                {
                    break;
                }
                TryAddNewPolylineToDictionary(random, coordinateList);
                i++;
            }
        }

        private void TryAddNewPolylineToDictionary(Random random, KeyValuePair<string, List<GoogleApi.Entities.Common.Coordinate>> coordinateList)
        {
            var polyline = new Polyline()
            {
                StrokeColor = GetRandomColor(random),
                StrokeWidth = 8
            };
            foreach (var coordinate in coordinateList.Value)
            {
                polyline.Geopath.Add(GetLocationFromCoordinate(coordinate));
            }
            Polylines.TryAdd(coordinateList.Key, polyline);
        }

        private void TryAddNewPinToPinsDictionary(KeyValuePair<string, List<GoogleApi.Entities.Common.Coordinate>> coordinateList)
        {
            if (PlacesDetailsForPins.Count == 0)
                return;
            PlacesDetailsForPins.TryGetValue(coordinateList.Key, out var placeDetail);
            var pin = new Pin()
            {
                Label = placeDetail.Name,
                Address = placeDetail.FormattedAddress,
                Location = GetLocationFromCoordinate(coordinateList.Value.FirstOrDefault())
            };
            Pins.TryAdd(coordinateList.Key, pin);
        }

        private static bool IsLastPolyline(Dictionary<string, List<GoogleApi.Entities.Common.Coordinate>> polylinesDictionary, int i)
        {
            return i == polylinesDictionary.Count;
        }

        private static Location GetLocationFromCoordinate(GoogleApi.Entities.Common.Coordinate coordinate)
        {
            return new Location(coordinate.Latitude, coordinate.Longitude);
        }

        private static Color GetRandomColor(Random random)
        {
            return new Color((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
        }
    }
}
