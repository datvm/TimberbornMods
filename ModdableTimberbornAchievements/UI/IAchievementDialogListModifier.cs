namespace ModdableTimberbornAchievements.UI;

public interface IAchievementDialogListModifier
{
    void ModifyDialog(AchievementDialog dialog);
    void ModifyList(AchievementDialog dialog, bool showSecrets);
}
