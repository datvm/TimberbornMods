global using Timberborn.TickSystem;
global using Timberborn.ScienceSystem;
global using Timberborn.MechanicalSystem;
global using Bindito.Core;

namespace LateGamePower;

public class ScienceToPowerComponent : TickableComponent
{

    MechanicalNode? mechanicalNode;
    ScienceToPowerService? scienceToPower;

    public bool IsEnabled => mechanicalNode?.IsGenerator == true;

    [Inject]
    public void InjectDependencies(ScienceToPowerService scienceToPower)
    {
        UpdateResources();
    }

    public void Awake()
    {
        UpdateResources();
    }

    void UpdateResources()
    {
        mechanicalNode = GetComponentFast<MechanicalNode>();

        if (mechanicalNode is not null)
        {
            UnityEngine.Debug.Log($"Mechanical Node found: {mechanicalNode.IsGenerator}, I/O: {mechanicalNode.PowerInput} {mechanicalNode.PowerOutput}");
        }
    }

    public override void Tick()
    {
        if (!mechanicalNode) { return; }
    }
}
