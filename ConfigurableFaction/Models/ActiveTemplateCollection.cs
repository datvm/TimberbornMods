namespace ConfigurableFaction.Models;

public class ActiveTemplateCollection
{
    readonly ISpecService specs = null!;
    public static readonly ActiveTemplateCollection Empty = new();

    public FrozenSet<string> CollectionIds { get; private set; } = [];

    ActiveTemplateCollection() { }
    public ActiveTemplateCollection(ISpecService specs)
    {
        this.specs = specs;
    }

    public void Aggregate()
    {
        HashSet<string> ids = [];

        foreach (var f in specs.GetSpecs<FactionSpec>())
        {
            ids.AddRange(f.TemplateCollectionIds);
        }

        CollectionIds = [..ids];
    }

}
