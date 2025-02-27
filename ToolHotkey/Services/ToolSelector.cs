using Timberborn.BlockObjectTools;
using Timberborn.EntitySystem;
using Timberborn.ToolSystem;

namespace ToolHotkey.Services;

public class ToolSelector(
    InputService input,
    PrefabService prefabs,
    ILoc t,
    KeyBindingRegistry keyBindingRegistry,
    ToolButtonService toolButtons,
    ToolManager tools
) : IInputProcessor, ILoadableSingleton, IUnloadableSingleton
{
    readonly Dictionary<string, AdditionalToolSpec> assignedKeys = [];

    public void Load()
    {
        InitToolList();
        LoadAssignedList();

        input.AddInputProcessor(this);
    }

    void InitToolList()
    {
        var tools = prefabs.GetAll<PlaceableBlockObjectSpec>();

        List<AdditionalToolSpec> additionalTools = [];
        foreach (var tool in tools)
        {
            var id = tool.name;
            var groupid = tool.ToolGroupId;

            var nameKey = tool.GetComponentFast<LabeledEntitySpec>()?.DisplayNameLocKey ?? id;

            additionalTools.Add(new(id, groupid, nameKey, t.T(nameKey)));
        }

        ToolPersistence.SaveTools(additionalTools);
    }

    void LoadAssignedList()
    {
        var tools = ToolPersistence.LoadTools();
        assignedKeys.Clear();

        var bindings = keyBindingRegistry.KeyBindings
            .Where(q => q.GroupId == ModStarter.KeyGroupId)
            .ToDictionary(q => q.Id);

        foreach (var t in tools)
        {
            var keybindId = string.Format(ModStarter.ToolKeyId, t.Id);

            // Outdated keybindings
            if (!bindings.TryGetValue(keybindId, out var binding))
            {
                Debug.Log($"Keybinding not found {keybindId} for tool {t.Id}, probably a new tool was added");
                continue;
            }

            if (!binding.PrimaryInputBinding.IsDefined && !binding.SecondaryInputBinding.IsDefined) { continue; }
            Debug.Log($"Keybinding {keybindId} for tool {t.Id} is defined");
            assignedKeys.Add(keybindId, t);
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

    void SelectTool(in AdditionalToolSpec tool)
    {
        var id = tool.Id;

        var btn = toolButtons._toolButtons.FirstOrDefault(q =>
        {
            switch (q.Tool)
            {
                case BlockObjectTool bo:
                    if (bo.Prefab.name == id)
                    {
                        return true;
                    }

                    return false;
            }

            return false;
        });

        if (btn is null)
        {
            Debug.Log($"Tool button not found for tool {id}. This should not happen");
            return;
        }

        var grp = toolButtons.GetToolGroupButton(btn);
        grp.Select();
        btn.Select();
    }

    public void Unload()
    {
        input.RemoveInputProcessor(this);
    }



}
