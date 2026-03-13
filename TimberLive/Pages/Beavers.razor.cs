namespace TimberLive.Pages;

sealed partial class Beavers : IDisposable
{
    static readonly CharacterType[] AllTypes = [.. Enum.GetValues<CharacterType>().Order()];

    RefreshableDataFetcher? refresher;
    Dictionary<string, ParsedBonusTypeSpec> bonusSpecs = [];

    HttpPopulation? population;
    bool showAdultAging;

    bool IsReady => bonusSpecs.Count > 0;

    protected override async Task OnInitializedAsync()
    {
        bonusSpecs = await Specs.GetSpecsAsync<ParsedBonusTypeSpec>(b => b.Id);

        refresher = RefreshableDataFetcher.Create(FetchAsync, Storage);
        refresher.Start();
    }

    async Task FetchAsync()
    {
        population = await Api.GetAsync<HttpPopulation>("characters");

        StateHasChanged();
    }

    static string GetProgressLabel(CharacterType t) => t switch
    {
        CharacterType.Adult => "Aging: ",
        CharacterType.Child => "Adulthood: ",
        CharacterType.Bot => "Deterioration: ",
        _ => throw new InvalidOperationException($"Unknown character type: {t}")
    };

    async Task SelectAsync(Guid id, bool focus) => await Api.SelectEntityAsync(id, focus, !focus);

    async Task RenameAsync(HttpCharacter c)
    {
        var name = c.Name;

        var newName = Js.Invoke<string?>("prompt", "Enter the new name: ", name)?.Trim();

        if (string.IsNullOrEmpty(newName) || newName == name) { return; }

        await Api.RenameEntityAsync(c.Entity.EntityId, newName);
        refresher?.RefreshNow();
    }

    HttpCharacterBuilding? GetBuilding(Guid? id) => id is null
        ? null
        : population!.RelevantBuildings.GetValueOrDefault(id.Value);

    public void Dispose()
    {
        refresher?.Dispose();
    }
}
