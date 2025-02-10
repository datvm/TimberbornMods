namespace QuickStart;

public class ModStarter : IModStarter
{
    
    public void StartMod(IModEnvironment _)
    {
        new Harmony(nameof(QuickStart)).PatchAll();
    }

}
