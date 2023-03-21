using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Tripaui.Services;
using Tripaui.ViewModels.Trips;
using Map = Microsoft.Maui.Controls.Maps.Map;

namespace Tripaui.Views.Trips;

public partial class MapPage : ContentPage
{

    MapViewModel _dc;
    public MapPage(MapViewModel viewmodel)
    {
        InitializeComponent();
        this.BindingContext = _dc = viewmodel;
    }


    private async void ContentPage_Appearing(object sender, EventArgs e)
    {
        await _dc.PageAppearing();
        MapSpan mapSpan = new(_dc.Pins.First().Value.Location, 0.01, 0.01);
        map.MoveToRegion(mapSpan);
       
        foreach (var polyline in _dc.Polylines)
        {
            map.MapElements.Add(polyline.Value);
        }
        int i = 0;
        foreach (var pin in _dc.Pins)
        {
            Pin indexedPin = new Pin()
            {
                Label = pin.Value.Label + $"{i}",
                Address = pin.Value.Address,
                Location = pin.Value.Location
            };
            map.Pins.Add(indexedPin);
        }

        map.Pins.Add(_dc.CurrentLocationPin);
        
    }

}