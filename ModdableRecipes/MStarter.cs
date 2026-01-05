namespace ModdableRecipes;

public class MStarter : IModStarter
{
    public void StartMod(IModEnvironment modEnvironment) => new Harmony(nameof(ModdableRecipes)).PatchAll();
}
