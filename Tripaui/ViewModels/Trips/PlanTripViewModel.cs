using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GoogleApi.Entities.Maps.Directions.Response;
using Services;
using Shared.Common;
using Shared.Responses;
using System.Diagnostics;
using Tripaui.Extensions;
using Tripaui.Views.Controlls;

namespace Tripaui.ViewModels.Trips
{
    [QueryProperty(nameof(Trip), nameof(Trip))]
    [QueryProperty(nameof(TripId), nameof(TripId))]
    public sealed partial class PlanTripViewModel : BaseViewModel
    {
        private readonly TripsService _tripsService;
        private readonly DirectionsService _directionsService;

        [ObservableProperty]
        private TripDetailsDto trip;

        [ObservableProperty]
        private string tripId;

        [ObservableProperty]
        private TripPlan plan = new();

        public PlanTripViewModel(
            TripsService tripsService, 
            DirectionsService directionsService)
        {
            _tripsService = tripsService;
            _directionsService = directionsService;
        }

        [RelayCommand]
        private void Test()
        {
            Debug.WriteLine(Trip.TripModel.Name);
        }

        [RelayCommand]
        private async Task PageAppearing()
        {
            IsBusy = true;
            var tripId = TripId;
            if (String.IsNullOrEmpty(tripId))
            { 
                IsBusy = true; 
                return; 
            }
            var tripPlan = await TryGetPlan(tripId);
            // jak plan nie istnieje to utworz nowy plan
            if (tripPlan == null)
            {
                await UpdatePlan(null);
                IsBusy = false;
                return;
            } 
            
            var updateExistingPlan = ShouldUpdateExistingPlan(tripPlan);
            if (updateExistingPlan)
                await UpdatePlan(tripPlan);
            else if (tripPlan != null)
                Plan = tripPlan;

            IsBusy = false;
        }

        private bool ShouldUpdateExistingPlan(TripPlan tripPlan)
        {
            return TimeChanged(tripPlan) || PlaceOrderChanged(tripPlan) || TravelModeChanged(tripPlan);
        }

        private bool TimeChanged(TripPlan tripPlan)
        {
            if (tripPlan.StartDate != Trip.TripModel.StartDate ||
                tripPlan.StartTime != Trip.TripModel.StartTime)
                return true;
            else
                return false;
        }

        private bool PlaceOrderChanged(TripPlan tripPlan)
        {
            var planPlacesIds = tripPlan.SinglePlacePlans.Select(e => e.PlaceId).ToList();
            planPlacesIds.Add(tripPlan.Destination.PlaceId);
            if (Trip.TripModel.Places.Count != planPlacesIds.Count)
                return true;
            var tripAndPlanIds = Trip.TripModel.Places.Zip(planPlacesIds, (tp, pp) => new { tripPlaceId = tp, planPlaceId = pp });
            foreach (var tp in tripAndPlanIds)
            {
                if (tp.tripPlaceId != tp.planPlaceId)
                    return true;
            }
            return false;
        }

        private bool TravelModeChanged(TripPlan tripPlan)
        {
            if (tripPlan.TravelModeEnum != Trip.TripModel.TravelModeEnum)
                return true;
            if (tripPlan.TransitModeEnum != Trip.TripModel.TransitModeEnum)
                return true;
            return false;
        }

        

        private async Task<TripPlan> TryGetPlan(string tripId)
        {
            var plan = await _directionsService.GetItemOfTrip(tripId) as OkResponse;
            if (plan != null)
                return (TripPlan)(plan.Payload);
            else
                return null;
        }

        private async Task UpdatePlan(TripPlan plan)
        {
            var response = await _directionsService.GetTripDirectionReponse(Trip.TripModel);
            if (await response.ValidateResponse() > 0)
            {
                IsBusy = true;
                return;
            }
            var directionsResponse = (DirectionsResponse)((OkResponse)response).Payload;
            var route = directionsResponse.Routes.FirstOrDefault();
            var waypoints = directionsResponse.WayPoints.ToList();
            Plan = _directionsService
                .GenerateBasicTripPlan(route, waypoints, Trip, plan);
            var savePlanResponse = await _directionsService.SaveItemForTripAsync(TripId, Plan);
            await savePlanResponse.ValidateResponse();
        }

        [RelayCommand]
        private async Task PlayVoiceHint(VoiceHintItem voiceHint)
        {
            SpeechOptions so = new()
            {
                Volume = 1
            };
            await TextToSpeech.Default.SpeakAsync(voiceHint.TextHint);
        }


        [RelayCommand]
        private async Task GoBack()
        {
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private void IncrementTime(PlacePlan singlePlacePlan)
        {
            singlePlacePlan.TimeToSpendHere += TimeSpan.FromMinutes(5);
        }

        [RelayCommand]
        private void Decrement(PlacePlan singlePlacePlan)
        {
            var minute5 = TimeSpan.FromMinutes(5);
            if (singlePlacePlan.TimeToSpendHere < minute5)
                return;
            singlePlacePlan.TimeToSpendHere -= minute5;
        }

        [RelayCommand]
        private async Task SavePlan()
        {
            //TODO wygenerować i zapisać nowe z directions xd//
            IsBusy = true;
            var updateExistingPlan = ShouldUpdateExistingPlan(Plan);
            if (updateExistingPlan)
            {
                await UpdatePlan(Plan);
                await Shell.Current.ShowPopupAsync(new SuccessPopup("Plan updated Successfully."));
            }
            IsBusy = false;
           
        }
    }


}
