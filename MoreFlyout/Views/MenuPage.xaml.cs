using Microsoft.UI.Xaml.Controls;

using MoreFlyout.ViewModels;

namespace MoreFlyout.Views;

public sealed partial class MenuPage : Page
{
    public MenuViewModel ViewModel
    {
        get;
    }

    public MenuPage()
    {
        ViewModel = App.GetService<MenuViewModel>();
        InitializeComponent();
    }
}
