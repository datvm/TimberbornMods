namespace ConfigurableFaction.Definitions;

public class GoodDef(GoodSpec GoodSpec)
{
    public GoodSpec GoodSpec { get; } = GoodSpec;
    public string Id => GoodSpec.Id;
    public string DisplayName => GoodSpec.DisplayName.Value;

    public FrozenSet<string> RequiredNeeds { get; } = GoodSpec.HasConsumptionEffects
        ? GoodSpec.ConsumptionEffects.Select(e => e.NeedId).ToFrozenSet()
        : [];
}
