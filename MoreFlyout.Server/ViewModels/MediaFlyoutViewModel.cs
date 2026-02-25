using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media.Imaging;
using MoreFlyout.Server.Services;
using MoreFlyout.Server.Utils;
using System.Diagnostics;

namespace MoreFlyout.Server.ViewModels;

public partial class MediaFlyoutViewModel : ObservableRecipient
{
    private readonly DispatcherQueue? _dispatcherQueue;
    private MediaServices? _mediaServices;

    [ObservableProperty]
    public partial string MediaTitle { get; set; } = "Unknown Title";

    [ObservableProperty]
    public partial string MediaArtist { get; set; } = "Unknown Artist";

    [ObservableProperty]
    public partial BitmapImage? MediaCoverImage { get; set; }

    [ObservableProperty]
    public partial string PlayPauseFontIconGlyph { get; set; } = "\uF5B0";

    [ObservableProperty]
    public partial bool IsPlayPauseButtonEnabled { get; set; }

    [ObservableProperty]
    public partial bool IsPreviousButtonEnabled { get; set; }

    [ObservableProperty]
    public partial bool IsNextButtonEnabled { get; set; }

    [RelayCommand]
    private void PlayPause()
    {
        SimulationKeyboard.MediaTogglePlayPause();
    }

    [RelayCommand]
    private void Previous()
    {
        SimulationKeyboard.MediaPrevious();
    }

    [RelayCommand]
    private void Next()
    {
        SimulationKeyboard.MediaNext();
    }

    public MediaFlyoutViewModel()
    {
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        _ = InitializeMediaControlsAsync();
    }

    public async Task InitializeMediaControlsAsync()
    {
        try
        {
            _mediaServices = new MediaServices();

            _mediaServices.MediaInfoChanged += OnMediaInfoChanged;
            _mediaServices.PlaybackStatusChanged += OnPlaybackStatusChanged;

            await _mediaServices.InitializeAsync(_dispatcherQueue);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Initialize failed: {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (_mediaServices is not null)
        {
            _mediaServices.MediaInfoChanged -= OnMediaInfoChanged;
            _mediaServices.PlaybackStatusChanged -= OnPlaybackStatusChanged;
            _mediaServices.Dispose();
            _mediaServices = null;
        }
    }

    private void OnMediaInfoChanged(object? sender, MediaInfoChangedEventArgs e)
    {
        if (e.MediaInfo is null)
        {
            return;
        }

        _dispatcherQueue?.TryEnqueue(() =>
        {
            MediaTitle = e.MediaInfo.Title;
            MediaArtist = e.MediaInfo.Artist;
            MediaCoverImage = e.MediaInfo.Thumbnail;
        });
    }

    private void OnPlaybackStatusChanged(object? sender, PlaybackStatusChangedEventArgs e)
    {
        if (e.Status is null)
        {
            return;
        }

        _dispatcherQueue?.TryEnqueue(() =>
        {
            PlayPauseFontIconGlyph = e.Status.PlaybackState == PlaybackState.Playing ? "\uF8AE" : "\uF5B0";
            IsPlayPauseButtonEnabled = e.Status.CanPlay || e.Status.CanPause;
            IsPreviousButtonEnabled = e.Status.CanGoPrevious;
            IsNextButtonEnabled = e.Status.CanGoNext;
        });
    }
}