namespace ConfigurableTopBar.Services;

public class TopBarConfigProvider(
    ILoc t,
    AssetRefService assetRefService
) : IUnloadableSingleton
{
    public const string FileName = "TopBarConfig.json";
    public static readonly string FilePath = Path.Combine(UserDataFolder.Folder, FileName);

    public const string GroupLocKeyFormat = "ConfigurableTopBar.GoodGroup.{0}.Name";

    public List<EditableGoodGroupSpec> Groups { get; private set; } = [];
    public EditableGoodGroupSpec SpecialGroup { get; private set; } = null!;
    IEnumerable<EditableGoodGroupSpec> AllGroups => Groups.Concat(SpecialGroup.Goods.Count > 0 ? [SpecialGroup] : []);

    readonly HashSet<string> selectingGoods = [];
    public bool IsSelectingGoodsEmpty => selectingGoods.Count == 0;

    public event Action GroupsChanged = null!;
    public event Action<bool> SelectingGoodsEmptyChanged = null!;

    bool initialized;

    readonly Loc t = (Loc)t;

    public void Save()
    {
        File.WriteAllText(FilePath, JsonConvert.SerializeObject(Groups, Formatting.Indented));
    }

    public IEnumerable<GoodGroupSpec> CompileGoodGroupSpecs()
    {
        var order = 0;
        foreach (var g in AllGroups)
        {
            order += 10;
            var locKey = string.Format(GroupLocKeyFormat, g.Id);

            t._localization[locKey] = g.Name;

            yield return new()
            {
                Id = g.Id,
                Order = order,
                Icon = assetRefService.CreateAssetRef<Sprite>(g.Icon),
                SingleResourceGroup = g.SingleResourceGroup,
                DisplayNameLocKey = locKey,
                DisplayName = new(g.Name),
            };
        }
    }

    public FrozenDictionary<string, CompiledGoodSpecItem> CompileGoodSpecs()
    {
        var result = new List<KeyValuePair<string, CompiledGoodSpecItem>>();

        foreach (var grp in AllGroups)
        {
            var order = 0;
            foreach (var good in grp.Goods)
            {
                order += 10;
                result.Add(new(good.Id, new(good.Id, grp.Id, order)));
            }
        }

        return result.ToFrozenDictionary();
    }

    public void ChangeSelectingGood(string id, bool add)
    {
        var wasEmpty = selectingGoods.Count == 0;
        if (add)
        {
            selectingGoods.Add(id);
        }
        else
        {
            selectingGoods.Remove(id);
        }

        var nowEmpty = selectingGoods.Count == 0;
        if (wasEmpty != nowEmpty)
        {
            SelectingGoodsEmptyChanged?.Invoke(nowEmpty);
        }
    }

    public void ClearSelectingGoods() => selectingGoods.Clear();

    public bool IsGoodSelecting(string id) => selectingGoods.Contains(id);
    
    public void Initialize(ISpecService specs, GoodSpriteProvider? goodSpriteProvider = null)
    {
        if (initialized) { return; }
        initialized = true;

        SpecialGroup = EditableGoodGroupSpec.CreateSpecialGroup(t);
        Groups = LoadFile();

        var groupsByIds = InitializeGroups(specs, goodSpriteProvider);
        InitializeGoods(specs, groupsByIds, goodSpriteProvider);

        Save();
    }

    public bool MoveGroup(EditableGoodGroupSpec group, bool up)
        => MoveItem(Groups, group, up);

    public void DeleteGroup(EditableGoodGroupSpec group)
    {
        foreach (var good in group.Goods)
        {
            SpecialGroup.Goods.Add(good);
        }
        Groups.Remove(group);

        GroupsChanged();
    }

    public void MoveGoods(EditableGoodGroupSpec target)
    {
        if (!Groups.Contains(target)) { return; }

        foreach (var grp in AllGroups)
        {
            if (grp == target) { continue; }

            for (int i = 0; i < grp.Goods.Count; i++)
            {
                var good = grp.Goods[i];
                if (selectingGoods.Contains(good.Id))
                {
                    grp.Goods.RemoveAt(i);
                    target.Goods.Add(good);
                    i--;
                }
            }
        }

        GroupsChanged();
    }

    public bool MoveGood(EditableGoodGroupSpec group, EditableGoodSpec good, bool up)
        => MoveItem(group.Goods, good, up);

    static bool MoveItem<T>(List<T> list, T item, bool up)
    {
        var index = list.IndexOf(item);
        if (index == -1) { return false; }
        if (up)
        {
            if (index == 0) { return false; }
            list[index] = list[index - 1];
            list[index - 1] = item;
        }
        else
        {
            if (index >= list.Count - 1) { return false; }
            list[index] = list[index + 1];
            list[index + 1] = item;
        }
        return true;
    }

    Dictionary<string, EditableGoodGroupSpec> InitializeGroups(ISpecService specs, GoodSpriteProvider? goodSpriteProvider)
    {
        var isEmpty = Groups.Count == 0;
        List<EditableGoodGroupSpec> pendingGroups = [];
        Dictionary<string, EditableGoodGroupSpec> existingGroups = Groups.ToDictionary(q => q.Id);

        foreach (var spec in specs.GetSpecs<GoodGroupSpec>().OrderBy(q => q.Order))
        {
            if (spec.Icon is not null)
            {
                goodSpriteProvider?.AddGoodGroup(spec.Icon);
            }

            if (existingGroups.TryGetValue(spec.Id, out var grp))
            {
                grp.IsBuiltIn = true;
                continue;
            }
            else if (isEmpty)
            {
                grp = new EditableGoodGroupSpec(
                    spec.Id,
                    spec.Icon?.Path ?? GoodSpriteProvider.QuestionMarkPath,
                    spec.DisplayName.Value,
                    true);
                existingGroups.Add(spec.Id, grp);
                pendingGroups.Add(grp);
            }   
        }
        Groups.AddRange(pendingGroups);

        return existingGroups;
    }

    void InitializeGoods(ISpecService specs, Dictionary<string, EditableGoodGroupSpec> groups, GoodSpriteProvider? goodSpriteProvider)
    {
        Dictionary<string, EditableGoodSpec> goodsByIds = [];
        foreach (var group in groups.Values)
        {
            foreach (var good in group.Goods)
            {
                goodsByIds.Add(good.Id, good);
            }
        }

        var goods = specs.GetSpecs<GoodSpec>();
        foreach (var spec in goods.OrderBy(q => q.GoodOrder))
        {
            if (spec.Icon is not null)
            {
                goodSpriteProvider?.AddGood(spec.Icon);
            }

            if (goodsByIds.TryGetValue(spec.Id, out var editable)) // Already exists
            {
                editable.GoodSpec = spec;
            }
            else if (groups.TryGetValue(spec.GoodGroupId, out var group)) // Group exists (in case a group was removed)
            {
                AddGood(spec, group);
            }
            else // Add to special group if group not found
            {
                AddGood(spec, SpecialGroup);
            }
        }

        // Just cleanup: remove goods that no longer exist and duplicates
        HashSet<string> encounteredGoods = [];
        foreach (var grp in groups.Values)
        {
            var grpGoods = grp.Goods;

            for (int i = 0; i < grpGoods.Count; i++)
            {
                var g = grpGoods[i];

                if (!encounteredGoods.Add(g.Id)
                    || g.GoodSpec is null)
                {
                    grpGoods.RemoveAt(i);
                    i--;
                }
            }
        }

        static void AddGood(GoodSpec spec, EditableGoodGroupSpec group)
        {
            var editable = new EditableGoodSpec(spec.Id)
            {
                GoodSpec = spec,
            };
            group.Goods.Add(editable);
        }
    }

    List<EditableGoodGroupSpec> LoadFile()
    {
        if (!File.Exists(FilePath)) { return []; }
        return JsonConvert.DeserializeObject<List<EditableGoodGroupSpec>>(File.ReadAllText(FilePath)) ?? [];
    }

    public void Reset()
    {
        Groups = [];
        Save();
    }

    public void Unload()
    {
        Save();
    }
}
