namespace GlobalWellbeing;

[Context("Game")]
public class GameModConfig : Configurator
{
    public override void Configure()
    {
        Bind<WellBeingBuff>().AsSingleton();
        Bind<HighscoreWellbeingBuff>().AsSingleton();

        this.BindTemplateModule()
            .AddDecorator<BeaverSpec, BeaverBuffTracker>()
            .Bind();
    }
}

public class ModStarter : IModStarter
{

    public void StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(GlobalWellbeing)).PatchAll();
    }

}