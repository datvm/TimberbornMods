namespace DecorativePlants;

public class MStarter : IModStarter
{

    public static string ModFolder { get; private set; } = null!;

    public void StartMod(IModEnvironment modEnvironment)
    {
        ModFolder = modEnvironment.ModPath;
    }

}
