namespace MoreFlyout.Server.Helpers;

public class FlyoutTimerHelper
{
    private readonly DispatcherTimer _hiddenTimer;
    private readonly Action _onTimeout;

    public FlyoutTimerHelper(int timeoutMilliseconds, Action onTimeout)
    {
        _onTimeout = onTimeout ?? throw new ArgumentNullException(nameof(onTimeout));
        
        _hiddenTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(timeoutMilliseconds)
        };
        _hiddenTimer.Tick += OnTimerTick;
    }

    public void Start()
    {
        _hiddenTimer.Stop();
        _hiddenTimer.Start();
    }

    public void Stop()
    {
        _hiddenTimer.Stop();
    }

    private void OnTimerTick(object? sender, object e)
    {
        _hiddenTimer.Stop();
        _onTimeout?.Invoke();
    }
}
