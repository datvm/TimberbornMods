namespace PopControl.Components;

public abstract class BasePopControlBlocker : TickableComponent, IPausableComponent
{

    public abstract bool IsBeaver { get; }

#nullable disable
    BlockableBuilding blockableBuilding;
    DistrictBuilding districtBuilding;

    PopControlService service;
#nullable enable

    public bool IsBlocking { get; private set; }

    [Inject]
    public void Inject(PopControlService service)
    {
        this.service = service;
    }

    public void Awake()
    {
        blockableBuilding = GetComponentFast<BlockableBuilding>();
        districtBuilding = GetComponentFast<DistrictBuilding>();
    }

    public override void Tick()
    {
        var shouldBlock = service.ShouldPreventBreeding(districtBuilding, IsBeaver);
        if (shouldBlock == IsBlocking) { return; }

        IsBlocking = shouldBlock;
        if (shouldBlock)
        {
            blockableBuilding.Block(this);
        }
        else
        {
            blockableBuilding.Unblock(this);
        }
    }

}

public class BreedingPodPopControlBlocker : BasePopControlBlocker
{
    public override bool IsBeaver { get; } = true;
}

public class BotManufactoryPopControlBlocker : BasePopControlBlocker
{
    public override bool IsBeaver { get; } = false;
}