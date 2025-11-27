global using DemoModdableAchievements.Achievements;

namespace DemoModdableAchievements;

[Context("Game")]
public class MGameConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindAchievement<DemoAchievement>()
            .BindAchievement<SecretAchievement>()
        ;
    }
}
