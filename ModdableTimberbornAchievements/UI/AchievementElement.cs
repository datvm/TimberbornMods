namespace ModdableTimberbornAchievements.UI;

public class AchievementElement : VisualElement
{

    public VisualElement ContentPanel { get; }
    public ModdableAchievementSpec Spec { get; }
    public bool IsUnlocked { get; private set; }
    public bool ShowDetails { get; private set; }

    public AchievementElement(ModdableAchievementSpec spec, ILoc t, bool isUnlocked, bool showSecret)
    {
        Spec = spec;
        IsUnlocked = isUnlocked;

        this.SetAsRow().SetMarginBottom(10).SetPadding(5)
            .SetBorder(Color.gray);

        var parent = this;

        parent.AddImage(spec.Icon.Asset).SetSize(30).SetMarginRight(10);

        var content = ContentPanel = parent.AddChild().SetFlexGrow().SetFlexShrink();
        ShowDetails = isUnlocked || !spec.IsSecret || showSecret;

        var name = spec.Name.Value;
        if (isUnlocked)
        {
            name = t.T("LV.MTA.UnlockedAchTitle", name);
        }
        else if (spec.IsSecret)
        {
            name = t.T("LV.MTA.SecretAchTitle", name);
        }

        content.AddLabel(name.Bold());

        var desc = ShowDetails ? spec.Description.Value : t.T("LV.MTA.Secret");
        content.AddLabel(desc);
    }
}