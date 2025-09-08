namespace TImprove4UX;

public class MStarter : IModStarter
{

    public static bool HasDirectionalDynamite { get; private set; }

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        Harmony harmony = new(nameof(TImprove4UX));
        harmony.PatchAll();

        HasDirectionalDynamite = AppDomain.CurrentDomain
            .GetAssemblies()
            .FastAny(q => q.GetName().Name == "DirectionalDynamite");
    }

}
