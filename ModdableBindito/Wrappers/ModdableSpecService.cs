
namespace ModdableBindito.Wrappers;

public class ModdableSpecService : SpecService,
    ILoadableSingleton, ISpecService
{

    // Do not remove this, it's for TimberApi
    private readonly new Dictionary<Type, List<Lazy<Blueprint>>> _cachedBlueprints;

    private readonly IEnumerable<ISpecServiceFrontRunner> frontRunners;
    ImmutableArray<ISpecLoader> loaders = [];
    ImmutableArray<ISpecModifier> modifiers = [];

    public ModdableSpecService(
        SerializedObjectReaderWriter serializedObjectReaderWriter,
        IAssetLoader assetLoader,
        BlueprintDeserializer blueprintDeserializer,
        IEnumerable<IBlueprintModifierProvider> blueprintModifierProviders,
        IEnumerable<ISpecServiceFrontRunner> frontRunners
    ) : base(serializedObjectReaderWriter, assetLoader, blueprintDeserializer, blueprintModifierProviders)
    {
        this.frontRunners = frontRunners;
        _cachedBlueprints = base._cachedBlueprints;
    }

    void ILoadableSingleton.Load()
    {
        // Base Load
        base.Load();

        loaders = [.. frontRunners
            .OfType<ISpecLoader>()
            .OrderByDescending(x => x.Priority)];
        modifiers = [.. frontRunners
            .OfType<ISpecModifier>()
            .OrderByDescending(x => x.Priority)];
    }

    T ISpecService.GetSingleSpec<T>()
    {
        return WrappedGetSpec(
            (loader, out result) => loader.TryGetSingSpec(out result),
            base.GetSingleSpec<T>,
            (modifier, result) => modifier.ModifyGetSingleSpec(result)
        );
    }

    IEnumerable<T> ISpecService.GetSpecs<T>()
    {
        return WrappedGetSpec(
            (loader, out result) => loader.TryGetSpecs(out result),
            () => base.GetSpecs<T>(),
            (modifier, result) => modifier.ModifyGetSpecs(result)
        );
    }

    delegate bool TryGetSpecDelegate<T>(ISpecLoader loader, [MaybeNullWhen(false)] out T? spec);

    T WrappedGetSpec<T>(TryGetSpecDelegate<T> tryLoader, Func<T> getOriginalResult, Func<ISpecModifier, T, T> modifySpec)
    {
        var hasResult = false;
        T? result = default;

        foreach (var loader in loaders)
        {
            if (tryLoader(loader, out result))
            {
                hasResult = true;
                break;
            }
        }

        if (!hasResult)
        {
            result = getOriginalResult();
        }

        foreach (var modifier in modifiers)
        {
            result = modifySpec(modifier, result!);
        }

        return result!;
    }

}
