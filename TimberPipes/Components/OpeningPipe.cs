namespace TimberPipes.Components;

[AddTemplateModule2(typeof(BuildingPipe))]
public class BuildingPipePortState : BaseComponent, IAwakableComponent, IFinishedStateListener
{

#nullable disable
    BuildingPipe buildingPipe;
#nullable enable

    PausableBuilding? pausableBuilding;
    bool isValve;

    public void Awake()
    {
        buildingPipe = GetComponent<BuildingPipe>();
        pausableBuilding = this.GetComponentOrNull<PausableBuilding>();
        isValve = HasComponent<ValvePipe>();
    }

    public void RefreshPortStatus()
    {
        if (buildingPipe.Ports is not { } ports) { return; }

        var hasChanged = false;
        var paused = pausableBuilding is { Paused: true };

        foreach (var p in ports.Values)
        {
            var target = (p.OverrideState ?? p.PortSpec.State).WithPause(paused, isValve);
            if (p.State == target) { continue; }

            hasChanged = true;
            p.State = target;
        }

        if (!hasChanged) { return; }
        buildingPipe.Graph?.RaisePortChanged(buildingPipe);
    }

    public void OnEnterFinishedState() => RefreshPortStatus();
    public void OnExitFinishedState() { } // No need closing, they all will be destroyed soon.

}
