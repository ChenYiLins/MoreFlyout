using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MoreFlyout.Views;

namespace MoreFlyout.Taskbar;

public class TrayFlyoutContextMenu
{
    public Window contextMenuWindow = new WindowEx()
    {
        IsAlwaysOnTop = true,
        IsResizable = false,
        IsShownInSwitchers = false,
    };

    public TrayFlyoutContextMenu()
    {
        contextMenuWindow.SetExtendedWindowStyle(ExtendedWindowStyle.Transparent);
        contextMenuWindow.SetWindowOpacity(0);
        contextMenuWindow.ExtendsContentIntoTitleBar = true;
        var rootFrame = new Frame();
        contextMenuWindow.Content = rootFrame;
        rootFrame.Navigate(typeof(MenuPage), string.Empty);

        contextMenuWindow.Activated += ContextMenuWindow_Activated;
    }

    private void ContextMenuWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        if (args.WindowActivationState == WindowActivationState.Deactivated) contextMenuWindow.Close();
    }
}