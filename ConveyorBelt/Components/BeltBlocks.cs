namespace ConveyorBelt.Components;

static class BeltBlocks
{
    public static bool HostsBelt(BaseComponent host)
        => host.GetComponent<BeltCarrier>()
            || host.GetComponent<BeltMerger>()
            || host.GetComponent<BeltSplitter>()
            || host.GetComponent<BeltLift>();
}
