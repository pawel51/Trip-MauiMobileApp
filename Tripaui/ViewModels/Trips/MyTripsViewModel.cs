using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevExpress.Data.Native;
using Services;
using Shared.Common;
using Shared.Entities;
using Shared.Responses;
using System.Collections.ObjectModel;
using Tripaui.Extensions;
using Tripaui.Views.Trips;

namespace Tripaui.ViewModels.Trips
{
    
    public sealed partial class MyTripsViewModel : BaseViewModel
    {
        private readonly TripsService _tripsService;
        private readonly DirectionsService _directionsService;
        [ObservableProperty]
        ObservableCollection<TripSmallDetaisDto> trips;

        [ObservableProperty]
        private bool isTripCollectionRefreshing = false;


        public MyTripsViewModel(TripsService tripsService, DirectionsService directionsService)
        {
            _tripsService = tripsService;
            _directionsService = directionsService;
        }

        [RelayCommand]
        private async Task PageAppearing()
        {
            IsBusy = true;
            var response = await _tripsService.GetAllTripsOfLoggedInUser();
            if (await response.ValidateResponse() > 0)
            {
                IsBusy = false;
                return;
            }
            Trips = ((CollectionResponse<TripSmallDetaisDto>)response).Collection;
            IsBusy = false;
        }

        [RelayCommand]
        private async Task GoToAddTripPage()
        {
            await Shell.Current.GoToAsync(nameof(AddTripPage), new Dictionary<string, object>
            {
                { "PlaceId", ""}
            });
        }

        [RelayCommand]
        private async Task GoToDetails(TripSmallDetaisDto tripDto)
        {
            await Shell.Current.GoToAsync($"{nameof(TripDetailsPage)}", new Dictionary<string, object>()
            {
                { "TripId", tripDto.TripModel.Id.ToString() },
                { "PlaceId", "" },
                { "CanModify", true }
            });
        }

        [RelayCommand]
        async Task DeleteTrip(TripSmallDetaisDto tripDto)
        {
            var confirmDelete = await Shell.Current.DisplayAlert("Warning",
                $"Do you want to delete trip: {tripDto.TripModel.Name}?",
                "Yes", "No");
            if (!confirmDelete)
                return;

            IsTripCollectionRefreshing = true;

            var deletePlanResponse = 
                await _directionsService.DeleteItemOfTrip(tripDto.TripModel.Id.ToString());
            await deletePlanResponse.ValidateResponse();

            var response = await _tripsService.DeleteTripById(tripDto.TripModel.Id.ToString());
            
            if (await response.ValidateResponse() > 0)
            {
                IsTripCollectionRefreshing = false;
                return;
            }
            Trips.Remove(tripDto);
            IsTripCollectionRefreshing = false;
        }

        [RelayCommand]
        async Task AddToArchive(TripSmallDetaisDto tripDto)
        {
            var response = await _tripsService.PatchTripAsync(
                tripDto.TripModel.Id.ToString(),
                "IsArchivized",
                true
                );
            if (await response.ValidateResponse() > 0)
                return;
            IsTripCollectionRefreshing = true;
            Trips.Remove(tripDto);
            IsTripCollectionRefreshing = false;
        }

    }
}
