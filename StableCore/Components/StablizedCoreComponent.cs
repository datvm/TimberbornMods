namespace StableCore.Components;

public class StablizedCoreComponent : BaseComponent, IAwakableComponent, IBlockObjectDeletionBlocker
{
#nullable disable
    TimedComponentActivator timedComponentActivator;
    BlockObject blockObject;
#nullable enable

    public bool Finished => blockObject.IsFinished;
    public int ArmingDays => timedComponentActivator._spec.DaysUntilActivation;
    public bool Armed => timedComponentActivator.IsEnabled;

    public bool NoForcedDelete { get; } = false;
    public bool IsStackedDeletionBlocked { get; } = true;
    public bool IsDeletionBlocked { get; } = true;
    public string ReasonLocKey { get; } = "LV.StC.NoDelete";

    public void Awake()
    {
        timedComponentActivator = GetComponent<TimedComponentActivator>();
        blockObject = GetComponent<BlockObject>();
    }

    public void Arm()
    {
        if (!Finished) { return; }

        timedComponentActivator.EnableActivator();
        timedComponentActivator.OnCycleDayStarted(new());
    }

}
