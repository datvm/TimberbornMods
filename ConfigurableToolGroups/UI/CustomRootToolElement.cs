namespace ConfigurableToolGroups.UI;

public abstract class CustomRootToolElement<TTool>(
    ToolButtonFactory toolButtonFactory,
    TTool tool
) : CustomBottomBarElement
    where TTool : ITool
{

    protected RootToolButtonColor Color { get; set; } = RootToolButtonColor.Blue;
    protected abstract string ImageName { get; }

    public override IEnumerable<BottomBarElement> GetElements()
    {
        var btn = toolButtonFactory.CreateGroupless(tool, ImageName, Color);
        yield return BottomBarElement.CreateSingleLevel(btn.Root);
    }
}
