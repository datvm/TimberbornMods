namespace ConfigurableFaction.UI;

public abstract class DefaultEntryCollectionPanel<TEntry>(DataAggregatorService aggregator, ILoc t) : CollapsiblePanel, IEntryCollectionPanel
    where TEntry : SettingEntryElement
{
    protected readonly DataAggregatorService aggregator = aggregator;

    protected abstract string TitleLoc { get; }
    protected abstract TEntry CreateEntryElement(EffectiveEntry entry);

    public ImmutableArray<TEntry> Entries { get; private set; } = [];
    IEnumerable<SettingEntryElement> IEntryCollectionPanel.Entries => Entries;

    public void Initialize(IEnumerable<EffectiveEntry> entity)
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
public class GoodCollectionPanel(DataAggregatorService aggregator, ILoc t) : DefaultEntryCollectionPanel<GoodEntryElement>(aggregator, t)
{
    protected override string TitleLoc => "LV.CF.Goods";
    protected override GoodEntryElement CreateEntryElement(EffectiveEntry entry) => new(entry, aggregator);
}

[BindTransient(Contexts = BindAttributeContext.MainMenu)]
public class NeedCollectionPanel(DataAggregatorService aggregator, ILoc t) : DefaultEntryCollectionPanel<NeedEntryElement>(aggregator, t)
{
    protected override string TitleLoc => "LV.CF.Needs";
    protected override NeedEntryElement CreateEntryElement(EffectiveEntry entry) => new(entry, aggregator);
}

[BindTransient(Contexts = BindAttributeContext.MainMenu)]
public class PlantCollectionPanel(DataAggregatorService aggregator, ILoc t) : DefaultEntryCollectionPanel<TemplateEntryElement>(aggregator, t)
{
    protected override string TitleLoc => "LV.CF.Plants";
    protected override TemplateEntryElement CreateEntryElement(EffectiveEntry entry) => new(entry, aggregator, t);
}