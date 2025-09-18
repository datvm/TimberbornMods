namespace ExtendedBuilderReach;

public class MStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(ExtendedBuilderReach)).PatchAll();
    }

}
