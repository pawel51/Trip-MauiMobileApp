using CommunityToolkit.Maui.Views;

namespace Tripaui.Views.Controlls;

public partial class ValidationErrorPopup : Popup
{

    public ValidationErrorPopup(string msg)
    {
        InitializeComponent();
        ErroLabel.Text = msg;
    }
    private void ClosePopup(object sender, EventArgs e) => Close();
}