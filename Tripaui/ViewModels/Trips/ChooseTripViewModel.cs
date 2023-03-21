using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Services;
using Shared.Common;
using Shared.Responses;
using System.Collections.ObjectModel;
using Tripaui.Extensions;
using Tripaui.Views.Trips;

namespace Tripaui.ViewModels.Trips
{
    [QueryProperty(nameof(PlaceId), nameof(PlaceId))]
    [QueryProperty(nameof(Place), nameof(Place))]
    public sealed partial class ChooseTripViewModel : BaseViewModel
    {
        private readonly TripsService _tripsService;

        [ObservableProperty]
        ObservableCollection<TripSmallDetaisDto> trips;

        [ObservableProperty]
        private string placeId = "";

        [ObservableProperty]
        private PlaceDetailsDto place;

        public ChooseTripViewModel(TripsService tripsService)
        {
            _tripsService = tripsService;
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
                { "PlaceId", PlaceId ?? ""},
                { "Place", Place }
            });
        }

        [RelayCommand]
        private async Task GoToDetails(TripSmallDetaisDto tripDto)
        {
            await Shell.Current.GoToAsync($"{nameof(TripDetailsPage)}", new Dictionary<string, object>()
            {
                { "TripId", tripDto.TripModel.Id.ToString() },
                { "PlaceId", PlaceId ?? ""},
                { "Place", Place }
            });
        }

    }
}
