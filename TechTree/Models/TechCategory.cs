namespace TechTree.Models;

public record TechCategory(TechTreeCategorySpec Spec)
{
    public string Id => Spec.Id;
    public string Name => Spec.Name.Value;

    public ImmutableArray<TechItem> Techs { get; internal set; } = [];

}
