namespace MoreHttpApi.Shared;

public record HttpBlueprint(
    string Name,
    HttpBlueprint[] Children,
    HttpComponentSpec[] Specs
);

public record HttpComponentSpec(
    object SerializableData,
    string TypeName
);