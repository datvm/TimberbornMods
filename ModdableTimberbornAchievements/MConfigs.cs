namespace ModdableTimberbornAchievements;

[Context("MainMenu")]
public class MMainMenuConfig : MainMenuAttributeConfigurator;

[Context("Game")]
public class MGameConfig : GameAttributeConfigurator
{
    public override void Configure()
    {
        base.Configure();

        this
            .BindAlertFragment<AchievementAlert>()
        ;
    }
}