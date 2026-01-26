namespace ModdableToolGroupsHotkeys.Services;

public class ToolHotkeySpecService(
    ISpecService specs,
    KeyBindingsBox keyBindingsBox,
    KeyBindingRegistry keyBindingRegistry,
    KeyBindingSpecService keyBindingSpecService,
    CustomBlockObjectButtons customBlockObjectButtons,
    ModdableCustomToolButtonService customButtonService,
    ILoc t
) : ILoadableSingleton
{

    public FrozenDictionary<string, ModdableToolGroupButton> BlockObjectGroupHotkeys { get; private set; } = FrozenDictionary<string, ModdableToolGroupButton>.Empty;
    public FrozenDictionary<string, ToolButton> BlockObjectToolHotkeys { get; private set; } = FrozenDictionary<string, ToolButton>.Empty;
    public FrozenDictionary<string, IToolHotkeyDefinition> ToolHotkeys { get; private set; } = FrozenDictionary<string, IToolHotkeyDefinition>.Empty;

    readonly SpecService specs = (SpecService)specs;

    public void Load()
    {
        AppendSpecs();
        RefreshService();
        UpdateUI();
    }

    void AppendSpecs()
    {
        List<KeyBindingSpec> keys = [];

        AppendToolSpeccs(keys);
        AppendBlockObjectToolSpecs(keys);

        AddToSpecService(keys);
    }

    void AppendToolSpeccs(List<KeyBindingSpec> keys)
    {
        var definitions = customButtonService.ElementsByIds.Values
            .OfType<IHotkeySupportedTool>()
            .Where(q => !customButtonService.RemovingElementTypes.Contains(q.GetType()))
            .SelectMany(t => t.GetHotkeys())
            .Distinct(ToolHotkeyDefinitionComparer.Instance)
            .Select(h => new KeyValuePair<IToolHotkeyDefinition, string>(h, t.T(h.LocKey)))
            .OrderBy(q => q.Key.Order is null)
            .ThenBy(q => q.Key.Order ?? 0)
            .ThenBy(q => q.Value)
            .Select(q => q.Key);

        var counter = 0;
        foreach (var def in definitions)
        {
            if (def.Order is null)
            {
                counter += 10;
            }

            keys.Add(new()
            {
                Id = def.Id,
                GroupId = def.GroupId,
                LocKey = def.LocKey,
                Order = def.Order ?? counter,
                DevModeOnly = def.IsDevTool,
            });
        }

        ToolHotkeys = definitions.ToFrozenDictionary(q => q.Id);
    }

    void AppendBlockObjectToolSpecs(List<KeyBindingSpec> keys)
    {
        var grpHotkeys = AppendBlockObjectGroup(
            customBlockObjectButtons.ToolGroupButtonsById, b => b.Spec.DisplayNameLocKey,
            keys, "BuildingToolGroups");
        BlockObjectGroupHotkeys = grpHotkeys.ToFrozenDictionary();

        var toolHotkeys = AppendBlockObjectGroup(
            customBlockObjectButtons.ToolButtonsById, b => ((BlockObjectTool)b.Tool).Template.GetSpec<LabeledEntitySpec>().DisplayNameLocKey,
            keys, "BuildingTools");
        BlockObjectToolHotkeys = toolHotkeys.ToFrozenDictionary();
    }

    List<KeyValuePair<string, T>> AppendBlockObjectGroup<T>(IReadOnlyDictionary<string, T> btns, Func<T, string> locFunc, List<KeyBindingSpec> keys, string groupId)
    {
        Dictionary<string, List<(string Id, string Loc)>> nameToHotkeyMap = [];
        List<KeyValuePair<string, T>> hotkeys = [];
        foreach (var (id, grpBtn) in btns)
        {
            var hotkeyId = $"MTG.BlockObject.Group.{id}";
            hotkeys.Add(new(hotkeyId, grpBtn));

            var loc = locFunc(grpBtn);
            nameToHotkeyMap.GetOrAdd(t.T(loc)).Add((hotkeyId, loc));
        }

        var orderedNames = nameToHotkeyMap.Keys.OrderBy(q => q);
        var counter = 0;
        foreach (var name in orderedNames)
        {
            var ids = nameToHotkeyMap[name];
            foreach (var (id, loc) in ids)
            {
                keys.Add(new()
                {
                    Id = id,
                    GroupId = groupId,
                    LocKey = loc,
                    Order = counter++,
                });
            }
        }

        return hotkeys;
    }

    void AddToSpecService(List<KeyBindingSpec> keys)
    {
        var blueprints = specs._cachedBlueprintsBySpecs[typeof(KeyBindingSpec)];

        foreach (var key in keys)
        {
            var bp = new Blueprint(key.Id + $".{nameof(KeyBindingSpec)}.json", [key], []);
            blueprints.Add(new(() => bp));
        }
    }

    void RefreshService()
    {
        keyBindingSpecService._keyBindingDefinitions.Clear();
        keyBindingSpecService.Load();

        keyBindingRegistry._keyBindings.Clear();
        keyBindingRegistry._keyBindingsById.Clear();
        keyBindingRegistry.Load();
    }

    void UpdateUI()
    {
        var list = keyBindingsBox._root.Q("Content");
        list.Clear();
        
        var grps = keyBindingsBox._keyBindingRowFactory.CreateAll();
        foreach (var grp in grps)
        {
            list.Add(grp.Root);
        }        
    }

}
