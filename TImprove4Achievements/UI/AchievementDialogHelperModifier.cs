namespace TImprove4Achievements.UI;

public class AchievementDialogHelperModifier(
    AchievementHelperService service,
    ILoc t,
    DialogService diag,
    ModdableAchievementUnlocker unlocker
) : IAchievementDialogListModifier
{

#nullable disable
    Toggle chkRevealSecret;
    AchievementDialog diagAchievements;

    TextField txtFilter;
    Toggle chkFilterLocked, chkFilterUnlocked;
#nullable enable

    public void ModifyDialog(AchievementDialog dialog)
    {
        diagAchievements = dialog;

        var achievementList = dialog.AchievementsList;

        var newOptions = achievementList.AddChild().SetMarginBottom(10);
        newOptions.InsertSelfBefore(achievementList);

        var options = newOptions.AddRow().AlignItems().SetMarginBottom(10);
        
        chkRevealSecret = options.AddToggle(t.T("LV.T4A.ShowSecret"), onValueChanged: v => OnRevealSecretChanged(v));
        options.AddChild().SetMarginLeftAuto();
        options.AddGameButtonPadded(t.T("LV.T4A.ClearUnlocked"), onClick: ClearUnlocks);

        var filter = newOptions.AddRow().AlignItems().SetMarginBottom(10);
        filter.AddLabel(t.T("LV.T4A.Filter")).SetMarginRight().SetFlexShrink(0);
        txtFilter = filter.AddTextField(changeCallback: _ => Refilter()).SetFlexGrow().SetMarginRight(5);
        chkFilterLocked = filter.AddToggle(t.T("LV.T4A.FilterLocked"), onValueChanged: _ => Refilter())
            .SetFlexShrink(0).SetMarginRight(5);
        chkFilterUnlocked = filter.AddToggle(t.T("LV.T4A.FilterUnlocked"), onValueChanged: _ => Refilter())
            .SetFlexShrink(0);
        
        chkFilterLocked.SetValueWithoutNotify(true);
        chkFilterUnlocked.SetValueWithoutNotify(true);

        void Refilter() => Filter(dialog);
    }

    public void ModifyList(AchievementDialog dialog, bool showSecret)
    {
        AddHints(dialog);
        Filter(dialog);
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

    async void ClearUnlocks()
    {
        if (!await diag.ConfirmAsync("LV.T4A.ClearUnlockedConfirm", true)) { return; }
        unlocker.Clear();

        diagAchievements.ShowAchievements();

        diag.Alert("LV.T4A.ClearDone", true);
    }

    async void OnRevealSecretChanged(bool reveal)
    {
        if (reveal && !await diag.ConfirmAsync("LV.T4A.ShowSecretConfirm", true))
        {
            chkRevealSecret.SetValueWithoutNotify(false);
            return;
        }

        diagAchievements.ShowAchievements(reveal);
    }

    void ActivateStep(BaseAchievementHelper helper, int step)
    {
        TimberUiUtils.LogVerbose(() => $"[{nameof(TImprove4Achievements)}] Activating hint step {step} for achievement {helper.Id}");
        helper.ActivateStep(step);
    }

    void Filter(AchievementDialog diag)
    {
        var kw = txtFilter.value.Trim();
        var hasKw = kw.Length > 0;

        var showLocked = chkFilterLocked.value;
        var showUnlocked = chkFilterUnlocked.value;

        foreach (var grp in diag.AchievementGroupPanels)
        {
            foreach (var a in grp.AchievementElements)
            {
                a.SetDisplay(Match(a));
            }
        }

        bool Match(AchievementElement a)
        {
            var isUnlocked = a.IsUnlocked;
            return ((isUnlocked && showUnlocked) || (!isUnlocked && showLocked)) 
                && (!hasKw
                || a.Spec.Name.Value.Contains(kw, StringComparison.CurrentCultureIgnoreCase)
                || (a.ShowDetails && a.Spec.Description.Value.Contains(kw, StringComparison.CurrentCultureIgnoreCase)));
        }
    }

}
