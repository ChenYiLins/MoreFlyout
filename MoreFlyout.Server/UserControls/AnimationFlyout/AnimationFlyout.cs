using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using MoreFlyout.Server.Helpers;
using MoreFlyout.Server.Utils;

namespace MoreFlyout.Server.UserControls.AnimationFlyout;

[ContentProperty(Name = nameof(Islands))]
public partial class AnimationFlyout : Control, IDisposable
{
    private const string PART_RootGrid = "PART_RootGrid";
    private const string PART_IslandsGrid = "PART_IslandsGrid";

    private readonly XamlIslandHostWindow? _host;
    private bool? _wasTaskbarLightLastTimeChecked;
    private bool? _wasTaskbarColorPrevalenceLastTimeChecked;
    private bool _isPopupAnimationPlaying;

    private Grid? RootGrid;
    private Grid? IslandsGrid;

    internal ContentBackdropManager? BackdropManager { get; private set; }

    public bool IsOpen { get; private set; }

    public AnimationFlyout()
    {
        DefaultStyleKey = typeof(AnimationFlyout);

        _host = new XamlIslandHostWindow();
        _host.SetContent(this);
        _host.UpdateWindowVisibility(false);
        _host.WindowInactivated += HostWindow_Inactivated;
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        RootGrid = GetTemplateChild(PART_RootGrid) as Grid ?? throw new MissingFieldException($"Could not find {PART_RootGrid} in the given {nameof(AnimationFlyout)}'s style.");
        IslandsGrid = GetTemplateChild(PART_IslandsGrid) as Grid ?? throw new MissingFieldException($"Could not find {PART_IslandsGrid} in the given {nameof(AnimationFlyout)}'s style.");

        UpdateIslands();
    }

    public void Show()
    {
        if (_host?.DesktopWindowXamlSource is null || RootGrid is null || _isPopupAnimationPlaying)
            return;

        _isPopupAnimationPlaying = true;
        _host.Maximize();

        _ = Task.Run(async () =>
        {
            RootGrid.DispatcherQueue.TryEnqueue(async () =>
            {
                UpdateLayout();
                await Task.Delay(1);

                UpdateFlyoutTheme();
                UpdateBackdropManager();
                UpdateFlyoutRegion();

                // Ensure to hide first
                if (RootGrid.RenderTransform is TranslateTransform translateTransform)
                {
                    if (PopupDirection is Orientation.Vertical)
                        translateTransform.Y = DesiredSize.Height;
                    else
                        translateTransform.X = DesiredSize.Width;
                }

                UpdateLayout();
                await Task.Delay(1);

                _host.UpdateWindowVisibility(true);

                if (IsTransitionAnimationEnabled)
                {
                    var storyboard =
                        PopupDirection is Orientation.Vertical
                            ? AnimatedHelpers.GetWindows11BottomToTopTransitionStoryboard(RootGrid, (int)DesiredSize.Height, 0)
                            : AnimatedHelpers.GetWindows11RightToLeftTransitionStoryboard(RootGrid, (int)DesiredSize.Width, 0);
                    storyboard.Begin();
                    storyboard.Completed += OpenAnimationStoryboard_Completed;
                }
            });
        });
    }

    public void Hide()
    {
        if (RootGrid is null || _isPopupAnimationPlaying)
            return;

        _isPopupAnimationPlaying = true;

        if (IsTransitionAnimationEnabled)
        {
            var storyboard =
                PopupDirection is Orientation.Vertical
                    ? AnimatedHelpers.GetWindows11TopToBottomTransitionStoryboard(RootGrid, 0, (int)DesiredSize.Height)
                    : AnimatedHelpers.GetWindows11LeftToRightTransitionStoryboard(RootGrid, 0, (int)DesiredSize.Width);
            storyboard.Begin();
            storyboard.Completed += CloseAnimationStoryboard_Completed;
        }
    }

    private void UpdateBackdropManager(bool coerce = false)
    {
        var isTaskbarLight = SystemTheme.GetCurrentSystemTheme() == ElementTheme.Light;
        var isTaskbarColorPrevalence = false;
        bool shouldUpdateBackdrop = _wasTaskbarLightLastTimeChecked != isTaskbarLight || _wasTaskbarColorPrevalenceLastTimeChecked != isTaskbarColorPrevalence;
        _wasTaskbarLightLastTimeChecked = isTaskbarLight;
        _wasTaskbarColorPrevalenceLastTimeChecked = isTaskbarColorPrevalence;

        if (!shouldUpdateBackdrop && !coerce)
            return;

        var controller = BackdropControllerHelpers.CreateController(BackdropKind, isTaskbarLight, isTaskbarColorPrevalence, Resources);
        if (controller is null)
            return;

        BackdropManager?.Dispose();
        BackdropManager = null;
        BackdropManager = ContentBackdropManager.Create(controller, ElementCompositionPreview.GetElementVisual(IslandsGrid).Compositor, ActualTheme);

        UpdateBackdrop(true);
    }

    private void UpdateBackdrop(bool coerce = false)
    {
        foreach (var island in Islands)
            island.UpdateBackdrop(IsBackdropEnabled, coerce);
    }

    private void UpdateFlyoutTheme()
    {
        var theme = SystemTheme.GetCurrentSystemTheme() == ElementTheme.Light ? ElementTheme.Light : ElementTheme.Dark;
        foreach (var island in Islands)
            island.RequestedTheme = theme;
    }

    private void UpdateIslands()
    {
        if (IslandsGrid is null)
            return;

        IslandsGrid.Children.Clear();
        IslandsGrid.RowDefinitions.Clear();
        IslandsGrid.ColumnDefinitions.Clear();

        if (IslandsOrientation is Orientation.Vertical)
        {
            for (int index = 0; index < Islands.Count; index++)
            {
                if (Islands[index] is not AnimationFlyoutIsland island)
                    continue;

                IslandsGrid.RowDefinitions.Add(new() { Height = GridLength.Auto });
                Grid.SetRow(island, index);
                Grid.SetColumn(island, 0);
                island.SetOwner(this);
                IslandsGrid.Children.Add(island);
            }
        }
        else
        {
            for (int index = 0; index < Islands.Count; index++)
            {
                if (Islands[index] is not AnimationFlyoutIsland island)
                    continue;

                IslandsGrid.ColumnDefinitions.Add(new() { Width = GridLength.Auto });
                Grid.SetRow(island, 0);
                Grid.SetColumn(island, index);
                island.SetOwner(this);
                IslandsGrid.Children.Add(island);
            }
        }
    }

    private void UpdateFlyoutRegion()
    {
        if (_host?.DesktopWindowXamlSource is null || IslandsGrid is null)
            return;

        var flyoutWidth = DesiredSize.Width * _host.XamlIslandRasterizationScale;
        var flyoutHeight = DesiredSize.Height * _host.XamlIslandRasterizationScale;

        _host?.SetHWndRectRegion(new((int)((_host.WindowSize.Width - flyoutWidth - 12) / 2), (int)(_host.WindowSize.Height - flyoutHeight), (int)_host.WindowSize.Width, (int)_host.WindowSize.Height));
    }

    private void OpenAnimationStoryboard_Completed(object? sender, object e)
    {
        if (sender is not Storyboard storyboard)
            return;

        storyboard.Completed -= OpenAnimationStoryboard_Completed;
        _isPopupAnimationPlaying = false;
        IsOpen = true;
    }

    private void CloseAnimationStoryboard_Completed(object? sender, object e)
    {
        if (sender is not Storyboard storyboard)
            return;

        storyboard.Completed -= CloseAnimationStoryboard_Completed;
        _isPopupAnimationPlaying = false;
        IsOpen = false;
        _host?.UpdateWindowVisibility(false);
    }

    private void HostWindow_Inactivated(object? sender, EventArgs e)
    {
        if (HideOnLostFocus)
            Hide();
    }

    public void Dispose()
    {
        BackdropManager?.Dispose();
        _host!.WindowInactivated -= HostWindow_Inactivated;
        _host?.Dispose();
    }
}
