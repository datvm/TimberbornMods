namespace Timberborn.CoreUI;

public static class TimberUiDialogExtensions
{

    public static async Task<bool?> ShowAsync(
        this DialogBoxShower diag,
        string message,
        string?[]? buttons = default,
        bool messageLocalized = true
    )
    {
        var builder = diag.Create();

        if (messageLocalized)
        {
            builder.SetLocalizedMessage(message);
        }
        else
        {
            builder.SetMessage(message);
        }

        var tcs = new TaskCompletionSource<bool?>();
        
        buttons ??= [];

        var okText = buttons.ElementAtOrDefault(0);
        builder.SetConfirmButton(() => tcs.SetResult(true), okText);

        if (buttons.Length > 1)
        {
            builder.SetCancelButton(() => tcs.SetResult(false), buttons[1]);
        }

        if (buttons.Length > 2)
        {
            builder.SetInfoButton(() => tcs.SetResult(null), buttons[2]);
        }
        builder.Show();

        return await tcs.Task;
    }

}
