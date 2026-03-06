namespace PausableSensors;

public class MStarter : IModStarter
{
    public void StartMod(IModEnvironment modEnvironment) => new Harmony(nameof(PausableSensors)).PatchAll();
}
