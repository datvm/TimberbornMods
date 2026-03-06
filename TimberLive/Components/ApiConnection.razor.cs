namespace TimberLive.Components;

partial class ApiConnection
{

    [Parameter]
    public bool WasDisconnected { get; set; }

    string url = "";
    public string Url => url;

    bool connecting;
    string? err;

    protected override void OnInitialized()
    {
        url = Api.CurrentUri.AbsoluteUri;
    }

    async Task ConnectAsync()
    {
        if (connecting) { return; }

        connecting = true;
        StateHasChanged();

        try
        {
            err = await Api.ConnectAsync(url);
        }
        catch (Exception ex)
        {
            err = ex.Message;
        }
        finally
        {
            connecting = false;
            StateHasChanged();
        }
    }

}
