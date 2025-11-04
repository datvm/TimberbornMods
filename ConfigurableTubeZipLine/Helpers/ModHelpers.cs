namespace ConfigurableTubeZipLine.Helpers;

public static class ModHelpers
{
    public static bool HasZiporter { get; } = TimberUiUtils.LoadedAssemblyNames.Contains("Ziporter");

    public static bool IsTubeway<T>(this T component) where T : ComponentSpec
        => component.HasSpec<TubeSpec>() || component.HasSpec<TubeStationSpec>();

    public static bool IsZipline<T>(this T component) where T : ComponentSpec
        => component.HasSpec<ZiplineTowerSpec>();

    public static float CalculateCost(int bonusSpeed)
    {
        return 1f / ((100 + bonusSpeed) / 100f);
    }

}
