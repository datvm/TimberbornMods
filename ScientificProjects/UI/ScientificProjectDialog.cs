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
    readonly List<(VisualElement, List<ProjectRow>)> projectGroupEls = [];

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

        this.SetMaxHeight(Screen.height * .75f);
        style.maxWidth = MathF.Min(600f, Screen.width * .75f);

        CreateScienceStatus(Content);
        list = Content.AddScrollView(name: "ProjectList");

        RefreshContent();
    }

    public void RefreshContent()
    {
        list.Clear();
        projectGroupEls.Clear();
        dailyCost = 0;

        ReloadList();
        OnDailyCostChanged();
    }

    void ReloadList()
    {
        var groups = projects.GetAllProjectGroups();

        foreach (var g in groups)
        {
            var groupEl = list.AddChild();
            var projectEls = new List<ProjectRow>();
            projectGroupEls.Add((groupEl, projectEls));

            groupEl.AddGameLabel(g.Spec.DisplayName.Bold().Color(TimberbornTextColor.Solid), size: UiBuilder.GameLabelSize.Big);
            groupEl.AddGameLabel(g.Spec.Description.Color(TimberbornTextColor.Solid)).SetMarginBottom();

            var container = groupEl.AddChild();
            foreach (var p in g.Projects)
            {
                var row = CreateProjectRow(container, p);
                projectEls.Add(row);
            }
        }
    }

    ProjectRow CreateProjectRow(VisualElement container, ScientificProjectInfo p)
    {
        var row = container
            .AddChild<ProjectRow>(name: "ProjectRow")
            .SetInfo(p, defaultIcon, t);

        if (p.Spec.HasSteps || p.Spec.HasScalingCost)
        {
            var cost = projects.GetCost(p);
            dailyCost += cost;
            row.ProjectRowInfo.SetCurrentCost(cost);
        }

        row.OnUnlockRequested += (info, _) => RequestUnlock(info);
        row.ProjectRowInfo.OnLevelSelected += OnLevelSelected;

        return row;
    }

    public new DialogBox Show(VisualElementInitializer? initializer, PanelStack panelStack, Action? confirm = default, Action? cancel = default)
    {
        return diag = base.Show(initializer, panelStack, confirm, cancel);
    }

    static void DoNothing() { }

}
