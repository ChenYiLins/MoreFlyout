using Microsoft.UI.Xaml.Markup;
using Microsoft.Windows.ApplicationModel.Resources;

namespace MoreFlyout.App.Helpers;

[MarkupExtensionReturnType(ReturnType = typeof(string))]
public sealed partial class ResourceString : MarkupExtension
{
    private static readonly ResourceLoader _ResourceLoader = new();

    public string Name { get; set; } = string.Empty;

    protected override object ProvideValue()
    {
        try
        {
            return _ResourceLoader.GetString(Name);
        }
        catch (Exception)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load resource string for key {Name}");
            return $"[Resource not found: {Name}]";
        }
    }
}
