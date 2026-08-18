namespace TimberPipes.Components;

[AddTemplateModule2(typeof(BuildingPipe))]
public class BuildingPipePortState : BaseComponent, IAwakableComponent, IFinishedStateListener
{

#nullable disable
    BuildingPipe buildingPipe;
#nullable enable

    PausableBuilding? pausableBuilding;
    MechanicalBuilding? mechBuilding;

    public void Awake()
    {
        buildingPipe = GetComponent<BuildingPipe>();
        pausableBuilding = GetComponent<PausableBuilding>();
        mechBuilding = GetComponent<MechanicalBuilding>();
    }

    public void RefreshPortStatus()
    {
        if (buildingPipe.Ports is not { } ports) { return; }

        var hasChanged = false;
        var shouldCloseAll = buildingPipe
            && (!pausableBuilding || pausableBuilding!.Paused)
            && (!mechBuilding || !mechBuilding!.ActiveAndPowered);

        foreach (var p in ports.Values)
        {
            var target = shouldCloseAll
                ? PipePortState.Closed
                : (p.OverrideState ?? p.PortSpec.State);
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
