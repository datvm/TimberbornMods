namespace ConfigurablePowerLines;

public class MStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(ConfigurablePowerLines)).PatchAll();
    }

}
