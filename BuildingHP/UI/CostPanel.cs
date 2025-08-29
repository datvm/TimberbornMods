namespace BuildingHP.UI;

public class CostBox : VisualElement
{

    readonly GoodItemFactory goodItemFactory;
    public VisualElement MaterialBox { get; private set; }

    public CostBox(GoodItemFactory goodItemFactory)
    {
        this.goodItemFactory = goodItemFactory;

        this.SetAsRow().SetWrap().AlignItems();
        MaterialBox = this.AddRow().SetWrap().AlignItems();
    }

    public CostBox SetMaterials(IEnumerable<GoodAmountSpecNew> goods)
    {
        return SetMaterials(goods.Select(q => new GoodAmountSpec()
        {
            _goodId = q.Id,
            _amount = q.Amount,
        }));
    }

    public CostBox SetMaterials(IEnumerable<GoodAmountSpec> goods)
    {
        MaterialBox.Clear();

        foreach (var good in goods)
        {
            MaterialBox.Add(goodItemFactory.Create(good));
        }

        return this;
    }

}
