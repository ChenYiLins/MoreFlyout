namespace MoreFlyout.App.Contracts.Services;

public interface IErrorService
{
    Task ShowErrorMessage(Exception ex, XamlRoot xamlRoot, string location, string extraInfo = "");
}
