namespace TechTreeBlueprintScript.Specs;

public record ScriptBlueprint(string Name, string Path, FrozenDictionary<string, IBlueprintSpec> Specs)
{

    public T? GetNamedSpec<T>() where T : IBlueprintSpec
        => Specs.TryGetValue(typeof(T).Name, out var spec) ? (T)spec : default;

    public T? GetSpec<T>() where T : class
        => Specs.FirstOrDefault(kv => kv.Value is T).Value as T;

    public object GetSpec(Type type)
        => Specs.First(kv => kv.Value.GetType() == type).Value;

    public bool TryGetSpec<T>([NotNullWhen(true)] out T? spec) where T : class
    {
        spec = GetSpec<T>();
        return spec is not null;
    }

    public string GetTemplateName() => GetSpec<TemplateSpec>()?.TemplateName
        ?? throw new InvalidOperationException($"Blueprint '{Name}' does not have a TemplateSpec");

    public bool HasSpec<T>() where T : IBlueprintSpec => Specs.ContainsKey(typeof(T).Name);

    public bool IsPlant => HasSpec<PlantableSpec>();
    public bool IsPlayerBuilding => HasSpec<BuildingSpec>() && HasSpec<PlaceableBlockObjectSpec>();

}
