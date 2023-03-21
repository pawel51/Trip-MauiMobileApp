using DevExpress.Maui.DataForm;
using GoogleApi.Entities.Maps.Common.Enums;
using Tripaui.ViewModels.Trips;

namespace Tripaui.Views.Trips;

public partial class AddTripPage : ContentPage
{
    private readonly AddTripViewModel _addTripViewModel;

    public AddTripPage(AddTripViewModel addTripViewModel)
    {
        InitializeComponent();
        this.BindingContext = _addTripViewModel = addTripViewModel;
    }
}