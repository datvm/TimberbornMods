namespace ModdableTimberbornAchievements.UI;

public class AchievementElement : VisualElement
{

    public AchievementElement(ModdableAchievementSpec spec, ILoc t, bool isUnlocked, bool showSecret)
    {
        this.SetAsRow().AlignItems().SetMarginBottom(10).SetPadding(5)
            .SetBorder(Color.gray);

        var parent = this;

        parent.AddImage(spec.Icon.Asset).SetSize(30).SetMarginRight(10);

        var content = parent.AddChild().SetFlexGrow().SetFlexShrink();
        var shouldShowDescription = isUnlocked || !spec.IsSecret || showSecret;

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

        var desc = shouldShowDescription ? spec.Description.Value : t.T("LV.MTA.Secret");
        content.AddLabel(desc);
    }
}