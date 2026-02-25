using Microsoft.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices;
using Windows.Media.Control;
using Windows.Storage.Streams;

namespace MoreFlyout.Server.Services;

public partial class MediaServices : IDisposable
{
    private static readonly Logger _Logger = LogManager.GetCurrentClassLogger();

    private GlobalSystemMediaTransportControlsSessionManager? _sessionManager;
    private GlobalSystemMediaTransportControlsSession? _currentSession;
    private Microsoft.UI.Dispatching.DispatcherQueue? _dispatcherQueue;
    private bool _isDisposed;

    #region Defined Events

    public event EventHandler<MediaInfoChangedEventArgs>? MediaInfoChanged;
    public event EventHandler<PlaybackStatusChangedEventArgs>? PlaybackStatusChanged;
    public event EventHandler<SessionChangedEventArgs>? CurrentSessionChanged;
    public event EventHandler<TimelineChangedEventArgs>? TimelineChanged;

    #endregion

    #region Initialization

    public async Task InitializeAsync(Microsoft.UI.Dispatching.DispatcherQueue? dispatcherQueue = null)
    {
        _dispatcherQueue = dispatcherQueue;

        try
        {
            _sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
            _sessionManager.CurrentSessionChanged += OnCurrentSessionChanged;
            await UpdateCurrentSessionAsync();
        }
        catch (Exception ex)
        {
            _Logger.Error(ex, $"Initialize SMTC failed: {ex.Message}");
            throw new Exception($"Initialize SMTC failed: {ex.Message}", ex);
        }
    }

    private async Task UpdateCurrentSessionAsync()
    {
        try
        {
            // Unsubscribe events from previous sessions
            if (_currentSession is not null)
            {
                _currentSession.MediaPropertiesChanged -= OnMediaPropertiesChanged;
                _currentSession.PlaybackInfoChanged -= OnPlaybackInfoChanged;
                _currentSession.TimelinePropertiesChanged -= OnTimelinePropertiesChanged;
            }

            // Get the new current session
            _currentSession = _sessionManager?.GetCurrentSession();

            if (_currentSession is not null)
            {
                _currentSession.MediaPropertiesChanged += OnMediaPropertiesChanged;
                _currentSession.PlaybackInfoChanged += OnPlaybackInfoChanged;
                _currentSession.TimelinePropertiesChanged += OnTimelinePropertiesChanged;

                await LoadMediaInfoAsync();
                LoadPlaybackInfo();
                LoadTimelineInfo();

                CurrentSessionChanged?.Invoke(this, new SessionChangedEventArgs { SourceAppId = _currentSession.SourceAppUserModelId });
            }
            else
            {
                MediaInfoChanged?.Invoke(this, new MediaInfoChangedEventArgs { MediaInfo = null });
                PlaybackStatusChanged?.Invoke(this, new PlaybackStatusChangedEventArgs { Status = null });
            }
        }
        catch (Exception ex)
        {
            _Logger.Error(ex, $"Update the current session failed: {ex.Message}");
        }
    }

    #endregion

    #region Get Media Info

    private async Task LoadMediaInfoAsync()
    {
        if (_currentSession is null)
        {
            return;
        }

        try
        {
            var mediaProperties = await _currentSession.TryGetMediaPropertiesAsync();

            if (mediaProperties is null)
            {
                return;
            }

            var mediaInfo = new MediaInfo
            {
                Title = mediaProperties.Title ?? "None title",
                Artist = mediaProperties.Artist ?? "None artist",
                AlbumTitle = mediaProperties.AlbumTitle ?? string.Empty,
                AlbumArtist = mediaProperties.AlbumArtist ?? string.Empty,
                TrackNumber = mediaProperties.TrackNumber,
                AlbumTrackCount = mediaProperties.AlbumTrackCount,
                Subtitle = mediaProperties.Subtitle ?? string.Empty,
                Thumbnail = await LoadThumbnailAsync(mediaProperties.Thumbnail),
            };

            MediaInfoChanged?.Invoke(this, new MediaInfoChangedEventArgs { MediaInfo = mediaInfo });
        }
        catch (COMException ex)
        {
            _Logger.Error(ex, $"Failed to load media information: {ex.Message}");
        }
        catch (Exception ex)
        {
            _Logger.Error(ex, $"Exception occurred while loading media information: {ex.Message}");
        }
    }

    private async Task<BitmapImage?> LoadThumbnailAsync(IRandomAccessStreamReference? thumbnailReference)
    {
        if (thumbnailReference is null)
        {
            return null;
        }

        IRandomAccessStreamWithContentType? stream = null;
        try
        {
            stream = await thumbnailReference.OpenReadAsync();

            if (stream is null || stream.Size == 0)
            {
                _Logger.Warn("Thumbnail stream is null or has size 0");
                return null;
            }

            // Create BitmapImage
            BitmapImage? image;

            if (_dispatcherQueue is not null)
            {
                var tcs = new TaskCompletionSource<BitmapImage?>();

                _dispatcherQueue.TryEnqueue(async void () =>
                {
                    try
                    {
                        var bitmapImage = new BitmapImage();
                        await bitmapImage.SetSourceAsync(stream);
                        tcs.SetResult(bitmapImage);
                    }
                    catch (Exception ex)
                    {
                        _Logger.Error(ex, $"Failed to set image source on UI thread: {ex.Message}");
                        tcs.SetException(ex);
                    }
                });

                image = await tcs.Task;
            }
            else
            {
                _Logger.Warn("No DispatcherQueue provided, thumbnail loading may fail");
                image = new BitmapImage();
                await image.SetSourceAsync(stream);
            }

            return image;
        }
        catch (COMException comEx)
        {
            _Logger.Error($"Exception in loading thumbnail COM: 0x{comEx.HResult:X8} - {comEx.Message}");
            return null;
        }
        catch (Exception ex)
        {
            _Logger.Error(ex, $"Failed to load thumbnail: {ex.Message}");
            return null;
        }
        finally
        {
            stream?.Dispose();
        }
    }

    #endregion

    #region Get Playback Info

    private void LoadPlaybackInfo()
    {
        if (_currentSession is null)
        {
            return;
        }

        try
        {
            var playbackInfo = _currentSession.GetPlaybackInfo();

            if (playbackInfo is null)
            {
                return;
            }

            var status = new PlaybackStatus
            {
                PlaybackState = ConvertPlaybackStatus(playbackInfo.PlaybackStatus),
                IsShuffleActive = playbackInfo.IsShuffleActive ?? false,
                AutoRepeatMode = ConvertAutoRepeatMode(playbackInfo.AutoRepeatMode),
                PlaybackRate = playbackInfo.PlaybackRate ?? 1.0,
            };

            var controls = playbackInfo.Controls;
            status.CanPause = controls.IsPauseEnabled;
            status.CanPlay = controls.IsPlayEnabled;
            status.CanGoNext = controls.IsNextEnabled;
            status.CanGoPrevious = controls.IsPreviousEnabled;
            status.CanSeek = controls.IsPlaybackPositionEnabled;
            status.CanChangeRepeatMode = controls.IsRepeatEnabled;
            status.CanChangeShuffle = controls.IsShuffleEnabled;

            PlaybackStatusChanged?.Invoke(this, new PlaybackStatusChangedEventArgs { Status = status });
        }
        catch (Exception ex)
        {
            _Logger.Error($"Failed to load playback status: {ex.Message}");
        }
    }

    public PlaybackStatus? GetCurrentPlaybackStatus()
    {
        if (_currentSession is null)
        {
            return null;
        }

        try
        {
            var playbackInfo = _currentSession.GetPlaybackInfo();

            if (playbackInfo is null)
            {
                return null;
            }

            var status = new PlaybackStatus
            {
                PlaybackState = ConvertPlaybackStatus(playbackInfo.PlaybackStatus),
                IsShuffleActive = playbackInfo.IsShuffleActive ?? false,
                AutoRepeatMode = ConvertAutoRepeatMode(playbackInfo.AutoRepeatMode),
                PlaybackRate = playbackInfo.PlaybackRate ?? 1.0,
            };

            var controls = playbackInfo.Controls;
            status.CanPause = controls.IsPauseEnabled;
            status.CanPlay = controls.IsPlayEnabled;
            status.CanGoNext = controls.IsNextEnabled;
            status.CanGoPrevious = controls.IsPreviousEnabled;
            status.CanSeek = controls.IsPlaybackPositionEnabled;
            status.CanChangeRepeatMode = controls.IsRepeatEnabled;
            status.CanChangeShuffle = controls.IsShuffleEnabled;

            return status;
        }
        catch (Exception ex)
        {
            _Logger.Error($"Failed to get playback status: {ex.Message}");
            return null;
        }
    }

    #endregion

    #region Get Timeline Info
    private void LoadTimelineInfo()
    {
        if (_currentSession is null)
        {
            return;
        }

        try
        {
            var timelineProperties = _currentSession.GetTimelineProperties();

            if (timelineProperties is not null)
            {
                var timeline = new TimelineInfo
                {
                    StartTime = timelineProperties.StartTime,
                    EndTime = timelineProperties.EndTime,
                    MinSeekTime = timelineProperties.MinSeekTime,
                    MaxSeekTime = timelineProperties.MaxSeekTime,
                    Position = timelineProperties.Position,
                    LastUpdatedTime = timelineProperties.LastUpdatedTime,
                };

                TimelineChanged?.Invoke(this, new TimelineChangedEventArgs { Timeline = timeline });
            }
        }
        catch (Exception ex)
        {
            _Logger.Error($"Failed to load timeline information: {ex.Message}");
        }
    }

    public TimelineInfo? GetCurrentTimelineInfo()
    {
        if (_currentSession is null)
        {
            return null;
        }

        try
        {
            var timelineProperties = _currentSession.GetTimelineProperties();

            if (timelineProperties is null)
            {
                return null;
            }

            return new TimelineInfo
            {
                StartTime = timelineProperties.StartTime,
                EndTime = timelineProperties.EndTime,
                MinSeekTime = timelineProperties.MinSeekTime,
                MaxSeekTime = timelineProperties.MaxSeekTime,
                Position = timelineProperties.Position,
                LastUpdatedTime = timelineProperties.LastUpdatedTime,
            };
        }
        catch (Exception ex)
        {
            _Logger.Error($"Failed to load timeline information: {ex.Message}");
            return null;
        }
    }

    #endregion

    #region Playback Controls

    public async Task<bool> PlayAsync()
    {
        if (_currentSession is null)
        {
            return false;
        }

        try
        {
            return await _currentSession.TryPlayAsync();
        }
        catch (Exception ex)
        {
            _Logger.Error($"Playing failed: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> PauseAsync()
    {
        if (_currentSession is null)
        {
            return false;
        }

        try
        {
            return await _currentSession.TryPauseAsync();
        }
        catch (Exception ex)
        {
            _Logger.Error($"Pause failed: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> TogglePlayPauseAsync()
    {
        if (_currentSession is null)
        {
            return false;
        }

        try
        {
            return await _currentSession.TryTogglePlayPauseAsync();
        }
        catch (Exception ex)
        {
            _Logger.Error($"Toggle play/pause failed: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SkipNextAsync()
    {
        if (_currentSession is null)
        {
            return false;
        }

        try
        {
            return await _currentSession.TrySkipNextAsync();
        }
        catch (Exception ex)
        {
            _Logger.Error($"The next failed: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SkipPreviousAsync()
    {
        if (_currentSession is null)
        {
            return false;
        }

        try
        {
            return await _currentSession.TrySkipPreviousAsync();
        }
        catch (Exception ex)
        {
            _Logger.Error($"The previous failed: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> StopAsync()
    {
        if (_currentSession is null)
        {
            return false;
        }

        try
        {
            return await _currentSession.TryStopAsync();
        }
        catch (Exception ex)
        {
            _Logger.Error($"Stop failed: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ChangePlaybackPositionAsync(TimeSpan position)
    {
        if (_currentSession is null)
        {
            return false;
        }

        try
        {
            return await _currentSession.TryChangePlaybackPositionAsync(position.Ticks);
        }
        catch (Exception ex)
        {
            _Logger.Error($"Failed to change playback position: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ChangeShuffleActiveAsync(bool isActive)
    {
        if (_currentSession is null)
        {
            return false;
        }
        try
        {
            return await _currentSession.TryChangeShuffleActiveAsync(isActive);
        }
        catch (Exception ex)
        {
            _Logger.Error($"Failed to change shuffle: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ChangeAutoRepeatModeAsync(Windows.Media.MediaPlaybackAutoRepeatMode mode)
    {
        if (_currentSession is null)
        {
            return false;
        }

        try
        {
            return await _currentSession.TryChangeAutoRepeatModeAsync(mode);
        }
        catch (Exception ex)
        {
            _Logger.Error($"Failed to change the repeating mode: {ex.Message}");
            return false;
        }
    }

    #endregion

    #region Event Handlers

    private void OnCurrentSessionChanged(GlobalSystemMediaTransportControlsSessionManager sender, CurrentSessionChangedEventArgs args)
    {
        _ = UpdateCurrentSessionAsync();
    }

    private void OnMediaPropertiesChanged(GlobalSystemMediaTransportControlsSession sender, MediaPropertiesChangedEventArgs args)
    {
        _ = LoadMediaInfoAsync();
    }

    private void OnPlaybackInfoChanged(GlobalSystemMediaTransportControlsSession sender, PlaybackInfoChangedEventArgs args)
    {
        LoadPlaybackInfo();
    }

    private void OnTimelinePropertiesChanged(GlobalSystemMediaTransportControlsSession sender, TimelinePropertiesChangedEventArgs args)
    {
        LoadTimelineInfo();
    }

    #endregion

    #region Auxiliary method

    private static PlaybackState ConvertPlaybackStatus(GlobalSystemMediaTransportControlsSessionPlaybackStatus status)
    {
        return status switch
        {
            GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing => PlaybackState.Playing,
            GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused => PlaybackState.Paused,
            GlobalSystemMediaTransportControlsSessionPlaybackStatus.Stopped => PlaybackState.Stopped,
            GlobalSystemMediaTransportControlsSessionPlaybackStatus.Closed => PlaybackState.Closed,
            GlobalSystemMediaTransportControlsSessionPlaybackStatus.Opened => PlaybackState.Opened,
            GlobalSystemMediaTransportControlsSessionPlaybackStatus.Changing => PlaybackState.Changing,
            _ => PlaybackState.Unknown,
        };
    }

    private static RepeatMode ConvertAutoRepeatMode(Windows.Media.MediaPlaybackAutoRepeatMode? mode)
    {
        if (!mode.HasValue)
        {
            return RepeatMode.None;
        }

        return mode.Value switch
        {
            Windows.Media.MediaPlaybackAutoRepeatMode.None => RepeatMode.None,
            Windows.Media.MediaPlaybackAutoRepeatMode.Track => RepeatMode.Track,
            Windows.Media.MediaPlaybackAutoRepeatMode.List => RepeatMode.List,
            _ => RepeatMode.None,
        };
    }

    #endregion

    #region Dispose

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        try
        {
            if (_currentSession is not null)
            {
                _currentSession.MediaPropertiesChanged -= OnMediaPropertiesChanged;
                _currentSession.PlaybackInfoChanged -= OnPlaybackInfoChanged;
                _currentSession.TimelinePropertiesChanged -= OnTimelinePropertiesChanged;
                _currentSession = null;
            }

            _sessionManager?.CurrentSessionChanged -= OnCurrentSessionChanged;
            _sessionManager = null;

            _isDisposed = true;
        }
        catch (Exception ex)
        {
            _Logger.Error(ex, $"Exception occurred during disposal: {ex.Message}");
        }

        GC.SuppressFinalize(this);
    }

    #endregion
}

#region Data Models

public class MediaInfo
{
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public string AlbumTitle { get; set; } = string.Empty;
    public string AlbumArtist { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public int TrackNumber { get; set; }
    public int AlbumTrackCount { get; set; }
    public BitmapImage? Thumbnail { get; set; }
}

public class PlaybackStatus
{
    public PlaybackState PlaybackState { get; set; }
    public bool IsShuffleActive { get; set; }
    public RepeatMode AutoRepeatMode { get; set; }
    public double PlaybackRate { get; set; }

    public bool CanPause { get; set; }
    public bool CanPlay { get; set; }
    public bool CanGoNext { get; set; }
    public bool CanGoPrevious { get; set; }
    public bool CanSeek { get; set; }
    public bool CanChangeRepeatMode { get; set; }
    public bool CanChangeShuffle { get; set; }
}

public class TimelineInfo
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public TimeSpan MinSeekTime { get; set; }
    public TimeSpan MaxSeekTime { get; set; }
    public TimeSpan Position { get; set; }
    public DateTimeOffset LastUpdatedTime { get; set; }
}

public enum PlaybackState
{
    Unknown,
    Closed,
    Opened,
    Changing,
    Stopped,
    Playing,
    Paused,
}

public enum RepeatMode
{
    None,
    Track,
    List,
}

#endregion

#region Event Args

public class MediaInfoChangedEventArgs : EventArgs
{
    public MediaInfo? MediaInfo { get; set; }
}

public class PlaybackStatusChangedEventArgs : EventArgs
{
    public PlaybackStatus? Status { get; set; }
}

public class SessionChangedEventArgs : EventArgs
{
    public string SourceAppId { get; set; } = string.Empty;
}

public class TimelineChangedEventArgs : EventArgs
{
    public TimelineInfo? Timeline { get; set; }
}

#endregion
