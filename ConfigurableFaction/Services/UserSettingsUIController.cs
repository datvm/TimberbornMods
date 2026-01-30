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

    public Dictionary<string, EffectiveEntry> Buildings = [];
    public Dictionary<string, EffectiveEntry> Plants = [];
    public Dictionary<string, EffectiveEntry> Goods = [];
    public Dictionary<string, EffectiveEntry> Needs = [];

    Dictionary<string, EffectiveEntry>[] allLists = [];
    IEnumerable<EffectiveEntry> AllEntries => allLists.SelectMany(l => l.Values);

    public void Initialize()
    {
        allLists = [Buildings, Plants, Goods, Needs];

        aggregator.Initialize();
    }

    public IGrouping<BlockObjectToolGroupSpec, EffectiveEntry>[] GetBuildings(FactionDef faction)
    {
        List<IGrouping<BlockObjectToolGroupSpec, EffectiveEntry>> result = [];

        foreach (var grp in faction.BuildingsByGroups)
        {
            result.Add(new Grouping<BlockObjectToolGroupSpec, EffectiveEntry>(
                aggregator.ToolGroupsByIds[grp.Key],
                [.. grp.Value.Select(b => Buildings[b.Id])]));
        }

        return [.. result.OrderBy(q => q.Key.Order)];
    }

    public IEnumerable<EffectiveEntry> GetPlants(FactionDef faction) => GetEntries(faction.Plants, Plants);
    public IEnumerable<EffectiveEntry> GetGoods(FactionDef faction) => GetEntries(faction.Goods, Goods);
    public IEnumerable<EffectiveEntry> GetNeeds(FactionDef faction)
        => faction.Needs.Select(n => Needs[n.Id]);

    static IEnumerable<EffectiveEntry> GetEntries<T>(IEnumerable<T> list, Dictionary<string, EffectiveEntry> entries)
        where T : IIdDef
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

        static void SyncList(Dictionary<string, EffectiveEntry> src, HashSet<string> target, HashSet<string> commonIds, bool clear)
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
    void ToggleEntry(string id, bool check, Dictionary<string, EffectiveEntry> list, bool refreshLock = true)
    {
        list[id].SetChecked(check);

        if (refreshLock)
        {
            RefreshLock();
        }
    }

    public void RebuildLists()
    {
        foreach (var l in allLists)
        {
            l.Clear();
        }

        foreach (var b in aggregator.Templates.AllBuildings)
        {
            Buildings[b.Id] = new(b.Id);
        }

        foreach (var p in aggregator.Templates.AllPlants)
        {
            Plants[p.Id] = new(p.Id);
        }

        foreach (var g in aggregator.Goods.ItemsByIds.Values)
        {
            Goods[g.Id] = new(g.Id);
        }

        foreach (var n in aggregator.Needs.ItemsByIds.Values)
        {
            Needs[n.Id] = new(n.Id);
        }

        ListChanged();

        SetCheckList();
    }

    void SetCheckList()
    {
        var curr = Current;
        var faction = aggregator.Factions.ItemsById[Current.FactionId];

        CheckList(Buildings, faction.Buildings.Select(q => q.Id), Current.Buildings);
        CheckList(Plants, faction.Plants.Select(q => q.Id), Current.Plants);
        CheckList(Goods, faction.Goods.Select(q => q.Id), Current.Goods);
        CheckList(Needs, faction.Needs.Select(q => q.Id), Current.Needs);

        // Lock District Center
        if (curr.Clear)
        {
            var districtCenterId = faction.Spec.StartingBuildingId;
            Buildings[aggregator.Templates.IdsByTemplateNames[districtCenterId]].LockByBase();
        }

        RefreshLock();

        void CheckList(Dictionary<string, EffectiveEntry> list, IEnumerable<string> defaults, IEnumerable<string> checks)
        {
            if (!curr.Clear)
            {
                foreach (var id in defaults)
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

        StateChanged();

        static void PerformLock(
            Dictionary<string, EffectiveEntry> entries,
            Action<string>? performLockRequirement,
            Action<string> baseLock)
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