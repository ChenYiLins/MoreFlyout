using Microsoft.Windows.ApplicationModel.Resources;

namespace MoreFlyout.App.Helpers;

public static class ResourceExtensions
{
    private static readonly ResourceLoader _ResourceLoader = new();

    public static string GetLocalized(this string resourceKey)
    {
        try
        {
            return _ResourceLoader.GetString(resourceKey);
        }
        catch (Exception)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load resource string for key {resourceKey}");
            return $"[Resource not found: {resourceKey}]";
        }
    }
}
