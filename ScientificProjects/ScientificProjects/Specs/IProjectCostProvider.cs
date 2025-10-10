namespace ScientificProjects.Specs;

public interface IProjectCostProvider
{
    IEnumerable<string> CanCalculateCostForIds { get; }
    int CalculateCost(ScientificProjectSpec project, int level);
}
