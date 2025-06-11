namespace ConfigurableFaction.UI;

public class FactionNeedsPanel(FactionOptionsService optionsService, ILoc t) : FactionIdsPanel<NeedSpec>(optionsService, t)
{
    protected override string HeaderLoc { get; } = "LV.CFac.Needs";
    protected override HashSet<string> OptionsList => options.Needs;
    protected override HashSet<string> LockedInList => options.LockedInNeeds;
    protected override HashSet<string> ExistingList => options.ExistingNeeds;

    protected override string GetId(NeedSpec spec) => spec.Id;
    protected override ImmutableArray<NeedSpec> GetSpecs(FactionInfo faction) => faction.Needs;
    protected override string GetText(NeedSpec spec) => spec.DisplayName.Value + (spec.CharacterType == "Bot" ? " (Bot)" : "");

    protected override void OnRowChanged(string id, bool add)
    {
        if (add)
        {
            optionsService.AddNeed(options, id);
        }
        else
        {
            optionsService.RemoveNeed(options, id);
        }
    }
}
