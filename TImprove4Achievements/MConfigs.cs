global using TImprove4Achievements.Services;
global using TImprove4Achievements.UI;

namespace TImprove4Achievements;

[Context("Game")]
public class MGameConfigs : Configurator
{
    static readonly Type[] HelperTypes = [.. typeof(MGameConfigs).Assembly
        .GetTypes()
        .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(BaseAchievementHelper)))];


    public override void Configure()
    {
        this
            .BindSingleton<AchievementHelperService>()

            .BindSingleton<AchievementHelperDialogShower>()
            .BindTransient<AchievementHelperDialog>();
        ;

        foreach (var helper in HelperTypes)
        {
            this.MultiBind(typeof(BaseAchievementHelper), helper).AsSingleton();
        }
    }

}
