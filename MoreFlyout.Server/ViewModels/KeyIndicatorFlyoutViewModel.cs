using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoreFlyout.Server.Helpers;
using MoreFlyout.Server.Utils;

namespace MoreFlyout.Server.ViewModels;

public partial class KeyIndicatorFlyoutViewModel : ObservableRecipient
{
    [ObservableProperty]
    public partial string? Status { get; set; }

    [RelayCommand]
    private void ToggleLock()
    {
        if (Status == "StatusWords_CapsLock".GetLocalized() || Status == "StatusWords_CapsUnlock".GetLocalized())
        {
            SimulationKeyboard.ToggleCapsLock();
        }
        else if (Status == "StatusWords_NumLock".GetLocalized() || Status == "StatusWords_NumUnlock".GetLocalized())
        {
            SimulationKeyboard.ToggleNumberLock();
        }
    }

    public KeyIndicatorFlyoutViewModel()
    {

    }
}