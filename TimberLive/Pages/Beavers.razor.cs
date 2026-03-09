namespace TimberLive.Pages;

partial class Beavers
{
    static readonly CharacterType[] AllTypes = [.. Enum.GetValues<CharacterType>().Order()];

    HttpPopulation? population;

    protected override async Task OnInitializedAsync()
    {
        population = await Api.GetAsync<HttpPopulation>("characters");
    }

    static string GetProgressLabel(CharacterType t) => t switch
    {
        CharacterType.Adult => "Aging",
        CharacterType.Child => "Adulthood",
        CharacterType.Bot => "Deterioration",
        _ => throw new InvalidOperationException($"Unknown character type: {t}")
    };

}
