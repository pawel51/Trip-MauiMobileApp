using Tripaui.ViewModels.Trips;

namespace Tripaui.Views.Trips;

public partial class ChooseTripPage : ContentPage
{
    public ChooseTripPage(ChooseTripViewModel viewModel)
    {
        InitializeComponent();
        this.BindingContext = viewModel;
    }
}