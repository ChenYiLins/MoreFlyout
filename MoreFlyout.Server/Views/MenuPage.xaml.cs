using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MoreFlyout.Server.ViewModels;

namespace MoreFlyout.Server.Views;

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
        Loaded += MenuPage_Loaded;
    }

    private void MenuPage_Loaded(object sender, RoutedEventArgs e) => this.ContextFlyout.ShowAt(this);

    private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender as MenuFlyoutItem == ProjectMenuFlyoutItem)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = "https://github.com/ChenYiLins/MoreFlyout.Server",
                UseShellExecute = true
            });
        }
        else if (sender as MenuFlyoutItem == CloseMenuFlyoutItem)
        {
            App.Current.Exit();
        }
    }
}
