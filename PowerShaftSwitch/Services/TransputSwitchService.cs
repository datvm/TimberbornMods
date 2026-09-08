namespace PowerShaftSwitch.Services;

public class TransputSwitchService : ILoadableSingleton, IUnloadableSingleton
{
    public static TransputSwitchService? Instance { get; private set; }

    public void Load()
    {
        Instance = this;
    }

    public void Unload()
    {
        Instance = null;
    }

    public ImmutableHashSet<PausableBuilding> GetPausedSwitches(MechanicalNode mechNode)
    {
        var nodes = mechNode.Graph?.Nodes;
        if (nodes is null) { return []; }

        HashSet<PausableBuilding> pausedBuildings = [];
        foreach (var node in nodes)
        {
            var pausable = node.GetComponentFast<PausableBuilding>();
            if (pausable && pausable.Paused)
            {
                pausedBuildings.Add(pausable);
            }
        }

        return [.. pausedBuildings];
    }

}
