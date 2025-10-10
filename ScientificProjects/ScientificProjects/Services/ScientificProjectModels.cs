namespace ScientificProjects.Services;

public record class OnScientificProjectUnlockedEvent(ScientificProjectSpec Project);
public record class OnScientificProjectLevelChangeEvent(ScientificProjectSpec Project, int Level);
public readonly record struct OnScientificProjectDailyCostChargedEvent(int Cost);
public readonly record struct OnScientificProjectDailyNotEnoughEvent(int Need, int Current);

public readonly record struct ScientificProjectLevels(int Today, int NextDay)
{
    public static readonly ScientificProjectLevels NoLevel = new(1, 1);
}

public record class ScientificProjectInfo(
    ScientificProjectSpec Spec,
    bool Unlocked,
    ScientificProjectLevels Levels,
    ScientificProjectSpec? RequiredProject,
    bool RequiredProjectUnlocked
)
{
    public bool Active => !Spec.HasSteps || Levels.Today > 0;

    public string TodayName => Spec.HasSteps ? $"{Spec.DisplayName} ({Levels.Today})" : Spec.DisplayName;

    public float GetEffect(int parameterIndex) => Spec.GetEffect(parameterIndex, Levels.Today);
}
