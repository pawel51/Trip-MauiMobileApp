using Shared.Utils;
using Tripaui.ViewModels.Places;
using Tripaui.Views.Controlls;

namespace Tripaui.Views.Places;

public partial class PlacesListPage : ContentPage
{
    private readonly PlaceListViewModel _placeListViewModel;

    public PlacesListPage(PlaceListViewModel placeListViewModel)
    {
        InitializeComponent();
        BindingContext = _placeListViewModel = placeListViewModel;
        placeListViewModel.FiltersContent = FiltersContent;
        placeListViewModel.SortingContent = SortingContent;

    }

    private void SearchBar_SearchButtonPressed(object sender, EventArgs e)
    {
        _placeListViewModel.DeSelectAllCommand.Execute(null);
        _placeListViewModel.DeselectRecommendedCommand.Execute(null);
        TurnOffAllButtons(recommendedStates);
        TurnOffAllButtons(filterStates);
        TurnOffAllButtons(sortStates);

    }

    private Dictionary<Button, bool> recommendedStates = new();
    private void RecommendedItem_Clicked(object sender, EventArgs e)
    {
        var button = ((Button)sender);

        if (recommendedStates.ContainsKey(button))
        {
            SwitchButtonToOpposite(recommendedStates, button);
        }
        else
        {
            TurnOnButton(recommendedStates, button);
        }
    }


    private Dictionary<Button, bool> filterStates = new();
    private void FilterItem_Clicked(object sender, EventArgs e)
    {
        var button = ((Button)sender);

        if (filterStates.ContainsKey(button))
        {
            SwitchButtonToOpposite(filterStates, button);
        }
        else
        {
            TurnOnButton(filterStates, button);
        }
    }

    

    private void DeselectBtn_Clicked(object sender, EventArgs e)
    {
        TurnOffAllButtons(filterStates);
    }

    private Dictionary<Button, bool> sortStates = new();

    private void SortingItem_Clicked(object sender, EventArgs e)
    {
        var button = ((Button)sender);
        if (sortStates.ContainsKey(button))
        {
            if (sortStates[button])
            {
                button.BackgroundColor = Color.FromArgb("#e85d04");
            }
            else
            {
                TurnOffAllButtons(sortStates);
                button.BackgroundColor = Color.FromArgb("#9d0208");
            }
            sortStates[button] = !sortStates[button];
        }
        else
        {
            TurnOffAllButtons(sortStates);
            TurnOnButton(sortStates, button);
        }

    }
    private void TurnOnButton(Dictionary<Button, bool> buttonStates, Button button)
    {
        buttonStates.Add(button, true);
        button.BackgroundColor = Color.FromArgb("#9d0208");
    }

    private void SwitchButtonToOpposite(Dictionary<Button, bool> buttonStates, Button button)
    {
        if (buttonStates[button])
        {
            button.BackgroundColor = Color.FromArgb("#e85d04");
        }
        else
            button.BackgroundColor = Color.FromArgb("#9d0208");
        buttonStates[button] = !buttonStates[button];
    }

    private void TurnOffAllButtons(Dictionary<Button, bool> buttonStates)
    {
        foreach (Button buttonState in buttonStates.Keys)
        {
            buttonStates[buttonState] = false;
            buttonState.BackgroundColor = Color.FromArgb("#e85d04");
        }
    }

}