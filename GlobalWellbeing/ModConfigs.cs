namespace GlobalWellbeing;

[Context("Game")]
public class GameModConfig : Configurator
{
    public override void Configure()
    {
        
    }
}

public class ModStarter : IModStarter
{

    public void StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(GlobalWellbeing)).PatchAll();
    }

}