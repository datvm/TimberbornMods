namespace RealStars;

public class MStarter : IModStarter
{
    public void StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(RealStars)).PatchAll();
    }
}
