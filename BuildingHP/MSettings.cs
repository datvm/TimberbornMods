namespace BuildingHP;

public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{

    public override string ModId { get; } = nameof(BuildingHP);

    public ModSetting<bool> EnableMaintenance { get; } = new(true, ModSettingDescriptor
        .CreateLocalized("LV.BHP.EnableMaintenance")
        .SetLocalizedTooltip("LV.BHP.EnableMaintenanceDesc"));

}
