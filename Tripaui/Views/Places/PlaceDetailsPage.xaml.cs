using Tripaui.ViewModels.Places;

namespace Tripaui.Views.Places;

public partial class PlaceDetailsPage : ContentPage
{
    private readonly PlaceDetailsViewModel _placeDetailsViewModel;

    public PlaceDetailsPage(PlaceDetailsViewModel placeDetailsViewModel)
    {
        InitializeComponent();
        BindingContext = _placeDetailsViewModel = placeDetailsViewModel;
    }
}