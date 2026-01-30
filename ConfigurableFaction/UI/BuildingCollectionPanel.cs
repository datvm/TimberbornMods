namespace ConfigurableFaction.UI;

[BindTransient(Contexts = BindAttributeContext.MainMenu)]
public class BuildingCollectionPanel(ILoc t, DataAggregatorService aggregator) : CollapsiblePanel, IEntryCollectionPanel
{
    public ImmutableArray<TemplateEntryElement> Entries { get; private set; } = [];
    IEnumerable<SettingEntryElement> IEntryCollectionPanel.Entries => Entries;

    public void Initialize(IEnumerable<IGrouping<BlockObjectToolGroupSpec, EffectiveEntry>> groupedEntries)
    {
        SetTitle(t.T("LV.CF.Buildings").Bold());
        SetExpand(false);

        List<TemplateEntryElement> entries = [];
        foreach (var grp in groupedEntries)
        {
            var grpInfo = grp.Key;
            
            var grpEl = new CollapsiblePanel().SetMarginBottom();
            grpEl.SetExpand(false);

            var header = grpEl.HeaderLabel.SetAsRow().AlignItems();
            header.AddImage(grpInfo.Icon.Asset).SetSize(24).SetMarginRight(5);
            header.AddGameLabel(t.T(grpInfo.NameLocKey).Bold());

            Container.Add(grpEl);
            
            foreach (var entry in grp)
            {
                var el = new TemplateEntryElement(entry, aggregator, t).SetMarginBottom(10);
                grpEl.Container.Add(el);
                entries.Add(el);
            }
        }
        Entries = [.. entries];
    }

}
