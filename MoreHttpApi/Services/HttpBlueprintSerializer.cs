namespace MoreHttpApi.Services;

[BindSingleton]
public class HttpBlueprintSerializer
{

    public HttpBlueprint GetSerializableBlueprint(Blueprint blueprint) => new(
        blueprint.Name,
        [.. blueprint.Children.Select(GetSerializableBlueprint)],
        [.. blueprint.Specs.Select(SerializeSpec)]
    );

    public JToken SerializeBlueprint(Blueprint blueprint) => JToken.FromObject(GetSerializableBlueprint(blueprint));

    public HttpComponentSpec SerializeSpec(ComponentSpec spec) => new(
        spec.Serialize(),
        spec.GetType().FullName
    );

}
