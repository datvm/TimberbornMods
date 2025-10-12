namespace BeavlineLogistics.Services;

public class BeavlineService(
    BuildingInventoryProvider buildingInventoryProvider,
    BlockService blockService,
    ITimeTriggerFactory timeTriggerFactory
) : ILoadableSingleton
{
    public readonly BuildingInventoryProvider BuildingInventoryProvider = buildingInventoryProvider;
    public readonly ITimeTriggerFactory TimeTriggerFactory = timeTriggerFactory;

    readonly HashSet<BeavlineComponent> beavlines = [];

    public event EventHandler<BeavlineComponent> OnBeavlineRegistered = null!;
    public event EventHandler<BeavlineComponent> OnBeavlineUnregistered = null!;

    public void Load()
    {
        OnBeavlineRegistered += OnAdded;
        OnBeavlineUnregistered += OnRemoved;
    }

    void OnAdded(object sender, BeavlineComponent e)
    {
        var adjacents = FindAdjacentBeavlines(e);

        foreach (var adj in adjacents)
        {
            e.AddConnectedBuilding(adj);
            adj.AddConnectedBuilding(e);
        }
    }

    void OnRemoved(object sender, BeavlineComponent e)
    {
        var adjacents = FindAdjacentBeavlines(e);

        foreach (var adj in adjacents)
        {
            adj.RemoveConnectedBuilding(e);
        }
    }

    IEnumerable<BeavlineComponent> FindAdjacentBeavlines(BeavlineComponent e)
    {
        var blockObjs = blockService.FindAdjacentBlockObjects(e.GetComponentFast<BlockObject>());

        // Find active beavlines
        foreach (var obj in blockObjs)
        {
            var beav = obj.GetComponentFast<BeavlineComponent>();
            if (beav && beav != e && beavlines.Contains(beav))
            {
                yield return beav;
            }
        }
    }

    public void Register(BeavlineComponent comp)
    {
        if (!beavlines.Add(comp)) { return; }
        OnBeavlineRegistered(this, comp);
    }

    public void Unregister(BeavlineComponent comp)
    {
        if (!beavlines.Remove(comp)) { return; }
        OnBeavlineUnregistered(this, comp);
    }

}
