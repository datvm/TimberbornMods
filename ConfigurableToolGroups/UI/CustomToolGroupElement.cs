namespace ConfigurableToolGroups.UI;

public abstract class CustomToolGroupElement(
    ToolGroupButtonFactory toolGroupButtonFactory,
    ToolGroupService toolGroupService,
    ToolButtonFactory toolButtonFactory
) : CustomBottomBarElement
{
    protected readonly ToolGroupButtonFactory toolGroupButtonFactory = toolGroupButtonFactory;
    protected readonly ToolGroupService toolGroupService = toolGroupService;
    protected readonly ToolButtonFactory toolButtonFactory = toolButtonFactory;

    protected ToolGroupColor Color { get; set; } = ToolGroupColor.Blue;
    Func<ToolGroupSpec, ToolGroupButton> CreateFunc => Color switch
    {
        ToolGroupColor.Blue => toolGroupButtonFactory.CreateBlue,
        ToolGroupColor.Green => toolGroupButtonFactory.CreateGreen,
        _ => throw new ArgumentOutOfRangeException(nameof(Color), "Invalid tool group color: " + Color),
    };

    protected abstract void AddChildren(ToolGroupButton parentButton);
    public override IEnumerable<BottomBarElement> GetElements()
    {
        var grp = toolGroupService.GetGroup(Id);
        var btn = CreateFunc(grp);

        AddChildren(btn);

        yield return BottomBarElement.CreateMultiLevel(btn.Root, btn.ToolButtonsElement);
    }

    protected void AddToolButtonChild(ToolGroupButton parentButton, IEnumerable<ToolButton> toolButtons)
    {
        foreach (var b in toolButtons)
        {
            toolGroupService.AssignToGroup(parentButton._toolGroup, b.Tool);
            parentButton.AddTool(b);
        }
    }

    protected void AddToolButtonChild(ToolGroupButton parentButton, IEnumerable<(ITool Tool, string Image)> toolButtonsDefinitions)
    {
        var container = parentButton.ToolButtonsElement;
        
        AddToolButtonChild(parentButton, toolButtonsDefinitions
            .Select(d => toolButtonFactory.Create(d.Tool, d.Image, container))
        );
    }

    public enum ToolGroupColor
    {
        Blue,
        Green
    }

}
