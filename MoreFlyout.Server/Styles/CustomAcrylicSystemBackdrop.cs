using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;

namespace MoreFlyout.Server.Styles;

public partial class AcrylicSystemBackdrop : Microsoft.UI.Xaml.Media.SystemBackdrop
{
    private DesktopAcrylicController? acrylicController;

    protected override void OnTargetConnected(ICompositionSupportsSystemBackdrop connectedTarget, XamlRoot xamlRoot)
    {
        // Call the base method to initialize the default configuration object.
        base.OnTargetConnected(connectedTarget, xamlRoot);

        // This example does not support sharing MicaSystemBackdrop instances.
        if (acrylicController is not null)
        {
            throw new Exception("This controller cannot be shared");
        }

        acrylicController = new DesktopAcrylicController();

        // Set configuration.
        var defaultConfig = GetDefaultSystemBackdropConfiguration(connectedTarget, xamlRoot);
        defaultConfig.IsInputActive = true;
        acrylicController.SetSystemBackdropConfiguration(defaultConfig);

        // Add target.
        acrylicController.AddSystemBackdropTarget(connectedTarget);
    }

    protected override void OnTargetDisconnected(ICompositionSupportsSystemBackdrop disconnectedTarget)
    {
        base.OnTargetDisconnected(disconnectedTarget);

        acrylicController?.RemoveSystemBackdropTarget(disconnectedTarget);
        acrylicController = null;
    }
}