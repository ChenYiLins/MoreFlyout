using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Controls;
using MoreFlyout.App.Models;

namespace MoreFlyout.App.UserControls;

public sealed partial class SettingsGroup : UserControl
{
    public SettingsGroup()
    {
        InitializeComponent();
    }

    [GeneratedDependencyProperty(DefaultValueCallback = nameof(CreateDefaultDividerThickness))]
    public partial Thickness DividerThickness { get; set; }

    [GeneratedDependencyProperty(DefaultValue = true)]
    public partial bool IsItemClickable { get; set; }

    [GeneratedDependencyProperty]
    public partial object? ItemsSource { get; set; }

#pragma warning disable CA1859
    private static object CreateDefaultDividerThickness() => new Thickness(0, 1, 0, 0);
#pragma warning restore CA1859

    private void OnSettingsGroupRepeaterElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
    {
        if (args.Element is SettingsCard settingsCard)
        {
            ChangeBorderAndCornerRadius(settingsCard, args.Index, sender.ItemsSourceView.Count);
        }
    }

    private static void ChangeBorderAndCornerRadius(SettingsCard settingsCard, int index, int itemCount)
    {
        settingsCard.Padding = new Thickness(25, 0, 25, 0);
        settingsCard.CornerRadius = new CornerRadius(0);
        settingsCard.BorderThickness = new Thickness(1, 1, 1, 0);

        if (index == 0)
        {
            settingsCard.BorderThickness = new Thickness(1, 1, 1, 0);
            settingsCard.CornerRadius = new CornerRadius(4, 4, 0, 0);
        }

        if (index == itemCount - 1)
        {
            settingsCard.CornerRadius = new CornerRadius(0, 0, 4, 4);
            settingsCard.BorderThickness = itemCount == 1 ? new Thickness(1, 0, 1, 1) : new Thickness(1);
        }
    }

    private void OnSettingsCardClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element && element.Tag is SettingsGroupItem item)
        {
            item.ClickCommand?.Execute(item.Tag);
        }
    }
}
