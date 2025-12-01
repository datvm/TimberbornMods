namespace TImprove4Achievements.Services;

public abstract class BaseAchievementHelper
{
    public string Id { get; protected set; } = null!;
    public virtual bool Secret { get; }

    public bool Enabled => Achievement.IsEnabled;

    public abstract int StepsCount { get; }
    public abstract string GetStepDescription(int step, ILoc t);

    public abstract Type AchievementType { get; }
    public Achievement Achievement { get; protected set; } = null!;
    public ModdableAchievementSpec AchievementSpec { get; protected set; } = null!;

    public abstract void ActivateStep(int step);

    internal abstract void Initialize(Achievement achievement, ModdableAchievementSpec spec);

}

public abstract class BaseAchievementHelper<T>() : BaseAchievementHelper where T : Achievement
{
    protected ImmutableArray<string> stepLocs = [];

    public override int StepsCount => stepLocs.Length;
    public override string GetStepDescription(int step, ILoc t) => t.T(stepLocs[step]);

    public override Type AchievementType { get; } = typeof(T);
    public new T Achievement { get; private set; } = null!;

    public BaseAchievementHelper(string baseLocKey, int steps) : this()
    {
        stepLocs = CreateStepLocs(baseLocKey, steps);
    }

    internal override void Initialize(Achievement achievement, ModdableAchievementSpec spec)
    {
        base.Achievement = achievement;
        Achievement = (T)achievement;
        Id = achievement.Id;
        AchievementSpec = spec;
    }

    protected static ImmutableArray<string> CreateStepLocs(string key, int steps)
        => [.. Enumerable.Range(0, steps).Select(q => $"{key}.Step{q}")];

}