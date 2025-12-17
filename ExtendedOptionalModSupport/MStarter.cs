namespace ExtendedOptionalModSupport;

public class MStarter : IModStarter
{
    public void StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(ExtendedOptionalModSupport)).PatchAll();
    }
}
