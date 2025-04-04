namespace ModdableBindito.Wrappers;

public class SpecServiceWrapper(
    SerializedObjectReaderWriter serializedObjectReaderWriter,
    IAssetLoader assetLoader,
    BlueprintDeserializer blueprintDeserializer,
    IEnumerable<IBlueprintModifierProvider> blueprintModifierProviders,
    IEnumerable<ISpecServiceFrontRunner> frontRunners) : ISpecService, ILoadableSingleton
{

    readonly SpecService wrapped = new(
            serializedObjectReaderWriter,
            assetLoader,
            blueprintDeserializer,
            blueprintModifierProviders);
    readonly ImmutableArray<ISpecLoader> loaders = [.. frontRunners
            .OfType<ISpecLoader>()
            .OrderByDescending(x => x.Priority)];
    readonly ImmutableArray<ISpecModifier> modifiers = [.. frontRunners
            .OfType<ISpecModifier>()
            .OrderByDescending(x => x.Priority)];

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
            if (tryLoader(loader,out result))
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
