namespace TimberModBuilder.Services;

public record ModBuilderBlueprint(
    string Id,
    string TypeName,
    Dictionary<string, object> Content,
    Dictionary<string, ModBuilderBlueprint>? SubBlueprints = null,
    string? overridingFileName = null)
{

    public string TypeForFileName => TypeName.Replace("Spec", "");
    public string FileName => overridingFileName ?? $"{TypeForFileName}.{Id}.json";

    public string ToJson()
    {
        var values = new Dictionary<string, object>()
        {
            [TypeName] = Content
        };

        if (SubBlueprints?.Any() == true)
        {
            foreach (var (name, bp) in SubBlueprints)
            {
                values[name] = bp;
            }
        }

        return JsonConvert.SerializeObject(values, Formatting.Indented);
    }

}