namespace MoreNaming;

public class MStarter : IModStarter
{
    public void StartMod(IModEnvironment modEnvironment) => new Harmony(nameof(MoreNaming)).PatchAll();
}
