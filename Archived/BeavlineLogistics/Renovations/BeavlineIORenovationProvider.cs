namespace BeavlineLogistics.Renovations;

public abstract class BeavlineIORenovationProvider(DefaultRenovationProviderDependencies di) : DefaultRenovationProvider(di)
{
    public const string RenovationInId = "BeavlineIn";
    public const string RenovationOutId = "BeavlineOut";

    public abstract bool Input { get; }

    public override string? CanRenovate(BuildingRenovationComponent building)
    {
        var active = ValidateActive(building);
        if (active is not null) { return active; }

        var beavline = building.GetComponentFast<BeavlineComponent>();
        if (!beavline) { return t.T("LV.BL.ErrInvalidBuilding"); }

        if (Input && !beavline.CanTakeIn) { return t.T("LV.BL.ErrNoInput"); }
        if (!Input && !beavline.CanGiveOut) { return t.T("LV.BL.ErrNoOutput"); }

        return null;
    }

}

public class BeavlineInRenovationProvider(DefaultRenovationProviderDependencies di) : BeavlineIORenovationProvider(di)
{
    public override string Id { get; } = RenovationInId;
    public override bool Input { get; } = true;
}

public class BeavlineOutRenovationProvider(DefaultRenovationProviderDependencies di) : BeavlineIORenovationProvider(di)
{
    public override string Id { get; } = RenovationOutId;
    public override bool Input { get; } = false;
}