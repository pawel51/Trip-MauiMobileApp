using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Services;
using Shared.Entities;
using Shared.Responses;
using Tripaui.Extensions;
using Tripaui.Forms;
using Tripaui.Views.Controlls;

namespace Tripaui.ViewModels.Trips
{
    [QueryProperty(nameof(TripId), nameof(TripId))]
    public sealed partial class EditTripViewModel : BaseViewModel
    {
        public TripForm Form { get; } = new();
        private TripModel _editedTrip { get; set; } 

        private readonly TripsService _tripService;

        [ObservableProperty]
        private string tripId;

        public EditTripViewModel(TripsService tripService)
        {
            _tripService = tripService;
        }

        [RelayCommand]
        private async Task PageAppearing()
        {
            IsBusy = true;
            var response = await _tripService.GetByIdAsync(TripId);
            if (await response.ValidateResponse() > 0)
            {
                IsBusy = false;
                return;
            }
            _editedTrip = ((OkResponse)response).Payload as TripModel;
            if (_editedTrip == null)
            {
                IsBusy = false;
                await Shell.Current.ShowPopupAsync(new ValidationErrorPopup("Trip was not found"));
                return;
            }
            Form.StartDate = _editedTrip.StartDate;
            Form.StartTime = _editedTrip.StartTime;
            Form.EndDate = _editedTrip.EndDate;
            Form.TravelMode = _editedTrip.TravelModeEnum;
            Form.TransitMode = _editedTrip.TransitModeEnum;
            Form.Name = _editedTrip.Name;
            IsBusy = false;
        }

        [RelayCommand]
        private async Task UpdateTrip()
        {
            IsBusy = true;
            if (await Form.ValidateForm() +
                await Form.CheckIfStartDateIsSmallerThenEndDate() > 0)
            {
                IsBusy = false;
                return;
            }
            _editedTrip.StartDate = Form.StartDate;
            _editedTrip.EndDate = Form.EndDate;
            _editedTrip.Name = Form.Name;
            _editedTrip.StartTime = Form.StartTime;
            var response = await _tripService.AddTrip(_editedTrip);
            if (await response.ValidateResponse() > 0)
            {
                IsBusy = false;
                return;
            }
            await Shell.Current.GoToAsync("..", new Dictionary<string, object>()
            {
                { "PlaceId", "" }
            });
        }

        [RelayCommand]
        private async Task GoBack()
        {
            await Shell.Current.GoToAsync("..", new Dictionary<string, object>()
            {
                { "PlaceId", "" }
            });
        }

    }
}
