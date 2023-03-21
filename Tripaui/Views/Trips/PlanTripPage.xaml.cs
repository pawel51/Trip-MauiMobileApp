using Tripaui.ViewModels.Trips;

namespace Tripaui.Views.Trips;

public partial class PlanTripPage : ContentPage
{
	public PlanTripPage(PlanTripViewModel viewModel)
	{
		InitializeComponent();
		this.BindingContext = viewModel;
	}
}