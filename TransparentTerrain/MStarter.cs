namespace TransparentTerrain;

public class MStarter : IModStarter
{

    public void StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(TransparentTerrain)).PatchAll();
    }

}
