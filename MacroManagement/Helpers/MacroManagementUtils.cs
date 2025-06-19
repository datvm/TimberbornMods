namespace MacroManagement.Helpers;

public static class MacroManagementUtils
{

    public const MacroManagementSelectionFlags All = MacroManagementSelectionFlags.Running | MacroManagementSelectionFlags.Paused;

    public static string GetCommandWithHotkey(this InputBindingDescriber describer, string command, string hotkeyId)
    {
        var hotkey = describer.GetInputBindingText(hotkeyId);

        return string.IsNullOrEmpty(hotkey) ? command : $"{command} [{hotkey}]";
    }

}
