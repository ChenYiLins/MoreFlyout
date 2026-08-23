using System.Windows.Input;

namespace MoreFlyout.App.Models;

public partial class SettingsGroupItem : ObservableObject
{
    [ObservableProperty]
    public partial string Label { get; set; } = string.Empty;

    [ObservableProperty]
    public partial IconElement? Icon { get; set; }

    [ObservableProperty]
    public partial bool IsNew { get; set; }

    [ObservableProperty]
    public partial bool IsLocked { get; set; }

    [ObservableProperty]
    public partial bool IsEnabled { get; set; }

    [ObservableProperty]
    public partial object? Tag { get; set; }

    [ObservableProperty]
    public partial ICommand? ClickCommand { get; set; }

    private bool _isUpdating;

    public Action<SettingsGroupItem>? EnabledChangedCallback { get; set; }

    public void UpdateStatus(bool isEnabled)
    {
        _isUpdating = true;
        try
        {
            IsEnabled = isEnabled;
        }
        finally
        {
            _isUpdating = false;
        }
    }

    partial void OnIsEnabledChanged(bool value)
    {
        if (!_isUpdating)
        {
            EnabledChangedCallback?.Invoke(this);
        }
    }
}
