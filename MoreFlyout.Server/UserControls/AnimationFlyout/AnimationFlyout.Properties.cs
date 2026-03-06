using System.Collections.ObjectModel;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Controls.Primitives;
using MoreFlyout.Server.Helpers;

namespace MoreFlyout.Server.UserControls.AnimationFlyout;

public partial class AnimationFlyout
{
    private readonly ObservableCollection<AnimationFlyoutIsland> _islands = [];
    public IList<AnimationFlyoutIsland> Islands => _islands;

    [GeneratedDependencyProperty]
    public partial object? IslandsSource { get; set; }

    [GeneratedDependencyProperty(DefaultValue = true)]
    public partial bool IsBackdropEnabled { get; set; }

    [GeneratedDependencyProperty(DefaultValue = Orientation.Vertical)]
    public partial Orientation PopupDirection { get; set; }

    [GeneratedDependencyProperty(DefaultValue = Orientation.Vertical)]
    public partial Orientation IslandsOrientation { get; set; }

    [GeneratedDependencyProperty(DefaultValue = FlyoutPlacementMode.BottomEdgeAlignedRight)]
    public partial FlyoutPlacementMode Placement { get; set; }

    [GeneratedDependencyProperty]
    public partial MenuFlyout? MenuFlyout { get; set; }

    [GeneratedDependencyProperty(DefaultValue = true)]
    public partial bool IsTransitionAnimationEnabled { get; set; }

    [GeneratedDependencyProperty(DefaultValue = true)]
    public partial bool HideOnLostFocus { get; set; }

    [GeneratedDependencyProperty(DefaultValue = BackdropKind.Acrylic)]
    public partial BackdropKind BackdropKind { get; set; }

    partial void OnIslandsSourcePropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not IEnumerable<AnimationFlyoutIsland> newIslands)
            return;

        Islands.Clear();

        foreach (var island in newIslands)
            Islands.Add(island);

        UpdateIslands();
    }

    partial void OnIsBackdropEnabledPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        if ((bool)e.NewValue == (bool)e.OldValue)
            return;

        UpdateBackdropManager(true);
    }

    partial void OnBackdropKindPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        if ((BackdropKind)e.NewValue == (BackdropKind)e.OldValue)
            return;

        UpdateBackdropManager(true);
    }
}
