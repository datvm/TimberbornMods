namespace BeaverJukebox;

public class MStarter : IModStarter
{

    public static string ModPath { get; private set; } = "";

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        ModPath = modEnvironment.ModPath;
        new Harmony(nameof(BeaverJukebox)).PatchAll();
    }
}