namespace ConfigurableFaction;

[BindSingleton(Contexts = BindAttributeContext.MainMenu)]
public class MSettings(
    UserSettingsService userSettingsService,
    ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{
    public override string ModId => nameof(ConfigurableFaction);

    public ConfigurableFactionSettings Settings { get; } = new(userSettingsService);

}
