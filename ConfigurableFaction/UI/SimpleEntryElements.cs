namespace ConfigurableFaction.UI;

public class NeedEntryElement : SettingEntryElement
{
    public NeedEntryElement(EffectiveEntry<NeedDef> entry) : base(entry)
    {
        Keyword = entry.Data.DisplayName;
        this.AddLabel(Keyword);

        Keyword = Keyword.ToLower();
    }
}

public class GoodEntryElement : SettingEntryElement
{

    public GoodEntryElement(EffectiveEntry<GoodDef> entry) : base(entry)
    {
        Keyword = entry.Data.DisplayName;
        this.AddIconSpan().SetContent(entry.Data.GoodSpec.IconSmall.Value, postfixText: Keyword, size: 24);

        Keyword = Keyword.ToLower();
    }

}

public class TemplateEntryElement<T> : SettingEntryElement
    where T : TemplateDefBase
{

    public TemplateEntryElement(EffectiveEntry<T> entry, ILoc t) : base(entry)
    {
        var template = entry.Data;
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