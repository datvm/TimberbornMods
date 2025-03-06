namespace ScientificProjects.Management;

public record class OnScientificProjectUnlockedEvent(ScientificProjectSpec Project);
public record class OnScientificProjectLevelChangeEvent(ScientificProjectSpec Project, int Level);
public readonly record struct OnScientificProjectDailyCostChargedEvent(int Cost);
public readonly record struct OnScientificProjectDailyNotEnoughEvent(int Need, int Current);

public record class ScientificProjectGroupInfo(ScientificProjectGroupSpec Spec, IEnumerable<ScientificProjectInfo> Projects);
public record class ScientificProjectInfo(ScientificProjectSpec Spec, bool Unlocked, int Level, int TodayLevel, ScientificProjectInfo? PreqProject);

public enum ScientificProjectUnlockStatus
{
    CanUnlock,
    Unlocked,
    RequirementLocked,
    CostLocked,
}