using CommunityToolkit.Mvvm.Input;
using Services;
using Shared.Entities;
using Tripaui.Extensions;
using Tripaui.Forms;
using Tripaui.Views;
using Tripaui.Views.Places;

namespace Tripaui.ViewModels
{
    public partial class RegisterViewModel : BaseViewModel
    {
        private readonly AuthService _authService;
        private readonly UsersService _usersService;
        private CancellationTokenSource rotationCt;
        public Image LogoImage;

        public RegisterViewModel(AuthService authService, UsersService usersService)
        {
            _authService = authService;
            _usersService = usersService;
            Form = new UserRegisterForm();
        }

        #region Properties
        public UserRegisterForm Form { get; }

        #endregion


        [RelayCommand]
        private async Task PageAppearing()
        {
            rotationCt = new();
            Task.Run(RotateLogo, rotationCt.Token);
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

        [RelayCommand]
        async Task Register()
        {
            if (await Form.ValidateForm() > 0) return;
            if (await Form.CheckIfPasswordsMatch() > 0) return;
            IsBusy = true;
            var response = await _authService
                .Register(new MyUserModel(
                    Guid.NewGuid(), 
                    Form.Username, 
                    Form.Password, 
                    Form.Email));

            if (await response.ValidateResponse() > 0)
            {
                IsBusy = false;
                return;
            }
            await Shell.Current.GoToAsync($"//{nameof(PlacesListPage)}");
            IsBusy = false;
        }

        [RelayCommand]
        async Task GoToLogin()
        {
            await Shell.Current.GoToAsync($"{nameof(LoginPage)}");
        }

    }
}
