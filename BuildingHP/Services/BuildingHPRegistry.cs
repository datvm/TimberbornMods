namespace BuildingHP.Services;

public class BuildingHPRegistry
{

    readonly PriorityList<BuildingRenovation> renoQueue = new(5);
    readonly PriorityList<BuildingRenovationStockpileComponent> stockpiles = new(5);

    public IReadOnlyPriorityList<BuildingRenovation> RenovationQueue => renoQueue;
    public IReadOnlyPriorityList<BuildingRenovationStockpileComponent> Stockpiles => stockpiles;

    public void RegisterRenovation(BuildingRenovation buildingRenovation, Priority priority)
    {
        renoQueue.Add(buildingRenovation, (int)priority);
    }

    public void UnregisterRenovation(BuildingRenovation buildingRenovation)
    {
        renoQueue.Remove(buildingRenovation);
    }

    public void CleanupRenovationMaterialQueue()
    {
        var toRemove = renoQueue.PrioritySortedItems.Where(r => r.IsDone || r.IsGoodAcquired).ToArray();

        foreach (var r in toRemove)
        {
            renoQueue.Remove(r);
        }
    }

    public void RegisterStockpile(BuildingRenovationStockpileComponent stockpileComponent, Priority priority)
    {
        stockpiles.Add(stockpileComponent, (int)priority);
    }

    public void UnregisterStockpile(BuildingRenovationStockpileComponent stockpileComponent)
    {
        stockpiles.Remove(stockpileComponent);
    }

    public int GetPriority(BuildingRenovation repairComponent) => renoQueue.GetPriority(repairComponent);
    public int GetPriority(BuildingRenovationStockpileComponent buildingHPStockpileComponent) => stockpiles.GetPriority(buildingHPStockpileComponent);

    public bool TryGetPriority(BuildingRenovationComponent comp, out int priority, out BuildingRenovation? reno)
        => renoQueue.TryGetPriority(r => r.Building == comp, out priority, out reno);
}
