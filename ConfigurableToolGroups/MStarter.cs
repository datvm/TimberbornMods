namespace ConfigurableToolGroups;

public class MStarter : IModStarter
{
    public void StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(ConfigurableToolGroups)).PatchAll();
    }
}
