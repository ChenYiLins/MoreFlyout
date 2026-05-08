using CommunityToolkit.WinUI;
using Microsoft.UI.Content;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using MoreFlyout.Server.Helpers;
using MoreFlyout.Server.Utils;

namespace MoreFlyout.Server;

public sealed partial class FlyoutControl : UserControl
{
    private readonly XamlIslandHostWindow? _host;
    private bool _wasTaskbarLightLastTimeChecked;
    private bool _wasTaskbarColorPrevalenceLastTimeChecked;
    private ContentExternalBackdropLink? _backdropLink;
    private bool _isBackdropLinkAttached;
    private bool _isPopupAnimationPlaying;

    [GeneratedDependencyProperty]
    public partial UIElement? RootContent { get; set; }

    public bool IsOpen { get; private set; }

    public FlyoutControl()
    {
        InitializeComponent();

        _host = new XamlIslandHostWindow();
        _host.SetContent(this);
        _host.UpdateWindowVisibility(false);
    }

    public void Show()
    {
        if (_host?.DesktopWindowXamlSource is null || RootGrid is null || _isPopupAnimationPlaying)
        {
            return;
        }

        _isPopupAnimationPlaying = true;
        _host.Maximize();

        _ = Task.Run(async () =>
        {
            RootGrid.DispatcherQueue.TryEnqueue(async () =>
            {
                UpdateLayout();
                await Task.Delay(1);

                UpdateFlyoutTheme();
                UpdateFlyoutRegion();

                // Ensure to hide first
                if (RootGrid.RenderTransform is TranslateTransform translateTransform)
                {
                    translateTransform.Y = DesiredSize.Height;
                }

                UpdateLayout();
                await Task.Delay(1);

                _host.UpdateWindowVisibility(true);

                var storyboard = AnimatedHelpers.GetWindows11BottomToTopTransitionStoryboard(RootGrid, (int)DesiredSize.Height, 0);
                storyboard.Begin();
                storyboard.Completed += OpenAnimationStoryboard_Completed;
            });
        });
    }

    public void Hide()
    {
        if (RootGrid is null || _isPopupAnimationPlaying)
        {
            return;
        }

        _isPopupAnimationPlaying = true;

        var storyboard = AnimatedHelpers.GetWindows11TopToBottomTransitionStoryboard(RootGrid, 0, (int)DesiredSize.Height);
        storyboard.Begin();
        storyboard.Completed += CloseAnimationStoryboard_Completed;
    }

    private void Content_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateBackdropVisual();
        UpdateFlyoutRegion();
    }

    private void UpdateBackdropVisual()
    {
        if (BackdropTargetGrid is null || _backdropLink is null || GridContentPresenter is null)
        {
            return;
        }

        var r = (float)(IslandsGrid.CornerRadius.TopLeft - 1);
        var compositor = _backdropLink.PlacementVisual.Compositor;

        _backdropLink.PlacementVisual.Size = new((float)IslandsGrid.ActualWidth, (float)IslandsGrid.ActualHeight);
        _backdropLink.PlacementVisual.Clip = compositor.CreateRectangleClip(
            0,
            0,
            (float)GridContentPresenter.ActualWidth,
            (float)GridContentPresenter.ActualHeight,
            new(Convert.ToSingle(BackdropTargetGrid.CornerRadius.TopLeft - 1), Convert.ToSingle(BackdropTargetGrid.CornerRadius.TopLeft - 1)),
            new(Convert.ToSingle(BackdropTargetGrid.CornerRadius.TopRight - 1), Convert.ToSingle(BackdropTargetGrid.CornerRadius.TopRight - 1)),
            new(Convert.ToSingle(BackdropTargetGrid.CornerRadius.BottomRight - 1), Convert.ToSingle(BackdropTargetGrid.CornerRadius.BottomRight - 1)),
            new(Convert.ToSingle(BackdropTargetGrid.CornerRadius.BottomLeft - 1), Convert.ToSingle(BackdropTargetGrid.CornerRadius.BottomLeft - 1))
        );

        ElementCompositionPreview.SetElementChildVisual(BackdropTargetGrid, _backdropLink.PlacementVisual);
    }

    private void UpdateFlyoutTheme()
    {
        RequestedTheme = SystemTheme.GetCurrentSystemTheme() == ElementTheme.Light ? ElementTheme.Light : ElementTheme.Dark;
    }

    private void UpdateFlyoutRegion()
    {
        if (_host?.DesktopWindowXamlSource is null || IslandsGrid is null)
        {
            return;
        }

        var flyoutWidth = DesiredSize.Width * _host.XamlIslandRasterizationScale;
        var flyoutHeight = DesiredSize.Height * _host.XamlIslandRasterizationScale;

        _host?.SetHWndRectRegion(
            new((int)((_host.WindowSize.Width - flyoutWidth - 32) / 2), (int)(_host.WindowSize.Height - flyoutHeight), (int)_host.WindowSize.Width + 32, (int)_host.WindowSize.Height)
        );
    }

    private void OpenAnimationStoryboard_Completed(object? sender, object e)
    {
        if (sender is not Storyboard storyboard)
        {
            return;
        }

        storyboard.Completed -= OpenAnimationStoryboard_Completed;
        _isPopupAnimationPlaying = false;
        IsOpen = true;
    }

    private void CloseAnimationStoryboard_Completed(object? sender, object e)
    {
        if (sender is not Storyboard storyboard)
        {
            return;
        }

        storyboard.Completed -= CloseAnimationStoryboard_Completed;
        _isPopupAnimationPlaying = false;
        IsOpen = false;
        _host?.UpdateWindowVisibility(false);
    }

    partial void OnRootContentChanged(UIElement? newValue)
    {
        if (newValue is null || newValue is not FrameworkElement newContent)
        {
            return;
        }

        GridContentPresenter.Content = newContent;

        if (GridContentPresenter.Content is FrameworkElement oldContent)
        {
            oldContent.SizeChanged -= Content_SizeChanged;
        }

        newContent.SizeChanged += Content_SizeChanged;
    }

    public void Dispose()
    {
        _host?.Dispose();
    }
}
