
namespace TImprove4UX.Services;

public class CollapsibleEntityPanelService : ISaveableSingleton, ILoadableSingleton, IUnloadableSingleton
{
    public const string DoNotCollapseTag = "DoNotCollapse";
    public const string DoNotCollapseLocValue = "[DO_NOT_COLLAPSE]";

    #region Ignored list
    public static readonly FrozenSet<Type> IgnoredPanels = [
        typeof(StatusListFragment),
        typeof(GoodCarrierFragment),
        typeof(FloodgateFragment),
        typeof(DynamiteFragment),
#if TIMBERU7
        typeof(DemolishablePriorityFragment),
#endif
        typeof(GrowableFragment),
        typeof(GatherableFragment),
        typeof(GatherablePrioritizerFragment),
        typeof(DyingNaturalResourceFragment),
        typeof(ForesterFragment),
        typeof(PlantablePrioritizerFragment),
        typeof(HaulCandidateFragment),
        typeof(ManufactoryFragment),
        typeof(ProductivityFragment),
        typeof(ProductionProgressFragment),
        typeof(ManufactoryTogglableRecipesFragment),
        typeof(RuinFragment),
        typeof(ScienceNeedingBuildingFragment),
        typeof(WaterMoverFragment),
        typeof(WaterSourceRegulatorFragment),
        typeof(RuinModelShufflingFragment),
#if TIMBERV1
        typeof(UnstableCoreFragment),
        typeof(TimedComponentActivatorFragment),
        typeof(BlueprintDebugFragment),
#endif
    ];
    #endregion

    const string SaveKey = "TImprove4UX.CollapsibleEntityPanelService.CollapsedList";

    public static CollapsibleEntityPanelService? Instance { get; private set; }

    readonly Dictionary<VisualElement, string> panelNames = [];

    readonly MSettings s;
    readonly ILoc t;
    readonly EntityPanel entityPanel;

    readonly IListSettingStorage perSave;
    readonly IListSettingStorage globalSave = new GlobalListSettingStorage(SaveKey);
    public IListSettingStorage CurrentStorage => s.CollapseEntityPanelGlobal.Value ? globalSave : perSave;

    public CollapsibleEntityPanelService(
        MSettings s,
        ISingletonLoader loader,
        IEntityPanel entityPanel,
        ILoc t
    )
    {
        Instance = this;
        this.s = s;
        this.t = t;
        this.entityPanel = (EntityPanel)entityPanel;

        perSave = new PerSaveListSettingStorage(SaveKey, loader);
    }

    public void RegisterPanelFragment(VisualElement panel, IEntityPanelFragment fragment)
    {
        var type = fragment.GetType();
        if (IgnoredPanels.Contains(type)
            || type.GetCustomAttributes<DescriptionAttribute>().Any(a => a.Description == DoNotCollapseTag))
        {
            return;
        }


        if (panelNames.ContainsKey(panel))
        {
            Debug.LogWarning($"[TImprove4UX] Duplicate registration of entity panel fragment '{fragment.GetType().Name}'");
        }

        panelNames[panel] = fragment.GetType().Name;
    }

    void OnStorageChanged(object _, bool isGlobal)
    {
        if (isGlobal)
        {
            globalSave.ClearAndImport(perSave.Items);
        }
        else
        {
            perSave.ClearAndImport(globalSave.Items);
        }
    }

    void ChangeFragmentsToCollapsiblePanels()
    {
        var fragments = entityPanel._root.Q("Fragments")
            ?? throw new InvalidOperationException("Failed to find 'Fragments' element in EntityPanel");

        foreach (var fragment in fragments.Children())
        {
            if (fragment.ClassListContains(UiCssClasses.FragmentClass))
            {
                ChangeSubpanelToCollapsiblePanel(fragment, fragment, null);
            }
            else
            {
                var panels = fragment.Query(className: UiCssClasses.FragmentClass).ToList();
                if (panels.Count == 0) { continue; }

                for (int i = 0; i < panels.Count; i++)
                {
                    var p = panels[i];
                    ChangeSubpanelToCollapsiblePanel(fragment, p, i > 0 ? p.name : null);
                }
            }
        }
    }

    void ChangeSubpanelToCollapsiblePanel(VisualElement fragment, VisualElement panel, string? subPanelName)
    {
        // Skip if already a collapsible panel
        if (panel.childCount == 1 && panel.Children().First() is CollapsiblePanel) { return; }

        // Skip if not a known fragment
        if (!panelNames.TryGetValue(fragment, out var panelName)) { return; }

        var key = "Name_" + panelName + (subPanelName is null ? "" : ("_" + subPanelName));
        
        var title = t.T(key);
        if (title == DoNotCollapseLocValue) { return; }

        if (key == title)
        {
            Debug.LogWarning($"[TImprove4UX] Missing localization for entity panel fragment name '{panelName}' (key: '{key}')");
        }

        var isCollapsed = IsCollapsed(panelName);

        var colPanel = new CollapsiblePanel()
            .SetTitle(title);
        colPanel.SetExpandWithoutNotify(!isCollapsed);
        colPanel.ExpandChanged += expand => IsCollapsedChanged(panelName, !expand);

        foreach (var child in panel.Children().ToArray())
        {
            colPanel.Container.Add(child);
        }
        panel.Add(colPanel);
    }

    public bool IsCollapsed(string panel) => CurrentStorage.Contains(panel);

    void IsCollapsedChanged(string name, bool isCollapsed)
    {
        if (isCollapsed)
        {
            CurrentStorage.Add(name);
        }
        else
        {
            CurrentStorage.Remove(name);
        }
    }

    public void Load()
    {
        if (!s.CollapseEntityPanel.Value) { return; }

        perSave.Load();
        globalSave.Load();

        ChangeFragmentsToCollapsiblePanels();
        s.CollapseEntityPanelGlobal.ValueChanged += OnStorageChanged;
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        perSave.Save(singletonSaver);
        globalSave.Save(singletonSaver);
    }

    public void Unload()
    {
        Instance = null;
    }
}
