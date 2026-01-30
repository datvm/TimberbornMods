namespace ConfigurableFaction.UI;

[BindTransient(Contexts = BindAttributeContext.MainMenu)]
public class FactionCollectionPanel(
    IContainer container,
    UserSettingsUIControllerScope controllerScope
) : CollapsiblePanel
{
#nullable disable
    public BuildingCollectionPanel Buildings { get; private set; }
    public PlantCollectionPanel Plants { get; private set; }
    public GoodCollectionPanel Goods { get; private set; }
    public NeedCollectionPanel Needs { get; private set; }
#nullable enable

    public IEnumerable<IEntryCollectionPanel> AllPanels => [Buildings, Plants, Goods, Needs];
    public IEnumerable<SettingEntryElement> AllEntries => AllPanels.SelectMany(p => p.Entries);

    public void Initialize(string factionId)
    {
        var controller = controllerScope.Controller;
        var faction = controller.GetFaction(factionId);

        SetTitle(faction.Spec.DisplayName.Value.Bold().Highlight());

        Buildings = container.GetInstance<BuildingCollectionPanel>();
        Buildings.Initialize(controller.GetBuildings(faction));
        RegisterEventHandlers(Buildings, controller.ToggleBuilding);

        Plants = CreatePanel<PlantCollectionPanel, TemplateEntryElement>(controller.GetPlants, controller.TogglePlant);
        Goods = CreatePanel<GoodCollectionPanel, GoodEntryElement>(controller.GetGoods, controller.ToggleGood);
        Needs = CreatePanel<NeedCollectionPanel, NeedEntryElement>(controller.GetNeeds, controller.ToggleNeed);

        foreach (VisualElement p in AllPanels)
        {
            Container.Add(p.SetMarginBottom(10));
        }

        T CreatePanel<T, TEntry>(Func<FactionDef, IEnumerable<EffectiveEntry>> getEntriesFunc, Action<string, bool> handler)
            where T : DefaultEntryCollectionPanel<TEntry>
            where TEntry : SettingEntryElement
        {
            var p = container.GetInstance<T>();
            p.Initialize(getEntriesFunc(faction));
            RegisterEventHandlers(p, handler);
            return p;
        }

        void RegisterEventHandlers(IEntryCollectionPanel panel, Action<string, bool> handler)
        {
            foreach (var e in panel.Entries)
            {
                e.OnToggled += v => handler(e.Entry.Id, v);
            }
        }
    }

    public void UpdateEntriesStates()
    {
        foreach (var e in AllEntries)
        {
            e.UpdateEntryState();
        }
    }

    public void SetFilter(string keyword)
    {
        foreach (var entry in AllEntries)
        {
            entry.SetFilter(keyword);
        }
    }

}
