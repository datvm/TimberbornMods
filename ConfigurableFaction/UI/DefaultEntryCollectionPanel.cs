namespace ConfigurableFaction.UI;

public abstract class DefaultEntryCollectionPanel<TEntry, T>(DataAggregatorService aggregator, ILoc t) : CollapsiblePanel, IEntryCollectionPanel
    where TEntry : SettingEntryElement
{
    protected readonly DataAggregatorService aggregator = aggregator;

    protected abstract string TitleLoc { get; }
    protected abstract TEntry CreateEntryElement(EffectiveEntry<T> entry);

    public ImmutableArray<TEntry> Entries { get; private set; } = [];
    IEnumerable<SettingEntryElement> IEntryCollectionPanel.Entries => Entries;

    public void Initialize(IEnumerable<EffectiveEntry<T>> entity)
    {
        SetTitle(t.T(TitleLoc).Bold());
        SetExpand(false);

        List<TEntry> entries = [];
        foreach (var e in entity)
        {
            var entry = CreateEntryElement(e);
            entries.Add(entry);
            Container.Add(entry);
        }

        Entries = [.. entries];
    }
}

[BindTransient(Contexts = BindAttributeContext.MainMenu)]
public class GoodCollectionPanel(DataAggregatorService aggregator, ILoc t) : DefaultEntryCollectionPanel<GoodEntryElement, GoodDef>(aggregator, t)
{
    protected override string TitleLoc => "LV.CF.Goods";
    protected override GoodEntryElement CreateEntryElement(EffectiveEntry<GoodDef> entry) => new(entry);
}

[BindTransient(Contexts = BindAttributeContext.MainMenu)]
public class NeedCollectionPanel(DataAggregatorService aggregator, ILoc t) : DefaultEntryCollectionPanel<NeedEntryElement, NeedDef>(aggregator, t)
{
    protected override string TitleLoc => "LV.CF.Needs";
    protected override NeedEntryElement CreateEntryElement(EffectiveEntry<NeedDef> entry) => new(entry);
}

[BindTransient(Contexts = BindAttributeContext.MainMenu)]
public class PlantCollectionPanel(DataAggregatorService aggregator, ILoc t) : DefaultEntryCollectionPanel<TemplateEntryElement<PlantDef>, PlantDef>(aggregator, t)
{
    readonly ILoc t = t;
    protected override string TitleLoc => "LV.CF.Plants";
    protected override TemplateEntryElement<PlantDef> CreateEntryElement(EffectiveEntry<PlantDef> entry) => new(entry, t);
}