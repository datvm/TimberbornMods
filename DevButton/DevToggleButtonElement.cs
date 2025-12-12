
namespace DevButton;

public class DevToggleButtonElement(
    VisualElementLoader loader,
    ToolButtonFactory toolButtonFactory,
    DevModeManager devModeManager
) : SimpleRootButton(loader, toolButtonFactory), ILoadableSingleton
{

    public override string Id { get; } = "LV.DevToggleButton";
    protected override string ImageName { get; } = "ToggleDevTool";

    public void Load()
    {
        Text = "Dev";
    }

    protected override void OnClicked()
    {
        if (devModeManager.Enabled)
        {
            devModeManager.Disable();
        }
        else
        {
            devModeManager.Enable();
        }
    }
}
