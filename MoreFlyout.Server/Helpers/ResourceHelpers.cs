using Microsoft.UI.Xaml.Markup;
using Microsoft.Windows.ApplicationModel.Resources;

namespace MoreFlyout.Server.Helpers;

[MarkupExtensionReturnType(ReturnType = typeof(string))]
public sealed partial class ResourceString : MarkupExtension
{
    private static readonly ResourceLoader ResourceLoader = new();

    public string Name { get; set; } = string.Empty;

    protected override object ProvideValue() => ResourceLoader.GetString(Name);
}
