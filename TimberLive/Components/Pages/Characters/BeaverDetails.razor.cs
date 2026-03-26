namespace TimberLive.Components.Pages.Characters;

sealed partial class BeaverDetails : IDisposable
{

    [Parameter]
    public Guid Id { get; set; }

    RefreshableDataFetcher? refresher;
    HttpCharacterDetailed? character;

    bool notFound;
    bool showAdultProgress;

    protected override async Task OnInitializedAsync()
    {
        refresher = RefreshableDataFetcher.Create(LoadAsync, Storage);
    }

    async Task LoadAsync()
    {
        try
        {
            character = await Api.GetAsync<HttpCharacterDetailed>($"characters/{Uri.EscapeDataString(Id.ToString())}");
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
        var name = character!.Basic.Name;

        var newName = Js.Invoke<string?>("prompt", "Enter the new name: ", name)?.Trim();

        if (string.IsNullOrEmpty(newName) || newName == name) { return; }

        await Api.RenameEntityAsync(Id, newName);
        refresher?.RefreshNow();
    }

    HttpCharacterBuilding? GetBuilding(Guid? id) => id is null ? null : character!.Buildings[id.Value];

    public void Dispose()
    {
        refresher?.Dispose();
    }

    int CarryingWeight
    {
        get
        {
            var g = character!.CarryingGood;
            if (g is null) { return 0; }

            return g.Amount * CommonData.Goods[g.Id].Weight;
        }
    }

}
