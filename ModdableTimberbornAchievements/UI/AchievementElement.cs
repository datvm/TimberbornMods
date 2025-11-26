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
        var shouldShowContent = isUnlocked || !spec.IsSecret || showSecret;
        if (shouldShowContent)
        {
            var name = spec.Name.Value;
            if (isUnlocked)
            {
                name = (name + " - " + t.T("LV.MTA.Unlocked")).Color(TimberbornTextColor.Green).Italic();
            }
            else if (!isUnlocked && spec.IsSecret)
            {
                name = (name + " - " + "[SECRET]").Color(TimberbornTextColor.Red);
            }
            
            content.AddLabel(name.Bold());
            content.AddLabel(spec.Description.Value);
        }
        else
        {
            content.AddLabel(t.T("LV.MTA.Secret"));
        }
    }

}
