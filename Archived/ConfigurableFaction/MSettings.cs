namespace ConfigurableFaction;

public class MSettings(
    ISettings settings,
    ModSettingsOwnerRegistry modSettingsOwnerRegistry,
    ModRepository modRepository,
    FactionOptionsProvider factionOptionsProvider
) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{

    public override string ModId { get; } = nameof(ConfigurableFaction);

    public SettingDialogModSetting Settings { get; } = new(factionOptionsProvider);

}
