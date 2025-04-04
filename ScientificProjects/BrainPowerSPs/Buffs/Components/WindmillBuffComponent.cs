namespace BrainPowerSPs.Buffs.Components;

public class WindmillBuffComponent : BaseComponent, IFinishedStateListener
{

    public int PeakHeight { get; private set; }

    public void OnEnterFinishedState()
    {
        var block = GetComponentFast<BlockObject>();
        var blockObjSpec = GetComponentFast<BlockObjectSpec>();

        if (!block || !blockObjSpec)
        {
            Debug.LogWarning($"{nameof(WindmillBuffComponent)} requires a {nameof(BlockObject)} and {nameof(BlockObjectSpec)} components.");
            return;
        }

        PeakHeight = block.Coordinates.z + blockObjSpec.BlocksSpec.Size.z;
    }

    public void OnExitFinishedState()
    {
        PeakHeight = 0;
    }

}
