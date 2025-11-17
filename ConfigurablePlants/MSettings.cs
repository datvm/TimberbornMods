namespace ConfigurablePlants;

public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{
    public static readonly ReadonlyTextModSetting.TextSettings Section = new(TextAnchor.MiddleCenter);

    public override string ModId { get; } = nameof(ConfigurablePlants);

    public static bool WithoutGroundValue { get; private set; }
    public static bool NoConUpValue { get; private set; }

    // Placement Section
    public ReadonlyTextModSetting PlacementSection { get; } = new(ModSettingDescriptor.CreateLocalized("LV.CPl.PlacementSect"), Section);
    public ModSetting<bool> RemoveCorner { get; } = CreateB(nameof(RemoveCorner));
    public ModSetting<bool> RemovePath { get; } = CreateB(nameof(RemovePath));
    public ModSetting<bool> WithoutGround { get; } = CreateB(nameof(WithoutGround), false);
    public ModSetting<bool> NoConUp { get; } = CreateB(nameof(NoConUp), false);

    // All Plants Section
    public ModSetting<float> ReproducibleChanceMultiplier { get; } = CreateF(nameof(ReproducibleChanceMultiplier));

    // Growth Section
    public ReadonlyTextModSetting GrowthSection { get; } = new(ModSettingDescriptor.CreateLocalized("LV.CPl.GrowthSect"), Section);

    // Groups
    public ImmutableArray<MSettingPlantGroup> PlantGroups { get; private set; } = [];
    public float[][] PlantGroupsValues => [.. PlantGroups.Select(g => g.Values)];

    static ModSetting<bool> CreateB(string key, bool defaultValue = true)
        => new(defaultValue, ModSettingDescriptor
            .CreateLocalized("LV.CPl." + key)
            .SetLocalizedTooltip("LV.CPl." + key + "Desc"));

    public static ModSetting<float> CreateF(string key, float defaultValue = 1f)
        => new(defaultValue, ModSettingDescriptor
            .CreateLocalized("LV.CPl." + key)
            .SetLocalizedTooltip("LV.CPl." + key + "Desc"));

    public override void OnAfterLoad()
    {
        base.OnAfterLoad();

        AddPlantGroups();

        NoConUp.Descriptor.SetEnableCondition(() => WithoutGround.Value);

        WithoutGround.ValueChanged += (_, v) => WithoutGroundValue = v;
        WithoutGroundValue = WithoutGround.Value;

        NoConUp.ValueChanged += (_, v) => NoConUpValue = v;
        NoConUpValue = NoConUp.Value;
    }

    void AddPlantGroups()
    {
        PlantGroups = [..MSettingPlantGroup.AllGroups.Select(g =>
        {
            var grp = new MSettingPlantGroup (g);

            AddNonPersistentModSetting(grp.SectionLabel);

            for (int i = 0; i < grp.ModSettings.Length; i++)
            {
                var s = grp.ModSettings[i];

                if (s is not null) 
                {
                    AddCustomModSetting(grp.ModSettings[i], grp.GetId(i));
                }
            }

            return grp;
        })];
    }

}
