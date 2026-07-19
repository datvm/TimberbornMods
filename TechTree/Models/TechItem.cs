namespace TechTree.Models;

public record TechItem(TechTreeItemSpec Spec, TechCategory Category)
{
    public string Id => Spec.Id;
    public string Name => Spec.Name.Value;
}
