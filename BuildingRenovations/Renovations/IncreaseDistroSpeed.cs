namespace BuildingRenovations.Renovations;

public abstract class IncreaseDistroSpeed(string id, string? requiredId = null) : RenovationBase
{
    public override string Id => id;

    public override bool CanRenovate(BuildingRenovationComponent building)
        => building.GetComponent<StockpileDistroSender>();

    public override string? GetUnavailableReason(BuildingRenovationComponent building)
        => building.Service.GetRequiredRenovationIdReason(building, requiredId);

    public override void OnCompleted(BuildingRenovationComponent building, bool isLoad)
    {
        var sender = building.GetComponent<StockpileDistroSender>();
        if (!sender) { return; }

        // Parameters[0] = transfer-rate multiplier delta (e.g. 0.5 => +50% items/hour).
        // Timer multiplies hours-per-item, so invert: hoursMultiplier = 1 / (1 + delta).
        var rateDelta = Spec.Parameters[0];
        var hoursMultiplier = 1f / (1f + rateDelta);

        var id = "Renovation_" + Id;
        sender.Timer.SetModifier(id, hoursMultiplier);

        building.GetComponent<BonusDescriptionComponent>().AddBonus(new(
            id,
            Spec.Title.Value,
            (t, _) => t.T("LV.BRe.IncreaseDistroSpeedBuffDesc", Spec.Parameters[0])
        ));
    }
}

[BindRenovation]
public class IncreaseDistroSpeed1() : IncreaseDistroSpeed(nameof(IncreaseDistroSpeed1));

[BindRenovation]
public class IncreaseDistroSpeed2() : IncreaseDistroSpeed(nameof(IncreaseDistroSpeed2), nameof(IncreaseDistroSpeed1));
