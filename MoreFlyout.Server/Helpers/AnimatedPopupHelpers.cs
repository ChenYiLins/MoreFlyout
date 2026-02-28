using System.Numerics;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Hosting;

namespace MoreFlyout.Server.Helpers;

public static class AnimatedPopupHelpers
{
    public static void OpenPopupWithAnimation(Popup parentPopup,Grid popupContent)
    {
        // 1. 必须先打开，才能获取到 Visual
        parentPopup.IsOpen = true;

        // 2. 获取 Child 的 Visual
        var visual = ElementCompositionPreview.GetElementVisual(popupContent);
        var compositor = visual.Compositor;

        // 3. 立即设置起始状态（向下偏 40px，透明度 0）
        visual.Offset = new System.Numerics.Vector3(0, 40, 0);

        // 4. 定义缓动曲线 (FastOutSlowIn)
        var easing = compositor.CreateCubicBezierEasingFunction(new Vector2(0f, 0f), new Vector2(0.2f, 1f));

        // 5. 创建位移动画
        var offsetAnim = compositor.CreateScalarKeyFrameAnimation();
        offsetAnim.InsertKeyFrame(1.0f, 0f, easing); // 最终回到原位 (0)
        offsetAnim.Duration = TimeSpan.FromMilliseconds(400);

        // 启动动画
        visual.StartAnimation("Offset.Y", offsetAnim);
    }

    public static void HidePopupWithAnimation(Popup parentPopup, Grid popupContent)
    {
        var visual = ElementCompositionPreview.GetElementVisual(popupContent);
        var compositor = visual.Compositor;
        var easing = compositor.CreateCubicBezierEasingFunction(new Vector2(0f, 0f), new Vector2(0.2f, 1f));

        // 创建退出动画：向下位移并消失
        var offsetAnim = compositor.CreateScalarKeyFrameAnimation();
        offsetAnim.InsertKeyFrame(1.0f, 40f, easing); // 向下移动 40 像素
        offsetAnim.Duration = TimeSpan.FromMilliseconds(200);

        // 启动动画组
        var batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
        visual.StartAnimation("Offset.Y", offsetAnim);
        batch.End();

        // 等待动画完成
        batch.Completed += (s, e) =>
        {
            parentPopup.IsOpen = false;

            // 重置状态，以便下次打开
            visual.Offset = new System.Numerics.Vector3(0, 0, 0);
        };
    }
}
