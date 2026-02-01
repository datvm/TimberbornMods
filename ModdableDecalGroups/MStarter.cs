namespace ModdableDecalGroups;

public class MStarter : IModStarter
{
    public void StartMod(IModEnvironment modEnvironment) => new Harmony(nameof(ModdableDecalGroups)).PatchAll();
}
