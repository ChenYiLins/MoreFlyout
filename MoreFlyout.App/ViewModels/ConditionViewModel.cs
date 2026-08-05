using MoreFlyout.App.Contracts.Services;

namespace MoreFlyout.App.ViewModels;

public partial class ConditionViewModel : ObservableObject
{
    private bool _isLoading;
    private readonly IErrorService _errorService;

    [ObservableProperty]
    public partial bool GameModeEnabled { get; set; }

    [ObservableProperty]
    public partial bool EnergySaverEnabled { get; set; }

    public ConditionViewModel(IErrorService errorService)
    {
        _errorService = errorService;

        LoadConfig();
    }

    private void LoadConfig()
    {
        _isLoading = true;
        try
        {
            GameModeEnabled = ConfigManager.Instance.ServiceSettings.GameMode;
            EnergySaverEnabled = ConfigManager.Instance.ServiceSettings.EnergySaver;
        }
        finally
        {
            _isLoading = false;
        }
    }

    partial void OnGameModeEnabledChanged(bool value)
    {
        if (_isLoading)
        {
            return;
        }

        ConfigManager.Instance.ServiceSettings.GameMode = value;
        ConfigManager.Save();
    }

    partial void OnEnergySaverEnabledChanged(bool value)
    {
        if (_isLoading)
        {
            return;
        }

        ConfigManager.Instance.ServiceSettings.EnergySaver = value;
        ConfigManager.Save();
    }
}
