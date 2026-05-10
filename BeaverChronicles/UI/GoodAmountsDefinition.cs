namespace BeaverChronicles.UI;

public readonly record struct GoodAmountsDefinition(ImmutableArray<GoodAmountDefinition> Goods)
{
    public GoodAmountsDefinition(IEnumerable<GoodAmount> goods, GoodModifier modifier)
        : this([.. goods.Select(g => new GoodAmountDefinition(g, modifier))]) { }

    public GoodAmountsDefinition(GoodAmount good, GoodModifier modifier)
        : this([new(good, modifier)]) { }

}

public readonly record struct GoodAmountDefinition(GoodAmount GoodAmount, GoodModifier Modifier);