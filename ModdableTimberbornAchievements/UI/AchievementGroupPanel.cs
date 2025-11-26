namespace ModdableTimberbornAchievements.UI;

public class AchievementGroupPanel : CollapsiblePanel
{

    readonly Image icon;
    readonly ModdableAchievementSpecService specs;
    readonly ILoc t;
    readonly ModdableAchievementUnlocker unlocker;

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
        icon.sprite = grp.Icon.Asset;

        var achs = specs.AchievementsByGroupIds[grp.Id];
        var unlockedCount = 0;
        foreach (var ach in achs)
        {
            var isUnlocked = unlocker.IsUnlocked(ach.Id);
            if (isUnlocked)
            {
                unlockedCount++;
            }

            Container.AddChild(() => new AchievementElement(ach, t, isUnlocked, showSecret));
        }

        SetTitle(t.T("LV.MTA.GroupTitle", grp.Name.Value, unlockedCount, achs.Length));

        return this;
    }

}
