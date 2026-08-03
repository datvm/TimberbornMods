namespace BlueprintReader.Providers;

public abstract class BaseIdProvider<T>(BlueprintProvider blueprintProvider) where T : class, IIdBlueprintSpec
{
    public FrozenDictionary<string, T> ItemById { get; } = blueprintProvider.GetSpecs<T>().ToFrozenDictionary(s => s.Id);

    public T Get(string id) => ItemById[id];
    public T? GetOrNull(string id) => ItemById.TryGetValue(id, out var item) ? item : null;
    public bool TryGet(string id, [NotNullWhen(true)] out T? item) => ItemById.TryGetValue(id, out item);
}

[BindSingleton]
public class RecipeProvider(BlueprintProvider blueprintProvider) : BaseIdProvider<RecipeSpec>(blueprintProvider);