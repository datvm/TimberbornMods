namespace ModdableBindito;

public class TestSpecFrontRunner : ISpecLoader, ISpecModifier, ILoadableSingleton
{
    public int Priority { get; } = 0;

    public void Load()
    {
        Debug.Log($"This class loads before the {nameof(ISpecService)}");
    }

    public T ModifyGetSingleSpec<T>(T current) where T : ComponentSpec
    {
        Debug.Log($"Modifying {typeof(T)}");
        return current;
    }

    public IEnumerable<T> ModifyGetSpecs<T>(IEnumerable<T> current) where T : ComponentSpec
    {
        Debug.Log($"Modifying {typeof(T)}");
        return current;
    }

    public bool TryGetSingSpec<T>([MaybeNullWhen(false)] out T? spec) where T : ComponentSpec
    {
        Debug.Log($"Getting {typeof(T)}");
        spec = default;
        return false;
    }

    public bool TryGetSpecs<T>([MaybeNullWhen(false)] out IEnumerable<T>? specs) where T : ComponentSpec
    {
        Debug.Log($"Getting {typeof(T)}");
        specs = default;
        return false;
    }
}
