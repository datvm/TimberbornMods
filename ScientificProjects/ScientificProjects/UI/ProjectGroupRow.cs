using ScientificProjects.Extensions;

namespace ScientificProjects.UI;

public class ProjectGroupRow : VisualElement
{
    Texture2D defaultIcon = null!;
    ILoc t = null!;

    VisualElement projectList = null!;
    Button btnCollapse = null!;
    Button btnExpand = null!;

    readonly List<ProjectRow> projectRows = [];
    public IReadOnlyList<ProjectRow> ProjectRows => projectRows;

    public ScientificProjectGroupInfo GroupInfo { get; private set; } = null!;

    public event Action<ScientificProjectGroupInfo> OnGroupCollapsedToggled = delegate { };

    public ProjectGroupRow SetFilter(in ScientificProjectFilter filter)
    {
        var hasMatch = false;
        foreach (var row in projectRows)
        {
            if (row.SetFilter(filter)) { hasMatch = true; }
        }

        this.ToggleDisplayStyle(hasMatch);

        return this;
    }

    public ProjectGroupRow SetInfo(ScientificProjectGroupInfo g, Texture2D defaultIcon, ILoc t)
    {
        this.defaultIcon = defaultIcon;
        this.t = t;
        GroupInfo = g;

        var header = this.AddChild().SetAsRow();
        {
            var headerText = header.AddChild().SetFlexGrow().SetFlexShrink(1);

            headerText.AddGameLabel(g.Spec.DisplayName.Bold().Color(TimberbornTextColor.Solid), size: UiBuilder.GameLabelSize.Big);
            headerText.AddGameLabel(g.Spec.Description.Color(TimberbornTextColor.Solid)).SetMarginBottom();

            btnExpand = header.AddPlusButton(name: "ExpandGroup", size: UiBuilder.GameButtonSize.Medium).AddAction(ToggleCollapse).SetFlexShrink();
            btnCollapse = header.AddMinusButton(name: "CollapseGroup", size: UiBuilder.GameButtonSize.Medium).AddAction(ToggleCollapse).SetFlexShrink();
        }

        projectList = this.AddChild();

        SetCollapsedVisibility();
        return this;
    }

    public ProjectRow AddProject(ScientificProjectInfo p)
    {
        var row = CreateProjectRow(projectList, p);
        projectRows.Add(row);

        return row;
    }

    ProjectRow CreateProjectRow(VisualElement container, ScientificProjectInfo p)
    {
        var row = container
            .AddChild<ProjectRow>(name: "ProjectRow")
            .SetInfo(p, defaultIcon, t, this);

        return row;
    }

    void ToggleCollapse()
    {
        OnGroupCollapsedToggled(GroupInfo);
        SetCollapsedVisibility();
    }

    void SetCollapsedVisibility()
    {
        projectList.ToggleDisplayStyle(!GroupInfo.Collapsed);
        btnCollapse.ToggleDisplayStyle(!GroupInfo.Collapsed);
        btnExpand.ToggleDisplayStyle(GroupInfo.Collapsed);
    }

}
