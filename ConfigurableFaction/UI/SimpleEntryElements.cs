namespace ConfigurableFaction.UI;

public class NeedEntryElement : SettingEntryElement
{
    public NeedEntryElement(EffectiveEntry entry, DataAggregatorService aggregator) : base(entry)
    {
        Keyword = aggregator.Needs.ItemsByIds[Entry.Id].DisplayName.Value;
        this.AddLabel(Keyword);

        Keyword = Keyword.ToLower();
    }
}

public class GoodEntryElement : SettingEntryElement
{

    public GoodEntryElement(EffectiveEntry entry, DataAggregatorService aggregator) : base(entry)
    {
        var g = aggregator.Goods.ItemsByIds[Entry.Id];
        Keyword = g.DisplayName;

        this.AddIconSpan().SetContent(g.GoodSpec.IconSmall.Value, postfixText: Keyword, size: 24);

        Keyword = Keyword.ToLower();
    }

}

public class TemplateEntryElement : SettingEntryElement
{

    public TemplateEntryElement(EffectiveEntry entry, DataAggregatorService aggregator, ILoc t) : base(entry)
    {
        var template = aggregator.Templates.ItemsByIds[Entry.Id];
        Keyword = template.DisplayName;
        this.AddIconSpan().SetContent(template.Sprite, postfixText: Keyword, size: 24);

        var planterGroup = template.PlanterGroup;

        if (planterGroup is not null)
        {
            this.AddGameLabel(t.T("LV.CF.PlanterGroup", planterGroup)).SetMargin(left: 10);
            Keyword += " " + planterGroup;
        }

        Keyword = Keyword.ToLower();
    }

}