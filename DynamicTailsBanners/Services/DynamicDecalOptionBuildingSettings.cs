using ModdableTimberborn.BuildingSettings;

namespace DynamicTailsBanners.Services;

public record DynamicDecalOptionBuildingSettingsModel(
    Guid?[] EntityIds,
    string? SettingsJson
) : EntityIdModelBase(EntityIds)
{

    public static DynamicDecalOptionBuildingSettingsModel Create(DynamicDecalOption comp) => new(
        [..comp.Components.Select(c => c?.EntityId)],
        comp.SerializedSettings
    );

}

[MultiBind(typeof(IBuildingSettings))]
public class DynamicDecalOptionBuildingSettings(ILoc t) 
    : EntityIdBuildingSettingsBase<DynamicDecalOption, DynamicDecalOptionBuildingSettingsModel>(t)
{
    public override string DescribeModel(DynamicDecalOptionBuildingSettingsModel model) => "";

    protected override bool ApplyModel(DynamicDecalOptionBuildingSettingsModel model, DynamicDecalOption target)
    {
        _ = target.ApplyModelAsync(model);
        return true;
    }

    protected override DynamicDecalOptionBuildingSettingsModel GetModel(DynamicDecalOption duplicable)
        => DynamicDecalOptionBuildingSettingsModel.Create(duplicable);
}
