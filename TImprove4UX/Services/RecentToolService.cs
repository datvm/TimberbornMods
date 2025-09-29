namespace TImprove4UX.Services;

public class RecentToolService(
    ToolManager toolManager,
    EventBus eb,
    InputService inputService
) : ILoadableSingleton, IInputProcessor
{
    public const string RecentToolHotkeyId = "RecentTool";

    Tool? prevTool;
    Tool? currTool;

    public void Load()
    {
        eb.Register(this);
        inputService.AddInputProcessor(this);
    }

    public bool ProcessInput()
    {
        if (!inputService.IsKeyDown(RecentToolHotkeyId)) { return false; }

        if (prevTool is not null)
        {
            toolManager.SwitchTool(prevTool);
        }
        return true;
    }

    [OnEvent]
    public void OnToolEntered(ToolEnteredEvent e)
    {
        prevTool = currTool;
        currTool = e.Tool;
    }

}
