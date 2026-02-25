namespace MoreFlyout.App.Views;

public sealed partial class KeyIndicatorPage : Page
{
    public KeyIndicatorViewModel ViewModel { get; }

    public KeyIndicatorPage()
    {
        ViewModel = App.GetService<KeyIndicatorViewModel>();
        InitializeComponent();
    }
}
