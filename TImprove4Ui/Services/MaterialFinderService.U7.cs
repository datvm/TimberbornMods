namespace TImprove4Ui.Services;

public class MaterialFinderService(
    Highlighter highlighter,
    MSettings s,
    DistrictContextService districtContextService,
    EntitySelectionService entitySelectionService,
    InputService inputService
) : ILoadableSingleton, IUnloadableSingleton
{
    const string AlternateKey = "AlternateClickableAction";

    public static MaterialFinderService? Instance { get; private set; }

    public void OnCounterHover(TopBarCounterRow row)
    {
        if (!s.HighlightStorage.Value) { return; }

        HighlightBuildingsWithMaterial(row._goodId);
    }

    public void OnCounterLeft(TopBarCounterRow _)
    {
        if (!s.HighlightStorage.Value) { return; }

        Unhighlight();
    }

    public void OnCounterClicked(TopBarCounterRow row)
    {
        if (!s.HighlightStorage.Value) { return; }

        SelectNextBuildingWithMaterial(row._goodId);
    }

    public void HighlightBuildingsWithMaterial(string goodId)
    {
        foreach (var inventory in inventoryService.PublicOutputInventories)
        {
            if (inventory.OutputGoods.Contains(goodId)
                && inventory.AmountInStock(goodId) > 0)
            {
                highlighter.HighlightSecondary(inventory, s.HighlightSimilarColor.Color);
            }
        }
    }

    public void Unhighlight()
    {
        highlighter.UnhighlightAllSecondary();
    }

    public void SelectNextBuildingWithMaterial(string goodId)
    {
        var alternate = inputService.IsKeyDown(AlternateKey);

        var selectingDistrict = alternate ? districtContextService.SelectedDistrict : null;
        var selectingBuilding = entitySelectionService.SelectedObject?.GameObjectFast;

        Inventory? firstInventory = null;
        bool shouldSelectNext = selectingBuilding is null;

        foreach (var inventory in inventoryService.PublicOutputInventories)
        {
            if (!inventory.OutputGoods.Contains(goodId)
                || !InventoryInDistrict(inventory, selectingDistrict)
                || inventory.AmountInStock(goodId) <= 0) { continue; }

            firstInventory ??= inventory;

            if (shouldSelectNext)
            {
                SelectBuilding(inventory);
                return;
            }

            if (inventory.GameObjectFast == selectingBuilding)
            {
                shouldSelectNext = true;
                continue;
            }
        }

        if (firstInventory is not null)
        {
            SelectBuilding(firstInventory);
        }
    }

    void SelectBuilding(BaseComponent comp) => entitySelectionService.Select(comp);

    static bool InventoryInDistrict(Inventory inventory, DistrictCenter? district) 
        => district is null || inventory.GetComponent<DistrictBuilding>().InstantDistrict == district;

    public void Load()
    {
        Instance = this;
    }

    public void Unload()
    {
        Instance = null;
    }
}