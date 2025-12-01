namespace ModdableTimberbornAchievements.UI;

public class AchievementDialog : DialogBoxElement
{

    public readonly VisualElement AchievementsList;
    readonly IContainer container;
    readonly ModdableAchievementSpecService specs;
    readonly ModdableAchievementUnlocker unlocker;
    readonly DialogService dialogService;

    public ImmutableArray<AchievementGroupPanel> AchievementGroupPanels { get; private set; } = [];
    ImmutableArray<IAchievementDialogListModifier> modifiers;

    public AchievementDialog(
        IContainer container,
        ModdableAchievementSpecService specs,
        ILoc t,
        VisualElementInitializer veInit,
        DevModeManager devModeManager,
        ModdableAchievementUnlocker unlocker,
        DialogService dialogService,
        IEnumerable<IAchievementDialogListModifier> modifiers
    )
    {
        this.container = container;
        this.specs = specs;
        this.unlocker = unlocker;
        this.dialogService = dialogService;
        this.modifiers = [.. modifiers];

        SetDialogPercentSize(height: .8f);

        SetTitle(t.T("LV.MTA.Achievements"));
        AddCloseButton();

        if (devModeManager.Enabled)
        {
            AddDevButtons();
        }

        AchievementsList = Content.AddChild();
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

    public void ShowAchievements(bool showSecrets)
    {
        AchievementsList.Clear();
        List<AchievementGroupPanel> els = [];
        foreach (var grp in specs.AchievementGroups)
        {
            els.Add(AchievementsList.AddChild(() => container.GetInstance<AchievementGroupPanel>().Init(grp, showSecrets)));
        }

        AchievementGroupPanels = [.. els];

        if (modifiers.Length > 0)
        {
            foreach (var m in modifiers)
            {
                m.Modify(this, showSecrets);
            }
        }
    }

}