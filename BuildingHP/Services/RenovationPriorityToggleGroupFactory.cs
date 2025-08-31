namespace BuildingHP.Services;

public class RenovationPriorityToggleGroupFactory(
    PriorityToggleGroupFactory priorityToggleGroupFactory,
    BuilderPrioritySpriteLoader builderPrioritySpriteLoader
)
{
    static readonly string DecreaseRenoPriorityKey = "RenovationDecreasePriority";
    static readonly string IncreaseRenoPriorityKey = "RenovationIncreasePriority";

    static readonly string DecreaseStockpilePriorityKey = "RenovationStockpileDecreasePriority";
    static readonly string IncreaseStockpilePriorityKey = "RenovationStockpileIncreasePriority";

    public PriorityToggleGroup CreateForStockpile(VisualElement parent)
        => Create(parent, "LV.BHP.SupplyPriority", DecreaseStockpilePriorityKey, IncreaseStockpilePriorityKey);

    public PriorityToggleGroup CreateForRenovation(VisualElement parent, bool @short = false)
        => Create(parent, @short ? "LV.BHP.RenoPriorityShort" : "LV.BHP.RenoPriority", DecreaseRenoPriorityKey, IncreaseRenoPriorityKey);

    public PriorityToggleGroup CreateForRenovationWithEmptyLabel(VisualElement parent)
        => Create(parent, "Empty", DecreaseRenoPriorityKey, IncreaseRenoPriorityKey);

    public PriorityToggleGroup Create(VisualElement parent, string labelLoc, string decreaseKey, string increaseKey)
        => priorityToggleGroupFactory.Create(parent, labelLoc, builderPrioritySpriteLoader, decreaseKey, increaseKey);

}
