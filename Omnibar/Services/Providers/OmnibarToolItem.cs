namespace Omnibar.Services.Providers;

public class OmnibarToolItem : IOmnibarItem
{
    public string Title { get; }
    public Sprite? Sprite { get; }

    public ToolButton ToolButton { get; }
    public IOmnibarDescriptor? Description { get; }

    public OmnibarToolItem(
        ToolButton toolButton,
        ILoc t,
        IContainer container
    )
    {
        ToolButton = toolButton;

        var (title, desc) = GetToolInfo(
            toolButton.Tool,
            t,
            container
        );
        Title = title ?? "N/A";
        Description = desc;

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

    static (string?, IOmnibarDescriptor?) GetToolInfo(
        Tool tool,
        ILoc t
,
        IContainer container)
    {
        string? title = null;
        IOmnibarDescriptor? desc = null;

        switch (tool)
        {
            case CursorTool:
                title = "LV.OB.Cursor".T(t);
                break;
            case PlantingTool pt:
                title = pt.PlantableSpec.GetComponentFast<LabeledEntitySpec>().DisplayNameLocKey.T(t);
                desc = new PlantingToolDescriptor(container, pt.PlantableSpec.GetComponentFast<GrowableSpec>());
                break;
            case BuilderPriorityTool bt:
                title = $"{"ToolGroups.Priority".T(t)}: {bt.Description().Title}";
                goto default;
            case BlockObjectTool bot:
                title = bot.Prefab.GetComponentFast<LabeledEntitySpec>().DisplayNameLocKey.T(t);
                desc = new BuildingToolDescriptor(bot, container);
                break;
            default:
                if (tool.DevModeTool)
                {
                    title = tool.GetType().Name;
                    desc = new SimpleLabelDescriptor("This is a DEV TOOL!");
                    break;
                }

                var toolDesc = tool.Description();
                title ??= toolDesc?.Title;

                var descText = toolDesc?.Sections.FirstOrDefault().Content;
                if (!string.IsNullOrEmpty(descText))
                {
                    desc = new SimpleLabelDescriptor(descText!);
                }

                break;
        }

        return (title, desc);
    }

}
