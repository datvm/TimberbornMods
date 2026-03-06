namespace TimberLive.Helpers;

public static class BlueprintExtensions
{
    public static string GetTypeName<T>() where T : ParsedComponentSpec => ParsedComponentSpec.ModelTypeMapping[typeof(T)];

    extension(HttpComponentSpec spec)
    {
        public T Parse<T>(HttpBlueprint blueprint) where T : ParsedComponentSpec
        {
            var json = (JsonElement)spec.SerializableData;
            var parsed = json.Deserialize<T>() ?? throw new InvalidOperationException($"Failed to deserialize component spec of type {spec.TypeName}");
            parsed.Blueprint = blueprint;

            return parsed;
        }
    }

    extension(ParsedComponentSpec spec)
    {
        public T GetComponent<T>() where T : ParsedComponentSpec
            => spec.Blueprint.GetComponent<T>();
    }

    extension(HttpBlueprint blueprint)
    {

        public T GetComponent<T>(string? typeName = null) where T : ParsedComponentSpec
            => blueprint.GetOptionalComponent<T>(typeName) 
            ?? throw new InvalidOperationException($"Component of type {typeof(T).Name} not found in blueprint.");

        public T? GetOptionalComponent<T>(string? typeName = null) where T : ParsedComponentSpec
        {
            typeName ??= GetTypeName<T>();
            var spec = blueprint.Specs.FirstOrDefault(s => s.TypeName == typeName);

            return spec?.Parse<T>(blueprint);
        }
    }

}
