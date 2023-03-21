using Tripaui.ViewModels.Trips;

namespace Tripaui.Views.Trips;

public partial class EditTripPage : ContentPage
{
	public EditTripPage(EditTripViewModel viewmodel)
	{
		InitializeComponent();
		this.BindingContext = viewmodel;
	}
}