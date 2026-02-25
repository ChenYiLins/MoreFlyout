using MoreFlyout.Server.ViewModels;

namespace MoreFlyout.Server.Views
{
    public sealed partial class TrayIconMenuFlyoutPage : Page
    {
        public TrayIconMenuFlyoutViewModel ViewModel { get; set; }

        public TrayIconMenuFlyoutPage()
        {
            ViewModel = new TrayIconMenuFlyoutViewModel();
            InitializeComponent();

            this.Bindings.Update();
        }
    }
}
