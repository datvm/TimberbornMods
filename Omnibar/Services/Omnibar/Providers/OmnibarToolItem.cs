
namespace Omnibar.Services.Omnibar.Providers;

public class OmnibarToolItem : IOmnibarItemWithTodoList
{
    public string Title { get; }
    public Sprite? Sprite { get; }

    public ToolButton ToolButton { get; }
    public IOmnibarDescriptor? Description { get; }

    public BuildingSpec? BuildingSpec { get; }
    public string? BuildingName { get; }
    public string? BuildingDisplayName { get; }

    public bool CanAddToTodoList { get; }
    public IEnumerable<string> HotkeyPrompts { get; private set; } = [];

    readonly IContainer container;

    public OmnibarToolItem(
        ToolButton toolButton,
        ILoc t,
        IContainer container
    )
    {
        ToolButton = toolButton;
        this.container = container;

        var (title, desc, buildingSpec) = GetToolInfo(
            toolButton.Tool,
            t,
            container
        );
        Title = title ?? "N/A";
        Description = desc;
        CanAddToTodoList = buildingSpec;

        BuildingSpec = buildingSpec;
        BuildingName = buildingSpec?.GetComponentFast<PrefabSpec>().Name;
        BuildingDisplayName = buildingSpec?.GetComponentFast<LabeledEntitySpec>().DisplayNameLocKey.T(t);

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

    public void AddToTodoList(bool append)
    {
        var ctrl = container.GetInstance<TodoListController>();
        ctrl.AddBuildingAsync(BuildingName!, BuildingDisplayName!, append);
    }

    public bool SetIcon(Image image)
    {
        if (Sprite is null) { return false; }

        image.sprite = Sprite;
        return true;
    }

    static (string?, IOmnibarDescriptor?, BuildingSpec?) GetToolInfo(
        Tool tool,
        ILoc t,
        IContainer container)
    {
        string? title = null;
        IOmnibarDescriptor? desc = null;
        BuildingSpec? buildingSpec = null;

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
                buildingSpec = GetCanAddToTodoList(bot);
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

        return (title, desc, buildingSpec);
    }

    static BuildingSpec? GetCanAddToTodoList(BlockObjectTool bot)
    {
        var buildingSpec = bot.Prefab.GetComponentFast<BuildingSpec>();
        return buildingSpec && (buildingSpec.ScienceCost > 0 || buildingSpec.BuildingCost.Count > 0) ? buildingSpec : null;
    }

}
