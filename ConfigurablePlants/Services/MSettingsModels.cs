namespace ConfigurablePlants.Services;

public class MSettingPlantGroup(MSettingPlantGroupType group)
{
    public static readonly ImmutableArray<MSettingPlantGroupProperty> AllProperties = [.. Enum.GetValues(typeof(MSettingPlantGroupProperty))
        .Cast<MSettingPlantGroupProperty>()
        .OrderBy(q => q)];
    public static readonly ImmutableArray<MSettingPlantGroupType> AllGroups = [.. Enum.GetValues(typeof(MSettingPlantGroupType))
        .Cast<MSettingPlantGroupType>()
        .OrderBy(q => q)];

    public MSettingPlantGroupType Group { get; } = group;

    public ReadonlyTextModSetting SectionLabel => new(ModSettingDescriptor.CreateLocalized($"LV.CPl.{Group}Sect"), MSettings.Section);
    public readonly ImmutableArray<ModSetting<float>?> ModSettings = CreateModSettings(group);
    public float[] Values => [.. ModSettings.Select(q => q?.Value ?? 0)];

    public float this[MSettingPlantGroupProperty index] => ModSettings[(int)index]?.Value ?? 0;
    public string GetId(int i) => $"{Group}{AllProperties[i]}";

    static ImmutableArray<ModSetting<float>?> CreateModSettings(MSettingPlantGroupType group)
    {
        ModSetting<float>?[] s = new ModSetting<float>?[AllProperties.Length];

        var isProduct = group == MSettingPlantGroupType.Product;
        for (int i = 0; i < AllProperties.Length; i++)
        {
            var p = AllProperties[i];

            s[i] = isProduct && (p is MSettingPlantGroupProperty.PlantingTime or MSettingPlantGroupProperty.DemolishTime)
                ? null
                : MSettings.CreateF(p.ToString());
        }

        return [.. s];
    }

}

public enum MSettingPlantGroupType
{
    Tree,
    Crop,
    Product,
}

public enum MSettingPlantGroupProperty
{
    PlantingTime,
    GrowthRate,
    OutputMul,
    HarvestTime,
    DemolishTime,
}
