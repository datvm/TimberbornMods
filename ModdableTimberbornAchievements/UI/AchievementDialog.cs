namespace ModdableTimberbornAchievements.UI;

public class AchievementDialog : DialogBoxElement
{

    readonly VisualElement list;
    readonly IContainer container;
    readonly ModdableAchievementSpecService specs;

    Button? btnShowSecrets;

    public AchievementDialog(
        IContainer container,
        ModdableAchievementSpecService specs,
        ILoc t,
        VisualElementInitializer veInit,
        DevModeManager devModeManager
    )
    {
        SetDialogPercentSize(height: .8f);

        this.container = container;
        this.specs = specs;

        SetTitle(t.T("LV.MTA.Achievements"));
        AddCloseButton();

        if (devModeManager.Enabled)
        {
            btnShowSecrets = Content.AddGameButtonPadded("[DEV] Show secret achievements details", onClick: () => ShowAchievements(true))
                .SetMarginBottom();
        }

        list = Content.AddChild();
        ShowAchievements(false);

        this.Initialize(veInit);
    }

    void ShowAchievements(bool showSecrets)
    {
        if (showSecrets)
        {
            btnShowSecrets?.RemoveFromHierarchy();
            btnShowSecrets = null;
        }

        list.Clear();
        foreach (var grp in specs.AchievementGroups)
        {
            list.AddChild(() => container.GetInstance<AchievementGroupPanel>().Init(grp, showSecrets));
        }

    }

}
