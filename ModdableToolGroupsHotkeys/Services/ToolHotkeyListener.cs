namespace ModdableToolGroupsHotkeys.Services;

public class ToolHotkeyListener(
    ToolHotkeySpecService toolHotkeySpecService,
    KeyBindingEventService keyBindingEventService,
    ToolButtonService toolButtonService,
    ModdableToolGroupButtonService moddableToolGroupButtonService,
    CustomBlockObjectButtons customBlockObjectButtons
) : ILoadableSingleton
{
    private readonly Dictionary<IToolbarButton, ToolGroupButton> _buttonToGroupCache = [];
    private readonly Dictionary<ToolGroupButton, ModdableToolGroupButton> _toolGroupToModdableCache = [];
    private readonly Dictionary<ToolGroupButton, ToolGroupButton[]> _parentChainCache = [];

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
        _buttonToGroupCache.Clear();
        _toolGroupToModdableCache.Clear();
        _parentChainCache.Clear();

        // Build button-to-group cache
        foreach (var groupButton in toolButtonService._toolGroupButtons)
        {
            foreach (var toolButton in groupButton.ToolButtons)
            {
                _buttonToGroupCache[toolButton] = groupButton;
            }
        }

        // Build ToolGroupButton-to-ModdableToolGroupButton cache
        foreach (var moddableGroup in customBlockObjectButtons.ToolGroupButtonsById.Values)
        {
            _toolGroupToModdableCache[moddableGroup.ToolGroupButton] = moddableGroup;
        }

        // Build parent chain cache for all groups
        foreach (var groupButton in toolButtonService._toolGroupButtons)
        {
            var chain = new List<ToolGroupButton>();
            var info = moddableToolGroupButtonService[groupButton];

            while (info?.Parent != null)
            {
                chain.Add(info.Parent.Button);
                info = info.Parent;
            }

            chain.Reverse();
            _parentChainCache[groupButton] = chain.ToArray();
        }
    }

    void SelectToolButtonWithGroups(ToolButton toolButton)
    {
        // Find which ModdableToolGroupButton contains this ToolButton
        var groupButton = FindModdableGroupForToolButton(toolButton);
        if (groupButton != null)
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
            if (toolGroup != null)
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
        if (_parentChainCache.TryGetValue(groupButton, out var chain))
        {
            return chain;
        }

        // Cache miss means do nothing
        return Array.Empty<ToolGroupButton>();
    }

    ModdableToolGroupButton? FindModdableGroupForToolButton(ToolButton toolButton)
    {
        // O(1) lookup: button -> ToolGroupButton -> ModdableToolGroupButton
        var toolGroupButton = FindToolGroupForButton(toolButton);
        if (toolGroupButton != null && _toolGroupToModdableCache.TryGetValue(toolGroupButton, out var moddableGroup))
        {
            return moddableGroup;
        }

        return null;
    }

    ToolGroupButton? FindToolGroupForButton(IToolbarButton button)
    {
        // O(1) lookup from pre-computed cache
        _buttonToGroupCache.TryGetValue(button, out var groupButton);
        return groupButton;
    }

    void RegisterAction(string id, Action onDown)
    {
        var ev = keyBindingEventService.Get(id);
        ev.OnDown += onDown;
    }

}
