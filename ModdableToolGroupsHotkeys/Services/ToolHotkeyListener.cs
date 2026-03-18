namespace ModdableToolGroupsHotkeys.Services;

public class ToolHotkeyListener(
    ToolHotkeySpecService toolHotkeySpecService,
    KeyBindingEventService keyBindingEventService,
    ToolButtonService toolButtonService,
    ModdableToolGroupButtonService moddableToolGroupButtonService,
    CustomBlockObjectButtons customBlockObjectButtons
) : ILoadableSingleton
{
    readonly Dictionary<IToolbarButton, ToolGroupButton> buttonToGroupCache = [];
    readonly Dictionary<ToolGroupButton, ModdableToolGroupButton> toolGroupToModdableCache = [];
    readonly Dictionary<ToolGroupButton, ToolGroupButton[]> parentChainCache = [];

    public void Load()
    {
        // Pre-compute mappings for O(1) lookup at runtime
        BuildCaches();

        foreach (var (id, btn) in toolHotkeySpecService.BlockObjectToolHotkeys)
        {
            RegisterAction(id, () => SelectToolButtonWithGroups(btn));
        }

        foreach (var (id, btn) in toolHotkeySpecService.BlockObjectGroupHotkeys)
        {
            RegisterAction(id, () => SelectGroupWithParents(btn.ToolGroupButton));
        }

        foreach (var (id, tool) in toolHotkeySpecService.ToolHotkeys)
        {
            RegisterAction(id, () => SelectToolWithGroup(tool));
        }
    }

    void BuildCaches()
    {
        // Clear caches
        buttonToGroupCache.Clear();
        toolGroupToModdableCache.Clear();
        parentChainCache.Clear();

        // Build button-to-group cache
        foreach (var groupButton in toolButtonService._toolGroupButtons)
        {
            foreach (var toolButton in groupButton.ToolButtons)
            {
                buttonToGroupCache[toolButton] = groupButton;
            }
        }

        // Build ToolGroupButton-to-ModdableToolGroupButton cache
        foreach (var moddableGroup in customBlockObjectButtons.ToolGroupButtonsById.Values)
        {
            toolGroupToModdableCache[moddableGroup.ToolGroupButton] = moddableGroup;
        }

        // Build parent chain cache for all groups
        foreach (var groupButton in toolButtonService._toolGroupButtons)
        {
            var chain = new List<ToolGroupButton>();
            var info = moddableToolGroupButtonService[groupButton];

            while (info?.Parent is not null)
            {
                chain.Add(info.Parent.Button);
                info = info.Parent;
            }

            chain.Reverse();
            parentChainCache[groupButton] = [.. chain];
        }
    }

    void SelectToolButtonWithGroups(ToolButton toolButton)
    {
        // Find which ModdableToolGroupButton contains this ToolButton
        var groupButton = FindModdableGroupForToolButton(toolButton);
        if (groupButton is not null)
        {
            SelectGroupWithParents(groupButton.ToolGroupButton);
        }

        // Select the tool itself
        toolButton.Select();
    }

    void SelectToolWithGroup(IToolHotkeyDefinition toolHotkey)
    {
        // First, try to select the tool group that contains this tool
        if (toolHotkey is ButtonToolHotkeyDefinition btnHotkey)
        {
            var toolGroup = FindToolGroupForButton(btnHotkey.Button);
            if (toolGroup is not null)
            {
                SelectGroupWithParents(toolGroup);
            }
        }

        // Then select the tool itself
        toolHotkey.Select();
    }

    void SelectGroupWithParents(ToolGroupButton groupButton)
    {
        // Get the parent chain
        var parentChain = GetParentChain(groupButton);

        // Select all parents from root to leaf
        foreach (var parent in parentChain)
        {
            parent.Select();
        }

        // Finally select the immediate group
        groupButton.Select();
    }

    ToolGroupButton[] GetParentChain(ToolGroupButton groupButton)
    {
        // O(1) lookup from pre-computed cache
        if (parentChainCache.TryGetValue(groupButton, out var chain))
        {
            return chain;
        }

        // Cache miss means do nothing
        return [];
    }

    ModdableToolGroupButton? FindModdableGroupForToolButton(ToolButton toolButton)
    {
        // O(1) lookup: button -> ToolGroupButton -> ModdableToolGroupButton
        var toolGroupButton = FindToolGroupForButton(toolButton);
        if (toolGroupButton is not null && toolGroupToModdableCache.TryGetValue(toolGroupButton, out var moddableGroup))
        {
            return moddableGroup;
        }

        return null;
    }

    ToolGroupButton? FindToolGroupForButton(IToolbarButton button)
    {
        // O(1) lookup from pre-computed cache
        buttonToGroupCache.TryGetValue(button, out var groupButton);
        return groupButton;
    }

    void RegisterAction(string id, Action onDown)
    {
        var ev = keyBindingEventService.Get(id);
        ev.OnDown += onDown;
    }

}
