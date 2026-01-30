namespace ConfigurableFaction.Services.SpecAppenders;

public abstract class BaseCollectionModifier<T>(CurrentFactionSettingsProvider factionProvider) : BaseSpecTransformer<T>, ILoadableSingleton
    where T : ComponentSpec
{
    protected FactionUserSetting current = null!;

    public void Load()
    {
        current = factionProvider.CurrentSettings;
    }

    public override T? Transform(T spec) => GetId(spec) switch
    {
        ConfigurableFactionUtils.CommonCollectionId => spec,
        ConfigurableFactionUtils.ModCollectionId => ModifyModCollection(spec),
        _ => current.Clear ? ClearCollection(spec) : spec
    };

    protected abstract string GetId(T spec);
    protected abstract T ModifyModCollection(T spec);
    protected abstract T ClearCollection(T spec);

}
