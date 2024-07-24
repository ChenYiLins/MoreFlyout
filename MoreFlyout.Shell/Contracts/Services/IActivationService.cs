namespace MoreFlyout.Shell.Contracts.Services;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
}
