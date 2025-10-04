namespace ModdableTimberbornDemo.Features.DI;

public class DemoNewItemFactionSpecModifier : BaseSpecModifier<FactionSpec>
{
    protected override IEnumerable<FactionSpec> Modify(IEnumerable<FactionSpec> specs)
    {
        foreach (var spec in specs)
        {
            var goods = spec.Goods;

            if (goods.Contains("Plank") && !goods.Contains("Plank10"))
            {
                Debug.Log("Appending Plank10 to " + spec.Id);

                yield return spec with
                {
                    Goods = [..goods.Append("Plank10")],
                };
            }
        }
    }
}

public class DemoNewItemGoodSpecModifier : BaseSpecModifier<GoodSpec>
{
    protected override IEnumerable<GoodSpec> Modify(IEnumerable<GoodSpec> specs)
    {
        foreach (var good in specs)
        {
            yield return good;

            if (good.Id == "Plank")
            {
                yield return good with
                {
                    Id = "Plank10",
                    DisplayNameLocKey = "Plank x10",
                    DisplayName = new("Plank x10"),
                    PluralDisplayNameLocKey = "Planks x10",
                    PluralDisplayName = new("Planks x10"),                    
                };
            }
        }
    }
}
