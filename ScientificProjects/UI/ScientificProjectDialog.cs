namespace ScientificProjects.UI;

public partial class ScientificProjectDialog : DialogBoxElement
{

    readonly ILoc t;
    readonly ScientificProjectService projects;
    readonly DialogBoxShower diagShower;
    readonly InputService input;
    readonly ScienceService sciences;
    readonly Texture2D defaultIcon;

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

        Content.style.height = Screen.height * .75f;
        Content.style.maxWidth = MathF.Min(600f, Screen.width * .75f);

        CreateScienceStatus(Content);

        list = Content.AddScrollView(name: "ProjectList")
            .SetFlexGrow(1).SetFlexShrink(1);
        list.style.minHeight = 0;

        RefreshContent();
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
        var groups = projects.GetAllProjectGroups();

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
        }
    }

    public void ToggleGroupCollapse(ScientificProjectGroupInfo info)
    {
        info.Collapsed = !info.Collapsed;
        projects.SetGroupCollapsed(info.Spec.Id, info.Collapsed);
    }

    public new DialogBox Show(VisualElementInitializer? initializer, PanelStack panelStack, Action? confirm = default, Action? cancel = default)
    {
        return diag = base.Show(initializer, panelStack, confirm, cancel);
    }

    static void DoNothing() { }

}
