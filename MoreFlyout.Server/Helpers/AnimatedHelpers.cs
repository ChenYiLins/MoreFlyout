using Microsoft.UI.Xaml.Media.Animation;

namespace MoreFlyout.Server.Helpers;

internal static class AnimatedHelpers
{
    private static Storyboard CreateStoryboard(DependencyObject target, string property, int from, int to, double durationMs, double cp1x, double cp1y, double cp2x, double cp2y)
    {
        var storyboard = new Storyboard();
        var keyFrames = new DoubleAnimationUsingKeyFrames() { EnableDependentAnimation = true };

        keyFrames.KeyFrames.Add(new DiscreteDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero), Value = from });
        keyFrames.KeyFrames.Add(
            new SplineDoubleKeyFrame()
            {
                KeySpline = new() { ControlPoint1 = new(cp1x, cp1y), ControlPoint2 = new(cp2x, cp2y) },
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(durationMs)),
                Value = to,
            }
        );

        Storyboard.SetTarget(keyFrames, target);
        Storyboard.SetTargetProperty(keyFrames, property);
        storyboard.Children.Add(keyFrames);
        return storyboard;
    }

    internal static Storyboard GetWindows11BottomToTopTransitionStoryboard(DependencyObject target, int from, int to) =>
        CreateStoryboard(target, "(UIElement.RenderTransform).(TranslateTransform.Y)", from, to, 267, 0.1, 0.9, 0.4, 1.0);

    internal static Storyboard GetWindows11TopToBottomTransitionStoryboard(DependencyObject target, int from, int to) =>
        CreateStoryboard(target, "(UIElement.RenderTransform).(TranslateTransform.Y)", from, to, 200, 0.2, 0.0, 0.9, 0.0);

    internal static Storyboard GetWindows11RightToLeftTransitionStoryboard(DependencyObject target, int from, int to) =>
        CreateStoryboard(target, "(UIElement.RenderTransform).(TranslateTransform.X)", from, to, 167, 0.1, 0.9, 0.4, 1.0);

    internal static Storyboard GetWindows11LeftToRightTransitionStoryboard(DependencyObject target, int from, int to) =>
        CreateStoryboard(target, "(UIElement.RenderTransform).(TranslateTransform.X)", from, to, 167, 0.2, 0.0, 0.9, 0.0);
}
