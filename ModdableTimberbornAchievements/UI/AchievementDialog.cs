namespace ModdableTimberbornAchievements.UI;

public class AchievementDialog : DialogBoxElement
{

    readonly VisualElement list;
    readonly IContainer container;
    readonly ModdableAchievementSpecService specs;
    readonly ModdableAchievementUnlocker unlocker;
    readonly DialogService dialogService;

    public AchievementDialog(
        IContainer container,
        ModdableAchievementSpecService specs,
        ILoc t,
        VisualElementInitializer veInit,
        DevModeManager devModeManager,
        ModdableAchievementUnlocker unlocker,
        DialogService dialogService
    )
    {
        this.container = container;
        this.specs = specs;
        this.unlocker = unlocker;
        this.dialogService = dialogService;

        SetDialogPercentSize(height: .8f);

        SetTitle(t.T("LV.MTA.Achievements"));
        AddCloseButton();

        if (devModeManager.Enabled)
        {
            AddDevButtons();
        }

        list = Content.AddChild();
        ShowAchievements(false);

        this.Initialize(veInit);
    }

    void AddDevButtons()
    {
        var devButtons = Content.AddCollapsiblePanel(title: "[DEV Tools]").Container;

        devButtons.AddGameButtonPadded("Show secret achievements details", onClick: () => ShowAchievements(true));
        devButtons.AddGameButtonPadded("Disable Steam achievement sync for this section", onClick: () => ModdableStoreAchievement.DisableSyncing = true);
        devButtons.AddGameButtonPadded("Clear unlocked", onClick: ClearUnlocks);
    }

    async void ClearUnlocks()
    {
        if (!await dialogService.ConfirmAsync("Are you sure to clear all? (Reload the game to take effect)")) { return; }

        unlocker.Clear();
    }

    void ShowAchievements(bool showSecrets)
    {
        list.Clear();
        foreach (var grp in specs.AchievementGroups)
        {
            list.AddChild(() => container.GetInstance<AchievementGroupPanel>().Init(grp, showSecrets));
        }

    }

}