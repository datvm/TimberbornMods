namespace FasterConfigurableStorages;

public class ModStarter : IModStarter
{

    public void StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(FasterConfigurableStorages)).PatchAll();
    }

}
