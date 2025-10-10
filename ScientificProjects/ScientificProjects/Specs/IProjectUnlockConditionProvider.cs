namespace ScientificProjects.Specs;

public interface IProjectUnlockConditionProvider
{
    IEnumerable<string> CanCheckUnlockConditionForIds { get; }
    string? CheckForUnlockCondition(ScientificProjectSpec project);
}
