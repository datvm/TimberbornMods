namespace Omnibar.UI.TodoList;

public class TodoListPanelItemBuildingTotal : BuildingCostBox
{

#nullable disable
    TodoListBuildingDetails[] buildings;
    ScienceService scienceService;
#nullable enable

    public TodoListPanelItemBuildingTotal Init(TodoListBuildingDetails[] buildings, GoodItemFactory goodItemFactory, ScienceService scienceService)
    {
        this.buildings = buildings;
        this.scienceService = scienceService;

        var materials = CalculateMaterialCost();
        SetMaterials(materials, goodItemFactory);

        SetScience(0, 0);
        UpdateData();

        return this;
    }

    IEnumerable<GoodAmountSpec> CalculateMaterialCost()
    {
        Dictionary<string, int> costs = [];

        foreach (var building in buildings)
        {
            var quantity = building.Entry.Quantity;
            foreach (var cost in building.BuildingSpec.BuildingCost)
            {
                costs[cost.GoodId] = (costs.TryGetValue(cost.GoodId, out var curr) ? curr : 0)
                    + cost.Amount * quantity;
            }
        }

        return costs.Select(q => new GoodAmountSpec()
        {
            _goodId = q.Key,
            _amount = q.Value,
        });
    }

    int CalculateScienceCost()
    {
        var sum = 0;

        foreach (var b in buildings)
        {
            if (b.Entry.IsLocked())
            {
                sum += b.BuildingSpec.ScienceCost;
            }
        }

        return sum;
    }

    public void UpdateData()
    {
        if (!HasScience) { return; }

        var cost = CalculateScienceCost();
        if (cost == 0)
        {
            SetScience(null, null);
        }
        else
        {
            SetScience(scienceService.SciencePoints, cost);
        }
    }

}
