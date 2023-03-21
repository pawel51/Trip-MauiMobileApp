using DevExpress.Maui.DataForm;
using Tripaui.ViewModels;

namespace Tripaui.Views;

public partial class RegisterPage : ContentPage
{
    private readonly RegisterViewModel _registerViewModel;

    public RegisterPage(RegisterViewModel registerViewModel)
    {
        InitializeComponent();
        BindingContext = _registerViewModel = registerViewModel;
        _registerViewModel.LogoImage = LogoImage;
    }

}