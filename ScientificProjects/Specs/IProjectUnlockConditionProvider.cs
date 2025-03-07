namespace ScientificProjects.Management;

public interface IProjectUnlockConditionProvider
{
    IEnumerable<string> CanCheckUnlockConditionForIds { get; }
    string? CheckForUnlockCondition(ScientificProjectInfo project);
}
