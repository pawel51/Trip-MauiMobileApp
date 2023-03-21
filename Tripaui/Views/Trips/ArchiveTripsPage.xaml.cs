using Tripaui.ViewModels.Trips;

namespace Tripaui.Views.Trips;

public partial class ArchiveTripsPage : ContentPage
{
    private readonly ArchiveTripsViewModel _vm;

    public ArchiveTripsPage(ArchiveTripsViewModel vm)
	{
		InitializeComponent();
        BindingContext = _vm = vm;
    }
}