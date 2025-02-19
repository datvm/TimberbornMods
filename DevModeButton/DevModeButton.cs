using Timberborn.AssetSystem;
using Timberborn.Debugging;


namespace DevModeButton;
public class DevModeButton : GrouplessBottomBarButton
{
    readonly DevModeManager devMode;

    public DevModeButton(VisualElementLoader veLoader, IAssetLoader assetLoader, DevModeManager devMode) : base(veLoader, assetLoader)
    {
        SpritePath = "Sprites/timberui-dev";
        Click = ToggleDevMode;
        BottomText = "Dev Mode";

        this.devMode = devMode;
    }

    void ToggleDevMode()
    {
        if (devMode.Enabled)
        {
            devMode.Disable();
        }
        else
        {
            devMode.Enable();
        }
    }

}