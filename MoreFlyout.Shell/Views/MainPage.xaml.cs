using Microsoft.UI.Xaml.Controls;
using MoreFlyout.Shell.Helpers;
using MoreFlyout.Shell.ViewModels;

namespace MoreFlyout.Shell.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();

        AppTitleBarText.Text = "AppDisplayName".GetLocalized();
    }
}
