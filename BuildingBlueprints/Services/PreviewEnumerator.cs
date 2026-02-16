namespace BuildingBlueprints.Services;

public class PreviewEnumerator
{

    public FrozenDictionary<string, ImmutableArray<Preview>> PreviewsByTemplates { get; }
    readonly Dictionary<string, int> counters = [];

    public ImmutableArray<Preview> AllPreviews { get; }

    public PreviewEnumerator(IEnumerable<ValueTuple<string, IEnumerable<Preview>>> previewsByTemplates)
    {
        PreviewsByTemplates = previewsByTemplates.ToFrozenDictionary(kv => kv.Item1, kv => kv.Item2.ToImmutableArray());

        foreach (var k in PreviewsByTemplates.Keys)
        {
            counters[k] = -1;
        }

        AllPreviews = [.. PreviewsByTemplates.Values.SelectMany(arr => arr)];
    }

    public Preview GetNext(string templateName)
    {
        var previews = PreviewsByTemplates[templateName];
        var index = ++counters[templateName];

        if (index >= previews.Length)
        {
            throw new ArgumentOutOfRangeException($"No more previews available for template '{templateName}'. Requested index: {index}, available: {previews.Length}");
        }

        return previews[index];
    }

    public void Reset()
    {
        foreach (var k in PreviewsByTemplates.Keys)
        {
            counters[k] = -1;
        }
    }

}
