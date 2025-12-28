
namespace ConfigurableHousing.Patches.V1;

public class DwellingAdder(MSettings s, FactionService factions) : ITemplateCollectionIdProvider
{
    const string FolktailsId = "Dwellings.Folktails";
    const string IronTeethId = "Dwellings.IronTeeth";

    public IEnumerable<string> GetTemplateCollectionIds()
        => s.AddOtherFaction.Value ? GetOtherFactionsIds() : [];

    public string[] GetOtherFactionsIds() => factions.Current.Id switch
    {
        "Folktails" => [IronTeethId,],
        "IronTeeth" => [FolktailsId,],
        "GreedyBuilders" => [],
        _ => [FolktailsId, IronTeethId,],
    };

}
