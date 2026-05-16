namespace TimberLive.Components.Pages.Buildings;

sealed partial class BuildingDetails : IDisposable
{
    [Parameter]
    public Guid Id { get; set; }

    RefreshableDataFetcher? refresher;
    HttpBuilding? building;

    bool notFound;

    protected override async Task OnInitializedAsync()
    {
        refresher = RefreshableDataFetcher.Create(LoadAsync, Storage);
    }

    async Task LoadAsync()
    {
        try
        {
            building = await Api.GetAsync<HttpBuilding>($"buildings/{Uri.EscapeDataString(Id.ToString())}");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            notFound = true;
            refresher!.Pause();
        }
        
        StateHasChanged();
    }

    async Task SelectAsync(bool follow) => await Api.SelectEntityAsync(Id, !follow, follow);

    async Task RenameAsync()
    {
        var name = building!.Name.EntityName;

        var newName = Js.Invoke<string?>("prompt", "Enter the new name: ", name)?.Trim();

        if (string.IsNullOrEmpty(newName) || newName == name) { return; }

        await Api.RenameEntityAsync(Id, newName);
        refresher?.RefreshNow();
    }

    async Task TogglePauseAsync(bool paused)
    {
        var url = $"buildings/{Uri.EscapeDataString(Id.ToString())}/toggle-pause?paused={paused}";
        await Api.GetStringAsync(url);
        refresher?.RefreshNow();
    }

    public void Dispose()
    {
        refresher?.Dispose();
    }
}
