using System.Numerics;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Hosting;

namespace MoreFlyout.Server.Helpers;

public static class AnimatedPopupHelpers
{
    public static void OpenPopupWithAnimation(Popup parentPopup, FrameworkElement popupContent)
    {
        parentPopup.IsOpen = true;

        ElementCompositionPreview.SetIsTranslationEnabled(parentPopup, true);

        var visual = ElementCompositionPreview.GetElementVisual(parentPopup);
        var compositor = visual.Compositor;

        var currentTranslation = popupContent.Translation;
        popupContent.Translation = new Vector3(currentTranslation.X, 0f, currentTranslation.Z);
        visual.Opacity = 0f;

        var easing = compositor.CreateCubicBezierEasingFunction(new Vector2(0.1f, 0.9f), new Vector2(0.2f, 1f));

        var offsetAnim = compositor.CreateScalarKeyFrameAnimation();
        offsetAnim.InsertKeyFrame(1.0f, 0f, easing);
        offsetAnim.Duration = TimeSpan.FromMilliseconds(400);

        var opacityAnim = compositor.CreateScalarKeyFrameAnimation();
        opacityAnim.InsertKeyFrame(1.0f, 1.0f, easing);
        opacityAnim.Duration = TimeSpan.FromMilliseconds(300);

        visual.StartAnimation("Translation.Y", offsetAnim);
        visual.StartAnimation("Opacity", opacityAnim);
    }

    public static void HidePopupWithAnimation(Popup parentPopup, FrameworkElement popupContent)
    {
        if (!parentPopup.IsOpen) return;

        var visual = ElementCompositionPreview.GetElementVisual(parentPopup);
        var compositor = visual.Compositor;
        var easing = compositor.CreateCubicBezierEasingFunction(new Vector2(0.7f, 0f), new Vector2(0.8f, 0.1f));

        var offsetAnim = compositor.CreateScalarKeyFrameAnimation();
        offsetAnim.InsertKeyFrame(1.0f, 40f, easing);
        offsetAnim.Duration = TimeSpan.FromMilliseconds(250);

        var opacityAnim = compositor.CreateScalarKeyFrameAnimation();
        opacityAnim.InsertKeyFrame(1.0f, 0f, easing);
        opacityAnim.Duration = TimeSpan.FromMilliseconds(200);

        var batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
        visual.StartAnimation("Translation.Y", offsetAnim);
        visual.StartAnimation("Opacity", opacityAnim);
        batch.End();

        batch.Completed += (s, e) =>
        {
            parentPopup.IsOpen = false;

            var currentTranslation = popupContent.Translation;
            popupContent.Translation = new Vector3(currentTranslation.X, 0f, currentTranslation.Z);
            visual.Opacity = 1.0f;
        };
    }
}
