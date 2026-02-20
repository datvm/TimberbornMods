namespace BuildingBlueprints.Services;

[BindSingleton]
public class BlueprintWorkshopServiceResolver(IEnumerable<IBlueprintWorkshopService> providers) : ILoadableSingleton
{

    IBlueprintWorkshopService? provider;
    public IBlueprintWorkshopService Provider => provider ?? throw new NotSupportedException();

    public bool IsSupported => provider is not null;

    public void Load()
    {
        provider = providers.FirstOrDefault(p => p.IsSupported);
    }

}
