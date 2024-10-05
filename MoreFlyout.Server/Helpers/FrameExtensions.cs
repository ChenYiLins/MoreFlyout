using System.Diagnostics.CodeAnalysis;
using Microsoft.UI.Xaml.Controls;

namespace MoreFlyout.Server.Helpers;

[RequiresUnreferencedCode("This functionality is not compatible with trimming.")]
public static class FrameExtensions
{
    public static object? GetPageViewModel(this Frame frame) => frame?.Content?.GetType().GetProperty("ViewModel")?.GetValue(frame.Content, null);
}
