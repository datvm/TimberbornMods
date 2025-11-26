
namespace ModdableTimberbornAchievements;

[Context("Game")]
[Context("MainMenu")]
public class MAllContextConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindSingleton<ModdableAchievementSpecService>()
            .BindSingleton<ModdableAchievementUnlocker>()

            .BindTransient<AchievementDialog>()
            .BindTransient<AchievementGroupPanel>()
        ;
    }
}

[Context("MainMenu")]
public class MMainMenuConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindSingleton<MainMenuDialogShower>()
        ;
    }
}

[Context("Game")]
public class MGameConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindSingleton<GameAchievementDialogShower>()
        ;
    }
}