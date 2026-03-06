namespace MoreHttpApi.Services;

[BindSingleton]
public class HttpBlueprintSerializer
{
    public static readonly JsonSerializerSettings SpecSerializerSettings = new()
    {
        Converters = [
            new ComponentSpecConverter(),
            new AssetRefConverter(),
            new UnityValuesConverter(),
            new LocalizedTextConverter(),
        ],
        ContractResolver = new BlueprintContractResolver(),
    };

    public HttpBlueprint GetSerializableBlueprint(Blueprint blueprint) => new(
        blueprint.Name,
        [.. blueprint.Children.Select(GetSerializableBlueprint)],
        [.. blueprint.Specs.Select(SerializeSpec)]
    );

    public JToken SerializeBlueprint(Blueprint blueprint) => JToken.FromObject(GetSerializableBlueprint(blueprint));

    public HttpComponentSpec SerializeSpec(ComponentSpec spec) => new(
        SerializeSpecData(spec),
        spec.GetType().FullName
    );

    public JObject SerializeSpecData(ComponentSpec spec) => JObject.Parse(JsonConvert.SerializeObject(spec, SpecSerializerSettings));

}
