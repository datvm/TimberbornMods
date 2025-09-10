namespace ModdableTimberborn.Helpers;

public static class ModdableTimberbornUtils
{

    public static readonly FrozenSet<string> LoadedAssemblyNames;
    public static readonly bool HasMoreModLogs;

    public static ConfigurationContext CurrentContext { get; internal set; } = ConfigurationContext.Bootstrapper;

    static ModdableTimberbornUtils()
    {
        LoadedAssemblyNames = AppDomain.CurrentDomain.GetAssemblies()
            .Select(asm => asm.GetName().Name)
            .ToFrozenSet();

        HasMoreModLogs = LoadedAssemblyNames.Contains("MoreModLogs");
    }

    public static void LogDev(Func<string> msg)
    {
        if (!HasMoreModLogs) { return; }
        Debug.Log(msg());
    }

}
