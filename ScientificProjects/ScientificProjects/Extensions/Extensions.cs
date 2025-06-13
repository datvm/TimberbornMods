namespace ScientificProjects;

public readonly struct EnumerableReadonlyHashset<T>(ReadOnlyHashSet<T> set) : IEnumerable<T>
{
    public IEnumerator<T> GetEnumerator()
    {
        return set.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return set.GetEnumerator();
    }
}

public static class ResearchProjectsModExtensions
{

    public static TInstance CreateInstance<TBuff, TInstance>(this TBuff buff, IEnumerable<ScientificProjectInfo> projects, out TInstance result)
        where TBuff : CommonProjectsBuff
        where TInstance : CommonProjectBuffInstance<TBuff>, new()
    {
        result = buff.buffs.CreateBuffInstance<TBuff, TInstance, IEnumerable<ScientificProjectInfo>>(buff, projects);
        return result;
    }

    public static int LevelOr0(this IProjectCostProvider _, ScientificProjectInfo info, Func<int, int> calculate)
        => info.Level == 0 ? 0 : calculate(info.Level);
    public static int LevelOr0F(this IProjectCostProvider _, ScientificProjectInfo info, Func<int, float> calculate)
        => info.Level == 0 ? 0 : (int)MathF.Ceiling(calculate(info.Level));
    public static NotSupportedException ThrowNotSupportedEx(this ScientificProjectSpec spec)
    {
        return new NotSupportedException($"Cannot calculate cost for Id {spec.Id} ({spec.DisplayName})");
    }

    public static EnumerableReadonlyHashset<T> AsEnumerable<T>(this ReadOnlyHashSet<T> set) => new(set);

    public static GameWeatherStage GetWeatherStage(this HazardousWeatherApproachingTimer timer)
    {
        return timer._weatherService.IsHazardousWeather
            ? GameWeatherStage.Hazardous
            : timer.GetProgress() >= 0
                ? GameWeatherStage.Warning
                : GameWeatherStage.Temperate;
    }

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

}

public enum GameWeatherStage
{
    Temperate,
    Warning,
    Hazardous,
}

public static class ScientificModExtensions
{

    public static T SetFlexShrink<T>(this T element, float value = 0) where T : VisualElement
    {
        element.style.flexShrink = value;
        return element;
    }

    public static T SetFlex101<T>(this T element) where T : VisualElement
    {
        element.style.flexGrow = 1;
        element.style.flexShrink = 0;
        element.style.flexBasis = 1;
        return element;
    }

}
