global using Timberborn.BuildingTools;
global using Timberborn.InputSystem;
global using UiBuilder.CommonUi;

namespace ScientificProjects.UI;

public class ScientificProjectDialog : DialogBoxElement
{
    const int IconSize = 32;

    readonly ILoc t;
    readonly ScientificProjectService projects;
    readonly DialogBoxShower diagShower;
    readonly InputService input;
    readonly Texture2D defaultIcon;

    ScrollView list = null!;
    readonly List<(VisualElement, List<VisualElement>)> projectGroupEls = [];

    public ScientificProjectDialog(ILoc t, ScientificProjectService projects, IAssetLoader assets, DialogBoxShower diagShower, InputService input)
    {
        this.t = t;
        this.projects = projects;
        this.diagShower = diagShower;
        this.input = input;

        defaultIcon = assets.Load<Texture2D>("Sprites/TopBar/Science");

        Init();
    }

    void Init()
    {
        SetTitle("LV.SP.Title".T(t));
        AddCloseButton();

        this.SetMaxHeight(Screen.height * .75f);
        style.maxWidth = MathF.Min(600f, Screen.width * .75f);

        list = Content.AddScrollView(name: "ProjectList");

        RefreshContent();
    }

    public void RefreshContent()
    {
        list.Clear();
        projectGroupEls.Clear();

        list.AddGameLabel(text: "LV.SP.DayCostNotice".T(t));

        ReloadList();
    }

    void ReloadList()
    {
        var groups = projects.GetAllProjectGroups();

        foreach (var g in groups)
        {
            var groupEl = list.AddChild();
            var projectEls = new List<VisualElement>();
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

    VisualElement CreateProjectRow(VisualElement container, ScientificProjectInfo p)
    {
        var spec = p.Spec;

        var row = container.AddChild(name: "Project").SetAsRow().SetMarginBottom();
        row.style.alignItems = Align.Center;

        var img = row.AddImage(spec.Icon ?? defaultIcon)
            .SetSize(IconSize, IconSize)
            .SetMarginRight()
            .SetFlexShrink();

        var info = row
            .AddChild(name: "Info")
            .SetMarginRight()
            .SetFlexGrow();
        {
            var display = spec.DisplayName.Bold();
            if (p.Unlocked && !spec.HasSteps)
            {
                display = display.Italic();
            }
            info.AddGameLabel(display, size: UiBuilder.GameLabelSize.Big);
            info.AddGameLabel(spec.Effect).SetMarginBottom();

            if (spec.HasScalingCost)
            {
                AddScienceCostLine(info, spec.ScalingCostDisplay!);
            }
            else if (spec.HasSteps)
            {
                AddScienceCostLine(info, "LV.SP.ScienceStepCost".T(t, spec.ScienceCost));
            }

            if (p.PreqProject?.Unlocked == false)
            {
                info.AddGameLabel("LV.SP.PreqLocked".T(t, p.PreqProject.Spec.DisplayName).Color(TimberbornTextColor.Red));
            }

            if (spec.HasSteps && p.Unlocked)
            {
                var slider = info
                    .AddSliderInt(
                        label: "LV.SP.Level".T(t),
                        values: new(0, spec.MaxSteps, p.Level)
                    )
                    .AddEndLabel(p.Level.ToString());

                slider.RegisterChangeCallback(value =>
                {
                    var v = value.newValue;

                    slider.AddEndLabel(v.ToString());
                    OnLevelSelected(spec, v, info);
                });
            }

            if (spec.HasSteps || spec.HasScalingCost)
            {
                var cost = projects.GetCost(p);

                var label = info.AddGameLabel(name: "CurrentCost");
                SetCurrentCost(label, cost);
            }
        }

        if (!spec.HasSteps)
        {
            if (p.Unlocked)
            {
                row.AddLabel("LV.SP.Unlocked".T(t))
                    .SetFlexShrink();
            }
            else
            {
                var btn = row.AddChild<ScienceButton>(name: "UnlockSection");
                btn.Cost = p.Spec.ScienceCost;
                btn.RegisterCallback<ClickEvent>(_ => RequestUnlock(p));
            }
        }

        return row;
    }

    public bool ForceUnlock => input.IsKeyHeld(BuildingToolLocker.InstantUnlockKey);

    void RequestUnlock(ScientificProjectInfo p)
    {
        var status = projects.TryToUnlock(p.Spec.Id, ForceUnlock);
        if (status is not null)
        {
            ShowUnlockError(status.Value);
            return;
        }

        RefreshContent();
    }

    void AddScienceCostLine(VisualElement parent, string cost)
    {
        parent.AddChild<ScienceCostLine>()
            .AddIcon(defaultIcon, "LV.SP.CustomCost".T(t))
            .SetCost(cost);
    }

    void ShowUnlockError(ScientificProjectUnlockStatus error)
    {
        diagShower.Create()
            .SetMessage($"LV.SP.UnlockErr{error}".T(t))
            .SetConfirmButton(DoNothing, "Core.OK".T(t))
            .Show();
    }

    void OnLevelSelected(ScientificProjectSpec spec, int level, VisualElement info)
    {
        projects.SetLevel(spec, level);

        var cost = projects.GetCost(spec);

        var label = info.Q<Label>("CurrentCost");
        if (label is not null)
        {
            SetCurrentCost(label, cost);
        }
    }

    void SetCurrentCost(Label label, int cost)
    {
        label.text = "LV.SP.CurrentCost".T(t, NumberFormatter.Format(cost));
    }

    public void AddDevMode()
    {
        var row = this.AddChild().SetAsRow().SetMarginBottom();

        row.AddGameLabel("Dev buttons: ");
        row.AddGameButton("Clear all unlocks", DevClearAllUpgrades);

        row.InsertSelfBefore(Content.Children().First());
    }

    void DevClearAllUpgrades()
    {
        projects.ClearAllUpgrades();
        RefreshContent();
    }

    static void DoNothing() { }

}
