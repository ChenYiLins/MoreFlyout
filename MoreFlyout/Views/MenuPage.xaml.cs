using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MoreFlyout.Taskbar;
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

        Loaded += MenuPage_Loaded;
        ContextMenu.Closed += ContextMenu_Closed;
    }

    private void ContextMenu_Closed(object? sender, object e) => SystemTrayIcon.trayIconContextMenu.contextMenuWindow.Close();

    private void MenuPage_Loaded(object sender, RoutedEventArgs e) => ContextMenu.ShowAt(this);

    private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender as MenuFlyoutItem == ProjectMenuFlyoutItem)
        {

        }
        else if (sender as MenuFlyoutItem == CloseMenuFlyoutItem)
        {
            App.SystemTrayIcon.Hide();
            Environment.Exit(0);
            Application.Current.Exit();
        }
    }
}
