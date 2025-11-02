namespace TImprove4UX.Services;

public class RecentToolService(
    ToolService toolService,
    EventBus eb,
    InputService inputService
) : ILoadableSingleton, IInputProcessor
{
    public const string RecentToolHotkeyId = "RecentTool";

    ITool? prevTool;
    ITool? currTool;

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
            toolService.SwitchTool(prevTool);
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
