namespace TImprove.Helpers;

public static class TImproveUtils
{

    public static ImmutableArray<T> GetEnumValues<T>() where T : Enum
        => [.. Enum.GetValues(typeof(T)).Cast<T>().OrderBy(q => q)];

    public static IList<LimitedStringModSettingValue> GetEnumSettings<T>(ImmutableArray<T> values, string prefix) where T : Enum
        => [.. values.Select(q => new LimitedStringModSettingValue(
            q.ToString(), $"{prefix}{q}"))];

    public static LimitedStringModSetting CreateLimitedStringModSetting<T>(ImmutableArray<T> values, string locKey) where T : Enum => new(
        0,
        GetEnumSettings(values, locKey),
        ModSettingDescriptor
            .CreateLocalized(locKey)
            .SetLocalizedTooltip(locKey + "Desc")
    );

}
