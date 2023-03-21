using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Services;
using Shared.Common;
using Shared.Responses;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tripaui.Extensions;
using Tripaui.Views.Trips;

namespace Tripaui.ViewModels.Trips
{
    public partial class ArchiveTripsViewModel : BaseViewModel
    {
        private readonly TripsService _tripsService;
        private readonly DirectionsService _directionsService;
        [ObservableProperty]
        ObservableCollection<TripSmallDetaisDto> trips;

        [ObservableProperty]
        private bool isTripCollectionRefreshing = false;

        public ArchiveTripsViewModel(TripsService tripsService, DirectionsService directionsService)
        {
            _tripsService = tripsService;
            _directionsService = directionsService;
        }


        [RelayCommand]
        private async Task PageAppearing()
        {
            IsBusy = true;
            var response = await _tripsService.GetAllArchivizedTripsOfLoggedInUser();
            if (await response.ValidateResponse() > 0)
            {
                IsBusy = false;
                return;
            }
            Trips = ((CollectionResponse<TripSmallDetaisDto>)response).Collection;
            IsBusy = false;
        }

        [RelayCommand]
        private async Task GoToDetails(TripSmallDetaisDto tripDto)
        {
            await Shell.Current.GoToAsync($"{nameof(TripDetailsPage)}", new Dictionary<string, object>()
            {
                { "TripId", tripDto.TripModel.Id.ToString() },
                { "PlaceId", ""},
                { "CanModify", false }
            });
        }

        [RelayCommand]
        async Task RemoveFromArchive(TripSmallDetaisDto tripDto)
        {
            var response = await _tripsService.PatchTripAsync(
                tripDto.TripModel.Id.ToString(),
                "IsArchivized",
                false
                );
            if (await response.ValidateResponse() > 0)
                return;
            IsTripCollectionRefreshing = true;
            Trips.Remove(tripDto);
            IsTripCollectionRefreshing = false;
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
    }
}
