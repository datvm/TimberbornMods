using ScientificProjects.Extensions;

namespace ScientificProjects.UI;

partial class ScientificProjectDialog
{
    int dailyCost = 0;
    Label lblDailyCost = null!;
    VisualElement pnlNotEnough = null!;
    Label lblScience = null!;
    bool isPaying;

    VisualElement CreateScienceStatus(VisualElement container)
    {
        var content = container.AddChild().SetMarginBottom();

        var scienceBar = content.AddChild().SetAsRow();
        {
            lblDailyCost = scienceBar.AddGameLabel(name: "DailyCost")
                .SetFlex101();
            lblScience = scienceBar.AddGameLabel(name: "CurrentScience")
                .SetFlex101();
        }

        pnlNotEnough = content.AddChild();
        {
            pnlNotEnough.AddGameLabel("LV.SP.NotEnoughScienceNextDay".T(t).Color(TimberbornTextColor.Red));

            var buttons = pnlNotEnough.AddChild().SetAsRow().SetMarginBottom();
            buttons.AddMenuButton(text: "LV.SP.PayDayCost".T(t), onClick: PayToday);
            buttons.AddChild().SetFlexGrow();
            buttons.AddButton(text: "LV.SP.SkipToday".T(t), onClick: SkipToday, style: UiBuilder.GameButtonStyle.Text);
        }

        pnlNotEnough.ToggleDisplayStyle(false);

        content.AddGameLabel(text: "LV.SP.DayCostNotice".T(t));

        return content;
    }

    void PayToday()
    {
        if (sciences.SciencePoints < dailyCost)
        {
            diagShower.Create()
                .SetMessage("LV.SP.NotEnoughSciencePay".T(t))
                .SetConfirmButton(DoNothing, "Core.OK".T(t))
                .Show();
            return;
        }

        projects.PayForToday(dailyCost);
        diag?.Close();
    }

    void SkipToday()
    {
        diagShower.Create()
            .SetMessage("LV.SP.SkipConfirm".T(t))
            .SetConfirmButton(PerformSkipToday, "Core.OK".T(t))
            .SetCancelButton(DoNothing, "Core.Cancel".T(t))
            .Show();
    }

    void PerformSkipToday()
    {
        // Just close this dialog
        diag?.Close();
    }

    void OnDailyCostChanged()
    {
        SetDailyCost();
        SetCurrentScience(sciences.SciencePoints, dailyCost, projects.ScienceGainedToday);
    }

    void OnCloseButtonClicked()
    {
        if (isPaying)
        {
            SkipToday();
        }
        else
        {
            Close();
        }
    }

    void OnLevelSelected(ScientificProjectSpec spec, int level, ProjectRowInfo info)
    {
        var prevCost = projects.GetCost(spec);
        dailyCost -= prevCost;
        projects.SetLevel(spec, level);

        var cost = projects.GetCost(spec);
        dailyCost += cost;

        info.SetCurrentCost(cost);
        OnDailyCostChanged();
    }

    void SetDailyCost()
    {
        lblDailyCost.text = "LV.SP.NextDayCost".T(t, NumberFormatter.Format(dailyCost));
    }

    void SetCurrentScience(int current, int dailyCost, int? gainedYesterday)
    {
        var formattedSci = NumberFormatter.Format(current);

        var text = gainedYesterday is null
            ? "LV.SP.CurrentScience".T(t, formattedSci)
            : "LV.SP.CurrentSciencePlus".T(t, 
                formattedSci,
                (gainedYesterday > 0 ? "+" : "") + NumberFormatter.Format(gainedYesterday.Value));

        if (current < dailyCost)
        {
            text = text.Color(TimberbornTextColor.Red);
        }

        lblScience.text = text;
    }

    void Close()
    {
        diag?.OnUICancelled();
    }

    public void AddNotEnoughScience(OnScientificProjectDailyNotEnoughEvent e)
    {
        isPaying = true;
        pnlNotEnough.ToggleDisplayStyle(true);
    }

}
