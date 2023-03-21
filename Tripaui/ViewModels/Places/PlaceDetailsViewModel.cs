using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Services;
using Shared.Responses;
using Tripaui.Extensions;
using Tripaui.Views.Controlls;
using Tripaui.Views.Places;
using Tripaui.Views.Trips;

namespace Tripaui.ViewModels.Places
{
    [QueryProperty(nameof(PlaceId), nameof(PlaceId))]
    public sealed partial class PlaceDetailsViewModel : BaseViewModel
    {
        private readonly PlacesService _placesService;
        private readonly TripsService _tripsService;

        public PlaceDetailsViewModel(PlacesService placesService, TripsService tripsService)
        {
            _placesService = placesService;
            _tripsService = tripsService;
        }

        #region Properties
        [ObservableProperty]
        private PlaceDetailsDto place;

        [ObservableProperty]
        private string placeId;

        #endregion

        #region Commands

        [RelayCommand]
        private async Task PageAppearing()
        {
            IsBusy = true;
            var response = await _placesService.GetPlaceDetails(PlaceId);
            IsBusy = false;
            if (await response.ValidateResponse() > 0)
            {
                await Shell.Current.GoToAsync($"//{nameof(PlacesListPage)}");
                return;
            }
            Place = (PlaceDetailsDto)response;

        }

        [RelayCommand]
        async Task NavigateToBuilding()
        {
            var placemark = new Placemark
            {
                Location = new Location(Place.PlacesDetailsResponse.Geometry.Location.Latitude, Place.PlacesDetailsResponse.Geometry.Location.Longitude),   
            };
            var options = new MapLaunchOptions { Name = Place.PlacesDetailsResponse.Name };

            try
            {
                await Map.Default.OpenAsync(placemark, options);
            }
            catch (Exception)
            {
                // No map application available to open or placemark can not be located
            }
        }

        [RelayCommand]
        async Task GoBack()
        {
            await Shell.Current.GoToAsync($"..", new Dictionary<string, object>()
            {
                { "PlaceId", "" }
            });
        }

        [RelayCommand]
        async Task AddPlaceToTrip()
        {
            await Shell.Current.GoToAsync($"{nameof(ChooseTripPage)}", new Dictionary<string, object>()
            {
                { "PlaceId", PlaceId },
                { "Place", Place }
            });
        }

        [RelayCommand]
        async Task OpenBrowserCommand(string uriParam)
        {
            if (String.IsNullOrEmpty(uriParam))
            {
                await Shell.Current.ShowPopupAsync(new ValidationErrorPopup("Profile unavailable!"));
            }
            try
            {
                Uri uri = new Uri(uriParam);
                await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                await Shell.Current.ShowPopupAsync(new ValidationErrorPopup(ex.Message));
            }
        }
        #endregion
    }
}
