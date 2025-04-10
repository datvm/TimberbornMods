namespace ModdableBindito.Wrappers;

public class SpecServiceWrapper : ISpecService, ILoadableSingleton
{

    // For TimberApi
    private readonly Dictionary<Type, List<Lazy<Blueprint>>> _cachedBlueprints;

    readonly SpecService wrapped;
    readonly ImmutableArray<ISpecLoader> loaders;
    readonly ImmutableArray<ISpecModifier> modifiers;

    public SpecServiceWrapper(
        SerializedObjectReaderWriter serializedObjectReaderWriter,
        IAssetLoader assetLoader,
        BlueprintDeserializer blueprintDeserializer,
        IEnumerable<IBlueprintModifierProvider> blueprintModifierProviders,
        IEnumerable<ISpecServiceFrontRunner> frontRunners
    )
    {
        wrapped = new(
            serializedObjectReaderWriter,
            assetLoader,
            blueprintDeserializer,
            blueprintModifierProviders);
        loaders = [.. frontRunners
            .OfType<ISpecLoader>()
            .OrderByDescending(x => x.Priority)];
        modifiers = [.. frontRunners
            .OfType<ISpecModifier>()
            .OrderByDescending(x => x.Priority)];
        _cachedBlueprints = wrapped._cachedBlueprints;
    }

    public T GetSingleSpec<T>() where T : ComponentSpec
    {
        return WrappedGetSpec(
            (ISpecLoader loader, out T? result) => loader.TryGetSingSpec(out result),
            () => wrapped.GetSingleSpec<T>(),
            (modifier, result) => modifier.ModifyGetSingleSpec(result)
        );
    }

    public IEnumerable<T> GetSpecs<T>() where T : ComponentSpec
    {
        return WrappedGetSpec(
            (ISpecLoader loader, out IEnumerable<T>? result) => loader.TryGetSpecs(out result),
            () => wrapped.GetSpecs<T>(),
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

    public void Load()
    {
        wrapped.Load();
    }
}
