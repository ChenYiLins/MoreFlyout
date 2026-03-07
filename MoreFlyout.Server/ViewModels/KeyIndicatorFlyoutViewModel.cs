using CommunityToolkit.Mvvm.Input;
using MoreFlyout.Server.Helpers;
using MoreFlyout.Server.Utils;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace MoreFlyout.Server.ViewModels;

public partial class KeyIndicatorFlyoutViewModel : ObservableRecipient
{
    private int _currentKey;

    [ObservableProperty]
    public partial string? Status { get; set; }

    [ObservableProperty]
    public partial string? ModeGlyph { get; set; }

    [ObservableProperty]
    public partial string? StatusGlyph { get; set; }

    [ObservableProperty]
    public partial bool IsLocked { get; set; }

    public void Update(int vkCode, bool isLocked)
    {
        _currentKey = vkCode;
        IsLocked = isLocked;

        if (vkCode == (int)VIRTUAL_KEY.VK_NUMLOCK)
        {
            ModeGlyph = "\uF146";
            Status = isLocked ? "StatusWords_NumUnlock".GetLocalized() : "StatusWords_NumLock".GetLocalized();
            StatusGlyph = isLocked ? "\uE785" : "\uE72E";
        }
        else if (vkCode == (int)VIRTUAL_KEY.VK_CAPITAL)
        {
            ModeGlyph = "\uE97E";
            Status = isLocked ? "StatusWords_CapsUnlock".GetLocalized() : "StatusWords_CapsLock".GetLocalized();
            StatusGlyph = isLocked ? "\uE785" : "\uE72E";
        }
    }

    [RelayCommand]
    private void ToggleLock()
    {
        if (_currentKey == (int)VIRTUAL_KEY.VK_CAPITAL)
        {
            SimulationKeyboard.ToggleCapsLock();
        }
        else if (_currentKey == (int)VIRTUAL_KEY.VK_NUMLOCK)
        {
            SimulationKeyboard.ToggleNumberLock();
        }
    }
}
