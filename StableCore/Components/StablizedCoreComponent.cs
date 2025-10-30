namespace StableCore.Components;

public class StablizedCoreComponent : BaseComponent, IAwakableComponent, IBlockObjectDeletionBlocker
{
#nullable disable
    TimedComponentActivator timedComponentActivator;
#nullable enable

    public int ArmingDays => timedComponentActivator._spec.DaysUntilActivation;
    public bool Armed => timedComponentActivator.IsEnabled;

    public bool NoForcedDelete { get; } = false;
    public bool IsStackedDeletionBlocked { get; } = true;
    public bool IsDeletionBlocked { get; } = true;
    public string ReasonLocKey { get; } = "LV.StC.NoDelete";

    public void Awake()
    {
        timedComponentActivator = GetComponent<TimedComponentActivator>();
    }

    public void Arm()
    {
        timedComponentActivator.EnableActivator();
        timedComponentActivator.OnCycleDayStarted(new());
    }

}
