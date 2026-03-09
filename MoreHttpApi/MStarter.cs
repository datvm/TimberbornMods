namespace MoreHttpApi;

public class MStarter : IModStarter
{
    public void StartMod(IModEnvironment modEnvironment) => new Harmony(nameof(MoreHttpApi)).PatchAll();
}
