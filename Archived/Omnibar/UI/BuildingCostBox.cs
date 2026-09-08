namespace Omnibar.UI;

public class BuildingCostBox : VisualElement
{

    public ScienceBox ScienceBox { get; private set; }
    public VisualElement MaterialBox { get; private set; }
    public bool HasScience { get; private set; }

    public TodoListBuildingDetails? Building { get; set; }

    public BuildingCostBox()
    {
        var container = this;
        this.SetAsRow().SetWrap().AlignItems();

        ScienceBox = container.AddChild<ScienceBox>().SetMarginRight().SetDisplay(false);
        MaterialBox = container.AddRow().SetWrap().AlignItems();
    }

    public BuildingCostBox SetScience(int? science, int? totalScience)
    {
        if (science is null && totalScience is null)
        {
            ScienceBox.SetDisplay(false);
            HasScience = false;
        }
        else
        {
            ScienceBox.SetScience(science, totalScience);
            ScienceBox.SetDisplay(true);
            HasScience = true;
        }

        return this;
    }

    public BuildingCostBox SetMaterials(IEnumerable<GoodAmountSpec> goods, GoodItemFactory goodItemFactory)
    {
        MaterialBox.Clear();

        foreach (var good in goods)
        {
            MaterialBox.Add(goodItemFactory.Create(good));
        }

        return this;
    }

}
