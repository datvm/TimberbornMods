namespace DirectionalDynamite;

public class MStarter : IModStarter
{

    public static bool HasMacroManagement { get; private set; }

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(DirectionalDynamite)).PatchAll();

        HasMacroManagement = AppDomain.CurrentDomain
            .GetAssemblies()
            .FastAny(q => q.GetName().Name == nameof(MacroManagement));
    }

}
