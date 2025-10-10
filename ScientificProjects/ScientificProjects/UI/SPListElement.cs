namespace ScientificProjects.UI;

public class SPListElement : ScrollView
{    
    public ImmutableArray<SPGroupElement> Groups { get; private set; } = [];

    private readonly ScientificProjectRegistry registry;
    private readonly IContainer container;

    public SPListElement(
        ScientificProjectRegistry registry,
        IContainer container
    )
    {
        this.registry = registry;
        this.container = container;

        this.SetFlexGrow().SetFlexShrink()
            .AddClass(UiCssClasses.ScrollGreenDecorated);
        style.minHeight = 0;
    }

    public void ReloadList()
    {
        List<SPGroupElement> groups = [];
        foreach (var spec in registry.AllGroups)
        {
            var grp = this.AddChild(container.GetInstance<SPGroupElement>);
            grp.Initialize(spec);

            groups.Add(grp);
        }

        Groups = [.. groups];
    }

    public void ApplyFilter(ScientificProjectFilter filter)
    {
        foreach (var grp in Groups)
        {
            grp.SetFilter(filter);
        }
    }

    public new void Clear()
    {
        Groups = [];
        base.Clear();
    }

}
