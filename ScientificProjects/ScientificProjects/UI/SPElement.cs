using Timberborn.TimeSystemUI;

namespace ScientificProjects.UI;

public class SPElement(
    ScientificProjectDialogController controller,
    DialogService diag,
    ILoc t,
    ScientificProjectUnlockService unlocks,
    ScientificProjectDailyService dailyService
) : VisualElement
{
    const int IconSize = 32;
    static readonly ImmutableArray<string> FlavorTextClasses = ["flavor-text"];

    Label? lblNextDayCost;

    public ScientificProjectInfo ScientificProjectInfo { get; private set; } = null!;

    public event Action? OnProjectUnlocked;
    public event Action? OnDailyCostChanged;

    public SPElement Initialize(ScientificProjectInfo p)
    {
        this.SetMarginBottom();

        ScientificProjectInfo = p;
        var spec = p.Spec;

        var panel = this.AddRow().AlignItems();

        AddIcon(panel, spec);

        var middlePanel = panel.AddChild()
            .SetFlexGrow().SetFlexShrink()
            .SetMarginRight();

        AddInfoPanel(middlePanel, p, spec);        
        AddCostPanel(middlePanel, p, spec);

        var tailPanel = panel.AddChild()
            .SetFlexShrink(0);
        AddUnlockSection(tailPanel, p, spec);

        return this;
    }

    void AddIcon(VisualElement parent, ScientificProjectSpec spec)
    {
        var img = parent.AddImage(spec.Icon ?? controller.DefaultIcon)
            .SetSize(IconSize, IconSize)
            .SetMarginRight()
            .SetFlexShrink(0);
        img.style.alignSelf = Align.FlexStart;
    }

    void AddInfoPanel(VisualElement parent, ScientificProjectInfo info, ScientificProjectSpec spec)
    {
        var panel = parent.AddChild().SetMarginBottom();

        var header = panel.AddRow().AlignItems();

        var title = spec.DisplayName.Bold();
        if (info.Unlocked && !spec.HasSteps)
        {
            title = title.Italic();
        }
        header.AddGameLabel(title, size: UiBuilder.GameLabelSize.Big);

        if (spec.NeedUnlock && info.Unlocked)
        {
            header.AddGameLabel("LV.SP.Unlocked".T(t)).SetMarginLeftAuto();
        }

        panel.AddGameLabel(spec.Effect);

        if (spec.Lore is not null)
        {
            panel.AddGameLabel(
                text: spec.Lore,
                name: "Lore",
                color: UiBuilder.GameLabelColor.Yellow,
                additionalClasses: FlavorTextClasses
            );
        }
    }

    void AddCostPanel(VisualElement parent, ScientificProjectInfo info, ScientificProjectSpec spec)
    {
        var panel = parent.AddChild();

        if (spec.HasScalingCost)
        {
            AddScienceCostLine(panel, spec.ScalingCostDisplay!);
        }
        else if (spec.HasSteps)
        {
            AddScienceCostLine(panel, t.T("LV.SP.ScienceStepCost", spec.ScienceCost));
        }

        if (info.RequiredProject is not null && !info.RequiredProjectUnlocked)
        {
            panel.AddGameLabel(t.T("LV.SP.PreqLocked", info.RequiredProject.DisplayName));
            return;
        }

        if (!info.Unlocked || !spec.HasSteps) { return; }

        var slider = panel.AddSliderInt(
            label: t.T("LV.SP.Level"),
            values: new(0, spec.MaxSteps, info.Levels.NextDay)
        )
            .AddEndLabel(v => v.ToString())
            .RegisterChange(OnLevelSelected);

        var dailyCostPanel = panel.AddRow().AlignItems();
        lblNextDayCost = dailyCostPanel.AddGameLabel(name: "NextDayCost").SetMarginRight();
        dailyCostPanel.AddGameLabel(t.T("LV.SP.ActiveLevel", info.Levels.Today), name: "CurrentLevel")
            .SetMarginLeftAuto();
        SetDailyCostLabel();
    }

    void OnLevelSelected(int level)
    {
        dailyService.SetLevel(ScientificProjectInfo.Spec, level);
        ScientificProjectInfo = ScientificProjectInfo with
        {
            Levels = ScientificProjectInfo.Levels with { NextDay = level }
        };

        SetDailyCostLabel();
        OnDailyCostChanged?.Invoke();
    }

    void SetDailyCostLabel()
    {
        var cost = dailyService.GetDailyCost(ScientificProjectInfo.Spec);
        lblNextDayCost?.text = t.T("LV.SP.CurrentCost", cost);
    }

    void AddScienceCostLine(VisualElement parent, string cost)
    {
        parent.AddChild<ScienceCostLine>()
            .AddIcon(controller.DefaultIcon, t.T("LV.SP.CustomCost"))
            .SetCost(cost);
    }

    void AddUnlockSection(VisualElement parent, ScientificProjectInfo info, ScientificProjectSpec spec)
    {
        if (!spec.NeedUnlock || info.Unlocked) { return; }

        var unlockPanel = parent.AddChild();

        var btn = unlockPanel.AddChild<ScienceButton>(name: "UnlockSection")
            .SetMarginBottom(10);
        btn.Cost = spec.ScienceCost;
        btn.RegisterCallback<ClickEvent>(_ => OnUnlockRequested());

        if (spec.NeedReload)
        {
            unlockPanel.AddGameLabel(t.T("LV.SP.ReloadNeeded"));
        }
    }

    public bool SetFilter(in ScientificProjectFilter filter)
    {
        var match = ScientificProjectInfo.MatchFilter(filter);
        this.SetDisplay(match);
        return match;
    }

    async void OnUnlockRequested()
    {
        if (controller.ShouldAllowDevUnlock)
        {
            PerformUnlock(true);
        }
        else
        {
            var spec = ScientificProjectInfo.Spec;

            var err = unlocks.CanUnlock(spec);
            if (err is null)
            {
                var confirm = await diag.ConfirmAsync(t.T("LV.SP.UnlockConfirm", spec.DisplayName, spec.ScienceCost));
                if (confirm)
                {
                    PerformUnlock(false);
                }
            }
            else
            {
                diag.Alert(err);
            }
        }
    }

    void PerformUnlock(bool force)
    {
        var err = unlocks.TryToUnlock(ScientificProjectInfo.Spec, force);
        if (err is not null)
        {
            diag.Alert(err);
            return;
        }

        if (ScientificProjectInfo.Spec.NeedReload)
        {
            diag.Alert("LV.SP.ReloadNotice", true);
        }

        OnProjectUnlocked?.Invoke();
    }

}
