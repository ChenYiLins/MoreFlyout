using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Content;
using MoreFlyout.Server.Utils;

namespace MoreFlyout.Server.Helpers;

public partial class ContentBackdropHelpers : Microsoft.UI.Xaml.Media.SystemBackdrop
{
    private DesktopAcrylicController? _acrylicController;
    private SystemBackdropConfiguration? _backdropConfiguration;

    protected override void OnTargetConnected(ICompositionSupportsSystemBackdrop connectedTarget, XamlRoot xamlRoot)
    {
        base.OnTargetConnected(connectedTarget, xamlRoot);

        if (_acrylicController is not null)
        {
            throw new Exception("This controller cannot be shared");
        }
        _acrylicController = new DesktopAcrylicController();

        if (connectedTarget is ContentExternalBackdropLink backdropLink)
        {
            backdropLink.ExternalBackdropBorderMode = CompositionBorderMode.Soft;
        }

        _backdropConfiguration = new SystemBackdropConfiguration
        {
            IsInputActive = true,
            Theme = (SystemBackdropTheme)SystemTheme.GetCurrentSystemTheme()
        };

        _acrylicController.SetSystemBackdropConfiguration(_backdropConfiguration);
        _acrylicController.AddSystemBackdropTarget(connectedTarget);
    }

    protected override void OnTargetDisconnected(ICompositionSupportsSystemBackdrop disconnectedTarget)
    {
        base.OnTargetDisconnected(disconnectedTarget);

        _acrylicController?.RemoveSystemBackdropTarget(disconnectedTarget);
        _acrylicController = null;
    }
}
