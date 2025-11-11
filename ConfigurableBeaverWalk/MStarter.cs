namespace ConfigurableBeaverWalk;

public class MStarter : IModStarter
{

    public void StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(ConfigurableBeaverWalk)).PatchAll();
    }

}
