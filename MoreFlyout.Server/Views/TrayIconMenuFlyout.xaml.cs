using MoreFlyout.Server.ViewModels;

namespace MoreFlyout.Server.Views;

public sealed partial class TrayIconMenuFlyout : Page
{
    public TrayIconMenuFlyoutViewModel ViewModel { get; set; }

    public TrayIconMenuFlyout()
    {
        ViewModel = new TrayIconMenuFlyoutViewModel();
        InitializeComponent();

        this.Bindings.Update();
    }
}
