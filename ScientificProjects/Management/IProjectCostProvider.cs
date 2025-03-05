namespace ScientificProjects.Management;

public interface IProjectCostProvider
{
    IEnumerable<string> CanCalculateCostForIds { get; }
    int CalculateCost(ScientificProjectInfo project);
}
