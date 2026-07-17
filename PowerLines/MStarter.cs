namespace PowerLines;

public class MStarter : IModStarter
{
    public void StartMod(IModEnvironment modEnvironment)
        => new Harmony(nameof(PowerLines)).PatchAll();
}
