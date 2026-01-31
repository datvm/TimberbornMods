namespace ConfigurableFaction.Services;

[BindSingleton(Contexts = BindAttributeContext.MainMenu)]
public class UserSettingsUIControllerScope(IContainer container)
{

    UserSettingsUIController? current;

    public UserSettingsUIController Controller => current ??= container.GetInstance<UserSettingsUIController>();
    public void SaveAndUnload()
    {
        Controller.Save();
        current = null;
    }

}

[BindTransient(Contexts = BindAttributeContext.MainMenu)]
public class UserSettingsUIController(
    UserSettingsService userSettingsService,
    DataAggregatorService aggregator
)
{

    public event Action ListChanged = null!;
    public event Action StateChanged = null!;

    public FactionUserSetting Current { get; private set; } = null!;
    public FactionDef GetFaction(string factionId) => aggregator.Factions.ItemsById[factionId];
    public FactionDef CurrentFaction => GetFaction(Current.FactionId);

    public Dictionary<string, EffectiveEntry<BuildingDef>> Buildings = [];
    public Dictionary<string, EffectiveEntry<PlantDef>> Plants = [];
    public Dictionary<string, EffectiveEntry<GoodDef>> Goods = [];
    public Dictionary<string, EffectiveEntry<NeedDef>> Needs = [];

    IEnumerable<EffectiveEntry> AllEntries => [.. Buildings.Values, .. Plants.Values, .. Goods.Values, .. Needs.Values];

    public void Initialize()
    {
        aggregator.Initialize();
    }

    public IGrouping<BlockObjectToolGroupSpec, EffectiveEntry<BuildingDef>>[] GetBuildings(FactionDef faction)
    {
        List<IGrouping<BlockObjectToolGroupSpec, EffectiveEntry<BuildingDef>>> result = [];

        foreach (var grp in faction.BuildingsByGroups)
        {
            result.Add(new Grouping<BlockObjectToolGroupSpec, EffectiveEntry<BuildingDef>>(
                aggregator.ToolGroupsByIds[grp.Key],
                [.. grp.Value.Select(b => Buildings[b.Id])]));
        }

        return [.. result.OrderBy(q => q.Key.Order)];
    }

    public IEnumerable<EffectiveEntry<PlantDef>> GetPlants(FactionDef faction) => GetEntries(faction.Plants, Plants);
    public IEnumerable<EffectiveEntry<GoodDef>> GetGoods(FactionDef faction) => GetEntries(faction.Goods, Goods);
    public IEnumerable<EffectiveEntry<NeedDef>> GetNeeds(FactionDef faction) => GetEntries(faction.Needs, Needs);

    static IEnumerable<TEntry> GetEntries<T, TEntry>(IEnumerable<T> list, Dictionary<string, TEntry> entries)
        where T : IIdDef
        where TEntry : EffectiveEntry
        => list.Select(e => entries[e.Id]);

    public void Import(string path)
    {
        userSettingsService.Import(path);
        Save();
        RebuildLists();
    }

    public void Export(string path)
    {
        Save();
        userSettingsService.Export(path);
    }

    public void Save()
    {
        var curr = Current;
        var clear = curr.Clear;

        SyncList(Buildings, curr.Buildings, aggregator.Templates.CommonBuildingsIds, clear);
        SyncList(Plants, curr.Plants, aggregator.Templates.CommonPlantsIds, clear);
        SyncList(Goods, curr.Goods, aggregator.Goods.CommonItemsIds, clear);
        SyncList(Needs, curr.Needs, aggregator.Needs.CommonItemsIds, clear);

        userSettingsService.Save();

        static void SyncList<TEntry>(Dictionary<string, TEntry> src, HashSet<string> target, HashSet<string> commonIds, bool clear)
            where TEntry : EffectiveEntry
        {
            target.Clear();

            foreach (var item in src.Values)
            {
                if (item.Checked && (!item.IsClearLocked || clear) && !commonIds.Contains(item.Id))
                {
                    target.Add(item.Id);
                }
            }
        }
    }

    public void SetFaction(string id)
    {
        Current = userSettingsService.GetOrAddFaction(id);
        RebuildLists();
    }

    public void DeselectAll()
    {
        Current.DeselectAll();
        RebuildLists();
        StateChanged();
    }

    public void ChangeClear(bool clear)
    {
        Current.Clear = clear;
        RebuildLists();
    }

    public void ToggleBuilding(string id, bool check) => ToggleEntry(id, check, Buildings);
    public void TogglePlant(string id, bool check) => ToggleEntry(id, check, Plants);
    public void ToggleGood(string id, bool check) => ToggleEntry(id, check, Goods);
    public void ToggleNeed(string id, bool check) => ToggleEntry(id, check, Needs, refreshLock: false);
    void ToggleEntry<TEntry>(string id, bool check, Dictionary<string, TEntry> list, bool refreshLock = true)
        where TEntry : EffectiveEntry
    {
        list[id].SetChecked(check);

        if (refreshLock)
        {
            RefreshLock();
        }
    }

    public void RebuildLists()
    {
        Buildings.Clear();
        Plants.Clear();
        Goods.Clear();
        Needs.Clear();

        foreach (var b in aggregator.Templates.AllBuildings)
        {
            Buildings[b.Id] = new(b.Id, b);
        }

        foreach (var p in aggregator.Templates.AllPlants)
        {
            Plants[p.Id] = new(p.Id, p);
        }

        foreach (var g in aggregator.Goods.ItemsByIds.Values)
        {
            Goods[g.Id] = new(g.Id, g);
        }

        foreach (var n in aggregator.Needs.ItemsByIds.Values)
        {
            Needs[n.Id] = new(n.Id, n);
        }

        ListChanged();

        SetCheckList();
    }

    void SetCheckList()
    {
        var curr = Current;
        var faction = aggregator.Factions.ItemsById[Current.FactionId];

        CheckList(Buildings, faction.Buildings, Current.Buildings);
        CheckList(Plants, faction.Plants, Current.Plants);
        CheckList(Goods, faction.Goods, Current.Goods);
        CheckList(Needs, faction.Needs, Current.Needs);

        // Lock District Center
        if (curr.Clear)
        {
            var districtCenterId = faction.Spec.StartingBuildingId;

            var dcIds = aggregator.Templates.IdsByTemplateNames[districtCenterId];
            foreach (var id in dcIds)
            {
                Buildings[id].LockByBase();
            }
        }

        RefreshLock();

        void CheckList<TEntry>(Dictionary<string, TEntry> list, IEnumerable<IIdDef> defaults, IEnumerable<string> checks)
            where TEntry : EffectiveEntry
        {
            if (!curr.Clear)
            {
                foreach (var id in defaults.Select(q => q.Id))
                {
                    list[id].LockByBase();
                }
            }

            foreach (var id in checks)
            {
                list[id].SetChecked(true);
            }
        }
    }

    void RefreshLock()
    {
        foreach (var entry in AllEntries)
        {
            entry.Locked = false;
        }

        PerformLock(Buildings, LockTemplateRequirements, LockBuilding);
        PerformLock(Plants, LockTemplateRequirements, LockPlant);
        PerformLock(Goods, LockGoodRequirements, LockGood);
        PerformLock(Needs, null, LockNeed);

        LockSelectedTemplates();

        StateChanged();

        static void PerformLock<TEntry>(
            Dictionary<string, TEntry> entries,
            Action<string>? performLockRequirement,
            Action<string> baseLock)
            where TEntry : EffectiveEntry
        {
            foreach (var entry in entries.Values)
            {
                if (entry.IsClearLocked)
                {
                    baseLock(entry.Id);
                }
                else if (entry.Checked && performLockRequirement is not null)
                {
                    performLockRequirement(entry.Id);
                }
            }
        }
    }

    void LockSelectedTemplates()
    {
        HashSet<string> checkedTemplates = [];

        foreach (var b in Buildings.Values)
        {
            if (b.Checked)
            {
                checkedTemplates.Add(b.Data.TemplateName);
            }
        }

        foreach (var p in Plants.Values)
        {
            if (p.Checked)
            {
                checkedTemplates.Add(p.Data.TemplateName);
            }
        }

        foreach (var b in Buildings.Values)
        {
            if (!b.Checked && checkedTemplates.Contains(b.Data.TemplateName))
            {
                b.SetChecked(false, true);
            }
        }

        foreach (var p in Plants.Values)
        {
            if (!p.Checked && checkedTemplates.Contains(p.Data.TemplateName))
            {
                p.SetChecked(false, true);
            }
        }
    }

    void LockBuilding(string id)
    {
        Buildings[id].SetChecked(true, true);
        LockTemplateRequirements(id);
    }

    void LockPlant(string id)
    {
        Plants[id].SetChecked(true, true);
        LockTemplateRequirements(id);
    }

    void LockTemplateRequirements(string id)
    {
        var template = aggregator.Templates.ItemsByIds[id];
        foreach (var g in template.RequiredGoods)
        {
            LockGood(g.Id);
        }
        foreach (var n in template.RequiredNeeds)
        {
            LockNeed(n);
        }

        foreach (var grp in aggregator.ExclusiveGroups)
        {
            if (!grp.Templates.Contains(id)) { continue; }

            foreach (var t in grp.Templates)
            {
                if (t == id) { continue; }

                Buildings[t].SetChecked(false, true);
            }
        }
    }

    void LockGood(string id)
    {
        Goods[id].SetChecked(true, true);
        LockGoodRequirements(id);
    }

    void LockGoodRequirements(string id)
    {
        foreach (var n in aggregator.Goods.ItemsByIds[id].RequiredNeeds)
        {
            LockNeed(n);
        }
    }

    void LockNeed(string id) => Needs[id].SetChecked(true, true);
}

public class EffectiveEntry(string id)
{
    public string Id { get; } = id;
    public bool Checked { get; set; }
    public bool Locked { get; set; }
    public bool IsClearLocked { get; set; }

    public void SetChecked(bool isChecked, bool isLocked = false)
    {
        Checked = isChecked;
        Locked = isLocked;
    }

    public void Reset()
    {
        Checked = false;
        Locked = false;
        IsClearLocked = false;
    }

    public void LockByBase()
    {
        Checked = true;
        Locked = true;
        IsClearLocked = true;
    }

}

public class EffectiveEntry<T>(string id, T data) : EffectiveEntry(id)
{
    public T Data { get; } = data;
}