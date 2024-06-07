namespace MoreFlyout.Server.Contracts.Services;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
}
