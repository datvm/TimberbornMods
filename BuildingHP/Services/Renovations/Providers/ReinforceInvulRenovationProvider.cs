namespace BuildingHP.Services.Renovations.Providers;

public class ReinforceInvulRenovationProvider(DefaultRenovationProviderDependencies di) : DefaultRenovationProvider(di)
{
    public const string RenoId = "ReinforceInvul";

    public override string Id { get; } = RenoId;
    public override string? CanRenovate(BuildingRenovationComponent building) => ValidateActive(building);
}
