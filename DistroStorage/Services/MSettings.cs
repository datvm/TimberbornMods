namespace DistroStorage.Services;

[BindSingleton(Contexts = BindAttributeContext.All)]
public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{
    
    public override string ModId => nameof(DistroStorage);

    public ModSetting<int> Range { get; } = new(3, CreateDesc("Range"));
    public ModSetting<float> TransferRate { get; } = new(2, CreateDesc("TransferRate"));

    public ModSetting<bool> StockpileEnableDefault { get; } = new(false, CreateDesc("StockpileEnableDefault"));

    public ModSetting<bool> EnableConstruction { get; } = new(true, CreateDesc("EnableConstruction"));
    public ModSetting<bool> ConstructionEnableDefault { get; } = new(true, CreateDesc("EnableDefault"));

    public ModSetting<bool> EnableManufactory { get; } = new(true, CreateDesc("EnableManufactory"));
    public ModSetting<bool> ManufactoryEnableDefault { get; } = new(true, CreateDesc("EnableDefault"));

    public override void OnBeforeLoad()
    {
        base.OnBeforeLoad();

        ConstructionEnableDefault.Descriptor.SetEnableCondition(() => EnableConstruction.Value);
        ManufactoryEnableDefault.Descriptor.SetEnableCondition(() => EnableManufactory.Value);
    }

    static ModSettingDescriptor CreateDesc(string name) => ModSettingDescriptor
        .CreateLocalized("LV.DS." + name)
        .SetLocalizedTooltip($"LV.DS.{name}Desc");

}