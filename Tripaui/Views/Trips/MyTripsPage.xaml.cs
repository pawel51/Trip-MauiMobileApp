using Tripaui.ViewModels.Trips;

namespace Tripaui.Views.Trips;

public partial class MyTripsPage : ContentPage
{
    private readonly MyTripsViewModel _myTripsViewModel;

    public MyTripsPage(MyTripsViewModel myTripsViewModel)
    {
        InitializeComponent();
        this.BindingContext = _myTripsViewModel = myTripsViewModel;
    }
}