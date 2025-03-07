namespace ScientificProjects.Management;

public record class OnScientificProjectUnlockedEvent(ScientificProjectSpec Project);
public record class OnScientificProjectLevelChangeEvent(ScientificProjectSpec Project, int Level);
public readonly record struct OnScientificProjectDailyCostChargedEvent(int Cost);
public readonly record struct OnScientificProjectDailyNotEnoughEvent(int Need, int Current);

public record class ScientificProjectGroupInfo(ScientificProjectGroupSpec Spec, IEnumerable<ScientificProjectInfo> Projects, bool Collapsed)
{

    public bool Collapsed { get; set; } = Collapsed;

}

public record class ScientificProjectInfo(ScientificProjectSpec Spec, bool Unlocked, int Level, int TodayLevel, ScientificProjectInfo? PreqProject);
