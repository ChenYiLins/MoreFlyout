using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MoreFlyout.Views;

namespace MoreFlyout.Taskbar;

public class TrayFlyoutContextMenu
{
    public WindowEx ContextMenuWindow = new()
    {
        IsAlwaysOnTop = true,
        IsShownInSwitchers = false,
        ExtendsContentIntoTitleBar = true,
    };

    public TrayFlyoutContextMenu()
    {
        ContextMenuWindow.SetForegroundWindow();
        ContextMenuWindow.SetWindowOpacity(0);
        ContextMenuWindow.Activated += ContextMenuWindow_Activated;

        var rootFrame = new Frame();
        ContextMenuWindow.Content = rootFrame;
        rootFrame.Navigate(typeof(MenuPage), string.Empty);
    }

    private void ContextMenuWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        if (args.WindowActivationState == WindowActivationState.Deactivated) ContextMenuWindow.Close();
    }
}