namespace BuildingBlueprints.Services;

[BindSingleton]
public class BlueprintPreviewRepository(
    PreviewFactory previewFactory,
    TemplateNameMapper templateNameMapper
)
{

    readonly Dictionary<string, List<Preview>> previewsByTemplates = [];

    public PreviewEnumerator GetPreviews(IEnumerable<ValueTuple<string, int>> templatesAndCounts)
        => new(templatesAndCounts.Select(kv => (kv.Item1, GetPreviews(kv.Item1, kv.Item2))));

    public IEnumerable<Preview> GetPreviews(string templateName, int required)
    {
        var list = previewsByTemplates.GetOrAdd(templateName);

        if (list.Count < required)
        {
            FillInPreviews(templateName, list, required);
        }

        return list.Take(required);
    }

    void FillInPreviews(string templateName, List<Preview> list, int required)
    {
        var template = templateNameMapper.GetTemplate(templateName);
        var placeable = template.GetSpec<PlaceableBlockObjectSpec>();

        while (list.Count < required)
        {
            list.Add(previewFactory.Create(placeable));
        }
    }

}
