namespace MoreHttpApi.Shared;

public static class ParsedComponentSpecHelper
{

    public static string MapTypeName(Type type) => ParsedComponentSpec.ModelTypeMapping[type];

    extension (HttpBlueprint blueprint)
    {
        public bool HasComponent<T>() where T : ParsedComponentSpec => blueprint.HasComponent(typeof(T));

        public bool HasComponent(Type type)
        {
            var name = MapTypeName(type);
            return blueprint.Specs.Any(s => s.TypeName == name);
        }

    }

}
