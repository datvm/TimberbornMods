namespace ModdableToolGroupsDemo.Services;

public class DevToolToggleTool(DevModeManager devModeManager, ToolService toolService) : ITool
{
    public void Enter()
    {
        if (devModeManager.Enabled)
        {
            devModeManager.Disable();

        }
        else
        {
            devModeManager.Enable();
        }

        toolService.SwitchToDefaultTool();
    }

    public void Exit() { }
}
