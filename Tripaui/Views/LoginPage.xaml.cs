using System.Text.RegularExpressions;
using Tripaui.ViewModels;

namespace Tripaui.Views;

public partial class LoginPage : ContentPage
{
	private readonly LoginViewModel _loginViewModel;

	public LoginPage(LoginViewModel loginViewModel)
	{
		InitializeComponent();
		BindingContext = _loginViewModel = loginViewModel;
		_loginViewModel.LogoImage = LogoImage;
	}

}