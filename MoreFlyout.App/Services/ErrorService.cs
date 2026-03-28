using System.Diagnostics;
using MoreFlyout.App.Contracts.Services;
using MoreFlyout.App.Helpers;

namespace MoreFlyout.App.Services;

public class ErrorService : IErrorService
{
    private readonly Queue<DialogRequest> _dialogQueue = new();
    private bool _isDialogShowing;

    public async Task ShowErrorMessageAsync(Exception ex, XamlRoot xamlRoot, string location, string extraInfo = "")
    {
        var error = "ErrorMessageBox_Content".GetLocalized() + $"\n\nError occurred in: {location}" + $"\nSource: {ex.Source}" + $"\nMessage: {ex.Message}";
        if (extraInfo.Length > 0)
        {
            error += $"\nExtra Detail: {extraInfo}";
        }

        var request = new DialogRequest
        {
            Title = "ErrorOccurred".GetLocalized(),
            Content = error,
            XamlRoot = xamlRoot,
        };

        await EnqueueDialog(request);
    }

    private async Task EnqueueDialog(DialogRequest request)
    {
        _dialogQueue.Enqueue(request);

        if (!_isDialogShowing)
        {
            await ProcessDialogQueue();
        }
    }

    private async Task ProcessDialogQueue()
    {
        while (_dialogQueue.Count > 0)
        {
            _isDialogShowing = true;
            var request = _dialogQueue.Dequeue();

            ContentDialog dialog = new()
            {
                Title = request.Title,
                Content = request.Content,
                DefaultButton = ContentDialogButton.Primary,
                PrimaryButtonText = "Yes",
                CloseButtonText = "No",
                XamlRoot = request.XamlRoot,
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                const string issueUri = @"https://github.com/ChenYiLins/MoreFlyout/issues";
                Process.Start(new ProcessStartInfo(issueUri) { UseShellExecute = true, Verb = "open" });
            }
        }

        _isDialogShowing = false;
    }

    private class DialogRequest
    {
        public required string Title { get; init; }
        public required string Content { get; init; }
        public required XamlRoot XamlRoot { get; init; }
    }
}
