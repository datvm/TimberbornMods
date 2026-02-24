namespace DecorativePlants;

[BindSingleton(Contexts = BindAttributeContext.MainMenu)]
public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{
    public override string ModId => nameof(DecorativePlants);

    public DecorativePlantsSetting Settings { get; } = new();

}
