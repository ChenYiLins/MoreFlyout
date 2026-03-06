using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;

namespace MoreFlyout.Server.Helpers;

internal partial class PersistentAcrylicBackdrop : Microsoft.UI.Xaml.Media.SystemBackdrop
{
    private sealed class ControllerEntry : IDisposable
    {
        public required ICompositionSupportsSystemBackdrop Target { get; init; }
        public required DesktopAcrylicController Controller { get; init; }

        public void Initialize(SystemBackdropConfiguration configuration)
        {
            Controller.AddSystemBackdropTarget(Target);
            Controller.SetSystemBackdropConfiguration(configuration);
        }

        public void Dispose()
        {
            Controller.RemoveSystemBackdropTarget(Target);
            Controller.Dispose();
        }
    }

    private readonly HashSet<ControllerEntry> _controllers = [];

    protected override void OnTargetConnected(ICompositionSupportsSystemBackdrop target, XamlRoot xamlRoot)
    {
        base.OnTargetConnected(target, xamlRoot);

        var configuration = GetDefaultSystemBackdropConfiguration(target, xamlRoot);
        configuration.IsInputActive = true;

        var entry = new ControllerEntry { Target = target, Controller = new DesktopAcrylicController() };
        entry.Initialize(configuration);
        _controllers.Add(entry);
    }

    protected override void OnTargetDisconnected(ICompositionSupportsSystemBackdrop target)
    {
        base.OnTargetDisconnected(target);

        var entry = _controllers.FirstOrDefault(x => x.Target == target);
        if (entry is null)
            return;

        entry.Dispose();
        _controllers.Remove(entry);
    }
}
