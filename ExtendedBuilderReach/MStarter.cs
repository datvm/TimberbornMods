namespace ExtendedBuilderReach;

public class MStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        var h = new Harmony(nameof(ExtendedBuilderReach));
        h.PatchAll();
    }

}
