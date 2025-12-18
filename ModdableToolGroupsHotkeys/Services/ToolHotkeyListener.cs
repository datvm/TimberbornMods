namespace ModdableToolGroupsHotkeys.Services;

public class ToolHotkeyListener(
    ToolHotkeySpecService toolHotkeySpecService,
    KeyBindingEventService keyBindingEventService
) : ILoadableSingleton
{

    public void Load()
    {
        foreach (var (id, btn) in toolHotkeySpecService.BlockObjectToolHotkeys)
        {
            RegisterAction(id, btn.Select);
        }

        foreach (var (id, btn) in toolHotkeySpecService.BlockObjectGroupHotkeys)
        {
            RegisterAction(id, btn.ToolGroupButton.Select);
        }

        foreach (var (id, tool) in toolHotkeySpecService.ToolHotkeys)
        {
            RegisterAction(id, tool.Select);
        }
    }

    void RegisterAction(string id, Action onDown)
    {
        var ev = keyBindingEventService.Get(id);
        ev.OnDown += onDown;
    }

}
