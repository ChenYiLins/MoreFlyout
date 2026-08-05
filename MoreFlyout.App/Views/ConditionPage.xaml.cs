namespace MoreFlyout.App.Views;

public sealed partial class ConditionPage : Page
{
    public ConditionViewModel ViewModel { get; }

    public ConditionPage()
    {
        ViewModel = App.GetService<ConditionViewModel>();
        InitializeComponent();
    }
}
