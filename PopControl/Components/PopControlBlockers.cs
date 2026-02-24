namespace PopControl.Components;

public abstract class BasePopControlBlocker(PopControlService service) : TickableComponent, IPausableComponent, IAwakableComponent
{

    public abstract bool IsBeaver { get; }

#nullable disable
    BlockableObject blockableObject;
    DistrictBuilding districtBuilding;
#nullable enable

    public bool IsBlocking { get; private set; }

    public void Awake()
    {
        blockableObject = GetComponent<BlockableObject>();
        districtBuilding = GetComponent<DistrictBuilding>();
    }

    public override void Tick()
    {
        var shouldBlock = service.ShouldPreventBreeding(districtBuilding, IsBeaver);
        if (shouldBlock == IsBlocking) { return; }

        IsBlocking = shouldBlock;
        if (shouldBlock)
        {
            blockableObject.Block(this);
        }
        else
        {
            blockableObject.Unblock(this);
        }
    }

}

public class BreedingPodPopControlBlocker(PopControlService service) : BasePopControlBlocker(service)
{
    public override bool IsBeaver => true;
}

public class BotManufactoryPopControlBlocker(PopControlService service) : BasePopControlBlocker(service)
{
    public override bool IsBeaver => false;
}