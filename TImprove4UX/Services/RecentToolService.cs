namespace TImprove4UX.Services;

public class RecentToolService(
    ToolManager toolManager,
    EventBus eb,
    InputService inputService
) : ILoadableSingleton, IInputProcessor
{
    public const string RecentToolHotkeyId = "RecentTool";
    
    Tool? recentTool;

    public void Load()
    {
        eb.Register(this);
        inputService.AddInputProcessor(this);
    }

    public bool ProcessInput()
    {
        if (!inputService.IsKeyDown(RecentToolHotkeyId)) { return false; }

        if (recentTool is not null)
        {
            toolManager.SwitchTool(recentTool);
        }
        return true;
    }

    [OnEvent]
    public void OnToolEntered(ToolEnteredEvent e)
    {
        recentTool = e.Tool;
    }

}
