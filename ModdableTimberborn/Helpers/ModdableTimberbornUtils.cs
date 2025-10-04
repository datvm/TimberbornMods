namespace ModdableTimberborn.Helpers;

public static class ModdableTimberbornUtils
{

    public static ConfigurationContext CurrentContext { get; internal set; } = ConfigurationContext.Bootstrapper;

    public static void LogVerbose(Func<string> msg)
    {
        TimberUiUtils.LogVerbose(() => $"[{nameof(ModdableTimberborn)}] {msg()}");
    }

}
