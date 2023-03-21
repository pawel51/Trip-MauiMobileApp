using Tripaui.ViewModels;

namespace Tripaui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell(new AppShellViewModel());
    }
}
