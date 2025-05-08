namespace Omnibar.Services.Providers;

public class OmnibarToolItem : IOmnibarItem
{
    public string Title { get; }
    public string? Description { get; }
    public Sprite? Sprite { get; }

    public ToolButton ToolButton { get; }

    public OmnibarToolItem(ToolButton toolButton, ILoc t)
    {
        ToolButton = toolButton;

        var desc = toolButton.Tool.Description();
        Debug.Log(toolButton.Tool.GetType() + ": " + desc?.Title);

        Title = GetToolName(toolButton.Tool, t) ?? "N/A";
        
        try
        {
            Sprite = toolButton.Root.Q("ToolImage")?.style.backgroundImage.value.sprite;
        }
        catch (Exception)
        {
            Sprite = null;
        }
    }

    public void Execute()
    {
        ToolButton.Select();
    }

    public bool SetIcon(Image image)
    {
        if (Sprite is null) { return false; }

        image.sprite = Sprite;
        return true;
    }

    static string? GetToolName(Tool tool, ILoc t)
    {
        switch (tool)
        {
            case CursorTool:
                return "LV.OB.Cursor".T(t);
            case PlantingTool pt:
                return pt.PlantableSpec.GetComponentFast<LabeledEntitySpec>().DisplayNameLocKey.T(t);
            case BuilderPriorityTool bt:
                return $"{"ToolGroups.Priority".T(t)}: {bt.Description().Title}";
            case BlockObjectTool bot:
                return bot.Prefab.GetComponentFast<LabeledEntitySpec>().DisplayNameLocKey.T(t);
            case BeaverGeneratorTool:
            case BotGeneratorTool:
            case WaterHeightBrushTool:
                return tool.GetType().Name;
            default:
                var desc = tool.Description();
                return desc?.Title;
        }
    }

}
