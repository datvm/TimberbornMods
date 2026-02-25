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
            .MultiBindSingleton<IAchievementDialogListModifier, AchievementDialogHelperModifier>()
        ;

        foreach (var helper in HelperTypes)
        {
            this.MultiBind(typeof(BaseAchievementHelper), helper).AsSingleton();
        }
    }

}
