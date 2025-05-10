namespace Omnibar.UI;

public class OmnibarController(
    InputService inputs,
    OmnibarBox omnibarBox,
    EventBus eb
) : IInputProcessor, ILoadableSingleton, IUnloadableSingleton
{
    const string OpenKeyId = "OmnibarOpen";
    bool keyDown;

    public void Load()
    {
        eb.Register(this);
    }

    public bool ProcessInput()
    {
        if (inputs.IsKeyUp(OpenKeyId) && keyDown)
        {
            keyDown = false;
            return omnibarBox.ShouldOpen();
        }
        else if (inputs.IsKeyDown(OpenKeyId))
        {
            keyDown = true;
        }

        return false;
    }

    public void Unload()
    {
        inputs.RemoveInputProcessor(this);
    }

    [OnEvent]
    public void OnToolEntered(ToolEnteredEvent ev)
    {
        if (ev.Tool is CursorTool)
        {
            inputs.AddInputProcessor(this);
        }
    }

    [OnEvent]
    public void OnToolExited(ToolExitedEvent ev)
    {
        if (ev.Tool is CursorTool)
        {
            inputs.RemoveInputProcessor(this);
        }
    }

}
