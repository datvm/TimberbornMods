namespace BeavlineLogistics.Renovations;

public class BeavlineOutSpeedRenovationProvider(DefaultRenovationProviderDependencies di) : DefaultRenovationProvider(di)
{
    public const string RenoId = "BeavlineOutSpeed";

    public override string Id { get; } = RenoId;

    public override string? CanRenovate(BuildingRenovationComponent building)
    {
        var err = ValidateActive(building);
        if (err is not null) { return err; }

        var comp = building.GetComponentFast<BeavlineComponent>();
        return (!comp || !comp.HasOutput) ? t.T("LV.BL.ErrNeedOutput") : null;
    }
}
