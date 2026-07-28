namespace ExtendedBuilderReach;

public class MStarter : IModStarter
{

    public void StartMod(IModEnvironment modEnvironment) => new Harmony(nameof(ExtendedBuilderReach)).PatchAll();

}
