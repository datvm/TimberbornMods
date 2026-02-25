namespace ModdableTimberbornAchievements.UI;

[BindTransient(Contexts = BindAttributeContext.Game | BindAttributeContext.MainMenu)]
public class AchievementDialog : DialogBoxElement
{

    public VisualElement AchievementsList { get; }
    readonly IContainer container;
    readonly ModdableAchievementSpecService specs;

    public ImmutableArray<AchievementGroupPanel> AchievementGroupPanels { get; private set; } = [];
    readonly ImmutableArray<IAchievementDialogListModifier> modifiers;

    public bool RevealSecrets { get; private set; }

    public AchievementDialog(
        IContainer container,
        ModdableAchievementSpecService specs,
        ILoc t,
        VisualElementInitializer veInit,
        DevModeManager devModeManager,
        IEnumerable<IAchievementDialogListModifier> modifiers
    )
    {
        this.container = container;
        this.specs = specs;
        this.modifiers = [.. modifiers];

        SetDialogPercentSize(height: .8f);

        SetTitle(t.T("LV.MTA.Achievements"));
        AddCloseButton();

        if (devModeManager.Enabled)
        {
            AddDevButtons();
        }

        var storeAch = container.GetInstance<IStoreAchievements>();
        if (storeAch is ModdableStoreAchievement msa && msa.CanSync)
        {
            Content.AddMenuButton(t.T("LV.MTA.SyncStoreAchievements"), onClick: () => SyncStoreAchievements(msa)).SetMarginBottom(10);
        }

        AchievementsList = Content.AddChild();

        foreach (var m in modifiers)
        {
            m.ModifyDialog(this);
        }

        ShowAchievements(false);

        this.Initialize(veInit);
    }

    void AddDevButtons()
    {
        var devButtons = Content.AddCollapsiblePanel(title: "[DEV Tools]", name: "DevTools").Container;
    }

    void SyncStoreAchievements(ModdableStoreAchievement msa)
    {
        msa.SyncStoreUnlocked();
        ShowAchievements();
    }

    public void ShowAchievements() => ShowAchievements(RevealSecrets);

    public void ShowAchievements(bool showSecrets)
    {
        RevealSecrets = showSecrets;
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
                m.ModifyList(this, showSecrets);
            }
        }
    }

}