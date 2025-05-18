namespace ConfigurableTubeZipLine;
public class ModStarter : IModStarter
{

    public static bool ZiporterInstalled = AppDomain.CurrentDomain.GetAssemblies()
        .Any(q => q.GetName().Name == "Ziporter");

    public void StartMod(IModEnvironment modEnvironment)
    {
        Harmony harmony = new(nameof(ConfigurableTubeZipLine));
        harmony.PatchAll();
    }

}
