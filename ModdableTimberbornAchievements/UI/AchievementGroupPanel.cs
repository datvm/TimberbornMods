namespace ModdableTimberbornAchievements.UI;

public class AchievementGroupPanel : CollapsiblePanel
{

    readonly Image icon;
    readonly ModdableAchievementSpecService specs;
    readonly ILoc t;
    readonly ModdableAchievementUnlocker unlocker;

    public ImmutableArray<AchievementElement> AchievementElements { get; private set; } = [];
    public ModdableAchievementGroupSpec GroupSpec { get; private set; } = null!;

    public AchievementGroupPanel(
        ModdableAchievementSpecService specs,
        ILoc t,
        ModdableAchievementUnlocker unlocker
    )
    {
        this.specs = specs;
        this.t = t;
        this.unlocker = unlocker;
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

            els.Add(Container.AddChild(() => new AchievementElement(ach, t, isUnlocked, showSecret)));
        }

        AchievementElements = [.. els];
        SetTitle(t.T("LV.MTA.GroupTitle", grp.Name.Value, unlockedCount, achs.Length));

        return this;
    }

}
