namespace ConfigurableTubeZipLine;
public class ModStarter : IModStarter
{

    public void StartMod(IModEnvironment modEnvironment)
    {
        Harmony harmony = new(nameof(ConfigurableTubeZipLine));
        harmony.PatchAll();
    }

}
