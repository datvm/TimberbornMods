namespace TestMod;

[Context("Game")]
public class GameConfig : Configurator
{
    public override void Configure()
    {
    }
}

public class ModStarter : IModStarter
{

    public void StartMod(IModEnvironment modEnvironment)
    {
        var harmony = new Harmony(nameof(TestMod));
        harmony.PatchAll();

    }

}
