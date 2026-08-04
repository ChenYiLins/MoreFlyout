using MoreFlyout.Service.ViewModels;

namespace MoreFlyout.Service.Views;

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
