namespace ConfigurableToolGroups.UI.BuiltInRootProviders;

public abstract class DevToolCustomRootElement<T, TTool>(T button, ToolButtonService toolButtonService) : BuiltInButtonCustomRootElement<T>(button)
    where T : IBottomBarElementsProvider
    where TTool : ITool
{

    public override IEnumerable<IToolHotkeyDefinition> GetHotkeys()
    {
        var id = $"Tool.{typeof(TTool).Name}";

        var toolButton = toolButtonService.GetToolButton<TTool>();
        return [new ButtonToolHotkeyDefinition(id, id + ".Name", toolButton) 
        {
            Order = int.MaxValue,
            IsDevTool = true,
        }];
    }

}

public class BeaverGeneratorButtonCustomRootElement(BeaverGeneratorButton button, ToolButtonService toolButtonService)
    : DevToolCustomRootElement<BeaverGeneratorButton, BeaverGeneratorTool>(button, toolButtonService)
{ }

public class BotGeneratorButtonCustomRootElement(BotGeneratorButton button, ToolButtonService toolButtonService)
    : DevToolCustomRootElement<BotGeneratorButton, BotGeneratorTool>(button, toolButtonService)
{ }

public class WaterHeightBrushButtonCustomRootElement(WaterHeightBrushButton button, ToolButtonService toolButtonService)
    : DevToolCustomRootElement<WaterHeightBrushButton, WaterHeightBrushTool>(button, toolButtonService)
{ }
