using CommunityToolkit.Mvvm.ComponentModel;
using MoreFlyout.Contracts.Services;

namespace MoreFlyout.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    private readonly IThemeSelectorService _themeSelectorService;

    public MainViewModel(IThemeSelectorService themeSelectorService)
    {
        _themeSelectorService = themeSelectorService;
        _themeSelectorService.SetThemeAsync(Microsoft.UI.Xaml.ElementTheme.Dark);
    }
}
