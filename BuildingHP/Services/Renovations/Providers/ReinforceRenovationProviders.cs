namespace BuildingHP.Services.Renovations.Providers;

public abstract class BaseReinforceRenovationProvider(DefaultRenovationProviderDependencies di) : DefaultRenovationProvider(di)
{

    public int ExtraHP => (int)RenovationSpec.Parameters[0];

    public override string? CanRenovate(BuildingRenovationComponent building)
    {
        var reinf = building.GetComponentFast<BuildingReinforcementComponent>();

        return reinf.Delta is null || ExtraHP > reinf.Delta.Value
            ? null
            : t.T("LV.BHP.HigherReinforce");
    }

}

public class Reinforce1RenovationProvider(DefaultRenovationProviderDependencies di) : BaseReinforceRenovationProvider(di)
{
    public override string Id { get; } = "Reinforce1";
}

public class Reinforce2RenovationProvider(DefaultRenovationProviderDependencies di) : BaseReinforceRenovationProvider(di)
{
    public override string Id { get; } = "Reinforce2";
}

public class Reinforce3RenovationProvider(DefaultRenovationProviderDependencies di) : BaseReinforceRenovationProvider(di)
{
    public override string Id { get; } = "Reinforce3";
}
