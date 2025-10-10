namespace ScientificProjects.UI;

public class SPGroupElement(
    ScientificProjectGroupService groupService,
    ScientificProjectService sp,
    IContainer container,
    VisualElementInitializer veInit
) : CollapsiblePanel
{

    public ImmutableArray<SPElement> ProjectElements { get; private set; } = [];
    public ScientificProjectGroupSpec GroupSpec { get; private set; } = null!;

    public void Initialize(ScientificProjectGroupSpec g)
    {
        GroupSpec = g;

        SetHeader();
        AddProjects();

        var shouldCollapse = groupService.ShouldCollapse(g.Id);
        SetExpand(!shouldCollapse);

        ExpandChanged += OnExpandChanged;
    }

    void OnExpandChanged(bool expand)
    {
        groupService.SetCollapsed(GroupSpec.Id, !expand);
    }

    public void SetFilter(in ScientificProjectFilter filter)
    {
        var hasMatch = false;
        foreach (var row in ProjectElements)
        {
            if (row.SetFilter(filter)) { hasMatch = true; }
        }

        this.SetDisplay(hasMatch);
    }

    void SetHeader()
    {
        var originalLabel = HeaderLabel;

        var headerContent = originalLabel.AddChild().SetFlexGrow();
        headerContent.AddGameLabel(GroupSpec.DisplayName.Bold().Color(TimberbornTextColor.Solid), size: UiBuilder.GameLabelSize.Big);
        headerContent.AddGameLabel(GroupSpec.Description.Color(TimberbornTextColor.Solid)).SetMarginBottom();

        headerContent.InsertSelfBefore(originalLabel);
        originalLabel.RemoveFromHierarchy();
    }

    void AddProjects()
    {
        List<SPElement> projects = [];
        foreach (var info in sp.GetGroupProjects(GroupSpec.Id))
        {
            var el = Container.AddChild(container.GetInstance<SPElement>);

            el.Initialize(info);
            el.Initialize(veInit);
            projects.Add(el);
        }

        ProjectElements = [.. projects];
    }

}
