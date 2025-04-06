using ScientificProjects.Extensions;

namespace ScientificProjects.UI;

public partial class ScientificProjectDialog : DialogBoxElement
{

    readonly ILoc t;
    readonly ScientificProjectService projects;
    readonly DialogBoxShower diagShower;
    readonly InputService input;
    readonly ScienceService sciences;
    readonly Texture2D defaultIcon;

    FilterBox filterBox = null!;
    ScientificProjectFilter filter = ScientificProjectFilter.Default;

    ScrollView list = null!;
    readonly List<ProjectGroupRow> projectGroupRows = [];

    DialogBox? diag;

    public ScientificProjectDialog(ILoc t, ScientificProjectService projects, IAssetLoader assets, DialogBoxShower diagShower, InputService input, ScienceService sciences)
    {
        this.t = t;
        this.projects = projects;
        this.diagShower = diagShower;
        this.input = input;
        this.sciences = sciences;

        defaultIcon = assets.Load<Texture2D>("Sprites/TopBar/Science");

        Init();
    }

    void Init()
    {
        SetTitle("LV.SP.Title".T(t));
        AddCloseButton(OnCloseButtonClicked);

        CreateScienceStatus(Content);

        filterBox = Content.AddChild<FilterBox>().Init(t);
        filterBox.OnFilterChanged += OnFilterChanged;

        list = Content.AddScrollView(name: "ProjectList")
            .SetFlexGrow(1).SetFlexShrink(1);
        list.style.minHeight = 0;

        RefreshContent();
    }

    void OnFilterChanged(ScientificProjectFilter filter)
    {
        this.filter = filter;
        foreach (var row in projectGroupRows)
        {
            row.SetFilter(filter);
        }
    }

    void SetDialogSize()
    {
        if (panel is null) { return; }

        var scale = panel.scaledPixelsPerPoint;
        Content.style.height = Screen.height * .75f / scale;
        style.maxWidth = Screen.width * .75f / scale;
    }

    public void RefreshContent()
    {
        list.Clear();
        projectGroupRows.Clear();
        dailyCost = 0;

        ReloadList();
        OnDailyCostChanged();
    }

    void ReloadList()
    {
        var groups = projects.GetAllProjectGroups(true);

        ref var filter = ref this.filter;

        foreach (var g in groups)
        {
            var projectGroupRow = list.AddChild<ProjectGroupRow>()
                .SetInfo(g, defaultIcon, t);
            projectGroupRows.Add(projectGroupRow);
            projectGroupRow.OnGroupCollapsedToggled += ToggleGroupCollapse;

            foreach (var p in g.Projects)
            {
                var row = projectGroupRow.AddProject(p);

                if ((p.Spec.HasSteps || p.Spec.HasScalingCost) && p.Unlocked)
                {
                    var cost = projects.GetCost(p);
                    dailyCost += cost;
                    row.ProjectRowInfo.SetCurrentCost(cost);
                }

                row.OnUnlockRequested += (info, _) => RequestUnlock(info);
                row.ProjectRowInfo.OnLevelSelected += OnLevelSelected;
            }

            projectGroupRow.SetFilter(in filter);
        }
    }

    public void ToggleGroupCollapse(ScientificProjectGroupInfo info)
    {
        info.Collapsed = !info.Collapsed;
        projects.SetGroupCollapsed(info.Spec.Id, info.Collapsed);
    }

    public new DialogBox Show(VisualElementInitializer? initializer, PanelStack panelStack, Action? confirm = default, Action? cancel = default)
    {
        diag = base.Show(initializer, panelStack, confirm, cancel);
        SetDialogSize();

        return diag;
    }

    static void DoNothing() { }

}
