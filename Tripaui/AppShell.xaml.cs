using CommunityToolkit.Mvvm.Messaging;
using Tripaui.Messages;
using Tripaui.ViewModels;
using Tripaui.Views;
using Tripaui.Views.Places;
using Tripaui.Views.Trips;

namespace Tripaui;

public partial class AppShell : Shell
{
    public AppShell(AppShellViewModel vm)
    {
        this.BindingContext = vm;
        InitializeComponent();
        
        // all routes unaccessible from shell should be registered
        Routing.RegisterRoute($"{nameof(LoginPage)}", typeof(LoginPage));
        Routing.RegisterRoute($"{nameof(RegisterPage)}", typeof(RegisterPage));
        Routing.RegisterRoute($"{nameof(PlaceDetailsPage)}" , typeof(PlaceDetailsPage));

        Routing.RegisterRoute($"{nameof(TripDetailsPage)}", typeof(TripDetailsPage));
        Routing.RegisterRoute($"{nameof(AddTripPage)}", typeof(AddTripPage));
        Routing.RegisterRoute($"{nameof(EditTripPage)}", typeof(EditTripPage));
        Routing.RegisterRoute($"{nameof(ChooseTripPage)}", typeof(ChooseTripPage));
        Routing.RegisterRoute($"{nameof(MapPage)}", typeof(MapPage));
        Routing.RegisterRoute($"{nameof(PlanTripPage)}", typeof(PlanTripPage));
        Routing.RegisterRoute($"{nameof(ReviewsPage)}", typeof(ReviewsPage));
        // never navigate to the root route from which you did not started
    }

    private void MenuItem_Clicked(object sender, EventArgs e)
    {
        Current.GoToAsync($"{nameof(LoginPage)}");
    }
}
