namespace ScientificProjects.Helpers;

public static class ScientificProjectsExtensions
{

    public static int LevelOr0(this IProjectCostProvider _, int level, Func<int, int> calculate)
        => level == 0 ? 0 : calculate(level);
    public static int LevelOr0F(this IProjectCostProvider _, int level, Func<int, float> calculate)
        => level == 0 ? 0 : (int)MathF.Ceiling(calculate(level));
    public static NotSupportedException ThrowNotSupportedEx(this ScientificProjectSpec spec)
        => new($"This implementation does not support {spec}");

    public static bool IsAvailableTo(this ScientificProjectSpec spec, string factionId)
        => spec.Factions.Length == 0 || spec.Factions.Contains(factionId);

    public static bool MatchFilter(this ScientificProjectInfo info, in ScientificProjectFilter filter)
    {
        var spec = info.Spec;
        var (kw, flags) = filter;

        if (!string.IsNullOrWhiteSpace(kw) &&
            !(spec.DisplayName.Contains(kw, StringComparison.OrdinalIgnoreCase)
                || spec.Effect.Contains(kw, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        if ((flags & ScientificProjectFilterFlags.Unlocked) == 0 && info.Unlocked) { return false; }
        if ((flags & ScientificProjectFilterFlags.Locked) == 0 && !info.Unlocked) { return false; }
        if ((flags & ScientificProjectFilterFlags.Daily) == 0 && spec.HasSteps) { return false; }
        if ((flags & ScientificProjectFilterFlags.OneTime) == 0 && !spec.HasSteps) { return false; }

        return true;
    }

    public static string DescribeEffect(this ScientificProjectSpec spec, ILoc t, float eff, bool percent = false)
        => t.T(percent ? "LV.SP.BuffEffGenericPerc" : "LV.SP.BuffEffGeneric", eff, spec.DisplayName);

    public static string DescribePercentEffect(this ScientificProjectSpec spec, ILoc t, float eff)
        => spec.DescribeEffect(t, eff, true);

    public static string DescribeEffect(this ScientificProjectInfo info, ILoc t, int parameterIndex, bool percent = false)
    {
        var spec = info.Spec;
        var eff = info.GetEffect(parameterIndex);

        return spec.DescribeEffect(t, eff, percent);
    }

    public static string DescribePercentEffect(this ScientificProjectSpec spec, ILoc t, int parameterIndex)
        => spec.DescribeEffect(t, spec.Parameters[parameterIndex], true);

    public static Configurator BindScientificProjectListener<T>(this Configurator configurator, bool alsoBindSelf = false)
        where T : class, IBaseScientificProjectListener
        => configurator.MultiBindSingleton<IBaseScientificProjectListener, T>(alsoBindSelf);

    public static Configurator BindScientificProjectCostProvider<T>(this Configurator configurator, bool alsoBindSelf = false)
        where T : class, IProjectCostProvider
        => configurator.MultiBindSingleton<IProjectCostProvider, T>(alsoBindSelf);

    public static Configurator BindScientificProjectUnlockConditionProvider<T>(this Configurator configurator, bool alsoBindSelf = false)
        where T : class, IProjectUnlockConditionProvider
        => configurator.MultiBindSingleton<IProjectUnlockConditionProvider, T>(alsoBindSelf);

}
