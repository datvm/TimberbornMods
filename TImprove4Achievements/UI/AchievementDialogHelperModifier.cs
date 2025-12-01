namespace TImprove4Achievements.UI;

public class AchievementDialogHelperModifier(
    AchievementHelperService service,
    ILoc t,
    DialogService diag
) : IAchievementDialogListModifier
{

    public void Modify(AchievementDialog dialog, bool showSecret)
    {
        ToggleShowSecretButton(dialog, showSecret);
        AddHints(dialog);
    }

    void AddHints(AchievementDialog dialog)
    {
        foreach (var grp in dialog.AchievementGroupPanels)
        {
            foreach (var el in grp.AchievementElements)
            {
                if (!el.ShowDetails || el.IsUnlocked) { continue; }

                var ach = el.Spec;
                if (!service.HelpersByIds.TryGetValue(ach.Id, out var helper)) { continue; }

                var helpersPanel = el.ContentPanel.AddChild().SetMargin(top: 10);

                if (!service.AchievementsByType[helper.AchievementType].IsEnabled)
                {
                    helpersPanel.AddLabel(t.T("LV.T4A.Unavailable"));
                    continue;
                }

                for (int i = 0; i < helper.StepsCount; i++)
                {
                    var z = i;
                    var stepPanel = helpersPanel.AddRow().AlignItems().SetMarginBottom(5);

                    stepPanel.AddLabel(t.T("LV.T4A.HintStep", i + 1, helper.GetStepDescription(i, t)))
                        .SetFlexGrow().SetFlexShrink();
                    stepPanel.AddMenuButton(t.T("LV.T4A.HintAction", i + 1),
                        onClick: () => ActivateStep(helper, z))
                        .SetFlexShrink(0);
                }
            }
        }
    }

    void ToggleShowSecretButton(AchievementDialog dialog, bool showSecret)
    {
        const string Name = $"{nameof(TImprove4Achievements)}RevealSecrets";

        var btn = dialog.Content.Q(Name);
        if (showSecret)
        {
            btn?.RemoveFromHierarchy();
        }
        else
        {
            if (btn is not null) { return; }

            btn = dialog.AddGameButtonPadded(t.T("LV.T4A.ShowSecret"), onClick: () => RevealSecrets(dialog), name: Name)
                .SetMarginLeftAuto()
                .SetMarginBottom();
            btn.InsertSelfBefore(dialog.AchievementsList);
        }
    }

    async void RevealSecrets(AchievementDialog dialog)
    {
        if (await diag.ConfirmAsync("LV.T4A.ShowSecretConfirm", true) != true) { return; }
        dialog.ShowAchievements(true);
    }

    void ActivateStep(BaseAchievementHelper helper, int step)
    {
        TimberUiUtils.LogVerbose(() => $"[{nameof(TImprove4Achievements)}] Activating hint step {step} for achievement {helper.Id}");
        helper.ActivateStep(step);
    }

}
