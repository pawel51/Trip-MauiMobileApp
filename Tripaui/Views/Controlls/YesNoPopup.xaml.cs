using CommunityToolkit.Maui.Views;

namespace Tripaui.Views.Controlls;

public partial class YesNoPopup : Popup
{
    public YesNoPopup(string msg)
    {
        InitializeComponent();
        ErroLabel.Text = msg;
    }

    private void YesClicked(object sender, EventArgs e) => Close(true);
    private void NoClicked(object sender, EventArgs e) => Close(false);
}