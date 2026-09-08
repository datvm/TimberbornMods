using Timberborn.BlockObjectTools;
using Timberborn.EntitySystem;
using Timberborn.ToolSystem;

namespace ToolHotkey.Services;

public readonly record struct ToolButtonInfo(ToolGroupButton? Group, ToolButton Tool);

public class ToolSelector(
    InputService input,
    KeyBindingRegistry keyBindingRegistry,
    ToolButtonService toolButtons,
    ILoc t,
    PreviewFactory previewFac
) : IInputProcessor, ILoadableSingleton, IUnloadableSingleton
{
    readonly Dictionary<string, ToolButtonInfo> assignedKeys = [];

    public void Load()
    {
        var tools = LoadToolList();
        LoadAssignedList(tools);

        input.AddInputProcessor(this);
    }

    List<AdditionalToolSpec> LoadToolList()
    {
        List<AdditionalToolSpec> tools = [];
        HashSet<string> ids = [];
        ScanAllButtons(null, info =>
        {
            var spec = ExtractToolButtonInfo(in info);
            if (spec is null
                || ids.Contains(spec.Value.Id)) { return; }

            ids.Add(spec.Value.Id);
            tools.Add(spec.Value);
        });

        ToolPersistence.SaveTools(tools);
        return tools;
    }

    void LoadAssignedList(List<AdditionalToolSpec> tools)
    {
        assignedKeys.Clear();

        var bindings = keyBindingRegistry.KeyBindings
            .Where(q => q.GroupId == ModStarter.KeyGroupId)
            .ToDictionary(q => q.Id);

        ScanAllButtons(null, info =>
        {
            var spec = ExtractToolButtonInfo(info);
            if (spec is null) { return; }

            var hotKeyId = string.Format(ModStarter.ToolKeyId, spec.Value.Id);
            if (assignedKeys.ContainsKey(hotKeyId)) { return; }

            if (!bindings.TryGetValue(hotKeyId, out var binding)) { return; }
            assignedKeys.Add(hotKeyId, info);
        });
    }

    AdditionalToolSpec? ExtractToolButtonInfo(in ToolButtonInfo info)
    {
        var (grpBtn, toolBtn) = info;
        var tool = toolBtn.Tool;

        string? descTitle;
        if (tool is BlockObjectTool boTool)
        {
            var prefab = boTool.Prefab;
            var preview = previewFac.Create(prefab);

            var label = preview.GetComponentFast<LabeledEntity>();
            if (label is null)
            {
                Debug.Log($"Prefab {prefab.name} has no label");
                return default;
            }

            descTitle = label.DisplayName;
        }
        else
        {
            var desc = tool.Description();
            descTitle = desc?.HasTitle == true ? desc.Title : null;
        }

        if (descTitle is null)
        {
            Debug.Log($"Tool {tool.GetType().FullName} has no title");
            return default;
        }

        var toolName = descTitle;
        var grpName = grpBtn?._toolGroup.DisplayNameLocKey;

        var toolId = grpName is null ? toolName : $"{grpName}.{toolName}";
        var toolDisplay = grpName is null ? toolName : $"{t.T(grpName)} -> {toolName}";

        return new(toolId, toolDisplay, grpName);
    }

    void ScanAllButtons(Action<ToolGroupButton>? onGroupFound, Action<ToolButtonInfo>? onButtonFound)
    {
        foreach (var btn in toolButtons._rootButtons)
        {
            if (btn is ToolGroupButton grp)
            {
                onGroupFound?.Invoke(grp);
                if (onButtonFound is null) { continue; }

                ScanGroupButtons(grp, onButtonFound);
            }
            else if (btn is ToolButton toolBtn && onButtonFound is not null)
            {
                onButtonFound?.Invoke(new(null, toolBtn));
            }
        }
    }

    void ScanGroupButtons(ToolGroupButton grp, Action<ToolButtonInfo> onButtonFound)
    {
        foreach (var btn in grp._toolButtons)
        {
            onButtonFound.Invoke(new(grp, btn));
        }
    }

    public bool ProcessInput()
    {
        foreach (var (k, tool) in assignedKeys)
        {
            if (input.IsKeyDown(k))
            {
                SelectTool(in tool);
                return true;
            }
        }

        return false;
    }

    void SelectTool(in ToolButtonInfo info)
    {
        info.Group?.Select();
        info.Tool.Select();
    }

    public void Unload()
    {
        input.RemoveInputProcessor(this);
    }

}
