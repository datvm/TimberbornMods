namespace MoreAchievements.Services;

[BindSingleton(Contexts = BindAttributeContext.MainMenu | BindAttributeContext.Game)]
public class AchievementDescriptionUpdater(ModdableAchievementSpecService specs) : ILoadableSingleton
{

    public void Load()
    {
        var achs = specs.AchievementsByIds;

        TransformDescription(NoAchForCyclesAchievement.AchId, [NoAchForCyclesAchievement.RequiredCycles]);
        TransformDescription(DemolishResourcesAchievement.AchId, [DemolishResourcesAchievement.DemolishCount]);
        TransformDescription(FillAllTanksBadwaterAchievement.AchId, [FillAllTanksBadwaterAchievement.MinimumBadwater]);

        void TransformDescription(string id, object[] formatParameters)
        {
            var spec = achs[id];
            SetDescription(spec, string.Format(spec.Description.Value, formatParameters));
        }
    }

    static readonly MethodInfo SetDescriptionMethod = typeof(ModdableAchievementSpec)
        .GetProperty(nameof(ModdableAchievementSpec.Description))
        .GetSetMethod(true);
    static void SetDescription(ModdableAchievementSpec spec, string desc)
    {
        SetDescriptionMethod.Invoke(spec, [new LocalizedText(desc)]);
    }

}
