namespace TimberPipes.Components;

[AddTemplateModule2(typeof(BuildingPipe))]
public class BuildingPipePortState : BaseComponent, IAwakableComponent, IFinishedStateListener
{
#nullable disable
    BuildingPipe buildingPipe;
#nullable enable

    BlockableObject? blockable;
    FlowLimitPipe? flowLimit;
    bool isValve;

    public void Awake()
    {
        buildingPipe = GetComponent<BuildingPipe>();
        blockable = this.GetComponentOrNull<BlockableObject>();
        flowLimit = this.GetComponentOrNull<FlowLimitPipe>();
        isValve = HasComponent<ValvePipe>() || HasComponent<DischargePipe>();
    }

    public void RefreshPortStatus()
    {
        if (buildingPipe.Ports is not { } ports)
        {
            return;
        }

        var hasChanged = false;
        var paused = blockable is { IsUnblocked: false } || flowLimit is { IsClosed: true };

        foreach (var p in ports.Values)
        {
            var target = p.PortSpec.State.WithPause(paused, isValve);
            if (p.State == target)
            {
                continue;
            }

            hasChanged = true;
            p.State = target;
        }

        if (!hasChanged)
        {
            return;
        }

        buildingPipe.Graph?.RaisePortChanged(buildingPipe);
    }

    public void OnEnterFinishedState() => RefreshPortStatus();
    public void OnExitFinishedState() { }
}
