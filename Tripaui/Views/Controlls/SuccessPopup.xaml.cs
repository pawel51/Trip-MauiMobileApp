using CommunityToolkit.Maui.Views;

namespace Tripaui.Views.Controlls;

public partial class SuccessPopup : Popup
{
	public SuccessPopup(string msg)
	{
		InitializeComponent();
		LabelName.Text = msg;
	}

    private void ClosePopup(object sender, EventArgs e) => Close();
}