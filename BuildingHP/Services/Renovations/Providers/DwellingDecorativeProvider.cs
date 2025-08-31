namespace BuildingHP.Services.Renovations.Providers;

public class DwellingDecorativeProvider(DefaultRenovationProviderDependencies di) : DefaultRenovationProvider(di)
{
    public const string RenoId = "DwellingDecorative";

    public override string Id { get; } = RenoId;

    public override string? CanRenovate(BuildingRenovationComponent building)
        => ValidateActiveAndComponent<Dwelling>(building, "LV.BHP.NotDwelling");
}
