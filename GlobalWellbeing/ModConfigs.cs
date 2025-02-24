namespace GlobalWellbeing;

[Context("Game")]
public class GameModConfig : Configurator
{
    public override void Configure()
    {
        Bind<WellBeingBuff>().AsSingleton();
        Bind<HighscoreWellbeingBuff>().AsSingleton();

        MultiBind<TemplateModule>().ToProvider(() =>
        {
            TemplateModule.Builder b = new();
            b.AddDecorator<BeaverSpec, BeaverBuffTracker>();
            return b.Build();
        }).AsSingleton();
    }
}

public class ModStarter : IModStarter
{

    public void StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(GlobalWellbeing)).PatchAll();
    }

}