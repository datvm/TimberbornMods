namespace ModdableTimberbornAchievements.UI;

[BindTransient(Contexts = BindAttributeContext.Game | BindAttributeContext.MainMenu)]
public class AchievementGroupPanel : CollapsiblePanel
{

    readonly Image icon;
    readonly ModdableAchievementSpecService specs;
    readonly ILoc t;
    readonly ModdableAchievementUnlocker unlocker;
    readonly DialogService diag;

    public ImmutableArray<AchievementElement> AchievementElements { get; private set; } = [];
    public ModdableAchievementGroupSpec GroupSpec { get; private set; } = null!;

    public event EventHandler? OnResetPerformed;

    public AchievementGroupPanel(
        ModdableAchievementSpecService specs,
        ILoc t,
        ModdableAchievementUnlocker unlocker,
        DialogService diag
    )
    {
        this.specs = specs;
        this.t = t;
        this.unlocker = unlocker;
        this.diag = diag;

        icon = this.AddImage().SetSize(30).SetMarginRight(5);
        icon.InsertSelfBefore(HeaderLabel);

        this.SetMarginBottom();
    }

    public AchievementGroupPanel Init(ModdableAchievementGroupSpec grp, bool showSecret)
    {
        GroupSpec = grp;
        icon.sprite = grp.Icon.Asset;

        var achs = specs.AchievementsByGroupIds[grp.Id];
        var unlockedCount = 0;
        List<AchievementElement> els = [];

        foreach (var ach in achs)
        {
            var isUnlocked = unlocker.IsUnlocked(ach.Id);
            if (isUnlocked)
            {
                unlockedCount++;
            }

            var el = Container.AddChild(() => new AchievementElement(ach, t, isUnlocked, showSecret, true));
            el.OnResetRequested += (_, _) => OnResetRequested(ach.Id);
            els.Add(el);
        }

        AchievementElements = [.. els];
        SetTitle(t.T("LV.MTA.GroupTitle", grp.Name.Value, unlockedCount, achs.Length));

        return this;
    }

    async void OnResetRequested(string id)
    {
        if (!await diag.ConfirmAsync(t.T("LV.MTA.LockAchConfirm")))
        {
            return;
        }

        if (unlocker.Lock(id) && OnResetPerformed is not null)
        {
            OnResetPerformed(this, EventArgs.Empty);
        }
        
    }
}
