namespace ConfigurableTopBar;

public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{

    public override string ModId { get; } = nameof(ConfigurableTopBar);

    public ConfigurableTopBarSetting Panel { get; } = new();

}
