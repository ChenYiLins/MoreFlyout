namespace MoreFlyout.App.Views;

public sealed partial class MediaPage : Page
{
    public MediaViewModel ViewModel { get; }

    public MediaPage()
    {
        ViewModel = App.GetService<MediaViewModel>();
        InitializeComponent();
    }
}
