namespace ConfigurableToolGroups.UI;

public abstract class CustomRootToolGroupElement(
    ToolGroupService toolGroupService,
    ModdableToolGroupButtonFactory grpButtonFac
) : CustomBottomBarElement
{
    protected readonly ToolGroupService toolGroupService = toolGroupService;

    protected ToolButtonColor Color { get; set; } = ToolButtonColor.Blue;

    protected abstract void AddChildren(ModdableToolGroupButton btn);
    public override IEnumerable<BottomBarElement> GetElements()
    {
        var spec = toolGroupService.GetGroup(Id);
        var btn = grpButtonFac.Create(spec, null, Color);
        AddChildren(btn);

        yield return btn.ToBottomBarElement();
    }
}
