namespace TimberUi.Services;

public partial class DialogService(
    VisualElementInitializer veInit,
    PanelStack panelStack,
    ILoc t,
    DialogBoxShower diagShower
)
{

    public void Alert(string message, bool localized = false)
    {
        _ = AlertAsync(message, localized);
    }

    public async Task AlertAsync(string message, bool localized = false)
    {
        TaskCompletionSource<bool> tcs = new();
        var builder = CreateDialogWithMessage(message, localized);
        builder.SetConfirmButton(() => tcs.SetResult(true));
        builder.Show();
        await tcs.Task;
    }

    public async Task<bool> ConfirmAsync(
        string message, bool localized = false,
        string? okText = null, string? localizedOkText = null,
        string? cancelText = null, string? localizedCancelText = null
    )
    {
        TaskCompletionSource<bool> tcs = new();
        var builder = CreateDialogWithMessage(message, localized);

        if (localizedOkText is not null)
        {
            okText = t.T(localizedOkText);
        }
        if (string.IsNullOrEmpty(okText))
        {
            builder.SetConfirmButton(() => tcs.SetResult(true));
        }
        else
        {
            builder.SetConfirmButton(() => tcs.SetResult(true), okText);
        }

        if (localizedCancelText is not null)
        {
            cancelText = t.T(localizedCancelText);
        }
        if (string.IsNullOrEmpty(cancelText))
        {
            builder.SetCancelButton(() => tcs.SetResult(false));
        }
        else
        {
            builder.SetCancelButton(() => tcs.SetResult(false), cancelText);
        }

        return await tcs.Task;
    }

    public async Task<string?> PromptAsync(
        string? prompt = null,
        string? defaultValue = null,
        string? title = null
    )
    {
        var diag = new PromptDialogElement(t);
        if (!string.IsNullOrEmpty(title))
        {
            diag.SetTitle(title);
        }
        if (!string.IsNullOrEmpty(prompt))
        {
            diag.SetPrompt(prompt);
        }
        if (!string.IsNullOrEmpty(defaultValue))
        {
            diag.SetValue(defaultValue);
        }

        return await diag.ShowAsync(veInit, panelStack);
    }

    DialogBoxShower.Builder CreateDialogWithMessage(string message, bool localized)
    {
        var builder = diagShower.Create();
        if (localized)
        {
            builder.SetLocalizedMessage(message);
        }
        else
        {
            builder.SetMessage(message);
        }

        return builder;
    }

}
