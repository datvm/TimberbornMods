namespace DevButton;

[Context("Game")]
public class MGameConfig : Configurator
{
    public override void Configure()
    {
        // Add Toggle button
        this.MultiBindCustomTool<DevToggleButtonElement>();
    }
}
