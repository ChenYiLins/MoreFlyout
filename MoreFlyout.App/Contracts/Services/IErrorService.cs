namespace MoreFlyout.App.Contracts.Services;

public interface IErrorService
{
    Task ShowErrorMessageAsync(Exception ex, XamlRoot xamlRoot, string location, string extraInfo = "");
}
