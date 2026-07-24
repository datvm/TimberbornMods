namespace StockpileBalancer.Services;

public enum DistroBalancerConnectionMode
{
    None = 0,
    HorizontalSide = 1,
    AnyTouch = 2,
}

public abstract class DistroBalancer(
    string id,
    DistroBalancerConnectionMode mode,
    string? requiredId = null
) : RenovationBase
{
    public override string Id => id;

    public override bool CanRenovate(BuildingRenovationComponent building)
        => building.GetComponent<StockpileBalancerComponent>();

    public override string? GetUnavailableReason(BuildingRenovationComponent building)
        => building.Service.GetRequiredRenovationIdReason(building, requiredId);

    public override void OnCompleted(BuildingRenovationComponent building, bool isLoad)
        => building.GetComponent<StockpileBalancerComponent>().SetConnectionMode(mode);
}

[BindRenovation]
public class DistroBalancer1() : DistroBalancer(nameof(DistroBalancer1), DistroBalancerConnectionMode.HorizontalSide);

[BindRenovation]
public class DistroBalancer2() : DistroBalancer(nameof(DistroBalancer2), DistroBalancerConnectionMode.AnyTouch, nameof(DistroBalancer1));