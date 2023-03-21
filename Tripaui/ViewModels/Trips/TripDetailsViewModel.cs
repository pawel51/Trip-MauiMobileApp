using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GoogleApi.Entities.Common;
using GoogleApi.Entities.Common.Enums;
using GoogleApi.Entities.Maps.Common.Enums;
using GoogleApi.Entities.Maps.Directions.Response;
using GoogleApi.Entities.Places.Search.Common.Enums;
using Services;
using Shared.Common;
using Shared.Entities;
using Shared.Responses;
using System.Collections.ObjectModel;
using System.Reactive.Joins;
using Tripaui.Extensions;
using Tripaui.Views.Controlls;
using Tripaui.Views.Places;
using Tripaui.Views.Trips;

namespace Tripaui.ViewModels.Trips
{
    [QueryProperty(nameof(TripId), nameof(TripId))]
    [QueryProperty(nameof(PlaceId), nameof(PlaceId))]
    [QueryProperty(nameof(Place), nameof(Place))]
    [QueryProperty(nameof(CanModify), nameof(CanModify))]
    public sealed partial class TripDetailsViewModel : BaseViewModel
    {
        public List<TravelMode> TravelModes { get; init; } = Enum.GetValues(typeof(TravelMode)).Cast<TravelMode>().ToList();
        public List<TransitMode> TransitModes { get; init; } = Enum.GetValues(typeof(TransitMode)).Cast<TransitMode>().ToList();

        private readonly TripsService _tripsService;
        private readonly PlacesService _placesService;
        private readonly RecommendationService _recommendationService;
        private readonly DirectionsService _directionsService;
        [ObservableProperty]
        private TripDetailsDto trip;

        [ObservableProperty]
        private string tripId;

        [ObservableProperty]
        private string placeId;

        [ObservableProperty]
        private PlaceDetailsDto place;

        [ObservableProperty]
        private TravelMode selectedTravelMode = TravelMode.Walking;

        [ObservableProperty]
        private bool canModify = true;

        partial void OnSelectedTravelModeChanged(TravelMode value)
        {
            Task.Run(() => _tripsService.PatchTripAsync(tripId, "TravelModeEnum", value));
            if (value == TravelMode.Transit)
                IsTransitVisible = true;
            else
                IsTransitVisible = false;
        }

        [ObservableProperty]
        private TransitMode selectedTransitMode = TransitMode.Bus;
        partial void OnSelectedTransitModeChanged(TransitMode value)
        {
            Task.Run(() => _tripsService.PatchTripAsync(tripId, "TransitMode", value));
        }

        [ObservableProperty]
        private bool isTransitVisible = false;

        public TripDetailsViewModel(
            TripsService tripsService, 
            PlacesService placesService, 
            RecommendationService recommendationService,
            DirectionsService directionsService)
        {
            _tripsService = tripsService;
            _placesService = placesService;
            _recommendationService = recommendationService;
            _directionsService = directionsService;
        }

        [RelayCommand]
        private async Task GetRecommendedOrder()
        {
            if (await CheckIfTwoPlacesWereAdded())
                return;
            var response = await _directionsService.GetTripDirectionReponse(Trip.TripModel, optimizeWaypoints: true);
            if (await response.ValidateResponse() > 0)
            {
                return;
            }
            var directionsResponse = (DirectionsResponse)((OkResponse)response).Payload;
            await ReorderPlacesOnTrip(directionsResponse);
        }

        private async Task ReorderPlacesOnTrip(DirectionsResponse directionsResponse)
        {
            List<PlaceDetailsDto> newPlaceOrder = new();
            foreach (var responseWaypoint in directionsResponse.WayPoints)
            {
                var oldPlace = Trip.PlaceDetailsList
                    .FirstOrDefault(p => p.PlacesDetailsResponse.PlaceId.Equals(responseWaypoint.PlaceId));
                if (oldPlace is null)
                {
                    await Shell.Current.DisplayAlert("Error", "Response PlaceIds don't match request placeIds", "OK");
                    return;
                }
                newPlaceOrder.Add(oldPlace);
            }
            Trip.PlaceDetailsList.Clear();
            foreach (var p in newPlaceOrder)
            {
                Trip.PlaceDetailsList.Add(p);
            }
        }

        private async Task UpdatePlan(TripPlan plan)
        {
            var response = await _directionsService.GetTripDirectionReponse(Trip.TripModel, optimizeWaypoints: true);
            if (await response.ValidateResponse() > 0)
            {
                IsBusy = true;
                return;
            }
            var directionsResponse = (DirectionsResponse)((OkResponse)response).Payload;
            var route = directionsResponse.Routes.FirstOrDefault();
            var waypoints = directionsResponse.WayPoints.ToList();
            TripPlan tripPlan = _directionsService
                .GenerateBasicTripPlan(route, waypoints, Trip, plan);
            var savePlanResponse = await _directionsService.SaveItemForTripAsync(TripId, tripPlan);
            await savePlanResponse.ValidateResponse();
        }

        private async Task<TripPlan> TryGetPlan(string tripId)
        {
            var plan = await _directionsService.GetItemOfTrip(tripId) as OkResponse;
            if (plan != null)
                return (TripPlan)(plan.Payload);
            else
                return null;
        }

        [RelayCommand]
        private async Task PageAppearing()
        {
            IsBusy = true;
            if (!String.IsNullOrEmpty(PlaceId))
            {
                var addTripResponse = await _tripsService.AddPlaceToTripAsync(TripId, PlaceId);
                await _recommendationService.PlaceAddedToTrip(Place.PlacesDetailsResponse);
                PlaceId = "Added";
            }
            var response = await _tripsService.GetTripDetails(TripId);
            if (await response.ValidateResponse() > 0)
            {
                await Shell.Current.GoToAsync($"//{nameof(MyTripsPage)}");
            }
            else
            {
                Trip = response as TripDetailsDto;
                SelectedTransitMode = Trip.TripModel.TransitModeEnum;
                SelectedTravelMode = Trip.TripModel.TravelModeEnum;
            }
                
            IsBusy = false;
        }

        [RelayCommand]
        private async Task NavigatedTo()
        {
            if (PlaceId == "Added")
            {
                await Shell.Current.ShowPopupAsync(new SuccessPopup("Successfuly added new place to the trip."));
            }
        }

        [RelayCommand]
        private async Task GoBack()
        {
            if (PlaceId == "Added")
            {
                await Shell.Current.GoToAsync($"//{nameof(PlacesListPage)}");
            }
            else if (CanModify == true)
            {
                await Shell.Current.GoToAsync($"//{nameof(MyTripsPage)}");
            }
            else if (CanModify == false)
            {
                await Shell.Current.GoToAsync($"//{nameof(ArchiveTripsPage)}");
            }
            PlaceId = "";
        }

        [RelayCommand]
        private async Task GoToDetails(PlaceDetailsDto placeDto)
        {
            await Shell.Current.GoToAsync($"{nameof(PlaceDetailsPage)}", new Dictionary<string, object>()
            {
                { "PlaceId", placeDto.PlacesDetailsResponse.PlaceId }
            });
        }

        [RelayCommand]
        private async Task EditTrip()
        {
            await Shell.Current.GoToAsync($"{nameof(EditTripPage)}", new Dictionary<string, object>()
            {
                { "TripId", TripId }
            });
        }

        [RelayCommand]
        private async Task GoToSearchPlace()
        {
            double radius = GetSearchRadiusFromTravelMode();
            var placeList = Trip.PlaceDetailsList.ToList();
            Coordinate baseCoordinate = _recommendationService.GetMeanCoordinateForTrip(placeList);
            PlaceLocationType category = _recommendationService.GetTopCategoryForTrip(placeList).FirstOrDefault();

            await Shell.Current.GoToAsync($"//{nameof(PlacesListPage)}", new Dictionary<string, object>
            {
                { "Coordinate", baseCoordinate },
                { "Radius", radius },
                { "Category", category },
                { "ShowRecommended", true }
            });
        }

        [RelayCommand]
        private async Task GoToReviews()
        {
            if (Trip.PlaceDetailsList.Count == 0)
            {
                await Shell.Current.DisplayAlert("No places", "Add some place first", "OK");
                return;
            }
            var placesNames = Trip.PlaceDetailsList.Select(e => new PlaceName()
            {
                Name = e.PlacesDetailsResponse.Name,
                Id = e.PlacesDetailsResponse.PlaceId
            }).ToList();
            await Shell.Current.GoToAsync($"{nameof(ReviewsPage)}", new Dictionary<string, object>()
            {
                { "Trip", Trip },
                { "TripId", TripId },
                { "PlacesNames", placesNames }
            });
        }

        [RelayCommand]
        private async Task DeletePlaceFromTrip(PlaceDetailsDto placeDto)
        {
            var confirmDelete = await Shell.Current.DisplayAlert("Warning", 
                $"Do you want to delete {placeDto.PlacesDetailsResponse.Name} from current trip?",
                "Yes", "No");
            if (!confirmDelete)
                return;
            var response = await _tripsService.DeletePlaceFromTrip(
                TripId,
                placeDto.PlacesDetailsResponse.PlaceId);
            if (await response.ValidateResponse() > 0)
            {
                return;
            }
            Trip.PlaceDetailsList.Remove(placeDto);
        }

        [RelayCommand]
        private async Task GoToMapPage()
        {
            if (await CheckIfTwoPlacesWereAdded())
                return;
            await Shell.Current.GoToAsync($"{nameof(MapPage)}",
                new Dictionary<string, object>()
                {
                    { "Trip", Trip.TripModel },
                });
        }

        private async Task<bool> CheckIfTwoPlacesWereAdded()
        {
            if (Trip.TripModel.Places.Count < 2)
            {
                await Shell.Current.DisplayAlert("Not enough places added", "Add more places, by clicking green plus button", "OK");
                return true;
            }
            return false;
        }

        [RelayCommand]
        private async Task UpdateTrip()
        {
            Trip.TripModel.Places = Trip.PlaceDetailsList
                .Select(e => e.PlacesDetailsResponse.PlaceId)
                .ToList();



            var response = await _tripsService.AddTrip(Trip.TripModel);
            if (await response.ValidateResponse() > 0)
            {
                var oldTripResponse = await _tripsService.GetTripDetails(TripId);
                if (await response.ValidateResponse() > 0)
                {
                    return;
                }
                Trip = oldTripResponse as TripDetailsDto;
            }
        }

        [RelayCommand]
        private async Task GoToTripPlan()
        {
            if (await CheckIfTwoPlacesWereAdded())
                return;
            if (StartDateIsInPast())
            {
                await Shell.Current.ShowPopupAsync(new ValidationErrorPopup("Start date is in the past"));
                return;
            }

            await Shell.Current.GoToAsync(nameof(PlanTripPage), new Dictionary<string, object>()
            {
                { "Trip",  Trip },
                { "TripId", Trip.TripModel.Id.ToString() }
            });
        }


        private double GetSearchRadiusFromTravelMode()
        {
            double radius = 50000;
            switch (SelectedTravelMode)
            {
                case TravelMode.Bicycling:
                    radius = 20000;
                    break;
                case TravelMode.Driving:
                    radius = 100000;
                    break;
                case TravelMode.Transit:
                    radius = 50000;
                    break;
                case TravelMode.Walking:
                    radius = 10000;
                    break;
            }
            return radius;
        }

        private bool StartDateIsInPast()
        {
            var date = DateTime.Now;
            if (Trip.TripModel.StartDate < date)
            {
                return true;
            }
            return false;
        }
    }
}