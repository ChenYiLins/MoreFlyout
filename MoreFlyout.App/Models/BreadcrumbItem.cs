namespace MoreFlyout.App.Models;

public partial class BreadcrumbItem : ObservableObject
{
    public BreadcrumbItem() { }

    public object? Content { get; set; }
    public object? Tag { get; set; }

    [ObservableProperty]
    public partial Thickness BreadcrumbBarTextBlockPadding { get; set; }
}
