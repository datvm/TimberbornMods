namespace ScientificProjects.UI;

public class ProjectRowInfo : VisualElement
{

    Texture2D defaultIcon = null!;
    ILoc t = null!;

    Label lblCost = null!;

    public event Action<ScientificProjectSpec, int, ProjectRowInfo> OnLevelSelected = delegate { };

    public ProjectRowInfo SetInfo(ScientificProjectInfo p, Texture2D defaultIcon, ILoc t)
    {
        this.defaultIcon = defaultIcon;
        this.t = t;

        this.SetMarginRight()
            .SetFlexGrow();

        var spec = p.Spec;

        var display = spec.DisplayName.Bold();
        if (p.Unlocked && !spec.HasSteps)
        {
            display = display.Italic();
        }
        this.AddGameLabel(display, size: UiBuilder.GameLabelSize.Big);
        this.AddGameLabel(spec.Effect).SetMarginBottom();

        if (spec.HasScalingCost)
        {
            AddScienceCostLine(this, spec.ScalingCostDisplay!);
        }
        else if (spec.HasSteps)
        {
            AddScienceCostLine(this, "LV.SP.ScienceStepCost".T(t, spec.ScienceCost));
        }

        if (p.PreqProject?.Unlocked == false)
        {
            this.AddGameLabel("LV.SP.PreqLocked".T(t, p.PreqProject.Spec.DisplayName).Color(TimberbornTextColor.Red));
        }

        var isUnlockedStep = p.Unlocked && spec.HasSteps;
        if (isUnlockedStep)
        {
            var slider = this
                .AddSliderInt(
                    label: "LV.SP.Level".T(t),
                    values: new(0, spec.MaxSteps, p.Level)
                )
                .AddEndLabel(p.Level.ToString());

            slider.RegisterChangeCallback(value =>
            {
                var v = value.newValue;

                slider.AddEndLabel(v.ToString());
                OnLevelSelected(spec, v, this);
            });
        }

        var costContainer = this.AddChild().SetAsRow();
        {
            lblCost = costContainer.AddGameLabel(name: "CurrentCost").SetMarginRight();
            lblCost.ToggleDisplayStyle(false);

            if (isUnlockedStep)
            {
                costContainer.AddChild().SetFlexGrow();
                costContainer.AddGameLabel(text: "LV.SP.ActiveLevel".T(t, p.TodayLevel), name: "CurrentLevel");
            }
        }

        return this;
    }

    void AddScienceCostLine(VisualElement parent, string cost)
    {
        parent.AddChild<ScienceCostLine>()
            .AddIcon(defaultIcon, "LV.SP.CustomCost".T(t))
            .SetCost(cost);
    }

    public void SetCurrentCost(int cost)
    {
        lblCost.ToggleDisplayStyle(true);
        lblCost.text = "LV.SP.CurrentCost".T(t, NumberFormatter.Format(cost));
    }

}
