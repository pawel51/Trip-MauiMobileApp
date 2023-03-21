using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Services;
using Shared.Entities;
using Shared.Responses;
using Tripaui.Extensions;
using Tripaui.Forms;
using Tripaui.Views.Trips;

namespace Tripaui.ViewModels.Trips
{
    [QueryProperty(nameof(PlaceId), nameof(PlaceId))]
    [QueryProperty(nameof(Place), nameof(Place))]
    public sealed partial class AddTripViewModel : BaseViewModel
    {
        private readonly TripsService _tripService;

        [ObservableProperty]
        private string placeId = "";

        [ObservableProperty]
        private PlaceDetailsDto place;

        public AddTripViewModel(TripsService tripService)
        {
            _tripService = tripService;
        }
        public TripForm Form { get; } = new();

        public TripDataProvider ComboboxDataProvider { get; set; } = new();

        [RelayCommand]
        private async Task AddTrip()
        {
            IsBusy = true;
            if (await Form.ValidateForm() +
                await Form.CheckIfStartDateIsSmallerThenEndDate() > 0)
            {
                IsBusy = false;
                return;
            }
            TripModel newTrip = new()
            {
                StartTime = Form.StartTime,
                StartDate = Form.StartDate,
                EndDate = Form.EndDate,
                Name = Form.Name,
                TravelModeEnum = Form.TravelMode,
                TransitModeEnum = Form.TransitMode
            };
            var response = await _tripService.AddTrip(newTrip);
            if (await response.ValidateResponse() > 0)
            {
                IsBusy = false;
                return;
            }
            await Shell.Current.GoToAsync($"{nameof(TripDetailsPage)}", new Dictionary<string, object>()
            {
                { "TripId", newTrip.Id.ToString() },
                { "PlaceId", PlaceId ?? "" },
                { "Place", Place ?? new PlaceDetailsDto() }
            });
        }

        [RelayCommand]
        private async Task GoBack()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
