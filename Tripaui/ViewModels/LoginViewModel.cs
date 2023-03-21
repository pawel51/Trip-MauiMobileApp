using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Services;
using Tripaui.Extensions;
using Tripaui.Forms;
using Tripaui.Views;
using Tripaui.Views.Controlls;
using Tripaui.Views.Places;

namespace Tripaui.ViewModels
{
    [QueryProperty(nameof(Email), "email")]
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly AuthService _authService;
        private readonly IConnectivity _connectivity;
        internal Image LogoImage;
        private CancellationTokenSource rotationCt;

        public LoginViewModel(AuthService authService, IConnectivity connectivity)
        {
            
            _authService = authService;
            _connectivity = connectivity;
        }


        #region Properties

        public UserLoginForm Form { get; } = new();

        public string Email { get => Form.Email; set => Form.Email = value; }
        #endregion

        #region Commands
        [RelayCommand]
        async Task Login()
        {
            if (_connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await Shell.Current.ShowPopupAsync(new ValidationErrorPopup("No internet connection"));
            }
            if (await Form.ValidateForm() > 0)
            {
                IsBusy = false;
                return;
            }
            IsBusy = true;
            var response = await _authService.Login(Form.Email.TrimEnd(), Form.Password);
            if (await response.ValidateResponse() > 0)
            {
                IsBusy = false;
                return;
            }
            await Shell.Current.GoToAsync($"//{nameof(PlacesListPage)}");
            IsBusy = false;
            rotationCt.Cancel();
        }

        [RelayCommand]
        async Task GoToRegister()
        {
            await Shell.Current.GoToAsync($"{nameof(RegisterPage)}");
            rotationCt.Cancel();
        }

        [RelayCommand]
        private async Task PageAppearing()
        {
            rotationCt = new();
            Task.Run(RotateLogo, rotationCt.Token);

            await TryGetMicrophonePermission();

            await TryGetStorageReadPermission();

            await TryGetStorageWritePermission();

            await TryGetFineLocationPermission();
        }

        private async Task RotateLogo()
        {
            long i = 1;
            while (true)
            {
                await LogoImage.RotateYTo(360 * i, 10000, Easing.Linear);
                i++;
            }
        }

        private static async Task TryGetStorageWritePermission()
        {
            if (await Permissions.CheckStatusAsync<Permissions.StorageWrite>() != PermissionStatus.Granted)
            {
                if (await Permissions.RequestAsync<Permissions.StorageWrite>() != PermissionStatus.Granted)
                {
                    //await Shell.Current.DisplayAlert("Permissions Required", "Needs storage permission to temporarily read/write recorded audio", "OK");
                    return;
                }
            }
            return;
        }

        private static async Task TryGetStorageReadPermission()
        {
            if (await Permissions.CheckStatusAsync<Permissions.StorageRead>() != PermissionStatus.Granted)
            {
                if (await Permissions.RequestAsync<Permissions.StorageRead>() != PermissionStatus.Granted)
                {
                    //await Shell.Current.DisplayAlert("Permissions Required", "Needs storage permission to temporarily read/write recorded audio", "OK");
                    return;
                }
            }
            return;
        }

        private static async Task TryGetMicrophonePermission()
        {
            if (await Permissions.CheckStatusAsync<Permissions.Microphone>() != PermissionStatus.Granted)
            {
                if (await Permissions.RequestAsync<Permissions.Microphone>() != PermissionStatus.Granted)
                {
                    //await Shell.Current.DisplayAlert("Permissions Required", "Needs microphone permission to record audio", "OK");
                    return;
                }
            }
            return;
        }

        private static async Task TryGetFineLocationPermission()
        {
            if (await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>() != PermissionStatus.Granted)
            {
                if (await Permissions.RequestAsync<Permissions.LocationWhenInUse>() != PermissionStatus.Granted)
                {
                    //await Shell.Current.DisplayAlert("Permissions Required", "Needs location permission to look for places nearby", "OK");
                    return;
                }
            }
            return;
        }
        #endregion

    }
}
