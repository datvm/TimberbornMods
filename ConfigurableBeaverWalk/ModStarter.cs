namespace ConfigurableBeaverWalk;

public class ModStarter : IModStarter
{

    public void StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(ConfigurableBeaverWalk)).PatchAll();
    }

}
