namespace ToggleAllMods;

public class ModStarter : IModStarter
{
    public static string ModPath { get; private set; } = null!;

    public void StartMod(IModEnvironment modEnvironment)
    {
        ModPath = modEnvironment.ModPath;
    }

}
