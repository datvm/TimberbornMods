namespace ConfigurableFaction;

public class MStarter : IModStarter
{

    public static bool HasTimberModBuilder { get; private set; }

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        HasTimberModBuilder = AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.GetName().Name == "TimberModBuilder");

        new Harmony(nameof(ConfigurableFaction)).PatchAll();
    }

}
