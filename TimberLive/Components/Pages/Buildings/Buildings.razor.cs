namespace TimberLive.Components.Pages.Buildings;

sealed partial class Buildings : IDisposable
{
    RefreshableDataFetcher? refresher;
    HttpGroupedBuildings? buildingsData;

    string filterKeyword = "";

    protected override async Task OnInitializedAsync()
    {
        refresher = RefreshableDataFetcher.Create(FetchAsync, Storage);
        refresher.Start();
    }

    async Task FetchAsync()
    {
        buildingsData = await Api.GetAsync<HttpGroupedBuildings>("buildings");
        StateHasChanged();
    }

    IEnumerable<HttpBuilding> FilteredBuildingsInGroup(HttpGroupedBuildingTemplate group)
    {
        if (string.IsNullOrWhiteSpace(filterKeyword))
        {
            return group.Buildings;
        }

        return group.Buildings.Where(b => b.Name.EntityName.Contains(filterKeyword, StringComparison.OrdinalIgnoreCase));
    }

    async Task SelectAsync(Guid id) => await Api.SelectEntityAsync(id, true);

    async Task TogglePauseAsync(Guid id, bool paused)
    {
        var url = $"buildings/{Uri.EscapeDataString(id.ToString())}/toggle-pause?paused={paused}";
        await Api.GetStringAsync(url);
        refresher?.RefreshNow();
    }

    async Task RenameAsync(HttpBuilding building)
    {
        var name = building.Name.EntityName;

        var newName = Js.Invoke<string?>("prompt", "Enter the new name: ", name)?.Trim();

        if (string.IsNullOrEmpty(newName) || newName == name) { return; }

        await Api.RenameEntityAsync(building.Entity.EntityId, newName);
        refresher?.RefreshNow();
    }

    public void Dispose()
    {
        refresher?.Dispose();
    }
}