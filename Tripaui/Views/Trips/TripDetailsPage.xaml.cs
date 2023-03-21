using Microsoft.Maui.Controls.Shapes;
using Shared.Responses;
using Tripaui.ViewModels.Trips;

namespace Tripaui.Views.Trips;

public partial class TripDetailsPage : ContentPage
{
    private readonly TripDetailsViewModel _viewmodel;

    public TripDetailsPage(TripDetailsViewModel viewmodel)
    {
        InitializeComponent();
        this.BindingContext = _viewmodel = viewmodel;
    }

    private void DXCollectionView_DragItemOver(object sender, DevExpress.Maui.CollectionView.DropItemEventArgs e)
    {
        // cannot drop item on lower position
        //if (e.DropItemHandle > e.ItemHandle)
        //    e.Allow = false;
    }

    private void DXCollectionView_DropItem(object sender, DevExpress.Maui.CollectionView.DropItemEventArgs e)
    {
        // cannot drop items on first and fifth position
        //if (e.DropItemHandle == 0 || e.DropItemHandle == 4)
        //{
        //    e.Allow = false;
        //    DisplayAlert("Alert", "You cannot replace this task: "
        //    + ((PlaceDetailsDto)e.DropItem).PlacesDetailsResponse.Name, "OK");
        //}
    }
    private void DXCollectionView_CompleteItemDragDrop(object sender, DevExpress.Maui.CollectionView.CompleteItemDragDropEventArgs e)
    {
        // completed dragdrop
        //DisplayAlert("Reminder", "The item with Name: "
        //    + ((PlaceDetailsDto)e.Item).PlacesDetailsResponse.Name + " has changed its position to "
        //    + (e.ItemHandle + 1) + ". Don't miss the deadline!", "OK");
    }
}

    