namespace RadialToolbar.UI;

[BindSingleton(Contexts = BindAttributeContext.NonMenu)]
public class ToolBarTriggerer(
    InputService inputService,
    ToolbarController toolbar
) : ILoadableSingleton, IInputProcessor
{
    public const string ToggleKey = "ToggleRadialToolbar";

    public void Load()
    {
        inputService.AddInputProcessor(this);
    }

    public bool ProcessInput()
    {
        if (inputService.IsKeyDown(ToggleKey))
        {
            toolbar.ToggleDisplay();
            return true;
        }

        return false;
    }

}
