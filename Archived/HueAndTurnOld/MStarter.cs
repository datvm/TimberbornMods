namespace HueAndTurn;

public class MStarter : IModStarter
{

    public static string ModPath { get; private set; } = "";

    public static bool HasMacroManagement { get; private set; }

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        ModPath = modEnvironment.ModPath;

        HasMacroManagement = AppDomain.CurrentDomain
            .GetAssemblies()
            .FastAny(q => q.GetName().Name == nameof(MacroManagement));
    }

}
