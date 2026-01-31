namespace ConfigurableFaction.Models;

public class NeedDef(NeedSpec NeedSpec) : IIdDef
{
    public NeedSpec NeedSpec { get; } = NeedSpec;
    public string Id => NeedSpec.Id;
    public string DisplayName => NeedSpec.DisplayName.Value;

}
