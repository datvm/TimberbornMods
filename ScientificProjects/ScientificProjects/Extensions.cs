namespace System;

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
            : (timer.GetProgress() >= 0
                ? GameWeatherStage.Warning
                : GameWeatherStage.Temperate);
    }

    public static bool IsAvailableTo(this ScientificProjectSpec spec, string factionId) => spec.Factions is null || spec.Factions.Value.Contains(factionId);

}

public enum GameWeatherStage
{
    Temperate,
    Warning,
    Hazardous,
}

internal static class ModExtensions
{

    public static string T(this string s, ILoc t)
    {
        return t.T(s);
    }

    public static string T<T1>(this string s, ILoc t, T1 param1)
    {
        return t.T(s, param1);
    }

    public static string T<T1, T2>(this string s, ILoc t, T1 param1, T2 param2)
    {
        return t.T(s, param1, param2);
    }

    public static string T<T1, T2, T3>(this string s, ILoc t, T1 param1, T2 param2, T3 param3)
    {
        return t.T(s, param1, param2, param3);
    }

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
