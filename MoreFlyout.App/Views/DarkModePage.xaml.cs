namespace MoreFlyout.App.Views;

public sealed partial class DarkModePage : Page
{
    public DarkModeViewModel ViewModel { get; }

    public DarkModePage()
    {
        ViewModel = App.GetService<DarkModeViewModel>();
        InitializeComponent();
    }
}
