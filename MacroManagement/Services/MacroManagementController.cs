namespace MacroManagement.Services;

public class MacroManagementController(
    ToolButtonService toolButtonService,
    EntityBadgeService entityBadgeService,

    MultiSelectService multiSelectService,

    InputBindingDescriber inputBindingDescriber,
    DevModeManager devModeManager
) : IPostLoadableSingleton
{
    public const string CopyKeyId = "MacroManagementCopy";
    public const string SelectAllKeyId = "MacroManagementSelectAll";
    public const string SelectDistrictKeyId = "MacroManagementSelectDistrict";

    FrozenDictionary<string, ToolButton> toolButtons = FrozenDictionary<string, ToolButton>.Empty;
    
    public void PostLoad()
    {
        InitToolButtons();
    }

    void InitToolButtons()
    {
        Dictionary<string, ToolButton> toolButtonsByName = [];

        foreach (var toolButton in toolButtonService.ToolButtons)
        {
            if (toolButton.Tool is not BlockObjectTool boTool) { continue; }

            var prefab = boTool.Prefab.GetComponentFast<PrefabSpec>();
            if (!prefab) { continue; }

            toolButtonsByName.Add(prefab.Name, toolButton);
        }

        toolButtons = toolButtonsByName.ToFrozenDictionary();
    }

    public bool TryGetInfo(BaseComponent? component, [NotNullWhen(true)] out MacroManagementInfo? info)
    {
        info = default;
        if (!component) { return false; }

        // Find out if it's the MMComponent, then use the original
        var mm = component.GetComponentFast<MMComponent>();
        if (mm)
        {
            component = mm.Original.Prefab;
        }

        var prefab = component.GetComponentFast<PrefabSpec>();
        if (!prefab) { return false; }

        var label = component.GetComponentFast<LabeledEntity>();
        if (!label) { return false; }

        var toolButton = GetToolButton(prefab);
        if (toolButton is null
            || (toolButton.DevModeTool && !devModeManager.Enabled)) { return false; }

        var district = component.GetComponentFast<DistrictBuilding>();

        info = new(prefab, label, toolButton, district ? district : null);
        return true;
    }

    public string? GetBadgeName(BaseComponent component)
    {
        var badge = entityBadgeService.GetHighestPriorityEntityBadge(component);
        return badge?.GetEntityName();
    }

    public void RequestCopy(in MacroManagementInfo info)
    {
        info.ToolButton?.Select();
    }

    public void RequestSelection(in MacroManagementInfo info, MacroManagementSelectionFlags flags)
        => multiSelectService.SelectItems(info, flags);

    public string? GetCopyHotkey() => inputBindingDescriber.GetInputBindingText(CopyKeyId);

    ToolButton? GetToolButton(PrefabSpec prefab) => toolButtons.TryGetValue(prefab.Name, out var toolButton) ? toolButton : null;

}
