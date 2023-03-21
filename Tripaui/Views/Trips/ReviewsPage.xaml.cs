using Tripaui.ViewModels.Trips;

namespace Tripaui.Views.Trips;

public partial class ReviewsPage : ContentPage
{
    ReviewsViewModel _viewModel;

    private readonly Animation Rotate0to180z;
    private readonly Animation Rotate180to0z;
    private readonly Animation ExpandVerticalLayout;
    private readonly Animation HideVerticalLayout;
    public ReviewsPage(ReviewsViewModel viewmodel)
	{
		InitializeComponent();
		this.BindingContext = _viewModel = viewmodel;
		viewmodel.AddBottomSheet = AddBottomSheet;
		viewmodel.UpdateBottomSheet = UpdateBottomSheet;

        Rotate0to180z = new Animation(v => SortingIcon.Rotation = v, 0, 180);
        Rotate180to0z = new Animation(v => SortingIcon.Rotation = v, 180, 0);

        ExpandVerticalLayout = new Animation(v => SortingButtonsLayout.HeightRequest = v, 45, 110);
        HideVerticalLayout = new Animation(v => SortingButtonsLayout.HeightRequest = v, 110, 45);

    }


    private void SortinButtonsHeaderLayout_Tapped(object sender, TappedEventArgs e)
    {
        if (_viewModel.IsSortExpanded)
        {
            HideVerticalLayout.Commit(this,
            nameof(Rotate180to0z),
            16,
            250,
            Easing.CubicInOut,
            (v, c) => SortingButtonsLayout.HeightRequest = 45,
            () => false);

            Rotate180to0z.Commit(this,
            nameof(Rotate180to0z),
            16,
            250,
            Easing.CubicInOut,
            (v, c) => SortingIcon.Rotation = 0,
            () => false);
        }
        else
        {
            ExpandVerticalLayout.Commit(this,
            nameof(Rotate180to0z),
            16,
            250,
            Easing.CubicInOut,
            (v, c) => SortingButtonsLayout.HeightRequest = 110,
            () => false);

            Rotate0to180z.Commit(this,
            nameof(Rotate0to180z),
            16,
            250,
            Easing.CubicInOut,
            (v, c) => SortingIcon.Rotation = 180,
            () => false);
        }
        _viewModel.IsSortExpanded = !_viewModel.IsSortExpanded;
    }
}