namespace TimberLive.Components;

partial class ApiConnection
{

    [Parameter]
    public bool WasDisconnected { get; set; }

    string url = "";
    string password = "";
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
            err = await Api.ConnectAsync(url, password);
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
