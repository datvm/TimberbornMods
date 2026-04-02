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
        TransformDescription(LaunchWonderMaxWellbeingAchievement.AchId, [LaunchWonderMaxWellbeingAchievement.MaxWellbeing]);
        TransformDescription(LaunchWonderExactWellbeingAchievement.AchId, [LaunchWonderExactWellbeingAchievement.RequiredWellbeing]);
        TransformDescription(LaunchWonderDistrictsExactWellbeingAchievement.AchId, [
            LaunchWonderDistrictsExactWellbeingAchievement.RequiredWellbeing,
            LaunchWonderDistrictsExactWellbeingAchievement.DistrictCount1,LaunchWonderDistrictsExactWellbeingAchievement.DistrictCount2
        ]);
        TransformDescription(NameKitSirBeavington3Achievement.AchId, [NameKitSirBeavington3Achievement.RequiredName]);
        TransformDescription(OnlyEggplantsAchievement.AchId, [OnlyEggplantsAchievement.AmountRequired]);
        TransformDescription(ReachPopBeforeCycleAchievement.AchId, [ReachPopBeforeCycleAchievement.RequiredBeavers, ReachPopBeforeCycleAchievement.BeforeCycle]);
        TransformDescription(WorksiteFarFromCenterAchievement.AchId, [WorksiteFarFromCenterAchievement.RequiredDistance]);
        TransformDescription(BuildZiplineLoopAchievement.AchId, [BuildZiplineLoopAchievement.LoopCount]);
        TransformDescription(BuildUndergroundTubewayAchievement.AchId, [BuildUndergroundTubewayAchievement.RequiredLength]);
        TransformDescription(NoFloodForCyclesAchievement.AchId, [NoFloodForCyclesAchievement.RequiredCycles]);

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
